import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

/**
 * Bulk Actions Component
 * Provides bulk operations for subscription management
 */
@Component({
  selector: 'app-bulk-actions',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './bulk-actions.component.html',
  styleUrls: ['./bulk-actions.component.scss']
})
export class BulkActionsComponent implements OnInit {
  @Input() selectedItems: any[] = [];
  @Input() totalItems: number = 0;
  @Input() isLoading: boolean = false;
  @Output() bulkAction = new EventEmitter<BulkActionRequest>();
  @Output() selectAll = new EventEmitter<boolean>();
  @Output() clearSelection = new EventEmitter<void>();

  isAllSelected: boolean = false;
  showConfirmation: boolean = false;
  selectedAction: BulkActionType | null = null;
  actionReason: string = '';

  availableActions: BulkAction[] = [
    {
      type: 'pause',
      label: 'Pause',
      icon: 'bi-pause-circle',
      description: 'Pause selected subscriptions',
      color: 'warning',
      requiresReason: false
    },
    {
      type: 'resume',
      label: 'Resume',
      icon: 'bi-play-circle',
      description: 'Resume paused subscriptions',
      color: 'success',
      requiresReason: false
    },
    {
      type: 'cancel',
      label: 'Cancel',
      icon: 'bi-x-circle',
      description: 'Cancel selected subscriptions',
      color: 'danger',
      requiresReason: true
    },
    {
      type: 'extend',
      label: 'Extend',
      icon: 'bi-calendar-plus',
      description: 'Extend subscription duration',
      color: 'info',
      requiresReason: false,
      requiresInput: true,
      inputLabel: 'Additional Days',
      inputType: 'number'
    }
  ];

  ngOnInit(): void {
    this.updateSelectionState();
  }

  ngOnChanges(): void {
    this.updateSelectionState();
  }

  onSelectAllChange(checked: boolean): void {
    this.isAllSelected = checked;
    this.selectAll.emit(checked);
  }

  onActionClick(action: BulkAction): void {
    if (this.selectedItems.length === 0) {
      return;
    }

    this.selectedAction = action.type;
    this.actionReason = '';
    
    if (action.requiresReason || action.requiresInput) {
      this.showConfirmation = true;
    } else {
      this.executeAction();
    }
  }

  onConfirmAction(): void {
    this.showConfirmation = false;
    this.executeAction();
  }

  onCancelAction(): void {
    this.showConfirmation = false;
    this.selectedAction = null;
    this.actionReason = '';
  }

  onClearSelection(): void {
    this.clearSelection.emit();
    this.isAllSelected = false;
  }

  private executeAction(): void {
    if (!this.selectedAction) return;

    const request: BulkActionRequest = {
      action: this.selectedAction,
      itemIds: this.selectedItems.map(item => item.id),
      reason: this.actionReason,
      additionalData: this.getAdditionalData()
    };

    this.bulkAction.emit(request);
    this.resetAction();
  }

  private getAdditionalData(): any {
    if (this.selectedAction === 'extend') {
      return {
        additionalDays: parseInt(this.actionReason) || 30
      };
    }
    return {};
  }

  private resetAction(): void {
    this.selectedAction = null;
    this.actionReason = '';
    this.showConfirmation = false;
  }

  private updateSelectionState(): void {
    this.isAllSelected = this.selectedItems.length > 0 && 
                        this.selectedItems.length === this.totalItems;
  }

  getSelectedCount(): number {
    return this.selectedItems.length;
  }

  getSelectionText(): string {
    const count = this.getSelectedCount();
    if (count === 0) return 'No items selected';
    if (count === this.totalItems) return `All ${count} items selected`;
    return `${count} of ${this.totalItems} items selected`;
  }

  getActionButtonClass(action: BulkAction): string {
    return `btn btn-outline-${action.color} btn-sm`;
  }

  isActionDisabled(action: BulkAction): boolean {
    if (this.selectedItems.length === 0) return true;
    if (this.isLoading) return true;

    // Check if action is applicable to selected items
    switch (action.type) {
      case 'pause':
        return !this.selectedItems.some(item => item.status === 'Active');
      case 'resume':
        return !this.selectedItems.some(item => item.status === 'Paused');
      case 'cancel':
        return !this.selectedItems.some(item => 
          ['Active', 'Paused', 'TrialActive'].includes(item.status)
        );
      default:
        return false;
    }
  }

  getConfirmationTitle(): string {
    const action = this.availableActions.find(a => a.type === this.selectedAction);
    return action ? `Confirm ${action.label}` : 'Confirm Action';
  }

  getConfirmationMessage(): string {
    const count = this.getSelectedCount();
    const action = this.availableActions.find(a => a.type === this.selectedAction);
    
    if (!action) return '';

    return `Are you sure you want to ${action.label.toLowerCase()} ${count} subscription${count > 1 ? 's' : ''}?`;
  }

  isReasonRequired(): boolean {
    const action = this.availableActions.find(a => a.type === this.selectedAction);
    return action?.requiresReason || false;
  }

  isInputRequired(): boolean {
    const action = this.availableActions.find(a => a.type === this.selectedAction);
    return action?.requiresInput || false;
  }

  getInputLabel(): string {
    const action = this.availableActions.find(a => a.type === this.selectedAction);
    return action?.inputLabel || '';
  }

  getInputType(): string {
    const action = this.availableActions.find(a => a.type === this.selectedAction);
    return action?.inputType || 'text';
  }

  canExecuteAction(): boolean {
    if (this.isReasonRequired() && !this.actionReason.trim()) return false;
    if (this.isInputRequired() && !this.actionReason.trim()) return false;
    return true;
  }
}

// Data Models
export interface BulkAction {
  type: BulkActionType;
  label: string;
  icon: string;
  description: string;
  color: 'primary' | 'secondary' | 'success' | 'danger' | 'warning' | 'info';
  requiresReason?: boolean;
  requiresInput?: boolean;
  inputLabel?: string;
  inputType?: string;
}

export type BulkActionType = 'pause' | 'resume' | 'cancel' | 'extend' | 'upgrade' | 'downgrade';

export interface BulkActionRequest {
  action: BulkActionType;
  itemIds: string[];
  reason?: string;
  additionalData?: any;
}
