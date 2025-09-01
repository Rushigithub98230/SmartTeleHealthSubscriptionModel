import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ManualActionsService {
  private baseUrl = '/webadmin/subscription-management/subscriptions';

  constructor(private http: HttpClient) {}

  pause(id: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/pause`, {});
  }

  resume(id: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/resume`, {});
  }

  cancel(id: string, reason?: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/cancel`, { reason });
  }

  upgrade(id: string, newPlanId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/upgrade`, { newPlanId });
  }

  refund(id: string, amount: number, reason?: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/refund`, { amount, reason });
  }
}
