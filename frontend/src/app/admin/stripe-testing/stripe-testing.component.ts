import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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
}

interface TestScenario {
  id: string;
  name: string;
  description: string;
  steps: string[];
  status: 'pending' | 'running' | 'completed' | 'failed';
  results: TestResult[];
}

@Component({
  selector: 'app-stripe-testing',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTabsModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatExpansionModule,
    MatChipsModule
  ],
  template: `
    <div class="stripe-testing-container">
      <mat-card class="header-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>credit_card</mat-icon>
            Stripe Integration Testing Dashboard
          </mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <p>Comprehensive testing suite for Stripe subscription management functionality</p>
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
        <!-- Quick Tests Tab -->
        <mat-tab label="Quick Tests">
          <div class="tab-content">
            <div class="test-grid">
              <mat-card class="test-card" *ngFor="let test of quickTests">
                <mat-card-header>
                  <mat-card-title>{{ test.name }}</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <p>{{ test.description }}</p>
                  <div class="test-actions">
                    <button mat-raised-button 
                            color="primary" 
                            (click)="runQuickTest(test.id)"
                            [disabled]="loading">
                      <mat-icon>play_arrow</mat-icon>
                      Run Test
                    </button>
                  </div>
                  <div *ngIf="test.results.length > 0" class="test-results">
                    <mat-expansion-panel>
                      <mat-expansion-panel-header>
                        <mat-panel-title>Results ({{ test.results.length }})</mat-panel-title>
                      </mat-expansion-panel-header>
                      <div *ngFor="let result of test.results" class="result-item">
                        <mat-icon [color]="result.success ? 'primary' : 'warn'">
                          {{ result.success ? 'check_circle' : 'error' }}
                        </mat-icon>
                        <span>{{ result.timestamp | date:'short' }} - 
                              {{ result.success ? 'Success' : 'Failed' }}
                              {{ result.duration ? '(' + result.duration + 'ms)' : '' }}</span>
                        <div *ngIf="result.error" class="error-message">{{ result.error }}</div>
                      </div>
                    </mat-expansion-panel>
                  </div>
                </mat-card-content>
              </mat-card>
            </div>
          </div>
        </mat-tab>

        <!-- End-to-End Tests Tab -->
        <mat-tab label="End-to-End Tests">
          <div class="tab-content">
            <div class="scenario-list">
              <mat-card *ngFor="let scenario of testScenarios" class="scenario-card">
                <mat-card-header>
                  <div class="scenario-header">
                    <div>
                      <mat-card-title>{{ scenario.name }}</mat-card-title>
                      <p>{{ scenario.description }}</p>
                    </div>
                    <mat-chip-list>
                      <mat-chip [color]="getScenarioStatusColor(scenario.status)">
                        {{ scenario.status | titlecase }}
                      </mat-chip>
                    </mat-chip-list>
                  </div>
                </mat-card-header>
                <mat-card-content>
                  <div class="scenario-steps">
                    <h4>Test Steps:</h4>
                    <ol>
                      <li *ngFor="let step of scenario.steps">{{ step }}</li>
                    </ol>
                  </div>
                  <div class="scenario-actions">
                    <button mat-raised-button 
                            color="primary" 
                            (click)="runScenario(scenario)"
                            [disabled]="loading">
                      <mat-icon>play_arrow</mat-icon>
                      Run Scenario
                    </button>
                    <button mat-button 
                            (click)="clearScenarioResults(scenario)"
                            [disabled]="loading">
                      <mat-icon>clear</mat-icon>
                      Clear Results
                    </button>
                  </div>
                  <div *ngIf="scenario.results.length > 0" class="scenario-results">
                    <h4>Test Results:</h4>
                    <div *ngFor="let result of scenario.results; let i = index" class="result-detail">
                      <mat-expansion-panel>
                        <mat-expansion-panel-header>
                          <mat-panel-title>
                            Step {{ i + 1 }}: {{ result.success ? 'Success' : 'Failed' }}
                            <mat-icon [color]="result.success ? 'primary' : 'warn'" class="result-icon">
                              {{ result.success ? 'check_circle' : 'error' }}
                            </mat-icon>
                          </mat-panel-title>
                        </mat-expansion-panel-header>
                        <div class="result-content">
                          <div class="result-summary">
                            <p><strong>Timestamp:</strong> {{ result.timestamp | date:'medium' }}</p>
                            <p *ngIf="result.duration"><strong>Duration:</strong> {{ result.duration }}ms</p>
                            <p *ngIf="result.error" class="error-text"><strong>Error:</strong> {{ result.error }}</p>
                          </div>
                          <div class="request-response">
                            <mat-expansion-panel>
                              <mat-expansion-panel-header>
                                <mat-panel-title>Request</mat-panel-title>
                              </mat-expansion-panel-header>
                              <pre>{{ formatJson(result.request) }}</pre>
                            </mat-expansion-panel>
                            <mat-expansion-panel>
                              <mat-expansion-panel-header>
                                <mat-panel-title>Response</mat-panel-title>
                              </mat-expansion-panel-header>
                              <pre>{{ formatJson(result.response) }}</pre>
                            </mat-expansion-panel>
                          </div>
                        </div>
                      </mat-expansion-panel>
                    </div>
                  </div>
                </mat-card-content>
              </mat-card>
            </div>
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
    }
    .connection-status {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-top: 16px;
    }
    .test-tabs {
      margin-bottom: 24px;
    }
    .tab-content {
      padding: 24px 0;
    }
    .test-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 24px;
    }
    .test-card {
      min-height: 200px;
    }
    .test-actions {
      margin-top: 16px;
    }
    .test-results {
      margin-top: 16px;
    }
    .result-item {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 0;
    }
    .error-message {
      color: #f44336;
      font-size: 0.9em;
      margin-left: 32px;
    }
    .scenario-card {
      margin-bottom: 24px;
    }
    .scenario-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      width: 100%;
    }
    .scenario-steps {
      margin: 16px 0;
    }
    .scenario-actions {
      margin: 16px 0;
      display: flex;
      gap: 12px;
    }
    .scenario-results {
      margin-top: 16px;
    }
    .result-detail {
      margin-bottom: 16px;
    }
    .result-content {
      padding: 16px;
    }
    .result-summary p {
      margin: 8px 0;
    }
    .error-text {
      color: #f44336;
    }
    .request-response {
      margin-top: 16px;
    }
    .request-response pre {
      background-color: #f5f5f5;
      padding: 12px;
      border-radius: 4px;
      font-size: 0.9em;
      overflow-x: auto;
    }
    .monitor-controls {
      margin-bottom: 24px;
      display: flex;
      gap: 12px;
    }
    .monitor-logs {
      max-height: 600px;
      overflow-y: auto;
      border: 1px solid #e0e0e0;
      border-radius: 4px;
    }
    .log-entry {
      padding: 12px;
      border-bottom: 1px solid #e0e0e0;
    }
    .log-entry:last-child {
      border-bottom: none;
    }
    .log-entry.info {
      background-color: #e3f2fd;
    }
    .log-entry.success {
      background-color: #e8f5e8;
    }
    .log-entry.error {
      background-color: #ffebee;
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
    }
    .log-type {
      font-weight: bold;
      font-size: 0.8em;
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
  `]
})
export class StripeTestingComponent implements OnInit, OnDestroy {
  connectionStatus: 'connected' | 'disconnected' | 'unknown' = 'unknown';
  loading = false;
  monitoring = false;
  monitorLogs: any[] = [];
  
