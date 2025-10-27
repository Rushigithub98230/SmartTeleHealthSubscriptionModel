import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, DecimalPipe } from '@angular/common';
import { CreateSubscriptionPlanDto, SubscriptionPlanDto } from '../../../core/models/subscription-plan.model';

@Component({
  selector: 'app-plan-form',
  templateUrl: './plan-form.component.html',
  styleUrls: ['./plan-form.component.scss'],
  imports: [CommonModule, ReactiveFormsModule, DecimalPipe],
  standalone: true
})
export class PlanFormComponent implements OnInit {
  @Input() plan?: SubscriptionPlanDto;
  @Input() isEditMode: boolean = false;
  @Output() planSubmit = new EventEmitter<CreateSubscriptionPlanDto>();
  @Output() cancel = new EventEmitter<void>();

  planForm!: FormGroup;
  showDiscountSection: boolean = false;

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    this.planForm = this.fb.group({
      // Basic Information
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      shortDescription: ['', [Validators.maxLength(200)]],
      price: [0, [Validators.required, Validators.min(0.01)]],
      
      // Discount Information
      discountedPrice: [null],
      discountValidUntil: [null],
      
      // Billing and Currency
      billingCycleId: ['', Validators.required],
      currencyId: ['', Validators.required],
      categoryId: ['', Validators.required],
      
      // Trial Configuration
      isTrialAllowed: [false],
      trialDurationInDays: [0, [Validators.min(0), Validators.max(365)]],
      
      // Marketing Properties
      isFeatured: [false],
      isMostPopular: [false],
      isTrending: [false],
      displayOrder: [0, [Validators.min(0)]],
      
      // Plan Features
      messagingCount: [0, [Validators.min(0)]],
      includesMedicationDelivery: [false],
      includesFollowUpCare: [false],
      deliveryFrequencyDays: [0, [Validators.min(0)]],
      maxPauseDurationDays: [0, [Validators.min(0)]],
      maxConcurrentUsers: [1, [Validators.min(1)]],
      gracePeriodDays: [0, [Validators.min(0)]],
      
      // Status
      isActive: [true],
      
      // Healthcare Pricing Model
      isAutoCalculatedPrice: [false],
      adminCommissionPercent: [null, [Validators.min(0), Validators.max(100)]],
      priceChangeNoticeDays: [10, [Validators.min(1), Validators.max(90)]]
    });

    // Add conditional validators for discount
    this.addDiscountValidators();
    
    // Watch for discount price changes
    this.planForm.get('discountedPrice')?.valueChanges.subscribe(() => {
      this.updateDiscountSectionVisibility();
    });

    // Watch for price changes to validate discount
    this.planForm.get('price')?.valueChanges.subscribe(() => {
      this.validateDiscountPrice();
    });

