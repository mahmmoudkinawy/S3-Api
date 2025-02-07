namespace S3DemoWebApi.Services;

public interface IMinioService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<Stream> DownloadFileAsync(string fileName);
}