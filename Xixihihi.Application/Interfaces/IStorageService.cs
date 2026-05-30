namespace Xixihihi.Application.Interfaces;

public interface IStorageService
{
    Task<(string Url, string PublicId)> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string publicId, CancellationToken cancellationToken = default);
}
