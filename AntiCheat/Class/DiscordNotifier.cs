using System.Net.Http.Json;
using System.Text.Json;
using static AntiCheat.AntiCheat;

namespace AntiCheat.Class;

public static class DiscordNotifier
{
    private static readonly HttpClient HttpClient = new();

    public static async Task SendDiscordEmbedAsync(string? description = null, string? title = null)
    {
        var config = Instance.Config.Webhook;

        int color;
        try
        {
            color = int.Parse(config.ColorHex.Replace("#", ""), System.Globalization.NumberStyles.HexNumber);
        }
        catch
        {
            color = 16711680;
        }

        var embed = new Dictionary<string, object?>
        {
            ["title"] = string.IsNullOrWhiteSpace(title) ? config.Title : title,
            ["description"] = description,
            ["color"] = color,
            ["thumbnail"] = string.IsNullOrWhiteSpace(config.ThumbnailUrl) ? null : new { url = config.ThumbnailUrl },
            ["image"] = string.IsNullOrWhiteSpace(config.ImageUrl) ? null : new { url = config.ImageUrl },
            ["footer"] = new { text = config.Footer },
            ["timestamp"] = DateTime.UtcNow.ToString("o")
        };

        var payload = new
        {
            username = config.Username,
            avatar_url = config.AvatarUrl,
            embeds = new[] { embed }
        };


        try
        {
            using HttpResponseMessage response = await HttpClient.PostAsJsonAsync(Instance.Config.DiscordWebhook, payload, JsonSerializerOptions.Default);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[DiscordNotifier] Discord error: {ex.Message}");
        }
    }
}
