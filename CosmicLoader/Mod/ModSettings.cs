using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmicLoader
{
    public class ModSettings
    {
        private static JsonSerializerSettings _jsonSerializerSettings =
            new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented};

        public void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, _jsonSerializerSettings));
        }

        public static ModSettings Load(string path)
        {
            return JsonConvert.DeserializeObject<ModSettings>(File.ReadAllText(path), _jsonSerializerSettings);
        }
    }
}
