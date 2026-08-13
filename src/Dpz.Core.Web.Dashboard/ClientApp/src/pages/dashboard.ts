import type { Chart, ChartConfiguration } from "chart.js";

type ChartApi = typeof import("chart.js");
type DashboardChart =
    | Chart<"line", number[], string>
    | Chart<"bar", number[], string>
    | Chart<"doughnut", number[], string>;

class DashboardPage {
    private readonly charts = new Map<string, DashboardChart>();
    private readonly carouselIntervals = new Map<string, number>();
    private chartModule: Promise<typeof import("chart.js").Chart> | null = null;

    public async initLineChart(canvasId: string, labels: string[], data: number[]): Promise<void> {
        if (!this.hasChartData(canvasId, labels, data)) {
            return;
        }

        const chartConfig: ChartConfiguration<"line", number[], string> = {
            type: "line",
            data: {
                labels,
                datasets: [
                    {
                        label: "访问量",
                        data,
                        borderWidth: 2,
                        borderColor: "#60a5fa",
                        backgroundColor: "rgba(96, 165, 250, 0.16)",
                        tension: 0.35,
                        fill: true,
                        pointBackgroundColor: "#0f172a",
                        pointBorderColor: "#60a5fa",
                        pointBorderWidth: 2,
                        pointRadius: 3,
                        pointHoverRadius: 5
                    }
                ]
            },
            options: this.getLineChartOptions()
        };

        await this.renderChart(canvasId, chartConfig);
    }

    public async initBarChart(canvasId: string, labels: string[], data: number[]): Promise<void> {
        if (!this.hasChartData(canvasId, labels, data)) {
            return;
        }

        const chartConfig: ChartConfiguration<"bar", number[], string> = {
            type: "bar",
            data: {
                labels,
                datasets: [
                    {
                        label: "访问量",
                        data,
                        borderWidth: 0,
                        borderRadius: 8,
                        borderSkipped: false,
                        backgroundColor: "rgba(52, 211, 153, 0.72)",
                        hoverBackgroundColor: "rgba(52, 211, 153, 0.95)"
                    }
                ]
            },
            options: this.getBarChartOptions()
        };

        await this.renderChart(canvasId, chartConfig);
    }

    public async initDoughnutChart(
        canvasId: string,
        labels: string[],
        data: number[]
    ): Promise<void> {
        if (!this.hasChartData(canvasId, labels, data)) {
            return;
        }

        const chartConfig: ChartConfiguration<"doughnut", number[], string> = {
            type: "doughnut",
            data: {
                labels,
                datasets: [
                    {
                        label: "访问量",
                        data,
                        borderColor: "#0f172a",
                        borderWidth: 3,
                        backgroundColor: [
                            "#60a5fa",
                            "#34d399",
                            "#fbbf24",
                            "#f472b6",
                            "#a78bfa",
                            "#22d3ee"
                        ],
                        hoverOffset: 6
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: "68%",
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        backgroundColor: "#020617",
                        borderColor: "rgba(148, 163, 184, 0.28)",
                        borderWidth: 1,
                        titleColor: "#f8fafc",
                        bodyColor: "#cbd5e1",
                        padding: 10
                    }
                }
            }
        };

        await this.renderChart(canvasId, chartConfig);
    }

    public async initChart(canvasId: string, labels: string[], data: number[]): Promise<void> {
        await this.initLineChart(canvasId, labels, data);
    }

    public initCarousel(containerId: string): void {
        const container = document.getElementById(containerId);
        if (!container) {
            this.clearCarouselInterval(containerId);
            return;
        }

        const slides = container.querySelectorAll<HTMLElement>(".dashboard-carousel__slide");
        if (slides.length === 0) {
            this.clearCarouselInterval(containerId);
            return;
        }

        let currentIndex = 0;
        slides.forEach((slide) => slide.classList.remove("is-active"));
        slides[0].classList.add("is-active");

        this.clearCarouselInterval(containerId);
        if (slides.length === 1) {
            return;
        }

        const interval = window.setInterval(() => {
            slides[currentIndex].classList.remove("is-active");
            currentIndex = (currentIndex + 1) % slides.length;
            slides[currentIndex].classList.add("is-active");
        }, 5000);

        this.carouselIntervals.set(containerId, interval);
    }

