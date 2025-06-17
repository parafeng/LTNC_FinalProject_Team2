using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class SharpenFilter : IImageFilter
    {
        public string Name => "Sharpen";
        
        public string Description => "Tăng độ sắc nét của ảnh";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("amount", out var amountObj) && 
                float.TryParse(amountObj.ToString(), out float amount))
            {
                // Giới hạn giá trị trong khoảng 0 đến 10
                amount = Math.Clamp(amount, 0f, 10f);
                
                // Mô phỏng tăng độ sắc nét bằng cách tăng độ tương phản
                image.Mutate(x => x.Contrast(amount * 0.1f));
            }
            else
            {
                // Mặc định tăng độ sắc nét với cài đặt mặc định
                image.Mutate(x => x.Contrast(0.5f));
            }
            
            return Task.CompletedTask;
        }
    }
} 