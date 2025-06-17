# MiniPhotoshop

Ứng dụng chỉnh sửa ảnh với nhiều bộ lọc và tính năng tạo ảnh bằng AI.

## Tính năng

- Chỉnh sửa ảnh với nhiều bộ lọc khác nhau
- Tạo ảnh mới bằng AI từ mô tả văn bản
- Lưu và tải xuống ảnh đã chỉnh sửa

## Cài đặt

### Yêu cầu

- .NET 6.0 SDK trở lên
- Docker (tùy chọn)

### Chạy ứng dụng cục bộ

1. Clone repository
2. Thiết lập biến môi trường:
   - `IMAGE_API_KEY`: API key cho 4oimageapi.io
   - `IMGBB_API_KEY`: API key cho imgbb.com (tùy chọn)
3. Chạy ứng dụng:
   ```
   dotnet run
   ```

### Chạy với Docker

```
docker build -t miniphotoshop .
docker run -p 8080:8080 -e IMAGE_API_KEY=your_api_key miniphotoshop
```

## Deploy lên Render.com

1. Đăng ký tài khoản tại [Render.com](https://render.com/)
2. Tạo một Web Service mới từ repository GitHub
3. Chọn "Build and deploy from a Git repository"
4. Cấu hình:
   - **Environment**: Docker
   - **Build Command**: (để trống)
   - **Start Command**: (để trống)
5. Thêm biến môi trường:
   - `IMAGE_API_KEY`: API key cho 4oimageapi.io
   - `IMGBB_API_KEY`: API key cho imgbb.com (tùy chọn)
6. Click "Create Web Service"

## Lưu ý

- Ứng dụng sẽ tự động "ngủ" sau 15 phút không hoạt động khi sử dụng tier miễn phí của Render.com
- Lần đầu tiên truy cập sau thời gian không hoạt động có thể mất 30-60 giây để khởi động lại

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