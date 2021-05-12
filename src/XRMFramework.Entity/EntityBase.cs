using Microsoft.Xrm.Sdk;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace XRMFramework.Core
{
    [DataContract()]
    public abstract class EntityBase : Entity, INotifyPropertyChanging, INotifyPropertyChanged
    {
        private readonly Dictionary<string, string> Cache = new Dictionary<string, string>();

        protected EntityBase(string entityLogicalName) : base(entityLogicalName)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangingEventHandler PropertyChanging;

        protected T GetAttribute<T>([CallerMemberName] string propertyName = null)
            => GetAttributeValue<T>(GetAttributeNameFromCache(propertyName));

        public IEnumerable<TPartyType> ConvertToActivityPartyList<TPartyType>(EntityCollection parties) where TPartyType : Entity
             => parties?.Entities.Select(party => party.ToEntity<TPartyType>());

        public void SetPartyListAttribute<TPartyType>(IEnumerable<TPartyType> parties, [CallerMemberName] string propertyName = null) where TPartyType : Entity
        {
            var collection = new EntityCollection();
            collection.Entities.AddRange(parties.Select(party => party));
            SetAttribute(collection, propertyName);
        }

        /// <summary>
        /// If the value exists in the entity attribute collection and is null
        /// a null reference exception is thrown
        /// </summary>
        /// <typeparam name="TAttributeType">The type of attribute</typeparam>
        /// <param name="propertyName">Property name to retrive</param>
        /// <exception cref="NullReferenceException">When the value in attribute collection null an error is thrown</exception>
        /// <returns>The requested attribute value</returns>
        protected TAttributeType GetAttributeAsNonNullable<TAttributeType>([CallerMemberName] string propertyName = null)
        {
            var attributeValue = GetAttribute<TAttributeType>(propertyName);

            if (attributeValue == null && Contains(propertyName))
            {
                throw new NullReferenceException($"{propertyName} cannot be null. This could be due to bad data quality.z");
            }

            return attributeValue;
        }

        protected TEnum? HandleGetEnum<TEnum>(OptionSetValue optionSetValue) where TEnum : struct
            => optionSetValue != null
                ? (TEnum)Enum.ToObject(typeof(TEnum), optionSetValue.Value)
                : default;

        protected OptionSetValue HandleSetEnum<TEnum>(TEnum? enumValue) where TEnum : struct
        {
            if (!enumValue.HasValue)
            {
                return null;
            }

            var enumName = Enum.GetName(typeof(TEnum), enumValue.Value);

            var val = Enum.Parse(typeof(TEnum), enumName);
            var t = (int)val;

            return new OptionSetValue(t);
        }

        protected void SetAttribute(object value, [CallerMemberName] string propertyName = null)
        {
            OnPropertyChanging(propertyName);
            SetAttributeValue(GetAttributeNameFromCache(propertyName), value);
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Do not allow the value to be set to a null value
        /// </summary>
        /// <typeparam name="TAttributeType"></typeparam>
        /// <param name="value">The non nullable value to set</param>
        /// <param name="propertyName">The property to set. Attribute name is reflected from this</param>
        protected void SetAttributeAsNonNullable(object value, [CallerMemberName] string propertyName = null)
        {
            if (value == null)
            {
                throw new NullReferenceException($"{propertyName} cannot be null");
            }

            SetAttribute(value, propertyName);
        }

        protected void SetIdAttribute(Guid? value, [CallerMemberName] string propertyName = null)
        {
            OnPropertyChanging(propertyName);
            SetAttributeValue(GetAttributeNameFromCache(propertyName), value);

            base.Id = value ?? Guid.Empty;

            OnPropertyChanged(propertyName);
        }

        private string GetAttributeName(string propertyName)
            => GetType()
                    .GetProperties()
                    .FirstOrDefault(p => p.Name == propertyName)
                    .GetCustomAttribute<AttributeLogicalNameAttribute>()
                    .LogicalName;

        private string GetAttributeNameFromCache(string propertyName)
        {
            if (!Cache.TryGetValue(propertyName, out string attributeName))
            {
                attributeName = GetAttributeName(propertyName);

                Cache[propertyName] = attributeName;
            }

            return attributeName;
        }

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void OnPropertyChanging(string propertyName)
            => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }
}