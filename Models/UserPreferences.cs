using System;

namespace ChatClient.Models
{
    public class UserPreferences
    {
        public string Id { get; set; }
        public string MachineId { get; set; }
        public string ThemeMode { get; set; } = "light";
        public string AccentColor { get; set; } = "#0078D4";
        public int FontSize { get; set; } = 12;
        public int WindowTransparency { get; set; } = 100;
        public string BackgroundImagePath { get; set; }
        public bool SoundEnabled { get; set; } = true;
        public int NotificationVolume { get; set; } = 70;
        public bool MessageNotificationEnabled { get; set; } = true;
        public bool JoinLeaveNotificationEnabled { get; set; } = true;
        public bool ErrorNotificationEnabled { get; set; } = true;
        public bool AutoConnectEnabled { get; set; } = false;
        public string LastConnectionHost { get; set; } = "localhost";
        public int LastConnectionPort { get; set; } = 9999;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
