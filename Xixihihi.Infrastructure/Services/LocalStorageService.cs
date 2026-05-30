using Microsoft.Extensions.Configuration;
using Xixihihi.Application.Interfaces;

namespace Xixihihi.Infrastructure.Services;

public class LocalStorageService : IStorageService
{
    private readonly string _uploadDir;
    
    public LocalStorageService(IConfiguration configuration)
    {
        var localPath = configuration["Storage:LocalPath"] ?? "wwwroot/uploads";
        _uploadDir = Path.Combine(Directory.GetCurrentDirectory(), localPath);
        if (!Directory.Exists(_uploadDir))
        {
            Directory.CreateDirectory(_uploadDir);
        }
    }

    public async Task<(string Url, string PublicId)> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var filePath = Path.Combine(_uploadDir, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream, cancellationToken);
        }

        return ($"/uploads/{uniqueFileName}", uniqueFileName);
    }


    public Task DeleteFileAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(publicId)) return Task.CompletedTask;

        var filePath = Path.Combine(_uploadDir, publicId);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}
