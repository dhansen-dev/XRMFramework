using System;
using System.Collections.Generic;
using System.Linq;

namespace XRMFramework.Util.Validation
{
    public static class IEnumerableValidations
    {
        public static IEnumerable<TType> IsNotEmpty<TType>(this IEnumerable<TType> list, string errormessage = null)
        {
            if (list.Count() == 0)
            {
                throw new ArgumentException(errormessage ?? "List cannot be empty");
            }

            return list;
        }
    }
}