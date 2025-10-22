import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartOptions } from 'chart.js';

/**
 * Doughnut Chart Component
 * Reusable doughnut chart for percentage distributions
 */
@Component({
  selector: 'app-doughnut-chart',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  template: `
    <div class="chart-container" [style.height.px]="height">
      <canvas baseChart
              [data]="doughnutChartData"
              [options]="doughnutChartOptions"
              [type]="'doughnut'">
      </canvas>
    </div>
  `,
  styles: [`
    .chart-container {
      position: relative;
      width: 100%;
      display: flex;
      justify-content: center;
      align-items: center;
    }
  `]
})
export class DoughnutChartComponent {
  @Input() data: ChartConfiguration<'doughnut'>['data'] | null = null;
  @Input() height: number = 300;
  @Input() options?: ChartOptions<'doughnut'>;

  get doughnutChartData(): ChartConfiguration<'doughnut'>['data'] {
    return this.data || {
      labels: [],
      datasets: []
    };
  }

  get doughnutChartOptions(): ChartConfiguration<'doughnut'>['options'] {
    return this.options || {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: true,
          position: 'bottom'
        },
        tooltip: {
          enabled: true,
          callbacks: {
            label: function(context) {
              const label = context.label || '';
              const value = context.parsed || 0;
              const total = (context.dataset.data as number[]).reduce((a, b) => a + b, 0);
              const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : '0';
              return `${label}: ${value} (${percentage}%)`;
            }
          }
        }
      }
    };
  }
}

