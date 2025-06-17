// Biến toàn cục
let previewTimeout;
const PREVIEW_DELAY = 300; // milliseconds

document.addEventListener('DOMContentLoaded', function() {
    // Kích hoạt xem trước khi thay đổi giá trị của input range
    document.querySelectorAll('.form-range').forEach(range => {
        range.addEventListener('input', function() {
            const form = this.closest('form');
            if (form) {
                // Hiển thị giá trị hiện tại (nếu có)
                const valueDisplay = form.querySelector('.range-value');
                if (valueDisplay) {
                    valueDisplay.textContent = this.value;
                }
                
                // Xem trước bộ lọc
                clearTimeout(previewTimeout);
                previewTimeout = setTimeout(() => {
                    previewFilter(form);
                }, PREVIEW_DELAY);
            }
        });
    });

    // Kích hoạt xem trước khi thay đổi giá trị của input number
    document.querySelectorAll('input[type="number"]').forEach(input => {
        input.addEventListener('input', function() {
            const form = this.closest('form');
            if (form) {
                clearTimeout(previewTimeout);
                previewTimeout = setTimeout(() => {
                    previewFilter(form);
                }, PREVIEW_DELAY);
            }
        });
    });

    // Điều chỉnh kích thước của phần xem trước khi cửa sổ thay đổi kích thước
    window.addEventListener('resize', adjustPreviewHeight);
    
    // Điều chỉnh kích thước ban đầu
    adjustPreviewHeight();
    
    // Xử lý sự kiện cuộn trang
    window.addEventListener('scroll', function() {
        const previewCard = document.querySelector('.sticky-top');
        if (previewCard) {
            // Thêm hiệu ứng khi cuộn trang
            if (window.scrollY > 100) {
                previewCard.style.boxShadow = '0 10px 20px rgba(0,0,0,0.15)';
            } else {
                previewCard.style.boxShadow = '0 4px 8px rgba(0,0,0,0.1)';
            }
        }
    });
    
    // Tải danh sách các bộ lọc đã áp dụng
    loadAppliedFilters();
    
    // Theo dõi form đặt lại ảnh
    document.querySelectorAll('form[action="/api/image/reset"]').forEach(form => {
        form.addEventListener('submit', function(e) {
            // Ngăn chặn hành vi mặc định của form
            e.preventDefault();
            
            // Hiển thị trạng thái đang xử lý
            const submitButton = form.querySelector('button[type="submit"]');
            if (submitButton) {
                const originalText = submitButton.innerHTML;
                submitButton.disabled = true;
                submitButton.innerHTML = `
                    <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                    Đang xử lý...
                `;
                
                // Gửi yêu cầu đặt lại ảnh bằng fetch API
                fetch('/api/image/reset', {
                    method: 'POST',
                    body: new FormData(form)
                })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Lỗi khi đặt lại ảnh');
                    }
                    return response.text();
                })
                .then(() => {
                    // Tải lại trang sau khi đặt lại ảnh thành công
                    window.location.reload();
                })
                .catch(error => {
                    console.error('Lỗi:', error);
                    alert('Lỗi khi đặt lại ảnh: ' + error.message);
                    
                    // Khôi phục nút submit
                    submitButton.disabled = false;
                    submitButton.innerHTML = originalText;
                });
            } else {
                // Nếu không tìm thấy nút submit, gửi form theo cách thông thường
                form.submit();
            }
        });
    });
    
    // Theo dõi các form áp dụng bộ lọc
    document.querySelectorAll('form[action="/api/image/apply"]').forEach(form => {
        form.addEventListener('submit', function(e) {
            // Không ngăn chặn form submit, để nó chuyển hướng bình thường
            // Nhưng lưu thông tin để biết rằng cần tải lại danh sách bộ lọc sau khi tải lại trang
            sessionStorage.setItem('reloadAppliedFilters', 'true');
        });
    });
    
    // Kiểm tra xem có cần tải lại danh sách bộ lọc không
    if (sessionStorage.getItem('reloadAppliedFilters') === 'true') {
        sessionStorage.removeItem('reloadAppliedFilters');
        loadAppliedFilters();
    }
});

