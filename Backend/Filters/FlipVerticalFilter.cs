using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class FlipVerticalFilter : IImageFilter
    {
        public string Name => "Flip Vertical";
        
        public string Description => "Lật ảnh theo chiều dọc";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            image.Mutate(x => x.Flip(FlipMode.Vertical));
            return Task.CompletedTask;
        }
    }
} 