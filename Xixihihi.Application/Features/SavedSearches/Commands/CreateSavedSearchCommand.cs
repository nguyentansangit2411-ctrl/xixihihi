using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.SavedSearches.Commands;

public class CreateSavedSearchCommand : IRequest<ApiResponse<SavedSearchDto>>
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public Guid? ProvinceId { get; set; }
}
