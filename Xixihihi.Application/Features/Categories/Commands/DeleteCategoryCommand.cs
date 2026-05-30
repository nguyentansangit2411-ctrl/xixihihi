using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.Categories.Commands;

public class DeleteCategoryCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
