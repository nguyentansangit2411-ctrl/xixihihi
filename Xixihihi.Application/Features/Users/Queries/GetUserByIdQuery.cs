using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.Users.Queries;

public class GetUserByIdQuery : IRequest<ApiResponse<UserDto>>
{
    public Guid UserId { get; set; }
}
