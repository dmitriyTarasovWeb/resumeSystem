using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace resumeSystem.Services;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    private const long MaxFileSize = 5 * 1024 * 1024;

    private static readonly string[] AllowedExtensions =
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    public CloudinaryService(Cloudinary cloudinary)
    {
        _cloudinary = cloudinary;
    }

    public async Task<string> UploadAvatarAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException(
                "Файл изображения не выбран.");
        }

        if (file.Length > MaxFileSize)
        {
            throw new ArgumentException(
                "Размер изображения не должен превышать 5 МБ.");
        }

        var extension =
            Path.GetExtension(file.FileName)
                .ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException(
                "Разрешены только JPG, JPEG, PNG и WebP.");
        }

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(
                file.FileName,
                stream),

            Folder = "resume-system/avatars",

            PublicId = Guid.NewGuid().ToString(),

            Overwrite = false
        };

        var result =
            await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
        {
            throw new InvalidOperationException(
                result.Error.Message);
        }

        return result.SecureUrl.ToString();
    }
}