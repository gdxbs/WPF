using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ChatClient.Models
{
    [Table("user_preferences")]
    public class UserPreferences : BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("theme_mode")]
        public string ThemeMode { get; set; } = "light";

        [Column("accent_color")]
        public string AccentColor { get; set; } = "#0078D4";

        [Column("font_size")]
        public int FontSize { get; set; } = 12;

        [Column("window_transparency")]
        public int WindowTransparency { get; set; } = 100;

        [Column("background_image_path")]
        public string BackgroundImagePath { get; set; }

        [Column("sound_enabled")]
        public bool SoundEnabled { get; set; } = true;

        [Column("notification_volume")]
        public int NotificationVolume { get; set; } = 70;

        [Column("message_notification_enabled")]
        public bool MessageNotificationEnabled { get; set; } = true;

        [Column("join_leave_notification_enabled")]
        public bool JoinLeaveNotificationEnabled { get; set; } = true;

        [Column("error_notification_enabled")]
        public bool ErrorNotificationEnabled { get; set; } = true;

        [Column("auto_connect_enabled")]
        public bool AutoConnectEnabled { get; set; } = false;

        [Column("last_connection_host")]
        public string LastConnectionHost { get; set; } = "localhost";

        [Column("last_connection_port")]
        public int LastConnectionPort { get; set; } = 9999;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
