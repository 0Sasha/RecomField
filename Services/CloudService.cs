using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using NuGet.Protocol;
namespace RecomField.Services;

public class CloudService : ICloudService<IFormFile>
{
    private readonly Cloudinary cloud;

    public CloudService(Cloudinary cloud) => this.cloud = cloud;

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file.Length > 500000) return new { error = "The file size is over 0.5MB" }.ToJson();
        else if (!file.ContentType.StartsWith("image") ||
            file.ContentType[6..] != "jpeg" && file.ContentType[6..] != "jpg" && file.ContentType[6..] != "png")
            return new { error = "Incorrect format of the file" }.ToJson();

        var uploadParams = new ImageUploadParams() { File = new FileDescription("file", file.OpenReadStream()) };
        var uploadResult = await cloud.UploadAsync(uploadParams);
        return uploadResult.StatusCode == System.Net.HttpStatusCode.OK ?
                new { location = uploadResult.Url }.ToJson() : new { error = "Error during upload" }.ToJson();
    }
}
