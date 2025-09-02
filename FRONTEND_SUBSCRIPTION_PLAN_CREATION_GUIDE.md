# Frontend Developer Guide: Subscription Plan Creation Screen

## Overview
This guide provides comprehensive information for implementing a subscription plan creation screen in the admin panel with a stepper form. The screen should allow administrators to create new subscription plans with detailed configuration including pricing, billing cycles, privileges, and marketing settings.

## Table of Contents
1. [API Endpoints](#api-endpoints)
2. [Request/Response Schemas](#requestresponse-schemas)
3. [Stepper Form Structure](#stepper-form-structure)
4. [Frontend Implementation Guide](#frontend-implementation-guide)
5. [Error Handling](#error-handling)
6. [Validation Rules](#validation-rules)
7. [Sample Implementation](#sample-implementation)

---

## API Endpoints

### Base URL
```
http://localhost:58677/api
```
**Note**: The backend runs on port 58677 (HTTP) or 58676 (HTTPS), not 61376. The 61376 port is for IIS Express configuration only.

### 1. Master Data APIs (Required for Dropdowns)

#### Get Billing Cycles
```http
GET /api/MasterData/billing-cycles
```
**Response:**
```json
{
  "data": [
    {
      "id": "guid",
      "name": "Monthly",
      "description": "Monthly billing cycle",
      "durationInDays": 30,
      "sortOrder": 1,
      "isActive": true
    },
    {
      "id": "guid",
      "name": "Quarterly", 
      "description": "Quarterly billing cycle",
      "durationInDays": 90,
      "sortOrder": 2,
      "isActive": true
    },
    {
      "id": "guid",
      "name": "Annual",
      "description": "Annual billing cycle", 
      "durationInDays": 365,
      "sortOrder": 3,
      "isActive": true
    }
  ],
  "message": "Billing cycles retrieved successfully",
  "statusCode": 200
}
```

#### Get Currencies
```http
GET /api/MasterData/currencies
```
**Response:**
```json
{
  "data": [
    {
      "id": "guid",
      "code": "USD",
      "name": "US Dollar",
      "symbol": "$",
      "sortOrder": 1,
      "isActive": true
    },
    {
      "id": "guid", 
      "code": "EUR",
      "name": "Euro",
      "symbol": "€",
      "sortOrder": 2,
      "isActive": true
    }
  ],
  "message": "Currencies retrieved successfully",
  "statusCode": 200
}
```

#### Get Privilege Types
```http
GET /api/MasterData/privilege-types
```
**Response:**
```json
{
  "data": [
    {
      "id": "guid",
      "name": "Consultation",
      "description": "Video consultation privileges",
      "sortOrder": 1,
      "isActive": true
    },
    {
      "id": "guid",
      "name": "Messaging", 
      "description": "Chat messaging privileges",
      "sortOrder": 2,
      "isActive": true
    },
    {
      "id": "guid",
      "name": "Medication",
      "description": "Medication delivery privileges", 
      "sortOrder": 3,
      "isActive": true
    }
  ],
  "message": "Privilege types retrieved successfully",
  "statusCode": 200
}
```

#### Get All Privileges
```http
GET /api/Privileges?page=1&pageSize=100
```
**Response:**
```json
{
  "data": {
    "privileges": [
      {
        "id": "guid",
        "name": "Video Consultation",
        "description": "One-on-one video consultation with healthcare provider",
        "privilegeTypeId": "guid",
        "privilegeType": {
          "id": "guid",
          "name": "Consultation"
        },
        "isActive": true
      },
      {
        "id": "guid",
        "name": "Unlimited Messaging",
        "description": "Unlimited chat messages with healthcare providers",
        "privilegeTypeId": "guid", 
        "privilegeType": {
          "id": "guid",
          "name": "Messaging"
        },
        "isActive": true
      }
    ],
    "pagination": {
      "totalCount": 10,
      "page": 1,
      "pageSize": 100,
      "totalPages": 1
    }
  },
  "message": "Privileges retrieved successfully",
  "statusCode": 200
}
```

### 2. Subscription Plan Creation API

#### Create Subscription Plan (Standard)
```http
POST /api/SubscriptionPlans
```
**Alternative endpoint:**
```http
POST /webadmin/subscription-management/plans
```
**Request Body:**
```json
{
  "name": "Premium Plan",
  "description": "Premium healthcare subscription with unlimited consultations",
  "price": 99.99,
  "billingCycleId": "guid",
  "currencyId": "guid", 
  "messagingCount": 100,
  "includesMedicationDelivery": true,
  "includesFollowUpCare": true,
  "deliveryFrequencyDays": 30,
  "maxPauseDurationDays": 90,
  "isActive": true,
  "isMostPopular": false,
  "isTrending": false,
  "displayOrder": 1,
  "features": "Unlimited consultations, Priority support, Medication delivery"
}
```

**Response:**
```json
{
  "data": {
    "id": "guid",
    "name": "Premium Plan",
    "description": "Premium healthcare subscription with unlimited consultations",
    "price": 99.99,
    "billingCycleId": "guid",
    "currencyId": "guid",
    "isActive": true,
    "isMostPopular": false,
    "isTrending": false,
    "displayOrder": 1,
    "features": "Unlimited consultations, Priority support, Medication delivery",
    "CreatedDate": "2025-01-09T13:30:00Z",
    "UpdatedDate": "2025-01-09T13:30:00Z"
  },
  "message": "Subscription plan created successfully",
  "statusCode": 201
}
```

#### Create Subscription Plan with Time-Based Privilege Limits (Alternative)
```http
POST /api/admin/AdminSubscription/plans
```
**Note**: This endpoint is for creating plans with detailed privilege time limits. Use this when you need to configure specific daily/weekly/monthly limits for privileges.
**Request Body:**
```json
{
  "planName": "Premium Plan",
  "description": "Premium healthcare subscription",
  "price": 99.99,
  "billingCycle": "Monthly",
  "durationMonths": 1,
  "privileges": [
    {
      "privilegeName": "Video Consultation",
      "totalValue": 10,
      "dailyLimit": 2,
      "weeklyLimit": 5,
      "monthlyLimit": 10,
      "description": "Video consultations with healthcare providers"
    },
    {
      "privilegeName": "Unlimited Messaging",
      "totalValue": -1,
      "dailyLimit": null,
      "weeklyLimit": null,
      "monthlyLimit": null,
      "description": "Unlimited chat messaging"
    }
  ]
}
```

### 3. Additional CRUD Operations

#### Get All Subscription Plans (Admin)
```http
GET /api/SubscriptionPlans?page=1&pageSize=20&searchTerm=&isActive=true
```

#### Get Subscription Plan by ID
```http
GET /api/SubscriptionPlans/{id}
```

#### Update Subscription Plan
```http
PUT /api/SubscriptionPlans/{id}
```

#### Delete Subscription Plan
```http
DELETE /api/SubscriptionPlans/{id}
```

### 4. User Subscription Management APIs

#### Get All User Subscriptions (Admin)
```http
GET /api/admin/AdminSubscription?page=1&pageSize=20&status=&searchTerm=
```

#### Get User Subscription by ID
```http
GET /api/admin/AdminSubscription/{id}
```

#### Update User Subscription Status
```http
PUT /api/admin/AdminSubscription/{id}/status
```

#### Cancel User Subscription
```http
POST /api/admin/AdminSubscription/{id}/cancel
```

---

## Request/Response Schemas

### CreateSubscriptionPlanDto
```typescript
interface CreateSubscriptionPlanDto {
  name: string;                    // Required, max 100 chars
  description?: string;            // Optional, max 500 chars
  price: number;                   // Required, decimal
  billingCycleId: string;          // Required, GUID
  currencyId: string;              // Required, GUID
  messagingCount: number;          // Default: 10
  includesMedicationDelivery: boolean; // Default: true
  includesFollowUpCare: boolean;   // Default: true
  deliveryFrequencyDays: number;   // Default: 30
  maxPauseDurationDays: number;    // Default: 90
  isActive: boolean;               // Default: true
  isMostPopular: boolean;          // Default: false
  isTrending: boolean;             // Default: false
  displayOrder: number;            // Required
  features?: string;               // Optional, max 1000 chars
}
```

### CreateSubscriptionPlanWithTimeLimitsDto
```typescript
interface CreateSubscriptionPlanWithTimeLimitsDto {
  planName: string;                // Required, max 100 chars
  description: string;             // Required, max 500 chars
  price: number;                   // Required, decimal
  billingCycle: string;            // Required, "Monthly", "Quarterly", "Annual"
  durationMonths: number;          // Required, 1-120 months
  privileges: PrivilegeTimeLimitDto[];
}

interface PrivilegeTimeLimitDto {
  privilegeName: string;           // Required, max 100 chars
  totalValue: number;              // Required, -1 for unlimited, >0 for limited
  dailyLimit?: number;             // Optional, max per day
  weeklyLimit?: number;            // Optional, max per week  
  monthlyLimit?: number;           // Optional, max per month
  description?: string;            // Optional, max 500 chars
}
```

### Master Data Types
```typescript
interface BillingCycle {
  id: string;
  name: string;
  description: string;
  durationInDays: number;
  sortOrder: number;
  isActive: boolean;
}

interface Currency {
  id: string;
  code: string;
  name: string;
  symbol: string;
  sortOrder: number;
  isActive: boolean;
}

interface PrivilegeType {
  id: string;
  name: string;
  description: string;
  sortOrder: number;
  isActive: boolean;
}

interface Privilege {
  id: string;
  name: string;
  description: string;
  privilegeTypeId: string;
  privilegeType: PrivilegeType;
  isActive: boolean;
}
```

### API Response Format
```typescript
interface JsonModel {
  data: any;
  message: string;
  statusCode: number;
}
```

---

## Stepper Form Structure

### Step 1: Basic Information
- **Plan Name** (required, text input, max 100 chars)
- **Description** (optional, textarea, max 500 chars)
- **Features** (optional, textarea, max 1000 chars)
- **Display Order** (required, number input)

### Step 2: Pricing & Billing
- **Price** (required, decimal input, min 0)
- **Billing Cycle** (required, dropdown from API)
- **Currency** (required, dropdown from API)
- **Duration Months** (required, number input, min 1)

### Step 3: Service Configuration
- **Messaging Count** (number input, default 10)
- **Includes Medication Delivery** (checkbox, default true)
- **Includes Follow-up Care** (checkbox, default true)
- **Delivery Frequency Days** (number input, default 30)
- **Max Pause Duration Days** (number input, default 90)

### Step 4: Privilege Configuration
- **Privilege Selection** (multi-select from API)
- **For each selected privilege:**
  - **Total Value** (number input, -1 for unlimited, 0 for disabled, >0 for limited)
  - **Daily Limit** (optional number input)
  - **Weekly Limit** (optional number input)
  - **Monthly Limit** (optional number input)
  - **Description** (optional text input)

### Step 5: Marketing & Display
- **Is Active** (checkbox, default true)
- **Is Most Popular** (checkbox, default false)
- **Is Trending** (checkbox, default false)
- **Preview** (read-only display of plan configuration)

---

## Frontend Implementation Guide

### 1. Component Structure
```
src/app/admin/subscription-plans/
├── create-subscription-plan/
│   ├── create-subscription-plan.component.ts
│   ├── create-subscription-plan.component.html
│   ├── create-subscription-plan.component.scss
│   └── create-subscription-plan.component.spec.ts
├── models/
│   ├── subscription-plan.model.ts
│   ├── master-data.model.ts
│   └── privilege.model.ts
└── services/
    └── subscription-plan.service.ts
```

### 2. Service Implementation
```typescript
// subscription-plan.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SubscriptionPlanService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // Master Data APIs
  getBillingCycles(): Observable<JsonModel> {
    return this.http.get<JsonModel>(`${this.baseUrl}/api/MasterData/billing-cycles`);
  }

  getCurrencies(): Observable<JsonModel> {
    return this.http.get<JsonModel>(`${this.baseUrl}/api/MasterData/currencies`);
  }

  getPrivilegeTypes(): Observable<JsonModel> {
    return this.http.get<JsonModel>(`${this.baseUrl}/api/MasterData/privilege-types`);
  }

  getPrivileges(page = 1, pageSize = 100): Observable<JsonModel> {
    return this.http.get<JsonModel>(`${this.baseUrl}/api/Privileges?page=${page}&pageSize=${pageSize}`);
  }

  // Subscription Plan APIs
  createSubscriptionPlan(plan: CreateSubscriptionPlanDto): Observable<JsonModel> {
    return this.http.post<JsonModel>(`${this.baseUrl}/api/SubscriptionPlans`, plan);
  }

  createSubscriptionPlanAlternative(plan: CreateSubscriptionPlanDto): Observable<JsonModel> {
    return this.http.post<JsonModel>(`${this.baseUrl}/webadmin/subscription-management/plans`, plan);
  }

  createSubscriptionPlanWithTimeLimits(plan: CreateSubscriptionPlanWithTimeLimitsDto): Observable<JsonModel> {
    return this.http.post<JsonModel>(`${this.baseUrl}/api/admin/AdminSubscription/plans`, plan);
  }
}
```

### 3. Component Implementation
```typescript
// create-subscription-plan.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { SubscriptionPlanService } from '../services/subscription-plan.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-create-subscription-plan',
  templateUrl: './create-subscription-plan.component.html',
  styleUrls: ['./create-subscription-plan.component.scss']
})
export class CreateSubscriptionPlanComponent implements OnInit {
  stepperForm: FormGroup;
  currentStep = 0;
  totalSteps = 5;
  
  // Master data
  billingCycles: BillingCycle[] = [];
  currencies: Currency[] = [];
  privileges: Privilege[] = [];
  
  // Loading states
  loading = false;
  saving = false;

  constructor(
    private fb: FormBuilder,
    private subscriptionPlanService: SubscriptionPlanService,
    private router: Router
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    this.loadMasterData();
  }

  private initializeForm(): void {
    this.stepperForm = this.fb.group({
      // Step 1: Basic Information
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      features: ['', [Validators.maxLength(1000)]],
      displayOrder: [1, [Validators.required, Validators.min(1)]],

      // Step 2: Pricing & Billing
      price: [0, [Validators.required, Validators.min(0)]],
      billingCycleId: ['', Validators.required],
      currencyId: ['', Validators.required],
      durationMonths: [1, [Validators.required, Validators.min(1)]],

      // Step 3: Service Configuration
      messagingCount: [10, [Validators.min(0)]],
      includesMedicationDelivery: [true],
      includesFollowUpCare: [true],
      deliveryFrequencyDays: [30, [Validators.min(1)]],
      maxPauseDurationDays: [90, [Validators.min(1)]],

      // Step 4: Privilege Configuration
      privileges: this.fb.array([]),

      // Step 5: Marketing & Display
      isActive: [true],
      isMostPopular: [false],
      isTrending: [false]
    });
  }

  private loadMasterData(): void {
    this.loading = true;
    
    // Load all master data in parallel
    forkJoin({
      billingCycles: this.subscriptionPlanService.getBillingCycles(),
      currencies: this.subscriptionPlanService.getCurrencies(),
      privileges: this.subscriptionPlanService.getPrivileges()
    }).subscribe({
      next: (data) => {
        this.billingCycles = data.billingCycles.data;
        this.currencies = data.currencies.data;
        this.privileges = data.privileges.data.privileges;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading master data:', error);
        this.loading = false;
        // Handle error - show toast/notification
      }
    });
  }

  // Stepper navigation
  nextStep(): void {
    if (this.isCurrentStepValid()) {
      this.currentStep++;
    }
  }

  previousStep(): void {
    this.currentStep--;
  }

  goToStep(step: number): void {
    if (step >= 0 && step < this.totalSteps) {
      this.currentStep = step;
    }
  }

  private isCurrentStepValid(): boolean {
    const currentStepGroup = this.getCurrentStepGroup();
    return currentStepGroup ? currentStepGroup.valid : false;
  }

  private getCurrentStepGroup(): FormGroup | null {
    switch (this.currentStep) {
      case 0: return this.stepperForm;
      case 1: return this.stepperForm;
      case 2: return this.stepperForm;
      case 3: return this.stepperForm;
      case 4: return this.stepperForm;
      default: return null;
    }
  }

  // Privilege management
  get privilegeFormArray(): FormArray {
    return this.stepperForm.get('privileges') as FormArray;
  }

  addPrivilege(privilege: Privilege): void {
    const privilegeGroup = this.fb.group({
      privilegeId: [privilege.id],
      privilegeName: [privilege.name],
      totalValue: [0, [Validators.required]],
      dailyLimit: [null],
      weeklyLimit: [null],
      monthlyLimit: [null],
      description: [privilege.description]
    });
    
    this.privilegeFormArray.push(privilegeGroup);
  }

  removePrivilege(index: number): void {
    this.privilegeFormArray.removeAt(index);
  }

  // Form submission
  onSubmit(): void {
    if (this.stepperForm.valid) {
      this.saving = true;
      
      const formValue = this.stepperForm.value;
      
      // Choose which API to use based on whether privileges are configured
      if (this.privilegeFormArray.length > 0) {
        this.createPlanWithTimeLimits(formValue);
      } else {
        this.createStandardPlan(formValue);
      }
    } else {
      this.markFormGroupTouched(this.stepperForm);
    }
  }

  private createStandardPlan(formValue: any): void {
    const planData: CreateSubscriptionPlanDto = {
      name: formValue.name,
      description: formValue.description,
      price: formValue.price,
      billingCycleId: formValue.billingCycleId,
      currencyId: formValue.currencyId,
      messagingCount: formValue.messagingCount,
      includesMedicationDelivery: formValue.includesMedicationDelivery,
      includesFollowUpCare: formValue.includesFollowUpCare,
      deliveryFrequencyDays: formValue.deliveryFrequencyDays,
      maxPauseDurationDays: formValue.maxPauseDurationDays,
      isActive: formValue.isActive,
      isMostPopular: formValue.isMostPopular,
      isTrending: formValue.isTrending,
      displayOrder: formValue.displayOrder,
      features: formValue.features
    };

    this.subscriptionPlanService.createSubscriptionPlan(planData).subscribe({
      next: (response) => {
        this.saving = false;
        // Handle success - show toast and navigate
        this.router.navigate(['/admin/subscription-plans']);
      },
      error: (error) => {
        this.saving = false;
        // Handle error - show error message
        console.error('Error creating subscription plan:', error);
      }
    });
  }

  private createPlanWithTimeLimits(formValue: any): void {
    const planData: CreateSubscriptionPlanWithTimeLimitsDto = {
      planName: formValue.name,
      description: formValue.description,
      price: formValue.price,
      billingCycle: this.getBillingCycleName(formValue.billingCycleId),
      durationMonths: formValue.durationMonths,
      privileges: formValue.privileges.map((p: any) => ({
        privilegeName: p.privilegeName,
        totalValue: p.totalValue,
        dailyLimit: p.dailyLimit,
        weeklyLimit: p.weeklyLimit,
        monthlyLimit: p.monthlyLimit,
        description: p.description
      }))
    };

    this.subscriptionPlanService.createSubscriptionPlanWithTimeLimits(planData).subscribe({
      next: (response) => {
        this.saving = false;
        // Handle success - show toast and navigate
        this.router.navigate(['/admin/subscription-plans']);
      },
      error: (error) => {
        this.saving = false;
        // Handle error - show error message
        console.error('Error creating subscription plan:', error);
      }
    });
  }

  private getBillingCycleName(billingCycleId: string): string {
    const cycle = this.billingCycles.find(c => c.id === billingCycleId);
    return cycle ? cycle.name : '';
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
      
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  // Getters for template
  get isFirstStep(): boolean {
    return this.currentStep === 0;
  }

  get isLastStep(): boolean {
    return this.currentStep === this.totalSteps - 1;
  }

  get canProceed(): boolean {
    return this.isCurrentStepValid() && !this.isLastStep;
  }

  get canSubmit(): boolean {
    return this.stepperForm.valid && this.isLastStep;
  }
}
```

### 4. Template Implementation
```html
<!-- create-subscription-plan.component.html -->
<div class="subscription-plan-creation">
  <div class="header">
    <h1>Create Subscription Plan</h1>
    <p>Configure a new subscription plan for your telehealth platform</p>
  </div>

  <!-- Stepper Progress -->
  <mat-stepper #stepper [linear]="true" class="stepper">
    
    <!-- Step 1: Basic Information -->
    <mat-step [stepControl]="stepperForm" label="Basic Information">
      <div class="step-content">
        <h3>Plan Details</h3>
        
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Plan Name</mat-label>
          <input matInput formControlName="name" placeholder="Enter plan name">
          <mat-error *ngIf="stepperForm.get('name')?.hasError('required')">
            Plan name is required
          </mat-error>
          <mat-error *ngIf="stepperForm.get('name')?.hasError('maxlength')">
            Plan name must be less than 100 characters
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea matInput formControlName="description" rows="3" 
                    placeholder="Enter plan description"></textarea>
          <mat-error *ngIf="stepperForm.get('description')?.hasError('maxlength')">
            Description must be less than 500 characters
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Features</mat-label>
          <textarea matInput formControlName="features" rows="4" 
                    placeholder="List key features (comma-separated)"></textarea>
          <mat-error *ngIf="stepperForm.get('features')?.hasError('maxlength')">
            Features must be less than 1000 characters
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Display Order</mat-label>
          <input matInput type="number" formControlName="displayOrder" min="1">
          <mat-error *ngIf="stepperForm.get('displayOrder')?.hasError('required')">
            Display order is required
          </mat-error>
        </mat-form-field>
      </div>

      <div class="step-actions">
        <button mat-raised-button color="primary" (click)="stepper.next()" 
                [disabled]="!stepperForm.get('name')?.valid || !stepperForm.get('displayOrder')?.valid">
          Next
        </button>
      </div>
    </mat-step>

    <!-- Step 2: Pricing & Billing -->
    <mat-step [stepControl]="stepperForm" label="Pricing & Billing">
      <div class="step-content">
        <h3>Pricing Configuration</h3>
        
        <div class="pricing-row">
          <mat-form-field appearance="outline">
            <mat-label>Price</mat-label>
            <input matInput type="number" formControlName="price" min="0" step="0.01">
            <span matPrefix>$&nbsp;</span>
            <mat-error *ngIf="stepperForm.get('price')?.hasError('required')">
              Price is required
            </mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Currency</mat-label>
            <mat-select formControlName="currencyId">
              <mat-option *ngFor="let currency of currencies" [value]="currency.id">
                {{currency.symbol}} {{currency.name}} ({{currency.code}})
              </mat-option>
            </mat-select>
            <mat-error *ngIf="stepperForm.get('currencyId')?.hasError('required')">
              Currency is required
            </mat-error>
          </mat-form-field>
        </div>

        <div class="billing-row">
          <mat-form-field appearance="outline">
            <mat-label>Billing Cycle</mat-label>
            <mat-select formControlName="billingCycleId">
              <mat-option *ngFor="let cycle of billingCycles" [value]="cycle.id">
                {{cycle.name}} ({{cycle.durationInDays}} days)
              </mat-option>
            </mat-select>
            <mat-error *ngIf="stepperForm.get('billingCycleId')?.hasError('required')">
              Billing cycle is required
            </mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Duration (Months)</mat-label>
            <input matInput type="number" formControlName="durationMonths" min="1">
            <mat-error *ngIf="stepperForm.get('durationMonths')?.hasError('required')">
              Duration is required
            </mat-error>
          </mat-form-field>
        </div>
      </div>

      <div class="step-actions">
        <button mat-button (click)="stepper.previous()">Previous</button>
        <button mat-raised-button color="primary" (click)="stepper.next()" 
                [disabled]="!stepperForm.get('price')?.valid || !stepperForm.get('billingCycleId')?.valid || !stepperForm.get('currencyId')?.valid">
          Next
        </button>
      </div>
    </mat-step>

    <!-- Step 3: Service Configuration -->
    <mat-step [stepControl]="stepperForm" label="Service Configuration">
      <div class="step-content">
        <h3>Service Settings</h3>
        
        <mat-form-field appearance="outline">
          <mat-label>Messaging Count</mat-label>
          <input matInput type="number" formControlName="messagingCount" min="0">
          <mat-hint>Number of messages included in this plan</mat-hint>
        </mat-form-field>

        <div class="checkbox-group">
          <mat-checkbox formControlName="includesMedicationDelivery">
            Includes Medication Delivery
          </mat-checkbox>
          
          <mat-checkbox formControlName="includesFollowUpCare">
            Includes Follow-up Care
          </mat-checkbox>
        </div>

        <div class="service-settings-row">
          <mat-form-field appearance="outline">
            <mat-label>Delivery Frequency (Days)</mat-label>
            <input matInput type="number" formControlName="deliveryFrequencyDays" min="1">
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Max Pause Duration (Days)</mat-label>
            <input matInput type="number" formControlName="maxPauseDurationDays" min="1">
          </mat-form-field>
        </div>
      </div>

      <div class="step-actions">
        <button mat-button (click)="stepper.previous()">Previous</button>
        <button mat-raised-button color="primary" (click)="stepper.next()">
          Next
        </button>
      </div>
    </mat-step>

    <!-- Step 4: Privilege Configuration -->
    <mat-step [stepControl]="stepperForm" label="Privilege Configuration">
      <div class="step-content">
        <h3>Configure Privileges</h3>
        
        <div class="privilege-selection">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Select Privileges</mat-label>
            <mat-select multiple (selectionChange)="onPrivilegeSelectionChange($event)">
              <mat-option *ngFor="let privilege of privileges" [value]="privilege">
                {{privilege.name}} - {{privilege.description}}
              </mat-option>
            </mat-select>
          </mat-form-field>
        </div>

        <div class="privilege-configuration" formArrayName="privileges">
          <div *ngFor="let privilegeGroup of privilegeFormArray.controls; let i = index" 
               [formGroupName]="i" class="privilege-item">
            <mat-card>
              <mat-card-header>
                <mat-card-title>{{privilegeGroup.get('privilegeName')?.value}}</mat-card-title>
                <mat-card-subtitle>{{privilegeGroup.get('description')?.value}}</mat-card-subtitle>
              </mat-card-header>
              
              <mat-card-content>
                <div class="privilege-limits">
                  <mat-form-field appearance="outline">
                    <mat-label>Total Value</mat-label>
                    <input matInput type="number" formControlName="totalValue" 
                           placeholder="-1 for unlimited, 0 for disabled, >0 for limited">
                    <mat-hint>-1 = Unlimited, 0 = Disabled, >0 = Limited</mat-hint>
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>Daily Limit</mat-label>
                    <input matInput type="number" formControlName="dailyLimit" min="0">
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>Weekly Limit</mat-label>
                    <input matInput type="number" formControlName="weeklyLimit" min="0">
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>Monthly Limit</mat-label>
                    <input matInput type="number" formControlName="monthlyLimit" min="0">
                  </mat-form-field>
                </div>
              </mat-card-content>
              
              <mat-card-actions>
                <button mat-button color="warn" (click)="removePrivilege(i)">
                  Remove
                </button>
              </mat-card-actions>
            </mat-card>
          </div>
        </div>
      </div>

      <div class="step-actions">
        <button mat-button (click)="stepper.previous()">Previous</button>
        <button mat-raised-button color="primary" (click)="stepper.next()">
          Next
        </button>
      </div>
    </mat-step>

    <!-- Step 5: Marketing & Display -->
    <mat-step [stepControl]="stepperForm" label="Marketing & Display">
      <div class="step-content">
        <h3>Marketing Settings</h3>
        
        <div class="marketing-checkboxes">
          <mat-checkbox formControlName="isActive">
            Active Plan
          </mat-checkbox>
          
          <mat-checkbox formControlName="isMostPopular">
            Mark as Most Popular
          </mat-checkbox>
          
          <mat-checkbox formControlName="isTrending">
            Mark as Trending
          </mat-checkbox>
        </div>

        <!-- Plan Preview -->
        <div class="plan-preview">
          <h4>Plan Preview</h4>
          <mat-card class="preview-card">
            <mat-card-header>
              <mat-card-title>{{stepperForm.get('name')?.value || 'Plan Name'}}</mat-card-title>
              <mat-card-subtitle>{{stepperForm.get('description')?.value || 'Plan Description'}}</mat-card-subtitle>
            </mat-card-header>
            
            <mat-card-content>
              <div class="preview-details">
                <p><strong>Price:</strong> ${{stepperForm.get('price')?.value || '0'}} 
                   {{getCurrencySymbol()}} / {{getBillingCycleName()}}</p>
                <p><strong>Features:</strong> {{stepperForm.get('features')?.value || 'No features specified'}}</p>
                <p><strong>Privileges:</strong> {{privilegeFormArray.length}} configured</p>
                <p><strong>Status:</strong> {{stepperForm.get('isActive')?.value ? 'Active' : 'Inactive'}}</p>
              </div>
            </mat-card-content>
          </mat-card>
        </div>
      </div>

      <div class="step-actions">
        <button mat-button (click)="stepper.previous()">Previous</button>
        <button mat-raised-button color="primary" (click)="onSubmit()" 
                [disabled]="!stepperForm.valid || saving">
          <mat-spinner *ngIf="saving" diameter="20"></mat-spinner>
          {{saving ? 'Creating...' : 'Create Plan'}}
        </button>
      </div>
    </mat-step>
  </mat-stepper>
</div>
```

### 5. Styling (SCSS)
```scss
// create-subscription-plan.component.scss
.subscription-plan-creation {
  max-width: 1200px;
  margin: 0 auto;
  padding: 24px;

  .header {
    text-align: center;
    margin-bottom: 32px;
    
    h1 {
      color: #1976d2;
      margin-bottom: 8px;
    }
    
    p {
      color: #666;
      font-size: 16px;
    }
  }

  .stepper {
    background: white;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    padding: 24px;
  }

  .step-content {
    padding: 24px 0;
    
    h3 {
      color: #333;
      margin-bottom: 24px;
      border-bottom: 2px solid #e0e0e0;
      padding-bottom: 8px;
    }
  }

  .full-width {
    width: 100%;
  }

  .pricing-row,
  .billing-row,
  .service-settings-row {
    display: flex;
    gap: 16px;
    margin-bottom: 16px;
    
    mat-form-field {
      flex: 1;
    }
  }

  .checkbox-group {
    display: flex;
    flex-direction: column;
    gap: 16px;
    margin: 24px 0;
  }

  .privilege-selection {
    margin-bottom: 24px;
  }

  .privilege-configuration {
    .privilege-item {
      margin-bottom: 16px;
      
      .privilege-limits {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
        gap: 16px;
        margin-top: 16px;
      }
    }
  }

  .marketing-checkboxes {
    display: flex;
    flex-direction: column;
    gap: 16px;
    margin-bottom: 32px;
  }

  .plan-preview {
    margin-top: 32px;
    
    h4 {
      color: #333;
      margin-bottom: 16px;
    }
    
    .preview-card {
      border: 2px solid #e0e0e0;
      
      .preview-details {
        p {
          margin: 8px 0;
          color: #666;
        }
      }
    }
  }

  .step-actions {
    display: flex;
    justify-content: space-between;
    margin-top: 32px;
    padding-top: 24px;
    border-top: 1px solid #e0e0e0;
  }
}

// Responsive design
@media (max-width: 768px) {
  .subscription-plan-creation {
    padding: 16px;
    
    .pricing-row,
    .billing-row,
    .service-settings-row {
      flex-direction: column;
    }
    
    .privilege-limits {
      grid-template-columns: 1fr !important;
    }
  }
}
```

---

## Error Handling

### 1. API Error Responses
```typescript
interface ApiError {
  message: string;
  statusCode: number;
  errors?: { [key: string]: string[] };
}
```

### 2. Error Handling in Component
```typescript
private handleApiError(error: any): void {
  let errorMessage = 'An unexpected error occurred';
  
  if (error.error?.message) {
    errorMessage = error.error.message;
  } else if (error.message) {
    errorMessage = error.message;
  }
  
  // Show error toast/notification
  this.showErrorToast(errorMessage);
  
  // Log error for debugging
  console.error('API Error:', error);
}

private showErrorToast(message: string): void {
  // Implement your toast/notification service
  // Example: this.toastService.showError(message);
}
```

### 3. Validation Error Handling
```typescript
getFieldError(fieldName: string): string {
  const field = this.stepperForm.get(fieldName);
  if (field?.errors && field.touched) {
    if (field.errors['required']) {
      return `${fieldName} is required`;
    }
    if (field.errors['maxlength']) {
      return `${fieldName} is too long`;
    }
    if (field.errors['min']) {
      return `${fieldName} must be greater than ${field.errors['min'].min}`;
    }
  }
  return '';
}
```

---

## Validation Rules

### 1. Form Validation
- **Plan Name**: Required, max 100 characters
- **Description**: Optional, max 500 characters
- **Price**: Required, minimum 0
- **Billing Cycle**: Required selection
- **Currency**: Required selection
- **Display Order**: Required, minimum 1
- **Features**: Optional, max 1000 characters

### 2. Business Rules
- **Privilege Values**: 
  - -1 = Unlimited
  - 0 = Disabled
  - >0 = Limited usage
- **Time Limits**: Optional but must be positive integers
- **Duration**: Must be at least 1 month

### 3. Custom Validators
```typescript
// Custom validator for privilege configuration
function privilegeValidator(control: AbstractControl): ValidationErrors | null {
  const totalValue = control.get('totalValue')?.value;
  const dailyLimit = control.get('dailyLimit')?.value;
  const weeklyLimit = control.get('weeklyLimit')?.value;
  const monthlyLimit = control.get('monthlyLimit')?.value;
  
  if (totalValue === 0 && (dailyLimit || weeklyLimit || monthlyLimit)) {
    return { invalidPrivilegeConfig: 'Cannot set time limits for disabled privilege' };
  }
  
  return null;
}
```

---

## Sample Implementation

### 1. Routing Configuration
```typescript
// app-routing.module.ts
const routes: Routes = [
  {
    path: 'admin/subscription-plans/create',
    component: CreateSubscriptionPlanComponent,
    canActivate: [AuthGuard, AdminGuard]
  }
];
```

### 2. Module Configuration
```typescript
// subscription-plans.module.ts
@NgModule({
  declarations: [
    CreateSubscriptionPlanComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatStepperModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatCheckboxModule,
    MatCardModule,
    MatProgressSpinnerModule,
    SubscriptionPlansRoutingModule
  ],
  providers: [
    SubscriptionPlanService
  ]
})
export class SubscriptionPlansModule { }
```

### 3. Environment Configuration
```typescript
// environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:58677'
};
```

---

## Testing Considerations

### 1. Unit Tests
- Test form validation
- Test stepper navigation
- Test API service calls
- Test error handling

### 2. Integration Tests
- Test complete form submission flow
- Test master data loading
- Test privilege configuration

### 3. E2E Tests
- Test complete user journey
- Test form validation messages
- Test success/error scenarios

---

## Additional Notes

### 1. Authentication
- Ensure admin authentication is properly configured
- Include JWT token in API requests
- Handle token expiration gracefully

### 2. Loading States
- Show loading indicators during API calls
- Disable form controls during submission
- Provide clear feedback to users

### 3. Accessibility
- Use proper ARIA labels
- Ensure keyboard navigation works
- Provide screen reader support

### 4. Performance
- Lazy load master data
- Implement form caching
- Optimize API calls

---

## Complete Implementation Flow

### 1. Subscription Plan CRUD Screens

#### A. Subscription Plan List Screen
- **Purpose**: Display all subscription plans with pagination, search, and filtering
- **API Endpoint**: `GET /api/SubscriptionPlans`
- **Features**:
  - Paginated table with plans
  - Search by name/description
  - Filter by active/inactive status
  - Actions: View, Edit, Delete, Activate/Deactivate
  - Create new plan button

#### B. Subscription Plan Creation Screen (Stepper Form)
- **Purpose**: Create new subscription plans with comprehensive configuration
- **API Endpoint**: `POST /api/SubscriptionPlans`
- **Features**:
  - 5-step stepper form
  - Real-time validation
  - Master data integration
  - Preview functionality
  - Error handling

#### C. Subscription Plan Edit Screen
- **Purpose**: Edit existing subscription plans
- **API Endpoint**: `PUT /api/SubscriptionPlans/{id}`
- **Features**:
  - Pre-populated form with existing data
  - Same validation as creation
  - Update functionality

### 2. User Subscription Management Screens

#### A. User Subscriptions List Screen
- **Purpose**: Manage all user subscriptions
- **API Endpoint**: `GET /api/admin/AdminSubscription`
- **Features**:
  - Paginated table with user subscriptions
  - Search by user name/email
  - Filter by status (active, paused, cancelled, expired)
  - Actions: View details, Update status, Cancel, Pause/Resume

#### B. User Subscription Details Screen
- **Purpose**: View and manage individual user subscription
- **API Endpoint**: `GET /api/admin/AdminSubscription/{id}`
- **Features**:
  - Complete subscription details
  - Usage statistics
  - Payment history
  - Status management
  - Communication logs

### 3. Implementation Checklist

#### Frontend Components Required:
- [ ] SubscriptionPlanListComponent
- [ ] SubscriptionPlanCreateComponent (Stepper)
- [ ] SubscriptionPlanEditComponent
- [ ] UserSubscriptionListComponent
- [ ] UserSubscriptionDetailsComponent
- [ ] Shared components: SearchBar, PaginationTable, StatusBadge

#### Services Required:
- [ ] SubscriptionPlanService
- [ ] UserSubscriptionService
- [ ] MasterDataService
- [ ] NotificationService (for toasts/alerts)

#### Models/Interfaces Required:
- [ ] SubscriptionPlanDto
- [ ] CreateSubscriptionPlanDto
- [ ] UpdateSubscriptionPlanDto
- [ ] UserSubscriptionDto
- [ ] MasterData interfaces (BillingCycle, Currency, Privilege)

#### Routing Configuration:
- [ ] `/admin/subscription-plans` - List view
- [ ] `/admin/subscription-plans/create` - Create form
- [ ] `/admin/subscription-plans/edit/:id` - Edit form
- [ ] `/admin/user-subscriptions` - User subscriptions list
- [ ] `/admin/user-subscriptions/:id` - User subscription details

### 4. Key Implementation Notes

#### Authentication & Authorization:
- All admin endpoints require authentication
- Include JWT token in request headers
- Handle token expiration gracefully

#### Error Handling:
- Implement global error interceptor
- Show user-friendly error messages
- Log errors for debugging

#### Loading States:
- Show loading indicators during API calls
- Disable form controls during submission
- Provide clear feedback to users

#### Validation:
- Client-side validation for immediate feedback
- Server-side validation error handling
- Custom validators for business rules

#### Performance:
- Implement pagination for large datasets
- Use OnPush change detection strategy
- Lazy load components where possible

This comprehensive guide should provide your frontend developer with all the necessary information to implement a robust subscription plan creation screen with a stepper form. The implementation follows Angular best practices and includes proper error handling, validation, and user experience considerations.
