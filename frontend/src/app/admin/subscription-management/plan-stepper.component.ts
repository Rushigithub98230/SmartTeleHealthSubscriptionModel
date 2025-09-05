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
    MatTooltipModule
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

  // Services
  private fb = inject(FormBuilder);
  private masterDataService = inject(MasterDataService);
  private subscriptionService = inject(SubscriptionService);
  private snackBar = inject(MatSnackBar);

  ngOnInit() {
    this.initializeForms();
    this.loadMasterData();
    
    if (this.editingPlan) {
      this.populateFormsForEdit();
    }
  }

  private initializeForms() {
    // Step 1: Basic Information
    this.basicInfoForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', Validators.maxLength(500)],
      shortDescription: ['', Validators.maxLength(200)],
      features: [''],
      terms: [''],
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
    // Load billing cycles
    this.masterDataService.getBillingCycles().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.billingCycles = response.data;
        }
      },
      error: (err) => {
        this.snackBar.open('Failed to load billing cycles', 'Close', { duration: 3000 });
      }
    });

    // Load currencies
    this.masterDataService.getCurrencies().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.currencies = response.data;
        }
      },
      error: (err) => {
        this.snackBar.open('Failed to load currencies', 'Close', { duration: 3000 });
      }
    });

    // Load categories
    this.subscriptionService.getCategories().subscribe({
      next: (response) => {
        console.log('=== CATEGORIES API RESPONSE DEBUG ===');
        console.log('Full response:', response);
        console.log('Response data type:', typeof response.data);
        console.log('Response data:', response.data);
        console.log('Response data keys:', response.data ? Object.keys(response.data) : 'No data');
        console.log('Response data JSON:', JSON.stringify(response.data, null, 2));
        console.log('Response data is array:', Array.isArray(response.data));
        console.log('Response data length:', response.data ? (Array.isArray(response.data) ? response.data.length : 'Not an array') : 'No data');
        console.log('=== END DEBUG ===');
        
        if (response.statusCode === 200) {
          // Handle the new response structure where categories are directly in data
          if (Array.isArray(response.data)) {
            this.categories = response.data;
            console.log('Using direct array categories:', this.categories);
          } else if (response.data && (response.data as any).categories) {
            // Fallback for old paginated response structure
            this.categories = (response.data as any).categories;
            console.log('Using old paginated categories structure:', this.categories);
          } else if (response.data && (response.data as any).Categories) {
            // Fallback for uppercase Categories
            this.categories = (response.data as any).Categories;
            console.log('Using uppercase Categories:', this.categories);
          } else if (response.data && typeof response.data === 'object' && Object.keys(response.data).length === 0) {
            // Handle empty object response - likely no categories in database
            this.categories = [];
            console.log('Empty object response - no categories found in database');
            this.snackBar.open('No categories found in database. Please contact administrator to add categories.', 'Close', { duration: 5000 });
          } else {
            this.categories = [];
            console.log('No categories found, using empty array');
            console.log('Response data structure:', JSON.stringify(response.data, null, 2));
          }
          console.log('Final categories array:', this.categories);
        } else {
          console.error('Categories API returned error status:', response.statusCode);
          this.categories = [];
        }
      },
      error: (err) => {
        console.error('Error loading categories:', err);
        this.snackBar.open('Failed to load categories', 'Close', { duration: 3000 });
        this.categories = [];
      }
    });

    // Load privilege types
    this.masterDataService.getPrivilegeTypes().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.privilegeTypes = response.data;
        }
      },
      error: (err) => {
        this.snackBar.open('Failed to load privilege types', 'Close', { duration: 3000 });
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
      },
      error: (err) => {
        console.error('Error loading privileges:', err);
        this.snackBar.open('Failed to load privileges', 'Close', { duration: 3000 });
      }
    });
  }

  private populateFormsForEdit() {
    if (!this.editingPlan) return;

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

    // Populate pricing
    this.pricingForm.patchValue({
      price: this.editingPlan.price,
      discountedPrice: this.editingPlan.discountedPrice,
      discountValidUntil: this.editingPlan.discountValidUntil,
      billingCycleId: this.editingPlan.billingCycleId,
      currencyId: this.editingPlan.currencyId
    });

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

    // Populate Stripe
    this.stripeForm.patchValue({
      stripeProductId: this.editingPlan.stripeProductId,
      stripeMonthlyPriceId: this.editingPlan.stripeMonthlyPriceId,
      stripeQuarterlyPriceId: this.editingPlan.stripeQuarterlyPriceId,
      stripeAnnualPriceId: this.editingPlan.stripeAnnualPriceId
    });

    // Populate privileges (privileges will be loaded separately)
    this.selectedPrivileges = [];
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
    if (this.isFormValid()) {
      const planData = this.buildPlanData();
      
      if (this.editingPlan) {
        this.planUpdated.emit(planData as UpdateSubscriptionPlanDto);
      } else {
        this.planCreated.emit(planData as CreateSubscriptionPlanDto);
      }
    } else {
      this.snackBar.open('Please fill in all required fields', 'Close', { duration: 3000 });
    }
  }

  private isFormValid(): boolean {
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
}
