using Minio.DataModel.Args;
using Minio.Exceptions;
using Minio;

namespace MinIO.Example.Services;

public class MinioService
{
    private readonly IMinioClient _minioClient;
    private readonly string _endpoint;

    public MinioService(IConfiguration configuration)
    {
        _endpoint = configuration["Minio:Endpoint"]!;
        var accessKey = configuration["Minio:AccessKey"];
        var secretKey = configuration["Minio:SecretKey"];
        var security = bool.Parse(configuration["Minio:Secure"]!);

        _minioClient = new MinioClient()
            .WithEndpoint(_endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(security)
            .Build();
    }

    public async Task UploadFileAsync(string bucketName, string objectName, string contentType, Stream data)
    {
        try
        {
            if (!await BucketExistsAsync(bucketName))
            {
                var mbArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);
            }

            await UploadObjectAsync(bucketName, objectName, contentType, data);
            Console.WriteLine($"Successfully uploaded {objectName}");
        }
        catch (MinioException e)
        {
            Console.WriteLine($"File Upload Error: {e.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task<string> GetPresignedUrlAsync(string bucketName, string objectName)
    {
        //Buradaki yapıda düzeltmeler yapılacak. ObjName olmasa bile gelen datayı dönüyor.
        if (await BucketExistsAsync(bucketName))
        {
            return $"http://{_endpoint}/{bucketName}/{objectName}"; // public bucket olması gerek
        }
        return "Url yok";

    }

    private async Task<bool> BucketExistsAsync(string bucketName)
    {
        var beArgs = new BucketExistsArgs().WithBucket(bucketName);
        return await _minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);
    }

    private async Task UploadObjectAsync(string bucketName, string objectName, string contentType, Stream data)
    {
        // Yeniden isimlendirme ile aynı isim sorununu ortadan kaldırdım.
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(objectName)}";

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(fileName)
            .WithStreamData(data)
            .WithObjectSize(data.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
    }
}
