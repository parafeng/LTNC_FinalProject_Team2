using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using MiniPhotoshop.Backend.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters.TransformFilters
{
    public class CropFilter : IFilter
    {
        public string Name => "Crop";
        public string Description => "Cắt một phần của ảnh";
        
        public Task<Image> Apply(Image image, Dictionary<string, object> parameters)
        {
            // Chuyển đổi tham số
            int x = ValidationUtils.ConvertToInt(parameters["x"]);
            int y = ValidationUtils.ConvertToInt(parameters["y"]);
            int width = ValidationUtils.ConvertToInt(parameters["width"]);
            int height = ValidationUtils.ConvertToInt(parameters["height"]);
            
            // Tạo rectangle để cắt
            var rectangle = new Rectangle(x, y, width, height);
            
            // Áp dụng bộ lọc
            image.Mutate(ctx => ctx.Crop(rectangle));
            
            return Task.FromResult(image);
        }
        
        public bool ValidateParameters(Dictionary<string, object> parameters, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            // Kiểm tra đầy đủ tham số
            if (!parameters.ContainsKey("x"))
            {
                errorMessage = "Thiếu tham số 'x'";
                return false;
            }
            
            if (!parameters.ContainsKey("y"))
            {
                errorMessage = "Thiếu tham số 'y'";
                return false;
            }
            
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
                int x = ValidationUtils.ConvertToInt(parameters["x"]);
                int y = ValidationUtils.ConvertToInt(parameters["y"]);
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
                
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Lỗi tham số: {ex.Message}";
                return false;
            }
        }
    }
} 