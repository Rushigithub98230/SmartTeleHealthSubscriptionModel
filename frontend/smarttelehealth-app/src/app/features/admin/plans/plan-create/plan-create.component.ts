import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  SubscriptionPlanService,
  CategoryService,
  PrivilegeService,
  MasterDataService,
  SystemSettingsService
} from '../../../../core/services';
import {
  CreateSubscriptionPlanDto,
  PlanPrivilegeDto,
  CategoryDto,
  PrivilegeDto
} from '../../../../core/models';
import { BillingCycleDto, CurrencyDto } from '../../../../core/models/master-data.model';
import { SystemSettingsDto } from '../../../../core/services/system-settings.service';

/**
 * Admin Create Plan Component - 4-Step Stepper Form
 * 
 * APIs Used:
 * - GET /api/Categories (Step 1)
 * - GET /api/Privileges?isActive=true (Step 2)
 * - POST /api/SubscriptionPlans (Step 4 - Submit)
 * 
 * Route: /webadmin/plans/create
 * Access: Admin only
 * 
 * Steps:
 * 1. Basic Info (name, description, price, category)
 * 2. Configure Privileges (select privileges, set limits, costs)
 * 3. Billing & Discounts (monthly/quarterly/annual discounts)
 * 4. Review & Create (summary, submit)
 */
@Component({
  selector: 'app-plan-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './plan-create.component.html',
  styleUrls: ['./plan-create.component.scss']
})
export class PlanCreateComponent implements OnInit {
  // Stepper state
  currentStep = 1;
  totalSteps = 4;
  today: string = ''; // For date input validation

  // Forms for each step
  basicInfoForm!: FormGroup;
  billingForm!: FormGroup;
  privilegeForm!: FormGroup;

  // Data
  categories: CategoryDto[] = [];
  availablePrivileges: PrivilegeDto[] = [];
  selectedPrivileges: PlanPrivilegeDto[] = [];
  billingCycles: BillingCycleDto[] = [];
  currencies: CurrencyDto[] = [];
  systemSettings: SystemSettingsDto | null = null;

  // UI state
  loading = false;
  creating = false;
  loadingCycles = false;
  error: string | null = null;
  selectedPrivilegeToAdd: PrivilegeDto | null = null;

  constructor(
    private fb: FormBuilder,
    private planService: SubscriptionPlanService,
    private categoryService: CategoryService,
    private privilegeService: PrivilegeService,
    private masterDataService: MasterDataService,
    private systemSettingsService: SystemSettingsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initForms();
    this.loadSystemSettings();
    this.loadCategories();
    this.loadPrivileges();
    this.loadBillingCycles();
    this.loadCurrencies();
    
    // ✅ Add real-time price updates
    this.setupPriceUpdateListeners();
    
    // ✅ Set today's date for discount expiry validation
    this.today = new Date().toISOString().split('T')[0];
  }

  /**
   * Setup listeners for real-time price updates
   */
  setupPriceUpdateListeners(): void {
    // Listen to billing form changes
    this.billingForm.valueChanges.subscribe(() => {
      console.log('💰 Billing form changed - price updated');
    });
  }

