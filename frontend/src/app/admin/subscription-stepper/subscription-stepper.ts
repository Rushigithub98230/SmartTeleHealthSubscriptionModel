import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatStepperModule } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { CreateSubscriptionPlanDto, SubscriptionPlanDto } from '../../models/subscription.models';

@Component({
  selector: 'app-subscription-stepper',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatStepperModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatCheckboxModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatIconModule
  ],
  template: `
    <mat-stepper [linear]="true" #stepper>
      <!-- Step 1: Basic Information -->
      <mat-step [stepControl]="basicInfoForm" label="Basic Information">
        <form [formGroup]="basicInfoForm">
          <div class="step-content">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Plan Name</mat-label>
              <input matInput formControlName="name" placeholder="Enter plan name">
              <mat-error *ngIf="basicInfoForm.get('name')?.hasError('required')">
                Plan name is required
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Description</mat-label>
              <textarea matInput formControlName="description" rows="3" placeholder="Enter plan description"></textarea>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Price</mat-label>
              <input matInput type="number" formControlName="price" placeholder="0.00">
              <mat-error *ngIf="basicInfoForm.get('price')?.hasError('required')">
                Price is required
              </mat-error>
              <mat-error *ngIf="basicInfoForm.get('price')?.hasError('min')">
                Price must be greater than 0
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Billing Cycle</mat-label>
              <mat-select formControlName="billingCycleId">
                <mat-option value="monthly">Monthly</mat-option>
                <mat-option value="quarterly">Quarterly</mat-option>
                <mat-option value="annual">Annual</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Currency</mat-label>
              <mat-select formControlName="currencyId">
                <mat-option value="usd">USD</mat-option>
                <mat-option value="eur">EUR</mat-option>
                <mat-option value="inr">INR</mat-option>
              </mat-select>
            </mat-form-field>
          </div>
          <div class="button-row">
            <button mat-button matStepperNext [disabled]="!basicInfoForm.valid">Next</button>
          </div>
        </form>
      </mat-step>

      <!-- Step 2: Features & Limits -->
      <mat-step [stepControl]="featuresForm" label="Features & Limits">
        <form [formGroup]="featuresForm">
          <div class="step-content">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Messaging Count</mat-label>
              <input matInput type="number" formControlName="messagingCount" placeholder="10">
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Delivery Frequency (Days)</mat-label>
              <input matInput type="number" formControlName="deliveryFrequencyDays" placeholder="30">
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Max Pause Duration (Days)</mat-label>
              <input matInput type="number" formControlName="maxPauseDurationDays" placeholder="90">
            </mat-form-field>

            <div class="checkbox-group">
              <mat-checkbox formControlName="includesMedicationDelivery">
                Includes Medication Delivery
              </mat-checkbox>
              <mat-checkbox formControlName="includesFollowUpCare">
                Includes Follow-up Care
              </mat-checkbox>
            </div>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Features (comma-separated)</mat-label>
              <textarea matInput formControlName="features" rows="3" placeholder="Feature 1, Feature 2, Feature 3"></textarea>
            </mat-form-field>
          </div>
          <div class="button-row">
            <button mat-button matStepperPrevious>Back</button>
            <button mat-button matStepperNext [disabled]="!featuresForm.valid">Next</button>
          </div>
        </form>
      </mat-step>

      <!-- Step 3: Display & Marketing -->
      <mat-step [stepControl]="displayForm" label="Display & Marketing">
        <form [formGroup]="displayForm">
          <div class="step-content">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Display Order</mat-label>
              <input matInput type="number" formControlName="displayOrder" placeholder="1">
            </mat-form-field>

            <div class="checkbox-group">
              <mat-checkbox formControlName="isActive">
                Active Plan
              </mat-checkbox>
              <mat-checkbox formControlName="isMostPopular">
                Mark as Most Popular
              </mat-checkbox>
              <mat-checkbox formControlName="isTrending">
                Mark as Trending
              </mat-checkbox>
            </div>
          </div>
          <div class="button-row">
            <button mat-button matStepperPrevious>Back</button>
            <button mat-button matStepperNext [disabled]="!displayForm.valid">Next</button>
          </div>
        </form>
      </mat-step>

      <!-- Step 4: Review & Submit -->
      <mat-step label="Review & Submit">
        <div class="step-content">
          <h3>Review Plan Details</h3>
          <div class="review-section">
            <h4>Basic Information</h4>
            <p><strong>Name:</strong> {{ getFormValue('name') }}</p>
            <p><strong>Description:</strong> {{ getFormValue('description') }}</p>
            <p><strong>Price:</strong> ${{ getFormValue('price') }}</p>
            <p><strong>Billing Cycle:</strong> {{ getFormValue('billingCycleId') }}</p>
            <p><strong>Currency:</strong> {{ getFormValue('currencyId') }}</p>
          </div>
          
          <div class="review-section">
            <h4>Features & Limits</h4>
            <p><strong>Messaging Count:</strong> {{ getFormValue('messagingCount') }}</p>
            <p><strong>Includes Medication Delivery:</strong> {{ getFormValue('includesMedicationDelivery') ? 'Yes' : 'No' }}</p>
            <p><strong>Includes Follow-up Care:</strong> {{ getFormValue('includesFollowUpCare') ? 'Yes' : 'No' }}</p>
            <p><strong>Delivery Frequency:</strong> {{ getFormValue('deliveryFrequencyDays') }} days</p>
            <p><strong>Max Pause Duration:</strong> {{ getFormValue('maxPauseDurationDays') }} days</p>
          </div>

          <div class="review-section">
            <h4>Display & Marketing</h4>
            <p><strong>Display Order:</strong> {{ getFormValue('displayOrder') }}</p>
            <p><strong>Most Popular:</strong> {{ getFormValue('isMostPopular') ? 'Yes' : 'No' }}</p>
            <p><strong>Trending:</strong> {{ getFormValue('isTrending') ? 'Yes' : 'No' }}</p>
            <p><strong>Active:</strong> {{ getFormValue('isActive') ? 'Yes' : 'No' }}</p>
          </div>
        </div>
        <div class="button-row">
          <button mat-button matStepperPrevious>Back</button>
          <button mat-raised-button color="primary" (click)="onSubmit()" [disabled]="!isFormValid()">
            {{ editMode ? 'Update Plan' : 'Create Plan' }}
          </button>
        </div>
      </mat-step>
    </mat-stepper>
  `,
  styles: [`
    .step-content {
      margin: 16px 0;
      min-height: 300px;
    }

    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }

    .button-row {
      margin-top: 16px;
      display: flex;
      gap: 8px;
      justify-content: flex-end;
    }

    .checkbox-group {
      margin: 16px 0;
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .review-section {
      margin-bottom: 24px;
      padding: 16px;
      border: 1px solid #e0e0e0;
      border-radius: 4px;
    }

    .review-section h4 {
      margin-top: 0;
      color: #1976d2;
    }

    .review-section p {
      margin: 8px 0;
    }
  `]
})
export class SubscriptionStepperComponent {
  @Input() editMode = false;
  @Input() existingPlan?: SubscriptionPlanDto;
  @Output() planSubmitted = new EventEmitter<CreateSubscriptionPlanDto>();
  @Output() planUpdated = new EventEmitter<{ id: string, plan: CreateSubscriptionPlanDto }>();

