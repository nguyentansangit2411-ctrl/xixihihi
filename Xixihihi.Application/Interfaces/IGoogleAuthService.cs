namespace Xixihihi.Application.Interfaces;

public class GoogleUserInfo
{
    public string ProviderKey { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
}

public interface IGoogleAuthService
{
    Task<GoogleUserInfo?> ValidateTokenAsync(string idToken, CancellationToken cancellationToken = default);
}
