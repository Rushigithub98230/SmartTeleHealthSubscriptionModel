import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatRadioModule } from '@angular/material/radio';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { SubscriptionService } from '../../services/subscription.service';

export interface BulkOperationsDialogData {
  selectedSubscriptions: string[];
  subscriptionCount: number;
}

export interface BulkOperationResult {
  action: string;
  success: boolean;
  message: string;
  results?: any[];
  summary?: {
    total: number;
    success: number;
    failed: number;
  };
}

@Component({
  selector: 'app-bulk-operations-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatRadioModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    ReactiveFormsModule
  ],
  template: `
    <div class="bulk-operations-dialog">
      <div mat-dialog-title class="dialog-header">
        <div class="header-info">
          <mat-icon>group_work</mat-icon>
          <div>
            <h2>Bulk Operations</h2>
            <p>{{ data.subscriptionCount }} subscription(s) selected</p>
          </div>
        </div>
        <button mat-icon-button mat-dialog-close>
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <div mat-dialog-content class="dialog-content">
        <form [formGroup]="bulkForm" (ngSubmit)="onSubmit()">
          <!-- Operation Type Selection -->
          <div class="form-section">
            <h3>Select Operation</h3>
            <mat-radio-group formControlName="operation" (change)="onOperationChange()">
              <mat-radio-button value="status">Update Status</mat-radio-button>
              <mat-radio-button value="cancel">Cancel Subscriptions</mat-radio-button>
              <mat-radio-button value="notify">Send Notifications</mat-radio-button>
            </mat-radio-group>
          </div>

          <!-- Status Update Section -->
          <div *ngIf="bulkForm.get('operation')?.value === 'status'" class="form-section">
            <h3>Status Update</h3>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>New Status</mat-label>
              <mat-select formControlName="newStatus">
                <mat-option value="Active">Active</mat-option>
                <mat-option value="Paused">Paused</mat-option>
                <mat-option value="Suspended">Suspended</mat-option>
                <mat-option value="Cancelled">Cancelled</mat-option>
                <mat-option value="Expired">Expired</mat-option>
              </mat-select>
            </mat-form-field>
          </div>

          <!-- Cancellation Section -->
          <div *ngIf="bulkForm.get('operation')?.value === 'cancel'" class="form-section">
            <h3>Cancellation Details</h3>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Reason for Cancellation</mat-label>
              <textarea matInput formControlName="reason" rows="3" placeholder="Enter reason for cancellation..."></textarea>
            </mat-form-field>
          </div>

          <!-- Notification Section -->
          <div *ngIf="bulkForm.get('operation')?.value === 'notify'" class="form-section">
            <h3>Notification Details</h3>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Notification Type</mat-label>
              <mat-select formControlName="notificationType">
                <mat-option value="info">Information</mat-option>
                <mat-option value="warning">Warning</mat-option>
                <mat-option value="error">Error</mat-option>
                <mat-option value="success">Success</mat-option>
              </mat-select>
            </mat-form-field>
            
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Message</mat-label>
              <textarea matInput formControlName="message" rows="4" placeholder="Enter notification message..."></textarea>
            </mat-form-field>
          </div>

          <!-- Confirmation Section -->
          <div class="form-section confirmation-section">
            <mat-checkbox formControlName="confirmAction">
              I confirm that I want to perform this bulk operation on {{ data.subscriptionCount }} subscription(s)
            </mat-checkbox>
          </div>
        </form>
      </div>

      <div mat-dialog-actions class="dialog-actions">
        <button mat-button mat-dialog-close [disabled]="isProcessing">Cancel</button>
        <button mat-raised-button 
                color="primary" 
                (click)="onSubmit()" 
                [disabled]="!bulkForm.valid || isProcessing">
          <mat-spinner *ngIf="isProcessing" diameter="20"></mat-spinner>
          <mat-icon *ngIf="!isProcessing">{{ getActionIcon() }}</mat-icon>
          {{ getActionButtonText() }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .bulk-operations-dialog {
      width: 90vw;
      max-width: 600px;
      display: flex;
      flex-direction: column;
    }

    .dialog-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 24px;
      border-bottom: 1px solid #e0e0e0;
    }

    .header-info {
      display: flex;
      align-items: center;
      gap: 16px;
    }

    .header-info mat-icon {
      font-size: 32px;
      width: 32px;
      height: 32px;
      color: #1976d2;
    }

    .header-info h2 {
      margin: 0;
      font-size: 24px;
      font-weight: 600;
    }

    .header-info p {
      margin: 0;
      color: #666;
    }

    .dialog-content {
      padding: 24px;
      max-height: 60vh;
      overflow-y: auto;
    }

    .form-section {
      margin-bottom: 24px;
    }

    .form-section h3 {
      margin: 0 0 16px 0;
      font-size: 18px;
      font-weight: 500;
      color: #333;
    }

    .full-width {
      width: 100%;
    }

    .confirmation-section {
      background-color: #f5f5f5;
      padding: 16px;
      border-radius: 8px;
      border: 1px solid #e0e0e0;
    }

    .dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid #e0e0e0;
      display: flex;
      justify-content: flex-end;
      gap: 12px;
    }

    .dialog-actions button {
      min-width: 120px;
    }

    mat-radio-group {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    mat-radio-button {
      margin-bottom: 8px;
    }

    @media (max-width: 768px) {
      .bulk-operations-dialog {
        width: 95vw;
        height: 90vh;
      }

      .dialog-content {
        padding: 16px;
      }

      .header-info {
        flex-direction: column;
        align-items: flex-start;
        gap: 8px;
      }
    }
  `]
})
export class BulkOperationsDialogComponent implements OnInit {
  bulkForm: FormGroup;
  isProcessing = false;

