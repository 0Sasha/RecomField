using Microsoft.AspNetCore.Identity;
namespace RecomField.Services;

public interface IUserService<TUser, TCookies, TLanguage>
{
    public Task<TUser> LoadAsync(string userId);

    public Task AddUserCookiesAsync(string? userId, TCookies cookies);

    public Task SaveThemeAsync(string? userId, bool isDark);

    public Task SaveLanguageAsync(string? userId, TLanguage language);

    public Task AddAdminRoleAsync(string userId);

    public Task RevokeAdminRoleAsync(string userId);

    public Task BlockAsync(string userId, int? days);

    public Task UnblockAsync(string userId);

    public Task RemoveAsync(string userId);
}
