// Toggle Sidebar for Mobile
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    sidebar.classList.toggle('active');
}

// Close sidebar when clicking outside on mobile
document.addEventListener('click', function (e) {
    const sidebar = document.getElementById('sidebar');
    const toggle = document.querySelector('.mobile-toggle');

    if (window.innerWidth <= 768 &&
        !sidebar.contains(e.target) &&
        !toggle.contains(e.target) &&
        sidebar.classList.contains('active')) {
        sidebar.classList.remove('active');
    }
});

document.addEventListener("DOMContentLoaded", function () {
    const ctx = document.getElementById('trendsChart').getContext('2d');

    if (!assessmentsData || assessmentsData.length === 0) {
        console.log("No assessment data available for chart.");
        return;
    }

    // Extract labels and scores
    const labels = assessmentsData.map(a => a.Date);
    const scores = assessmentsData.map(a => a.Score);

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: "Scores",
                data: scores,
                borderColor: "#2575fc",
                backgroundColor: "rgba(37, 117, 252, 0.2)",
                borderWidth: 2,
                fill: true,
                tension: 0.4,
                pointBackgroundColor: "#ff6600",
                pointRadius: 5
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    display: true,
                    labels: {
                        color: "#333"
                    }
                },
                tooltip: {
                    mode: 'index',
                    intersect: false
                }
            },
            scales: {
                x: {
                    ticks: { color: "#333" }
                },
                y: {
                    beginAtZero: true,
                    ticks: { color: "#333" }
                }
            }
        }
    });
});