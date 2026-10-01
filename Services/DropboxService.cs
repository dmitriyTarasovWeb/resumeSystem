using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

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

    public async Task UploadFileAsync(string fileName, string content)
    {
        var token = _configuration["Dropbox:AccessToken"];
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
            new AuthenticationHeaderValue("Bearer", token);

        request.Headers.Add("Dropbox-API-Arg", apiArg);

        var contentBytes = Encoding.UTF8.GetBytes(content);

        var contentBody = new ByteArrayContent(contentBytes);
        contentBody.Headers.ContentType =
            new MediaTypeHeaderValue("application/octet-stream");

        request.Content = contentBody;

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Dropbox error {response.StatusCode}: {responseBody}");
        }
    }
}