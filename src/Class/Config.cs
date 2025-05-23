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
            "ugc_map_info_received",
            "ugc_map_unsubscribed",
            "ugc_map_download_error",
            "ugc_file_download_finished",
            "ugc_file_download_start",
            "dm_bonus_weapon_start",
            "survival_announce_phase",
            "break_prop",
            "player_decal",
            "entity_visible",
            "instructor_server_hint_create",
            "instructor_server_hint_stop",
            "reset_game_titledata",
            "vote_ended",
            "vote_started",
            "vote_options",
            "endmatch_mapvote_selecting_map",
            "endmatch_cmm_start_reveal_items",
            "inventory_updated",
            "client_loadout_changed",
            "add_player_sonar_icon",
            "door_open",
            "door_closed",
            "door_break",
            "other_death",
            "bullet_damage",
            "item_purchase",
            "bomb_beginplant",
            "bomb_abortplant",
            "bomb_begindefuse",
            "bomb_abortdefuse",
            "hostage_stops_following",
            "hostage_rescued_all",
            "hostage_call_for_help",
            "vip_escaped",
            "vip_killed",
            "player_radio",
            "bomb_beep",
            "weapon_fire_on_empty",
            "weapon_zoom",
            "silencer_detach",
            "inspect_weapon",
            "weapon_zoom_rifle",
            "player_spawned",
            "item_pickup_slerp",
            "item_pickup_failed",
            "item_remove",
            "item_equip",
            "enter_buyzone",
            "exit_buyzone",
            "buytime_ended",
            "enter_bombzone",
            "exit_bombzone",
            "enter_rescue_zone",
            "exit_rescue_zone",
            "silencer_off",
            "silencer_on",
            "buymenu_open",
            "buymenu_close",
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
            "door_moving",
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
            "start_vote",
            "teamchange_pending",
            "material_default_complete",
            "cs_prev_next_spectator",
            "tournament_reward",
            "start_halftime",
            "ammo_refill",
            "parachute_pickup",
            "parachute_deploy",
            "dronegun_attack",
            "drone_dispatched",
            "loot_crate_visible",
            "loot_crate_opened",
            "open_crate_instr",
            "smoke_beacon_paradrop",
            "drone_cargo_detached",
            "drone_above_roof",
            "dz_item_interaction",
            "survival_teammate_respawn",
            "guardian_wave_restart",
            "bullet_flight_resolution",
            "server_shutdown",
            "server_message",
            "player_full_update",
            "local_player_team",
            "local_player_controller_team",
            "local_player_pawn_changed",
            "ragdoll_dissolved",
            "team_info",
            "team_score",
            "hltv_rank_camera",
            "hltv_rank_entity",
            "demo_stop",
            "map_shutdown",
            "map_transition",
            "hostname_changed",
            "game_message",
            "round_start_pre_entity",
            "round_start_post_nav",
            "teamplay_round_start",
            "player_hintmessage",
            "break_breakable",
            "broken_breakable",
            "door_close",
            "vote_failed",
            "vote_passed",
            "vote_cast_yes",
            "vote_cast_no",
            "achievement_event",
            "achievement_write_failed",
            "bonus_updated",
            "gameinstructor_draw",
            "gameinstructor_nodraw",
            "flare_ignite_npc",
            "helicopter_grenade_punt_miss",
            "physgun_pickup",
            "cart_updated",
            "store_pricesheet_updated",
            "item_schema_initialized",
            "drop_rate_modified",
            "event_ticket_modified",
            "gc_connected",
            "instructor_start_lesson",
            "instructor_close_lesson",
            "set_instructor_group_enabled",
            "clientside_lesson_closed",
            "dynamic_shadow_light_changed"
        ];
    }
}