using System;
using System.IO;
using Newtonsoft.Json;
using DuckDNSUpdater.Models;

namespace DuckDNSUpdater.Services
{
    public class ConfigurationManager
    {
        private const string CONFIG_FILE = "duckdns_config.json";
        
        public event EventHandler<string>? LogUpdated;
        
        public DuckDNSConfig LoadConfiguration()
        {
            try
            {
                if (File.Exists(CONFIG_FILE))
                {
                    string json = File.ReadAllText(CONFIG_FILE);
                    var config = JsonConvert.DeserializeObject<DuckDNSConfig>(json);
                    
                    OnLogUpdated("Configuration loaded from file");
                    return config ?? new DuckDNSConfig();
                }
                else
                {
                    OnLogUpdated("No configuration file found, using default settings");
                    return new DuckDNSConfig();
                }
            }
            catch (Exception ex)
            {
                OnLogUpdated($"Error loading configuration: {ex.Message}");
                return new DuckDNSConfig();
            }
        }
        
        public void SaveConfiguration(DuckDNSConfig config)
        {
            try
            {
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(CONFIG_FILE, json);
                
                OnLogUpdated("Configuration saved successfully");
            }
            catch (Exception ex)
            {
                OnLogUpdated($"Error saving configuration: {ex.Message}");
                throw;
            }
        }
        
        public bool ValidateConfiguration(DuckDNSConfig config)
        {
            return !string.IsNullOrWhiteSpace(config.Subdomain) && 
                   !string.IsNullOrWhiteSpace(config.Token);
        }
        
        protected virtual void OnLogUpdated(string message)
        {
            LogUpdated?.Invoke(this, message);
        }
    }
}
