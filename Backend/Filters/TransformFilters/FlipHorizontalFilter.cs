using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters.TransformFilters
{
    public class FlipHorizontalFilter : IFilter
    {
        public string Name => "Flip Horizontal";
        public string Description => "Lật ảnh theo chiều ngang";
        
        public Task<Image> Apply(Image image, Dictionary<string, object> parameters)
        {
            // Lật ảnh theo chiều ngang
            image.Mutate(x => x.Flip(FlipMode.Horizontal));
            
            return Task.FromResult(image);
        }
        
        public bool ValidateParameters(Dictionary<string, object> parameters, out string errorMessage)
        {
            // Bộ lọc này không cần tham số
            errorMessage = string.Empty;
            return true;
        }
    }
} 