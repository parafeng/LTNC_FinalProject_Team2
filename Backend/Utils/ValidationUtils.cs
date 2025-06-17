using System;
using System.Text.Json;

namespace MiniPhotoshop.Backend.Utils
{
    public static class ValidationUtils
    {
        public static int ConvertToInt(object value)
        {
            try
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "Giá trị không được phép là null");
                    
                // Xử lý JsonElement (khi nhận từ System.Text.Json)
                if (value is JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        return jsonElement.GetInt32();
                    }
                    else if (jsonElement.ValueKind == JsonValueKind.String)
                    {
                        if (int.TryParse(jsonElement.GetString(), out int result))
                        {
                            return result;
                        }
                        throw new FormatException($"Không thể chuyển đổi chuỗi '{jsonElement.GetString()}' thành số nguyên");
                    }
                    throw new FormatException($"Không thể chuyển đổi JsonElement kiểu {jsonElement.ValueKind} thành số nguyên");
                }
                
                // Xử lý các kiểu khác
                return Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Không thể chuyển đổi '{value}' thành số nguyên: {ex.Message}", ex);
            }
        }
        
        public static float ConvertToFloat(object value)
        {
            try
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "Giá trị không được phép là null");
                    
                // Xử lý JsonElement (khi nhận từ System.Text.Json)
                if (value is JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        return jsonElement.GetSingle();
                    }
                    else if (jsonElement.ValueKind == JsonValueKind.String)
                    {
                        if (float.TryParse(jsonElement.GetString(), out float result))
                        {
                            return result;
                        }
                        throw new FormatException($"Không thể chuyển đổi chuỗi '{jsonElement.GetString()}' thành số thực");
                    }
                    throw new FormatException($"Không thể chuyển đổi JsonElement kiểu {jsonElement.ValueKind} thành số thực");
                }
                
                // Xử lý các kiểu khác
                return Convert.ToSingle(value);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Không thể chuyển đổi '{value}' thành số thực: {ex.Message}", ex);
            }
        }
    }
} 