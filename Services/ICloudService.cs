namespace RecomField.Services;

public interface ICloudService<TFile>
{
    public Task<string> UploadImageAsync(TFile file);
}
