using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xixihihi.Application.Interfaces;

namespace Xixihihi.Infrastructure.Services;

public class CloudinaryService : IStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(IOptions<CloudinarySettings> settings, ILogger<CloudinaryService> logger)
    {
        _logger = logger;
        var account = new Account(
            settings.Value.CloudName,
            settings.Value.ApiKey,
            settings.Value.ApiSecret);

        _cloudinary = new Cloudinary(account);
    }

    public async Task<(string Url, string PublicId)> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = "xixihihi_products",
            Transformation = new Transformation()
                .Width(1200).Height(1200)
                .Crop("limit")       // Không crop, chỉ thu nhỏ nếu lớn hơn
                .Quality("auto:good") // Tự động chọn quality tốt nhất
                .FetchFormat("auto")  // Tự convert sang WebP/AVIF nếu browser hỗ trợ
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (uploadResult.Error != null)
        {
            _logger.LogError("Error uploading to Cloudinary: {Error}", uploadResult.Error.Message);
            throw new Exception($"Failed to upload image: {uploadResult.Error.Message}");
        }

        return (uploadResult.SecureUrl.ToString(), uploadResult.PublicId);
    }

    public async Task DeleteFileAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(publicId))
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            
            if (result.Error != null)
            {
                _logger.LogError("Error deleting from Cloudinary: {Error}", result.Error.Message);
            }
        }
    }
}
