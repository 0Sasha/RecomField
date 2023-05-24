namespace RecomField.Services;

public interface IUserService<TUser, TCookies, TLanguage>
{
    public Task<TUser> LoadUserAsync(string userId);

    public Task<TUser[]> GetUsersAsync(string type, int count, string? search);

    public Task AddUserCookiesAsync(string? userId, TCookies cookies);

    public Task SaveThemeAsync(string? userId, bool isDark);

    public Task SaveLanguageAsync(string? userId, TLanguage language);

    public Task AddAdminRoleAsync(string userId);

    public Task RevokeAdminRoleAsync(string userId);

    public Task BlockUserAsync(string userId, int? days);

    public Task UnblockUserAsync(string userId);

    public Task RemoveUserAsync(string userId);
}
