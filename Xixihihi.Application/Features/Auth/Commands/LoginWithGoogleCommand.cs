using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.Auth.Commands;

public class LoginWithGoogleCommand : IRequest<ApiResponse<AuthResponse>>
{
    public string Token { get; set; } = string.Empty;
}
