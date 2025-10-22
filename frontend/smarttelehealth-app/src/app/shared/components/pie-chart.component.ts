import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartOptions } from 'chart.js';

/**
 * Pie Chart Component
 * Reusable pie chart for showing parts of a whole
 */
@Component({
  selector: 'app-pie-chart',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  template: `
    <div class="chart-container" [style.height.px]="height">
      <canvas baseChart
              [data]="pieChartData"
              [options]="pieChartOptions"
              [type]="'pie'">
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
export class PieChartComponent {
  @Input() data: ChartConfiguration<'pie'>['data'] | null = null;
  @Input() height: number = 300;
  @Input() options?: ChartOptions<'pie'>;

  get pieChartData(): ChartConfiguration<'pie'>['data'] {
    return this.data || {
      labels: [],
      datasets: []
    };
  }

  get pieChartOptions(): ChartConfiguration<'pie'>['options'] {
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

