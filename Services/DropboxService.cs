using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace resumeSystem.Services;

public class DropboxService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public DropboxService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task UploadFileAsync(string fileName, string fileContent)
    {
        var accessToken = await GetAccessTokenAsync();

        var folder = _configuration["Dropbox:Folder"] ?? "/SupportTickets";

        var apiArg = JsonSerializer.Serialize(new
        {
            path = $"{folder.TrimEnd('/')}/{fileName}",
            mode = "add",
            autorename = true
        });

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://content.dropboxapi.com/2/files/upload");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        request.Headers.Add("Dropbox-API-Arg", apiArg);

        var fileBytes = Encoding.UTF8.GetBytes(fileContent);

        var body = new ByteArrayContent(fileBytes);

        body.Headers.ContentType =
            new MediaTypeHeaderValue("application/octet-stream");

        request.Content = body;

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Dropbox upload error {response.StatusCode}: {responseBody}");
        }
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var appKey = _configuration["Dropbox:AppKey"];
        var appSecret = _configuration["Dropbox:AppSecret"];
        var refreshToken = _configuration["Dropbox:RefreshToken"];

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.dropbox.com/oauth2/token");

        var auth =
            Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{appKey}:{appSecret}"));

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", auth);

        request.Content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken
            });

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Dropbox token refresh error {response.StatusCode}: {responseBody}");
        }

        var tokenResponse =
            JsonSerializer.Deserialize<DropboxTokenResponse>(
                responseBody,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (tokenResponse == null ||
            string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new Exception(
                $"Dropbox returned invalid token response: {responseBody}");
        }

        return tokenResponse.AccessToken;
    }

    private class DropboxTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
    }
}