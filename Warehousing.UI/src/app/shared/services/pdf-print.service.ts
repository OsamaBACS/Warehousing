import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of, firstValueFrom } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { CompaniesService } from '../../admin/services/companies.service';
import { PrinterConfiguration, parsePrinterConfiguration } from '../../admin/models/PrinterConfiguration';

export interface PrintRequest {
  htmlContent: string;
  title: string;
  type: 'document' | 'order' | 'report';
}

export interface OrderPrintRequest {
  orderId: number;
  orderDate: Date;
  customerName: string;
  totalAmount: number;
  items: OrderItemDto[];
}

export interface OrderItemDto {
  productName: string;
  quantity: number;
  unitPrice: number;
  total: number;
}

export interface ReportPrintRequest {
  reportType: string;
  startDate: Date;
  endDate: Date;
  parameters: { [key: string]: any };
}

@Injectable({
  providedIn: 'root'
})
export class PdfPrintService {
  private readonly baseUrl = `${environment.baseUrl}/print`;
  private printerConfig: PrinterConfiguration | null = null;

  constructor(
    private http: HttpClient,
    private companiesService: CompaniesService
  ) {
    this.loadPrinterConfiguration();
  }

  /**
   * Load printer configuration from company settings
   */
  private loadPrinterConfiguration(): void {
    this.companiesService.GetCompanies().subscribe({
      next: (company) => {
        if (company?.printerConfiguration) {
          this.printerConfig = parsePrinterConfiguration(company.printerConfiguration);
        }
      },
      error: (err) => {
      }
    });
  }

  /**
   * Get current printer configuration
   */
  getPrinterConfiguration(): PrinterConfiguration | null {
    return this.printerConfig;
  }

  /**
   * Check if current printer is POS/Thermal
   */
  isPosPrinter(): boolean {
    return this.printerConfig?.printerType === 'POS' || this.printerConfig?.printerType === 'Thermal';
  }

  /**
   * Generate PDF or ESC/POS from HTML content
   */
  generatePDF(htmlContent: string, title: string, type: 'document' | 'order' | 'report' = 'document'): Observable<Blob> {
    const request: PrintRequest = {
      htmlContent,
      title,
      type
    };

    return this.http.post(`${this.baseUrl}/generate-pdf`, request, {
      responseType: 'blob',
      observe: 'response'
    }).pipe(
      map(response => {
        // Check content type to determine if it's PDF or ESC/POS
        const contentType = response.headers.get('content-type') || '';
        const blob = response.body!;
        
        // Store content type in blob for later use
        (blob as any).contentType = contentType;
        (blob as any).isEscPos = contentType.includes('octet-stream') || contentType.includes('escpos');
        
        return blob;
      }),
      catchError(error => {
        throw error;
      })
    );
  }

