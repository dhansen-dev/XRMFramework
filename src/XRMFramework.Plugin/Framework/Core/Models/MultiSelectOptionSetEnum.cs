using Microsoft.Xrm.Sdk;

using System;
using System.Collections.Generic;
using System.Linq;

namespace XRMFramework.Core.Models
{
    public class MultiSelectOptionSetEnum<TEnum> : List<TEnum> where TEnum : Enum
    {
        public MultiSelectOptionSetEnum(OptionSetValueCollection values)
        {
            if (values != null)
            {
                var selectedOptions = values.Select(os => (TEnum)(object)os.Value);

                AddRange(selectedOptions.ToList());
            }
        }

        public OptionSetValueCollection ToOptionSetValueCollection()
            => new OptionSetValueCollection(this.Select(enm => new OptionSetValue(Convert.ToInt32(enm))).ToList());

        
    }
}
