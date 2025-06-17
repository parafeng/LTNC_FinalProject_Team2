using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class HueFilter : IImageFilter
    {
        public string Name => "Hue";
        
        public string Description => "Thay đổi sắc độ màu sắc";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("value", out var valueObj) && 
                float.TryParse(valueObj.ToString(), out float value))
            {
                // Giới hạn giá trị trong khoảng 0 đến 360 (độ)
                value = Math.Clamp(value, 0f, 360f);
                
                // Chuyển đổi từ độ sang radian
                float radians = value * MathF.PI / 180f;
                
                image.Mutate(x => x.Hue(radians));
            }
            
            return Task.CompletedTask;
        }
    }
} 