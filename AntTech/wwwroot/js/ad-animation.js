document.addEventListener('DOMContentLoaded', function() {
    const adContainer = document.querySelector('.scrolling-ad-container');
    if (!adContainer) return;
    
    // Thêm nút đóng quảng cáo
    const closeButton = document.createElement('span');
    closeButton.innerHTML = '&times;';
    closeButton.style.position = 'absolute';
    closeButton.style.right = '10px';
    closeButton.style.top = '50%';
    closeButton.style.transform = 'translateY(-50%)';
    closeButton.style.cursor = 'pointer';
    closeButton.style.fontSize = '16px';
    closeButton.style.color = '#6c757d';
    
    closeButton.addEventListener('click', function() {
        adContainer.style.display = 'none';
        // Lưu trạng thái vào sessionStorage để không hiển thị lại trong phiên
        sessionStorage.setItem('adClosed', 'true');
    });
    
    adContainer.style.position = 'relative';
    adContainer.appendChild(closeButton);
    
    // Kiểm tra xem quảng cáo đã bị đóng chưa
    if (sessionStorage.getItem('adClosed') === 'true') {
        adContainer.style.display = 'none';
    }
});