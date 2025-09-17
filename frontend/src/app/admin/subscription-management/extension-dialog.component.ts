import { Component, Inject, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatRadioModule } from '@angular/material/radio';

export interface ExtensionDialogData {
  subscriptionId: string;
  currentEndDate: Date;
  userName: string;
}

export interface ExtensionResult {
  days?: number;
  newEndDate?: Date;
  method: 'days' | 'date';
}

@Component({
  selector: 'app-extension-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatRadioModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>schedule</mat-icon>
      Extend Subscription
    </h2>
    
    <div mat-dialog-content>
      <div class="subscription-info">
        <p><strong>User:</strong> {{ data.userName }}</p>
        <p><strong>Current End Date:</strong> {{ data.currentEndDate | date:'medium' }}</p>
      </div>

      <form [formGroup]="extensionForm" class="extension-form">
        <div class="form-section">
          <h4>Choose Extension Method:</h4>
          
          <div class="extension-options">
            <div class="option-group">
              <mat-radio-button value="days" (change)="onMethodChange('days')">
                Extend by number of days
              </mat-radio-button>
              
              <mat-form-field appearance="outline" class="days-input" *ngIf="extensionMethod === 'days'">
                <mat-label>Additional Days</mat-label>
                <input matInput 
                       type="number" 
                       formControlName="additionalDays"
                       min="1"
                       max="365"
                       placeholder="Enter number of days">
                <mat-icon matSuffix>event</mat-icon>
                <mat-hint>Enter days to extend (1-365)</mat-hint>
                <mat-error *ngIf="extensionForm.get('additionalDays')?.hasError('required')">
                  Number of days is required
                </mat-error>
                <mat-error *ngIf="extensionForm.get('additionalDays')?.hasError('min')">
                  Must be at least 1 day
                </mat-error>
                <mat-error *ngIf="extensionForm.get('additionalDays')?.hasError('max')">
                  Cannot exceed 365 days
                </mat-error>
              </mat-form-field>
            </div>

            <div class="option-group">
              <mat-radio-button value="date" (change)="onMethodChange('date')">
                Set specific end date
              </mat-radio-button>
              
              <mat-form-field appearance="outline" class="date-input" *ngIf="extensionMethod === 'date'">
                <mat-label>New End Date</mat-label>
                <input matInput 
                       [matDatepicker]="picker" 
                       formControlName="newEndDate"
                       [min]="minDate"
                       placeholder="Choose new end date">
                <mat-datepicker-toggle matIconSuffix [for]="picker"></mat-datepicker-toggle>
                <mat-datepicker #picker></mat-datepicker>
                <mat-hint>Select a date after current end date</mat-hint>
                <mat-error *ngIf="extensionForm.get('newEndDate')?.hasError('required')">
                  New end date is required
                </mat-error>
                <mat-error *ngIf="extensionForm.get('newEndDate')?.hasError('invalidDate')">
                  Date must be after current end date
                </mat-error>
              </mat-form-field>
            </div>
          </div>
        </div>

        <div class="preview-section" *ngIf="getPreviewText()">
          <mat-icon>info</mat-icon>
          <div class="preview-content">
            <h4>Extension Preview:</h4>
            <p>{{ getPreviewText() }}</p>
          </div>
        </div>
      </form>
    </div>

    <div mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button 
        mat-raised-button 
        color="primary" 
        [disabled]="!extensionForm.valid"
        (click)="onConfirm()">
        Extend Subscription
      </button>
    </div>
  `,
  styles: [`
    .subscription-info {
      background-color: #f5f5f5;
      padding: 16px;
      border-radius: 8px;
      margin-bottom: 24px;
    }

    .subscription-info p {
      margin: 0 0 8px 0;
    }

    .extension-form {
      min-width: 500px;
    }

    .form-section h4 {
      margin: 0 0 16px 0;
      color: #333;
    }

    .extension-options {
      display: flex;
      flex-direction: column;
      gap: 24px;
    }

    .option-group {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .days-input,
    .date-input {
      margin-left: 24px;
      max-width: 300px;
    }

    .preview-section {
      display: flex;
      gap: 12px;
      align-items: flex-start;
      margin-top: 24px;
      padding: 16px;
      background-color: #e3f2fd;
      border-radius: 8px;
      border-left: 4px solid #2196f3;
    }

    .preview-content h4 {
      margin: 0 0 8px 0;
      color: #1976d2;
    }

    .preview-content p {
      margin: 0;
      color: #1565c0;
    }

    mat-dialog-content {
      min-height: 300px;
    }

    mat-radio-button {
      margin-bottom: 8px;
    }
  `]
})
export class ExtensionDialogComponent {
  private fb = inject(FormBuilder);
  
  extensionForm: FormGroup;
  extensionMethod: 'days' | 'date' = 'days';
  minDate = new Date();

  constructor(
    public dialogRef: MatDialogRef<ExtensionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ExtensionDialogData
  ) {
    // Set minimum date to tomorrow (current end date + 1 day)
    this.minDate = new Date(this.data.currentEndDate);
    this.minDate.setDate(this.minDate.getDate() + 1);

    this.extensionForm = this.fb.group({
      additionalDays: [30, [Validators.required, Validators.min(1), Validators.max(365)]],
      newEndDate: [null]
    });

    // Add custom validator for new end date
    this.extensionForm.get('newEndDate')?.setValidators([
      Validators.required,
      this.dateValidator.bind(this)
    ]);
  }

  onMethodChange(method: 'days' | 'date') {
    this.extensionMethod = method;
    
    if (method === 'days') {
      this.extensionForm.get('additionalDays')?.setValidators([
        Validators.required, 
        Validators.min(1), 
        Validators.max(365)
      ]);
      this.extensionForm.get('newEndDate')?.clearValidators();
      this.extensionForm.get('newEndDate')?.setValue(null);
    } else {
      this.extensionForm.get('newEndDate')?.setValidators([
        Validators.required,
        this.dateValidator.bind(this)
      ]);
      this.extensionForm.get('additionalDays')?.clearValidators();
      this.extensionForm.get('additionalDays')?.setValue(null);
    }
    
    this.extensionForm.get('additionalDays')?.updateValueAndValidity();
    this.extensionForm.get('newEndDate')?.updateValueAndValidity();
  }

  dateValidator(control: any) {
    if (!control.value) return null;
    
    const selectedDate = new Date(control.value);
    const currentEndDate = new Date(this.data.currentEndDate);
    
    if (selectedDate <= currentEndDate) {
      return { invalidDate: true };
    }
    
    return null;
  }

  getPreviewText(): string {
    if (this.extensionMethod === 'days') {
      const days = this.extensionForm.get('additionalDays')?.value;
      if (days) {
        const newDate = new Date(this.data.currentEndDate);
        newDate.setDate(newDate.getDate() + days);
        return `Subscription will be extended by ${days} day(s). New end date: ${newDate.toLocaleDateString()}`;
      }
    } else {
      const newEndDate = this.extensionForm.get('newEndDate')?.value;
      if (newEndDate) {
        const currentDate = new Date(this.data.currentEndDate);
        const selectedDate = new Date(newEndDate);
        const diffTime = selectedDate.getTime() - currentDate.getTime();
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
        return `Subscription will be extended by ${diffDays} day(s). New end date: ${selectedDate.toLocaleDateString()}`;
      }
    }
    return '';
  }

  onCancel() {
    this.dialogRef.close();
  }

  onConfirm() {
    if (this.extensionForm.valid) {
      const result: ExtensionResult = {
        method: this.extensionMethod
      };

      if (this.extensionMethod === 'days') {
        result.days = this.extensionForm.get('additionalDays')?.value;
      } else {
        result.newEndDate = this.extensionForm.get('newEndDate')?.value;
        // Calculate days for API compatibility
        const currentDate = new Date(this.data.currentEndDate);
        const selectedDate = new Date(result.newEndDate!);
        const diffTime = selectedDate.getTime() - currentDate.getTime();
        result.days = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
      }

      this.dialogRef.close(result);
    }
  }
}