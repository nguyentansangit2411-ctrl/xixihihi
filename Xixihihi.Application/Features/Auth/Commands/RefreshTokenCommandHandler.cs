using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Application.Interfaces;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

using Xixihihi.Domain.Interfaces;

namespace Xixihihi.Application.Features.Auth.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IBaseRepository<User> _userRepository;

    public RefreshTokenCommandHandler(IJwtService jwtService, IBaseRepository<User> userRepository, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hashedToken = _jwtService.HashToken(request.RefreshToken);
        var user = await _userRepository.GetFirstOrDefaultAsync(u => u.RefreshToken == hashedToken, cancellationToken);

        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = _jwtService.HashToken(refreshToken);
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);


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

        return ApiResponse<AuthResponse>.SuccessResponse(response, "Token refreshed successfully.");
    }
}

