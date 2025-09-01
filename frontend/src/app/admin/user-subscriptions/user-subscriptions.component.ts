import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { CommonModule } from '@angular/common';
import { UserSubscriptionsService } from './user-subscriptions.service';

@Component({
  selector: 'app-user-subscriptions',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatCheckboxModule,
    MatTableModule,
    MatIconModule,
    MatChipsModule
  ],
  templateUrl: './user-subscriptions.component.html',
  styleUrls: ['./user-subscriptions.component.scss']
})
export class UserSubscriptionsComponent implements OnInit {
  subscriptions: any[] = [];
  loading = false;
  error: string | null = null;
  filterForm: FormGroup;

  constructor(private userSubscriptionsService: UserSubscriptionsService, private fb: FormBuilder) {
    this.filterForm = this.fb.group({
      searchTerm: [''],
      status: [''],
      planId: [''],
      userId: [''],
      startDate: [''],
      endDate: [''],
      sortBy: [''],
      sortOrder: ['']
    });
  }

  ngOnInit() {
    this.loadSubscriptions();
  }

  loadSubscriptions() {
    this.loading = true;
    const filters = this.filterForm.value;
    this.userSubscriptionsService.getSubscriptions(filters).subscribe({
      next: (res: any) => {
        this.subscriptions = res?.data?.items || [];
        this.loading = false;
      },
      error: (err: any) => {
        this.error = err?.error?.Message || 'Failed to load subscriptions';
        this.loading = false;
      }
    });
  }

  onFilterChange() {
    this.loadSubscriptions();
  }

  viewDetails(subscription: any) {
    // Show details modal or route to details page
  }

  pause(subscriptionId: string) {
    this.userSubscriptionsService.pauseSubscription(subscriptionId).subscribe({
      next: () => this.loadSubscriptions(),
      error: (err: any) => this.error = err?.error?.Message || 'Pause failed'
    });
  }

  resume(subscriptionId: string) {
    this.userSubscriptionsService.resumeSubscription(subscriptionId).subscribe({
      next: () => this.loadSubscriptions(),
      error: (err: any) => this.error = err?.error?.Message || 'Resume failed'
    });
  }

  cancel(subscriptionId: string) {
    if (!confirm('Cancel this subscription?')) return;
    this.userSubscriptionsService.cancelSubscription(subscriptionId).subscribe({
      next: () => this.loadSubscriptions(),
      error: (err: any) => this.error = err?.error?.Message || 'Cancel failed'
    });
  }
}
