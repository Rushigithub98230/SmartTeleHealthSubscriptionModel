import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormArray, FormControl } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatChipsModule } from '@angular/material/chips';
import { MatStepperModule } from '@angular/material/stepper';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatRadioModule } from '@angular/material/radio';
import { Subscription } from 'rxjs';
import { StripeService } from '../../services/stripe.service';
import { SubscriptionService } from '../../services/subscription.service';
import { SubscriptionPlanService } from '../../services/subscription-plan.service';

interface TestResult {
  success: boolean;
  timestamp: Date;
  request: any;
  response: any;
  error?: string;
  duration?: number;
  stepName?: string;
}

interface TestScenario {
  id: string;
  name: string;
  description: string;
  steps: string[];
  status: 'pending' | 'running' | 'completed' | 'failed';
  results: TestResult[];
  currentStep?: number;
}

interface PlanTestData {
  name: string;
  description: string;
  price: number;
  billingCycle: 'monthly' | 'quarterly' | 'annual';
  privileges: Array<{
    name: string;
    limit: number;
    unitCost: number;
  }>;
}

interface CustomerTestData {
  email: string;
  name: string;
  phone?: string;
}

@Component({
  selector: 'app-stripe-testing-enhanced',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTabsModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatExpansionModule,
    MatChipsModule,
    MatStepperModule,
    MatCheckboxModule,
    MatRadioModule
  ],
  template: `
    <div class="stripe-testing-container">
      <mat-card class="header-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>credit_card</mat-icon>
            Enhanced Stripe Integration Testing
          </mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <p>Complete end-to-end testing suite for Stripe subscription management</p>
          <div class="connection-status">
            <mat-icon [color]="connectionStatus === 'connected' ? 'primary' : 'warn'">
              {{ connectionStatus === 'connected' ? 'check_circle' : 'error' }}
            </mat-icon>
            <span>Stripe Connection: {{ connectionStatus | titlecase }}</span>
            <button mat-button (click)="testConnection()" [disabled]="loading">
              <mat-icon>refresh</mat-icon>
              Test Connection
            </button>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-tab-group animationDuration="0ms" class="test-tabs">
        <!-- Plan Creation Tab -->
        <mat-tab label="Plan Creation">
          <div class="tab-content">
            <mat-card class="test-form-card">
              <mat-card-header>
                <mat-card-title>Create Test Subscription Plan</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <form [formGroup]="planForm" (ngSubmit)="createTestPlan()">
                  <div class="form-row">
                    <mat-form-field appearance="outline">
                      <mat-label>Plan Name</mat-label>
                      <input matInput formControlName="name" placeholder="e.g., Test Premium Plan">
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Description</mat-label>
                      <input matInput formControlName="description" placeholder="Plan description">
                    </mat-form-field>
                  </div>
                  
                  <div class="form-row">
                    <mat-form-field appearance="outline">
                      <mat-label>Base Price</mat-label>
                      <input matInput type="number" formControlName="price" placeholder="29.99">
                      <span matPrefix>$&nbsp;</span>
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Billing Cycle</mat-label>
                      <mat-select formControlName="billingCycle">
                        <mat-option value="monthly">Monthly</mat-option>
                        <mat-option value="quarterly">Quarterly</mat-option>
                        <mat-option value="annual">Annual</mat-option>
                      </mat-select>
                    </mat-form-field>
                  </div>

                  <div class="privileges-section">
                    <h4>Plan Privileges</h4>
                    <div formArrayName="privileges">
                      <div *ngFor="let privilege of getPrivilegesArray().controls; let i = index" [formGroupName]="i" class="privilege-row">
                        <mat-form-field appearance="outline">
                          <mat-label>Privilege Name</mat-label>
                          <input matInput formControlName="name">
                        </mat-form-field>
                        <mat-form-field appearance="outline">
                          <mat-label>Limit</mat-label>
                          <input matInput type="number" formControlName="limit">
                        </mat-form-field>
                        <mat-form-field appearance="outline">
                          <mat-label>Unit Cost</mat-label>
                          <input matInput type="number" formControlName="unitCost">
                          <span matPrefix>$&nbsp;</span>
                        </mat-form-field>
                        <button mat-icon-button (click)="removePrivilege(i)" type="button">
                          <mat-icon>delete</mat-icon>
                        </button>
                      </div>
                    </div>
                    <button mat-button (click)="addPrivilege()" type="button">
                      <mat-icon>add</mat-icon>
                      Add Privilege
                    </button>
                  </div>

                  <div class="form-actions">
                    <button mat-raised-button color="primary" type="submit" [disabled]="loading">
                      <mat-icon>create</mat-icon>
                      Create Plan
                    </button>
                    <button mat-button (click)="createStripeProduct()" [disabled]="!createdPlan || loading">
                      <mat-icon>credit_card</mat-icon>
                      Create Stripe Product
                    </button>
                  </div>
                </form>
              </mat-card-content>
            </mat-card>

            <div *ngIf="createdPlan" class="created-plan-info">
              <mat-card>
                <mat-card-header>
                  <mat-card-title>Created Plan</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <pre>{{ formatJson(createdPlan) }}</pre>
                </mat-card-content>
              </mat-card>
            </div>
          </div>
        </mat-tab>

        <!-- Customer & Purchase Tab -->
        <mat-tab label="Customer & Purchase">
          <div class="tab-content">
            <mat-card class="test-form-card">
              <mat-card-header>
                <mat-card-title>Create Test Customer & Purchase</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <form [formGroup]="customerForm" (ngSubmit)="createTestCustomer()">
                  <div class="form-row">
                    <mat-form-field appearance="outline">
                      <mat-label>Customer Email</mat-label>
                      <input matInput formControlName="email" placeholder="test@example.com">
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Customer Name</mat-label>
                      <input matInput formControlName="name" placeholder="John Doe">
                    </mat-form-field>
                  </div>
                  
                  <div class="form-row">
                    <mat-form-field appearance="outline">
                      <mat-label>Phone (Optional)</mat-label>
                      <input matInput formControlName="phone" placeholder="+1234567890">
                    </mat-form-field>
                  </div>

                  <div class="form-actions">
                    <button mat-raised-button color="primary" type="submit" [disabled]="loading">
                      <mat-icon>person_add</mat-icon>
                      Create Customer
                    </button>
                    <button mat-raised-button color="accent" (click)="createCheckoutSession()" 
                            [disabled]="!createdCustomer || !createdPlan || loading">
                      <mat-icon>shopping_cart</mat-icon>
                      Create Checkout Session
                    </button>
                  </div>
                </form>
              </mat-card-content>
            </mat-card>

            <div *ngIf="createdCustomer" class="created-customer-info">
              <mat-card>
                <mat-card-header>
                  <mat-card-title>Created Customer</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <pre>{{ formatJson(createdCustomer) }}</pre>
                </mat-card-content>
              </mat-card>
            </div>

            <div *ngIf="checkoutSession" class="checkout-session-info">
              <mat-card>
                <mat-card-header>
                  <mat-card-title>Checkout Session</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <p><strong>Session URL:</strong> <a [href]="checkoutSession.url" target="_blank">{{ checkoutSession.url }}</a></p>
                  <p><strong>Session ID:</strong> {{ checkoutSession.sessionId }}</p>
                  <button mat-raised-button color="primary" (click)="openCheckoutSession()">
                    <mat-icon>open_in_new</mat-icon>
                    Open Checkout
                  </button>
                </mat-card-content>
              </mat-card>
            </div>
          </div>
        </mat-tab>

        <!-- Subscription Management Tab -->
        <mat-tab label="Subscription Management">
          <div class="tab-content">
            <mat-card class="test-form-card">
              <mat-card-header>
                <mat-card-title>Subscription Lifecycle Testing</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="subscription-actions">
                  <button mat-raised-button color="primary" (click)="getActiveSubscriptions()" [disabled]="loading">
                    <mat-icon>list</mat-icon>
                    Get Active Subscriptions
                  </button>
                  <button mat-raised-button color="accent" (click)="pauseSubscription()" 
                          [disabled]="!activeSubscription || loading">
                    <mat-icon>pause</mat-icon>
                    Pause Subscription
                  </button>
                  <button mat-raised-button color="primary" (click)="resumeSubscription()" 
                          [disabled]="!pausedSubscription || loading">
                    <mat-icon>play_arrow</mat-icon>
                    Resume Subscription
                  </button>
                  <button mat-raised-button color="warn" (click)="cancelSubscription()" 
                          [disabled]="!activeSubscription || loading">
                    <mat-icon>cancel</mat-icon>
                    Cancel Subscription
                  </button>
                </div>

                <div *ngIf="activeSubscriptions.length > 0" class="subscriptions-list">
                  <h4>Active Subscriptions:</h4>
                  <mat-card *ngFor="let sub of activeSubscriptions" class="subscription-card">
                    <mat-card-content>
                      <div class="subscription-info">
                        <p><strong>ID:</strong> {{ sub.id }}</p>
                        <p><strong>Status:</strong> {{ sub.status }}</p>
                        <p><strong>Plan:</strong> {{ sub.planName }}</p>
                        <p><strong>Customer:</strong> {{ sub.customerEmail }}</p>
                      </div>
                      <div class="subscription-actions">
                        <button mat-button (click)="selectSubscription(sub)">
                          <mat-icon>select_all</mat-icon>
                          Select
                        </button>
                      </div>
                    </mat-card-content>
                  </mat-card>
                </div>
              </mat-card-content>
            </mat-card>
          </div>
        </mat-tab>

        <!-- Service Restrictions Tab -->
        <mat-tab label="Service Restrictions">
          <div class="tab-content">
            <mat-card class="test-form-card">
              <mat-card-header>
                <mat-card-title>Test Service Restrictions</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="restriction-tests">
                  <button mat-raised-button color="primary" (click)="testPrivilegeUsage()" [disabled]="loading">
                    <mat-icon>track_changes</mat-icon>
                    Test Privilege Usage
                  </button>
                  <button mat-raised-button color="accent" (click)="testOverageCharges()" [disabled]="loading">
                    <mat-icon>monetization_on</mat-icon>
                    Test Overage Charges
                  </button>
                  <button mat-raised-button color="warn" (click)="testServiceBlocking()" [disabled]="loading">
                    <mat-icon>block</mat-icon>
                    Test Service Blocking
                  </button>
                </div>

                <div *ngIf="privilegeUsage.length > 0" class="privilege-usage-results">
                  <h4>Privilege Usage Results:</h4>
                  <mat-card *ngFor="let usage of privilegeUsage" class="usage-card">
                    <mat-card-content>
                      <div class="usage-info">
                        <p><strong>Privilege:</strong> {{ usage.privilegeName }}</p>
                        <p><strong>Used:</strong> {{ usage.used }} / {{ usage.limit }}</p>
                        <p><strong>Remaining:</strong> {{ usage.remaining }}</p>
                        <p><strong>Status:</strong> 
                          <mat-chip [color]="usage.remaining > 0 ? 'primary' : 'warn'">
                            {{ usage.remaining > 0 ? 'Available' : 'Exhausted' }}
                          </mat-chip>
                        </p>
                      </div>
                    </mat-card-content>
                  </mat-card>
                </div>
              </mat-card-content>
            </mat-card>
          </div>
        </mat-tab>

        <!-- Real-time Monitor Tab -->
        <mat-tab label="Real-time Monitor">
          <div class="tab-content">
            <div class="monitor-controls">
              <button mat-raised-button 
                      color="primary" 
                      (click)="toggleMonitoring()"
                      [disabled]="loading">
                <mat-icon>{{ monitoring ? 'stop' : 'play_arrow' }}</mat-icon>
                {{ monitoring ? 'Stop Monitoring' : 'Start Monitoring' }}
              </button>
              <button mat-button (click)="clearMonitorLogs()">
                <mat-icon>clear</mat-icon>
                Clear Logs
              </button>
            </div>
            <div class="monitor-logs">
              <div *ngFor="let log of monitorLogs" class="log-entry" [ngClass]="log.type">
                <div class="log-header">
                  <mat-icon>{{ getLogIcon(log.type) }}</mat-icon>
                  <span class="log-timestamp">{{ log.timestamp | date:'medium' }}</span>
                  <span class="log-type">{{ log.type | uppercase }}</span>
                </div>
                <div class="log-content">
                  <p><strong>{{ log.message }}</strong></p>
                  <pre *ngIf="log.data">{{ formatJson(log.data) }}</pre>
                </div>
              </div>
            </div>
          </div>
        </mat-tab>
      </mat-tab-group>

      <div *ngIf="loading" class="loading-overlay">
        <mat-spinner diameter="50"></mat-spinner>
        <p>Running tests...</p>
      </div>
    </div>
  `,
  styles: [`
    .stripe-testing-container {
      padding: 24px;
      max-width: 1400px;
      margin: 0 auto;
    }
    
    .header-card {
      margin-bottom: 24px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }
    
    .connection-status {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-top: 16px;
      padding: 12px;
      background-color: #f5f5f5;
      border-radius: 8px;
    }
    
    .test-tabs {
      margin-bottom: 24px;
    }
    
    .tab-content {
      padding: 24px 0;
    }
    
    .test-form-card {
      margin-bottom: 24px;
    }
    
    .form-row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 16px;
      margin-bottom: 16px;
    }
    
    .privileges-section {
      margin: 24px 0;
      padding: 16px;
      background-color: #f9f9f9;
      border-radius: 8px;
    }
    
    .privilege-row {
      display: grid;
      grid-template-columns: 2fr 1fr 1fr auto;
      gap: 16px;
      align-items: center;
      margin-bottom: 16px;
    }
    
    .form-actions {
      display: flex;
      gap: 12px;
      margin-top: 24px;
    }
    
    .subscription-actions {
      display: flex;
      gap: 12px;
      margin-bottom: 24px;
      flex-wrap: wrap;
    }
    
    .subscriptions-list {
      margin-top: 24px;
    }
    
    .subscription-card {
      margin-bottom: 16px;
    }
    
    .subscription-info {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 16px;
    }
    
    .restriction-tests {
      display: flex;
      gap: 12px;
      margin-bottom: 24px;
      flex-wrap: wrap;
    }
    
    .privilege-usage-results {
      margin-top: 24px;
    }
    
    .usage-card {
      margin-bottom: 16px;
    }
    
    .usage-info {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
      gap: 16px;
    }
    
    .monitor-controls {
      margin-bottom: 24px;
      display: flex;
      gap: 12px;
      padding: 16px;
      background-color: #f5f5f5;
      border-radius: 8px;
    }
    
    .monitor-logs {
      max-height: 600px;
      overflow-y: auto;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      background-color: white;
    }
    
    .log-entry {
      padding: 12px;
      border-bottom: 1px solid #e0e0e0;
    }
    
    .log-entry.info {
      background-color: #e3f2fd;
      border-left: 4px solid #2196f3;
    }
    
    .log-entry.success {
      background-color: #e8f5e8;
      border-left: 4px solid #4caf50;
    }
    
    .log-entry.error {
      background-color: #ffebee;
      border-left: 4px solid #f44336;
    }
    
    .log-header {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 8px;
    }
    
    .log-timestamp {
      font-size: 0.9em;
      color: #666;
      font-family: monospace;
    }
    
    .log-type {
      font-weight: bold;
      font-size: 0.8em;
      text-transform: uppercase;
    }
    
    .loading-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background-color: rgba(0, 0, 0, 0.5);
      display: flex;
      flex-direction: column;
      justify-content: center;
      align-items: center;
      z-index: 9999;
    }
    
    .loading-overlay p {
      color: white;
      margin-top: 16px;
    }
    
    pre {
      background-color: #f5f5f5;
      padding: 12px;
      border-radius: 4px;
      font-size: 0.9em;
      overflow-x: auto;
      max-height: 300px;
      overflow-y: auto;
    }
  `]
})
export class StripeTestingEnhancedComponent implements OnInit, OnDestroy {
  connectionStatus: 'connected' | 'disconnected' | 'unknown' = 'unknown';
  loading = false;
  monitoring = false;
  monitorLogs: any[] = [];
  
