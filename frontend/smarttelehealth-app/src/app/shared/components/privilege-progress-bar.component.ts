import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Privilege Progress Bar Component
 * Displays visual progress bar for privilege usage with color coding
 * 
 * Color Coding:
 * - Green (< 70%): Healthy usage
 * - Yellow (70-90%): Warning - approaching limit
 * - Red (> 90%): Danger - near or at limit
 */
@Component({
  selector: 'app-privilege-progress-bar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="privilege-item mb-3">
      <div class="d-flex justify-content-between align-items-center mb-1">
        <strong>{{privilegeName}}</strong>
        <span [ngClass]="getUsageClass()">
          {{used}} / {{limit === -1 ? '∞' : limit}}
          <span *ngIf="limit > 0 && used > limit" class="badge bg-danger ms-1">OVERAGE</span>
        </span>
      </div>
      
      <div class="progress" style="height: 25px; border-radius: 0.5rem;">
        <div class="progress-bar" 
             [ngClass]="getProgressBarClass()"
             [style.width.%]="getPercentage()"
             role="progressbar"
             [attr.aria-valuenow]="used"
             [attr.aria-valuemin]="0"
             [attr.aria-valuemax]="limit">
          <span class="progress-text">{{getPercentage()}}%</span>
        </div>
      </div>
      
      <div class="d-flex justify-content-between mt-1">
        <small class="text-muted">
          <i class="bi bi-arrow-counterclockwise me-1"></i>
          Remaining: {{getRemaining()}}
        </small>
        <small class="text-muted" *ngIf="resetDate">
          <i class="bi bi-calendar-event me-1"></i>
          Resets: {{resetDate | date:'MMM d, yyyy'}}
        </small>
      </div>
    </div>
  `,
  styles: [`
    .privilege-item {
      padding: 0.75rem;
      background-color: #f8f9fa;
      border-radius: 0.5rem;
      border: 1px solid #dee2e6;
    }

    .progress {
      box-shadow: inset 0 1px 2px rgba(0,0,0,0.1);
    }

    .progress-bar {
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 600;
      transition: width 0.6s ease;
    }

    .progress-text {
      color: white;
      text-shadow: 0 1px 2px rgba(0,0,0,0.3);
    }

    .privilege-item:hover {
      background-color: #e9ecef;
      box-shadow: 0 0.125rem 0.25rem rgba(0,0,0,0.075);
    }
  `]
})
export class PrivilegeProgressBarComponent {
  @Input() privilegeName: string = '';
  @Input() used: number = 0;
  @Input() limit: number = 0;
  @Input() resetDate: Date | null = null;

  /**
   * Calculate usage percentage
   */
  getPercentage(): number {
    if (this.limit === -1) return 0; // Unlimited
    if (this.limit === 0) return 0; // Disabled
    if (this.limit === 0) return 100; // Prevent division by zero
    
    const percentage = (this.used / this.limit) * 100;
    return Math.min(Math.round(percentage), 100);
  }

  /**
   * Get progress bar color class based on usage percentage
   */
  getProgressBarClass(): string {
    const pct = this.getPercentage();
    
    if (this.limit === -1) return 'bg-info'; // Unlimited
    if (this.limit === 0) return 'bg-secondary'; // Disabled
    if (pct < 70) return 'bg-success';
    if (pct < 90) return 'bg-warning';
    return 'bg-danger';
  }

  /**
   * Get usage text color class
   */
  getUsageClass(): string {
    const pct = this.getPercentage();
    
    if (this.limit === -1) return 'text-info fw-bold'; // Unlimited
    if (this.limit === 0) return 'text-secondary'; // Disabled
    if (pct >= 100 || this.used > this.limit) return 'text-danger fw-bold';
    if (pct >= 90) return 'text-warning fw-bold';
    return 'text-success fw-bold';
  }

  /**
   * Get remaining amount text
   */
  getRemaining(): string {
    if (this.limit === -1) return 'Unlimited';
    if (this.limit === 0) return 'Disabled';
    
    const remaining = Math.max(0, this.limit - this.used);
    if (remaining === 0 && this.used > this.limit) {
      const overage = this.used - this.limit;
      return `0 (${overage} overage)`;
    }
    
    return remaining.toString();
  }
}

