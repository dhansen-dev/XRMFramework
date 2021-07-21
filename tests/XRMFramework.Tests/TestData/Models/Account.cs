using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmFramework.Tests.TestData.Models
{
    [EntityLogicalName("account")]
    public class Account : Entity
    {
        public Account() : base("account")
        {
        }


        [AttributeLogicalName("name")]
        public string AccountName { get; set; }
    }
}
