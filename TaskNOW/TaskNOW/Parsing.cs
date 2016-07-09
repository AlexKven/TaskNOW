using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace TaskNOW
{
    static class Parsing
    {
        public static IEnumerable<Project> ParseProjects(string response)
        {
            List<Project> result = new List<Project>();

            JsonValue root;
            JsonObject jsonObject;
            JsonValue.TryParse(response, out root);
            jsonObject = root.GetObject();
            while (jsonObject.Count == 1)
            {
                jsonObject = jsonObject[jsonObject.Keys.First()].GetObject();
            }

            if (jsonObject.ContainsKey("projects"))
            {
                var array = jsonObject["projects"].GetArray();
                foreach (var value in array)
                {
                    jsonObject = value.GetObject();
                    string name = jsonObject.ContainsKey("name") ? jsonObject.GetNamedString("name") : null;
                    int? color = jsonObject.ContainsKey("color") ? new int?((int)jsonObject.GetNamedNumber("color")) : null;
                    bool deleted = jsonObject.ContainsKey("is_deleted") ? (jsonObject.GetNamedNumber("is_deleted") != 0) : false;
                    bool collapsed = jsonObject.ContainsKey("collapsed") ? (jsonObject.GetNamedNumber("collapsed") != 0) : false;
                    int? id = jsonObject.ContainsKey("id") ? new int?((int)jsonObject.GetNamedNumber("id")) : null;
                    bool archived = jsonObject.ContainsKey("is_archived") ? (jsonObject.GetNamedNumber("is_archived") != 0) : false;
                    if (id.HasValue)
                    {
                        var project = new Project(name, color.HasValue ? color.Value : 0, deleted, collapsed, id.Value, archived);
                        result.Add(project);
                    }
                }
            }

            return result;
        }
    }
}
