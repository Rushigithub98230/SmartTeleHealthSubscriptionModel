import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Reusable Confirmation Dialog Component
 * Bootstrap modal for confirming actions
 * 
 * Usage:
 * <app-confirmation-dialog
 *   [show]="showDialog"
 *   [title]="'Confirm Action'"
 *   [message]="'Are you sure?'"
 *   [confirmText]="'Yes, Delete'"
 *   [confirmClass]="'btn-danger'"
 *   (confirmed)="onConfirm()"
 *   (cancelled)="onCancel()">
 * </app-confirmation-dialog>
 */
@Component({
  selector: 'app-confirmation-dialog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './confirmation-dialog.component.html',
  styleUrls: ['./confirmation-dialog.component.scss']
})
export class ConfirmationDialogComponent {
  @Input() show = false;
  @Input() title = 'Confirm Action';
  @Input() message = 'Are you sure you want to proceed?';
  @Input() confirmText = 'Confirm';
  @Input() cancelText = 'Cancel';
  @Input() confirmClass = 'btn-primary';
  @Input() icon = 'bi-question-circle';
  @Input() iconClass = 'text-warning';

  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  onConfirm(): void {
    this.confirmed.emit();
    this.show = false;
  }

  onCancel(): void {
    this.cancelled.emit();
    this.show = false;
  }
}


