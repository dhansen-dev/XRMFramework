using Microsoft.Xrm.Sdk;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XRMFramework.Core
{
    public sealed class CRMLogger
    {
        private int _currentIndentationLevel = 0;
        private int _initialIndentationLevel = 0;
        private readonly CRMLogger _parentLogger;
        private readonly ITracingService _tracingService;

        private readonly Dictionary<int, string> _tracingBlockHeaders = new Dictionary<int, string>();

        private CRMLogger(ITracingService tracingService)
        {
            _tracingService = tracingService;
        }

        private CRMLogger(ITracingService tracingService, int indentationLevel, CRMLogger parentLogger) : this(tracingService)
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

        public static CRMLogger GetRootLogger(ITracingService tracingService)
            => new CRMLogger(tracingService);

        public CRMLogger Log(string message)
            => TraceIndented(message);


        public CRMLogger NewBlock()
            => new CRMLogger(_tracingService, _currentIndentationLevel, this);

        public CRMLogger NewIndentedBlock()
            => new CRMLogger(_tracingService, _currentIndentationLevel, this)
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
            .Log("Attributes")
            .Log("=======================");
            
            LogAttributes(pex);

            return this;

            string GetStageName(int stage)
                =>    stage == 10 ? "PreValidation"
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
                        foreach (var input in pex.InputParameters)
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

                foreach (var keypair in e.Attributes)
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