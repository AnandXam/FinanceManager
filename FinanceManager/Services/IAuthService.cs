using FinanceManager.Models;

namespace FinanceManager.Services;

public interface IAuthService
{
    Task<UserProfile?> SignInWithGoogleAsync();
    Task<UserProfile?> SignInWithAppleAsync();
    Task SignOutAsync();
    Task<UserProfile?> GetCurrentUserAsync();
    bool IsAuthenticated { get; }
    event EventHandler<UserProfile?>? AuthStateChanged;
}
