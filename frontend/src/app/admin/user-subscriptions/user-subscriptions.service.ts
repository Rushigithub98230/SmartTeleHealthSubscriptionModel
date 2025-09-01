import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UserSubscriptionsService {
  private baseUrl = '/webadmin/subscription-management/subscriptions';

  constructor(private http: HttpClient) {}

  getSubscriptions(filters: any): Observable<any> {
    return this.http.get(this.baseUrl, { params: filters });
  }

  pauseSubscription(id: string): Observable<any> {
    return this.http.post(`/webadmin/subscription-management/subscriptions/${id}/pause`, {});
  }

  resumeSubscription(id: string): Observable<any> {
    return this.http.post(`/webadmin/subscription-management/subscriptions/${id}/resume`, {});
  }

  cancelSubscription(id: string): Observable<any> {
    return this.http.post(`/webadmin/subscription-management/subscriptions/${id}/cancel`, {});
  }
}
