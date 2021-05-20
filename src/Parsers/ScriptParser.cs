using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using VIToACS.Interfaces;
using VIToACS.Models.Script;

namespace VIToACS.Parsers
{
    /// <summary>
    /// Parser that parses data from script into scenes and elements for ingestion into Azure Search
    /// </summary>
    public class ScriptParser : ISceneParser<Models.Script.Scene>
    {
        static IEnumerable<string> GetStrings(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return new[] { element.GetString() };

                case JsonValueKind.Array:
                    return from item in element.EnumerateArray()
                           select item.GetString();
            }

            throw new ApplicationException($"Unexpected value kind: {element.ValueKind}.");
        }

        static IEnumerable<NamedEntity> GetNamedEntities(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return new NamedEntity[] {};

                case JsonValueKind.Array:
                    return from item in element.EnumerateArray()
                           let names = item.EnumerateObject()
                           from name in names
                           select new NamedEntity
                           {
                               Name = name.Value.ToString(),
                               Type = name.Name
                           };
            }

            throw new ApplicationException($"Unexpected value kind: {element.ValueKind}.");
        }
        /// <summary>
        /// Gets the scenes
        /// </summary>
        /// <param name="doc">the JSON document</param>
        /// <returns>a collection of scene objects</returns>
        /// <exception cref="ArgumentNullException">if doc is null</exception>
        IEnumerable<Scene> ISceneParser<Scene>.GetScenes(JsonDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            var scene = doc.RootElement.GetProperty("Scene");

            var scenes = from item in scene.EnumerateArray()
                         let elements = item.GetProperty("Element").EnumerateArray()
                         select new Scene
                         {
                             Id = item.GetProperty("id").GetInt32().ToString(),
                             Set = item.GetProperty("Set").GetString(),
                             InteriorExterior = item.GetProperty("INT/EXT").GetString(),
                             Time = item.GetProperty("Time").GetString(),
                             Location = item.GetProperty("Loc").GetString(),
                             Elements = from element in elements
                                        let id = element.GetProperty("id")
                                        where id.ValueKind == JsonValueKind.Number
                                        let hasDialog = element.TryGetProperty("Dialogue", out JsonElement dialog)
                                        select new Element
                                        {
                                            Id = element.GetProperty("id").GetInt32(),
                                            Type = element.GetProperty("Type").GetString(),
                                            Description = element.GetProperty("Description").GetString(),
                                            Parenthetical = element.GetProperty("Parenthetical").GetString(),
                                            Dialogue = hasDialog ? element.GetProperty("Dialogue").GetString() : null,
                                            Nouns = GetStrings(element.GetProperty("Noun")),
                                            NamedEntities = GetNamedEntities(element.GetProperty("NE"))
                                        }
                         };

            return scenes;
        }
    }
}
