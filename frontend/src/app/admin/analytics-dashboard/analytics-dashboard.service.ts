import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AnalyticsDashboardService {
  private baseUrl = '/api/admin/AdminSubscription';

  constructor(private http: HttpClient) {}

  getSummary(): Observable<any> {
    return this.http.get(`${this.baseUrl}/summary`);
  }

  getRevenue(): Observable<any> {
    return this.http.get(`${this.baseUrl}/revenue-metrics`);
  }

  getChurn(): Observable<any> {
    return this.http.get(`${this.baseUrl}/churn-analysis`);
  }

  getPlanPerformance(): Observable<any> {
    return this.http.get(`${this.baseUrl}/plan-performance`);
  }

  exportReport(format: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/export`, { params: { format } });
  }
}
