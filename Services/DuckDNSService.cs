using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DuckDNSUpdater.Services
{
    public enum IPResolutionMode
    {
        WebService, // Get IP from external web services (default)
        Local,      // Use local machine IP
        Fixed,      // Use a fixed IP address
        Host        // Resolve from hostname
    }

    public class DuckDNSService
    {
        private const string DUCKDNS_UPDATE_URL = "https://www.duckdns.org/update";
        
        // Multiple IP check services for redundancy
        private readonly List<string> _ipCheckUrls = new List<string>
        {
            "https://api.ipify.org",
            "https://icanhazip.com",
            "https://checkip.amazonaws.com",
            "https://ipinfo.io/ip"
        };
        
        public event EventHandler<string>? LogUpdated;
        
        public async Task<string> GetPublicIPAsync()
        {
            foreach (string ipCheckUrl in _ipCheckUrls)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(10);
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("DuckDNS-Updater/1.0");
                        
                        string ip = await client.GetStringAsync(ipCheckUrl);
                        string cleanIP = ip.Trim();
                        
                        // Validate that we got a valid IPv4 address
                        if (IPAddress.TryParse(cleanIP, out IPAddress? parsedIP) && 
                            parsedIP?.AddressFamily == AddressFamily.InterNetwork)
                        {
                            OnLogUpdated($"✓ Public IP retrieved: {cleanIP} (from {ipCheckUrl})");
                            return cleanIP;
                        }
                        else
                        {
                            OnLogUpdated($"⚠ Invalid IP format from {ipCheckUrl}: {cleanIP}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnLogUpdated($"⚠ Failed to get IP from {ipCheckUrl}: {ex.Message}");
                    // Continue to next service
                }
            }
            
            // If all services failed, throw an exception
            throw new Exception("Failed to retrieve public IP from all available services");
        }
        
        public async Task<bool> UpdateDuckDNSAsync(string subdomain, string token, string ip)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrWhiteSpace(subdomain))
                    throw new ArgumentException("Subdomain cannot be empty", nameof(subdomain));
                if (string.IsNullOrWhiteSpace(token))
                    throw new ArgumentException("Token cannot be empty", nameof(token));
                if (string.IsNullOrWhiteSpace(ip))
                    throw new ArgumentException("IP address cannot be empty", nameof(ip));
                
                // Validate IP address format
                if (!IPAddress.TryParse(ip, out IPAddress? parsedIP))
                    throw new ArgumentException($"Invalid IP address format: {ip}", nameof(ip));
                
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30); // Increased timeout for DuckDNS
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("DuckDNS-Updater/2.0");
                    
                    // Build URL with proper encoding
                    string url = BuildDuckDNSUrl(subdomain, token, ip);
                    OnLogUpdated($"Updating DuckDNS: {subdomain}.duckdns.org -> {ip}");
                    
                    HttpResponseMessage response = await client.GetAsync(url);
                    
                    // Check HTTP status
                    if (!response.IsSuccessStatusCode)
                    {
                        OnLogUpdated($"✗ HTTP Error {response.StatusCode}: {response.ReasonPhrase}");
                        return false;
                    }
                    
                    string responseContent = await response.Content.ReadAsStringAsync();
                    string trimmedResponse = responseContent.Trim();
                    
                    // Check DuckDNS response
                    bool success = trimmedResponse.Equals("OK", StringComparison.OrdinalIgnoreCase);
                    
                    if (success)
                    {
                        OnLogUpdated($"✓ DuckDNS update successful for {subdomain}.duckdns.org");
                    }
                    else
                    {
                        // Log the exact response for debugging
                        OnLogUpdated($"✗ DuckDNS update failed. Response: '{trimmedResponse}'");
                        
                        // Provide specific error messages based on common responses
                        switch (trimmedResponse.ToLower())
                        {
                            case "ko":
                                OnLogUpdated("  → Invalid domain or token");
                                break;
                            case "bad":
                                OnLogUpdated("  → Bad request format");
                                break;
                            default:
                                OnLogUpdated($"  → Unexpected response: {trimmedResponse}");
                                break;
                        }
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
        
        private string BuildDuckDNSUrl(string subdomain, string token, string ip)
        {
            // Use URL encoding to handle special characters properly
            string encodedSubdomain = Uri.EscapeDataString(subdomain);
            string encodedToken = Uri.EscapeDataString(token);
            string encodedIP = Uri.EscapeDataString(ip);
            
            return $"{DUCKDNS_UPDATE_URL}?domains={encodedSubdomain}&token={encodedToken}&ip={encodedIP}";
        }
        
        /// <summary>
        /// Gets the local machine's public-facing IP address by querying the local network interfaces
        /// </summary>
        public async Task<string> GetLocalIPAsync()
        {
            try
            {
                var host = await Dns.GetHostEntryAsync(Dns.GetHostName());
                foreach (IPAddress address in host.AddressList)
                {
                    // Look for IPv4 addresses that are not loopback or link-local
                    if (address.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(address) &&
                        !address.ToString().StartsWith("169.254")) // Exclude link-local addresses
                    {
                        OnLogUpdated($"✓ Local IP found: {address}");
                        return address.ToString();
                    }
                }
                
                throw new Exception("No suitable local IPv4 address found");
            }
            catch (Exception ex)
            {
                OnLogUpdated($"✗ Error getting local IP: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Gets IP address from a specific hostname
        /// </summary>
        public async Task<string> GetHostIPAsync(string hostname)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hostname))
                    throw new ArgumentException("Hostname cannot be empty", nameof(hostname));
                
                var hostEntry = await Dns.GetHostEntryAsync(hostname);
                foreach (IPAddress address in hostEntry.AddressList)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        OnLogUpdated($"✓ Host IP for {hostname}: {address}");
                        return address.ToString();
                    }
                }
                
                throw new Exception($"No IPv4 address found for hostname: {hostname}");
            }
            catch (Exception ex)
            {
                OnLogUpdated($"✗ Error resolving hostname {hostname}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Validates if a string is a valid IPv4 address
        /// </summary>
        public bool IsValidIPv4(string ip)
        {
            return IPAddress.TryParse(ip, out IPAddress? parsedIP) && 
                   parsedIP?.AddressFamily == AddressFamily.InterNetwork;
        }
        
        /// <summary>
        /// Resolves IP address based on the specified resolution mode
        /// </summary>
        public async Task<string> ResolveIPAsync(IPResolutionMode mode, string value = "")
        {
            switch (mode)
            {
                case IPResolutionMode.WebService:
                    return await GetPublicIPAsync();
                    
                case IPResolutionMode.Local:
                    return await GetLocalIPAsync();
                    
                case IPResolutionMode.Fixed:
                    if (string.IsNullOrWhiteSpace(value))
                        throw new ArgumentException("Fixed IP value cannot be empty");
                    if (!IsValidIPv4(value))
                        throw new ArgumentException($"Invalid IPv4 address: {value}");
                    OnLogUpdated($"✓ Using fixed IP: {value}");
                    return value;
                    
                case IPResolutionMode.Host:
                    if (string.IsNullOrWhiteSpace(value))
                        throw new ArgumentException("Hostname cannot be empty");
                    return await GetHostIPAsync(value);
                    
                default:
                    throw new ArgumentException($"Unsupported resolution mode: {mode}");
            }
        }
        
        /// <summary>
        /// Enhanced update method with IP resolution mode support
        /// </summary>
        public async Task<UpdateResult> CheckAndUpdateAsync(string subdomain, string token, string lastKnownIP, 
            IPResolutionMode resolutionMode = IPResolutionMode.WebService, string resolutionValue = "")
        {
            var result = new UpdateResult();
            const int maxRetries = 3;
            int retryCount = 0;
            
            while (retryCount < maxRetries)
            {
                try
                {
                    OnLogUpdated($"Resolving IP address using {resolutionMode} method... (Attempt {retryCount + 1}/{maxRetries})");
                    
                    // Get current IP using specified resolution mode
                    string currentIP = await ResolveIPAsync(resolutionMode, resolutionValue);
                    result.CurrentIP = currentIP;
                    result.PreviousIP = lastKnownIP;
                    result.IPChanged = currentIP != lastKnownIP || string.IsNullOrEmpty(lastKnownIP);
                    result.IPSource = resolutionMode.ToString();
                    
                    // Only update if IP has changed or this is the first check
                    if (result.IPChanged)
                    {
                        OnLogUpdated($"IP changed from '{lastKnownIP}' to '{currentIP}', updating DuckDNS...");
                        
                        // Update DuckDNS with retry logic
                        bool updateSuccess = false;
                        for (int updateRetry = 0; updateRetry < 2; updateRetry++)
                        {
                            try
                            {
                                updateSuccess = await UpdateDuckDNSAsync(subdomain, token, currentIP);
                                if (updateSuccess)
                                    break;
                                
                                if (updateRetry < 1) // Only retry once
                                {
                                    OnLogUpdated("⚠ Update failed, retrying in 5 seconds...");
                                    await Task.Delay(5000);
                                }
                            }
                            catch (Exception updateEx)
                            {
                                OnLogUpdated($"⚠ Update attempt {updateRetry + 1} failed: {updateEx.Message}");
                                if (updateRetry < 1)
                                {
                                    await Task.Delay(5000);
                                }
                                else
                                {
                                    throw; // Re-throw on final attempt
                                }
                            }
                        }
                        
                        result.UpdateSuccess = updateSuccess;
                        if (updateSuccess)
                        {
                            result.LastUpdateTime = DateTime.Now;
                        }
                    }
                    else
                    {
                        OnLogUpdated($"IP unchanged ({currentIP}), skipping update");
                        result.UpdateSuccess = true; // No update needed is considered success
                    }
                    
                    // Success - break out of retry loop
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    result.RetryCount = retryCount;
                    OnLogUpdated($"✗ Error (attempt {retryCount}): {ex.Message}");
                    
                    if (retryCount >= maxRetries)
                    {
                        result.UpdateSuccess = false;
                        result.ErrorMessage = ex.Message;
                        OnLogUpdated($"✗ Failed after {maxRetries} attempts");
                        break;
                    }
                    else
                    {
                        OnLogUpdated($"⚠ Retrying in {retryCount * 5} seconds...");
                        await Task.Delay(retryCount * 5000); // Progressive delay
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Legacy method for backward compatibility
        /// </summary>
        public async Task<UpdateResult> CheckAndUpdateAsync(string subdomain, string token, string lastKnownIP)
        {
            return await CheckAndUpdateAsync(subdomain, token, lastKnownIP, IPResolutionMode.WebService);
        }
        
        protected virtual void OnLogUpdated(string message)
        {
            LogUpdated?.Invoke(this, message);
        }
    }
    
    public class UpdateResult
    {
        public string CurrentIP { get; set; } = "";
        public string PreviousIP { get; set; } = "";
        public bool IPChanged { get; set; }
        public bool UpdateSuccess { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public string ErrorMessage { get; set; } = "";
        public int RetryCount { get; set; }
        public string IPSource { get; set; } = ""; // Which service provided the IP
        
        public override string ToString()
        {
            return $"IP: {CurrentIP}, Changed: {IPChanged}, Success: {UpdateSuccess}, " +
                   $"LastUpdate: {LastUpdateTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never"}";
        }
    }
}
