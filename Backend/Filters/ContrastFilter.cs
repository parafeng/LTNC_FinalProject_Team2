using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class ContrastFilter : IImageFilter
    {
        public string Name => "Contrast";
        
        public string Description => "Điều chỉnh độ tương phản của ảnh";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("value", out var valueObj) && 
                float.TryParse(valueObj.ToString(), out float value))
            {
                // Giới hạn giá trị trong khoảng -1 đến 1
                value = Math.Clamp(value, -1f, 1f);
                
                image.Mutate(x => x.Contrast(value));
            }
            
            return Task.CompletedTask;
        }
    }
} 