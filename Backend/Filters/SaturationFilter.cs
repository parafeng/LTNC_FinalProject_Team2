using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class SaturationFilter : IImageFilter
    {
        public string Name => "Saturation";
        
        public string Description => "Điều chỉnh độ bão hòa màu sắc";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("value", out var valueObj) && 
                float.TryParse(valueObj.ToString(), out float value))
            {
                // Giới hạn giá trị trong khoảng -1 đến 2
                value = Math.Clamp(value, -1f, 2f);
                
                image.Mutate(x => x.Saturate(value));
            }
            
            return Task.CompletedTask;
        }
    }
} 