import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  SubscriptionPlanService,
  SubscriptionService,
  PaymentService,
  AuthService,
  PrivilegeService,
  CommonService,
  MasterDataService
} from '../../../../core/services';
import {
  SubscriptionPlanDto,
  CreateSubscriptionDto,
  PaymentMethodDto,
  UserDto,
  PrivilegeDto
} from '../../../../core/models';
import { BillingCycleDto, CurrencyDto } from '../../../../core/models/master-data.model';

/**
 * Purchase Plan Component - 4-Step Checkout Stepper
 * 
 * APIs Used:
 * - GET /api/Billing/billing-cycles
 * - GET /api/SubscriptionPlans/{planId}
 * - GET /api/SubscriptionPlans/admin/privileges
 * - GET /api/Payment/methods
 * - POST /api/Subscriptions
 * 
 * Route: /web/subscriptions/purchase/:planId
 * Access: Authenticated users
 * 
 * Steps:
 * 1. Review Plan (with privileges, trial info)
 * 2. Select Billing Cycle (dynamic from backend)
 * 3. Payment Method
 * 4. Confirm & Purchase
 */
@Component({
  selector: 'app-purchase-plan',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './purchase-plan.component.html',
  styleUrls: ['./purchase-plan.component.scss']
})
export class PurchasePlanComponent implements OnInit {
  currentStep = 1;
  totalSteps = 4;
  
  planId!: string;
  plan: SubscriptionPlanDto | null = null;
  paymentMethods: PaymentMethodDto[] = [];
  currentUser: UserDto | null = null;
  availablePrivileges: PrivilegeDto[] = [];
  
  billingForm!: FormGroup;
  loading = false;
  purchasing = false;
  error: string | null = null;

  // Billing cycles - loaded from backend
  billingCycles: BillingCycleDto[] = [];
  loadingCycles = false;

  // Currencies - loaded from backend
  currencies: CurrencyDto[] = [];
  selectedCurrencyId: string = '';

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private planService: SubscriptionPlanService,
    private subscriptionService: SubscriptionService,
    private paymentService: PaymentService,
    private authService: AuthService,
    private privilegeService: PrivilegeService,
    private commonService: CommonService,
    private masterDataService: MasterDataService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.planId = this.route.snapshot.params['planId'];
    
