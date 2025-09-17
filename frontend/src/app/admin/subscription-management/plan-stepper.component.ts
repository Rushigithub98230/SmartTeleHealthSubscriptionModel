import { Component, OnInit, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatStepperModule, MatStepper } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';

import { 
  CreateSubscriptionPlanDto, 
  UpdateSubscriptionPlanDto, 
  SubscriptionPlanDto,
  MasterBillingCycle,
  MasterCurrency,
  MasterPrivilegeType,
  Privilege,
  PlanPrivilegeDto
} from '../../models/subscription.models';
import { MasterDataService } from '../../services/master-data.service';
import { SubscriptionService } from '../../services/subscription.service';
import { CommonService } from '../../services/common.service';

@Component({
  selector: 'app-plan-stepper',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatStepperModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatCheckboxModule,
    MatIconModule,
    MatCardModule,
    MatChipsModule,
    MatTooltipModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './plan-stepper.component.html',
  styleUrls: ['./plan-stepper.component.scss']
})
export class PlanStepperComponent implements OnInit {
  @Input() editingPlan: SubscriptionPlanDto | null = null;
  @Output() planCreated = new EventEmitter<CreateSubscriptionPlanDto>();
  @Output() planUpdated = new EventEmitter<UpdateSubscriptionPlanDto>();
  @Output() cancelled = new EventEmitter<void>();

  // Form groups for each step
  basicInfoForm!: FormGroup;
  pricingForm!: FormGroup;
  featuresForm!: FormGroup;
  trialMarketingForm!: FormGroup;
  stripeForm!: FormGroup;
  privilegesForm!: FormGroup;

  // Master data
  billingCycles: MasterBillingCycle[] = [];
  currencies: MasterCurrency[] = [];
  privilegeTypes: MasterPrivilegeType[] = [];
  privileges: Privilege[] = [];
  categories: any[] = [];

  // Privilege management
  selectedPrivileges: PlanPrivilegeDto[] = [];

  // Validation error handling
  backendValidationErrors: { [key: string]: string[] } = {};
  isSubmitting = false;
  
  // Make Object available in template
  Object = Object;

  // Services
  private fb = inject(FormBuilder);
  private masterDataService = inject(MasterDataService);
  private subscriptionService = inject(SubscriptionService);
  private snackBar = inject(MatSnackBar);
  private commonService = inject(CommonService);

  ngOnInit() {
    this.initializeForms();
    this.loadMasterData();
  }

