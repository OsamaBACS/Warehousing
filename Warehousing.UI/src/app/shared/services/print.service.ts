import { Injectable } from '@angular/core';
import { LanguageService } from '../../core/services/language.service';

@Injectable({ providedIn: 'root' })
export class PrintService {
  constructor(private lang: LanguageService) { }

  printHtml(content: string, title: string = 'Print Document'): void {
    const direction = this.lang.currentLang === 'ar' ? 'rtl' : 'ltr';
    const printWindow = window.open('', '_blank', 'width=800,height=600');

    if (!printWindow) return;

    printWindow.document.write(`
      <html dir="${direction}">
        <head>
          <title>${title}</title>
          <meta charset="UTF-8">
          <style>
            * {
              box-sizing: border-box;
            }
            
            body {
              font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
              padding: 20px;
              direction: ${direction};
              text-align: ${direction === 'rtl' ? 'right' : 'left'};
              color: #000;
              background: white;
              margin: 0;
              line-height: 1.6;
            }
            
            .print-header {
              text-align: center;
              margin-bottom: 30px;
              border-bottom: 2px solid #333;
              padding-bottom: 20px;
            }
            
            .print-header h1 {
              font-size: 28px;
              margin: 0 0 10px 0;
              color: #2c3e50;
            }
            
            .print-header h2 {
              font-size: 20px;
              margin: 0;
              color: #7f8c8d;
              font-weight: normal;
            }
            
            .print-info {
              display: flex;
              justify-content: space-between;
              margin-bottom: 30px;
              flex-wrap: wrap;
            }
            
            .print-info-item {
              background: #f8f9fa;
              border: 1px solid #dee2e6;
              border-radius: 8px;
              padding: 15px;
              margin: 5px;
              flex: 1;
              min-width: 200px;
            }
            
            .print-info-item strong {
              color: #495057;
              display: block;
              margin-bottom: 5px;
              font-size: 14px;
            }
            
            .print-info-item span {
              color: #212529;
              font-size: 16px;
            }
            
            table {
              width: 100%;
              border-collapse: collapse;
              margin: 20px 0;
              font-size: 14px;
              box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            }
            
            th {
              background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
              color: white;
              padding: 15px 12px;
              text-align: ${direction === 'rtl' ? 'right' : 'left'};
              font-weight: 600;
              font-size: 14px;
              border: 1px solid #5a6fd8;
            }
            
            td {
              padding: 12px;
              border: 1px solid #dee2e6;
              vertical-align: top;
              background: white;
            }
            
            tr:nth-child(even) td {
              background: #f8f9fa;
            }
            
            tr:hover td {
              background: #e3f2fd;
            }
            
            .text-center {
              text-align: center;
            }
            
            .text-end {
              text-align: ${direction === 'rtl' ? 'right' : 'left'};
            }
            
            .text-start {
              text-align: ${direction === 'rtl' ? 'left' : 'right'};
            }
            
            .print-total {
              background: #e8f5e8;
              border: 2px solid #28a745;
              border-radius: 8px;
              padding: 20px;
              margin: 30px 0;
              text-align: center;
            }
            
            .print-total h3 {
              margin: 0 0 10px 0;
              color: #155724;
              font-size: 18px;
            }
            
            .print-total .amount {
              font-size: 32px;
              font-weight: bold;
              color: #28a745;
            }
            
            .print-footer {
              margin-top: 40px;
              padding-top: 20px;
              border-top: 1px solid #dee2e6;
              text-align: center;
              color: #6c757d;
              font-size: 12px;
            }
            
            .row {
              display: flex;
              flex-wrap: wrap;
              margin: 0 -10px;
            }
            
            .col-12, .col-md-6 {
              padding: 0 10px;
              margin-bottom: 15px;
            }
            
            .col-12 {
              width: 100%;
            }
            
            .col-md-6 {
              width: 50%;
            }
            
            .badge {
              display: inline-block;
              padding: 4px 8px;
              font-size: 12px;
              font-weight: 500;
              line-height: 1;
              text-align: center;
              white-space: nowrap;
              vertical-align: baseline;
              border-radius: 4px;
            }
            
            .badge-success {
              color: #fff;
              background-color: #28a745;
            }
            
            .badge-danger {
              color: #fff;
              background-color: #dc3545;
            }
            
            .badge-warning {
              color: #212529;
              background-color: #ffc107;
            }
            
            .badge-info {
              color: #fff;
              background-color: #17a2b8;
            }
            
            @media print {
              @page {
                size: A4;
                margin: 15mm;
              }

              body {
                -webkit-print-color-adjust: exact;
                color-adjust: exact;
                zoom: 0.95;
              }

              .print-header {
                page-break-after: avoid;
              }

              table {
                page-break-inside: avoid;
                font-size: 12px;
              }

              th, td {
                border-color: #000 !important;
                padding: 8px 6px;
              }
              
              .col-md-6 {
                width: 50%;
                float: left;
              }
              
              .print-total {
                page-break-before: avoid;
              }
              
              .print-footer {
                page-break-before: avoid;
              }
            }
            
            @media screen {
              .print-info {
                display: flex;
              }
            }
            
            @media screen and (max-width: 768px) {
              .print-info {
                flex-direction: column;
              }
              
              .col-md-6 {
                width: 100%;
              }
            }
          </style>
        </head>
        <body onload="window.print();">
          ${content}
        </body>
      </html>
    `);

    printWindow.document.close();
  }

  // Enhanced method for printing with custom options
  printWithOptions(content: string, options: PrintOptions = {}): void {
    const {
      title = 'Print Document',
      showPrintDialog = true,
      printImmediately = true
    } = options;

    const direction = this.lang.currentLang === 'ar' ? 'rtl' : 'ltr';
    const printWindow = window.open('', '_blank', 'width=800,height=600');

    if (!printWindow) return;

    printWindow.document.write(`
      <html dir="${direction}">
        <head>
          <title>${title}</title>
          <meta charset="UTF-8">
          <style>
            /* Same styles as above */
            * { box-sizing: border-box; }
            body {
              font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
              padding: 20px;
              direction: ${direction};
              text-align: ${direction === 'rtl' ? 'right' : 'left'};
              color: #000;
              background: white;
              margin: 0;
              line-height: 1.6;
            }
            /* ... (same styles as above) ... */
          </style>
        </head>
        <body ${printImmediately ? 'onload="window.print();"' : ''}>
          ${content}
        </body>
      </html>
    `);

    printWindow.document.close();
    
    if (showPrintDialog && printImmediately) {
      printWindow.focus();
    }
  }

  // Method to generate PDF-ready HTML (for future PDF generation)
  generatePDFReadyHTML(content: string, title: string = 'Document'): string {
    const direction = this.lang.currentLang === 'ar' ? 'rtl' : 'ltr';
    
    return `
      <!DOCTYPE html>
      <html dir="${direction}">
        <head>
          <title>${title}</title>
          <meta charset="UTF-8">
          <style>
            /* PDF-optimized styles */
            body {
              font-family: 'DejaVu Sans', Arial, sans-serif;
              margin: 0;
              padding: 20px;
              color: #000;
              direction: ${direction};
            }
            /* ... (same styles as above but optimized for PDF) ... */
          </style>
        </head>
        <body>
          ${content}
        </body>
      </html>
    `;
  }
}

export interface PrintOptions {
  title?: string;
  showPrintDialog?: boolean;
  printImmediately?: boolean;
}