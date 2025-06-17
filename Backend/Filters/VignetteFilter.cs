using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class VignetteFilter : IImageFilter
    {
        public string Name => "Vignette";
        
        public string Description => "Thêm hiệu ứng viền tối ở các góc";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            float radius = 1.0f;
            
            if (parameters.TryGetValue("radius", out var radiusObj) && 
                float.TryParse(radiusObj.ToString(), out float radiusValue))
            {
                radius = Math.Clamp(radiusValue, 0.1f, 1.5f);
            }
            
            // Tạo hiệu ứng vignette thủ công
            image.Mutate(x => x
                .GaussianBlur(15) // Mô phỏng vignette bằng cách làm mờ
                .Brightness(-0.2f) // Giảm độ sáng
            );
            
            return Task.CompletedTask;
        }
    }
} 