  private initializeForms() {
    // Step 1: Basic Information
    this.basicInfoForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      shortDescription: ['', [Validators.maxLength(200)]],
      features: ['', [Validators.maxLength(1000)]],
      terms: ['', [Validators.maxLength(500)]],
      categoryId: ['', Validators.required]
    });

    // Step 2: Pricing
    this.pricingForm = this.fb.group({
      price: [0, [Validators.required, Validators.min(0.01)]],
      discountedPrice: [null, Validators.min(0)],
      discountValidUntil: [null],
      billingCycleId: ['', Validators.required],
      currencyId: ['', Validators.required]
    });

    // Step 3: Features & Limits
    this.featuresForm = this.fb.group({
      messagingCount: [10, [Validators.required, Validators.min(0)]],
      includesMedicationDelivery: [true],
      includesFollowUpCare: [true],
      deliveryFrequencyDays: [30, [Validators.required, Validators.min(1)]],
      maxPauseDurationDays: [90, [Validators.required, Validators.min(0)]],
      maxConcurrentUsers: [1, [Validators.required, Validators.min(1)]],
      gracePeriodDays: [0, [Validators.required, Validators.min(0)]]
    });

    // Step 4: Trial & Marketing
    this.trialMarketingForm = this.fb.group({
      isTrialAllowed: [false],
      trialDurationInDays: [0, [Validators.required, Validators.min(0)]],
      isFeatured: [false],
      isMostPopular: [false],
      isTrending: [false],
      displayOrder: [0, [Validators.required, Validators.min(0)]],
      effectiveDate: [null],
      expirationDate: [null]
    });

    // Step 5: Stripe Integration
    this.stripeForm = this.fb.group({
      stripeProductId: [''],
      stripeMonthlyPriceId: [''],
      stripeQuarterlyPriceId: [''],
      stripeAnnualPriceId: ['']
    });

    // Step 6: Privileges
    this.privilegesForm = this.fb.group({
      // This will be managed dynamically
    });

    // Step 7: Status
    this.basicInfoForm.addControl('isActive', this.fb.control(true));
  }

  private loadMasterData() {
    let loadedCount = 0;
    const totalLoads = 5; // billing cycles, currencies, categories, privilege types, privileges

    const checkIfAllLoaded = () => {
      loadedCount++;
      if (loadedCount === totalLoads && this.editingPlan) {
        // All master data loaded, now populate forms
        setTimeout(() => {
          console.log('All master data loaded, populating forms...');
          this.populateFormsForEdit();
        }, 200); // Increased delay to ensure forms are ready
      }
    };

    // Load billing cycles
    this.masterDataService.getBillingCycles().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.billingCycles = response.data;
        }
        checkIfAllLoaded();
      },
      error: (err) => {
        this.snackBar.open('Failed to load billing cycles', 'Close', { duration: 3000 });
        checkIfAllLoaded();
      }
    });

    // Load currencies
    this.masterDataService.getCurrencies().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.currencies = response.data;
        }
        checkIfAllLoaded();
      },
      error: (err) => {
        this.snackBar.open('Failed to load currencies', 'Close', { duration: 3000 });
        checkIfAllLoaded();
      }
    });

    // Load categories
    this.commonService.getWithAuth<any[]>('/api/Categories').subscribe({
      next: (response: any) => {
        if (response.statusCode === 200 && response.data) {
          this.categories = Array.isArray(response.data) ? response.data : [];
        } else {
          this.categories = [];
          this.snackBar.open('No categories found', 'Close', { duration: 3000 });
        }
        checkIfAllLoaded();
      },
      error: (err) => {
        console.error('Error loading categories:', err);
        this.snackBar.open('Failed to load categories', 'Close', { duration: 3000 });
        this.categories = [];
        checkIfAllLoaded();
      }
    });

    // Load privilege types
    this.masterDataService.getPrivilegeTypes().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.privilegeTypes = response.data;
        }
        checkIfAllLoaded();
      },
      error: (err) => {
        this.snackBar.open('Failed to load privilege types', 'Close', { duration: 3000 });
        checkIfAllLoaded();
      }
    });

    // Load privileges
    this.masterDataService.getPrivileges().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          // Handle the response structure - it might be wrapped in a data object
          if (response.data && Array.isArray(response.data)) {
            this.privileges = response.data;
          } else if (response.data && (response.data as any).privileges && Array.isArray((response.data as any).privileges)) {
            this.privileges = (response.data as any).privileges;
          } else {
            this.privileges = [];
          }
          console.log('Loaded privileges:', this.privileges);
        }
        checkIfAllLoaded();
      },
      error: (err) => {
        console.error('Error loading privileges:', err);
        this.snackBar.open('Failed to load privileges', 'Close', { duration: 3000 });
        checkIfAllLoaded();
      }
    });
  }

  private populateFormsForEdit() {
    if (!this.editingPlan) {
      console.log('No editing plan provided');
      return;
    }

    // Check if forms are initialized
    if (!this.basicInfoForm || !this.pricingForm || !this.featuresForm || !this.trialMarketingForm || !this.stripeForm) {
      console.log('Forms not initialized yet, retrying in 100ms...');
      setTimeout(() => {
        this.populateFormsForEdit();
      }, 100);
      return;
    }

    console.log('Populating forms for edit with plan:', this.editingPlan);

    // Populate basic info
    this.basicInfoForm.patchValue({
      name: this.editingPlan.name,
      description: this.editingPlan.description,
      shortDescription: this.editingPlan.shortDescription,
      features: this.editingPlan.features,
      terms: this.editingPlan.terms,
      categoryId: this.editingPlan.categoryId,
      isActive: this.editingPlan.isActive
    });

    console.log('Basic info form values after patch:', this.basicInfoForm.value);
    console.log('Basic info form valid:', this.basicInfoForm.valid);
    console.log('Basic info form touched:', this.basicInfoForm.touched);

    // Populate pricing
    this.pricingForm.patchValue({
      price: this.editingPlan.price,
      discountedPrice: this.editingPlan.discountedPrice,
      discountValidUntil: this.editingPlan.discountValidUntil,
      billingCycleId: this.editingPlan.billingCycleId,
      currencyId: this.editingPlan.currencyId
    });

    console.log('Pricing form values after patch:', this.pricingForm.value);

    // Populate features
    this.featuresForm.patchValue({
      messagingCount: this.editingPlan.messagingCount,
      includesMedicationDelivery: this.editingPlan.includesMedicationDelivery,
      includesFollowUpCare: this.editingPlan.includesFollowUpCare,
      deliveryFrequencyDays: this.editingPlan.deliveryFrequencyDays,
      maxPauseDurationDays: this.editingPlan.maxPauseDurationDays,
      maxConcurrentUsers: this.editingPlan.maxConcurrentUsers,
      gracePeriodDays: this.editingPlan.gracePeriodDays
    });

    console.log('Features form values after patch:', this.featuresForm.value);

    // Populate trial & marketing
    this.trialMarketingForm.patchValue({
      isTrialAllowed: this.editingPlan.isTrialAllowed,
      trialDurationInDays: this.editingPlan.trialDurationInDays,
      isFeatured: this.editingPlan.isFeatured,
      isMostPopular: this.editingPlan.isMostPopular,
      isTrending: this.editingPlan.isTrending,
      displayOrder: this.editingPlan.displayOrder,
      effectiveDate: this.editingPlan.effectiveDate,
      expirationDate: this.editingPlan.expirationDate
    });

    console.log('Trial & marketing form values after patch:', this.trialMarketingForm.value);

    // Populate Stripe
    this.stripeForm.patchValue({
      stripeProductId: this.editingPlan.stripeProductId,
      stripeMonthlyPriceId: this.editingPlan.stripeMonthlyPriceId,
      stripeQuarterlyPriceId: this.editingPlan.stripeQuarterlyPriceId,
      stripeAnnualPriceId: this.editingPlan.stripeAnnualPriceId
    });

    console.log('Stripe form values after patch:', this.stripeForm.value);

    // Load privileges for this plan
    this.loadPlanPrivileges();

    // Force form update and mark as touched to show values
    this.basicInfoForm.markAsTouched();
    this.pricingForm.markAsTouched();
    this.featuresForm.markAsTouched();
    this.trialMarketingForm.markAsTouched();
    this.stripeForm.markAsTouched();

    console.log('All forms populated and marked as touched');
  }

  private loadPlanPrivileges() {
    if (!this.editingPlan?.id) return;
    
    this.subscriptionService.getPlanPrivileges(this.editingPlan.id).subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          this.selectedPrivileges = response.data;
        }
      },
      error: (error) => {
        console.error('Error loading plan privileges:', error);
        this.snackBar.open('Failed to load plan privileges', 'Close', { duration: 3000 });
      }
    });
  }

  addPrivilege() {
    if (this.privileges.length === 0) {
      this.snackBar.open('No privileges available. Please create privileges first.', 'Close', { duration: 5000 });
      return;
    }

    const newPrivilege: PlanPrivilegeDto = {
      privilegeId: '',
      privilegeName: '',
      value: 1,
      usagePeriodId: '',
      usagePeriodName: '',
      durationMonths: 1,
      description: '',
      effectiveDate: new Date(),
      expirationDate: undefined,
      dailyLimit: undefined,
      weeklyLimit: undefined,
      monthlyLimit: undefined
    };
    this.selectedPrivileges.push(newPrivilege);
  }

  removePrivilege(index: number) {
    this.selectedPrivileges.splice(index, 1);
  }

  getPrivilegeName(privilegeId: string): string {
    const privilege = this.privileges.find(p => p.id === privilegeId);
    return privilege ? privilege.name : 'Unknown Privilege';
  }

  getBillingCycleName(billingCycleId: string): string {
    const cycle = this.billingCycles.find(c => c.id === billingCycleId);
    return cycle ? cycle.name : 'Unknown Cycle';
  }

  getCurrencyName(currencyId: string): string {
    const currency = this.currencies.find(c => c.id === currencyId);
    return currency ? currency.name : 'Unknown Currency';
  }

  getCategoryName(categoryId: string): string {
    if (!this.categories || !Array.isArray(this.categories)) {
      return 'Unknown Category';
    }
    const category = this.categories.find(c => c.id === categoryId);
    return category ? category.name : 'Unknown Category';
  }

  onPrivilegeChange(privilege: PlanPrivilegeDto, privilegeId: string) {
    const selectedPrivilege = this.privileges.find(p => p.id === privilegeId);
    if (selectedPrivilege) {
      privilege.privilegeName = selectedPrivilege.name;
      privilege.privilegeId = privilegeId;
    }
  }

  onUsagePeriodChange(privilege: PlanPrivilegeDto, usagePeriodId: string) {
    const selectedPeriod = this.privilegeTypes.find(p => p.id === usagePeriodId);
    if (selectedPeriod) {
      privilege.usagePeriodName = selectedPeriod.name;
      privilege.usagePeriodId = usagePeriodId;
    }
  }

  isPrivilegeFormValid(privilege: PlanPrivilegeDto): boolean {
    return !!(privilege.privilegeId && 
              privilege.value >= 0 && 
              privilege.usagePeriodId && 
              privilege.durationMonths > 0);
  }

  areAllPrivilegesValid(): boolean {
    return this.selectedPrivileges.every(p => this.isPrivilegeFormValid(p));
  }

  onSubmit(stepper: MatStepper) {
    this.clearBackendValidationErrors();
    
    if (this.isFormValid()) {
      this.isSubmitting = true;
      const planData = this.buildPlanData();
      
      if (this.editingPlan) {
        this.planUpdated.emit(planData as UpdateSubscriptionPlanDto);
      } else {
        this.planCreated.emit(planData as CreateSubscriptionPlanDto);
      }
    } else {
      this.showFormValidationErrors();
      this.snackBar.open('Please fix the validation errors before proceeding', 'Close', { duration: 5000 });
    }
  }

  isFormValid(): boolean {
    return this.basicInfoForm.valid && 
           this.pricingForm.valid && 
           this.featuresForm.valid && 
           this.trialMarketingForm.valid && 
           this.stripeForm.valid &&
           this.areAllPrivilegesValid();
  }

  private buildPlanData(): CreateSubscriptionPlanDto | UpdateSubscriptionPlanDto {
    const basicInfo = this.basicInfoForm.value;
    const pricing = this.pricingForm.value;
    const features = this.featuresForm.value;
    const trialMarketing = this.trialMarketingForm.value;
    const stripe = this.stripeForm.value;

    const planData: any = {
      ...basicInfo,
      ...pricing,
      ...features,
      ...trialMarketing,
      ...stripe,
      privileges: this.selectedPrivileges
    };

    // Ensure string fields are properly handled (convert null/undefined to empty string)
    planData.description = planData.description || '';
    planData.shortDescription = planData.shortDescription || '';
    planData.features = planData.features || '';
    planData.terms = planData.terms || '';
    planData.stripeProductId = planData.stripeProductId || '';
    planData.stripeMonthlyPriceId = planData.stripeMonthlyPriceId || '';
    planData.stripeQuarterlyPriceId = planData.stripeQuarterlyPriceId || '';
    planData.stripeAnnualPriceId = planData.stripeAnnualPriceId || '';

    // Convert privilege data to match backend expectations
    if (planData.privileges && Array.isArray(planData.privileges)) {
      planData.privileges = planData.privileges.map((privilege: any) => ({
        privilegeId: privilege.privilegeId || '',
        value: privilege.value || 1,
        usagePeriodId: privilege.usagePeriodId || '',
        durationMonths: privilege.durationMonths || 1,
        description: privilege.description || '',
        effectiveDate: privilege.effectiveDate || null,
        expirationDate: privilege.expirationDate || null,
        dailyLimit: privilege.dailyLimit || null,
        weeklyLimit: privilege.weeklyLimit || null,
        monthlyLimit: privilege.monthlyLimit || null
      }));
    }

    if (this.editingPlan) {
      planData.id = this.editingPlan.id;
    }

    return planData;
  }

  onCancel() {
    this.cancelled.emit();
  }

  navigateToPrivileges() {
    // This would typically navigate to a privileges management page
    // For now, we'll show a message
    this.snackBar.open('Privilege management feature coming soon. Please contact admin to create privileges.', 'Close', { duration: 5000 });
  }

  // Validation error handling methods
  clearBackendValidationErrors() {
    this.backendValidationErrors = {};
  }

  setBackendValidationErrors(errors: { [key: string]: string[] }) {
    this.backendValidationErrors = errors;
  }

  getBackendValidationError(fieldName: string): string[] {
    return this.backendValidationErrors[fieldName] || [];
  }

  hasBackendValidationError(fieldName: string): boolean {
    return this.backendValidationErrors[fieldName] && this.backendValidationErrors[fieldName].length > 0;
  }

  showFormValidationErrors() {
    // Mark all forms as touched to show validation errors
    this.basicInfoForm.markAllAsTouched();
    this.pricingForm.markAllAsTouched();
    this.featuresForm.markAllAsTouched();
    this.trialMarketingForm.markAllAsTouched();
    this.stripeForm.markAllAsTouched();
  }

  getFieldErrorMessage(form: FormGroup, fieldName: string): string {
    const field = form.get(fieldName);
    if (!field || !field.errors || !field.touched) {
      return '';
    }

    const errors = field.errors;
    
    if (errors['required']) {
      return `${this.getFieldDisplayName(fieldName)} is required`;
    }
    if (errors['maxlength']) {
      return `${this.getFieldDisplayName(fieldName)} must be no more than ${errors['maxlength'].requiredLength} characters`;
    }
    if (errors['minlength']) {
      return `${this.getFieldDisplayName(fieldName)} must be at least ${errors['minlength'].requiredLength} characters`;
    }
    if (errors['min']) {
      return `${this.getFieldDisplayName(fieldName)} must be at least ${errors['min'].min}`;
    }
    if (errors['max']) {
      return `${this.getFieldDisplayName(fieldName)} must be no more than ${errors['max'].max}`;
    }
    if (errors['email']) {
      return 'Please enter a valid email address';
    }
    if (errors['pattern']) {
      return `${this.getFieldDisplayName(fieldName)} format is invalid`;
    }

    return 'Invalid value';
  }

  getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      'name': 'Plan Name',
      'description': 'Description',
      'shortDescription': 'Short Description',
      'features': 'Features',
      'terms': 'Terms & Conditions',
      'categoryId': 'Category',
      'price': 'Price',
      'discountedPrice': 'Discounted Price',
      'billingCycleId': 'Billing Cycle',
      'currencyId': 'Currency',
      'messagingCount': 'Messaging Count',
      'deliveryFrequencyDays': 'Delivery Frequency',
      'maxPauseDurationDays': 'Max Pause Duration',
      'maxConcurrentUsers': 'Max Concurrent Users',
      'gracePeriodDays': 'Grace Period',
      'trialDurationInDays': 'Trial Duration',
      'displayOrder': 'Display Order'
    };
    return displayNames[fieldName] || fieldName;
  }

  getPrivilegeErrorMessage(privilege: PlanPrivilegeDto): string {
    if (!privilege.privilegeId) {
      return 'Please select a privilege';
    }
    if (privilege.value < -1) {
      return 'Value must be -1 (unlimited), 0 (disabled), or positive number';
    }
    if (!privilege.usagePeriodId) {
      return 'Please select a usage period';
    }
    if (privilege.durationMonths <= 0) {
      return 'Duration must be at least 1 month';
    }
    return '';
  }

  isFieldInvalid(form: FormGroup, fieldName: string): boolean {
    const field = form.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  isFieldValid(form: FormGroup, fieldName: string): boolean {
    const field = form.get(fieldName);
    return !!(field && field.valid && field.touched);
  }
}
