import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { SubscriptionService, SubscriptionPlan, Category, BillingCycle } from '../../services/subscription.service';
import { 
  SubscriptionDto, 
  SubscriptionPlanDto, 
  CreateSubscriptionPlanDto, 
  PaginatedResponse,
  SubscriptionDetailsDto,
  BillingRecordDto,
  UserSubscriptionPrivilegeUsageDto
} from '../../models/subscription.models';

@Component({
  selector: 'app-enhanced-subscription-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatPaginatorModule,
    MatCardModule,
    MatTabsModule,
    MatChipsModule,
    MatMenuModule,
    MatSnackBarModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatCheckboxModule,
    MatTooltipModule,
    MatSidenavModule,
    MatExpansionModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './enhanced-subscription-management.component.html',
  styleUrls: ['./enhanced-subscription-management.component.scss']
})
export class EnhancedSubscriptionManagementComponent implements OnInit, OnDestroy {
  private subscriptionService = inject(SubscriptionService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);

  // View mode - determines what content to show
  viewMode: 'subscriptions' | 'plans' | 'analytics' = 'subscriptions';

  // Data properties
  subscriptions: SubscriptionDto[] = [];
  plans: SubscriptionPlanDto[] = [];
  categories: Category[] = [];
  billingCycles: BillingCycle[] = [];
  
  // Loading states
  subscriptionsLoading = false;
  plansLoading = false;
  categoriesLoading = false;
  billingCyclesLoading = false;

  // Pagination
  subscriptionsPageSize = 20;
  subscriptionsCurrentPage = 0;
  subscriptionsTotalCount = 0;
  
  plansPageSize = 20;
  plansCurrentPage = 0;
  plansTotalCount = 0;

  // Filters
  subscriptionFilters: any = {
    searchTerm: '',
    status: [],
    planId: [],
    userId: [],
    startDate: null,
    endDate: null,
    sortBy: 'createdDate',
    sortOrder: 'desc'
  };

  planFilters: any = {
    searchTerm: '',
    categoryId: '',
    isActive: null,
    sortBy: 'name',
    sortOrder: 'asc'
  };

  // Forms
  subscriptionFilterForm: FormGroup;
  planFilterForm: FormGroup;

  // Selection
  selectedSubscriptions: string[] = [];
  selectedPlans: string[] = [];

  // Subscriptions
  private subscriptions$: Subscription[] = [];

  // Table columns
  subscriptionColumns = [
    'select',
    'userName',
    'planName',
    'status',
    'startDate',
    'nextBillingDate',
    'amount',
    'actions'
  ];

  planColumns = [
    'select',
    'name',
    'category',
    'price',
    'billingCycle',
    'status',
    'subscribers',
    'actions'
  ];

  constructor() {
    this.subscriptionFilterForm = this.fb.group({
      searchTerm: [''],
      status: [[]],
      planId: [[]],
      userId: [[]],
      startDate: [null],
      endDate: [null],
      sortBy: ['createdDate'],
      sortOrder: ['desc']
    });

    this.planFilterForm = this.fb.group({
      searchTerm: [''],
      categoryId: [''],
      isActive: [null],
      sortBy: ['name'],
      sortOrder: ['asc']
    });
  }

  ngOnInit() {
    this.loadInitialData();
    this.setupFilterSubscriptions();
  }

  ngOnDestroy() {
    this.subscriptions$.forEach(sub => sub.unsubscribe());
  }

  private loadInitialData() {
    this.loadSubscriptions();
    this.loadPlans();
    this.loadCategories();
    this.loadBillingCycles();
  }

  private setupFilterSubscriptions() {
    // Subscribe to filter form changes
    this.subscriptions$.push(
      this.subscriptionFilterForm.valueChanges.subscribe(() => {
        this.subscriptionsCurrentPage = 0;
        this.loadSubscriptions();
      })
    );

    this.subscriptions$.push(
      this.planFilterForm.valueChanges.subscribe(() => {
        this.plansCurrentPage = 0;
        this.loadPlans();
      })
    );
  }

  // Data loading methods
  loadSubscriptions() {
    this.subscriptionsLoading = true;
    const filters = this.subscriptionFilterForm.value;
    
    const subscription = this.subscriptionService.getAllSubscriptions(
      this.subscriptionsCurrentPage + 1,
      this.subscriptionsPageSize,
      filters.searchTerm,
      filters.status,
      filters.planId,
      filters.userId,
      filters.startDate,
      filters.endDate,
      filters.sortBy,
      filters.sortOrder
    ).subscribe({
      next: (response: any) => {
        if (response.statusCode === 200) {
          this.subscriptions = response.data.items || [];
          this.subscriptionsTotalCount = response.data.totalCount || 0;
        }
        this.subscriptionsLoading = false;
      },
      error: (error) => {
        console.error('Error loading subscriptions:', error);
        this.snackBar.open('Failed to load subscriptions', 'Close', { duration: 3000 });
        this.subscriptionsLoading = false;
      }
    });

    this.subscriptions$.push(subscription);
  }

  loadPlans() {
    this.plansLoading = true;
    const filters = this.planFilterForm.value;
    
    const subscription = this.subscriptionService.getAllPlans(
      this.plansCurrentPage + 1,
      this.plansPageSize,
      filters.searchTerm,
      filters.categoryId,
      filters.isActive
    ).subscribe({
      next: (response: any) => {
        if (response.statusCode === 200) {
          this.plans = response.data.items || [];
          this.plansTotalCount = response.data.totalCount || 0;
        }
        this.plansLoading = false;
      },
      error: (error) => {
        console.error('Error loading plans:', error);
        this.snackBar.open('Failed to load plans', 'Close', { duration: 3000 });
        this.plansLoading = false;
      }
    });

    this.subscriptions$.push(subscription);
  }

