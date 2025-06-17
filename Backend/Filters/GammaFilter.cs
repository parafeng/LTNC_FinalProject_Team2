using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class GammaFilter : IImageFilter
    {
        public string Name => "Gamma";
        
        public string Description => "Điều chỉnh gamma của ảnh";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("value", out var valueObj) && 
                float.TryParse(valueObj.ToString(), out float value))
            {
                // Giới hạn giá trị trong khoảng 0.1 đến 5
                value = Math.Clamp(value, 0.1f, 5f);
                
                // Mô phỏng hiệu ứng gamma bằng cách kết hợp các hiệu ứng khác
                image.Mutate(x => x
                    .Brightness(value - 1f)
                    .Contrast(value - 1f)
                );
            }
            
            return Task.CompletedTask;
        }
    }
} 