using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters.BasicFilters
{
    public class GrayscaleFilter : IFilter
    {
        public string Name => "Grayscale";
        public string Description => "Chuyển đổi ảnh sang thang độ xám";
        
        public Task<Image> Apply(Image image, Dictionary<string, object> parameters)
        {
            // Áp dụng bộ lọc grayscale
            image.Mutate(x => x.Grayscale());
            
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