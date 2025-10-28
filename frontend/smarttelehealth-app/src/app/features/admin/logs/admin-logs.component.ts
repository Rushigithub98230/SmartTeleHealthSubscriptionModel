import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCardModule } from '@angular/material/card';
import { MatBadgeModule } from '@angular/material/badge';
import { HubConnectionState } from '@microsoft/signalr';
import { Subject, takeUntil, debounceTime } from 'rxjs';
import { LogsService } from '../../../core/services/logs.service';
import { SignalRLogsService } from '../../../core/services/signalr-logs.service';
import { ApplicationLog, ApplicationLogFilterDto, LogStatistics } from '../../../core/models/logs.model';

@Component({
  selector: 'app-admin-logs',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    FormsModule,
    ScrollingModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatExpansionModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatSlideToggleModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatCardModule,
    MatBadgeModule
  ],
  templateUrl: './admin-logs.component.html',
  styleUrls: ['./admin-logs.component.scss']
})
export class AdminLogsComponent implements OnInit, OnDestroy {
  @ViewChild(CdkVirtualScrollViewport) viewport!: CdkVirtualScrollViewport;

  // Log data
  logs: ApplicationLog[] = [];
  displayedLogs: ApplicationLog[] = [];
  
  // Virtual scroll
  itemSize = 80;
  
  // Filters
  filterForm!: FormGroup;
  logLevels = ['Information', 'Warning', 'Error', 'Critical', 'Debug'];
  sources: string[] = [];
  
  // Real-time
  isRealTimeEnabled = true;
  autoScroll = true;
  connectionStatus: HubConnectionState = HubConnectionState.Disconnected;
  HubConnectionState = HubConnectionState; // For template use
  
  // Statistics
  statistics: LogStatistics | null = null;
  
  // Pagination
  pageSize = 100;
  totalLogs = 0;
  currentPage = 1;
  
  // Loading state
  isLoading = false;
  
  // Expanded log
  expandedLogId: number | null = null;
  
  private destroy$ = new Subject<void>();

  constructor(
    private logsService: LogsService,
    private signalRService: SignalRLogsService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    console.log('[AdminLogsComponent] ngOnInit - Component initialized');
    this.initializeFilters();
    console.log('[AdminLogsComponent] Filters initialized:', this.filterForm.value);
    this.loadLogs();
    console.log('[AdminLogsComponent] Loading logs...');
    this.connectToSignalR();
    console.log('[AdminLogsComponent] Connecting to SignalR...');
    this.loadStatistics();
    console.log('[AdminLogsComponent] Loading statistics...');
    this.subscribeToFilterChanges();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.signalRService.stopConnection();
  }

  private initializeFilters(): void {
    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);
    yesterday.setHours(0, 0, 0, 0);  // Start of yesterday
    
    const endOfToday = new Date();
    endOfToday.setHours(23, 59, 59, 999);  // End of today
    
