using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DuckDNSUpdater.Services
{
    public class DuckDNSService
    {
        private const string IP_CHECK_URL = "https://api.ipify.org";
        private const string DUCKDNS_UPDATE_URL = "https://www.duckdns.org/update";
        
        public event EventHandler<string>? LogUpdated;
        
        public async Task<string> GetPublicIPAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("DuckDNS-Updater/1.0");
                    
                    string ip = await client.GetStringAsync(IP_CHECK_URL);
                    return ip.Trim();
                }
            }
            catch (Exception ex)
            {
                OnLogUpdated($"Error getting public IP: {ex.Message}");
                throw;
            }
        }
        
        public async Task<bool> UpdateDuckDNSAsync(string subdomain, string token, string ip)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    string url = $"{DUCKDNS_UPDATE_URL}?domains={subdomain}&token={token}&ip={ip}";
                    
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    
                    string responseContent = await response.Content.ReadAsStringAsync();
                    bool success = responseContent.Trim().Equals("OK", StringComparison.OrdinalIgnoreCase);
                    
                    if (success)
                    {
                        OnLogUpdated($"✓ DuckDNS update successful for {subdomain}.duckdns.org");
                    }
                    else
                    {
                        OnLogUpdated($"✗ DuckDNS returned invalid response: {responseContent}");
                    }
                    
                    return success;
                }
            }
            catch (Exception ex)
            {
                OnLogUpdated($"✗ Error updating DuckDNS: {ex.Message}");
                throw;
            }
        }
        
        public async Task<UpdateResult> CheckAndUpdateAsync(string subdomain, string token, string lastKnownIP)
        {
            var result = new UpdateResult();
            
            try
            {
                OnLogUpdated("Checking public IP address...");
                
                // Get current public IP
                string currentIP = await GetPublicIPAsync();
                result.CurrentIP = currentIP;
                result.IPChanged = currentIP != lastKnownIP || string.IsNullOrEmpty(lastKnownIP);
                
                // Only update if IP has changed or this is the first check
                if (result.IPChanged)
                {
                    OnLogUpdated($"IP changed from '{lastKnownIP}' to '{currentIP}', updating DuckDNS...");
                    
                    // Update DuckDNS
                    result.UpdateSuccess = await UpdateDuckDNSAsync(subdomain, token, currentIP);
                    result.LastUpdateTime = DateTime.Now;
                }
                else
                {
                    OnLogUpdated($"IP unchanged ({currentIP}), skipping update");
                    result.UpdateSuccess = true; // No update needed is considered success
                }
            }
            catch (Exception ex)
            {
                OnLogUpdated($"✗ Error: {ex.Message}");
                result.UpdateSuccess = false;
                result.ErrorMessage = ex.Message;
            }
            
            return result;
        }
        
        protected virtual void OnLogUpdated(string message)
        {
            LogUpdated?.Invoke(this, message);
        }
    }
    
    public class UpdateResult
    {
        public string CurrentIP { get; set; } = "";
        public bool IPChanged { get; set; }
        public bool UpdateSuccess { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public string ErrorMessage { get; set; } = "";
    }
}
