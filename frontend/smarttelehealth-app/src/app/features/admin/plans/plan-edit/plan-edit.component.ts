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
    this.loadPlan();
  }

  initForms(): void {
    this.basicInfoForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: [''],
      price: [0, [Validators.required, Validators.min(0.01)]],
      categoryId: ['', Validators.required],
      isActive: [true],
      isMostPopular: [false],
      isTrending: [false],
      displayOrder: [0]
    });

    this.billingForm = this.fb.group({
      monthlyBillingDiscount: [0],
      quarterlyBillingDiscount: [5],
      annualBillingDiscount: [15],
      isAutoCalculatedPrice: [true],
      adminCommissionPercent: [10],
      priceChangeNoticeDays: [10]
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
      price: this.plan.price,
      categoryId: this.plan.categoryId,
      isActive: this.plan.isActive,
      isMostPopular: this.plan.isMostPopular,
      isTrending: this.plan.isTrending,
      displayOrder: this.plan.displayOrder
    });

    this.billingForm.patchValue({
      monthlyBillingDiscount: this.plan.monthlyBillingDiscount || 0,
      quarterlyBillingDiscount: this.plan.quarterlyBillingDiscount || 5,
      annualBillingDiscount: this.plan.annualBillingDiscount || 15,
      isAutoCalculatedPrice: this.plan.isAutoCalculatedPrice,
      adminCommissionPercent: this.plan.adminCommissionPercent || 10,
      priceChangeNoticeDays: this.plan.priceChangeNoticeDays || 10
    });

    // Load existing privileges
    if (this.plan.planPrivileges) {
      this.selectedPrivileges = this.plan.planPrivileges.map(pp => ({
        privilegeId: pp.privilegeId,
        value: pp.value,
        monthlyLimit: pp.monthlyLimit || pp.value,
        dailyLimit: pp.dailyLimit,
        weeklyLimit: pp.weeklyLimit,
        privilegeBaseCost: pp.privilegeBaseCost,
        unitCost: pp.unitCost
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
      return;
    }

    this.updating = true;
    this.error = null;

    const dto: UpdateSubscriptionPlanDto = {
      id: this.planId,
      name: this.basicInfoForm.value.name,
      description: this.basicInfoForm.value.description,
      price: this.basicInfoForm.value.price,
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
      monthlyBillingDiscount: this.billingForm.value.monthlyBillingDiscount,
      quarterlyBillingDiscount: this.billingForm.value.quarterlyBillingDiscount,
      annualBillingDiscount: this.billingForm.value.annualBillingDiscount
    };

    this.planService.updatePlan(this.planId, dto).subscribe({
      next: (response) => {
        this.updating = false;
        if (response.statusCode === 200) {
          this.router.navigate(['/webadmin/plans']);
        } else {
          this.error = response.message;
        }
      },
      error: (error) => {
        this.updating = false;
        this.error = error.message;
      }
    });
  }

  getCategoryName(categoryId: string): string {
    return this.categories.find(c => c.id === categoryId)?.name || 'Unknown';
  }

  addPrivilege(privilege: PrivilegeDto): void {
    this.selectedPrivileges.push({
      privilegeId: privilege.id,
      privilegeName: privilege.name,
      value: 10,
      monthlyLimit: 10,
      privilegeBaseCost: 10,
      unitCost: 15
    });
  }

  removePrivilege(index: number): void {
    this.selectedPrivileges.splice(index, 1);
  }

  getAvailablePrivileges(): PrivilegeDto[] {
    const selectedIds = this.selectedPrivileges.map(p => p.privilegeId);
    return this.availablePrivileges.filter(p => !selectedIds.includes(p.id));
  }
}

