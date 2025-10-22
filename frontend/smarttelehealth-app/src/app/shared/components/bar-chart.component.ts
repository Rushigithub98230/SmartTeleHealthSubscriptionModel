import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartOptions } from 'chart.js';

/**
 * Bar Chart Component
 * Reusable bar chart for comparisons and categorical data
 */
@Component({
  selector: 'app-bar-chart',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  template: `
    <div class="chart-container" [style.height.px]="height">
      <canvas baseChart
              [data]="barChartData"
              [options]="barChartOptions"
              [type]="'bar'">
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
export class BarChartComponent {
  @Input() data: ChartConfiguration<'bar'>['data'] | null = null;
  @Input() height: number = 300;
  @Input() options?: ChartOptions<'bar'>;
  @Input() horizontal: boolean = false;

  get barChartData(): ChartConfiguration<'bar'>['data'] {
    return this.data || {
      labels: [],
      datasets: []
    };
  }

  get barChartOptions(): ChartConfiguration<'bar'>['options'] {
    return this.options || {
      responsive: true,
      maintainAspectRatio: false,
      indexAxis: this.horizontal ? 'y' : 'x',
      plugins: {
        legend: {
          display: true,
          position: 'top'
        },
        tooltip: {
          enabled: true
        }
      },
      scales: {
        y: {
          beginAtZero: true
        }
      }
    };
  }
}

