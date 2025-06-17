using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using MiniPhotoshop.Backend.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters.TransformFilters
{
    public class RotateFilter : IFilter
    {
        public string Name => "Rotate";
        public string Description => "Xoay ảnh theo góc đã chọn";
        
        public Task<Image> Apply(Image image, Dictionary<string, object> parameters)
        {
            // Chuyển đổi tham số
            int angle = ValidationUtils.ConvertToInt(parameters["angle"]);
            
            // Áp dụng bộ lọc
            image.Mutate(x => x.Rotate(angle));
            
            return Task.FromResult(image);
        }
        
        public bool ValidateParameters(Dictionary<string, object> parameters, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            // Kiểm tra tham số
            if (!parameters.ContainsKey("angle"))
            {
                errorMessage = "Thiếu tham số 'angle'";
                return false;
            }
            
            try
            {
                int angle = ValidationUtils.ConvertToInt(parameters["angle"]);
                
                // Không cần giới hạn góc xoay vì ImageSharp có thể xử lý mọi giá trị góc
                
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