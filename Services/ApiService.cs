using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using StudentLifeTracker.Shared.DTOs;

namespace StudentProgressTracker.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    // TODO: Move to configuration (appsettings.json or Preferences) for production
    private readonly string _baseUrl = "https://spt-api-v2-defjczgvg9bgbcaw.eastus2-01.azurewebsites.net/";
    private const string TokenKey = "auth_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string UserKey = "user_data";

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            //BaseAddress = new Uri(_baseUrl),
            BaseAddress = new Uri("https://spt-api-v2-defjczgvg9bgbcaw.eastus2-01.azurewebsites.net/"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Ignore SSL certificate errors for localhost (development only)
#if DEBUG
        _httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        })
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
#endif

        // Load token if exists
        _ = LoadTokenAsync();
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(GetTokenAsync().Result);

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    private async Task LoadTokenAsync()
    {
        var token = await GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private async Task<string?> GetTokenAsync()
    {
        try
        {
            return await SecureStorage.GetAsync(TokenKey);
        }
        catch
        {
            return null;
        }
    }

    private async Task SaveTokenAsync(string token)
    {
        try
        {
            await SecureStorage.SetAsync(TokenKey, token);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving token: {ex.Message}");
        }
    }

    private async Task SaveRefreshTokenAsync(string refreshToken)
    {
        try
        {
            await SecureStorage.SetAsync(RefreshTokenKey, refreshToken);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving refresh token: {ex.Message}");
        }
    }

    private async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            return await SecureStorage.GetAsync(RefreshTokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveUserAsync(UserDTO user)
    {
        try
        {
            var userJson = JsonSerializer.Serialize(user);
            await SecureStorage.SetAsync(UserKey, userJson);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving user: {ex.Message}");
        }
    }

    public async Task<UserDTO?> GetUserAsync()
    {
        try
        {
            var userJson = await SecureStorage.GetAsync(UserKey);
            if (string.IsNullOrEmpty(userJson))
                return null;

            return JsonSerializer.Deserialize<UserDTO>(userJson);
        }
        catch
        {
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            SecureStorage.Remove(TokenKey);
            SecureStorage.Remove(RefreshTokenKey);
            SecureStorage.Remove(UserKey);
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during logout: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/register", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Data != null)
                {
                    await SaveTokenAsync(result.Data.Token);
                    await SaveRefreshTokenAsync(result.Data.RefreshToken);
                    await SaveUserAsync(result.Data.User);
                }

                return result ?? ApiResponse<AuthResponse>.ErrorResponse("Invalid response from server");
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return errorResponse ?? ApiResponse<AuthResponse>.ErrorResponse($"Registration failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Registration error: {ex.Message}");
            return ApiResponse<AuthResponse>.ErrorResponse($"Registration failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Data != null)
                {
                    await SaveTokenAsync(result.Data.Token);
                    await SaveRefreshTokenAsync(result.Data.RefreshToken);
                    await SaveUserAsync(result.Data.User);
                }

                return result ?? ApiResponse<AuthResponse>.ErrorResponse("Invalid response from server");
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return errorResponse ?? ApiResponse<AuthResponse>.ErrorResponse($"Login failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
            return ApiResponse<AuthResponse>.ErrorResponse($"Login failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            var refreshToken = await GetRefreshTokenAsync();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
            {
                return ApiResponse<AuthResponse>.ErrorResponse("No tokens available for refresh");
            }

            var request = new RefreshTokenRequest
            {
                Token = token,
                RefreshToken = refreshToken
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/refresh", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Data != null)
                {
                    await SaveTokenAsync(result.Data.Token);
                    await SaveRefreshTokenAsync(result.Data.RefreshToken);
                    await SaveUserAsync(result.Data.User);
                }

                return result ?? ApiResponse<AuthResponse>.ErrorResponse("Invalid response from server");
            }
            else
            {
                // Token refresh failed, user needs to login again
                await LogoutAsync();
                return ApiResponse<AuthResponse>.ErrorResponse("Token refresh failed. Please login again.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Token refresh error: {ex.Message}");
            await LogoutAsync();
            return ApiResponse<AuthResponse>.ErrorResponse($"Token refresh failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            var response = await _httpClient.GetAsync(endpoint);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result ?? ApiResponse<T>.ErrorResponse("Invalid response from server");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Try to refresh token
                var refreshResult = await RefreshTokenAsync();
                if (refreshResult.Success)
                {
                    // Retry the request
                    return await GetAsync<T>(endpoint);
                }
                return ApiResponse<T>.ErrorResponse("Unauthorized. Please login again.");
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return errorResponse ?? ApiResponse<T>.ErrorResponse($"Request failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GET error: {ex.Message}");
            return ApiResponse<T>.ErrorResponse($"Request failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result ?? ApiResponse<T>.ErrorResponse("Invalid response from server");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Try to refresh token
                var refreshResult = await RefreshTokenAsync();
                if (refreshResult.Success)
                {
                    // Retry the request
                    return await PostAsync<T>(endpoint, data);
                }
                return ApiResponse<T>.ErrorResponse("Unauthorized. Please login again.");
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return errorResponse ?? ApiResponse<T>.ErrorResponse($"Request failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"POST error: {ex.Message}");
            return ApiResponse<T>.ErrorResponse($"Request failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result ?? ApiResponse<T>.ErrorResponse("Invalid response from server");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Try to refresh token
                var refreshResult = await RefreshTokenAsync();
                if (refreshResult.Success)
                {
                    // Retry the request
                    return await PutAsync<T>(endpoint, data);
                }
                return ApiResponse<T>.ErrorResponse("Unauthorized. Please login again.");
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return errorResponse ?? ApiResponse<T>.ErrorResponse($"Request failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PUT error: {ex.Message}");
            return ApiResponse<T>.ErrorResponse($"Request failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            var response = await _httpClient.DeleteAsync(endpoint);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result ?? ApiResponse<T>.ErrorResponse("Invalid response from server");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Try to refresh token
                var refreshResult = await RefreshTokenAsync();
                if (refreshResult.Success)
                {
                    // Retry the request
                    return await DeleteAsync<T>(endpoint);
                }
                return ApiResponse<T>.ErrorResponse("Unauthorized. Please login again.");
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return errorResponse ?? ApiResponse<T>.ErrorResponse($"Request failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DELETE error: {ex.Message}");
            return ApiResponse<T>.ErrorResponse($"Request failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<GradeDTO>> SaveGradeAsync(GradeDTO grade)
    {
        return await PostAsync<GradeDTO>("/api/grades", grade);
    }

    public async Task<ApiResponse<List<GradeDTO>>> GetTermGradesAsync(int termId)
    {
        return await GetAsync<List<GradeDTO>>($"/api/grades/term/{termId}");
    }

    public async Task<ApiResponse<GpaResultDTO>> GetTermGPAAsync(int termId)
    {
        return await GetAsync<GpaResultDTO>($"/api/grades/gpa/{termId}");
    }

    public async Task<ApiResponse<GradeProjectionDTO>> GetGradeProjectionAsync(int courseId, double currentGrade, double finalWeight, string targetGrade)
    {
        var endpoint = $"/api/grades/projection/{courseId}?currentGrade={currentGrade}&finalWeight={finalWeight}&targetGrade={Uri.EscapeDataString(targetGrade)}";
        return await GetAsync<GradeProjectionDTO>(endpoint);
    }

    public async Task<ApiResponse<List<SearchResultDTO>>> SearchCoursesAsync(string query, string? status = null, int? termId = null)
    {
        var endpoint = $"/api/search/courses?query={Uri.EscapeDataString(query)}";
        if (!string.IsNullOrWhiteSpace(status))
        {
            endpoint += $"&status={Uri.EscapeDataString(status)}";
        }
        if (termId.HasValue)
        {
            endpoint += $"&termId={termId.Value}";
        }
        return await GetAsync<List<SearchResultDTO>>(endpoint);
    }

    public async Task<ApiResponse<List<SearchResultDTO>>> SearchTermsAsync(string query)
    {
        var endpoint = $"/api/search/terms?query={Uri.EscapeDataString(query)}";
        return await GetAsync<List<SearchResultDTO>>(endpoint);
    }

    public async Task<ApiResponse<List<SearchResultDTO>>> SearchAllAsync(string query)
    {
        var endpoint = $"/api/search/all?query={Uri.EscapeDataString(query)}";
        return await GetAsync<List<SearchResultDTO>>(endpoint);
    }

    public async Task<ApiResponse<GpaReportDTO>> GetGpaReportAsync(int termId)
    {
        return await GetAsync<GpaReportDTO>($"/api/reports/gpa/{termId}");
    }

    public async Task<byte[]?> DownloadGpaReportCsvAsync(int termId)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            var endpoint = $"/api/reports/gpa/{termId}/csv";
            var response = await _httpClient.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var refreshResult = await RefreshTokenAsync();
                if (refreshResult.Success)
                {
                    return await DownloadGpaReportCsvAsync(termId);
                }
                throw new UnauthorizedAccessException("Unauthorized. Please login again.");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"CSV download error: {response.StatusCode} - {errorContent}");
                return null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CSV download error: {ex.Message}");
            return null;
        }
    }

    public async Task<ApiResponse<TranscriptReportDTO>> GetTranscriptReportAsync()
    {
        return await GetAsync<TranscriptReportDTO>("/api/reports/transcript");
    }

    public async Task<byte[]?> DownloadTranscriptCsvAsync()
    {
        try
        {
            await EnsureAuthenticatedAsync();

            var endpoint = "/api/reports/transcript/csv";
            var response = await _httpClient.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var refreshResult = await RefreshTokenAsync();
                if (refreshResult.Success)
                {
                    return await DownloadTranscriptCsvAsync();
                }
                throw new UnauthorizedAccessException("Unauthorized. Please login again.");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"CSV download error: {response.StatusCode} - {errorContent}");
                return null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CSV download error: {ex.Message}");
            return null;
        }
    }

    private async Task EnsureAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        if (_httpClient.DefaultRequestHeaders.Authorization == null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    #region Financial Tracking Methods

    // Income Methods
    public async Task<ApiResponse<List<IncomeDTO>>> GetIncomesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var endpoint = "/api/financial/income";
        if (startDate.HasValue || endDate.HasValue)
        {
            var queryParams = new List<string>();
            if (startDate.HasValue)
                queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            if (endDate.HasValue)
                queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
            if (queryParams.Any())
                endpoint += "?" + string.Join("&", queryParams);
        }
        return await GetAsync<List<IncomeDTO>>(endpoint);
    }

    public async Task<ApiResponse<IncomeDTO>> GetIncomeAsync(int id)
    {
        return await GetAsync<IncomeDTO>($"/api/financial/income/{id}");
    }

    public async Task<ApiResponse<IncomeDTO>> CreateIncomeAsync(IncomeDTO income)
    {
        return await PostAsync<IncomeDTO>("/api/financial/income", income);
    }

    public async Task<ApiResponse<IncomeDTO>> UpdateIncomeAsync(int id, IncomeDTO income)
    {
        return await PutAsync<IncomeDTO>($"/api/financial/income/{id}", income);
    }

    public async Task<ApiResponse<bool>> DeleteIncomeAsync(int id)
    {
        return await DeleteAsync<bool>($"/api/financial/income/{id}");
    }

    // Expense Methods
    public async Task<ApiResponse<List<ExpenseDTO>>> GetExpensesAsync(DateTime? startDate = null, DateTime? endDate = null, int? categoryId = null)
    {
        var endpoint = "/api/financial/expense";
        var queryParams = new List<string>();
        if (startDate.HasValue)
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        if (endDate.HasValue)
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
        if (categoryId.HasValue)
            queryParams.Add($"categoryId={categoryId.Value}");
        if (queryParams.Any())
            endpoint += "?" + string.Join("&", queryParams);
        return await GetAsync<List<ExpenseDTO>>(endpoint);
    }

    public async Task<ApiResponse<ExpenseDTO>> GetExpenseAsync(int id)
    {
        return await GetAsync<ExpenseDTO>($"/api/financial/expense/{id}");
    }

    public async Task<ApiResponse<ExpenseDTO>> CreateExpenseAsync(ExpenseDTO expense)
    {
        return await PostAsync<ExpenseDTO>("/api/financial/expense", expense);
    }

    public async Task<ApiResponse<ExpenseDTO>> UpdateExpenseAsync(int id, ExpenseDTO expense)
    {
        return await PutAsync<ExpenseDTO>($"/api/financial/expense/{id}", expense);
    }

    public async Task<ApiResponse<bool>> DeleteExpenseAsync(int id)
    {
        return await DeleteAsync<bool>($"/api/financial/expense/{id}");
    }

    // Category Methods
    public async Task<ApiResponse<List<CategoryDTO>>> GetCategoriesAsync()
    {
        return await GetAsync<List<CategoryDTO>>("/api/financial/category");
    }

    public async Task<ApiResponse<CategoryDTO>> GetCategoryAsync(int id)
    {
        return await GetAsync<CategoryDTO>($"/api/financial/category/{id}");
    }

    public async Task<ApiResponse<CategoryDTO>> CreateCategoryAsync(CategoryDTO category)
    {
        return await PostAsync<CategoryDTO>("/api/financial/category", category);
    }

    public async Task<ApiResponse<CategoryDTO>> UpdateCategoryAsync(int id, CategoryDTO category)
    {
        return await PutAsync<CategoryDTO>($"/api/financial/category/{id}", category);
    }

    public async Task<ApiResponse<bool>> DeleteCategoryAsync(int id)
    {
        return await DeleteAsync<bool>($"/api/financial/category/{id}");
    }

    // Summary Method
    public async Task<ApiResponse<FinancialSummaryDTO>> GetFinancialSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var endpoint = "/api/financial/summary";
        var queryParams = new List<string>();
        if (startDate.HasValue)
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        if (endDate.HasValue)
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
        if (queryParams.Any())
            endpoint += "?" + string.Join("&", queryParams);
        return await GetAsync<FinancialSummaryDTO>(endpoint);
    }

    #endregion

    #region Terms and Courses Methods

    // Terms Methods
    public async Task<ApiResponse<List<TermDTO>>> GetTermsAsync()
    {
        return await GetAsync<List<TermDTO>>("/api/terms");
    }

    public async Task<ApiResponse<TermDTO>> GetTermAsync(int id)
    {
        return await GetAsync<TermDTO>($"/api/terms/{id}");
    }

    public async Task<ApiResponse<TermDTO>> CreateTermAsync(TermDTO term)
    {
        return await PostAsync<TermDTO>("/api/terms", term);
    }

    public async Task<ApiResponse<TermDTO>> UpdateTermAsync(int id, TermDTO term)
    {
        return await PutAsync<TermDTO>($"/api/terms/{id}", term);
    }

    public async Task<ApiResponse<bool>> DeleteTermAsync(int id)
    {
        return await DeleteAsync<bool>($"/api/terms/{id}");
    }

    // Courses Methods
    public async Task<ApiResponse<List<CourseDTO>>> GetCoursesByTermAsync(int termId)
    {
        return await GetAsync<List<CourseDTO>>($"/api/courses/term/{termId}");
    }

    public async Task<ApiResponse<CourseDTO>> GetCourseAsync(int id)
    {
        return await GetAsync<CourseDTO>($"/api/courses/{id}");
    }

    public async Task<ApiResponse<CourseDTO>> CreateCourseAsync(CourseDTO course)
    {
        return await PostAsync<CourseDTO>("/api/courses", course);
    }

    public async Task<ApiResponse<CourseDTO>> UpdateCourseAsync(int id, CourseDTO course)
    {
        return await PutAsync<CourseDTO>($"/api/courses/{id}", course);
    }

    public async Task<ApiResponse<bool>> DeleteCourseAsync(int id)
    {
        return await DeleteAsync<bool>($"/api/courses/{id}");
    }

    #endregion
}
