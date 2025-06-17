/**
 * MiniPhotoshop - Ứng dụng xử lý ảnh đơn giản
 */

class MiniPhotoshop {
    constructor() {
        this.currentImagePath = null;
        this.originalImagePath = null;
        this.imageElement = document.getElementById('preview-image');
        this.loadingIndicator = document.getElementById('loading-indicator');
        this.errorContainer = document.getElementById('error-container');
        this.errorMessage = document.getElementById('error-message');
        this.successContainer = document.getElementById('success-container');
        this.successMessage = document.getElementById('success-message');
        
        // Khởi tạo các component
        this.initUploader();
        this.initFilterButtons();
        this.initImageHistory();
    }
    
    /**
     * Khởi tạo phần tải lên ảnh
     */
    initUploader() {
        const uploadForm = document.getElementById('upload-form');
        const fileInput = document.getElementById('file-input');
        const uploadButton = document.getElementById('upload-button');
        
        // Xử lý sự kiện khi người dùng chọn file
        fileInput.addEventListener('change', () => {
            if (fileInput.files.length > 0) {
                uploadButton.disabled = false;
                uploadButton.classList.remove('btn-secondary');
                uploadButton.classList.add('btn-primary');
            } else {
                uploadButton.disabled = true;
                uploadButton.classList.remove('btn-primary');
                uploadButton.classList.add('btn-secondary');
            }
        });
        
        // Xử lý sự kiện khi người dùng tải lên file
        uploadForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            if (fileInput.files.length === 0) {
                this.showError('Vui lòng chọn một file ảnh');
                return;
            }
            
            const formData = new FormData();
            formData.append('file', fileInput.files[0]);
            
            try {
                this.showLoading('Đang tải lên ảnh...');
                
                const response = await fetch('/api/image/upload', {
                    method: 'POST',
                    body: formData
                });
                
                if (!response.ok) {
                    const error = await response.json();
                    throw new Error(error.error || 'Lỗi khi tải lên ảnh');
                }
                
                const data = await response.json();
                this.setImage(data.filePath);
                this.originalImagePath = data.filePath;
                this.addToHistory(data.filePath, 'Ảnh gốc');
                
                this.showSuccess('Tải lên ảnh thành công');
                
                // Kích hoạt các nút bộ lọc
                this.enableFilterButtons();
                
                // Đặt lại form
                uploadForm.reset();
                uploadButton.disabled = true;
                uploadButton.classList.remove('btn-primary');
                uploadButton.classList.add('btn-secondary');
            } catch (error) {
                this.showError(`Lỗi: ${error.message}`);
            } finally {
                this.hideLoading();
            }
        });
    }
    
    /**
     * Khởi tạo các nút bộ lọc
     */
    initFilterButtons() {
        // Khởi tạo các nút bộ lọc với dữ liệu từ API
        this.loadFilters();
        
        // Xử lý sự kiện cho các form bộ lọc
        document.querySelectorAll('.filter-form').forEach(form => {
            form.addEventListener('submit', async (e) => {
                e.preventDefault();
                
                const filterName = form.dataset.filter;
                const formData = new FormData(form);
                const parameters = {};
                
                // Chuyển đổi FormData thành object parameters
                for (const [key, value] of formData.entries()) {
                    parameters[key] = value;
                }
                
                await this.applyFilter(filterName, parameters);
            });
        });
        
        // Vô hiệu hóa các nút bộ lọc ban đầu
        this.disableFilterButtons();
    }
    
    /**
     * Tải danh sách bộ lọc từ API
     */
    async loadFilters() {
        try {
            const response = await fetch('/api/image/filters');
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.error || 'Lỗi khi tải danh sách bộ lọc');
            }
            
            const filters = await response.json();
            
            // Cập nhật UI với danh sách bộ lọc
            const filterContainer = document.getElementById('filter-container');
            
            filters.forEach(filter => {
                // Tạo UI cho từng bộ lọc theo loại
                this.createFilterUI(filter, filterContainer);
            });
        } catch (error) {
            this.showError(`Lỗi: ${error.message}`);
        }
    }
    
    /**
     * Tạo UI cho bộ lọc
     * @param {Object} filter - Thông tin bộ lọc
     * @param {HTMLElement} container - Container chứa UI bộ lọc
     */
    createFilterUI(filter, container) {
        // Tạo UI dựa trên loại bộ lọc
        switch (filter.name) {
            case 'Brightness':
                this.createBrightnessFilterUI(filter, container);
                break;
            case 'Resize':
                this.createResizeFilterUI(filter, container);
                break;
            case 'Rotate':
                this.createRotateFilterUI(filter, container);
                break;
            case 'Crop':
                this.createCropFilterUI(filter, container);
                break;
            // Thêm các bộ lọc khác ở đây
            default:
                console.warn(`Chưa có UI cho bộ lọc: ${filter.name}`);
                break;
        }
    }
    
    /**
     * Tạo UI cho bộ lọc độ sáng
     */
    createBrightnessFilterUI(filter, container) {
        const html = `
            <div class="card mb-3">
                <div class="card-header">
                    <h5>${filter.name}</h5>
                    <p class="text-muted">${filter.description}</p>
                </div>
                <div class="card-body">
                    <form class="filter-form" data-filter="${filter.name}">
                        <div class="form-group mb-3">
                            <label for="brightness-value">Độ sáng (-1 đến 1):</label>
                            <input type="range" class="form-range" id="brightness-value" name="value" 
                                min="-1" max="1" step="0.1" value="0">
                            <div class="d-flex justify-content-between">
                                <span>Tối</span>
                                <span>Bình thường</span>
                                <span>Sáng</span>
                            </div>
                        </div>
                        <button type="submit" class="btn btn-primary filter-button">Áp dụng</button>
                    </form>
                </div>
            </div>
        `;
        
        container.insertAdjacentHTML('beforeend', html);
    }
    
    /**
     * Tạo UI cho bộ lọc thay đổi kích thước
     */
    createResizeFilterUI(filter, container) {
        const html = `
            <div class="card mb-3">
                <div class="card-header">
                    <h5>${filter.name}</h5>
                    <p class="text-muted">${filter.description}</p>
                </div>
                <div class="card-body">
                    <form class="filter-form" data-filter="${filter.name}">
                        <div class="row">
                            <div class="col-md-6 mb-3">
                                <label for="resize-width">Chiều rộng (px):</label>
                                <input type="number" class="form-control" id="resize-width" name="width" min="1" required>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label for="resize-height">Chiều cao (px):</label>
                                <input type="number" class="form-control" id="resize-height" name="height" min="1" required>
                            </div>
                        </div>
                        <button type="submit" class="btn btn-primary filter-button">Áp dụng</button>
                    </form>
                </div>
            </div>
        `;
        
        container.insertAdjacentHTML('beforeend', html);
        
        // Cập nhật giá trị kích thước khi ảnh được tải
        this.imageElement.addEventListener('load', () => {
            const widthInput = document.getElementById('resize-width');
            const heightInput = document.getElementById('resize-height');
            
            if (widthInput && heightInput) {
                widthInput.value = this.imageElement.naturalWidth;
                heightInput.value = this.imageElement.naturalHeight;
            }
        });
    }
    
    /**
     * Tạo UI cho bộ lọc xoay ảnh
     */
    createRotateFilterUI(filter, container) {
        const html = `
            <div class="card mb-3">
                <div class="card-header">
                    <h5>${filter.name}</h5>
                    <p class="text-muted">${filter.description}</p>
                </div>
                <div class="card-body">
                    <form class="filter-form" data-filter="${filter.name}">
                        <div class="form-group mb-3">
                            <label for="rotate-angle">Góc xoay (độ):</label>
                            <div class="input-group">
                                <input type="number" class="form-control" id="rotate-angle" name="angle" value="90" required>
                                <span class="input-group-text">°</span>
                            </div>
                        </div>
                        <div class="btn-group w-100">
                            <button type="button" class="btn btn-outline-secondary" onclick="document.getElementById('rotate-angle').value = '-90'">Xoay trái 90°</button>
                            <button type="button" class="btn btn-outline-secondary" onclick="document.getElementById('rotate-angle').value = '90'">Xoay phải 90°</button>
                            <button type="button" class="btn btn-outline-secondary" onclick="document.getElementById('rotate-angle').value = '180'">Xoay 180°</button>
                        </div>
                        <button type="submit" class="btn btn-primary mt-3 w-100 filter-button">Áp dụng</button>
                    </form>
                </div>
            </div>
        `;
        
        container.insertAdjacentHTML('beforeend', html);
    }
    
    /**
     * Tạo UI cho bộ lọc cắt ảnh
     */
    createCropFilterUI(filter, container) {
        const html = `
            <div class="card mb-3">
                <div class="card-header">
                    <h5>${filter.name}</h5>
                    <p class="text-muted">${filter.description}</p>
                </div>
                <div class="card-body">
                    <form class="filter-form" data-filter="${filter.name}">
                        <div class="row">
                            <div class="col-md-6 mb-3">
                                <label for="crop-x">Vị trí X:</label>
                                <input type="number" class="form-control" id="crop-x" name="x" min="0" value="0" required>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label for="crop-y">Vị trí Y:</label>
                                <input type="number" class="form-control" id="crop-y" name="y" min="0" value="0" required>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label for="crop-width">Chiều rộng:</label>
                                <input type="number" class="form-control" id="crop-width" name="width" min="1" required>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label for="crop-height">Chiều cao:</label>
                                <input type="number" class="form-control" id="crop-height" name="height" min="1" required>
                            </div>
                        </div>
                        <p class="text-muted small">Gợi ý: Bạn có thể sử dụng công cụ chọn vùng trong trình duyệt để xác định tọa độ và kích thước.</p>
                        <button type="submit" class="btn btn-primary filter-button">Áp dụng</button>
                    </form>
                </div>
            </div>
        `;
        
        container.insertAdjacentHTML('beforeend', html);
        
        // Cập nhật giá trị kích thước khi ảnh được tải
        this.imageElement.addEventListener('load', () => {
            const widthInput = document.getElementById('crop-width');
            const heightInput = document.getElementById('crop-height');
            
            if (widthInput && heightInput) {
                widthInput.value = this.imageElement.naturalWidth;
                heightInput.value = this.imageElement.naturalHeight;
            }
        });
    }
    
    /**
     * Áp dụng bộ lọc cho ảnh
     * @param {string} filterName - Tên bộ lọc
     * @param {Object} parameters - Tham số cho bộ lọc
     */
    async applyFilter(filterName, parameters) {
        if (!this.currentImagePath) {
            this.showError('Vui lòng tải lên ảnh trước khi áp dụng bộ lọc');
            return;
        }
        
        try {
            this.showLoading(`Đang áp dụng bộ lọc ${filterName}...`);
            
            const response = await fetch('/api/image/apply-filter', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    imagePath: this.currentImagePath,
                    filterName: filterName,
                    parameters: parameters
                })
            });
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.error || `Lỗi khi áp dụng bộ lọc ${filterName}`);
            }
            
            const data = await response.json();
            this.setImage(data.filePath);
            this.addToHistory(data.filePath, `Áp dụng ${filterName}`);
            
            this.showSuccess(`Áp dụng bộ lọc ${filterName} thành công`);
        } catch (error) {
            this.showError(`Lỗi: ${error.message}`);
        } finally {
            this.hideLoading();
        }
    }
    
    /**
     * Khởi tạo lịch sử ảnh
     */
    initImageHistory() {
        const historyContainer = document.getElementById('history-container');
        const resetButton = document.getElementById('reset-button');
        
        // Xử lý sự kiện khi người dùng nhấn nút reset
        resetButton.addEventListener('click', () => {
            if (this.originalImagePath) {
                this.setImage(this.originalImagePath);
                this.showSuccess('Đã khôi phục ảnh gốc');
            }
        });
    }
    
    /**
     * Thêm một mục vào lịch sử
     * @param {string} imagePath - Đường dẫn ảnh
     * @param {string} action - Hành động đã thực hiện
     */
    addToHistory(imagePath, action) {
        const historyList = document.getElementById('history-list');
        const timestamp = new Date().toLocaleTimeString();
        
        const historyItem = document.createElement('li');
        historyItem.className = 'list-group-item d-flex justify-content-between align-items-center';
        historyItem.innerHTML = `
            <div>
                <span class="badge bg-secondary">${timestamp}</span>
                ${action}
            </div>
            <button class="btn btn-sm btn-outline-primary history-button">Khôi phục</button>
        `;
        
        // Xử lý sự kiện khi người dùng nhấn nút khôi phục
        const restoreButton = historyItem.querySelector('.history-button');
        restoreButton.addEventListener('click', () => {
            this.setImage(imagePath);
            this.showSuccess('Đã khôi phục ảnh từ lịch sử');
        });
        
        // Thêm vào đầu danh sách
        historyList.insertBefore(historyItem, historyList.firstChild);
        
        // Giới hạn số lượng mục lịch sử (giữ 10 mục gần nhất)
        const historyItems = historyList.querySelectorAll('li');
        if (historyItems.length > 10) {
            historyList.removeChild(historyItems[historyItems.length - 1]);
        }
    }
    
    /**
     * Hiển thị thông báo lỗi
     * @param {string} message - Thông báo lỗi
     */
    showError(message) {
        this.errorContainer.classList.remove('d-none');
        this.errorMessage.textContent = message;
        this.successContainer.classList.add('d-none');
        
        // Tự động ẩn sau 5 giây
        setTimeout(() => {
            this.errorContainer.classList.add('d-none');
        }, 5000);
    }
    
    /**
     * Hiển thị thông báo thành công
     * @param {string} message - Thông báo thành công
     */
    showSuccess(message) {
        this.successContainer.classList.remove('d-none');
        this.successMessage.textContent = message;
        this.errorContainer.classList.add('d-none');
        
        // Tự động ẩn sau 3 giây
        setTimeout(() => {
            this.successContainer.classList.add('d-none');
        }, 3000);
    }
    
    /**
     * Hiển thị loading
     * @param {string} message - Thông báo loading
     */
    showLoading(message = 'Đang xử lý...') {
        this.loadingIndicator.classList.remove('d-none');
        document.getElementById('loading-message').textContent = message;
    }
    
    /**
     * Ẩn loading
     */
    hideLoading() {
        this.loadingIndicator.classList.add('d-none');
    }
    
    /**
     * Đặt ảnh hiện tại
     * @param {string} imagePath - Đường dẫn ảnh
     */
    setImage(imagePath) {
        this.currentImagePath = imagePath;
        this.imageElement.src = imagePath;
        document.getElementById('image-container').classList.remove('d-none');
    }
    
    /**
     * Kích hoạt các nút bộ lọc
     */
    enableFilterButtons() {
        document.querySelectorAll('.filter-button').forEach(button => {
            button.disabled = false;
        });
    }
    
    /**
     * Vô hiệu hóa các nút bộ lọc
     */
    disableFilterButtons() {
        document.querySelectorAll('.filter-button').forEach(button => {
            button.disabled = true;
        });
    }
}

// Khởi tạo ứng dụng khi trang được tải
document.addEventListener('DOMContentLoaded', () => {
    window.app = new MiniPhotoshop();
}); 