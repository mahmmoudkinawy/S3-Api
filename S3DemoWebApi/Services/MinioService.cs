using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Options;
using S3DemoWebApi.Settings;

namespace S3DemoWebApi.Services;

public class MinioService : IMinioService
{
    private readonly AmazonS3Client _s3Client;
    private readonly MinioSettings _minioSettings;

    public MinioService(IOptions<MinioSettings> options)
    {
        _minioSettings = options.Value;

        var config = new AmazonS3Config
        {
            ServiceURL = _minioSettings.Endpoint,
            ForcePathStyle = true
        };

        _s3Client = new AmazonS3Client(_minioSettings.AccessKey, _minioSettings.SecretKey, config);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _minioSettings.BucketName);
            if (!bucketExists)
            {
                await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = _minioSettings.BucketName });
            }

            var putRequest = new PutObjectRequest
            {
                BucketName = _minioSettings.BucketName,
                Key = fileName,
                InputStream = fileStream,
                ContentType = contentType
            };

            await _s3Client.PutObjectAsync(putRequest);

            return $"{_minioSettings.Endpoint}/{_minioSettings.BucketName}/{fileName}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
    
    public async Task<Stream> DownloadFileAsync(string fileName)
    {
        try
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = _minioSettings.BucketName,
                Key = fileName
            };

            var response = await _s3Client.GetObjectAsync(getRequest);
            return response.ResponseStream;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error downloading file: {ex.Message}");
        }
    }
}