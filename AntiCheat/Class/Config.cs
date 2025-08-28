using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace AntiCheat.Class;

public class Config : BasePluginConfig
{
    [JsonPropertyName("Tag")] public string Tag { get; set; } = "{red}[AC] ";
    [JsonPropertyName("BanTimeSecond")] public int Time { get; set; } = 0;
    [JsonPropertyName("Type (PrintAll,PrintAdmin,Kick,Ban)")] public string Type { get; set; } = "PrintAdmin";
    [JsonPropertyName("DiscordWebhook")] public string DiscordWebhook { get; set; } = string.Empty;
    [JsonPropertyName("WebhookEmbed")] public WebhookConfig Webhook { get; set; } = new();
    [JsonPropertyName("Modules")] public ModulesConfig Modules { get; set; } = new();

    public class WebhookConfig
    {
        [JsonPropertyName("Title")] public string Title { get; set; } = "🚨 Cheat Detected";
        [JsonPropertyName("Color")] public string ColorHex { get; set; } = "#FF0000";
        [JsonPropertyName("Footer")] public string Footer { get; set; } = "AntiCheat System";
        [JsonPropertyName("Username")] public string Username { get; set; } = "AntiCheat Bot";
        [JsonPropertyName("AvatarUrl")] public string AvatarUrl { get; set; } = "";
        [JsonPropertyName("ThumbnailUrl")] public string ThumbnailUrl { get; set; } = "";
        [JsonPropertyName("ImageUrl")] public string ImageUrl { get; set; } = "";
    }

    public class ModulesConfig
    {
        [JsonPropertyName("RapidFire")] public RapidFireConfig RapidFire { get; set; } = new();
        [JsonPropertyName("Scroll")] public ScrollConfig Scroll { get; set; } = new();
        [JsonPropertyName("SilentAim")] public SilentAimConfig SilentAim { get; set; } = new();
        [JsonPropertyName("Spinbot")] public SpinbotConfig Spinbot { get; set; } = new();
        [JsonPropertyName("Teleport")] public TeleportConfig Teleport { get; set; } = new();
        [JsonPropertyName("Wallhack")] public WallhackConfig Wallhack { get; set; } = new();
        [JsonPropertyName("AntiDLL")] public AntiDLLConfig AntiDLL { get; set; } = new();
    }

    public class RapidFireConfig
    {
        [JsonPropertyName("Enabled")] public bool Enabled { get; set; } = true;
        [JsonPropertyName("MaxSuspicion")] public int MaxSuspicion { get; set; } = 3;
    }

    public class ScrollConfig
    {
        [JsonPropertyName("Enabled")] public bool Enabled { get; set; } = true;
        [JsonPropertyName("SampleSizeMin")] public int SampleSizeMin { get; set; } = 45;
        [JsonPropertyName("SampleSizeMax")] public int SampleSizeMax { get; set; } = 55;
    }

    public class SilentAimConfig
    {
        [JsonPropertyName("Enabled")] public bool Enabled { get; set; } = true;
        [JsonPropertyName("MaxSuspicion")] public int MaxSuspicion { get; set; } = 5;
        [JsonPropertyName("AngleThreshold")] public float AngleThreshold { get; set; } = 50f;
    }

    public class SpinbotConfig
    {
        [JsonPropertyName("Enabled")] public bool Enabled { get; set; } = true;
        [JsonPropertyName("MaxSuspicion")] public int MaxSuspicion { get; set; } = 5;
        [JsonPropertyName("AngularSpeedThreshold")] public float AngularSpeedThreshold { get; set; } = 4000f;
        [JsonPropertyName("MinimumAngleChange")] public float MinimumAngleChange { get; set; } = 5f;
    }

    public class TeleportConfig
    {
        [JsonPropertyName("Enabled")] public bool Enabled { get; set; } = true;
        [JsonPropertyName("MaxSuspicion")] public int MaxSuspicion { get; set; } = 5;
    }

    public class WallhackConfig
    {
        [JsonPropertyName("Enabled")] public bool Enabled { get; set; } = false;
    }

    public class AntiDLLConfig
    {
        [JsonPropertyName("Enabled")] public bool Enabled { get; set; } = true;
        [JsonPropertyName("Blacklist")]
        public string[] Blacklist { get; set; } = [];
    }
}
