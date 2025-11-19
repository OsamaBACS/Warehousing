import { Component, OnInit } from '@angular/core';
import { DashboardService } from '../../services/dashboard.service';
import { DashboardOverview, DashboardCard, RecentTransaction, TopProduct, StorePerformance, Alert } from '../../models/dashboard.models';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  // Loading states
  isLoading = true;
  isLoadingOverview = true;
  isLoadingTransactions = true;
  isLoadingTopProducts = true;
  isLoadingStorePerformance = true;
  isLoadingAlerts = true;

  // Data properties
  overview: DashboardOverview | null = null;
  recentTransactions: RecentTransaction[] = [];
  topProducts: TopProduct[] = [];
  storePerformance: StorePerformance[] = [];
  alerts: Alert[] = [];

  // Dashboard cards
  dashboardCards: DashboardCard[] = [];

  constructor(private dashboardService: DashboardService) { }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  async loadDashboardData(): Promise<void> {
    this.isLoading = true;
    
    try {
      // Load all dashboard data in parallel
      await Promise.all([
        this.loadOverview(),
        this.loadRecentTransactions(),
        this.loadTopProducts(),
        this.loadStorePerformance(),
        this.loadAlerts()
      ]);
    } catch (error) {
    } finally {
      this.isLoading = false;
    }
  }

  async loadOverview(): Promise<void> {
    try {
      this.overview = await this.dashboardService.getDashboardOverview().toPromise();
      this.createDashboardCards();
    } catch (error) {
    } finally {
      this.isLoadingOverview = false;
    }
  }

  async loadRecentTransactions(): Promise<void> {
    try {
      this.recentTransactions = await this.dashboardService.getRecentTransactions(5).toPromise() || [];
    } catch (error) {
    } finally {
      this.isLoadingTransactions = false;
    }
  }

  async loadTopProducts(): Promise<void> {
    try {
      this.topProducts = await this.dashboardService.getTopProducts(5).toPromise() || [];
    } catch (error) {
    } finally {
      this.isLoadingTopProducts = false;
    }
  }

  async loadStorePerformance(): Promise<void> {
    try {
      this.storePerformance = await this.dashboardService.getStorePerformance().toPromise() || [];
    } catch (error) {
    } finally {
      this.isLoadingStorePerformance = false;
    }
  }

  async loadAlerts(): Promise<void> {
    try {
      this.alerts = await this.dashboardService.getAlerts().toPromise() || [];
    } catch (error) {
    } finally {
      this.isLoadingAlerts = false;
    }
  }

  createDashboardCards(): void {
    if (!this.overview) return;

    this.dashboardCards = [
      {
        title: 'إجمالي المنتجات',
        value: this.overview.totalProducts,
        icon: 'bi bi-box-seam',
        color: 'text-primary',
        subtitle: 'منتج نشط'
      },
      {
        title: 'إجمالي المستودعات',
        value: this.overview.totalStores,
        icon: 'bi bi-building',
        color: 'text-success',
        subtitle: 'مستودع نشط'
      },
      {
        title: 'إجمالي المخزون',
        value: this.overview.totalInventoryQuantity,
        icon: 'bi bi-clipboard-data',
        color: 'text-info',
        subtitle: 'وحدة في المخزون'
      },
      {
        title: 'الطلبات الأخيرة',
        value: this.overview.recentOrders,
        icon: 'bi bi-cart-check',
        color: 'text-warning',
        subtitle: 'طلب في الأسبوع الماضي'
      },
      {
        title: 'منتجات قليلة المخزون',
        value: this.overview.lowStockProducts,
        icon: 'bi bi-exclamation-triangle',
        color: 'text-warning',
        subtitle: 'تحتاج إعادة تموين'
      },
      {
        title: 'منتجات نافدة المخزون',
        value: this.overview.zeroStockProducts,
        icon: 'bi bi-x-circle',
        color: 'text-danger',
        subtitle: 'خارج المخزون'
      }
    ];
  }

  getSeverityClass(severity: string): string {
    switch (severity) {
      case 'High':
        return 'text-danger';
      case 'Medium':
        return 'text-warning';
      case 'Low':
        return 'text-info';
      default:
        return 'text-secondary';
    }
  }

  getSeverityIcon(severity: string): string {
    switch (severity) {
      case 'High':
        return 'bi bi-exclamation-circle-fill';
      case 'Medium':
        return 'bi bi-exclamation-triangle-fill';
      case 'Low':
        return 'bi bi-info-circle-fill';
      default:
        return 'bi bi-circle-fill';
    }
  }

  getSeverityBadgeClass(severity: string): string {
    switch (severity) {
      case 'High':
        return 'bg-red-100 text-red-800';
      case 'Medium':
        return 'bg-yellow-100 text-yellow-800';
      case 'Low':
        return 'bg-blue-100 text-blue-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-JO', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      calendar: 'gregory'
    });
  }

  formatNumber(value: number): string {
    return new Intl.NumberFormat('en-JO').format(value);
  }
}
