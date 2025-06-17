using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class RotateFilter : IImageFilter
    {
        public string Name => "Rotate";
        
        public string Description => "Xoay ảnh theo góc độ";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("angle", out var angleObj) && 
                float.TryParse(angleObj.ToString(), out float angle))
            {
                // Xoay ảnh theo góc độ
                image.Mutate(x => x.Rotate(angle));
            }
            
            return Task.CompletedTask;
        }
    }
} 