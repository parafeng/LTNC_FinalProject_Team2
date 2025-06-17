using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class GlowFilter : IImageFilter
    {
        public string Name => "Glow";
        
        public string Description => "Thêm hiệu ứng phát sáng cho ảnh";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            float amount = 0.5f;
            
            if (parameters.TryGetValue("amount", out var amountObj) && 
                float.TryParse(amountObj.ToString(), out float amountValue))
            {
                amount = Math.Clamp(amountValue, 0.1f, 1.0f);
            }
            
            // Tạo hiệu ứng phát sáng bằng cách làm mờ và tăng độ sáng
            image.Mutate(x => x
                .GaussianBlur(10.0f)     // Làm mờ nhiều để tạo hiệu ứng
                .Brightness(0.3f)        // Tăng độ sáng
                .Saturate(1.2f)          // Tăng độ bão hòa màu
            );
            
            return Task.CompletedTask;
        }
    }
} 