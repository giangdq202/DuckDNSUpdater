namespace DuckDNSUpdater.Models
{
    public class DuckDNSConfig
    {
        public string Subdomain { get; set; } = "";
        public string Token { get; set; } = "";
        public int UpdateIntervalMinutes { get; set; } = 5;
        public bool AutoStart { get; set; } = false;
        public bool MinimizeToTray { get; set; } = false;
    }
}
