using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AvaloniaApplicationTest {
    internal class Setting {
        public static JsonNode load() {
            try {
                using (TextReader reader = new StreamReader("config.json", System.Text.Encoding.UTF8)) {
                    //一次性读完
                    string configJson = reader.ReadToEnd();
                    return JsonNode.Parse(configJson);
                }
            }
            catch (Exception) {
                // return new JsonObject();
                return JsonNode.Parse(@"{}" );
            }
        }

        public static void save(JsonArray siteArray, int selected) {
            using (TextWriter writer = new StreamWriter("config.json")) {
                // 构造配置
                var jConfig = new JsonObject{ 
                    ["website"] = siteArray,
                    ["selected"] = selected
                };
                writer.WriteLine(jConfig.ToString());
            }
        }
    }
}
