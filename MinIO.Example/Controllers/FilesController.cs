using Microsoft.AspNetCore.Mvc;
using MinIO.Example.Services;

namespace MinIO.Example.Controllers;
[Route("api/[controller]")]
[ApiController]
public class FilesController : ControllerBase
{
    private readonly MinioService _minioService;

    public FilesController(MinioService minioService)
    {
        _minioService = minioService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string bucketName)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Dosya seçilmedi.");
        }

        using (var stream = file.OpenReadStream())
        {
            await _minioService.UploadFileAsync(bucketName, file.FileName, file.ContentType, stream);
        }

        return Ok("Dosya yüklendi.");
    }

    [HttpGet("getUrl")]
    public async Task<IActionResult> GetUrl([FromForm] string bucketName, [FromForm] string objectName)
    {
        var url = await _minioService.GetPresignedUrlAsync(bucketName, objectName);

        return Ok(url);
    }
}
