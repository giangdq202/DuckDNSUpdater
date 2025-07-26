using System.Drawing;

namespace DuckDNSUpdater.Properties
{
    public static class Resources
    {
        public static Icon ApplicationIcon
        {
            get
            {
                // Use a system icon for application
                return SystemIcons.Information;
            }
        }
        
        public static Icon TrayIcon
        {
            get
            {
                // Use a network-related system icon for tray
                return SystemIcons.Exclamation;
            }
        }
    }
}
