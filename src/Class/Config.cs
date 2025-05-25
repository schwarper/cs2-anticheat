using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace AntiCheat;

public class Config : BasePluginConfig
{
    [JsonPropertyName("Tag")] public string Tag { get; set; } = "{red}[AC] ";
    [JsonPropertyName("Type (PrintAll,PrintAdmin,Kick,Ban)")] public string Type { get; set; } = "PrintAdmin";
    [JsonPropertyName("DiscordWebhook")] public string DiscordWebhook { get; set; } = string.Empty;
    [JsonPropertyName("Modules")] public ModulesConfig Modules { get; set; } = new();

    public class ModulesConfig
    {
        [JsonPropertyName("RapidFire")] public RapidFireConfig RapidFire { get; set; } = new();
        [JsonPropertyName("Scroll")] public BunnyHopConfig BunnyHop { get; set; } = new();
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

    public class BunnyHopConfig
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
        public string[] Blacklist { get; set; } = [
            "gameui_hidden",
            "player_chat",
            "player_score",
            "player_shoot",
            "game_init",
            "game_start",
            "game_end",
            "warmup_end",
            "dm_bonus_weapon_start",
            "survival_announce_phase",
            "break_prop",
            "player_decal",
            "reset_game_titledata",
            "endmatch_mapvote_selecting_map",
            "endmatch_cmm_start_reveal_items",
            "inventory_updated",
            "client_loadout_changed",
            "add_player_sonar_icon",
            "other_death",
            "bullet_damage",
            "hostage_rescued_all",
            "hostage_call_for_help",
            "vip_escaped",
            "vip_killed",
            "player_radio",
            "weapon_fire_on_empty",
            "silencer_detach",
            "inspect_weapon",
            "player_spawned",
            "item_pickup_slerp",
            "item_pickup_failed",
            "item_remove",
            "enter_rescue_zone",
            "exit_rescue_zone",
            "silencer_off",
            "silencer_on",
            "round_prestart",
            "round_poststart",
            "grenade_bounce",
            "molotov_detonate",
            "tagrenade_detonate",
            "inferno_extinguish",
            "decoy_firing",
            "bullet_impact",
            "player_footstep",
            "player_jump",
            "player_blind",
            "player_falldamage",
            "mb_input_lock_success",
            "mb_input_lock_cancel",
            "nav_blocked",
            "nav_generate",
            "achievement_info_loaded",
            "hltv_changed_mode",
            "show_deathpanel",
            "hide_deathpanel",
            "player_avenged_teammate",
            "achievement_earned_local",
            "repost_xbox_achievements",
            "match_end_conditions",
            "write_profile_data",
            "trial_time_expired",
            "update_matchmaking_stats",
            "enable_restart_voting",
            "sfuievent",
            "teamchange_pending",
            "material_default_complete",
            "cs_prev_next_spectator",
            "tournament_reward",
            "start_halftime",
            "ammo_refill",
            "parachute_pickup",
            "parachute_deploy"
        ];
    }
}