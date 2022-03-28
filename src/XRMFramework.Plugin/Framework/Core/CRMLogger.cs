using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.PluginTelemetry;

using Newtonsoft.Json;

using Soderberg.XRM.Plugins.Models.Party.Requests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRMFramework.Text;

namespace XRMFramework.Core
{
    public sealed class CRMLogger
    {
        private int _currentIndentationLevel = 0;
        private int _initialIndentationLevel = 0;
        private readonly CRMLogger _parentLogger;
        private readonly ITracingService _tracingService;
        private readonly ILogger _appInsightsLogger;

        private readonly Dictionary<int, string> _tracingBlockHeaders = new Dictionary<int, string>();

        private CRMLogger(ITracingService tracingService, ILogger appInsightsLogger)
        {
            _tracingService = tracingService;
            _appInsightsLogger = appInsightsLogger;
        }

        private CRMLogger(ITracingService tracingService, ILogger appInsightsLogger, int indentationLevel, CRMLogger parentLogger) : this(tracingService, appInsightsLogger)
        {
            _initialIndentationLevel = indentationLevel;
            _currentIndentationLevel = indentationLevel;
            _parentLogger = parentLogger;
        }

        public CRMLogger Close(bool all = false)
        {
            var totalIndentation = _currentIndentationLevel - _initialIndentationLevel;

            if (totalIndentation < 1)
            {
                return this;
            }

            do
            {
                var header = _tracingBlockHeaders[_currentIndentationLevel];

                var endBlockHeader = " ======== End " + header + " ======== ";

                _tracingBlockHeaders.Remove(totalIndentation);

                if (header != null)
                {
                    TraceIndented(endBlockHeader);
                }

                totalIndentation--;

            } while (totalIndentation != 0 && all);

            return this;
        }

        public void LogObject(object logObject)
            => Log(Json.Serialize(logObject));

        public static CRMLogger GetRootLogger(ITracingService tracingService, ILogger appInsightsLogger)
            => new CRMLogger(tracingService, appInsightsLogger);

        public CRMLogger Log(string message)
            => TraceIndented(message);


        public CRMLogger NewBlock()
            => new CRMLogger(_tracingService, _appInsightsLogger, _currentIndentationLevel, this);

        public CRMLogger NewIndentedBlock()
            => new CRMLogger(_tracingService, _appInsightsLogger, _currentIndentationLevel, this)
                        .AddIndent();

        public CRMLogger CloseBlock()
        {
            Close(true);
            return _parentLogger;
        }

        private CRMLogger TraceIndented(string message)
        {
            var indentedMessage = AddIndent(_currentIndentationLevel, message);

            _tracingService.Trace(indentedMessage);
            _appInsightsLogger.LogInformation(indentedMessage);

            return this;

            string AddIndent(int indentationLevel, string log)
            {
                var indentedLog = new StringBuilder();

                var indent = new string(' ', indentationLevel * 2);

                var delim = new[] { Environment.NewLine, "\n" };
                var lines = log.Split(delim, StringSplitOptions.None);

                foreach (var line in lines.Take(lines.Length - 1))
                {
                    indentedLog.Append(indent).AppendLine(line);
                }

                indentedLog.Append(indent).Append(lines[lines.Length - 1]);

                return indentedLog.ToString();
            }
        }

        public CRMLogger LogException(Exception exception, bool includeStackTrace, bool includeInnerExceptions)
        {

            CRMLogger nestedLogger = this;

            if (exception != null)
            {
                var logTitle = $"Logged exception of type {exception.GetType().Name} -> {exception.Message}";

                _appInsightsLogger.LogError(exception, logTitle);

                Log($"Logged exception of type {exception.GetType().Name} -> {exception.Message}");

                if (includeStackTrace)
                {
                    nestedLogger = nestedLogger
                                    .NewIndentedBlock()
                                        .Log(exception.StackTrace);
                }

                if (includeInnerExceptions)
                {
                    var innerException = exception.InnerException;

                    while (innerException != null)
                    {
                        nestedLogger = nestedLogger.
                                        NewIndentedBlock()
                                            .Log($"Logged inner exception of type {innerException.GetType().Name} -> {innerException.Message ?? "No message"}")
                                            .Log(innerException.StackTrace);

                        innerException = innerException.InnerException;
                    }
                }
            }

            return this;
        }

