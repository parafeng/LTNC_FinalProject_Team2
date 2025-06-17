using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using MiniPhotoshop.Backend.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters.BasicFilters
{
    public class BrightnessFilter : IFilter
    {
        public string Name => "Brightness";
        public string Description => "Điều chỉnh độ sáng của ảnh";
        
        public Task<Image> Apply(Image image, Dictionary<string, object> parameters)
        {
            // Chuyển đổi tham số
            float value = ValidationUtils.ConvertToFloat(parameters["value"]);
            
            // Ánh xạ từ [-1, 1] sang [0.1, 1.9]
            float brightnessFactor = 1.0f + (value * 0.9f);
            
            // Đảm bảo giá trị nằm trong khoảng an toàn [0.1, 1.9]
            brightnessFactor = System.Math.Max(0.1f, System.Math.Min(1.9f, brightnessFactor));
            
            // Áp dụng bộ lọc
            image.Mutate(x => x.Brightness(brightnessFactor));
            
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
                    errorMessage = "Giá trị độ sáng phải nằm trong khoảng từ -1 đến 1";
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