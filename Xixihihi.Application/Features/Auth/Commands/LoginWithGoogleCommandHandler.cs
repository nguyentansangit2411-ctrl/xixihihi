using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Application.Interfaces;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Enums;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

using Xixihihi.Domain.Interfaces;

namespace Xixihihi.Application.Features.Auth.Commands;

public class LoginWithGoogleCommandHandler : IRequestHandler<LoginWithGoogleCommand, ApiResponse<AuthResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IJwtService _jwtService;
    private readonly IBaseRepository<User> _userRepository;

    public LoginWithGoogleCommandHandler(
        IGoogleAuthService googleAuthService,
        IJwtService jwtService,
        IBaseRepository<User> userRepository,
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _googleAuthService = googleAuthService;
        _jwtService = jwtService;
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(LoginWithGoogleCommand request, CancellationToken cancellationToken)
    {
        var googleUserInfo = await _googleAuthService.ValidateTokenAsync(request.Token, cancellationToken);
        if (googleUserInfo == null)
        {
            throw new UnauthorizedException("Invalid Google token.");
        }

        // Tìm user theo Email
        var user = await _userRepository.GetFirstOrDefaultAsync(u => u.Email == googleUserInfo.Email, cancellationToken);

        if (user == null)
        {
            user = new User
            {
                Email = googleUserInfo.Email,
                DisplayName = googleUserInfo.Name,
                AvatarUrl = googleUserInfo.PictureUrl,
                Role = UserRole.Buyer, // Mặc định là Buyer khi đăng ký mới
                Status = UserStatus.Active,
                ExternalLogins = new List<ExternalLogin>
                {
                    new ExternalLogin
                    {
                        Provider = "Google",
                        ProviderKey = googleUserInfo.ProviderKey
                    }
                }
            };
            await _userRepository.AddAsync(user, cancellationToken);
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = _jwtService.HashToken(refreshToken);
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Dựa theo config ở CLAUDE.md


        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role,
            Status = user.Status
        };

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = userDto
        };

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<AuthResponse>.SuccessResponse(response, "Login successful.");
    }
}

