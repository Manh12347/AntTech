// JavaScript để điều khiển popup video
document.addEventListener('DOMContentLoaded', function() {
    const popup = document.getElementById('videoPopup');
    const closeBtn = document.querySelector('.close-popup');
    const iframe = document.getElementById('youtubeIframe');
    const popupTitle = document.getElementById('popupVideoTitle');
    const playButtons = document.querySelectorAll('.video-play-button');
    
    // Xử lý khi click vào nút play trên thumbnail
    playButtons.forEach(button => {
        button.addEventListener('click', function() {
            const videoId = this.getAttribute('data-video-id');
            const videoTitle = this.closest('.video-item').querySelector('.video-title').textContent;
            
            // Cập nhật tiêu đề video trong popup
            popupTitle.textContent = videoTitle;
            
            // Cập nhật src của iframe với autoplay=0 (tắt tự động phát)
            iframe.src = `https://www.youtube.com/embed/${videoId}?rel=0&autoplay=0`;
            
            // Hiển thị popup
            popup.classList.add('show');
        });
    });
    
    // Đóng popup khi click vào nút đóng
    if (closeBtn) {
        closeBtn.addEventListener('click', function() {
            popup.classList.remove('show');
            // Dừng video khi đóng popup
            iframe.src = '';
        });
    }
    
    // Đóng popup khi click bên ngoài
    document.addEventListener('click', function(event) {
        if (!popup.contains(event.target) && 
            !event.target.classList.contains('video-play-button') && 
            !event.target.closest('.video-play-button')) {
            popup.classList.remove('show');
            // Dừng video khi đóng popup
            iframe.src = '';
        }
    });
});