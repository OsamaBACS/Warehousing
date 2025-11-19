import { Component, OnInit } from '@angular/core';
import { PrinterConfigurationService, PrinterConfigurationDto } from '../../../services/printer-configuration.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LanguageService } from '../../../../core/services/language.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-printer-configurations',
  standalone: false,
  templateUrl: './printer-configurations.component.html',
  styleUrl: './printer-configurations.component.scss'
})
export class PrinterConfigurationsComponent implements OnInit {

  configurations: PrinterConfigurationDto[] = [];
  loading = false;

  constructor(
    private printerConfigService: PrinterConfigurationService,
    private router: Router,
    private route: ActivatedRoute,
    public lang: LanguageService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.loadConfigurations();
  }

  loadConfigurations(): void {
    this.loading = true;
    this.printerConfigService.GetPrinterConfigurations().subscribe({
      next: (configs) => {
        this.configurations = configs;
        this.loading = false;
      },
      error: (err) => {
        this.toastr.error('حدث خطأ أثناء تحميل إعدادات الطابعة', 'خطأ');
        this.loading = false;
      }
    });
  }

  openForm(configId: number | null): void {
    if (configId) {
      this.router.navigate(['../printer-configuration-form'], { relativeTo: this.route, queryParams: { configId } });
    } else {
      this.router.navigate(['../printer-configuration-form'], { relativeTo: this.route });
    }
  }

  deleteConfiguration(config: PrinterConfigurationDto): void {
    if (config.isDefault) {
      this.toastr.warning('لا يمكن حذف إعدادات الطابعة الافتراضية', 'تحذير');
      return;
    }

    if (confirm(`هل أنت متأكد من حذف إعدادات الطابعة "${config.nameAr}"؟`)) {
      this.printerConfigService.DeletePrinterConfiguration(config.id).subscribe({
        next: () => {
          this.toastr.success('تم حذف إعدادات الطابعة بنجاح', 'نجاح');
          this.loadConfigurations();
        },
        error: (err) => {
          this.toastr.error(err.error || 'حدث خطأ أثناء حذف إعدادات الطابعة', 'خطأ');
        }
      });
    }
  }

  getPrinterTypeLabel(type: string): string {
    const labels: { [key: string]: string } = {
      'A4': 'طابعة A4',
      'POS': 'طابعة نقاط البيع',
      'Thermal': 'طابعة حرارية',
      'Label': 'طابعة ملصقات'
    };
    return labels[type] || type;
  }

  getPrinterTypeColor(type: string): string {
    const colors: { [key: string]: string } = {
      'A4': 'bg-blue-100 text-blue-800',
      'POS': 'bg-green-100 text-green-800',
      'Thermal': 'bg-orange-100 text-orange-800',
      'Label': 'bg-purple-100 text-purple-800'
    };
    return colors[type] || 'bg-gray-100 text-gray-800';
  }
}

