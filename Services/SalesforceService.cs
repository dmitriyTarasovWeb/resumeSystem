using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace resumeSystem.Services;

public class SalesforceService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    private const string ApiVersion = "v66.0";

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
            "SELECT " +
            "Id, AccountId, FirstName, LastName, Email, " +
            "Phone, MobilePhone, Title, Department, " +
            "MailingStreet, MailingCity, MailingPostalCode, " +
            "Description, " +
            "Account.Name, Account.Phone, Account.Website, Account.Industry, " +
            "Account.BillingStreet, Account.BillingCity, Account.BillingPostalCode, " +
            "Account.ShippingStreet, Account.ShippingCity, Account.ShippingPostalCode, " +
            "Account.Description " +
            $"FROM Contact WHERE Email = '{escapedEmail}' LIMIT 1";

        var url =
            $"{baseUrl}/services/data/{ApiVersion}/query?q={Uri.EscapeDataString(query)}";

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
        JsonElement? account = null;

        if (record.TryGetProperty("AccountId", out var accountIdElement) &&
            accountIdElement.ValueKind != JsonValueKind.Null)
        {
            accountId = accountIdElement.GetString();
        }

        if (record.TryGetProperty("Account", out var accountElement) &&
            accountElement.ValueKind != JsonValueKind.Null)
        {
            account = accountElement;
        }

        return new SalesforceContactInfo
        {
            Id = GetString(record, "Id") ?? string.Empty,

            AccountId = accountId,
            AccountName = GetString(account, "Name"),
            AccountPhone = GetString(account, "Phone"),
            AccountWebsite = GetString(account, "Website"),
            AccountIndustry = GetString(account, "Industry"),

            BillingStreet = GetString(account, "BillingStreet"),
            BillingCity = GetString(account, "BillingCity"),
            BillingPostalCode = GetString(account, "BillingPostalCode"),

            ShippingStreet = GetString(account, "ShippingStreet"),
            ShippingCity = GetString(account, "ShippingCity"),
            ShippingPostalCode = GetString(account, "ShippingPostalCode"),

            AccountDescription = GetString(account, "Description"),

            FirstName = GetString(record, "FirstName"),
            LastName = GetString(record, "LastName"),
            Email = GetString(record, "Email"),
            Phone = GetString(record, "Phone"),
            MobilePhone = GetString(record, "MobilePhone"),
            Title = GetString(record, "Title"),
            Department = GetString(record, "Department"),

            MailingStreet = GetString(record, "MailingStreet"),
            MailingCity = GetString(record, "MailingCity"),
            MailingPostalCode = GetString(record, "MailingPostalCode"),

            Description = GetString(record, "Description")
        };
    }

    public async Task<bool> CreateOrUpdateAccountAndContactAsync(
        string accountName,
        string? accountPhone,
        string? website,
        string? industry,
        string? billingStreet,
        string? billingCity,
        string? billingPostalCode,
        string? shippingStreet,
        string? shippingCity,
        string? shippingPostalCode,
        string? accountDescription,
        string firstName,
        string lastName,
        string email,
        string? phone,
        string? mobilePhone,
        string? title,
        string? department,
        string? mailingStreet,
        string? mailingCity,
        string? mailingPostalCode,
        string? contactDescription)
    {
        var baseUrl = _configuration["Salesforce:BaseUrl"];
        var token = await GetAccessTokenAsync();

        var existingContact =
            await FindContactByEmailAsync(email);

        var jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        if (existingContact != null)
        {
            if (!string.IsNullOrEmpty(existingContact.AccountId))
            {
                var accountData = new
                {
                    Name = accountName,
                    Phone = NullIfEmpty(accountPhone),
                    Website = NullIfEmpty(website),
                    Industry = NullIfEmpty(industry),

                    BillingStreet = NullIfEmpty(billingStreet),
                    BillingCity = NullIfEmpty(billingCity),
                    BillingPostalCode = NullIfEmpty(billingPostalCode),

                    ShippingStreet = NullIfEmpty(shippingStreet),
                    ShippingCity = NullIfEmpty(shippingCity),
                    ShippingPostalCode = NullIfEmpty(shippingPostalCode),

                    Description = NullIfEmpty(accountDescription)
                };

                using var accountRequest = new HttpRequestMessage(
                    HttpMethod.Patch,
                    $"{baseUrl}/services/data/{ApiVersion}/sobjects/Account/{existingContact.AccountId}");

                accountRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                accountRequest.Content = new StringContent(
                    JsonSerializer.Serialize(accountData, jsonOptions),
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

                Phone = NullIfEmpty(phone),
                MobilePhone = NullIfEmpty(mobilePhone),
                Title = NullIfEmpty(title),
                Department = NullIfEmpty(department),

                MailingStreet = NullIfEmpty(mailingStreet),
                MailingCity = NullIfEmpty(mailingCity),
                MailingPostalCode = NullIfEmpty(mailingPostalCode),

                Description = NullIfEmpty(contactDescription)
            };

            using var contactRequest = new HttpRequestMessage(
                HttpMethod.Patch,
                $"{baseUrl}/services/data/{ApiVersion}/sobjects/Contact/{existingContact.Id}");

            contactRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            contactRequest.Content = new StringContent(
                JsonSerializer.Serialize(contactData, jsonOptions),
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
            Phone = NullIfEmpty(accountPhone),
            Website = NullIfEmpty(website),
            Industry = NullIfEmpty(industry),

            BillingStreet = NullIfEmpty(billingStreet),
            BillingCity = NullIfEmpty(billingCity),
            BillingPostalCode = NullIfEmpty(billingPostalCode),

            ShippingStreet = NullIfEmpty(shippingStreet),
            ShippingCity = NullIfEmpty(shippingCity),
            ShippingPostalCode = NullIfEmpty(shippingPostalCode),

            Description = NullIfEmpty(accountDescription)
        };

        using var createAccountRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl}/services/data/{ApiVersion}/sobjects/Account");

        createAccountRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        createAccountRequest.Content = new StringContent(
            JsonSerializer.Serialize(account, jsonOptions),
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

            Phone = NullIfEmpty(phone),
            MobilePhone = NullIfEmpty(mobilePhone),
            Title = NullIfEmpty(title),
            Department = NullIfEmpty(department),

            MailingStreet = NullIfEmpty(mailingStreet),
            MailingCity = NullIfEmpty(mailingCity),
            MailingPostalCode = NullIfEmpty(mailingPostalCode),

            Description = NullIfEmpty(contactDescription),

            AccountId = accountId
        };

        using var createContactRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl}/services/data/{ApiVersion}/sobjects/Contact");

        createContactRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        createContactRequest.Content = new StringContent(
            JsonSerializer.Serialize(contact, jsonOptions),
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

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return null;

        if (property.ValueKind == JsonValueKind.Null)
            return null;

        return property.GetString();
    }

    private static string? GetString(JsonElement? element, string propertyName)
    {
        if (!element.HasValue)
            return null;

        return GetString(element.Value, propertyName);
    }

    private static string? NullIfEmpty(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value;
    }

    public class SalesforceContactInfo
    {
        public string Id { get; set; } = string.Empty;

        public string? AccountId { get; set; }
        public string? AccountName { get; set; }
        public string? AccountPhone { get; set; }
        public string? AccountWebsite { get; set; }
        public string? AccountIndustry { get; set; }

        public string? BillingStreet { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingPostalCode { get; set; }

        public string? ShippingStreet { get; set; }
        public string? ShippingCity { get; set; }
        public string? ShippingPostalCode { get; set; }

        public string? AccountDescription { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? MobilePhone { get; set; }
        public string? Title { get; set; }
        public string? Department { get; set; }

        public string? MailingStreet { get; set; }
        public string? MailingCity { get; set; }
        public string? MailingPostalCode { get; set; }

        public string? Description { get; set; }
    }
}