  constructor(
    private fb: FormBuilder,
    private subscriptionService: SubscriptionService,
    private snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<BulkOperationsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: BulkOperationsDialogData
  ) {
    this.bulkForm = this.fb.group({
      operation: ['', Validators.required],
      newStatus: [''],
      reason: [''],
      notificationType: ['info'],
      message: [''],
      confirmAction: [false, Validators.requiredTrue]
    });
  }

  ngOnInit() {
    // Set up form validation based on operation type
    this.bulkForm.get('operation')?.valueChanges.subscribe(operation => {
      this.updateFormValidation(operation);
    });
  }

  onOperationChange() {
    const operation = this.bulkForm.get('operation')?.value;
    this.updateFormValidation(operation);
  }

  updateFormValidation(operation: string) {
    // Clear previous validators
    this.bulkForm.get('newStatus')?.clearValidators();
    this.bulkForm.get('reason')?.clearValidators();
    this.bulkForm.get('message')?.clearValidators();

    // Add validators based on operation type
    switch (operation) {
      case 'status':
        this.bulkForm.get('newStatus')?.setValidators([Validators.required]);
        break;
      case 'cancel':
        this.bulkForm.get('reason')?.setValidators([Validators.required]);
        break;
      case 'notify':
        this.bulkForm.get('message')?.setValidators([Validators.required]);
        break;
    }

    // Update validation
    this.bulkForm.get('newStatus')?.updateValueAndValidity();
    this.bulkForm.get('reason')?.updateValueAndValidity();
    this.bulkForm.get('message')?.updateValueAndValidity();
  }

  getActionIcon(): string {
    const operation = this.bulkForm.get('operation')?.value;
    switch (operation) {
      case 'status': return 'update';
      case 'cancel': return 'cancel';
      case 'notify': return 'notifications';
      default: return 'play_arrow';
    }
  }

  getActionButtonText(): string {
    if (this.isProcessing) return 'Processing...';
    
    const operation = this.bulkForm.get('operation')?.value;
    switch (operation) {
      case 'status': return 'Update Status';
      case 'cancel': return 'Cancel Subscriptions';
      case 'notify': return 'Send Notifications';
      default: return 'Execute';
    }
  }

  onSubmit() {
    if (!this.bulkForm.valid) {
      this.snackBar.open('Please fill in all required fields', 'Close', { duration: 3000 });
      return;
    }

    this.isProcessing = true;
    const formValue = this.bulkForm.value;

    let operation$;
    switch (formValue.operation) {
      case 'status':
        operation$ = this.subscriptionService.bulkUpdateStatus(
          this.data.selectedSubscriptions,
          formValue.newStatus
        );
        break;
      case 'cancel':
        operation$ = this.subscriptionService.bulkCancelSubscriptions(
          this.data.selectedSubscriptions,
          formValue.reason
        );
        break;
      case 'notify':
        operation$ = this.subscriptionService.bulkSendNotifications(
          this.data.selectedSubscriptions,
          formValue.message,
          formValue.notificationType
        );
        break;
      default:
        this.snackBar.open('Invalid operation selected', 'Close', { duration: 3000 });
        this.isProcessing = false;
        return;
    }

    operation$.subscribe({
      next: (response) => {
        this.isProcessing = false;
        if (response.statusCode === 200) {
          const result: BulkOperationResult = {
            action: formValue.operation,
            success: true,
            message: response.message,
            results: response.data?.results,
            summary: response.data?.summary
          };
          this.dialogRef.close(result);
        } else {
          this.snackBar.open(response.message || 'Operation failed', 'Close', { duration: 5000 });
        }
      },
      error: (error) => {
        this.isProcessing = false;
        console.error('Bulk operation error:', error);
        this.snackBar.open('Operation failed: ' + (error.message || 'Unknown error'), 'Close', { duration: 5000 });
      }
    });
  }
}
