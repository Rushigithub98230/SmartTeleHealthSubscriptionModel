import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { CommonModule } from '@angular/common';
import { PlanService } from './plan.service';

@Component({
  selector: 'app-plan-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatTableModule,
    MatIconModule,
    MatChipsModule
  ],
  templateUrl: './plan-management.component.html',
  styleUrls: ['./plan-management.component.scss']
})
export class PlanManagementComponent implements OnInit {
  plans: any[] = [];
  loading = false;
  error: string | null = null;
  planForm: FormGroup;
  editingPlan: any = null;

  constructor(private planService: PlanService, private fb: FormBuilder) {
    this.planForm = this.fb.group({
      name: ['', Validators.required],
      price: ['', Validators.required],
      duration: ['', Validators.required],
      benefits: [''],
      tags: [''],
      isActive: [true]
    });
  }

  ngOnInit() {
    this.loadPlans();
  }

  loadPlans() {
    this.loading = true;
    this.planService.getPlans().subscribe({
      next: (res: any) => {
        this.plans = res?.data?.items || [];
        this.loading = false;
      },
      error: (err: any) => {
        this.error = err?.error?.Message || 'Failed to load plans';
        this.loading = false;
      }
    });
  }

  editPlan(plan: any) {
    this.editingPlan = plan;
    this.planForm.patchValue(plan);
  }

  deletePlan(planId: string) {
    if (!confirm('Delete this plan?')) return;
    this.planService.deletePlan(planId).subscribe({
      next: () => this.loadPlans(),
      error: (err: any) => this.error = err?.error?.Message || 'Delete failed'
    });
  }

  activatePlan(planId: string) {
    this.planService.activatePlan(planId).subscribe({
      next: () => this.loadPlans(),
      error: (err: any) => this.error = err?.error?.Message || 'Activate failed'
    });
  }

  deactivatePlan(planId: string) {
    this.planService.deactivatePlan(planId).subscribe({
      next: () => this.loadPlans(),
      error: (err: any) => this.error = err?.error?.Message || 'Deactivate failed'
    });
  }

  onSubmit() {
    if (this.planForm.invalid) return;
    const data = this.planForm.value;
    if (this.editingPlan) {
      this.planService.updatePlan(this.editingPlan.id, data).subscribe({
        next: () => {
          this.editingPlan = null;
          this.planForm.reset();
          this.loadPlans();
        },
        error: (err: any) => this.error = err?.error?.Message || 'Update failed'
      });
    } else {
      this.planService.createPlan(data).subscribe({
        next: () => {
          this.planForm.reset();
          this.loadPlans();
        },
        error: (err: any) => this.error = err?.error?.Message || 'Create failed'
      });
    }
  }

  cancelEdit() {
    this.editingPlan = null;
    this.planForm.reset();
  }
}
