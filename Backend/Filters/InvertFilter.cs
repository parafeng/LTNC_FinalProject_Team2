using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class InvertFilter : IImageFilter
    {
        public string Name => "Invert";
        
        public string Description => "Đảo ngược màu sắc của ảnh";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            image.Mutate(x => x.Invert());
            return Task.CompletedTask;
        }
    }
} 