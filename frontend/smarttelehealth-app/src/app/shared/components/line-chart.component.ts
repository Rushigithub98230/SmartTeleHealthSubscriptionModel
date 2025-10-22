import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartOptions } from 'chart.js';

/**
 * Line Chart Component
 * Reusable line chart for trends and time-series data
 */
@Component({
  selector: 'app-line-chart',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  template: `
    <div class="chart-container" [style.height.px]="height">
      <canvas baseChart
              [data]="lineChartData"
              [options]="lineChartOptions"
              [type]="'line'">
      </canvas>
    </div>
  `,
  styles: [`
    .chart-container {
      position: relative;
      width: 100%;
    }
  `]
})
export class LineChartComponent {
  @Input() data: ChartConfiguration<'line'>['data'] | null = null;
  @Input() height: number = 300;
  @Input() options?: ChartOptions<'line'>;

  get lineChartData(): ChartConfiguration<'line'>['data'] {
    return this.data || {
      labels: [],
      datasets: []
    };
  }

  get lineChartOptions(): ChartConfiguration<'line'>['options'] {
    return this.options || {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: true,
          position: 'top'
        },
        tooltip: {
          enabled: true,
          mode: 'index',
          intersect: false
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            callback: function(value) {
              return '$' + value;
            }
          }
        }
      }
    };
  }
}

