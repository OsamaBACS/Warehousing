import { Injectable } from '@angular/core';
import { LanguageService } from '../../core/services/language.service';

@Injectable({ providedIn: 'root' })
export class PrintService {
  constructor(private lang: LanguageService) { }

  printHtml(content: string): void {
    const direction = this.lang.currentLang === 'ar' ? 'rtl' : 'ltr';
    const printWindow = window.open('', '_blank', 'width=800,height=600');

    if (!printWindow) return;

    printWindow.document.write(`
      <html dir="${direction}">
        <head>
          <title>Print</title>
          <style>
            body {
              font-family: Arial, sans-serif;
              padding: 20px;
              direction: ${direction};
              text-align: ${direction === 'rtl' ? 'right' : 'left'};
              color: #000;
            }
            h4, h2 {
              text-align: center;
              margin-bottom: 1rem;
            }
            table {
              width: 100%;
              border-collapse: collapse;
              margin-top: 15px;
              font-size: 14px;
            }
            th, td {
              border: 1px solid #333;
              padding: 8px;
              vertical-align: top;
            }
            th {
              background-color: #f2f2f2;
            }
            .text-center {
              text-align: center;
            }
            .text-end {
              text-align: right;
            }
            .table-bordered {
              border: 1px solid #000;
            }
            .row {
              display: flex;
              flex-wrap: wrap;
              margin-bottom: 1rem;
            }
            .col-12, .col-md-6 {
              width: 100%;
            }
            @media print {
              @page {
                size: auto;
                margin: 15mm;
              }

              body {
                zoom: 0.9;
              }

              table {
                page-break-inside: avoid;
                font-size: 12px;
              }

              th, td {
                border-color: #000 !important;
                padding: 6px;
              }
              .col-md-6 {
                width: 50%;
                float: left;
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
}