using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DCCovidConnect.Models;
using Newtonsoft.Json.Linq;
using SkiaSharp;

namespace DCCovidConnect.Services
{
    public class MapService
    {
        public Dictionary<string, StateObject> States { get; } = new Dictionary<string, StateObject>();

        private static MapService _instance;

        public static MapService Service => _instance ??= new MapService();

        private static string Namespace = "DCCovidConnect.Assets";
        private static string StatesFile = "states.json";
        private static string CountiesFile = "counties.json";

        private MapService()
        {
            // loads in all the data from the path json files
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MapService)).Assembly;
            using (var stream = assembly.GetManifestResourceStream($"{Namespace}.{StatesFile}"))
            {
                using (var reader = new StreamReader(stream))
                {
                    JArray stateArray = JArray.Parse(reader.ReadToEnd());
                    foreach (JObject state in stateArray)
                    {
                        //switch (state["state"].Value<string>())
                        //{
                        //    case "Virginia":
                        //    case "Maryland":
                        //        break;
                        //    default:
                        //        continue;
                        //}
                        States.Add(state["state"].Value<string>(), new StateObject
                        {
                            State = state["state"].Value<string>(),
                            StateAbbrev = state["state_abbrev"].Value<string>(),
                            Path = SKPath.ParseSvgPathData(state["path"].Value<string>())
                        });
                    }
                }
            }

            using (var stream = assembly.GetManifestResourceStream($"{Namespace}.{CountiesFile}"))
            {
                using (var reader = new StreamReader(stream))
                {
                    JArray countyArray = JArray.Parse(reader.ReadToEnd());
                    int i = 0;
                    foreach (JObject county in countyArray)
                    {
                        int fips = county["fips"].Type == JTokenType.Null ? --i : county["fips"].Value<int>();
                        States[county["state"].Value<string>()].Counties.Add(fips, new CountyObject
                        {
                            State = county["state"].Value<string>(),
                            StateAbbrev = county["state_abbrev"].Value<string>(),
                            County = county["county"].Value<string>(),
                            FIPS = fips,
                            Path = SKPath.ParseSvgPathData(county["path"].Value<string>())
                        });
                    }
                }
            }
        }
    }
}