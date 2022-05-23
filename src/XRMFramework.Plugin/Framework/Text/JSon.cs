
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace XRMFramework.Text
{
    public static class Json
    {
        private static JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            
        };

        public static void SetSerializerSettings(JsonSerializerSettings settings)
            => _serializerSettings = settings;

        public static TType Deserialize<TType>(string json)
            => JsonConvert.DeserializeObject<TType>(json);

        public static string Serialize<TType>(TType obj)
            => JsonConvert.SerializeObject(obj, _serializerSettings);
    }
}