  /**
   * Generate PDF for order
   */
  generateOrderPDF(orderData: OrderPrintRequest): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/generate-order-pdf`, orderData, {
      responseType: 'blob'
    });
  }

  /**
   * Generate PDF for report
   */
  generateReportPDF(reportData: ReportPrintRequest): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/generate-report-pdf`, reportData, {
      responseType: 'blob'
    });
  }

  /**
   * Print PDF or send ESC/POS to printer
   */
  async printPDF(htmlContent: string, title: string, type: 'document' | 'order' | 'report' = 'document'): Promise<void> {
    try {
      const blob = await firstValueFrom(this.generatePDF(htmlContent, title, type));
      if (!blob) {
        throw new Error('No print output generated');
      }

      // Check if this is ESC/POS output
      const isEscPos = (blob as any).isEscPos || this.isPosPrinter();
      
      if (isEscPos) {
        // For ESC/POS, send directly to printer or download
        await this.sendEscPosToPrinter(blob, title);
      } else {
        // For PDF, open in print dialog
        const url = URL.createObjectURL(blob);
        const printWindow = window.open(url, '_blank');
        
        if (printWindow) {
          printWindow.onload = () => {
            printWindow.print();
          };
        }
        
        // Clean up URL after a delay
        setTimeout(() => {
          URL.revokeObjectURL(url);
        }, 10000);
      }
    } catch (error) {
      throw error;
    }
  }

  /**
   * Send ESC/POS commands to printer
   */
  private async sendEscPosToPrinter(blob: Blob, title: string): Promise<void> {
    try {
      // Convert blob to array buffer
      const arrayBuffer = await blob.arrayBuffer();
      const bytes = new Uint8Array(arrayBuffer);

      // Try to send to printer using WebUSB API (if available)
      if ('usb' in navigator) {
        try {
          // Request access to USB device
          const device = await (navigator as any).usb.requestDevice({
            filters: [{ classCode: 7 }] // Printer class
          });
          
          await device.open();
          await device.selectConfiguration(1);
          await device.claimInterface(0);
          await device.transferOut(1, bytes);
          await device.close();
          
          return;
        } catch (usbError) {
        }
      }

      // Fallback: Download the file for manual printing
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `${title}.escpos`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
      
      // Show message to user
      alert('تم تحميل ملف ESC/POS. يرجى إرساله إلى الطابعة يدوياً أو استخدام برنامج الطباعة المخصص.');
    } catch (error) {
      throw error;
    }
  }

  /**
   * Download PDF or ESC/POS file
   */
  async downloadPDF(htmlContent: string, title: string, type: 'document' | 'order' | 'report' = 'document'): Promise<void> {
    try {
      const blob = await firstValueFrom(this.generatePDF(htmlContent, title, type));
      if (!blob) {
        throw new Error('No print output generated');
      }

      const isEscPos = (blob as any).isEscPos || this.isPosPrinter();
      const extension = isEscPos ? 'escpos' : 'pdf';
      const mimeType = isEscPos ? 'application/octet-stream' : 'application/pdf';

      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `${title}.${extension}`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
    } catch (error) {
      throw error;
    }
  }

  /**
   * Print order PDF
   */
  async printOrderPDF(orderData: OrderPrintRequest): Promise<void> {
    try {
      const pdfBlob = await firstValueFrom(this.generateOrderPDF(orderData));
      if (pdfBlob) {
        const url = URL.createObjectURL(pdfBlob);
        const printWindow = window.open(url, '_blank');
        
        if (printWindow) {
          printWindow.onload = () => {
            printWindow.print();
          };
        }
        
        setTimeout(() => {
          URL.revokeObjectURL(url);
        }, 10000);
      }
    } catch (error) {
      throw error;
    }
  }

  /**
   * Download order PDF
   */
  async downloadOrderPDF(orderData: OrderPrintRequest): Promise<void> {
    try {
      const pdfBlob = await firstValueFrom(this.generateOrderPDF(orderData));
      if (pdfBlob) {
        const url = URL.createObjectURL(pdfBlob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Order-${orderData.orderId}.pdf`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
      }
    } catch (error) {
      throw error;
    }
  }

  /**
   * Print report PDF
   */
  async printReportPDF(reportData: ReportPrintRequest): Promise<void> {
    try {
      const pdfBlob = await firstValueFrom(this.generateReportPDF(reportData));
      if (pdfBlob) {
        const url = URL.createObjectURL(pdfBlob);
        const printWindow = window.open(url, '_blank');
        
        if (printWindow) {
          printWindow.onload = () => {
            printWindow.print();
          };
        }
        
        setTimeout(() => {
          URL.revokeObjectURL(url);
        }, 10000);
      }
    } catch (error) {
      throw error;
    }
  }

  /**
   * Download report PDF
   */
  async downloadReportPDF(reportData: ReportPrintRequest): Promise<void> {
    try {
      const pdfBlob = await firstValueFrom(this.generateReportPDF(reportData));
      if (pdfBlob) {
        const url = URL.createObjectURL(pdfBlob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${reportData.reportType}.pdf`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
      }
    } catch (error) {
      throw error;
    }
  }

  /**
   * Check if PDF service is available
   */
  async isServiceAvailable(): Promise<boolean> {
    try {
      // Try to make a simple request to check if the service is available
      await firstValueFrom(this.http.get(`${this.baseUrl}/health`));
      return true;
    } catch (error) {
      return false;
    }
  }
}
