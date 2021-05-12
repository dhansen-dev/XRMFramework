using System;
using System.Linq;
using System.Reflection;

namespace XRMFramework.Mapping
{
    public class ModelMapper
    {
        public TModelTo MapTo<TModelTo>(object from)
        {
            var targetType = typeof(TModelTo);

            TModelTo targetInstance = default;

            var fromProperties = from.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => prop.CanRead);

            if (targetType.GetConstructors().Any(t => t.GetParameters().Any()))
            {
                var targetConstructor = targetType
                                            .GetConstructors()
                                            .OrderBy(t => t.GetParameters().Count())
                                            .First();

                var parameters = targetConstructor.GetParameters();

                var constructorArguments =

                from param in parameters
                join property in fromProperties
                on param.Name.ToLowerInvariant() equals property.Name.ToLowerInvariant()
                orderby param.Position ascending
                select property.GetValue(@from)
                ;

                targetInstance = (TModelTo)targetConstructor.Invoke(constructorArguments.ToArray());
            }

            var toProperties = typeof(TModelTo).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => prop.CanWrite);

            var matchingProperties =

                from toProp in toProperties
                join fromProp in fromProperties
                on toProp.Name equals fromProp.Name
                select new
                {
                    From = fromProp,
                    To = toProp
                };

            if (targetInstance == null)
            {
                targetInstance = (TModelTo)targetType.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
            }

            foreach (var propPair in matchingProperties)
            {
                var sourceValue = propPair.From.GetValue(from);
                var to = propPair.To;

                propPair.To.SetValue(targetInstance, sourceValue);
            }

            return targetInstance;
        }

        public TModelTo MapTo<TModelFrom, TModelTo>(TModelFrom from, TModelTo to)
            => MapTo<TModelTo>(from);
    }
}