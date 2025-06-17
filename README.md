# MiniPhotoshop - Ứng dụng chỉnh sửa ảnh

Ứng dụng MiniPhotoshop là một công cụ chỉnh sửa ảnh đơn giản với các tính năng cơ bản và tích hợp AI.

## Tính năng

- Tải lên và chỉnh sửa ảnh
- Các bộ lọc cơ bản: Grayscale, Sepia, Invert
- Điều chỉnh màu sắc và độ sáng: Brightness, Contrast, Saturation, Hue, Gamma
- Các biến đổi hình học: Resize, Rotate, Flip, Crop
- Các hiệu ứng đặc biệt: Blur, Sharpen, Pixelate, OilPaint
- **AI Image Editor**: Chỉnh sửa ảnh bằng ngôn ngữ tự nhiên sử dụng 4oimageapi.io

## Cài đặt

1. Clone repository
2. Cài đặt .NET SDK 8.0 hoặc cao hơn
3. Cấu hình API key cho 4oimageapi.io trong file `appsettings.json` hoặc biến môi trường

```json
{
  "IMAGE_API_KEY": "YOUR_4OIMAGEAPI_KEY_HERE"
}
```

4. Chạy ứng dụng
   ```bash
   dotnet run
   ```

## Sử dụng AI Image Editor

1. Tải lên ảnh của bạn
2. Nhấp vào nút AI Image Editor ở góc dưới bên phải
3. Nhập mô tả về cách bạn muốn chỉnh sửa ảnh, ví dụ:
   - "Thêm một hoàng hôn vào nền"
   - "Làm cho ảnh trông như tranh vẽ"
   - "Thay đổi thành cảnh đêm"
   - "Thêm núi vào nền"
   - "Làm cho ảnh ấm hơn và tăng độ tương phản"
4. Nhấp vào "Apply AI Edit" và đợi kết quả

## Yêu cầu hệ thống

- .NET SDK 8.0 hoặc cao hơn
- API key của 4oimageapi.io

## Lấy API key 4oimageapi.io

1. Truy cập [4oimageapi.io](https://4oimageapi.io/vi/api-key)
2. Đăng ký hoặc đăng nhập vào tài khoản
3. Tạo một API key mới
4. Sao chép API key và thêm vào cấu hình ứng dụng trong file appsettings.json

## Giấy phép

MIT License 