  private fb = inject(FormBuilder);

  basicInfoForm: FormGroup;
  featuresForm: FormGroup;
  displayForm: FormGroup;

  constructor() {
    this.basicInfoForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: [''],
      price: [0, [Validators.required, Validators.min(0.01)]],
      billingCycleId: ['', Validators.required],
      currencyId: ['', Validators.required]
    });

    this.featuresForm = this.fb.group({
      messagingCount: [10, [Validators.required, Validators.min(1)]],
      deliveryFrequencyDays: [30, [Validators.required, Validators.min(1)]],
      maxPauseDurationDays: [90, [Validators.required, Validators.min(1)]],
      includesMedicationDelivery: [true],
      includesFollowUpCare: [true],
      features: ['']
    });

    this.displayForm = this.fb.group({
      displayOrder: [1, [Validators.required, Validators.min(1)]],
      isActive: [true],
      isMostPopular: [false],
      isTrending: [false]
    });

    if (this.existingPlan) {
      this.populateFormsWithExistingData();
    }
  }

  ngOnInit() {
    if (this.existingPlan) {
      this.populateFormsWithExistingData();
    }
  }

  private populateFormsWithExistingData() {
    if (!this.existingPlan) return;

    this.basicInfoForm.patchValue({
      name: this.existingPlan.name,
      description: this.existingPlan.description,
      price: this.existingPlan.price,
      billingCycleId: this.existingPlan.billingCycleId,
      currencyId: this.existingPlan.currencyId
    });

    this.displayForm.patchValue({
      displayOrder: this.existingPlan.displayOrder,
      isActive: this.existingPlan.isActive,
      isMostPopular: this.existingPlan.isMostPopular,
      isTrending: this.existingPlan.isTrending
    });

    if (this.existingPlan.features) {
      this.featuresForm.patchValue({
        features: this.existingPlan.features
      });
    }
  }

  getFormValue(fieldName: string): any {
    return this.basicInfoForm.get(fieldName)?.value || 
           this.featuresForm.get(fieldName)?.value || 
           this.displayForm.get(fieldName)?.value;
  }

  isFormValid(): boolean {
    return this.basicInfoForm.valid && this.featuresForm.valid && this.displayForm.valid;
  }

  onSubmit() {
    if (!this.isFormValid()) return;

    const planData: CreateSubscriptionPlanDto = {
      ...this.basicInfoForm.value,
      ...this.featuresForm.value,
      ...this.displayForm.value
    };

    if (this.editMode && this.existingPlan) {
      this.planUpdated.emit({ id: this.existingPlan.id, plan: planData });
    } else {
      this.planSubmitted.emit(planData);
    }
  }
}