    public dispose(): void {
        this.charts.forEach((chart) => chart.destroy());
        this.charts.clear();
        this.carouselIntervals.forEach((interval) => window.clearInterval(interval));
        this.carouselIntervals.clear();
    }

    private hasChartData(canvasId: string, labels: string[], data: number[]): boolean {
        if (labels.length > 0 && data.length > 0) {
            return true;
        }

        this.destroyChart(canvasId);
        return false;
    }

    private async renderChart<T extends "line" | "bar" | "doughnut">(
        canvasId: string,
        chartConfig: ChartConfiguration<T, number[], string>
    ): Promise<void> {
        const canvas = document.getElementById(canvasId);
        if (!(canvas instanceof HTMLCanvasElement)) {
            this.destroyChart(canvasId);
            return;
        }

        this.destroyChart(canvasId);

        const Chart = await this.getChart();
        this.charts.set(canvasId, new Chart(canvas, chartConfig) as DashboardChart);
    }

    private destroyChart(canvasId: string): void {
        const chart = this.charts.get(canvasId);
        if (!chart) {
            return;
        }

        chart.destroy();
        this.charts.delete(canvasId);
    }

    private clearCarouselInterval(containerId: string): void {
        const interval = this.carouselIntervals.get(containerId);
        if (interval === undefined) {
            return;
        }

        window.clearInterval(interval);
        this.carouselIntervals.delete(containerId);
    }

    private getLineChartOptions(): ChartConfiguration<"line", number[], string>["options"] {
        return this.getAxisChartOptions() as ChartConfiguration<
            "line",
            number[],
            string
        >["options"];
    }

    private getBarChartOptions(): ChartConfiguration<"bar", number[], string>["options"] {
        return this.getAxisChartOptions() as ChartConfiguration<"bar", number[], string>["options"];
    }

    private getAxisChartOptions(): unknown {
        return {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                intersect: false,
                mode: "index"
            },
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: "#020617",
                    borderColor: "rgba(148, 163, 184, 0.28)",
                    borderWidth: 1,
                    titleColor: "#f8fafc",
                    bodyColor: "#cbd5e1",
                    padding: 10
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    border: {
                        display: false
                    },
                    grid: {
                        color: "rgba(148, 163, 184, 0.12)"
                    },
                    ticks: {
                        color: "#94a3b8",
                        precision: 0
                    }
                },
                x: {
                    border: {
                        display: false
                    },
                    grid: {
                        display: false
                    },
                    ticks: {
                        color: "#94a3b8",
                        maxRotation: 0,
                        autoSkipPadding: 16
                    }
                }
            }
        };
    }

    private getChart(): Promise<typeof import("chart.js").Chart> {
        this.chartModule ??= import("chart.js").then((module: ChartApi) => {
            module.Chart.register(
                module.ArcElement,
                module.BarController,
                module.BarElement,
                module.CategoryScale,
                module.DoughnutController,
                module.Filler,
                module.Legend,
                module.LineController,
                module.LineElement,
                module.LinearScale,
                module.PointElement,
                module.Tooltip
            );

            return module.Chart;
        });

        return this.chartModule;
    }
}

const dashboardPage = new DashboardPage();

export async function initLineChart(
    canvasId: string,
    labels: string[],
    data: number[]
): Promise<void> {
    await dashboardPage.initLineChart(canvasId, labels, data);
}

export async function initBarChart(
    canvasId: string,
    labels: string[],
    data: number[]
): Promise<void> {
    await dashboardPage.initBarChart(canvasId, labels, data);
}

export async function initDoughnutChart(
    canvasId: string,
    labels: string[],
    data: number[]
): Promise<void> {
    await dashboardPage.initDoughnutChart(canvasId, labels, data);
}

export async function initChart(canvasId: string, labels: string[], data: number[]): Promise<void> {
    await dashboardPage.initChart(canvasId, labels, data);
}

export function initCarousel(containerId: string): void {
    dashboardPage.initCarousel(containerId);
}

export function dispose(): void {
    dashboardPage.dispose();
}
