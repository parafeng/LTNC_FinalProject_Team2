using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class BoxBlurFilter : IImageFilter
    {
        public string Name => "BoxBlur";
        
        public string Description => "Làm mờ dạng hộp";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("radius", out var radiusObj) && 
                int.TryParse(radiusObj.ToString(), out int radius))
            {
                // Giới hạn bán kính trong khoảng 1 đến 20
                radius = Math.Clamp(radius, 1, 20);
                
                image.Mutate(x => x.BoxBlur(radius));
            }
            else
            {
                // Mặc định làm mờ với bán kính = 5
                image.Mutate(x => x.BoxBlur(5));
            }
            
            return Task.CompletedTask;
        }
    }
} 