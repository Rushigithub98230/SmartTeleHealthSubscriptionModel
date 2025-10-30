import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApplicationLogFilterDto, AuditLogFilterDto } from '../models/logs.model';

@Injectable({
  providedIn: 'root'
})
export class LogsService {
  private apiUrl = 'http://localhost:61376/api/Logs';

  constructor(private http: HttpClient) {}

  getApplicationLogs(filter: ApplicationLogFilterDto): Observable<any> {
    let params = new HttpParams();
    
    if (filter.logLevel && filter.logLevel.length > 0) {
      filter.logLevel.forEach(level => {
        params = params.append('logLevel', level);
      });
    }
    
    if (filter.source && filter.source.length > 0) {
      filter.source.forEach(src => {
        params = params.append('source', src);
      });
    }
    
    if (filter.startDate) {
      params = params.append('startDate', filter.startDate.toISOString());
    }
    
    if (filter.endDate) {
      params = params.append('endDate', filter.endDate.toISOString());
    }
    
    if (filter.userId) {
      params = params.append('userId', filter.userId.toString());
    }
    
    if (filter.operation) {
      params = params.append('operation', filter.operation);
    }
    
    if (filter.correlationId) {
      params = params.append('correlationId', filter.correlationId);
    }
    
    if (filter.searchTerm) {
      params = params.append('searchTerm', filter.searchTerm);
    }
    
    if (filter.page) {
      params = params.append('page', filter.page.toString());
    }
    
    if (filter.pageSize) {
      params = params.append('pageSize', filter.pageSize.toString());
    }
    
    return this.http.get(`${this.apiUrl}/application`, { params }).pipe(
      map((response: any) => response || { isSuccess: false, data: null })
    );
  }

  getAuditLogs(filter: AuditLogFilterDto): Observable<any> {
    let params = new HttpParams();
    
    if (filter.type && filter.type.length > 0) {
      filter.type.forEach(type => {
        params = params.append('type', type);
      });
    }
    
    if (filter.tableName && filter.tableName.length > 0) {
      filter.tableName.forEach(table => {
        params = params.append('tableName', table);
      });
    }
    
    if (filter.startDate) {
      params = params.append('startDate', filter.startDate.toISOString());
    }
    
    if (filter.endDate) {
      params = params.append('endDate', filter.endDate.toISOString());
    }
    
    if (filter.userId) {
      params = params.append('userId', filter.userId.toString());
    }
    
    if (filter.searchTerm) {
      params = params.append('searchTerm', filter.searchTerm);
    }
    
    if (filter.entityId) {
      params = params.append('entityId', filter.entityId);
    }
    
    if (filter.page) {
      params = params.append('page', filter.page.toString());
    }
    
    if (filter.pageSize) {
      params = params.append('pageSize', filter.pageSize.toString());
    }
    
    return this.http.get(`${this.apiUrl}/audit`, { params }).pipe(
      map((response: any) => response || { isSuccess: false, data: null })
    );
  }

  getAvailableTables(): Observable<any> {
    return this.http.get(`${this.apiUrl}/audit/tables`).pipe(
      map((response: any) => response || { isSuccess: false, data: null })
    );
  }

  getAvailableTypes(): Observable<any> {
    return this.http.get(`${this.apiUrl}/audit/types`).pipe(
      map((response: any) => response || { isSuccess: false, data: null })
    );
  }

  getFileLogs(startDate: Date, endDate: Date): Observable<any> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());
    
    return this.http.get(`${this.apiUrl}/file-logs`, { params }).pipe(
      map((response: any) => response || { isSuccess: false, data: null })
    );
  }

  getRecentFileLogs(count: number = 100): Observable<any> {
    const params = new HttpParams().set('count', count.toString());
    return this.http.get(`${this.apiUrl}/file-logs/recent`, { params }).pipe(
      map((response: any) => response || { isSuccess: false, data: null })
    );
  }

  getLogStatistics(startDate: Date, endDate: Date): Observable<any> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());
    
    return this.http.get(`${this.apiUrl}/statistics`, { params }).pipe(
      map((response: any) => response || { isSuccess: false, data: null })
    );
  }

  getLogById(logType: string, id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${logType}/${id}`).pipe(
      map((response: any) => response || { isSuccess: false, data: null })
    );
  }
}

