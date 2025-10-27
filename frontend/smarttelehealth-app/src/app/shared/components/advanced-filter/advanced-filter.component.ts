import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Subscription } from 'rxjs';
import { SubscriptionFilter, BillingFilter, FilterPreset, DEFAULT_SUBSCRIPTION_PRESETS, DEFAULT_BILLING_PRESETS } from '../../../core/models/filter.model';

/**
 * Advanced Filter Component
 * Reusable component for advanced filtering with inline expandable panel
 */
@Component({
  selector: 'app-advanced-filter',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './advanced-filter.component.html',
  styleUrls: ['./advanced-filter.component.scss']
})
export class AdvancedFilterComponent implements OnInit, OnDestroy {
  @Input() filterType: 'subscription' | 'billing' = 'subscription';
  @Input() presets: FilterPreset[] = [];
  @Input() isExpanded: boolean = false;
  @Output() filterChange = new EventEmitter<SubscriptionFilter | BillingFilter>();
  @Output() presetChange = new EventEmitter<string>();
  @Output() expandToggle = new EventEmitter<boolean>();

  filterForm!: FormGroup;
  availablePresets: FilterPreset[] = [];
  selectedPreset: string = 'all';
  private formSubscription?: Subscription;

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.initializePresets();
    this.loadCustomPresets();
    this.initializeForm();
    this.loadSavedFilter();
  }

  ngOnDestroy(): void {
    this.formSubscription?.unsubscribe();
  }

  private initializePresets(): void {
    if (this.presets.length > 0) {
      this.availablePresets = this.presets;
    } else {
      this.availablePresets = this.filterType === 'subscription' 
        ? DEFAULT_SUBSCRIPTION_PRESETS 
        : DEFAULT_BILLING_PRESETS;
    }
  }

  private initializeForm(): void {
    if (this.filterType === 'subscription') {
      this.filterForm = this.fb.group({
        searchTerm: [''],
        statuses: [[]],
        planIds: [[]],
        userIds: [[]],
        minAmount: [null],
        maxAmount: [null],
        createdDateFrom: [null],
        createdDateTo: [null],
        billingCycleIds: [[]],
        isTrial: [null],
        isPaused: [null],
        isCancelled: [null],
        isExpired: [null],
        hasFailedPayments: [null],
        hasActivePayments: [null],
        sortColumn: ['CreatedDate'],
        sortOrder: ['desc']
      });
    } else {
      this.filterForm = this.fb.group({
        searchTerm: [''],
        statuses: [[]],
        types: [[]],
        userIds: [[]],
        minAmount: [null],
        maxAmount: [null],
        createdDateFrom: [null],
        createdDateTo: [null],
        dueDateFrom: [null],
        dueDateTo: [null],
        isPaid: [null],
        isPending: [null],
        isFailed: [null],
        isOverdue: [null],
        isRefunded: [null],
        sortColumn: ['CreatedDate'],
        sortOrder: ['desc']
      });
    }

    // Subscribe to form changes
    this.formSubscription = this.filterForm.valueChanges.subscribe(() => {
      this.onFilterChange();
    });
  }

  private loadSavedFilter(): void {
    const savedFilter = this.getSavedFilter();
    if (savedFilter) {
      this.filterForm.patchValue(savedFilter);
    }
  }

  onPresetChange(presetId: string): void {
    this.selectedPreset = presetId;
    const preset = this.availablePresets.find(p => p.id === presetId);
    if (preset) {
      this.filterForm.patchValue(preset.filter);
      this.presetChange.emit(presetId);
    }
  }

  onFilterChange(): void {
    const filterValue = this.filterForm.value;
    this.saveFilter(filterValue);
    this.filterChange.emit(filterValue);
  }

  applyFilter(): void {
    this.onFilterChange();
  }

  resetFilter(): void {
    this.filterForm.reset();
    this.filterForm.patchValue({
      sortColumn: 'CreatedDate',
      sortOrder: 'desc'
    });
    this.selectedPreset = 'all';
    this.onFilterChange();
  }

  clearFilter(): void {
    this.resetFilter();
  }

  toggleExpansion(): void {
    this.isExpanded = !this.isExpanded;
    this.expandToggle.emit(this.isExpanded);
  }

  private saveFilter(filter: any): void {
    const key = `advanced_filter_${this.filterType}`;
    localStorage.setItem(key, JSON.stringify(filter));
  }

  private getSavedFilter(): any {
    const key = `advanced_filter_${this.filterType}`;
    const saved = localStorage.getItem(key);
    return saved ? JSON.parse(saved) : null;
  }

  // Helper methods for template
  getStatusOptions(): string[] {
    if (this.filterType === 'subscription') {
      return ['Active', 'TrialActive', 'Pending', 'Paused', 'Cancelled', 'Expired', 'PaymentFailed'];
    } else {
      return ['Paid', 'Pending', 'Failed', 'Overdue', 'Refunded', 'Cancelled'];
    }
  }

  getTypeOptions(): string[] {
    return ['Subscription', 'Overage', 'Consultation', 'Medication', 'LateFee'];
  }

  getBillingCycleOptions(): string[] {
    return ['Monthly', 'Quarterly', 'Annual'];
  }

  getSortColumnOptions(): string[] {
    if (this.filterType === 'subscription') {
      return ['CreatedDate', 'UpdatedDate', 'StartDate', 'EndDate', 'Amount', 'Status', 'PlanName'];
    } else {
      return ['CreatedDate', 'UpdatedDate', 'DueDate', 'PaidDate', 'Amount', 'Status', 'Type'];
    }
  }

  getSortOrderOptions(): string[] {
    return ['asc', 'desc'];
  }

  // Validation helpers
  isDateRangeValid(): boolean {
    const from = this.filterForm.get('createdDateFrom')?.value;
    const to = this.filterForm.get('createdDateTo')?.value;
    return !from || !to || new Date(from) <= new Date(to);
  }

  isAmountRangeValid(): boolean {
    const min = this.filterForm.get('minAmount')?.value;
    const max = this.filterForm.get('maxAmount')?.value;
    return !min || !max || min <= max;
  }

  getFilterSummary(): string {
    const formValue = this.filterForm.value;
    const activeFilters: string[] = [];

    if (formValue.searchTerm) activeFilters.push(`Search: "${formValue.searchTerm}"`);
    if (formValue.statuses?.length) activeFilters.push(`Status: ${formValue.statuses.join(', ')}`);
    if (formValue.minAmount || formValue.maxAmount) {
      const range = `${formValue.minAmount || 0} - ${formValue.maxAmount || '∞'}`;
      activeFilters.push(`Amount: ${range}`);
    }
    if (formValue.createdDateFrom || formValue.createdDateTo) {
      const from = formValue.createdDateFrom ? new Date(formValue.createdDateFrom).toLocaleDateString() : '∞';
      const to = formValue.createdDateTo ? new Date(formValue.createdDateTo).toLocaleDateString() : '∞';
      activeFilters.push(`Date: ${from} to ${to}`);
    }

    return activeFilters.length > 0 ? activeFilters.join(' | ') : 'No filters applied';
  }

  /**
   * Save current filter as a custom preset
   */
  saveAsPreset(presetName: string): void {
    const filterData = this.filterForm.value;
    const customPresets = this.getCustomPresets();
    
    const newPreset: FilterPreset = {
      id: `custom_${Date.now()}`,
      name: presetName,
      description: `Custom preset: ${presetName}`,
      filter: filterData,
      isDefault: false,
      createdAt: new Date()
    };
    
    customPresets.push(newPreset);
    localStorage.setItem(`custom_presets_${this.filterType}`, JSON.stringify(customPresets));
    
    // Update available presets
    this.availablePresets = [...this.availablePresets, newPreset];
  }

  /**
   * Delete a custom preset
   */
  deletePreset(presetId: string): void {
    const customPresets = this.getCustomPresets();
    const updatedPresets = customPresets.filter(p => p.id !== presetId);
    localStorage.setItem(`custom_presets_${this.filterType}`, JSON.stringify(updatedPresets));
    
    // Update available presets
    this.availablePresets = this.availablePresets.filter(p => p.id !== presetId);
    
    // If deleted preset was selected, reset to 'all'
    if (this.selectedPreset === presetId) {
      this.selectedPreset = 'all';
      this.applyPreset('all');
    }
  }

  /**
   * Get custom presets from localStorage
   */
  private getCustomPresets(): FilterPreset[] {
    const stored = localStorage.getItem(`custom_presets_${this.filterType}`);
    return stored ? JSON.parse(stored) : [];
  }

  /**
   * Load custom presets on initialization
   */
  private loadCustomPresets(): void {
    const customPresets = this.getCustomPresets();
    this.availablePresets = [...this.availablePresets, ...customPresets];
  }

  /**
   * Save current filter as preset with prompt
   */
  saveCurrentAsPreset(): void {
    const presetName = prompt('Enter preset name:');
    if (presetName && presetName.trim()) {
      this.saveAsPreset(presetName.trim());
    }
  }

  /**
   * Get preset by ID
   */
  getPresetById(presetId: string): FilterPreset | undefined {
    return this.availablePresets.find(p => p.id === presetId);
  }

  /**
   * Apply preset filter
   */
  applyPreset(presetId: string): void {
    const preset = this.getPresetById(presetId);
    if (preset && preset.filter) {
      this.filterForm.patchValue(preset.filter);
      this.selectedPreset = presetId;
      this.filterChange.emit(this.filterForm.value);
    }
  }
}
