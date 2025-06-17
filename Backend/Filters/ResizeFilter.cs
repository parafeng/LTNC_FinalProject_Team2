using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class ResizeFilter : IImageFilter
    {
        public string Name => "Resize";
        
        public string Description => "Thay đổi kích thước ảnh";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("width", out var widthObj) && 
                int.TryParse(widthObj.ToString(), out int width) &&
                parameters.TryGetValue("height", out var heightObj) && 
                int.TryParse(heightObj.ToString(), out int height))
            {
                // Đảm bảo kích thước hợp lệ
                width = Math.Max(1, width);
                height = Math.Max(1, height);
                
                // Thay đổi kích thước ảnh
                image.Mutate(x => x.Resize(width, height));
            }
            
            return Task.CompletedTask;
        }
    }
} 