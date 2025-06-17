/**
 * Bộ lọc điều chỉnh độ sáng
 */
class BrightnessFilter {
    constructor() {
        this.name = 'Brightness';
        this.description = 'Điều chỉnh độ sáng của ảnh';
    }

    /**
     * Tạo giao diện người dùng cho bộ lọc độ sáng
     * @param {HTMLElement} container - Container để thêm giao diện vào
     */
    createUI(container) {
        const html = `
            <div class="card mb-3">
                <div class="card-header">
                    <h5>${this.name}</h5>
                    <p class="text-muted">${this.description}</p>
                </div>
                <div class="card-body">
                    <form class="filter-form" data-filter="${this.name}">
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
     * Xác thực tham số đầu vào
     * @param {Object} parameters - Các tham số cần xác thực
     * @returns {boolean} - Kết quả xác thực
     */
    validateParameters(parameters) {
        if (!parameters.hasOwnProperty('value')) {
            return false;
        }
        
        const value = parseFloat(parameters.value);
        return !isNaN(value) && value >= -1 && value <= 1;
    }
    
    /**
     * Áp dụng bộ lọc (Hàm này được gọi từ backend)
     * @param {Image} image - Đối tượng ảnh cần xử lý
     * @param {Object} parameters - Các tham số cho bộ lọc
     * @returns {Image} - Ảnh sau khi đã xử lý
     */
    apply(image, parameters) {
        // Hàm này không được sử dụng ở client
        // Việc xử lý ảnh được thực hiện ở backend
        console.warn("Phương thức apply() không được sử dụng ở client side");
        return image;
    }
}

// Export class nếu môi trường hỗ trợ module
if (typeof module !== 'undefined' && typeof module.exports !== 'undefined') {
    module.exports = BrightnessFilter;
} else {
    // Đăng ký vào global namespace nếu là môi trường browser
    window.BrightnessFilter = BrightnessFilter;
} 