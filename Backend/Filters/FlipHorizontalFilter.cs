using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class FlipHorizontalFilter : IImageFilter
    {
        public string Name => "Flip Horizontal";
        
        public string Description => "Lật ảnh theo chiều ngang";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            image.Mutate(x => x.Flip(FlipMode.Horizontal));
            return Task.CompletedTask;
        }
    }
} 