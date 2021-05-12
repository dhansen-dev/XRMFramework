using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace XRMFramework.Text
{
    public static class Json
    {
        private static JavaScriptSerializer _json;
        private static readonly object _lockObject = new object();

        public static List<JavaScriptConverter> CustomConverters { get; } = new List<JavaScriptConverter>();

        public static JavaScriptTypeResolver CustomResolver { get; set; }

        public static int RecursionLimit { get => _json.RecursionLimit; set => _json.RecursionLimit = value; }
        public static int MaxJsonLength { get => _json.MaxJsonLength; set => _json.MaxJsonLength = value; }

        private static JavaScriptSerializer CreateOrReturnSerializer()
        {
            if(_json == null)
            {
                lock(_lockObject)
                {
                    if(_json != null)
                    {
                        return _json;
                    }

                    _json = new JavaScriptSerializer(CustomResolver);

                    _json.RegisterConverters(CustomConverters);
                }
            }

            return _json;
        }

        public static TType Deserialize<TType>(string json)
            => CreateOrReturnSerializer().Deserialize<TType>(json);

        public static string Serialize<TType>(TType obj)
            => CreateOrReturnSerializer().Serialize(obj);
    }
}