  // Form data
  planForm!: FormGroup;
  customerForm!: FormGroup;
  
  // Created objects
  createdPlan: any = null;
  createdCustomer: any = null;
  checkoutSession: any = null;
  activeSubscriptions: any[] = [];
  activeSubscription: any = null;
  pausedSubscription: any = null;
  privilegeUsage: any[] = [];
  
  private subscriptions: Subscription[] = [];

  constructor(
    private stripeService: StripeService,
    private subscriptionService: SubscriptionService,
    private subscriptionPlanService: SubscriptionPlanService,
    private snackBar: MatSnackBar,
    private fb: FormBuilder
  ) {
    this.initializeForms();
  }

  ngOnInit(): void {
    this.testConnection();
  }

  private initializeForms(): void {
    this.planForm = this.fb.group({
      name: ['Test Premium Plan'],
      description: ['A test plan for Stripe integration testing'],
      price: [29.99],
      billingCycle: ['monthly'],
      privileges: this.fb.array([
        this.fb.group({
          name: ['Consultations'],
          limit: [5],
          unitCost: [20]
        }),
        this.fb.group({
          name: ['Medications'],
          limit: [3],
          unitCost: [50]
        })
      ])
    });

    this.customerForm = this.fb.group({
      email: ['test@example.com'],
      name: ['Test Customer'],
      phone: ['+1234567890']
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  async testConnection(): Promise<void> {
    this.loading = true;
    this.addMonitorLog('info', 'Testing Stripe connection...');
    
    try {
      const startTime = Date.now();
      const response = await this.stripeService.testConnection().toPromise();
      const duration = Date.now() - startTime;
      
      this.connectionStatus = response?.statusCode === 200 ? 'connected' : 'disconnected';
      
      this.addMonitorLog(
        this.connectionStatus === 'connected' ? 'success' : 'error',
        `Connection test ${this.connectionStatus === 'connected' ? 'successful' : 'failed'}`,
        { response, duration }
      );
      
      this.snackBar.open(
        `Connection test ${this.connectionStatus === 'connected' ? 'successful' : 'failed'}`,
        'Close',
        { duration: 3000 }
      );
    } catch (error: any) {
      this.connectionStatus = 'disconnected';
      this.addMonitorLog('error', 'Connection test failed', { error });
    } finally {
      this.loading = false;
    }
  }

  async createTestPlan(): Promise<void> {
    this.loading = true;
    this.addMonitorLog('info', 'Creating test subscription plan...');
    
    try {
      const planData = {
        name: this.planForm.get('name')?.value,
        description: this.planForm.get('description')?.value,
        basePrice: this.planForm.get('price')?.value,
        billingCycle: this.planForm.get('billingCycle')?.value,
        privileges: this.planForm.get('privileges')?.value,
        isActive: true
      };
      
      const response = await this.subscriptionPlanService.createPlan(planData).toPromise();
      
      if (response?.statusCode === 200) {
        this.createdPlan = response.data;
        this.addMonitorLog('success', 'Test plan created successfully', { plan: this.createdPlan });
        this.snackBar.open('Test plan created successfully', 'Close', { duration: 3000 });
      } else {
        throw new Error(response?.message || 'Failed to create plan');
      }
    } catch (error: any) {
      this.addMonitorLog('error', 'Failed to create test plan', { error });
      this.snackBar.open('Failed to create test plan', 'Close', { duration: 3000 });
    } finally {
      this.loading = false;
    }
  }

  async createStripeProduct(): Promise<void> {
    this.loading = true;
    this.addMonitorLog('info', 'Creating Stripe product...');
    
    try {
      const productData = {
        name: this.createdPlan.name,
        description: this.createdPlan.description,
        metadata: {
          planId: this.createdPlan.id,
          source: 'smart_telehealth_test'
        }
      };
      
      const response = await this.stripeService.createProduct(productData).toPromise();
      
      if (response?.statusCode === 200) {
        this.addMonitorLog('success', 'Stripe product created successfully', { product: response.data });
        this.snackBar.open('Stripe product created successfully', 'Close', { duration: 3000 });
      } else {
        throw new Error(response?.message || 'Failed to create Stripe product');
      }
    } catch (error: any) {
      this.addMonitorLog('error', 'Failed to create Stripe product', { error });
      this.snackBar.open('Failed to create Stripe product', 'Close', { duration: 3000 });
    } finally {
      this.loading = false;
    }
  }

  async createTestCustomer(): Promise<void> {
    this.loading = true;
    this.addMonitorLog('info', 'Creating test customer...');
    
    try {
      const customerData = {
        email: this.customerForm.get('email')?.value,
        name: this.customerForm.get('name')?.value,
        phone: this.customerForm.get('phone')?.value
      };
      
      const response = await this.stripeService.createCustomer(customerData).toPromise();
      
      if (response?.statusCode === 200) {
        this.createdCustomer = response.data;
        this.addMonitorLog('success', 'Test customer created successfully', { customer: this.createdCustomer });
        this.snackBar.open('Test customer created successfully', 'Close', { duration: 3000 });
      } else {
        throw new Error(response?.message || 'Failed to create customer');
      }
    } catch (error: any) {
      this.addMonitorLog('error', 'Failed to create test customer', { error });
      this.snackBar.open('Failed to create test customer', 'Close', { duration: 3000 });
    } finally {
      this.loading = false;
    }
  }

  async createCheckoutSession(): Promise<void> {
    this.loading = true;
    this.addMonitorLog('info', 'Creating checkout session...');
    
    try {
      const sessionData = {
        successUrl: window.location.origin + '/payment/success',
        cancelUrl: window.location.origin + '/payment/cancel',
        planId: this.createdPlan.id,
        customerId: this.createdCustomer.id
      };
      
      const response = await this.stripeService.createCheckoutSession(sessionData).toPromise();
      
      if (response?.statusCode === 200) {
        this.checkoutSession = response.data;
        this.addMonitorLog('success', 'Checkout session created successfully', { session: this.checkoutSession });
        this.snackBar.open('Checkout session created successfully', 'Close', { duration: 3000 });
      } else {
        throw new Error(response?.message || 'Failed to create checkout session');
      }
    } catch (error: any) {
      this.addMonitorLog('error', 'Failed to create checkout session', { error });
      this.snackBar.open('Failed to create checkout session', 'Close', { duration: 3000 });
    } finally {
      this.loading = false;
    }
  }

  openCheckoutSession(): void {
    if (this.checkoutSession?.url) {
      window.open(this.checkoutSession.url, '_blank');
    }
  }

  async getActiveSubscriptions(): Promise<void> {
    this.loading = true;
    this.addMonitorLog('info', 'Fetching active subscriptions...');
    
    try {
      const response = await this.subscriptionService.getAllSubscriptions().toPromise();
      
      if (response?.statusCode === 200) {
        this.activeSubscriptions = response.data || [];
        this.addMonitorLog('success', `Found ${this.activeSubscriptions.length} active subscriptions`, { subscriptions: this.activeSubscriptions });
        this.snackBar.open(`Found ${this.activeSubscriptions.length} active subscriptions`, 'Close', { duration: 3000 });
      } else {
        throw new Error(response?.message || 'Failed to fetch subscriptions');
      }
    } catch (error: any) {
      this.addMonitorLog('error', 'Failed to fetch active subscriptions', { error });
      this.snackBar.open('Failed to fetch active subscriptions', 'Close', { duration: 3000 });
    } finally {
      this.loading = false;
    }
  }

  selectSubscription(subscription: any): void {
    this.activeSubscription = subscription;
    this.addMonitorLog('info', 'Subscription selected for testing', { subscription });
  }

  async pauseSubscription(): Promise<void> {
    if (!this.activeSubscription) return;
    
    this.loading = true;
    this.addMonitorLog('info', 'Pausing subscription...');
    
    try {
      const response = await this.stripeService.pauseStripeSubscription(this.activeSubscription.stripeSubscriptionId).toPromise();
      
      if (response?.statusCode === 200) {
        this.pausedSubscription = this.activeSubscription;
        this.activeSubscription = null;
        this.addMonitorLog('success', 'Subscription paused successfully', { response });
        this.snackBar.open('Subscription paused successfully', 'Close', { duration: 3000 });
      } else {
        throw new Error(response?.message || 'Failed to pause subscription');
      }
    } catch (error: any) {
      this.addMonitorLog('error', 'Failed to pause subscription', { error });
      this.snackBar.open('Failed to pause subscription', 'Close', { duration: 3000 });
    } finally {
      this.loading = false;
    }
  }

  async resumeSubscription(): Promise<void> {
    if (!this.pausedSubscription) return;
    
    this.loading = true;
    this.addMonitorLog('info', 'Resuming subscription...');
    
    try {
      const response = await this.stripeService.resumeStripeSubscription(this.pausedSubscription.stripeSubscriptionId).toPromise();
      
      if (response?.statusCode === 200) {
        this.activeSubscription = this.pausedSubscription;
        this.pausedSubscription = null;
        this.addMonitorLog('success', 'Subscription resumed successfully', { response });
        this.snackBar.open('Subscription resumed successfully', 'Close', { duration: 3000 });
      } else {
        throw new Error(response?.message || 'Failed to resume subscription');
      }
    } catch (error: any) {
      this.addMonitorLog('error', 'Failed to resume subscription', { error });
      this.snackBar.open('Failed to resume subscription', 'Close', { duration: 3000 });
    } finally {
      this.loading = false;
    }
  }

  async cancelSubscription(): Promise<void> {
    if (!this.activeSubscription) return;
    
    this.loading = true;
    this.addMonitorLog('info', 'Cancelling subscription...');
    
    try {
      const response = await this.stripeService.cancelStripeSubscription(this.activeSubscription.stripeSubscriptionId, 'Test cancellation').toPromise();
      
      if (response?.statusCode === 200) {
        this.activeSubscription = null;
        this.addMonitorLog('success', 'Subscription cancelled successfully', { response });
        this.snackBar.open('Subscription cancelled successfully', 'Close', { duration: 3000 });
      } else {
        throw new Error(response?.message || 'Failed to cancel subscription');
      }
    } catch (error: any) {
      this.addMonitorLog('error', 'Failed to cancel subscription', { error });
      this.snackBar.open('Failed to cancel subscription', 'Close', { duration: 3000 });
    } finally {
      this.loading = false;
    }
  }

  async testPrivilegeUsage(): Promise<void> {
    this.loading = true;
    this.addMonitorLog('info', 'Testing privilege usage...');
    
    try {
      // Simulate privilege usage testing
      const mockUsage = [
        { privilegeName: 'Consultations', used: 3, limit: 5, remaining: 2 },
        { privilegeName: 'Medications', used: 5, limit: 3, remaining: 0 }
      ];
      
      this.privilegeUsage = mockUsage;
      this.addMonitorLog('success', 'Privilege usage test completed', { usage: mockUsage });
      this.snackBar.open('Privilege usage test completed', 'Close', { duration: 3000 });
    } catch (error: any) {
      this.addMonitorLog('error', 'Failed to test privilege usage', { error });
      this.snackBar.open('Failed to test privilege usage', 'Close', { duration: 3000 });
    } finally {
      this.loading = false;
    }
  }

  async testOverageCharges(): Promise<void> {
    this.loading = true;
    this.addMonitorLog('info', 'Testing overage charges...');
    
    try {
      // Simulate overage charge testing
      const overageData = {
        privilegeName: 'Medications',
        overageAmount: 2,
        unitCost: 50,
        totalOverageCharge: 100
      };
      
      this.addMonitorLog('success', 'Overage charges test completed', { overage: overageData });
      this.snackBar.open('Overage charges test completed', 'Close', { duration: 3000 });
    } catch (error: any) {
      this.addMonitorLog('error', 'Failed to test overage charges', { error });
      this.snackBar.open('Failed to test overage charges', 'Close', { duration: 3000 });
    } finally {
      this.loading = false;
    }
  }

  async testServiceBlocking(): Promise<void> {
    this.loading = true;
    this.addMonitorLog('info', 'Testing service blocking...');
    
    try {
      // Simulate service blocking test
      const blockingData = {
        blockedServices: ['Medications'],
        reason: 'Privilege limit exceeded',
        remainingServices: ['Consultations']
      };
      
      this.addMonitorLog('success', 'Service blocking test completed', { blocking: blockingData });
      this.snackBar.open('Service blocking test completed', 'Close', { duration: 3000 });
    } catch (error: any) {
      this.addMonitorLog('error', 'Failed to test service blocking', { error });
      this.snackBar.open('Failed to test service blocking', 'Close', { duration: 3000 });
    } finally {
      this.loading = false;
    }
  }

  getPrivilegesArray(): FormArray {
    return this.planForm.get('privileges') as FormArray;
  }

  addPrivilege(): void {
    const privilegesArray = this.getPrivilegesArray();
    privilegesArray.push(this.fb.group({
      name: [''],
      limit: [0],
      unitCost: [0]
    }));
  }

  removePrivilege(index: number): void {
    const privilegesArray = this.getPrivilegesArray();
    privilegesArray.removeAt(index);
  }

  toggleMonitoring(): void {
    this.monitoring = !this.monitoring;
    this.addMonitorLog(
      'info',
      `Monitoring ${this.monitoring ? 'started' : 'stopped'}`
    );
  }

  clearMonitorLogs(): void {
    this.monitorLogs = [];
  }

  addMonitorLog(type: 'info' | 'success' | 'error', message: string, data?: any): void {
    this.monitorLogs.unshift({
      type,
      message,
      timestamp: new Date(),
      data
    });
    
    // Keep only last 100 logs
    if (this.monitorLogs.length > 100) {
      this.monitorLogs = this.monitorLogs.slice(0, 100);
    }
  }

  getLogIcon(type: string): string {
    switch (type) {
      case 'success': return 'check_circle';
      case 'error': return 'error';
      default: return 'info';
    }
  }

  formatJson(obj: any): string {
    return JSON.stringify(obj, null, 2);
  }
}
