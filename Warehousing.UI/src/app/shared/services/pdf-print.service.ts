import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

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

  constructor(private http: HttpClient) {}

  /**
   * Generate PDF from HTML content
   */
  generatePDF(htmlContent: string, title: string, type: 'document' | 'order' | 'report' = 'document'): Observable<Blob> {
    const request: PrintRequest = {
      htmlContent,
      title,
      type
    };

    return this.http.post(`${this.baseUrl}/generate-pdf`, request, {
      responseType: 'blob'
    });
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
   * Print PDF in new window
   */
  async printPDF(htmlContent: string, title: string, type: 'document' | 'order' | 'report' = 'document'): Promise<void> {
    try {
      const pdfBlob = await this.generatePDF(htmlContent, title, type).toPromise();
      if (pdfBlob) {
        const url = URL.createObjectURL(pdfBlob);
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
      console.error('Error printing PDF:', error);
      throw error;
    }
  }

  /**
   * Download PDF file
   */
  async downloadPDF(htmlContent: string, title: string, type: 'document' | 'order' | 'report' = 'document'): Promise<void> {
    try {
      const pdfBlob = await this.generatePDF(htmlContent, title, type).toPromise();
      if (pdfBlob) {
        const url = URL.createObjectURL(pdfBlob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${title}.pdf`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
      }
    } catch (error) {
      console.error('Error downloading PDF:', error);
      throw error;
    }
  }

  /**
   * Print order PDF
   */
  async printOrderPDF(orderData: OrderPrintRequest): Promise<void> {
    try {
      const pdfBlob = await this.generateOrderPDF(orderData).toPromise();
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
      console.error('Error printing order PDF:', error);
      throw error;
    }
  }

  /**
   * Download order PDF
   */
  async downloadOrderPDF(orderData: OrderPrintRequest): Promise<void> {
    try {
      const pdfBlob = await this.generateOrderPDF(orderData).toPromise();
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
      console.error('Error downloading order PDF:', error);
      throw error;
    }
  }

  /**
   * Print report PDF
   */
  async printReportPDF(reportData: ReportPrintRequest): Promise<void> {
    try {
      const pdfBlob = await this.generateReportPDF(reportData).toPromise();
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
      console.error('Error printing report PDF:', error);
      throw error;
    }
  }

  /**
   * Download report PDF
   */
  async downloadReportPDF(reportData: ReportPrintRequest): Promise<void> {
    try {
      const pdfBlob = await this.generateReportPDF(reportData).toPromise();
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
      console.error('Error downloading report PDF:', error);
      throw error;
    }
  }

  /**
   * Check if PDF service is available
   */
  async isServiceAvailable(): Promise<boolean> {
    try {
      // Try to make a simple request to check if the service is available
      await this.http.get(`${this.baseUrl}/health`).toPromise();
      return true;
    } catch (error) {
      return false;
    }
  }
}
