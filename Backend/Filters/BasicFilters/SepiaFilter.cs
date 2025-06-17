using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters.BasicFilters
{
    public class SepiaFilter : IFilter
    {
        public string Name => "Sepia";
        public string Description => "Áp dụng hiệu ứng màu sepia (nâu cổ điển)";
        
        public Task<Image> Apply(Image image, Dictionary<string, object> parameters)
        {
            // Áp dụng hiệu ứng sepia
            image.Mutate(x => x.Sepia());
            
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