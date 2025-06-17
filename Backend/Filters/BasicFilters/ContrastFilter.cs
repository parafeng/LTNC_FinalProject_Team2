using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using MiniPhotoshop.Backend.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters.BasicFilters
{
    public class ContrastFilter : IFilter
    {
        public string Name => "Contrast";
        public string Description => "Điều chỉnh độ tương phản của ảnh";
        
        public Task<Image> Apply(Image image, Dictionary<string, object> parameters)
        {
            // Chuyển đổi tham số
            float value = ValidationUtils.ConvertToFloat(parameters["value"]);
            
            // Ánh xạ từ [-1, 1] sang [0.5, 2.5]
            float contrastFactor = 1.0f + value;
            
            // Đảm bảo giá trị nằm trong khoảng an toàn [0.5, 2.5]
            contrastFactor = System.Math.Max(0.5f, System.Math.Min(2.5f, contrastFactor));
            
            // Áp dụng bộ lọc
            image.Mutate(x => x.Contrast(contrastFactor));
            
            return Task.FromResult(image);
        }
        
        public bool ValidateParameters(Dictionary<string, object> parameters, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            // Kiểm tra tham số
            if (!parameters.ContainsKey("value"))
            {
                errorMessage = "Thiếu tham số 'value'";
                return false;
            }
            
            try
            {
                float value = ValidationUtils.ConvertToFloat(parameters["value"]);
                
                // Kiểm tra giá trị trong khoảng hợp lệ
                if (value < -1 || value > 1)
                {
                    errorMessage = "Giá trị độ tương phản phải nằm trong khoảng từ -1 đến 1";
                    return false;
                }
                
                return true;
            }
            catch (System.Exception ex)
            {
                errorMessage = $"Lỗi tham số: {ex.Message}";
                return false;
            }
        }
    }
} 