        public CRMLogger AddIndent(int indentationToAdd = 1)
        {
            _currentIndentationLevel++;

            _tracingBlockHeaders[_currentIndentationLevel] = null;

            return this;
        }

        public CRMLogger LogExecutionContext(IPluginExecutionContext pex)
        {
            Log("Plugin context")
            .Log($"{nameof(pex.MessageName)}: {pex.MessageName}")
            .Log($"{nameof(pex.Stage)}: {GetStageName(pex.Stage)}")
            .Log($"Is async: {(pex.Mode == 1)}")
            .Log($"{nameof(pex.Depth)}: {pex.Depth}")
            .Log($"Primary entity: {pex.PrimaryEntityName} ({pex.PrimaryEntityId})")
            .Log($"{nameof(pex.UserId)}: {pex.UserId}")
            .Log($"{nameof(pex.InitiatingUserId)}: {pex.InitiatingUserId}")
            .Log($"{nameof(pex.BusinessUnitId)}: {pex.BusinessUnitId}")
            .Log($"{nameof(pex.CorrelationId)}: {pex.CorrelationId}")
            .Log("Attributes");

            LogAttributes(pex);

            return this;

            string GetStageName(int stage)
                => stage == 10 ? "PreValidation"
                    : stage == 20 ? "PreOperation"
                    : stage == 30 ? "MainOperation"
                    : stage == 40 ? "PostOperation"
                    : "Unknown";
        }


        private CRMLogger LogAttributes(IPluginExecutionContext pex)
        {
            pex.InputParameters.TryGetValue("Target", out object target);

            switch (target)
            {
                case null:
                    {
                        foreach (var input in pex.InputParameters.OrderBy(p => p.Key))
                        {
                            Log($"{input.Key}: {FormatValue(input.Value, null)}");
                        }

                        break;
                    }
                case Entity e:
                    {
                        Log(FormatAttributesFromEntity(e));

                        break;
                    }
                case EntityReference eref:
                    break;
            }

            return this;

            string FormatValue(object value, string formattedValue)
            {
                switch (value)
                {
                    case null:
                        return "null";

                    case OptionSetValue os:
                        return formattedValue != null ? formattedValue + $"({os.Value})" : os.Value.ToString();

                    case EntityReference eref:
                        return $"{formattedValue ?? eref.Name ?? "unknown"} ({eref.Id} / {eref.LogicalName})";

                    case string s when formattedValue != null:
                        return formattedValue;

                    case string s when s.Length == 0:
                        return "\"\"";

                    case string s:
                        return s;
                    case EntityCollection collection:
                        {
                            var entityString = string.Empty;

                            foreach (var entity in collection.Entities)
                            {
                                entityString += FormatAttributesFromEntity(entity);
                            }

                            return entityString;
                        }
                    default:
                        return formattedValue ?? value.ToString();
                }
            }

            string FormatAttributesFromEntity(Entity e)
            {
                if (!e.Attributes.Any())
                {
                    return string.Empty;
                }
                var formatString = "";
                var longestAttribute = e.Attributes.Keys.Max(x => x.Length);

                foreach (var keypair in e.Attributes.OrderBy(attr => attr.Key))
                {
                    e.FormattedValues.TryGetValue(keypair.Key, out string formattedValue);

                    formatString += ($"{keypair.Key.PadRight(longestAttribute + 2)}: {FormatValue(keypair.Value, formattedValue)}") + Environment.NewLine;
                }

                return formatString;
            }

        }

        public CRMLogger LogTime()
        {
            Log($"Log written: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.ffff}");

            return this;
        }
    }
}