// Hàm tải danh sách các bộ lọc đã áp dụng
function loadAppliedFilters() {
    const appliedFiltersList = document.getElementById('appliedFiltersList');
    if (!appliedFiltersList) return;
    
    const currentImagePath = document.querySelector('input[name="imagePath"]')?.value;
    if (!currentImagePath) return;
    
    // Hiển thị trạng thái đang tải
    appliedFiltersList.innerHTML = `
        <div class="list-group-item text-center">
            <div class="spinner-border spinner-border-sm text-primary" role="status"></div>
            <span class="ms-2">Đang tải...</span>
        </div>
    `;
    
    fetch(`/api/image/applied-filters?imagePath=${encodeURIComponent(currentImagePath)}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Lỗi khi tải danh sách bộ lọc đã áp dụng');
            }
            return response.json();
        })
        .then(filters => {
            // Xóa nội dung hiện tại
            appliedFiltersList.innerHTML = '';
            
            if (!filters || filters.length === 0) {
                appliedFiltersList.innerHTML = `
                    <div class="list-group-item text-center">
                        <span class="text-muted">Chưa có bộ lọc nào được áp dụng</span>
                    </div>
                `;
                return;
            }
            
            // Hiển thị danh sách các bộ lọc đã áp dụng
            filters.forEach((filter, index) => {
                const filterItem = document.createElement('div');
                filterItem.className = 'list-group-item';
                
                // Định dạng tham số
                const paramString = Object.entries(filter.parameters || {})
                    .map(([key, value]) => `${key}: ${value}`)
                    .join(', ');
                
                filterItem.innerHTML = `
                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <span class="badge bg-secondary me-2">${index + 1}</span>
                            <strong>${filter.filterName}</strong>
                        </div>
                    </div>
                    ${paramString ? `<small class="text-muted">${paramString}</small>` : ''}
                `;
                
                appliedFiltersList.appendChild(filterItem);
            });
        })
        .catch(error => {
            console.error('Lỗi:', error);
            appliedFiltersList.innerHTML = `
                <div class="list-group-item text-center text-danger">
                    <i class="bi bi-exclamation-triangle me-2"></i>
                    Không thể tải danh sách bộ lọc: ${error.message}
                </div>
            `;
        });
}

// Hàm điều chỉnh chiều cao của phần xem trước
function adjustPreviewHeight() {
    const imagePreview = document.getElementById('imagePreview');
    if (!imagePreview) return;
    
    const windowHeight = window.innerHeight;
    const previewCard = document.querySelector('.sticky-top');
    if (previewCard) {
        const cardTop = parseInt(previewCard.style.top || '0');
        const cardHeader = previewCard.querySelector('.card-header');
        const headerHeight = cardHeader ? cardHeader.offsetHeight : 0;
        
        // Đảm bảo phần xem trước không vượt quá kích thước màn hình
        const maxHeight = windowHeight - cardTop - headerHeight - 40; // Thêm padding
        
        const img = imagePreview.querySelector('img');
        if (img) {
            img.style.maxHeight = `${maxHeight}px`;
        }
    }
}

// Hàm xem trước bộ lọc
function previewFilter(form) {
    const imagePath = form.querySelector('input[name="imagePath"]').value;
    if (!imagePath) return;

    const formData = new FormData(form);
    
    // Hiển thị trạng thái đang tải
    const imagePreview = document.getElementById('imagePreview');
    imagePreview.classList.add('loading');
    
    const currentImage = imagePreview.querySelector('img');
    if (currentImage) {
        currentImage.style.opacity = '0.5';
    }
    
    // Gọi API xem trước
    fetch('/api/image/preview', {
        method: 'POST',
        body: formData
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Lỗi khi xem trước bộ lọc');
        }
        return response.json();
    })
    .then(data => {
        if (data.imageData) {
            // Cập nhật ảnh xem trước
            if (currentImage) {
                currentImage.src = data.imageData;
                currentImage.style.opacity = '1';
            } else {
                const newImage = document.createElement('img');
                newImage.src = data.imageData;
                newImage.style.maxWidth = '100%';
                newImage.className = 'img-fluid';
                
                // Điều chỉnh chiều cao tối đa
                adjustPreviewHeight();
                
                // Xóa placeholder nếu có
                while (imagePreview.firstChild) {
                    imagePreview.removeChild(imagePreview.firstChild);
                }
                
                imagePreview.appendChild(newImage);
            }
            
            // Xóa trạng thái đang tải
            imagePreview.classList.remove('loading');
        }
    })
    .catch(error => {
        console.error('Lỗi:', error);
        // Khôi phục độ mờ của ảnh
        if (currentImage) {
            currentImage.style.opacity = '1';
        }
        // Xóa trạng thái đang tải
        imagePreview.classList.remove('loading');
    });
}
