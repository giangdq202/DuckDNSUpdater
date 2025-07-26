using System.Drawing;
using System.Reflection;

namespace DuckDNSUpdater.Properties
{
    public static class Resources
    {
        public static Icon ApplicationIcon
        {
            get
            {
                try
                {
                    // Try to load from embedded resource first (for standalone executable)
                    var assembly = Assembly.GetExecutingAssembly();
                    using var stream = assembly.GetManifestResourceStream("DuckDNSUpdater.duckdns.ico");
                    if (stream != null)
                    {
                        return new Icon(stream);
                    }
                    
                    // Fallback to external file
                    string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "duckdns.ico");
                    if (File.Exists(iconPath))
                    {
                        return new Icon(iconPath);
                    }
                }
                catch
                {
                    // Fallback to system icon if loading fails
                }
                return SystemIcons.Information;
            }
        }
        
        public static Icon TrayIcon
        {
            get
            {
                // Use the same logic as ApplicationIcon
                return ApplicationIcon;
            }
        }
    }
}
