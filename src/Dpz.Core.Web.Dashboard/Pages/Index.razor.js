// Dashboard Chart & Carousel interactions using ES Modules
import Chart from 'https://esm.sh/chart.js/auto';

let dashboardChart = null;
let carouselInterval = null;

export function initChart(canvasId, labels, data) {
    const ctx = document.getElementById(canvasId);
    if(!ctx) return;

    if (dashboardChart) {
        dashboardChart.destroy();
    }


    const chartConfig = {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: '访问量',
                data: data,
                borderWidth: 2,
                borderColor: '#3b82f6',
                backgroundColor: 'rgba(59, 130, 246, 0.1)',
                tension: 0.4,
                fill: true,
                pointBackgroundColor: '#3b82f6',
                pointRadius: 4,
                pointHoverRadius: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(255, 255, 255, 0.05)'
                    },
                    ticks: {
                        color: '#94a3b8'
                    }
                },
                x: {
                    grid: {
                        display: false
                    },
                    ticks: {
                        color: '#94a3b8'
                    }
                }
            }
        }
    };

    dashboardChart = new Chart(ctx, chartConfig);
}

export function initCarousel(containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;

    const slides = container.querySelectorAll('.carousel-slide');
    if (slides.length === 0) return;

    let currentIndex = 0;
    
    // Reset all slides
    slides.forEach(s => s.classList.remove('active'));
    // Show first slide
    slides[0].classList.add('active');

    if (carouselInterval) clearInterval(carouselInterval);

    carouselInterval = setInterval(() => {
        slides[currentIndex].classList.remove('active');
        currentIndex = (currentIndex + 1) % slides.length;
        slides[currentIndex].classList.add('active');
    }, 5000);
}

export function dispose() {
    if (dashboardChart) {
        dashboardChart.destroy();
        dashboardChart = null;
    }
    if (carouselInterval) {
        clearInterval(carouselInterval);
        carouselInterval = null;
    }
}
