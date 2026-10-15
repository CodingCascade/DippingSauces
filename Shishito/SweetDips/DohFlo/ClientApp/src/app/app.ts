import { Component, OnDestroy, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Subscription } from 'rxjs';
import { DashboardChart } from './chart';
import { DashboardData } from './dashboard.models';
import { DashboardService } from './dashboard.service';

@Component({
  selector: 'app-root', standalone: true,
  imports: [CurrencyPipe, DatePipe, DecimalPipe, FormsModule, DashboardChart],
  templateUrl: './app.html', styleUrl: './app.css'
})
export class App implements OnDestroy {
  private readonly service = inject(DashboardService);
  private request?: Subscription;
  readonly data = signal<DashboardData | null>(null);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly signedOut = signal(false);
  readonly month = signal(this.localMonth());
  readonly currency = signal('USD');
  readonly palette = ['#009da4', '#ffa528', '#ff4253', '#2864ff',
    '#8a35dd', '#54776f', '#b15732', '#537ba2'];
  constructor() { this.load(); }
  ngOnDestroy() { this.request?.unsubscribe(); }
  load() {
    this.request?.unsubscribe();
    this.data.set(null);
    this.error.set(''); this.signedOut.set(false); this.loading.set(true);
    this.request = this.service.get(this.month(), this.currency()).subscribe({
      next: value => { this.data.set(value); this.loading.set(false); },
      error: (e: HttpErrorResponse) => {
        this.loading.set(false); this.signedOut.set(e.status === 401);
        this.error.set(e.status === 401 ? 'Please sign in to view your dashboard.'
          : e.status === 403 ? 'Your account cannot access financial data.'
          : e.status === 422 ? 'Check category types and split totals in your records.'
          : 'The dashboard could not load. Check the server and try again.');
      }
    });
  }
  labels() { return this.data()?.trend.map(t => t.date) ?? []; }
  balanceValues() { return this.data()?.trend.map(t => t.balance) ?? []; }
  incomeValues() { return this.data()?.trend.map(t => t.income) ?? []; }
  expenseValues() { return this.data()?.trend.map(t => t.expenses) ?? []; }
  spendingLabels() { return this.data()?.spending.map(s => s.name) ?? []; }
  spendingValues() { return this.data()?.spending.map(s => s.amount) ?? []; }
  color(index: number) { return this.palette[index % this.palette.length]; }
  spendingColors() { return this.data()?.spending.map((_, i) => this.color(i)) ?? []; }
  icon(kind: string) {
    return kind === 'Income' ? 'bi-currency-dollar'
      : kind === 'Transfer' ? 'bi-arrow-left-right' : 'bi-bag';
  }
  private localMonth() {
    const today = new Date();
    return `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}`;
  }
}
