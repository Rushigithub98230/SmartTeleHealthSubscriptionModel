import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  SubscriptionPlanService,
  CategoryService,
  PrivilegeService,
  MasterDataService
} from '../../../../core/services';
import {
  SubscriptionPlanDto,
  UpdateSubscriptionPlanDto,
  PlanPrivilegeDto,
  CategoryDto,
  PrivilegeDto
} from '../../../../core/models';
import { BillingCycleDto, CurrencyDto } from '../../../../core/models/master-data.model';

/**
 * Admin Edit Plan Component - 4-Step Stepper Form
 * Similar to Create Plan but loads existing data and updates
 * 
 * APIs Used:
 * - GET /api/SubscriptionPlans/{id} (load existing plan)
 * - GET /api/Categories
 * - GET /api/Privileges
 * - PUT /api/SubscriptionPlans/{id} (update plan)
 * 
 * Route: /webadmin/plans/edit/:id
 * Access: Admin only
 */
@Component({
  selector: 'app-plan-edit',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './plan-edit.component.html',
  styleUrls: ['./plan-edit.component.scss']
})
export class PlanEditComponent implements OnInit {
  planId!: string;
  plan: SubscriptionPlanDto | null = null;
  currentStep = 1;
  totalSteps = 4;

  basicInfoForm!: FormGroup;
  billingForm!: FormGroup;

  categories: CategoryDto[] = [];
  availablePrivileges: PrivilegeDto[] = [];
  selectedPrivileges: any[] = [];
  billingCycles: BillingCycleDto[] = []; // ✅ Added missing property with proper type
  currencies: CurrencyDto[] = [];         // ✅ Added missing property with proper type

