using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class BrightnessFilter : IImageFilter
    {
        public string Name => "Brightness";
        
        public string Description => "Điều chỉnh độ sáng của ảnh";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("value", out var valueObj) && 
                float.TryParse(valueObj.ToString(), out float value))
            {
                // Giới hạn giá trị trong khoảng -1 đến 1
                value = Math.Clamp(value, -1f, 1f);
                
                image.Mutate(x => x.Brightness(value));
            }
            
            return Task.CompletedTask;
        }
    }
} 