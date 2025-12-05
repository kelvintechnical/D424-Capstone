using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using StudentLifeTracker.Shared.DTOs;

namespace StudentProgressTracker.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://localhost:7119";
    private const string TokenKey = "auth_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string UserKey = "user_data";

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
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
}
