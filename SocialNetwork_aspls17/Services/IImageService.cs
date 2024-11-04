namespace SocialNetwork_aspls17.Services
{
    public interface IImageService
    {
        Task<string> SaveFile(IFormFile file);
    }
}
