using System.Net.Http.Json;
using System.Text.Json;
using static AntiCheat.AntiCheat;

namespace AntiCheat.Class;

public static class DiscordNotifier
{
    private static readonly HttpClient HttpClient = new();

    public static async Task SendDiscordAsync(string message)
    {
        var payload = new
        {
            content = message.Trim()
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