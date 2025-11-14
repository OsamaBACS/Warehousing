import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface PrinterConfigurationDto {
  id: number;
  nameAr: string;
  nameEn?: string;
  description?: string;
  printerType: string;
  paperFormat: string;
  paperWidth: number;
  paperHeight: number;
  margins?: string;
  fontSettings?: string;
  posSettings?: string;
  printInColor: boolean;
  printBackground: boolean;
  orientation: string;
  scale: number;
  isActive: boolean;
  isDefault: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class PrinterConfigurationService {
  url = environment.baseUrl + '/PrinterConfigurations/';

  constructor(private http: HttpClient) { }

  GetPrinterConfigurations(): Observable<PrinterConfigurationDto[]> {
    return this.http.get<PrinterConfigurationDto[]>(`${this.url}GetPrinterConfigurations`);
  }

  GetPrinterConfigurationById(id: number): Observable<PrinterConfigurationDto> {
    return this.http.get<PrinterConfigurationDto>(`${this.url}GetPrinterConfigurationById?id=${id}`);
  }

  SavePrinterConfiguration(config: PrinterConfigurationDto): Observable<any> {
    return this.http.post(`${this.url}SavePrinterConfiguration`, config);
  }

  DeletePrinterConfiguration(id: number): Observable<any> {
    return this.http.delete(`${this.url}DeletePrinterConfiguration?id=${id}`);
  }
}

