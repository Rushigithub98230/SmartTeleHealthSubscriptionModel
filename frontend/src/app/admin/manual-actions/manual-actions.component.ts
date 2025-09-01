import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { CommonModule } from '@angular/common';
import { ManualActionsService } from './manual-actions.service';

@Component({
  selector: 'app-manual-actions',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule
  ],
  templateUrl: './manual-actions.component.html',
  styleUrls: ['./manual-actions.component.scss']
})
export class ManualActionsComponent {
  actionForm: FormGroup;
  loading = false;
  error: string | null = null;
  success: string | null = null;

  constructor(private fb: FormBuilder, private manualActionsService: ManualActionsService) {
    this.actionForm = this.fb.group({
      subscriptionId: ['', Validators.required],
      actionType: ['', Validators.required],
      reason: [''],
      newPlanId: [''],
      refundAmount: ['']
    });
  }

  onSubmit() {
    if (this.actionForm.invalid) return;
    this.loading = true;
    this.error = null;
    this.success = null;
    const { subscriptionId, actionType, reason, newPlanId, refundAmount } = this.actionForm.value;
    let action$;
    switch (actionType) {
      case 'pause':
        action$ = this.manualActionsService.pause(subscriptionId);
        break;
      case 'resume':
        action$ = this.manualActionsService.resume(subscriptionId);
        break;
      case 'cancel':
        action$ = this.manualActionsService.cancel(subscriptionId, reason);
        break;
      case 'upgrade':
        action$ = this.manualActionsService.upgrade(subscriptionId, newPlanId);
        break;
      case 'refund':
        action$ = this.manualActionsService.refund(subscriptionId, refundAmount, reason);
        break;
      default:
        this.error = 'Invalid action type';
        this.loading = false;
        return;
    }
    action$.subscribe({
      next: () => {
        this.success = 'Action completed successfully';
        this.loading = false;
      },
      error: (err: any) => {
        this.error = err?.error?.Message || 'Action failed';
        this.loading = false;
      }
    });
  }
}
