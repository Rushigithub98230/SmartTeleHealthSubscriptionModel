import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface ConfirmationDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: 'warning' | 'danger' | 'info';
  requireReason?: boolean;
  reasonLabel?: string;
  reasonPlaceholder?: string;
}

@Component({
  selector: 'app-confirmation-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule
  ],
  template: `
    <div class="confirmation-dialog">
      <div mat-dialog-title class="dialog-header">
        <div class="header-content">
          <mat-icon [class]="getIconClass()">{{ getIcon() }}</mat-icon>
          <h2>{{ data.title }}</h2>
        </div>
      </div>

      <div mat-dialog-content class="dialog-content">
        <p class="message">{{ data.message }}</p>
        
        <form [formGroup]="reasonForm" *ngIf="data.requireReason">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>{{ data.reasonLabel || 'Reason' }}</mat-label>
            <textarea 
              matInput 
              formControlName="reason" 
              rows="3"
              [placeholder]="data.reasonPlaceholder || 'Please provide a reason for this action'">
            </textarea>
            <mat-error *ngIf="reasonForm.get('reason')?.hasError('required')">
              Reason is required
            </mat-error>
            <mat-error *ngIf="reasonForm.get('reason')?.hasError('minlength')">
              Please provide a more detailed reason (at least 10 characters)
            </mat-error>
          </mat-form-field>
        </form>
      </div>

      <div mat-dialog-actions class="dialog-actions">
        <button mat-button (click)="onCancel()" class="cancel-button">
          {{ data.cancelText || 'Cancel' }}
        </button>
        <button 
          mat-raised-button 
          [color]="getButtonColor()" 
          (click)="onConfirm()"
          [disabled]="data.requireReason && reasonForm.invalid"
          class="confirm-button">
          {{ data.confirmText || 'Confirm' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .confirmation-dialog {
      min-width: 400px;
      max-width: 600px;
    }

    .dialog-header {
      padding: 24px 24px 16px 24px;
      margin: 0;
    }

    .header-content {
      display: flex;
      align-items: center;
      gap: 16px;
    }

    .header-content h2 {
      margin: 0;
      font-size: 20px;
      font-weight: 600;
    }

    .header-content mat-icon {
      font-size: 32px;
      width: 32px;
      height: 32px;
    }

    .header-content mat-icon.warning {
      color: #f57c00;
    }

    .header-content mat-icon.danger {
      color: #f44336;
    }

    .header-content mat-icon.info {
      color: #1976d2;
    }

    .dialog-content {
      padding: 0 24px 16px 24px;
    }

    .message {
      margin: 0 0 16px 0;
      font-size: 16px;
      line-height: 1.5;
      color: #333;
    }

    .full-width {
      width: 100%;
    }

    .dialog-actions {
      padding: 16px 24px 24px 24px;
      display: flex;
      justify-content: flex-end;
      gap: 12px;
      margin: 0;
    }

    .cancel-button {
      color: #666;
    }

    .confirm-button.mat-warn {
      background-color: #f44336;
      color: white;
    }

    .confirm-button.mat-warn:hover {
      background-color: #d32f2f;
    }

    .confirm-button.mat-accent {
      background-color: #f57c00;
      color: white;
    }

    .confirm-button.mat-accent:hover {
      background-color: #ef6c00;
    }

    @media (max-width: 480px) {
      .confirmation-dialog {
        min-width: 280px;
      }

      .dialog-actions {
        flex-direction: column-reverse;
      }

      .dialog-actions button {
        width: 100%;
      }
    }
  `]
})
export class ConfirmationDialogComponent {
  reasonForm: FormGroup;

  constructor(
    public dialogRef: MatDialogRef<ConfirmationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmationDialogData,
    private fb: FormBuilder
  ) {
    this.reasonForm = this.fb.group({
      reason: ['', data.requireReason ? [Validators.required, Validators.minLength(10)] : []]
    });
  }

  getIcon(): string {
    switch (this.data.type) {
      case 'danger':
        return 'warning';
      case 'warning':
        return 'warning_amber';
      case 'info':
        return 'info';
      default:
        return 'help_outline';
    }
  }

  getIconClass(): string {
    return this.data.type || 'info';
  }

  getButtonColor(): 'primary' | 'accent' | 'warn' {
    switch (this.data.type) {
      case 'danger':
        return 'warn';
      case 'warning':
        return 'accent';
      default:
        return 'primary';
    }
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    if (this.data.requireReason) {
      const reason = this.reasonForm.get('reason')?.value;
      this.dialogRef.close({ confirmed: true, reason });
    } else {
      this.dialogRef.close(true);
    }
  }
}

