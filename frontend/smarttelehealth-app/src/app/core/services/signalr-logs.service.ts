import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { ApplicationLog, AuditLog } from '../models/logs.model';

@Injectable({
  providedIn: 'root'
})
export class SignalRLogsService {
  private hubConnection!: HubConnection;
  private connectionState = new BehaviorSubject<HubConnectionState>(HubConnectionState.Disconnected);
  private applicationLogs = new BehaviorSubject<ApplicationLog | null>(null);
  private auditLogs = new BehaviorSubject<AuditLog | null>(null);
  private isInitialized = false;

  constructor() {}

  private initializeConnection(): void {
    if (this.isInitialized) return;
    
    const token = localStorage.getItem('token') || '';
    
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`http://localhost:61376/logsHub?access_token=${token}`)
      .withAutomaticReconnect()
      .build();

    this.setupEventHandlers();
    this.isInitialized = true;
  }

  private setupEventHandlers(): void {
    console.log('[SignalRLogsService] Setting up event handlers');
    
    this.hubConnection.on('ReceiveApplicationLog', (log: ApplicationLog) => {
      console.log('[SignalRLogsService] ✅ Received application log:', log);
      this.applicationLogs.next(log);
    });

    this.hubConnection.on('ReceiveAuditLog', (log: AuditLog) => {
      console.log('[SignalRLogsService] ✅ Received audit log:', log);
      this.auditLogs.next(log);
    });

    this.hubConnection.onreconnecting(() => {
      console.warn('[SignalRLogsService] ⚠️ SignalR reconnecting...');
      this.connectionState.next(HubConnectionState.Reconnecting);
    });

    this.hubConnection.onreconnected(() => {
      console.log('[SignalRLogsService] ✅ SignalR reconnected successfully');
      this.connectionState.next(HubConnectionState.Connected);
    });

    this.hubConnection.onclose((error) => {
      console.error('[SignalRLogsService] ❌ SignalR connection closed', error);
      this.connectionState.next(HubConnectionState.Disconnected);
    });
    
    console.log('[SignalRLogsService] Event handlers configured');
  }

  async startConnection(): Promise<void> {
    console.log('[SignalRLogsService] startConnection called');
    this.initializeConnection();
    console.log('[SignalRLogsService] Hub initialized, current state:', this.hubConnection.state);
    
    if (this.hubConnection.state === HubConnectionState.Disconnected) {
      try {
        console.log('[SignalRLogsService] Attempting to start connection...');
        await this.hubConnection.start();
        this.connectionState.next(HubConnectionState.Connected);
        console.log('[SignalRLogsService] ✅ SignalR connection started successfully');
        console.log('[SignalRLogsService] Connection ID:', this.hubConnection.connectionId);
      } catch (err) {
        console.error('[SignalRLogsService] ❌ Error connecting to LogsHub:', err);
        this.connectionState.next(HubConnectionState.Disconnected);
        throw err;
      }
    } else {
      console.log('[SignalRLogsService] Connection already in state:', this.hubConnection.state);
    }
  }

  async stopConnection(): Promise<void> {
    if (this.isInitialized && this.hubConnection.state === HubConnectionState.Connected) {
      try {
        await this.hubConnection.stop();
        this.connectionState.next(HubConnectionState.Disconnected);
        console.log('SignalR connection stopped');
      } catch (err) {
        console.error('Error stopping SignalR connection:', err);
      }
    }
  }

  getApplicationLogs(): Observable<ApplicationLog | null> {
    return this.applicationLogs.asObservable();
  }

  getAuditLogs(): Observable<AuditLog | null> {
    return this.auditLogs.asObservable();
  }

  getConnectionState(): Observable<HubConnectionState> {
    return this.connectionState.asObservable();
  }

  isConnected(): boolean {
    return this.isInitialized && this.hubConnection.state === HubConnectionState.Connected;
  }
}

