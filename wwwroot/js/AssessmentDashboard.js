document.addEventListener('DOMContentLoaded', function () {
    const progressBars = document.querySelectorAll('.progress-bar');
    progressBars.forEach(bar => {
        const width = bar.style.width;
        bar.style.width = '0%';
        setTimeout(() => {
            bar.style.width = width;
        }, 100);
    });
});

function retakeAssessment(skillId) {
    window.location.href = `/Quiz/RetakeQuiz?skillId=${skillId}`;
}

function takeNewAssessment() {
    window.location.href = `/Skill/Index`;
}