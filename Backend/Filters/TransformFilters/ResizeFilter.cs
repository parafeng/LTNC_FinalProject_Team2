using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using MiniPhotoshop.Backend.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters.TransformFilters
{
    public class ResizeFilter : IFilter
    {
        public string Name => "Resize";
        public string Description => "Thay đổi kích thước ảnh";
        
        public Task<Image> Apply(Image image, Dictionary<string, object> parameters)
        {
            // Chuyển đổi tham số
            int width = ValidationUtils.ConvertToInt(parameters["width"]);
            int height = ValidationUtils.ConvertToInt(parameters["height"]);
            
            // Áp dụng bộ lọc
            image.Mutate(x => x.Resize(width, height));
            
            return Task.FromResult(image);
        }
        
        public bool ValidateParameters(Dictionary<string, object> parameters, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            // Kiểm tra tham số
            if (!parameters.ContainsKey("width"))
            {
                errorMessage = "Thiếu tham số 'width'";
                return false;
            }
            
            if (!parameters.ContainsKey("height"))
            {
                errorMessage = "Thiếu tham số 'height'";
                return false;
            }
            
            try
            {
                int width = ValidationUtils.ConvertToInt(parameters["width"]);
                int height = ValidationUtils.ConvertToInt(parameters["height"]);
                
                // Kiểm tra giá trị
                if (width <= 0)
                {
                    errorMessage = "Chiều rộng phải lớn hơn 0";
                    return false;
                }
                
                if (height <= 0)
                {
                    errorMessage = "Chiều cao phải lớn hơn 0";
                    return false;
                }
                
                // Giới hạn kích thước tối đa để tránh sử dụng quá nhiều bộ nhớ
                if (width > 5000 || height > 5000)
                {
                    errorMessage = "Kích thước tối đa cho phép là 5000x5000 pixel";
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