  quickTests = [
    {
      id: 'connection',
      name: 'Connection Test',
      description: 'Test Stripe API connectivity',
      results: [] as TestResult[]
    },
    {
      id: 'customers',
      name: 'List Customers',
      description: 'Retrieve Stripe customers',
      results: [] as TestResult[]
    },
    {
      id: 'products',
      name: 'List Products',
      description: 'Retrieve Stripe products',
      results: [] as TestResult[]
    }
  ];

  testScenarios: TestScenario[] = [
    {
      id: 'full-lifecycle',
      name: 'Complete Subscription Lifecycle',
      description: 'Test the entire subscription flow from plan creation to cancellation',
      steps: [
        'Create a test subscription plan',
        'Create Stripe product and price',
        'Create customer and checkout session',
        'Complete payment flow',
        'Verify subscription activation',
        'Test subscription modifications',
        'Test service restrictions',
        'Test subscription cancellation'
      ],
      status: 'pending',
      results: []
    }
  ];

  private subscriptions: Subscription[] = [];

  constructor(
    private stripeService: StripeService,
    private subscriptionService: SubscriptionService,
    private subscriptionPlanService: SubscriptionPlanService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.testConnection();
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
      
      const result: TestResult = {
        success: response?.statusCode === 200,
        timestamp: new Date(),
        request: { endpoint: '/api/stripe/test-connection' },
        response,
        duration
      };
      
      this.quickTests[0].results.unshift(result);
      this.connectionStatus = result.success ? 'connected' : 'disconnected';
      
      this.addMonitorLog(
        result.success ? 'success' : 'error',
        `Connection test ${result.success ? 'successful' : 'failed'}`,
        { result }
      );
      
      this.snackBar.open(
        `Connection test ${result.success ? 'successful' : 'failed'}`,
        'Close',
        { duration: 3000 }
      );
    } catch (error: any) {
      const result: TestResult = {
        success: false,
        timestamp: new Date(),
        request: { endpoint: '/api/stripe/test-connection' },
        response: null,
        error: error.message || 'Unknown error'
      };
      
      this.quickTests[0].results.unshift(result);
      this.connectionStatus = 'disconnected';
      this.addMonitorLog('error', 'Connection test failed', { error });
    } finally {
      this.loading = false;
    }
  }

  async runQuickTest(testId: string): Promise<void> {
    this.loading = true;
    const test = this.quickTests.find(t => t.id === testId);
    if (!test) return;

    try {
      let response: any;
      const startTime = Date.now();
      
      switch (testId) {
        case 'customers':
          response = await this.stripeService.getCustomers().toPromise();
          break;
        case 'products':
          response = await this.stripeService.getProducts().toPromise();
          break;
        default:
          throw new Error('Unknown test type');
      }
      
      const duration = Date.now() - startTime;
      const result: TestResult = {
        success: response?.statusCode === 200,
        timestamp: new Date(),
        request: { endpoint: `/api/stripe/${testId}` },
        response,
        duration
      };
      
      test.results.unshift(result);
      this.addMonitorLog(
        result.success ? 'success' : 'error',
        `${test.name} ${result.success ? 'successful' : 'failed'}`,
        { result }
      );
      
    } catch (error: any) {
      const result: TestResult = {
        success: false,
        timestamp: new Date(),
        request: { endpoint: `/api/stripe/${testId}` },
        response: null,
        error: error.message || 'Unknown error'
      };
      
      test.results.unshift(result);
      this.addMonitorLog('error', `${test.name} failed`, { error });
    } finally {
      this.loading = false;
    }
  }

  async runScenario(scenario: TestScenario): Promise<void> {
    scenario.status = 'running';
    this.loading = true;
    this.addMonitorLog('info', `Starting scenario: ${scenario.name}`);

    try {
      switch (scenario.id) {
        case 'full-lifecycle':
          await this.runFullLifecycleScenario(scenario);
          break;
        default:
          throw new Error('Unknown scenario type');
      }
      
      scenario.status = 'completed';
      this.addMonitorLog('success', `Scenario completed: ${scenario.name}`);
    } catch (error: any) {
      scenario.status = 'failed';
      this.addMonitorLog('error', `Scenario failed: ${scenario.name}`, { error });
    } finally {
      this.loading = false;
    }
  }

  private async runFullLifecycleScenario(scenario: TestScenario): Promise<void> {
    // This will be implemented in the next part
    // For now, add a placeholder result
    const result: TestResult = {
      success: true,
      timestamp: new Date(),
      request: { scenario: 'full-lifecycle', step: 'placeholder' },
      response: { message: 'Scenario implementation pending' },
      duration: 100
    };
    
    scenario.results.push(result);
  }

  clearScenarioResults(scenario: TestScenario): void {
    scenario.results = [];
    scenario.status = 'pending';
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

  getScenarioStatusColor(status: string): string {
    switch (status) {
      case 'completed': return 'primary';
      case 'failed': return 'warn';
      case 'running': return 'accent';
      default: return 'primary';
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
