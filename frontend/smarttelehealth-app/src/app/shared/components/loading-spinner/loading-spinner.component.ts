import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Reusable Loading Spinner Component
 * 
 * Usage:
 * <app-loading-spinner [size]="'large'" [message]="'Loading data...'"></app-loading-spinner>
 */
@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loading-spinner.component.html',
  styleUrls: ['./loading-spinner.component.scss']
})
export class LoadingSpinnerComponent {
  @Input() size: 'small' | 'medium' | 'large' = 'medium';
  @Input() message = 'Loading...';
  @Input() color: 'primary' | 'secondary' | 'success' | 'danger' | 'warning' | 'info' = 'primary';

  getSpinnerClass(): string {
    const sizeMap = {
      'small': 'spinner-border-sm',
      'medium': '',
      'large': 'spinner-large'
    };
    return `spinner-border text-${this.color} ${sizeMap[this.size]}`;
  }
}


