using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Util
{
    public class Config
    {
        public string serverAddress = "localhost:50051";
        public string folder = "";
        public string file = "test.fits";
        public int maxCubeSizeMb = 200;
        public int slicesPerMessage = 4;
        public int compressionPrecision = 12;

        private static Config _instance;

        private static string DefaultPath => $"{Application.persistentDataPath}/config.json";

        private static Config FromFile(string filepath = "")
        {
            if (filepath.Length == 0)
            {
                filepath = DefaultPath;
            }

            // Return default config if it doesn't exist
            if (!File.Exists(filepath))
            {
                var defaultConfig = new Config();
                defaultConfig.WriteToFile();
                return defaultConfig;
            }

            var jsonSerializer = new JsonSerializer();
            var sr = new StreamReader(filepath);
            var result = jsonSerializer.Deserialize(sr, typeof(Config));

            // Return default config if the JSON file was incorrect
            if (result == null)
            {
                return new Config();
            }

            return (Config)result;
        }

        public static Config Instance
        {
            get
            {
                _instance ??= FromFile();
                return _instance;
            }
        }

        public void WriteToFile(string filepath = "")
        {
            if (filepath.Length == 0)
            {
                filepath = DefaultPath;
            }

            Debug.Log($"Config file written to {filepath}");
            JsonSerializer serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            using StreamWriter sw = new StreamWriter(filepath);
            using JsonWriter writer = new JsonTextWriter(sw);
            serializer.Serialize(writer, this);
        }
    }
}