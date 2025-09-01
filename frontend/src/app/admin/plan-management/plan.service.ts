import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class PlanService {
  private baseUrl = 'http://localhost:61376/api/subscriptions/admin/plans';

  constructor(private http: HttpClient) {}

  getPlans(): Observable<any> {
    return this.http.get(this.baseUrl);
  }

  createPlan(data: any): Observable<any> {
    return this.http.post(this.baseUrl, data);
  }

  updatePlan(id: string, data: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/${id}`, data);
  }

  deletePlan(id: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }

  activatePlan(id: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/activate`, {});
  }

  deactivatePlan(id: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/deactivate`, {});
  }
}
