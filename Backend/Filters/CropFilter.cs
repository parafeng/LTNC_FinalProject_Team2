using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class CropFilter : IImageFilter
    {
        public string Name => "Crop";
        
        public string Description => "Cắt một phần của ảnh";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("x", out var xObj) && 
                int.TryParse(xObj.ToString(), out int x) &&
                parameters.TryGetValue("y", out var yObj) && 
                int.TryParse(yObj.ToString(), out int y) &&
                parameters.TryGetValue("width", out var widthObj) && 
                int.TryParse(widthObj.ToString(), out int width) &&
                parameters.TryGetValue("height", out var heightObj) && 
                int.TryParse(heightObj.ToString(), out int height))
            {
                // Đảm bảo tọa độ và kích thước hợp lệ
                x = Math.Max(0, x);
                y = Math.Max(0, y);
                width = Math.Max(1, Math.Min(width, image.Width - x));
                height = Math.Max(1, Math.Min(height, image.Height - y));
                
                // Cắt ảnh
                var rectangle = new Rectangle(x, y, width, height);
                image.Mutate(i => i.Crop(rectangle));
            }
            
            return Task.CompletedTask;
        }
    }
} 