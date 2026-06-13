import type { Chart, ChartConfiguration } from "chart.js";

type ChartApi = typeof import("chart.js");

class DashboardPage {
    private chart: Chart<"line", number[], string> | null = null;
    private carouselInterval: number | null = null;
    private chartModule: Promise<typeof import("chart.js").Chart> | null = null;

    public async initChart(canvasId: string, labels: string[], data: number[]): Promise<void> {
        const canvas = document.getElementById(canvasId);
        if (!(canvas instanceof HTMLCanvasElement)) {
            return;
        }

        this.chart?.destroy();

        const chartConfig: ChartConfiguration<"line", number[], string> = {
            type: "line",
            data: {
                labels,
                datasets: [
                    {
                        label: "访问量",
                        data,
                        borderWidth: 2,
                        borderColor: "#3b82f6",
                        backgroundColor: "rgba(59, 130, 246, 0.1)",
                        tension: 0.4,
                        fill: true,
                        pointBackgroundColor: "#3b82f6",
                        pointRadius: 4,
                        pointHoverRadius: 6
                    }
                ]
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
                            color: "rgba(255, 255, 255, 0.05)"
                        },
                        ticks: {
                            color: "#94a3b8"
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: "#94a3b8"
                        }
                    }
                }
            }
        };

        const Chart = await this.getChart();
        this.chart = new Chart(canvas, chartConfig);
    }

    public initCarousel(containerId: string): void {
        const container = document.getElementById(containerId);
        if (!container) {
            return;
        }

        const slides = container.querySelectorAll<HTMLElement>(".carousel-slide");
        if (slides.length === 0) {
            return;
        }

        let currentIndex = 0;
        slides.forEach((slide) => slide.classList.remove("active"));
        slides[0].classList.add("active");

        this.clearCarouselInterval();
        this.carouselInterval = window.setInterval(() => {
            slides[currentIndex].classList.remove("active");
            currentIndex = (currentIndex + 1) % slides.length;
            slides[currentIndex].classList.add("active");
        }, 5000);
    }

    public dispose(): void {
        this.chart?.destroy();
        this.chart = null;
        this.clearCarouselInterval();
    }

    private clearCarouselInterval(): void {
        if (this.carouselInterval === null) {
            return;
        }

        window.clearInterval(this.carouselInterval);
        this.carouselInterval = null;
    }

    private getChart(): Promise<typeof import("chart.js").Chart> {
        this.chartModule ??= import("chart.js").then((module: ChartApi) => {
            module.Chart.register(
                module.CategoryScale,
                module.Filler,
                module.Legend,
                module.LineController,
                module.LineElement,
                module.LinearScale,
                module.PointElement,
                module.Title,
                module.Tooltip
            );

            return module.Chart;
        });

        return this.chartModule;
    }
}

const dashboardPage = new DashboardPage();

export async function initChart(canvasId: string, labels: string[], data: number[]): Promise<void> {
    await dashboardPage.initChart(canvasId, labels, data);
}

export function initCarousel(containerId: string): void {
    dashboardPage.initCarousel(containerId);
}

export function dispose(): void {
    dashboardPage.dispose();
}
