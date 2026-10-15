export interface Spending { name: string; amount: number; }
export interface Trend {
  date: string; balance: number; income: number; expenses: number;
}
export interface RecentTransaction {
  id: number; date: string; name: string; amount: number;
  signedAmount: number; kind: string;
}
export interface DashboardData {
  month: string; currency: string; recordedBalance: number;
  income: number; expenses: number; spending: Spending[];
  trend: Trend[]; recent: RecentTransaction[]; unclassifiedCount: number;
}
