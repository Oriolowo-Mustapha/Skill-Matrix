document.addEventListener('DOMContentLoaded', function() {
    const cards = document.querySelectorAll('.stat-card');
    cards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
    });
    const badge = document.querySelector('.proficiency-badge');
    const scoreElement = document.querySelector('.score-text');
    
    if (scoreElement && badge) {
        const scoreText = scoreElement.textContent.replace('%', '');
        const score = parseInt(scoreText);
        console.log("Am working");
        
        if (score >= 90) {
            badge.style.background = 'linear-gradient(135deg, #10b981, #34d399)';
        } else if (score >= 75) {
            badge.style.background = 'linear-gradient(135deg, #3b82f6, #60a5fa)';
        } else if (score >= 60) {
            badge.style.background = 'linear-gradient(135deg, #f59e0b, #fbbf24)';
        } else {
            badge.style.background = 'linear-gradient(135deg, #ef4444, #f87171)';
        }
    }
});