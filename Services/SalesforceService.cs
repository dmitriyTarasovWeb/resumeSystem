using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace resumeSystem.Services;

public class SalesforceService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public SalesforceService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var baseUrl = _configuration["Salesforce:BaseUrl"];
        var clientId = _configuration["Salesforce:ClientId"];
        var clientSecret = _configuration["Salesforce:ClientSecret"];

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId!,
            ["client_secret"] = clientSecret!
        });

        var response = await _httpClient.PostAsync(
            $"{baseUrl}/services/oauth2/token",
            content);

        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Salesforce OAuth error: {body}");

        using var json = JsonDocument.Parse(body);

        return json.RootElement
            .GetProperty("access_token")
            .GetString()!;
    }

    private async Task<string?> FindContactIdByEmailAsync(
    string email,
    string token)
    {
        var baseUrl = _configuration["Salesforce:BaseUrl"];

        var query =
            $"SELECT Id FROM Contact WHERE Email = '{email.Replace("'", "\\'")}' LIMIT 1";

        var url =
            $"{baseUrl}/services/data/v66.0/query?q={Uri.EscapeDataString(query)}";

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Salesforce query error: {body}");

        using var json = JsonDocument.Parse(body);

        var records = json.RootElement.GetProperty("records");

        if (records.GetArrayLength() == 0)
            return null;

        return records[0]
            .GetProperty("Id")
            .GetString();
    }

    public async Task<bool> CreateAccountAndContactAsync(
    string accountName,
    string description,
    string firstName,
    string lastName,
    string email,
    string phone)
    {
        var baseUrl = _configuration["Salesforce:BaseUrl"];
        var token = await GetAccessTokenAsync();

        var existingContactId =
            await FindContactIdByEmailAsync(email, token);

        if (existingContactId != null)
        {
            var contactData = new
            {
                FirstName = firstName,
                LastName = string.IsNullOrWhiteSpace(lastName)
                    ? firstName
                    : lastName,
                Email = email,
                Phone = phone,
                Description = description
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Patch,
                $"{baseUrl}/services/data/v66.0/sobjects/Contact/{existingContactId}");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            request.Content = new StringContent(
                JsonSerializer.Serialize(contactData),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Salesforce Contact update error: {body}");

            return false;
        }

        var accountData = new
        {
            Name = accountName,
            Description = description
        };

        using var accountRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl}/services/data/v66.0/sobjects/Account");

        accountRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        accountRequest.Content = new StringContent(
            JsonSerializer.Serialize(accountData),
            Encoding.UTF8,
            "application/json");

        var accountResponse =
            await _httpClient.SendAsync(accountRequest);

        var accountBody =
            await accountResponse.Content.ReadAsStringAsync();

        if (!accountResponse.IsSuccessStatusCode)
            throw new Exception(
                $"Salesforce Account error: {accountBody}");

        using var accountJson =
            JsonDocument.Parse(accountBody);

        var accountId =
            accountJson.RootElement
                .GetProperty("id")
                .GetString();

        var contactDataNew = new
        {
            FirstName = firstName,
            LastName = string.IsNullOrWhiteSpace(lastName)
                ? firstName
                : lastName,
            Email = email,
            Phone = phone,
            Description = description,
            AccountId = accountId
        };

        using var contactRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl}/services/data/v66.0/sobjects/Contact");

        contactRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        contactRequest.Content = new StringContent(
            JsonSerializer.Serialize(contactDataNew),
            Encoding.UTF8,
            "application/json");

        var contactResponse =
            await _httpClient.SendAsync(contactRequest);

        var contactBody =
            await contactResponse.Content.ReadAsStringAsync();

        if (!contactResponse.IsSuccessStatusCode)
            throw new Exception(
                $"Salesforce Contact error: {contactBody}");

        return true;
    }
}