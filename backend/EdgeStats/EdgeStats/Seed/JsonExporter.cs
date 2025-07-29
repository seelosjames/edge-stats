using System.Text.Json;
using System.Text.Json.Serialization;

namespace EdgeStats.Seed
{
    public static class JsonExporter
    {
        public static void ExportToJson<T>(List<T> data, string fileName)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            var json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(Path.Combine("dummy_data_output", $"{fileName}.json"), json);
        }
    }
}
