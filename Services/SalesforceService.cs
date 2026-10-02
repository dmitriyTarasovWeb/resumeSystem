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

    public async Task<SalesforceContactInfo?> FindContactByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var baseUrl = _configuration["Salesforce:BaseUrl"];
        var token = await GetAccessTokenAsync();

        var escapedEmail = email
            .Replace("\\", "\\\\")
            .Replace("'", "\\'");

        var query =
            $"SELECT Id, AccountId, FirstName, LastName, Email, Phone, Description, Account.Name " +
            $"FROM Contact WHERE Email = '{escapedEmail}' LIMIT 1";

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

        var record = records[0];

        string? accountId = null;
        string? accountName = null;

        if (record.TryGetProperty("AccountId", out var accountIdElement) &&
            accountIdElement.ValueKind != JsonValueKind.Null)
        {
            accountId = accountIdElement.GetString();
        }

        if (record.TryGetProperty("Account", out var accountElement) &&
            accountElement.ValueKind != JsonValueKind.Null &&
            accountElement.TryGetProperty("Name", out var accountNameElement))
        {
            accountName = accountNameElement.GetString();
        }

        return new SalesforceContactInfo
        {
            Id = record.GetProperty("Id").GetString()!,
            AccountId = accountId,
            AccountName = accountName,
            FirstName = record.TryGetProperty("FirstName", out var firstName)
                ? firstName.GetString()
                : null,
            LastName = record.TryGetProperty("LastName", out var lastName)
                ? lastName.GetString()
                : null,
            Email = record.TryGetProperty("Email", out var emailElement)
                ? emailElement.GetString()
                : null,
            Phone = record.TryGetProperty("Phone", out var phoneElement)
                ? phoneElement.GetString()
                : null,
            Description = record.TryGetProperty("Description", out var descriptionElement)
                ? descriptionElement.GetString()
                : null
        };
    }

    public async Task<bool> CreateOrUpdateAccountAndContactAsync(
        string accountName,
        string description,
        string firstName,
        string lastName,
        string email,
        string phone)
    {
        var baseUrl = _configuration["Salesforce:BaseUrl"];
        var token = await GetAccessTokenAsync();

        var existingContact =
            await FindContactByEmailAsync(email);

        if (existingContact != null)
        {
            if (!string.IsNullOrEmpty(existingContact.AccountId))
            {
                var accountData = new
                {
                    Name = accountName,
                    Description = description
                };

                using var accountRequest = new HttpRequestMessage(
                    HttpMethod.Patch,
                    $"{baseUrl}/services/data/v66.0/sobjects/Account/{existingContact.AccountId}");

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
                        $"Salesforce Account update error: {accountBody}");
            }

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

            using var contactRequest = new HttpRequestMessage(
                HttpMethod.Patch,
                $"{baseUrl}/services/data/v66.0/sobjects/Contact/{existingContact.Id}");

            contactRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            contactRequest.Content = new StringContent(
                JsonSerializer.Serialize(contactData),
                Encoding.UTF8,
                "application/json");

            var contactResponse =
                await _httpClient.SendAsync(contactRequest);

            var contactBody =
                await contactResponse.Content.ReadAsStringAsync();

            if (!contactResponse.IsSuccessStatusCode)
                throw new Exception(
                    $"Salesforce Contact update error: {contactBody}");

            return false;
        }

        var account = new
        {
            Name = accountName,
            Description = description
        };

        using var createAccountRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl}/services/data/v66.0/sobjects/Account");

        createAccountRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        createAccountRequest.Content = new StringContent(
            JsonSerializer.Serialize(account),
            Encoding.UTF8,
            "application/json");

        var createAccountResponse =
            await _httpClient.SendAsync(createAccountRequest);

        var createAccountBody =
            await createAccountResponse.Content.ReadAsStringAsync();

        if (!createAccountResponse.IsSuccessStatusCode)
            throw new Exception(
                $"Salesforce Account error: {createAccountBody}");

        using var accountJson =
            JsonDocument.Parse(createAccountBody);

        var accountId =
            accountJson.RootElement
                .GetProperty("id")
                .GetString()!;

        var contact = new
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

        using var createContactRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl}/services/data/v66.0/sobjects/Contact");

        createContactRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        createContactRequest.Content = new StringContent(
            JsonSerializer.Serialize(contact),
            Encoding.UTF8,
            "application/json");

        var createContactResponse =
            await _httpClient.SendAsync(createContactRequest);

        var createContactBody =
            await createContactResponse.Content.ReadAsStringAsync();

        if (!createContactResponse.IsSuccessStatusCode)
            throw new Exception(
                $"Salesforce Contact error: {createContactBody}");

        return true;
    }



    public class SalesforceContactInfo
    {
        public string Id { get; set; } = string.Empty;
        public string? AccountId { get; set; }
        public string? AccountName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Description { get; set; }
    }
}