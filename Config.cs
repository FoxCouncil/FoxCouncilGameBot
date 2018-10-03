// Copyright (c) 2018 The Fox Council

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace FCGameBot
{
    internal static class Config
    {
        private const string ConfigFileName = "FCGameBot.userprefs";
        private const string ConfigAzureWebRoot = "WEBROOT_PATH";

        private static string _configPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName);

        public static ConfigData Data { get; private set; } = new ConfigData();

        public static void Load()
        {
            var azureWebRoot = Environment.GetEnvironmentVariable(ConfigAzureWebRoot);

            _configPath = Path.Combine(string.IsNullOrEmpty(azureWebRoot) ? Directory.GetCurrentDirectory() : azureWebRoot, ConfigFileName);

            if (!File.Exists(_configPath))
            {
                Console.WriteLine("Creating config at: " + _configPath);
                Save();
            }
            else
            {
                Data = JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText(_configPath));
            }
        }

        public static void Save()
        {
            using (var configWriter = File.CreateText(_configPath))
            {
                configWriter.Write(JsonConvert.SerializeObject(Data, Formatting.Indented));
            }
        }
    }
}
