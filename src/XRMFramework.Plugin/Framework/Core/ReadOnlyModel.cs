using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace XRMFramework.Core
{
    public class ReadOnlyModel
    {
        private readonly Dictionary<string, object> _setOnce = new Dictionary<string, object>();

        protected TPropertyType SetIfNotAlreadySet<TPropertyType>(TPropertyType value, [CallerMemberName] string propertyName = "")
        {
            if (_setOnce.TryGetValue(propertyName, out object propValue))
            {
                throw new InvalidOperationException("Property " + propertyName + " can only be set once");
            }

            _setOnce.Add(propertyName, value);

            return value;
        }

        protected TPropertyType GetReadOnlyValue<TPropertyType>([CallerMemberName] string propertyName = "")
            => _setOnce.TryGetValue(propertyName, out object value) ? value != null ? (TPropertyType)value : default : default;
    }
}