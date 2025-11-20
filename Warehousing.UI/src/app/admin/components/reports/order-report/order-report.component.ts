import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ReportsService } from '../../../services/reports.service';
import { OrderReportDetail, OrderReportResponse, OrderReportSummary } from '../../../models/order-report';

interface QuickRangeOption {
  key: string;
  label: string;
  days: number;
}

@Component({
  selector: 'app-order-report',
  standalone: false,
  templateUrl: './order-report.component.html',
  styleUrl: './order-report.component.scss'
})
export class OrderReportComponent implements OnInit {

  loading = false;
  errorMessage = '';
  reportData: OrderReportResponse | null = null;

  orderTypes = [
    { id: 2, key: 'REPORTS.SALES_TAB' },
    { id: 1, key: 'REPORTS.PURCHASE_TAB' }
  ];
  selectedOrderTypeId = 2;

  quickRanges: QuickRangeOption[] = [
    { key: 'today', label: 'REPORTS.QUICK_RANGES.TODAY', days: 0 },
    { key: 'seven', label: 'REPORTS.QUICK_RANGES.LAST_7_DAYS', days: 7 },
    { key: 'thirty', label: 'REPORTS.QUICK_RANGES.LAST_30_DAYS', days: 30 }
  ];
  activeQuickRange = 'today';

  filtersForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private reportsService: ReportsService,
  ) {
    this.filtersForm = this.fb.group({
      dateFrom: [this.getToday()],
      dateTo: [this.getToday()]
    });
  }

  ngOnInit(): void {
    this.loadReport();
  }

  getToday(): string {
    return new Date().toISOString().substring(0, 10);
  }

  applyQuickRange(range: QuickRangeOption) {
    const end = new Date();
    const start = new Date();
    if (range.days > 0) {
      start.setDate(end.getDate() - range.days);
    }
    this.filtersForm.patchValue({
      dateFrom: start.toISOString().substring(0, 10),
      dateTo: end.toISOString().substring(0, 10)
    });
    this.activeQuickRange = range.key;
    this.loadReport();
  }

  onOrderTypeChange(orderTypeId: number) {
    this.selectedOrderTypeId = orderTypeId;
    this.loadReport();
  }

  loadReport() {
    if (this.filtersForm.invalid) {
      this.filtersForm.markAllAsTouched();
      return;
    }

    const { dateFrom, dateTo } = this.filtersForm.value;
    this.loading = true;
    this.errorMessage = '';

    this.reportsService.getOrderSummary({
      orderTypeId: this.selectedOrderTypeId,
      dateFrom: dateFrom ? new Date(dateFrom).toISOString() : undefined,
      dateTo: dateTo ? new Date(dateTo).toISOString() : undefined,
      maxRecords: 200
    }).subscribe({
      next: (res) => {
        this.reportData = res;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'REPORTS.ERROR_LOADING';
        this.loading = false;
      }
    });
  }

  get summary(): OrderReportSummary | null {
    return this.reportData?.summary ?? null;
  }

  get details(): OrderReportDetail[] {
    return this.reportData?.orders ?? [];
  }

  trackByOrderId(_: number, item: OrderReportDetail) {
    return item.orderId;
  }
}

