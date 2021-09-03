using System;
using System.IO;
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

            
            using var sr = new StreamReader(filepath);
            Config result = JsonUtility.FromJson<Config>(sr.ReadToEnd());
            // Return default config if the JSON file was incorrect
            return result ?? new Config();
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

            var jsonString = JsonUtility.ToJson(this, true);
            using StreamWriter sw = new StreamWriter(filepath);
            sw.Write(jsonString);
            Debug.Log($"Config file written to {filepath}");
        }
    }
}