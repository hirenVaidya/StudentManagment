using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using StudentsWeb.Models;

namespace StudentsWeb.Services;

public class StudentsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public StudentsApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<(bool Success, LoginApiResponse? Data, string? Error)> LoginAsync(string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { email, password });
        if (!response.IsSuccessStatusCode)
        {
            var error = await ReadErrorAsync(response);
            return (false, null, error ?? "Invalid email or password.");
        }

        var data = await response.Content.ReadFromJsonAsync<LoginApiResponse>(JsonOptions);
        return data is null
            ? (false, null, "Unexpected response from server.")
            : (true, data, null);
    }

    public async Task<List<StudentViewModel>> GetStudentsAsync()
    {
        var response = await SendAuthorizedAsync(HttpMethod.Get, "api/students");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<StudentViewModel>>(JsonOptions) ?? [];
    }

    public async Task<StudentViewModel?> GetStudentAsync(int id)
    {
        var response = await SendAuthorizedAsync(HttpMethod.Get, $"api/students/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StudentViewModel>(JsonOptions);
    }

    public async Task<StudentViewModel> CreateStudentAsync(StudentFormViewModel model)
    {
        var response = await SendAuthorizedAsync(HttpMethod.Post, "api/students", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<StudentViewModel>(JsonOptions))!;
    }

    public async Task<StudentViewModel> UpdateStudentAsync(int id, StudentFormViewModel model)
    {
        var response = await SendAuthorizedAsync(HttpMethod.Put, $"api/students/{id}", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<StudentViewModel>(JsonOptions))!;
    }

    public async Task DeleteStudentAsync(int id)
    {
        var response = await SendAuthorizedAsync(HttpMethod.Delete, $"api/students/{id}");
        response.EnsureSuccessStatusCode();
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync(
        HttpMethod method,
        string url,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        AttachToken(request);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await _httpClient.SendAsync(request);
    }

    private void AttachToken(HttpRequestMessage request)
    {
        var token = _httpContextAccessor.HttpContext?.Session.GetString(SessionKeys.JwtToken);
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private static async Task<string?> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
            return error?.Message;
        }
        catch
        {
            return null;
        }
    }
}
