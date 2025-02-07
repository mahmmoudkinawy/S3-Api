using Microsoft.OpenApi.Models;
using S3DemoWebApi.Filters;
using S3DemoWebApi.Services;
using S3DemoWebApi.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<MinioSettings>(builder.Configuration.GetSection("MinIO"));

builder.Services.AddSingleton<IMinioService, MinioService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "S3 API", Version = "v1" });

    options.OperationFilter<FileUploadOperationFilter>();
});

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.MapPost(
        "/upload",
        async (HttpRequest request, IMinioService minioService, IFormFile file) =>
        {
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest("No file uploaded.");
            }

            var fileName = $"{Path.GetRandomFileName()}_{file.FileName}";

            using var fileStream = file.OpenReadStream();
            var fileUrl = await minioService.UploadFileAsync(
                fileStream,
                fileName,
                file.ContentType
            );

            return Results.Ok(new { fileName });
        }
    )
    .WithName("UploadImage")
    .WithOpenApi()
    .DisableAntiforgery();

app.MapGet(
        "/download/{fileName}",
        async (string fileName, IMinioService minioService) =>
        {
            try
            {
                var fileStream = await minioService.DownloadFileAsync(fileName);
                return Results.File(fileStream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }
    )
    .WithName("GetDownloadUrl")
    .WithOpenApi();

app.Run();
