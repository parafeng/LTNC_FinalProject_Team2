using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class GrayscaleFilter : IImageFilter
    {
        public string Name => "Grayscale";
        
        public string Description => "Chuyển ảnh sang thang màu xám";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            image.Mutate(x => x.Grayscale());
            return Task.CompletedTask;
        }
    }
} 