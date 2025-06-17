using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class SepiaFilter : IImageFilter
    {
        public string Name => "Sepia";
        
        public string Description => "Áp dụng hiệu ứng màu sepia cổ điển";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            image.Mutate(x => x.Sepia());
            return Task.CompletedTask;
        }
    }
} 