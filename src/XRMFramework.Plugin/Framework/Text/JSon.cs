
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace XRMFramework.Text
{
    public static class Json
    {
        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.Indented
        };

        public static TType Deserialize<TType>(string json)
            => JsonConvert.DeserializeObject<TType>(json);

        public static string Serialize<TType>(TType obj)
            => JsonConvert.SerializeObject(obj, serializerSettings);
    }
}