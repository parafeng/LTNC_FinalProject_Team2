using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class BlurFilter : IImageFilter
    {
        public string Name => "Blur";
        
        public string Description => "Làm mờ ảnh";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("amount", out var amountObj) && 
                float.TryParse(amountObj.ToString(), out float amount))
            {
                // Giới hạn giá trị trong khoảng 1 đến 20
                amount = Math.Clamp(amount, 1f, 20f);
                
                image.Mutate(x => x.GaussianBlur(amount));
            }
            else
            {
                // Mặc định làm mờ với cài đặt mặc định
                image.Mutate(x => x.GaussianBlur(5f));
            }
            
            return Task.CompletedTask;
        }
    }
} 