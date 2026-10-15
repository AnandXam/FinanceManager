using FinanceManager.Models;

namespace FinanceManager.Services;

/// <summary>
/// Authentication service that handles Google and Apple SSO.
/// Uses platform-specific implementations via Firebase Auth plugin
/// or native platform APIs.
/// </summary>
public class AuthService : IAuthService
{
    private readonly DatabaseService _databaseService;
    private UserProfile? _currentUser;

    public bool IsAuthenticated => _currentUser is not null;
    public event EventHandler<UserProfile?>? AuthStateChanged;

    public AuthService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<UserProfile?> SignInWithGoogleAsync()
    {
        try
        {
#if ANDROID
            // Android: Use Google Sign-In via Firebase
            var authResult = await CrossFirebaseAuth.Current.SignInWithGoogleAsync();
            if (authResult?.User is not null)
            {
                var firebaseUser = authResult.User;
                var user = new UserProfile
                {
                    Id = firebaseUser.Uid,
                    DisplayName = firebaseUser.DisplayName ?? "User",
                    Email = firebaseUser.Email ?? "",
                    PhotoUrl = firebaseUser.PhotoUrl?.ToString() ?? "",
                    AuthProvider = "Google",
                    LastLoginAt = DateTime.UtcNow
                };
                await SaveAndSetCurrentUser(user);
                return user;
            }
#elif IOS || MACCATALYST
            // iOS/Mac: Use Google Sign-In via Firebase
            var authResult = await CrossFirebaseAuth.Current.SignInWithGoogleAsync();
            if (authResult?.User is not null)
            {
                var firebaseUser = authResult.User;
                var user = new UserProfile
                {
                    Id = firebaseUser.Uid,
                    DisplayName = firebaseUser.DisplayName ?? "User",
                    Email = firebaseUser.Email ?? "",
                    PhotoUrl = firebaseUser.PhotoUrl?.ToString() ?? "",
                    AuthProvider = "Google",
                    LastLoginAt = DateTime.UtcNow
                };
                await SaveAndSetCurrentUser(user);
                return user;
            }
#else
            // Fallback for development/testing: use WebAuthenticator
            var result = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri("https://accounts.google.com/o/oauth2/v2/auth"),
                new Uri("com.financemanager.app://callback"));

            if (result is not null)
            {
                var accessToken = result.AccessToken;
                // In production, validate the token and get user info from Google API
                var user = new UserProfile
                {
                    Id = $"google_{Guid.NewGuid():N}",
                    DisplayName = "Google User",
                    Email = result.Properties.GetValueOrDefault("email", "user@gmail.com"),
                    AuthProvider = "Google",
                    LastLoginAt = DateTime.UtcNow
                };
                await SaveAndSetCurrentUser(user);
                return user;
            }
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Google Sign-In failed: {ex.Message}");
            throw new AuthenticationException("Google sign-in failed. Please try again.", ex);
        }

        return null;
    }

    public async Task<UserProfile?> SignInWithAppleAsync()
    {
        try
        {
#if IOS || MACCATALYST
            // iOS/Mac: Use native Apple Sign-In
            var authResult = await CrossFirebaseAuth.Current.SignInWithAppleAsync();
            if (authResult?.User is not null)
            {
                var firebaseUser = authResult.User;
                var user = new UserProfile
                {
                    Id = firebaseUser.Uid,
                    DisplayName = firebaseUser.DisplayName ?? "Apple User",
                    Email = firebaseUser.Email ?? "",
                    AuthProvider = "Apple",
                    LastLoginAt = DateTime.UtcNow
                };
                await SaveAndSetCurrentUser(user);
                return user;
            }
#elif ANDROID
            // Android: Apple Sign-In via Web (OAuth flow)
            var result = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri("https://appleid.apple.com/auth/authorize"),
                new Uri("com.financemanager.app://callback"));

            if (result is not null)
            {
                var user = new UserProfile
                {
                    Id = $"apple_{Guid.NewGuid():N}",
                    DisplayName = "Apple User",
                    Email = result.Properties.GetValueOrDefault("email", "user@icloud.com"),
                    AuthProvider = "Apple",
                    LastLoginAt = DateTime.UtcNow
                };
                await SaveAndSetCurrentUser(user);
                return user;
            }
#else
            // Fallback for development/testing
            var webResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri("https://appleid.apple.com/auth/authorize"),
                new Uri("com.financemanager.app://callback"));

            if (webResult is not null)
            {
                var user = new UserProfile
                {
                    Id = $"apple_{Guid.NewGuid():N}",
                    DisplayName = "Apple User",
                    Email = "user@icloud.com",
                    AuthProvider = "Apple",
                    LastLoginAt = DateTime.UtcNow
                };
                await SaveAndSetCurrentUser(user);
                return user;
            }
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Apple Sign-In failed: {ex.Message}");
            throw new AuthenticationException("Apple sign-in failed. Please try again.", ex);
        }

        return null;
    }

    public async Task SignOutAsync()
    {
        try
        {
#if ANDROID || IOS || MACCATALYST
            await CrossFirebaseAuth.Current.SignOutAsync();
#endif
            _currentUser = null;
            Preferences.Default.Remove("current_user_id");
            AuthStateChanged?.Invoke(this, null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Sign-Out failed: {ex.Message}");
        }
    }

    public async Task<UserProfile?> GetCurrentUserAsync()
    {
        if (_currentUser is not null)
            return _currentUser;

        var userId = Preferences.Default.Get<string>("current_user_id", string.Empty);
        if (!string.IsNullOrEmpty(userId))
        {
            _currentUser = await _databaseService.GetUserAsync(userId);
        }

        return _currentUser;
    }

    private async Task SaveAndSetCurrentUser(UserProfile user)
    {
        await _databaseService.SaveUserAsync(user);
        _currentUser = user;
        Preferences.Default.Set("current_user_id", user.Id);
        AuthStateChanged?.Invoke(this, user);
    }
}

public class AuthenticationException : Exception
{
    public AuthenticationException(string message, Exception? inner = null) : base(message, inner) { }
}
