 
namespace SocialNetwork_aspls17.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment? _webHostEnvironment;

        public ImageService(IWebHostEnvironment? webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> SaveFile(IFormFile file)
        {
            var saveImage = Path.Combine(_webHostEnvironment.WebRootPath, "images", file.FileName);
            using (var img=new FileStream(saveImage,FileMode.OpenOrCreate))
            {
                await file.CopyToAsync(img);
            }
            return file.FileName.ToString();
        }


    }
}
