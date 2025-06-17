using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters.TransformFilters
{
    public class FlipVerticalFilter : IFilter
    {
        public string Name => "Flip Vertical";
        public string Description => "Lật ảnh theo chiều dọc";
        
        public Task<Image> Apply(Image image, Dictionary<string, object> parameters)
        {
            // Lật ảnh theo chiều dọc
            image.Mutate(x => x.Flip(FlipMode.Vertical));
            
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