  loadCategories() {
    this.categoriesLoading = true;
    const subscription = this.subscriptionService.getCategories().subscribe({
      next: (categories) => {
        this.categories = categories;
        this.categoriesLoading = false;
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.categoriesLoading = false;
      }
    });

    this.subscriptions$.push(subscription);
  }

  loadBillingCycles() {
    this.billingCyclesLoading = true;
    const subscription = this.subscriptionService.getBillingCycles().subscribe({
      next: (cycles) => {
        this.billingCycles = cycles;
        this.billingCyclesLoading = false;
      },
      error: (error) => {
        console.error('Error loading billing cycles:', error);
        this.billingCyclesLoading = false;
      }
    });

    this.subscriptions$.push(subscription);
  }

  // Pagination handlers
  onSubscriptionsPageChange(event: PageEvent) {
    this.subscriptionsCurrentPage = event.pageIndex;
    this.subscriptionsPageSize = event.pageSize;
    this.loadSubscriptions();
  }

  onPlansPageChange(event: PageEvent) {
    this.plansCurrentPage = event.pageIndex;
    this.plansPageSize = event.pageSize;
    this.loadPlans();
  }

  // Selection handlers
  onSubscriptionSelectionChange(subscriptionId: string, checked: boolean) {
    if (checked) {
      this.selectedSubscriptions.push(subscriptionId);
    } else {
      this.selectedSubscriptions = this.selectedSubscriptions.filter(id => id !== subscriptionId);
    }
  }

  onPlanSelectionChange(planId: string, checked: boolean) {
    if (checked) {
      this.selectedPlans.push(planId);
    } else {
      this.selectedPlans = this.selectedPlans.filter(id => id !== planId);
    }
  }

  selectAllSubscriptions(checked: boolean) {
    if (checked) {
      this.selectedSubscriptions = this.subscriptions.map(s => s.id);
    } else {
      this.selectedSubscriptions = [];
    }
  }

  selectAllPlans(checked: boolean) {
    if (checked) {
      this.selectedPlans = this.plans.map(p => p.id);
    } else {
      this.selectedPlans = [];
    }
  }

  // Action methods
  viewSubscriptionDetails(subscription: SubscriptionDto) {
    // Implement subscription details dialog
    console.log('View subscription details:', subscription);
  }

  editSubscription(subscription: SubscriptionDto) {
    // Implement subscription edit dialog
    console.log('Edit subscription:', subscription);
  }

  cancelSubscription(subscription: SubscriptionDto) {
    // Implement subscription cancellation
    console.log('Cancel subscription:', subscription);
  }

  pauseSubscription(subscription: SubscriptionDto) {
    // Implement subscription pause
    console.log('Pause subscription:', subscription);
  }

  resumeSubscription(subscription: SubscriptionDto) {
    // Implement subscription resume
    console.log('Resume subscription:', subscription);
  }

  viewPlanDetails(plan: SubscriptionPlanDto) {
    // Implement plan details dialog
    console.log('View plan details:', plan);
  }

  editPlan(plan: SubscriptionPlanDto) {
    // Implement plan edit dialog
    console.log('Edit plan:', plan);
  }

  deletePlan(plan: SubscriptionPlanDto) {
    // Implement plan deletion
    console.log('Delete plan:', plan);
  }

  activatePlan(plan: SubscriptionPlanDto) {
    // Implement plan activation
    console.log('Activate plan:', plan);
  }

  deactivatePlan(plan: SubscriptionPlanDto) {
    // Implement plan deactivation
    console.log('Deactivate plan:', plan);
  }

  // Bulk operations
  performBulkSubscriptionAction(action: string) {
    if (this.selectedSubscriptions.length === 0) {
      this.snackBar.open('Please select subscriptions first', 'Close', { duration: 3000 });
      return;
    }

    console.log(`Perform bulk action ${action} on subscriptions:`, this.selectedSubscriptions);
  }

  performBulkPlanAction(action: string) {
    if (this.selectedPlans.length === 0) {
      this.snackBar.open('Please select plans first', 'Close', { duration: 3000 });
      return;
    }

    console.log(`Perform bulk action ${action} on plans:`, this.selectedPlans);
  }

  // Export functionality
  exportSubscriptions(format: 'csv' | 'excel') {
    console.log(`Export subscriptions as ${format}`);
  }

  exportPlans(format: 'csv' | 'excel') {
    console.log(`Export plans as ${format}`);
  }

  // Utility methods
  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'active': return 'primary';
      case 'paused': return 'accent';
      case 'cancelled': return 'warn';
      case 'expired': return 'warn';
      default: return 'basic';
    }
  }

  getCategoryName(categoryId: string): string {
    const category = this.categories.find(c => c.id === categoryId);
    return category ? category.name : 'Unknown';
  }

  getBillingCycleName(billingCycleId: string): string {
    return this.subscriptionService.getBillingCycleName(billingCycleId);
  }

  formatPrice(price: number): string {
    return this.subscriptionService.formatPrice(price);
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString();
  }

  // Clear filters
  clearSubscriptionFilters() {
    this.subscriptionFilterForm.reset({
      searchTerm: '',
      status: [],
      planId: [],
      userId: [],
      startDate: null,
      endDate: null,
      sortBy: 'createdDate',
      sortOrder: 'desc'
    });
  }

  clearPlanFilters() {
    this.planFilterForm.reset({
      searchTerm: '',
      categoryId: '',
      isActive: null,
      sortBy: 'name',
      sortOrder: 'asc'
    });
  }
}