    // Load existing plan data if in edit mode
    if (this.isEditMode && this.plan) {
      this.loadPlanData();
    }
  }

  private addDiscountValidators(): void {
    const discountedPriceControl = this.planForm.get('discountedPrice');
    const priceControl = this.planForm.get('price');
    const discountValidUntilControl = this.planForm.get('discountValidUntil');

    // Custom validator for discounted price
    discountedPriceControl?.setValidators([
      this.discountPriceValidator.bind(this)
    ]);

    // Custom validator for discount expiry date
    discountValidUntilControl?.setValidators([
      this.discountDateValidator.bind(this)
    ]);
  }

  private discountPriceValidator(control: any) {
    const discountedPrice = control.value;
    const basePrice = this.planForm.get('price')?.value;

    if (!discountedPrice) {
      return null; // No discount is valid
    }

    if (discountedPrice <= 0) {
      return { invalidDiscountPrice: 'Discounted price must be greater than 0' };
    }

    if (basePrice && discountedPrice >= basePrice) {
      return { invalidDiscountPrice: 'Discounted price must be less than base price' };
    }

    if (basePrice) {
      const maxDiscount = basePrice * 0.9; // 90% max discount
      if (discountedPrice < maxDiscount) {
        return { invalidDiscountPrice: 'Discount cannot exceed 90% of base price' };
      }
    }

    return null;
  }

  private discountDateValidator(control: any) {
    const discountValidUntil = control.value;
    const discountedPrice = this.planForm.get('discountedPrice')?.value;

    // Only validate if discounted price is set
    if (!discountedPrice) {
      return null;
    }

    if (!discountValidUntil) {
      return { required: 'Discount expiry date is required when discounted price is set' };
    }

    const expiryDate = new Date(discountValidUntil);
    const now = new Date();

    if (expiryDate <= now) {
      return { invalidDate: 'Discount expiry date must be in the future' };
    }

    const oneYearFromNow = new Date();
    oneYearFromNow.setFullYear(oneYearFromNow.getFullYear() + 1);

    if (expiryDate > oneYearFromNow) {
      return { invalidDate: 'Discount cannot be valid for more than 1 year' };
    }

    return null;
  }

  private updateDiscountSectionVisibility(): void {
    const discountedPrice = this.planForm.get('discountedPrice')?.value;
    this.showDiscountSection = !!discountedPrice;
  }

  private validateDiscountPrice(): void {
    const discountedPriceControl = this.planForm.get('discountedPrice');
    const discountValidUntilControl = this.planForm.get('discountValidUntil');
    
    // Re-validate discount fields when base price changes
    discountedPriceControl?.updateValueAndValidity();
    discountValidUntilControl?.updateValueAndValidity();
  }

  private loadPlanData(): void {
    if (this.plan) {
      this.planForm.patchValue({
        name: this.plan.name,
        description: this.plan.description,
        shortDescription: this.plan.shortDescription,
        price: this.plan.basePrice || this.plan.price,
        discountedPrice: this.plan.discountedPrice,
        discountValidUntil: this.plan.discountValidUntil,
        billingCycleId: this.plan.billingCycleId,
        currencyId: this.plan.currencyId,
        categoryId: this.plan.categoryId,
        isTrialAllowed: this.plan.isTrialAllowed,
        trialDurationInDays: this.plan.trialDurationInDays,
        isFeatured: this.plan.isFeatured,
        isMostPopular: this.plan.isMostPopular,
        isTrending: this.plan.isTrending,
        displayOrder: this.plan.displayOrder,
        messagingCount: this.plan.messagingCount,
        includesMedicationDelivery: this.plan.includesMedicationDelivery,
        includesFollowUpCare: this.plan.includesFollowUpCare,
        deliveryFrequencyDays: this.plan.deliveryFrequencyDays,
        maxPauseDurationDays: this.plan.maxPauseDurationDays,
        maxConcurrentUsers: this.plan.maxConcurrentUsers,
        gracePeriodDays: this.plan.gracePeriodDays,
        isActive: this.plan.isActive,
        isAutoCalculatedPrice: this.plan.isAutoCalculatedPrice,
        adminCommissionPercent: this.plan.adminCommissionPercent,
        priceChangeNoticeDays: this.plan.priceChangeNoticeDays
      });

      this.updateDiscountSectionVisibility();
    }
  }

  toggleDiscountSection(): void {
    this.showDiscountSection = !this.showDiscountSection;
    
    if (!this.showDiscountSection) {
      // Clear discount fields when hiding section
      this.planForm.patchValue({
        discountedPrice: null,
        discountValidUntil: null
      });
    }
  }

  calculateDiscountPercentage(): number {
    const basePrice = this.planForm.get('price')?.value;
    const discountedPrice = this.planForm.get('discountedPrice')?.value;

    if (!basePrice || !discountedPrice) {
      return 0;
    }

    const discountAmount = basePrice - discountedPrice;
    return Math.round((discountAmount / basePrice) * 100);
  }

  calculateDiscountAmount(): number {
    const basePrice = this.planForm.get('price')?.value;
    const discountedPrice = this.planForm.get('discountedPrice')?.value;

    if (!basePrice || !discountedPrice) {
      return 0;
    }

    return basePrice - discountedPrice;
  }

  onSubmit(): void {
    if (this.planForm.valid) {
      const formValue = this.planForm.value;
      
      // Clean up null values
      if (!formValue.discountedPrice) {
        formValue.discountedPrice = null;
        formValue.discountValidUntil = null;
      }

      this.planSubmit.emit(formValue);
    } else {
      // Mark all fields as touched to show validation errors
      Object.keys(this.planForm.controls).forEach(key => {
        this.planForm.get(key)?.markAsTouched();
      });
    }
  }

  onCancel(): void {
    this.cancel.emit();
  }

  // Helper methods for template
  hasError(fieldName: string, errorType: string): boolean {
    const field = this.planForm.get(fieldName);
    return !!(field && field.hasError(errorType) && field.touched);
  }

  getErrorMessage(fieldName: string): string {
    const field = this.planForm.get(fieldName);
    if (!field || !field.errors || !field.touched) {
      return '';
    }

    const errors = field.errors;
    
    if (errors['required']) {
      return `${this.getFieldDisplayName(fieldName)} is required`;
    }
    if (errors['min']) {
      return `${this.getFieldDisplayName(fieldName)} must be at least ${errors['min'].min}`;
    }
    if (errors['max']) {
      return `${this.getFieldDisplayName(fieldName)} must be at most ${errors['max'].max}`;
    }
    if (errors['maxlength']) {
      return `${this.getFieldDisplayName(fieldName)} must be at most ${errors['maxlength'].requiredLength} characters`;
    }
    if (errors['invalidDiscountPrice']) {
      return errors['invalidDiscountPrice'];
    }
    if (errors['invalidDate']) {
      return errors['invalidDate'];
    }

    return 'Invalid value';
  }

  private getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      name: 'Plan Name',
      description: 'Description',
      shortDescription: 'Short Description',
      price: 'Price',
      discountedPrice: 'Discounted Price',
      discountValidUntil: 'Discount Valid Until',
      billingCycleId: 'Billing Cycle',
      currencyId: 'Currency',
      categoryId: 'Category',
      trialDurationInDays: 'Trial Duration',
      displayOrder: 'Display Order',
      messagingCount: 'Messaging Count',
      deliveryFrequencyDays: 'Delivery Frequency',
      maxPauseDurationDays: 'Max Pause Duration',
      maxConcurrentUsers: 'Max Concurrent Users',
      gracePeriodDays: 'Grace Period',
      adminCommissionPercent: 'Admin Commission Percent',
      priceChangeNoticeDays: 'Price Change Notice Days'
    };

    return displayNames[fieldName] || fieldName;
  }
}