    this.filterForm = this.fb.group({
      logLevel: [[]],
      source: [[]],
      startDate: [yesterday],
      endDate: [endOfToday],
      searchTerm: [''],
      userId: [null],
      operation: [''],
      correlationId: ['']
    });
  }

  private subscribeToFilterChanges(): void {
    this.filterForm.valueChanges
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(500)  // Wait 500ms after user stops typing
      )
      .subscribe(() => {
        console.log('[AdminLogsComponent] Filter changed, reloading logs');
        this.currentPage = 1;  // Reset to first page
        this.loadLogs();
      });
  }

  private async connectToSignalR(): Promise<void> {
    console.log('[AdminLogsComponent] connectToSignalR - Starting connection...');
    try {
      await this.signalRService.startConnection();
      console.log('[AdminLogsComponent] SignalR connection initiated');
      
      this.signalRService.getConnectionState()
        .pipe(takeUntil(this.destroy$))
        .subscribe(state => {
          console.log('[AdminLogsComponent] Connection state changed:', state);
          this.connectionStatus = state;
        });

      this.signalRService.getApplicationLogs()
        .pipe(takeUntil(this.destroy$))
        .subscribe(log => {
          console.log('[AdminLogsComponent] Received real-time log:', log);
          if (log && this.isRealTimeEnabled) {
            console.log('[AdminLogsComponent] Adding real-time log to display');
            this.addRealTimeLog(log);
          } else {
            console.log('[AdminLogsComponent] Real-time disabled, ignoring log');
          }
        });
    } catch (error) {
      console.error('[AdminLogsComponent] Failed to connect to SignalR:', error);
    }
  }

  private addRealTimeLog(log: ApplicationLog): void {
    // Add to beginning of array
    this.logs.unshift(log);
    this.displayedLogs.unshift(log);
    
    // Limit to 1000 logs in memory
    if (this.logs.length > 1000) {
      this.logs.pop();
      this.displayedLogs.pop();
    }
    
    this.totalLogs++;
    
    // Auto-scroll to top if enabled
    if (this.autoScroll && this.viewport) {
      setTimeout(() => {
        this.viewport.scrollToIndex(0);
      }, 100);
    }
  }

  loadLogs(): void {
    console.log('[AdminLogsComponent] loadLogs - Starting to load logs');
    this.isLoading = true;
    const filter: ApplicationLogFilterDto = {
      ...this.filterForm.value,
      page: this.currentPage,
      pageSize: this.pageSize
    };
    console.log('[AdminLogsComponent] Filter applied:', filter);

    this.logsService.getApplicationLogs(filter)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          console.log('[AdminLogsComponent] Received logs response:', response);
          console.log('[AdminLogsComponent] Response type:', typeof response);
          console.log('[AdminLogsComponent] Response.statusCode:', response?.statusCode);
          console.log('[AdminLogsComponent] Response.data:', response?.data);
          
          if (response && response.statusCode === 200 && response.data) {
            this.logs = response.data.items || response.data;
            this.displayedLogs = [...this.logs];
            this.totalLogs = response.data.totalCount || this.logs.length;
            console.log('[AdminLogsComponent] ✅ Loaded logs count:', this.logs.length);
            console.log('[AdminLogsComponent] ✅ Total logs:', this.totalLogs);
            console.log('[AdminLogsComponent] First 3 logs:', this.logs.slice(0, 3));
            this.extractSources();
          } else {
            console.warn('[AdminLogsComponent] ⚠️ No data in response or response not successful');
            console.log('[AdminLogsComponent] Response statusCode:', response?.statusCode);
            this.logs = [];
            this.displayedLogs = [];
            this.totalLogs = 0;
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('[AdminLogsComponent] ❌ Error loading logs:', error);
          this.isLoading = false;
        }
      });
  }

  private extractSources(): void {
    const sourceSet = new Set(this.logs.map(log => log.source).filter(s => s));
    this.sources = Array.from(sourceSet).sort();
  }

  loadStatistics(): void {
    const startDate = this.filterForm.get('startDate')?.value || new Date(Date.now() - 24 * 60 * 60 * 1000);
    const endDate = this.filterForm.get('endDate')?.value || new Date();

    this.logsService.getLogStatistics(startDate, endDate)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            this.statistics = response.data;
          }
        },
        error: (error) => {
          console.error('Error loading statistics:', error);
        }
      });
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadLogs();
    this.loadStatistics();
  }

  clearFilters(): void {
    this.filterForm.reset({
      logLevel: [],
      source: [],
      startDate: new Date(Date.now() - 24 * 60 * 60 * 1000),
      endDate: new Date(),
      searchTerm: '',
      userId: null,
      operation: '',
      correlationId: ''
    });
    this.applyFilters();
  }

  toggleRealTime(): void {
    this.isRealTimeEnabled = !this.isRealTimeEnabled;
  }

  toggleAutoScroll(): void {
    this.autoScroll = !this.autoScroll;
  }

  refresh(): void {
    this.loadLogs();
    this.loadStatistics();
  }

  scrollToTop(): void {
    if (this.viewport) {
      this.viewport.scrollToIndex(0);
    }
  }

  exportLogs(): void {
    const dataStr = JSON.stringify(this.displayedLogs, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    const url = window.URL.createObjectURL(dataBlob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `logs-${new Date().toISOString()}.json`;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  toggleLogExpansion(logId: number): void {
    this.expandedLogId = this.expandedLogId === logId ? null : logId;
  }

  getLogLevelClass(level: string): string {
    const levelLower = level.toLowerCase();
    if (levelLower.includes('error') || levelLower.includes('critical')) {
      return 'log-error';
    }
    if (levelLower.includes('warning') || levelLower.includes('warn')) {
      return 'log-warning';
    }
    if (levelLower.includes('debug')) {
      return 'log-debug';
    }
    return 'log-info';
  }

  getLogLevelIcon(level: string): string {
    const levelLower = level.toLowerCase();
    if (levelLower.includes('error') || levelLower.includes('critical')) {
      return 'error';
    }
    if (levelLower.includes('warning') || levelLower.includes('warn')) {
      return 'warning';
    }
    if (levelLower.includes('debug')) {
      return 'bug_report';
    }
    return 'info';
  }

  formatTimestamp(timestamp: Date | string): string {
    const date = typeof timestamp === 'string' ? new Date(timestamp) : timestamp;
    return date.toLocaleString();
  }

  formatAdditionalData(data: string | undefined): string {
    if (!data) return '';
    try {
      const parsed = JSON.parse(data);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return data;
    }
  }

  getConnectionStatusText(): string {
    switch (this.connectionStatus) {
      case HubConnectionState.Connected:
        return 'Connected';
      case HubConnectionState.Connecting:
        return 'Connecting...';
      case HubConnectionState.Reconnecting:
        return 'Reconnecting...';
      case HubConnectionState.Disconnected:
        return 'Disconnected';
      default:
        return 'Unknown';
    }
  }

  getConnectionStatusIcon(): string {
    switch (this.connectionStatus) {
      case HubConnectionState.Connected:
        return 'check_circle';
      case HubConnectionState.Connecting:
      case HubConnectionState.Reconnecting:
        return 'sync';
      case HubConnectionState.Disconnected:
        return 'cancel';
      default:
        return 'help';
    }
  }

  getConnectionStatusClass(): string {
    switch (this.connectionStatus) {
      case HubConnectionState.Connected:
        return 'status-connected';
      case HubConnectionState.Connecting:
      case HubConnectionState.Reconnecting:
        return 'status-connecting';
      case HubConnectionState.Disconnected:
        return 'status-disconnected';
      default:
        return '';
    }
  }

  nextPage(): void {
    if (this.currentPage * this.pageSize < this.totalLogs) {
      this.currentPage++;
      this.loadLogs();
      this.scrollToTop();
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadLogs();
      this.scrollToTop();
    }
  }

  get startIndex(): number {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  get endIndex(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalLogs);
  }

  get totalPages(): number {
    return Math.ceil(this.totalLogs / this.pageSize);
  }
}

