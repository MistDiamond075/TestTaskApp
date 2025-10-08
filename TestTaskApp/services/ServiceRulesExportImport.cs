using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TestTaskApp.rules;

namespace TestTaskApp.services
{
    public static class ServiceRulesExportImport
    {
        private const int CurrentVersion = 1;

        private static readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        public static void ExportToJson(DTORules rules)
        {
            rules.Version = CurrentVersion;
            string json = JsonSerializer.Serialize(rules, options);
            File.WriteAllText("exported_rules.json", json);
        }

        public static DTORules ImportFromJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            var rules = JsonSerializer.Deserialize<DTORules>(json, options);

            if (rules == null)
                throw new InvalidOperationException("json reading error");

            if (rules.Version != CurrentVersion)
                throw new InvalidOperationException("different versions");

            return rules;
        }
    }
}
