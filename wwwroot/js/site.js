// Site.js - Common JavaScript functions for MiniPhotoshop

// Hàm tiện ích để tải nội dung từ Frontend/assets/js/app.js vào thẻ script
function loadScript(url) {
    return new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = url;
        script.onload = resolve;
        script.onerror = reject;
        document.head.appendChild(script);
    });
}

// Khởi tạo nút download để tải ảnh đã xử lý
document.addEventListener('DOMContentLoaded', function() {
    const downloadButton = document.getElementById('download-button');
    if (downloadButton) {
        downloadButton.addEventListener('click', function() {
            const image = document.getElementById('preview-image');
            if (image && image.src) {
                // Tạo thẻ a tạm thời để tải xuống
                const link = document.createElement('a');
                link.href = image.src;
                link.download = 'miniphotoshop_' + Date.now() + '.jpg';
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            }
        });
    }

    // Thêm chức năng đóng các alert tự động
    document.querySelectorAll('.alert .btn-close').forEach(button => {
        button.addEventListener('click', function() {
            const alert = this.closest('.alert');
            alert.classList.add('d-none');
        });
    });
}); 