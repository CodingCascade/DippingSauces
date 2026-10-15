import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DashboardData } from './dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);
  get(month: string, currency: string) {
    return this.http.get<DashboardData>('/api/dashboard', {
      params: { month, currency }
    });
  }
}
