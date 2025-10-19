import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PrivilegeUsageDto } from '../../../core/models';

/**
 * Reusable Privilege Usage Bar Component
 * Display privilege usage as color-coded progress bar
 * 
 * Usage:
 * <app-privilege-usage-bar [privilege]="privilegeUsage"></app-privilege-usage-bar>
 */
@Component({
  selector: 'app-privilege-usage-bar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './privilege-usage-bar.component.html',
  styleUrls: ['./privilege-usage-bar.component.scss']
})
export class PrivilegeUsageBarComponent {
  @Input() privilege!: PrivilegeUsageDto;
  @Input() showDetails = true;

  getUsagePercentage(): number {
    if (this.privilege.isUnlimited || this.privilege.allowedValue === 0) return 0;
    return Math.min(Math.round((this.privilege.usedValue / this.privilege.allowedValue) * 100), 100);
  }

  getProgressBarClass(): string {
    const percentage = this.getUsagePercentage();
    if (percentage < 50) return 'bg-success';
    if (percentage < 80) return 'bg-warning';
    return 'bg-danger';
  }
}