  /**
   * Load system settings for default values
   * API: GET /api/admin/SystemSettings
   */
  loadSystemSettings(): void {
    this.systemSettingsService.getSettings().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.systemSettings = response.data;
          console.log('✅ Loaded system settings:', this.systemSettings);
          // Apply system settings to forms after they're loaded
          this.applySystemSettingsToForms();
        }
      },
      error: (error) => {
        console.error('❌ Error loading system settings:', error);
        // Continue without system settings - forms will use hardcoded defaults
      }
    });
  }

  /**
   * Apply system settings as default values to forms
   */
  applySystemSettingsToForms(): void {
    if (!this.systemSettings) return;

    // Apply to billing form defaults
    this.billingForm.patchValue({
      adminCommissionPercent: this.systemSettings.defaultAdminCommissionPercent,
      priceChangeNoticeDays: this.systemSettings.defaultPriceChangeNoticeDays
    });

    console.log('✅ Applied system settings to forms:', {
      adminCommissionPercent: this.systemSettings.defaultAdminCommissionPercent,
      priceChangeNoticeDays: this.systemSettings.defaultPriceChangeNoticeDays
    });
  }

  /**
   * Initialize all forms
   */
  initForms(): void {
    // Step 1: Basic Info Form
    this.basicInfoForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', Validators.maxLength(500)],
      shortDescription: ['', Validators.maxLength(200)],
      // ✅ REMOVED: price field - will be calculated automatically from privileges
      categoryId: ['', Validators.required],
      billingCycleId: ['', Validators.required],  // Dynamic selection
      currencyId: ['', Validators.required],      // Dynamic selection
      isTrialAllowed: [false],
      trialDurationInDays: [0],
      isFeatured: [false],
      isMostPopular: [false],
      isTrending: [false],                        // ✅ ADDED
      displayOrder: [0],
      isActive: [true],
      // Plan features
      messagingCount: [10],
      includesMedicationDelivery: [true],
      includesFollowUpCare: [true],
      deliveryFrequencyDays: [30],
      maxPauseDurationDays: [90],
      maxConcurrentUsers: [1],
      gracePeriodDays: [0]
    });

    // Step 2: Privileges Form (for stepper validation)
    this.privilegeForm = this.fb.group({
      // This form is mainly for stepper validation
      // Actual privilege data is managed in selectedPrivileges array
    });

    // Step 3: Billing & Discounts Form
    // Note: Default values will be overridden by system settings in applySystemSettingsToForms()
    this.billingForm = this.fb.group({
      // ✅ Promotional Discount Fields (matching backend)
      discountPercentage: [0, [Validators.min(0), Validators.max(100)]],
      discountValidUntil: [null],
      
      // ✅ Billing Discount Field
      billingDiscountPercentage: [0, [Validators.min(0), Validators.max(100)]],
      
      // ✅ Commission and Settings (will be patched from system settings)
      adminCommissionPercent: [20, [Validators.min(0), Validators.max(100)]], // Default 20%, will be overridden
      priceChangeNoticeDays: [10, [Validators.min(0)]], // Default 10 days, will be overridden
      
      // ✅ Tax Fields
      defaultTaxPercentage: [0, [Validators.min(0), Validators.max(100)]],
      taxNotes: ['', Validators.maxLength(500)]
    });
  }

  /**
   * Load categories for dropdown
   * API: GET /api/Categories
   */
  loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.categories = response.data;
        }
      },
      error: (error) => console.error('Error loading categories:', error)
    });
  }

  /**
   * Load available privileges
   * API: GET /api/Privileges?isActive=true
   */
  loadPrivileges(): void {
    this.privilegeService.getActivePrivileges().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.availablePrivileges = response.data;
        }
      },
      error: (error) => console.error('Error loading privileges:', error)
    });
  }

  /**
   * Load billing cycles from backend dynamically
   * API: GET /api/MasterData/billing-cycles
   */
  loadBillingCycles(): void {
    this.loadingCycles = true;
    
    this.masterDataService.getBillingCycles().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.billingCycles = response.data;
          
          // Auto-select monthly cycle if available, otherwise first
          if (this.billingCycles.length > 0) {
            const monthlyCycle = this.billingCycles.find(c => c.name?.toLowerCase().includes('month'));
            const defaultCycle = monthlyCycle || this.billingCycles[0];
            
            this.basicInfoForm.patchValue({
              billingCycleId: defaultCycle.id
            });
          }
          
          console.log('✅ Loaded billing cycles from API:', this.billingCycles);
        }
        this.loadingCycles = false;
      },
      error: (error) => {
        console.error('❌ Error loading billing cycles:', error);
        this.billingCycles = [];
        this.loadingCycles = false;
      }
    });
  }

  /**
   * Load currencies from backend dynamically
   * API: GET /api/MasterData/currencies
   */
  loadCurrencies(): void {
    this.masterDataService.getCurrencies().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.currencies = response.data;
          
          // Auto-select USD if available, otherwise first currency
          if (this.currencies.length > 0) {
            const usdCurrency = this.currencies.find(c => c.code === 'USD');
            const defaultCurrency = usdCurrency || this.currencies[0];
            
            this.basicInfoForm.patchValue({
              currencyId: defaultCurrency.id
            });
          }
          
          console.log('✅ Loaded currencies from API:', this.currencies);
        }
      },
      error: (error) => {
        console.error('❌ Error loading currencies:', error);
        this.currencies = [];
      }
    });
  }

  /**
   * Navigate to next step
   */
  nextStep(): void {
    if (this.currentStep === 1 && this.basicInfoForm.invalid) {
      this.markFormGroupTouched(this.basicInfoForm);
      return;
    }

    if (this.currentStep === 3 && this.billingForm.invalid) {
      this.markFormGroupTouched(this.billingForm);
      return;
    }

    if (this.currentStep < this.totalSteps) {
      this.currentStep++;
    }
  }

  /**
   * Navigate to previous step
   */
  previousStep(): void {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  /**
   * Add privilege to plan
   * Sets Value (total count) as main field
   * All costs default to 0 and must be explicitly set by admin
   */
  addPrivilege(privilege: PrivilegeDto): void {
    const planPrivilege: PlanPrivilegeDto = {
      privilegeId: privilege.id,
      
      // MAIN ALLOCATION (Required) - Use sensible defaults based on privilege type
      value: this.getDefaultValueForPrivilege(privilege),
      
      // PRICING - Default to 0, admin must explicitly set costs
      privilegeBaseCost: 0,           // ✅ Default to 0 - admin must set explicitly
      unitCost: 0,                    // ✅ Default to 0 - admin must set explicitly
      
      // OTHER
      durationMonths: 1,
      description: undefined,
      effectiveDate: undefined,
      expirationDate: undefined
    };

    this.selectedPrivileges.push(planPrivilege);
    this.onPrivilegeValueChange(); // Recalculate price
    console.log('✅ Added privilege - Total count:', planPrivilege.value, 'Base cost:', planPrivilege.privilegeBaseCost);
  }

  /**
   * Remove privilege from plan
   */
  removePrivilege(index: number): void {
    this.selectedPrivileges.splice(index, 1);
    this.onPrivilegeValueChange(); // Recalculate price after removal
  }

  /**
   * Get scaled preview for monthly limit based on selected billing cycle
   * Shows admin how many privileges user will get for different billing cycles
   */
  getScaledPreview(monthlyLimit: number | undefined): string {
    if (!monthlyLimit || monthlyLimit === -1) return 'Unlimited';
    if (monthlyLimit === 0) return 'Disabled';

    const billingCycleId = this.basicInfoForm.value.billingCycleId;
    const selectedCycle = this.billingCycles.find(c => c.id === billingCycleId);
    
    if (!selectedCycle) return '';

    const monthsInCycle = selectedCycle.durationInDays / 30.0;
    const scaledValue = Math.ceil(monthlyLimit * monthsInCycle);

    return `${scaledValue} total`;
  }

  /**
   * Get available privileges (not already selected)
   */
  getAvailablePrivileges(): PrivilegeDto[] {
    const selectedIds = this.selectedPrivileges.map(p => p.privilegeId);
    return this.availablePrivileges.filter(p => !selectedIds.includes(p.id));
  }

  /**
   * Get privilege name by ID
   */
  getPrivilegeName(privilegeId: string): string {
    const priv = this.availablePrivileges.find(p => p.id === privilegeId);
    return priv?.name || 'Privilege';
  }

  /**
   * Handle privilege selection change
   */
  onPrivilegeSelectionChange(event: any): void {
    this.selectedPrivilegeToAdd = event.value;
  }

  /**
   * Get category name by ID
   */
  getCategoryName(categoryId: string): string {
    const category = this.categories.find(c => c.id === categoryId);
    return category?.name || 'Unknown Category';
  }

  /**
   * Get currency name by ID
   */
  getCurrencyName(currencyId: string): string {
    const currency = this.currencies.find(c => c.id === currencyId);
    return currency ? `${currency.code} - ${currency.name}` : 'Unknown Currency';
  }

  getBillingCycleName(billingCycleId: string): string {
    const cycle = this.billingCycles.find(c => c.id === billingCycleId);
    return cycle ? cycle.name : 'Unknown';
  }

  /**
   * Submit plan creation
   * API: POST /api/SubscriptionPlans/admin
   */
  submitPlan(): void {
    if (this.basicInfoForm.invalid || this.billingForm.invalid) {
      this.error = 'Please fill all required fields';
      return;
    }

    if (this.selectedPrivileges.length === 0) {
      this.error = 'Please configure at least one privilege';
      return;
    }

    // ✅ Validate privilege GUIDs are not empty
    const hasInvalidPrivileges = this.selectedPrivileges.some(p => 
      !p.privilegeId || 
      p.privilegeId === '00000000-0000-0000-0000-000000000000'
    );

    if (hasInvalidPrivileges) {
      this.error = 'Invalid privilege configuration. Please check privilege IDs.';
      console.error('❌ Invalid privileges detected:', this.selectedPrivileges);
      return;
    }

    // ✅ Validate that all privileges have explicit costs set
    const privilegesWithMissingCosts = this.selectedPrivileges.filter(p => 
      p.privilegeBaseCost === undefined || p.privilegeBaseCost === null || p.privilegeBaseCost < 0
    );

    if (privilegesWithMissingCosts.length > 0) {
      this.error = 'Please set explicit costs for all privileges. All costs must be explicitly entered (use 0 if no cost) and cannot be negative.';
      console.error('❌ Privileges with missing or invalid costs:', privilegesWithMissingCosts);
      return;
    }

    // ✅ Validate that all privileges have explicit unit costs set
    const privilegesWithMissingUnitCosts = this.selectedPrivileges.filter(p => 
      p.unitCost === undefined || p.unitCost === null || p.unitCost < 0
    );

    if (privilegesWithMissingUnitCosts.length > 0) {
      this.error = 'Please set explicit unit costs for all privileges. All costs must be explicitly entered (use 0 if no cost) and cannot be negative.';
      console.error('❌ Privileges with missing unit costs:', privilegesWithMissingUnitCosts);
      return;
    }

    this.creating = true;
    this.error = null;

    const dto: CreateSubscriptionPlanDto = {
      ...this.basicInfoForm.value,
      // ✅ Calculate base price automatically from privileges
      basePrice: this.calculateFinalPrice(),
      // ✅ All discount fields matching backend DTO exactly
      discountPercentage: this.billingForm.value.discountPercentage,
      discountValidUntil: this.billingForm.value.discountValidUntil,
      billingDiscountPercentage: this.billingForm.value.billingDiscountPercentage,
      isAutoCalculatedPrice: true, // ✅ Always true - price is calculated from privileges
      adminCommissionPercent: this.billingForm.value.adminCommissionPercent,
      priceChangeNoticeDays: this.billingForm.value.priceChangeNoticeDays,
      // ✅ Tax fields
      defaultTaxPercentage: this.billingForm.value.defaultTaxPercentage,
      taxNotes: this.billingForm.value.taxNotes,
      privileges: this.selectedPrivileges
    };

    // ✅ NEW: Log DTO for debugging
    console.log('📤 Creating plan with DTO:', JSON.stringify(dto, null, 2));

    this.planService.createPlan(dto).subscribe({
      next: (response) => {
        this.creating = false;
        
        if (response.statusCode === 201 || response.statusCode === 200) {
          console.log('✅ Plan created successfully:', response.data);
          alert('Plan created successfully!');
          // Success - navigate to plan list
          this.router.navigate(['/webadmin/plans']);
        } else {
          this.error = response.message || 'Failed to create plan';
          alert(this.error);
          console.error('❌ API returned non-success:', response);
        }
      },
      error: (error) => {
        this.creating = false;
        console.error('❌ HTTP Error:', error);
        
        // ✅ NEW: Show detailed validation errors
        if (error.error?.errors) {
          const validationErrors = Object.entries(error.error.errors)
            .map(([key, value]) => `${key}: ${value}`)
            .join(', ');
          this.error = `Validation errors: ${validationErrors}`;
        } else {
          this.error = error.error?.message || error.message || 'An error occurred while creating the plan';
        }
        
        alert(this.error);
      }
    });
  }

  /**
   * Calculate cost for a single privilege
   * For unlimited privileges (-1), use the explicit base cost set by admin
   * No automatic multiplication or special logic - admin must set explicit cost
   */
  calculatePrivilegeCost(priv: PlanPrivilegeDto): number {
    const value = priv.value || 0;
    const baseCost = priv.privilegeBaseCost || 0;
    
    // For unlimited (-1), use the explicit base cost set by admin
    if (value === -1) {
      // ✅ Use explicit base cost - no automatic multiplication
      return baseCost;
    }
    
    return value * baseCost;
  }

  /**
   * Calculate total privilege cost (sum of all privilege costs)
   */
  calculateTotalPrivilegeCost(): number {
    return this.selectedPrivileges.reduce((total, priv) => {
      return total + this.calculatePrivilegeCost(priv);
    }, 0);
  }

  /**
   * Calculate admin commission
   */
  calculateCommission(): number {
    const privilegeCost = this.calculateTotalPrivilegeCost();
    const commissionPercent = this.billingForm.value.adminCommissionPercent || 0;
    return privilegeCost * (commissionPercent / 100);
  }

  /**
   * Calculate final plan price matching backend logic exactly
   * Backend: BasePrice → Promotional Discount → Billing Discount
   */
  calculateFinalPrice(): number {
    const privilegeCost = this.calculateTotalPrivilegeCost();
    const commission = this.calculateCommission();
    
    // Step 1: Calculate base price (privileges + commission)
    let price = privilegeCost + commission;
    
    // Step 2: Apply promotional discount if valid (matching backend logic)
    const promotionalDiscountPercent = this.billingForm.value.discountPercentage || 0;
    const discountValidUntil = this.billingForm.value.discountValidUntil;
    
    if (promotionalDiscountPercent > 0 && this.isPromotionalDiscountValid(discountValidUntil)) {
      const promotionalDiscountAmount = price * (promotionalDiscountPercent / 100);
      price = price * (1 - (promotionalDiscountPercent / 100));
    }
    
    // Step 3: Apply billing discount (matching backend logic)
    const billingDiscountPercent = this.billingForm.value.billingDiscountPercentage || 0;
    if (billingDiscountPercent > 0) {
      const billingDiscountAmount = price * (billingDiscountPercent / 100);
      price = price * (1 - (billingDiscountPercent / 100));
    }
    
    // Ensure price doesn't go negative (matching backend)
    return Math.max(price, 0);
  }

  /**
   * Calculate total price including tax
   */
  calculateTotalWithTax(): number {
    const basePrice = this.calculateFinalPrice();
    const taxPercent = this.billingForm.value.defaultTaxPercentage || 0;
    const taxAmount = basePrice * (taxPercent / 100);
    return basePrice + taxAmount;
  }

  /**
   * Check if promotional discount is valid (matching backend validation)
   */
  private isPromotionalDiscountValid(discountValidUntil: Date | null): boolean {
    if (!discountValidUntil) return true; // No expiration = always valid
    return new Date(discountValidUntil) >= new Date();
  }

  /**
   * Get detailed price breakdown matching backend calculation exactly
   */
  getPriceBreakdown(): any {
    const privilegeCost = this.calculateTotalPrivilegeCost();
    const commission = this.calculateCommission();
    const commissionPercent = this.billingForm.value.adminCommissionPercent || 10;
    
    // Step 1: Base price (privileges + commission)
    let price = privilegeCost + commission;
    
    // Step 2: Promotional discount calculation
    const promotionalDiscountPercent = this.billingForm.value.discountPercentage || 0;
    const discountValidUntil = this.billingForm.value.discountValidUntil;
    const isPromotionalValid = this.isPromotionalDiscountValid(discountValidUntil);
    
    let promotionalDiscountAmount = 0;
    let priceAfterPromotional = price;
    
    if (promotionalDiscountPercent > 0 && isPromotionalValid) {
      promotionalDiscountAmount = price * (promotionalDiscountPercent / 100);
      priceAfterPromotional = price * (1 - (promotionalDiscountPercent / 100));
    }
    
    // Step 3: Billing discount calculation
    const billingDiscountPercent = this.billingForm.value.billingDiscountPercentage || 0;
    let billingDiscountAmount = 0;
    let finalPrice = priceAfterPromotional;
    
    if (billingDiscountPercent > 0) {
      billingDiscountAmount = priceAfterPromotional * (billingDiscountPercent / 100);
      finalPrice = priceAfterPromotional * (1 - (billingDiscountPercent / 100));
    }
    
    // Ensure final price doesn't go negative
    finalPrice = Math.max(finalPrice, 0);

    return {
      // Base calculation
      privilegeCost: privilegeCost,
      commission: commission,
      commissionPercent: commissionPercent,
      basePrice: price,
      
      // Promotional discount
      promotionalDiscountPercent: promotionalDiscountPercent,
      promotionalDiscountAmount: promotionalDiscountAmount,
      isPromotionalValid: isPromotionalValid,
      priceAfterPromotional: priceAfterPromotional,
      
      // Billing discount
      billingDiscountPercent: billingDiscountPercent,
      billingDiscountAmount: billingDiscountAmount,
      
      // Final result
      finalPrice: finalPrice,
      privilegeDetails: this.getPrivilegeCostBreakdown()
    };
  }

  /**
   * Get detailed breakdown of each privilege cost
   */
  getPrivilegeCostBreakdown(): any[] {
    return this.selectedPrivileges.map(privilege => {
      const privilegeCost = this.calculatePrivilegeCost(privilege);
      return {
        privilegeName: this.getPrivilegeName(privilege.privilegeId),
        value: privilege.value,
        baseCost: privilege.privilegeBaseCost,
        totalCost: privilegeCost,
        isUnlimited: privilege.value === -1,
        isDisabled: privilege.value === 0
      };
    });
  }

  /**
   * Called when privilege values change - updates calculated price display
   */
  onPrivilegeValueChange(): void {
    // ✅ Price is always calculated automatically - no toggle needed
    console.log('💰 Price updated:', this.calculateFinalPrice());
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      formGroup.get(key)?.markAsTouched();
    });
  }

  /**
   * Get default value for privilege based on privilege type
   */
  getDefaultValueForPrivilege(privilege: PrivilegeDto): number {
    // Set sensible defaults based on privilege name/type
    const privilegeName = privilege.name?.toLowerCase() || '';
    
    if (privilegeName.includes('consultation') || privilegeName.includes('appointment')) {
      return 5; // 5 consultations per month
    } else if (privilegeName.includes('message') || privilegeName.includes('chat')) {
      return 50; // 50 messages per month
    } else if (privilegeName.includes('prescription') || privilegeName.includes('medication')) {
      return 3; // 3 prescriptions per month
    } else if (privilegeName.includes('video') || privilegeName.includes('call')) {
      return 10; // 10 video calls per month
    } else if (privilegeName.includes('unlimited')) {
      return -1; // Unlimited
    } else {
      return 10; // Default to 10 for other privileges
    }
  }

  /**
   * Get selected billing cycle name for display
   */
  getSelectedBillingCycleName(): string {
    const billingCycleId = this.basicInfoForm.value.billingCycleId;
    const selectedCycle = this.billingCycles.find(c => c.id === billingCycleId);
    return selectedCycle?.name || 'Unknown';
  }

  /**
   * Get help text for discount field based on selected billing cycle
   */
  getDiscountHelpText(): string {
    const cycleName = this.getSelectedBillingCycleName().toLowerCase();
    switch (cycleName) {
      case 'monthly':
        return 'Set discount percentage for monthly billing (0% = no discount)';
      case 'quarterly':
        return 'Set discount percentage for quarterly billing (e.g., 5% for quarterly plans)';
      case 'annual':
        return 'Set discount percentage for annual billing (e.g., 15% for annual plans)';
      default:
        return 'Set discount percentage for this billing cycle';
    }
  }

}

