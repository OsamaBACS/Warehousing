import { Component, OnInit } from '@angular/core';
import { ActivityLogService, ActivityLogFilters, UserActivityLog, ActivityLogResponse } from '../../services/activity-log.service';
import { NotificationService } from '../../../core/services/notification.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { PermissionsEnum } from '../../constants/enums/permissions.enum';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-activity-logs',
  templateUrl: './activity-logs.component.html',
  styleUrls: ['./activity-logs.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule]
})
export class ActivityLogsComponent implements OnInit {
  // Optional stats placeholder to satisfy template bindings; can be wired later
  stats: any | null = null;
  activityLogs: UserActivityLog[] = [];
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;
  loading = false;
  selectedLog: UserActivityLog | null = null;

  filter: ActivityLogFilters = {
    page: 1,
    pageSize: 20
  };

  // Filter options
  availableActions = ['Login', 'Logout', 'CREATE', 'UPDATE', 'DELETE', 'VIEW', 'CLEAR_LOGS'];
  availableModules = ['Authentication', 'Users', 'Roles', 'Products', 'Categories', 'Orders', 'Inventory', 'WorkingHours', 'WorkingHoursException', 'ActivityLog'];

  permissionsEnum = PermissionsEnum;

  constructor(
    private activityLogService: ActivityLogService,
    private notificationService: NotificationService,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.loadActivityLogs();
  }

  loadActivityLogs(): void {
    this.loading = true;
    this.filter.page = this.currentPage;
    this.filter.pageSize = this.pageSize;

    this.activityLogService.getAllActivityLogs(this.filter).subscribe({
      next: (response: ActivityLogResponse) => {
        // Use the data as-is since the service now handles the mapping
        this.activityLogs = response.data;
        this.totalCount = response.totalCount;
        this.loading = false;
      },
      error: (error: unknown) => {
        console.error('Error loading activity logs:', error);
        this.notificationService.error('Failed to load activity logs');
        this.loading = false;
      }
    });
  }

  hasPermission(permission: string): boolean {
    return this.authService.hasPermission(permission);
  }

  applyFilter(): void {
    this.currentPage = 1;
    this.loadActivityLogs();
  }

  clearFilters(): void {
    this.filter = { page: 1, pageSize: this.pageSize };
    this.currentPage = 1;
    this.loadActivityLogs();
  }

  showLogDetails(log: UserActivityLog): void {
    // Ensure the selected log has all required properties with fallbacks
    this.selectedLog = {
      ...log,
      username: log.username || 'Unknown User',
      action: log.action || 'Unknown',
      module: log.module || 'Unknown',
      ipAddress: log.ipAddress || 'N/A',
      details: log.details || 'No details available',
      timestamp: log.timestamp || new Date().toISOString()
    };
    
    // Open modal using Bootstrap
    const modal = document.getElementById('logDetailsModal');
    if (modal) {
      const bootstrap = (window as any).bootstrap;
      if (bootstrap) {
        const modalInstance = new bootstrap.Modal(modal);
        modalInstance.show();
      }
    }
  }

  closeLogDetails(): void {
    this.selectedLog = null;
  }

  clearOldLogs(): void {
    if (confirm('Are you sure you want to clear activity logs older than 90 days? This action cannot be undone.')) {
      // TODO: Implement clearOldActivityLogs method in service
      this.notificationService.success('Clear old logs functionality will be implemented soon.');
    }
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadActivityLogs();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadActivityLogs();
  }

  deleteLog(log: UserActivityLog): void {
    if (confirm(`Are you sure you want to delete this log entry?`)) {
      // TODO: Implement deleteActivityLog method in service
      this.notificationService.success('Delete log functionality will be implemented soon.');
    }
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleString();
  }

  getActionClass(action: string): string {
    const actionClasses: { [key: string]: string } = {
      'Login': 'badge-success',
      'Logout': 'badge-info',
      'CREATE': 'badge-primary',
      'UPDATE': 'badge-warning',
      'DELETE': 'badge-danger',
      'VIEW': 'badge-secondary',
      'CLEAR_LOGS': 'badge-dark'
    };
    return actionClasses[action] || 'badge-secondary';
  }

  getModuleClass(module: string): string {
    const moduleClasses: { [key: string]: string } = {
      'Authentication': 'text-primary',
      'Users': 'text-success',
      'Roles': 'text-info',
      'Products': 'text-warning',
      'Categories': 'text-danger',
      'Orders': 'text-purple',
      'Inventory': 'text-orange',
      'WorkingHours': 'text-teal',
      'WorkingHoursException': 'text-teal',
      'ActivityLog': 'text-muted'
    };
    return moduleClasses[module] || 'text-muted';
  }

  getPageNumbers(): number[] {
    const totalPages = this.getTotalPages();
    const pages: number[] = [];
    const startPage = Math.max(1, this.currentPage - 2);
    const endPage = Math.min(totalPages, this.currentPage + 2);

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    return pages;
  }

  getTotalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  getUniqueUsers(): number {
    const uniqueUsers = new Set(this.activityLogs.map(log => log.userId));
    return uniqueUsers.size;
  }

  Math = Math; // Make Math available in template

  getCurrentDate(): Date {
    return new Date();
  }

  trackByLogId(index: number, log: UserActivityLog): number {
    return log.id;
  }

  getUserAvatarColor(userId: number): string {
    const colors = [
      '#e74c3c', '#3498db', '#2ecc71', '#f39c12', '#9b59b6',
      '#1abc9c', '#34495e', '#e67e22', '#95a5a6', '#f1c40f'
    ];
    return colors[userId % colors.length];
  }
}