  loading = false;
  updating = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private planService: SubscriptionPlanService,
    private categoryService: CategoryService,
    private privilegeService: PrivilegeService,
    private masterDataService: MasterDataService
  ) {}

  ngOnInit(): void {
    this.planId = this.route.snapshot.params['id'];
    this.initForms();
    this.loadCategories();
    this.loadPrivileges();
    this.loadBillingCycles();
    this.loadCurrencies();
    this.loadPlan();
  }

  initForms(): void {
    this.basicInfoForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: [''],
      basePrice: [0, [Validators.required, Validators.min(0)]], // ✅ Use BasePrice to match backend
      categoryId: ['', Validators.required],
      billingCycleId: ['', Validators.required], // ✅ Added missing field
      currencyId: ['', Validators.required],     // ✅ Added missing field
      isActive: [true],
      isMostPopular: [false],
      isTrending: [false],
      displayOrder: [0]
    });

    this.billingForm = this.fb.group({
      discountPercentage: [0, [Validators.min(0), Validators.max(100)]],
      discountValidUntil: [null],
      billingDiscountPercentage: [0, [Validators.min(0), Validators.max(100)]],
      isAutoCalculatedPrice: [true],
      adminCommissionPercent: [10, [Validators.min(0), Validators.max(100)]],
      priceChangeNoticeDays: [10, [Validators.min(0)]]
    });
  }

  loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.categories = response.data;
        }
      }
    });
  }

  loadPrivileges(): void {
    this.privilegeService.getActivePrivileges().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.availablePrivileges = response.data;
        }
      }
    });
  }

  /**
   * Load billing cycles from backend dynamically
   * API: GET /api/MasterData/billing-cycles
   */
  loadBillingCycles(): void {
    this.masterDataService.getBillingCycles().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.billingCycles = response.data;
          console.log('✅ Loaded billing cycles for edit:', this.billingCycles);
        }
      },
      error: (error) => {
        console.error('❌ Error loading billing cycles for edit:', error);
        this.billingCycles = [];
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
          console.log('✅ Loaded currencies for edit:', this.currencies);
        }
      },
      error: (error) => {
        console.error('❌ Error loading currencies for edit:', error);
        this.currencies = [];
      }
    });
  }

  /**
   * Load existing plan data
   * API: GET /api/SubscriptionPlans/{id}
   */
  loadPlan(): void {
    this.loading = true;

    this.planService.getPlanById(this.planId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.plan = response.data;
          this.populateFormWithPlanData();
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message;
        this.loading = false;
      }
    });
  }

  /**
   * Populate forms with existing plan data
   */
  populateFormWithPlanData(): void {
    if (!this.plan) return;

    this.basicInfoForm.patchValue({
      name: this.plan.name,
      description: this.plan.description,
      basePrice: this.plan.basePrice, // ✅ Use basePrice to match backend
      categoryId: this.plan.categoryId,
      billingCycleId: this.plan.billingCycleId, // ✅ Added missing field
      currencyId: this.plan.currencyId,         // ✅ Added missing field
      isActive: this.plan.isActive,
      isMostPopular: this.plan.isMostPopular,
      isTrending: this.plan.isTrending,
      displayOrder: this.plan.displayOrder
    });

    this.billingForm.patchValue({
      discountPercentage: this.plan.discountPercentage || 0,
      discountValidUntil: this.plan.discountValidUntil,
      billingDiscountPercentage: this.plan.billingDiscountPercentage || 0,
      isAutoCalculatedPrice: this.plan.isAutoCalculatedPrice,
      adminCommissionPercent: this.plan.adminCommissionPercent || 10,
      priceChangeNoticeDays: this.plan.priceChangeNoticeDays || 10
    });

    // Load existing privileges
    if (this.plan.planPrivileges) {
      this.selectedPrivileges = this.plan.planPrivileges.map(pp => ({
        privilegeId: pp.privilegeId,
        value: pp.value,
        privilegeBaseCost: pp.privilegeBaseCost,
        unitCost: pp.unitCost,
        durationMonths: pp.durationMonths || 1,
        description: pp.description,
        effectiveDate: pp.effectiveDate,
        expirationDate: pp.expirationDate
      }));
    }
  }

  nextStep(): void {
    if (this.currentStep < this.totalSteps) {
      this.currentStep++;
    }
  }

  previousStep(): void {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  /**
   * Submit plan update
   * API: PUT /api/SubscriptionPlans/{id}
   */
  submitUpdate(): void {
    if (this.basicInfoForm.invalid || this.billingForm.invalid) {
      this.error = 'Please fill all required fields';
      this.markFormGroupTouched(this.basicInfoForm);
      this.markFormGroupTouched(this.billingForm);
      return;
    }

    this.updating = true;
    this.error = null;

    const dto: UpdateSubscriptionPlanDto = {
      id: this.planId,
      name: this.basicInfoForm.value.name,
      description: this.basicInfoForm.value.description,
      basePrice: this.basicInfoForm.value.basePrice, // ✅ Use basePrice to match backend
      categoryId: this.basicInfoForm.value.categoryId,
      billingCycleId: this.plan?.billingCycleId || '',
      currencyId: this.plan?.currencyId || '',
      isActive: this.basicInfoForm.value.isActive,
      isMostPopular: this.basicInfoForm.value.isMostPopular,
      isTrending: this.basicInfoForm.value.isTrending,
      displayOrder: this.basicInfoForm.value.displayOrder,
      isAutoCalculatedPrice: this.billingForm.value.isAutoCalculatedPrice,
      adminCommissionPercent: this.billingForm.value.adminCommissionPercent,
      priceChangeNoticeDays: this.billingForm.value.priceChangeNoticeDays,
      discountPercentage: this.billingForm.value.discountPercentage,
      discountValidUntil: this.billingForm.value.discountValidUntil,
      billingDiscountPercentage: this.billingForm.value.billingDiscountPercentage
    };

    this.planService.updatePlan(this.planId, dto).subscribe({
      next: (response) => {
        this.updating = false;
        if (response.statusCode === 200) {
          alert('Plan updated successfully!');
          this.router.navigate(['/webadmin/plans']);
        } else {
          this.error = response.message || 'Update failed';
          alert(this.error);
        }
      },
      error: (error) => {
        this.updating = false;
        console.error('Update error:', error);
        
        if (error.error?.errors) {
          const validationErrors = Object.entries(error.error.errors)
            .map(([key, value]) => `${key}: ${value}`)
            .join(', ');
          this.error = `Validation errors: ${validationErrors}`;
        } else {
          this.error = error.error?.message || error.message || 'An error occurred while updating the plan';
        }
        
        alert(this.error);
      }
    });
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      formGroup.get(key)?.markAsTouched();
    });
  }

  getCategoryName(categoryId: string): string {
    return this.categories.find(c => c.id === categoryId)?.name || 'Unknown';
  }

  getPrivilegeName(privilegeId: string): string {
    const priv = this.availablePrivileges.find(p => p.id === privilegeId);
    return priv?.name || 'Unknown Privilege';
  }

  getBillingCycleName(billingCycleId: string): string {
    const cycle = this.billingCycles.find(c => c.id === billingCycleId);
    return cycle?.name || 'Unknown Cycle';
  }

  addPrivilege(privilege: PrivilegeDto): void {
    this.selectedPrivileges.push({
      privilegeId: privilege.id,
      privilegeName: privilege.name,
      value: this.getDefaultValueForPrivilege(privilege), // ✅ Use intelligent defaults
      privilegeBaseCost: 0,  // ✅ Default to 0 - admin must set explicitly
      unitCost: 0,           // ✅ Default to 0 - admin must set explicitly
      durationMonths: 1
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

  removePrivilege(index: number): void {
    this.selectedPrivileges.splice(index, 1);
  }

  getAvailablePrivileges(): PrivilegeDto[] {
    const selectedIds = this.selectedPrivileges.map(p => p.privilegeId);
    return this.availablePrivileges.filter(p => !selectedIds.includes(p.id));
  }
}