    this.initForm();
    this.loadBillingCycles();  // ✅ Load from backend
    this.loadCurrencies();     // ✅ Load from backend
    this.loadPlan();
    this.loadPrivileges();
    if (this.currentUser) {
      this.loadPaymentMethods();
    }
  }

  initForm(): void {
    this.billingForm = this.fb.group({
      billingCycleId: ['', Validators.required],
      paymentMethodId: ['', Validators.required],
      autoRenew: [true]
    });
  }

  /**
   * Load billing cycles dynamically from backend
   * API: GET /api/MasterData/billing-cycles
   */
  loadBillingCycles(): void {
    this.loadingCycles = true;
    this.masterDataService.getBillingCycles().subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          this.billingCycles = response.data;
          
          // Set default to monthly (shortest duration)
          if (this.billingCycles.length > 0) {
            const monthly = this.billingCycles.find(bc => 
              bc.name?.toLowerCase().includes('month') && bc.durationInDays <= 31
            ) || this.billingCycles[0];
            
            if (monthly) {
              this.billingForm.patchValue({ billingCycleId: monthly.id });
            }
          }
          
          console.log('✅ Loaded billing cycles from backend:', this.billingCycles);
        }
        this.loadingCycles = false;
      },
      error: (error) => {
        console.error('❌ Error loading billing cycles:', error);
        this.loadingCycles = false;
        this.error = 'Failed to load billing options. Please refresh the page.';
      }
    });
  }

  /**
   * Load currencies dynamically from backend
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
            this.selectedCurrencyId = defaultCurrency.id;
          }
          
          console.log('✅ Loaded currencies from API:', this.currencies);
        }
      },
      error: (error) => {
        console.error('❌ Error loading currencies:', error);
        this.error = 'Failed to load currencies. Please refresh the page.';
      }
    });
  }

  /**
   * Load plan details
   */
  loadPlan(): void {
    this.loading = true;
    this.planService.getPlanById(this.planId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.plan = response.data;
          console.log('✅ Loaded plan:', this.plan);
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load plan details';
        this.loading = false;
      }
    });
  }

  /**
   * ✅ FIX #4: Load privileges for display
   * API: GET /api/SubscriptionPlans/admin/privileges
   */
  loadPrivileges(): void {
    this.privilegeService.getActivePrivileges().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.availablePrivileges = response.data;
          console.log('✅ Loaded privileges:', this.availablePrivileges);
        }
      },
      error: (error) => console.error('Error loading privileges:', error)
    });
  }

  /**
   * Load user's payment methods
   */
  loadPaymentMethods(): void {
    if (!this.currentUser) return;
    
    this.paymentService.getPaymentMethods(this.currentUser.id).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.paymentMethods = response.data;
          
          // Auto-select default payment method
          const defaultMethod = this.paymentMethods.find(pm => pm.isDefault);
          if (defaultMethod) {
            this.billingForm.patchValue({ paymentMethodId: defaultMethod.id });
          }
        }
      },
      error: (error) => console.error('Error loading payment methods:', error)
    });
  }

  /**
   * ✅ FIX #2: Calculate final price using PLAN'S discount fields
   * This matches backend calculation exactly
   */
  calculateFinalPrice(): number {
    if (!this.plan) return 0;
    
    const cycleId = this.billingForm.value.billingCycleId;
    const cycle = this.billingCycles.find(c => c.id === cycleId);
    
    if (!cycle) return this.plan.price;
    
    // Step 1: Calculate base price for billing cycle
    const monthlyPrice = this.plan.price;
    const monthsInCycle = cycle.durationInDays / 30;
    const basePrice = monthlyPrice * monthsInCycle;
    
    // Step 2: Get discount from PLAN (matches backend logic)
    let discountPercent = 0;
    
    const cycleName = cycle.name?.toLowerCase() || '';
    if (cycleName.includes('annual') || cycleName.includes('year')) {
      discountPercent = this.plan.annualBillingDiscount || 0;
    } else if (cycleName.includes('quarter')) {
      discountPercent = this.plan.quarterlyBillingDiscount || 0;
    } else if (cycleName.includes('month')) {
      discountPercent = this.plan.monthlyBillingDiscount || 0;
    }
    
    // Step 3: Apply discount (same formula as backend)
    const discount = basePrice * (discountPercent / 100);
    const finalPrice = basePrice - discount;
    
    console.log(`💰 Price calculation: $${monthlyPrice}/mo × ${monthsInCycle} months = $${basePrice}, discount ${discountPercent}% = $${finalPrice}`);
    
    return finalPrice;
  }

  /**
   * Get total price (same as final price for subscription model)
   */
  getTotalPrice(): number {
    return this.calculateFinalPrice();
  }

  /**
   * Get discount percent being applied
   */
  getDiscountPercent(): number {
    if (!this.plan) return 0;
    
    const cycleId = this.billingForm.value.billingCycleId;
    const cycle = this.billingCycles.find(c => c.id === cycleId);
    
    if (!cycle) return 0;
    
    const cycleName = cycle.name?.toLowerCase() || '';
    if (cycleName.includes('annual') || cycleName.includes('year')) {
      return this.plan.annualBillingDiscount || 0;
    } else if (cycleName.includes('quarter')) {
      return this.plan.quarterlyBillingDiscount || 0;
    } else if (cycleName.includes('month')) {
      return this.plan.monthlyBillingDiscount || 0;
    }
    
    return 0;
  }

  /**
   * Get base price (before discount)
   */
  getBasePrice(): number {
    if (!this.plan) return 0;
    
    const cycleId = this.billingForm.value.billingCycleId;
    const cycle = this.billingCycles.find(c => c.id === cycleId);
    
    if (!cycle) return this.plan.price;
    
    const monthlyPrice = this.plan.price;
    const monthsInCycle = cycle.durationInDays / 30;
    return monthlyPrice * monthsInCycle;
  }

  /**
   * Navigate to next step
   */
  nextStep(): void {
    if (this.currentStep === 2 && this.billingForm.get('billingCycleId')?.invalid) {
      this.error = 'Please select a billing cycle';
      return;
    }
    
    if (this.currentStep === 3 && this.billingForm.get('paymentMethodId')?.invalid) {
      this.error = 'Please select a payment method';
      return;
    }
    
    if (this.currentStep < this.totalSteps) {
      this.error = null;
      this.currentStep++;
    }
  }

  /**
   * Navigate to previous step
   */
  previousStep(): void {
    if (this.currentStep > 1) {
      this.error = null;
      this.currentStep--;
    }
  }

  /**
   * Submit purchase - creates subscription
   * API: POST /api/Subscriptions
   */
  submitPurchase(): void {
    if (this.billingForm.invalid || !this.currentUser || !this.plan) {
      this.error = 'Please complete all required fields';
      return;
    }

    this.purchasing = true;
    this.error = null;

    const dto: CreateSubscriptionDto = {
      userId: this.currentUser.id,
      planId: this.planId,
      price: this.plan.price,
      billingCycleId: this.billingForm.value.billingCycleId,  // ✅ Dynamic GUID from API
      currencyId: this.selectedCurrencyId,  // ✅ Dynamic GUID from API
      paymentMethodId: this.billingForm.value.paymentMethodId,
      autoRenew: this.billingForm.value.autoRenew,
      startImmediately: true,
      isActive: true
    };

    console.log('📤 Submitting subscription:', dto);

    this.subscriptionService.createSubscription(dto).subscribe({
      next: (response) => {
        this.purchasing = false;
        
        if (response.statusCode === 200 || response.statusCode === 201) {
          console.log('✅ Subscription created successfully:', response.data);
          // Success - redirect to subscriptions page
          this.router.navigate(['/web/subscriptions'], {
            queryParams: { success: 'true', newSubscription: 'true' }
          });
        } else {
          this.error = response.message || 'Purchase failed';
        }
      },
      error: (error) => {
        this.purchasing = false;
        this.error = error.message || 'An error occurred during purchase';
        console.error('❌ Purchase error:', error);
      }
    });
  }

  /**
   * Get selected billing cycle
   */
  getSelectedCycle(): BillingCycleDto | undefined {
    const cycleId = this.billingForm.value.billingCycleId;
    return this.billingCycles.find(c => c.id === cycleId);
  }

  /**
   * Get selected payment method
   */
  getSelectedPaymentMethod(): PaymentMethodDto | undefined {
    const methodId = this.billingForm.value.paymentMethodId;
    return this.paymentMethods.find(pm => pm.id === methodId);
  }

  /**
   * ✅ FIX #4: Get privilege name by ID
   */
  getPrivilegeName(privilegeId: string): string {
    const priv = this.availablePrivileges.find(p => p.id === privilegeId);
    return priv?.name || 'Privilege';
  }

  /**
   * ✅ FIX #5: Calculate trial end date
   */
  calculateTrialEndDate(): Date {
    if (!this.plan || !this.plan.isTrialAllowed) {
      return new Date();
    }
    const endDate = new Date();
    endDate.setDate(endDate.getDate() + this.plan.trialDurationInDays);
    return endDate;
  }

  /**
   * Check if plan has trial
   */
  hasTrialPeriod(): boolean {
    return !!(this.plan && this.plan.isTrialAllowed && this.plan.trialDurationInDays > 0);
  }

  /**
   * Get savings amount
   */
  getSavingsAmount(): number {
    const basePrice = this.getBasePrice();
    const finalPrice = this.calculateFinalPrice();
    return basePrice - finalPrice;
  }
}

