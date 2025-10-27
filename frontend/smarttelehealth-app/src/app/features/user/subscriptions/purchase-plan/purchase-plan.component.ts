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
import { StripeCheckoutService } from '../../../../core/services/stripe-checkout.service';
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
  imports: [CommonModule, ReactiveFormsModule],
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

  // CRITICAL FIX: Centralized pricing from backend
  effectivePrice: number | null = null;
  loadingPrice = false;

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
    private masterDataService: MasterDataService,
    private stripeCheckoutService: StripeCheckoutService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.planId = this.route.snapshot.params['planId'];
    
    console.log('🎯 [PURCHASE-PLAN] Component initialized');
    console.log('👤 [PURCHASE-PLAN] Current user:', {
      id: this.currentUser?.id,
      email: this.currentUser?.email,
      name: this.currentUser?.fullName
    });
    console.log('📋 [PURCHASE-PLAN] Plan ID:', this.planId);
    
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
      // billingCycleId removed - comes from selected plan (fixed)
      paymentMethodId: [''], // Remove required validator - will be handled conditionally
      autoRenew: [true]
    });
  }

  /**
   * Load billing cycles dynamically from backend (for display purposes only)
   * API: GET /api/MasterData/billing-cycles
   * 
   * NOTE: Billing cycle is FIXED in the selected plan
   * This method only loads cycles to display the name/duration to the user
   */
  loadBillingCycles(): void {
    this.loadingCycles = true;
    this.masterDataService.getBillingCycles().subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          this.billingCycles = response.data;
          console.log('✅ Loaded billing cycles for display:', this.billingCycles);
        }
        this.loadingCycles = false;
      },
      error: (error) => {
        console.error('❌ Error loading billing cycles:', error);
        this.loadingCycles = false;
        // Non-critical error - can still purchase
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
   * Load plan details and effective price
   */
  loadPlan(): void {
    console.log('📋 [PURCHASE-PLAN] Loading plan details for plan:', this.planId);
    this.loading = true;
    this.loadingPrice = true;
    
    // Load plan details
    this.planService.getPlanById(this.planId).subscribe({
      next: (response) => {
        console.log('✅ [PURCHASE-PLAN] Plan loaded:', {
          statusCode: response.statusCode,
          planName: response.data?.name,
          planPrice: response.data?.basePrice || response.data?.price,
          billingCycle: response.data?.billingCycleName
        });
        
        if (response.statusCode === 200) {
          this.plan = response.data;
          console.log('✅ [PURCHASE-PLAN] Plan details:', this.plan);
          
          // CRITICAL FIX: Load effective price from backend
          this.loadEffectivePrice();
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('❌ [PURCHASE-PLAN] Error loading plan:', error);
        this.error = 'Failed to load plan details';
        this.loading = false;
        this.loadingPrice = false;
      }
    });
  }

  /**
   * CRITICAL FIX: Load effective price from centralized backend API
   */
  loadEffectivePrice(): void {
    this.planService.getEffectivePrice(this.planId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.effectivePrice = response.data.EffectivePrice;
          console.log('✅ Loaded effective price:', this.effectivePrice);
        }
        this.loadingPrice = false;
      },
      error: (error) => {
        console.error('Failed to load effective price:', error);
        this.loadingPrice = false;
        // Don't set error - fallback to local calculation
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
    if (!this.currentUser) {
      console.log('⚠️ [PURCHASE-PLAN] No current user - skipping payment methods load');
      return;
    }
    
    console.log('💳 [PURCHASE-PLAN] Loading payment methods for user:', this.currentUser.id);
    
    this.paymentService.getPaymentMethods(this.currentUser.id).subscribe({
      next: (response) => {
        console.log('✅ [PURCHASE-PLAN] Payment methods loaded:', {
          statusCode: response.statusCode,
          methodCount: response.data?.length || 0,
          methods: response.data
        });
        
        if (response.statusCode === 200) {
          this.paymentMethods = response.data;
          
          // Auto-select default payment method if available
          const defaultMethod = this.paymentMethods.find(pm => pm.isDefault);
          if (defaultMethod) {
            this.billingForm.patchValue({ paymentMethodId: defaultMethod.id });
            console.log('🎯 [PURCHASE-PLAN] Auto-selected default payment method:', defaultMethod.id);
          }
          
          console.log('✅ [PURCHASE-PLAN] Loaded payment methods:', this.paymentMethods.length);
        }
      },
      error: (error) => {
        console.error('❌ [PURCHASE-PLAN] Error loading payment methods:', error);
        // Don't set error - user can still proceed with Stripe checkout
      }
    });
  }

  /**
   * CRITICAL FIX: Calculate final price using centralized backend API
   * This ensures frontend and backend use identical pricing calculations
   */
  calculateFinalPrice(): number {
    if (!this.plan) return 0;
    
    // Use the effective price from the backend API if available
    if (this.effectivePrice !== null) {
      return this.effectivePrice;
    }
    
    // Fallback to local calculation if API hasn't been called yet
    let basePrice = this.plan.basePrice || this.plan.price || 0;
    
    // Apply promotional discount if available and valid
    if (this.plan.discountedPrice && this.plan.discountValidUntil) {
      const now = new Date();
      const validUntil = new Date(this.plan.discountValidUntil);
      if (now <= validUntil) {
        basePrice = this.plan.discountedPrice;
      }
    }
    
    // Apply billing discount if set
    const billingDiscount = this.plan.billingDiscountPercentage || this.plan.billingDiscount;
    if (billingDiscount && billingDiscount > 0) {
      const discountAmount = basePrice * (billingDiscount / 100);
      basePrice = basePrice - discountAmount;
    }
    
    return Math.max(basePrice, 0); // Ensure price doesn't go negative
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
    
    // NEW ARCHITECTURE: Each plan has a single billing discount for its specific billing cycle
    // The plan is already tied to a specific billing cycle, so use its billingDiscount
    return this.plan.billingDiscountPercentage || this.plan.billingDiscount || 0;
  }

  /**
   * Get base price
   */
  getBasePrice(): number {
    if (!this.plan) return 0;
    return this.plan.basePrice || this.plan.price || 0;  // Plan price is the base price
  }

  /**
   * Navigate to next step
   */
  nextStep(): void {
    // For step 2 (payment method selection), handle different scenarios
    if (this.currentStep === 2) {
      // If user has no payment methods, redirect to Stripe checkout
      if (this.paymentMethods.length === 0) {
        console.log('🛒 [PURCHASE-PLAN] No payment methods - redirecting to Stripe checkout');
        this.submitPurchaseWithStripeCheckout();
        return;
      }
      
      // If user has payment methods but hasn't selected one, show error
      const selectedPaymentMethod = this.billingForm.get('paymentMethodId')?.value;
      if (!selectedPaymentMethod || selectedPaymentMethod.trim() === '') {
        this.error = 'Please select a payment method';
        return;
      }
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
   * Submit purchase using Stripe Checkout (Recommended)
   * More secure and PCI compliant
   */
  submitPurchaseWithStripeCheckout(): void {
    console.log('🛒 [PURCHASE-PLAN] Stripe checkout initiated');
    console.log('👤 [PURCHASE-PLAN] User:', {
      id: this.currentUser?.id,
      email: this.currentUser?.email
    });
    console.log('📋 [PURCHASE-PLAN] Plan:', {
      id: this.planId,
      name: this.plan?.name,
      price: this.calculateFinalPrice()
    });
    
    if (!this.currentUser || !this.plan) {
      this.error = 'Please complete all required fields';
      console.error('❌ [PURCHASE-PLAN] Missing required data:', {
        hasUser: !!this.currentUser,
        hasPlan: !!this.plan
      });
      return;
    }

    this.purchasing = true;
    this.error = null;

    const request = {
      planId: this.planId,
      successUrl: `${window.location.origin}/web/subscriptions/success?session_id={CHECKOUT_SESSION_ID}`,
      cancelUrl: `${window.location.origin}/web/subscriptions/purchase/${this.planId}?cancelled=true`
    };

    console.log('🔗 [PURCHASE-PLAN] Creating checkout session with request:', request);

    this.stripeCheckoutService.createCheckoutSession(request).subscribe({
      next: (response) => {
        console.log('✅ [PURCHASE-PLAN] Checkout session created:', {
          statusCode: response.statusCode,
          hasUrl: !!response.data?.url
        });
        
        this.purchasing = false;
        
        if (response.statusCode === 200 && response.data?.url) {
          console.log('🚀 [PURCHASE-PLAN] Redirecting to Stripe checkout');
          // Redirect to Stripe checkout
          this.stripeCheckoutService.redirectToCheckout(response.data.url);
        } else {
          this.error = response.message || 'Failed to create checkout session';
          console.error('❌ [PURCHASE-PLAN] Checkout session creation failed:', response.message);
        }
      },
      error: (error) => {
        console.error('❌ [PURCHASE-PLAN] Checkout session error:', error);
        this.purchasing = false;
        this.error = error.message || 'Failed to create checkout session';
      }
    });
  }

  /**
   * Submit purchase - creates subscription (Direct API method)
   * API: POST /api/Subscriptions
   * Note: This method is less secure than Stripe Checkout
   */
  submitPurchase(): void {
    if (this.billingForm.invalid || !this.currentUser || !this.plan) {
      this.error = 'Please complete all required fields';
      return;
    }

    // Validate payment method before submission
    const paymentMethodId = this.billingForm.value.paymentMethodId;
    if (!paymentMethodId) {
      this.error = 'Please select a payment method';
      return;
    }

    // Check if payment method exists in user's payment methods
    const selectedPaymentMethod = this.paymentMethods.find(pm => pm.id === paymentMethodId);
    if (!selectedPaymentMethod) {
      this.error = 'Selected payment method is not valid';
      return;
    }

    this.purchasing = true;
    this.error = null;

    const dto: CreateSubscriptionDto = {
      userId: this.currentUser.id,
      planId: this.planId,
      price: this.plan.basePrice || this.plan.price || 0,
      // REMOVED: billingCycleId - comes from plan (fixed billing cycle)
      currencyId: this.plan.currencyId,  // ✅ From plan
      paymentMethodId: this.billingForm.value.paymentMethodId,
      autoRenew: this.billingForm.value.autoRenew,
      startImmediately: true,
      isActive: true
    };

    console.log('📤 Submitting subscription:', dto);
    console.log('✅ Using plan\'s fixed billing cycle:', this.plan.billingCycleId);

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
          // Handle specific error cases
          if (response.statusCode === 400) {
            this.error = response.message || 'Invalid request. Please check your information and try again.';
          } else if (response.statusCode === 404) {
            this.error = 'The selected plan is no longer available. Please choose a different plan.';
          } else if (response.statusCode === 500) {
            this.error = 'A server error occurred. Please try again later or contact support.';
          } else {
            this.error = response.message || 'Purchase failed. Please try again.';
          }
        }
      },
      error: (error) => {
        this.purchasing = false;
        
        // Handle network and other errors
        if (error.status === 0) {
          this.error = 'Network error. Please check your connection and try again.';
        } else if (error.status === 401) {
          this.error = 'Your session has expired. Please log in again.';
          // Redirect to login
          this.router.navigate(['/auth/login']);
        } else if (error.status === 403) {
          this.error = 'You do not have permission to perform this action.';
        } else if (error.status >= 500) {
          this.error = 'Server error. Please try again later or contact support.';
        } else {
          this.error = error.message || 'An unexpected error occurred during purchase.';
        }
        
        console.error('❌ Purchase error:', error);
      }
    });
  }

  /**
   * Get plan's billing cycle (fixed) - now uses plan's embedded billing cycle data
   */
  getSelectedCycle(): BillingCycleDto | undefined {
    if (!this.plan) return undefined;
    
    // Use embedded billing cycle data from plan if available
    if (this.plan.billingCycleName) {
      return {
        id: this.plan.billingCycleId,
        name: this.plan.billingCycleName,
        description: this.plan.billingCycleDescription,
        durationInDays: this.plan.billingCycleDurationInDays,
        isActive: true,
        displayOrder: 1
      };
    }
    
    // Fallback to lookup from loaded billing cycles
    return this.billingCycles.find(c => c.id === this.plan!.billingCycleId);
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
    return Math.max(basePrice - finalPrice, 0); // Ensure savings don't go negative
  }
}

