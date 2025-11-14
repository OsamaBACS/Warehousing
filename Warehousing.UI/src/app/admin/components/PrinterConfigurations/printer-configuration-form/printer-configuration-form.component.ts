import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PrinterConfigurationService, PrinterConfigurationDto } from '../../../services/printer-configuration.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { LanguageService } from '../../../../core/services/language.service';

@Component({
  selector: 'app-printer-configuration-form',
  standalone: false,
  templateUrl: './printer-configuration-form.component.html',
  styleUrl: './printer-configuration-form.component.scss'
})
export class PrinterConfigurationFormComponent implements OnInit {

  configForm!: FormGroup;
  showPosSettings = false;
  isDefault = false;

  constructor(
    private fb: FormBuilder,
    private printerConfigService: PrinterConfigurationService,
    private router: Router,
    private route: ActivatedRoute,
    private toastr: ToastrService,
    public lang: LanguageService
  ) { }

  ngOnInit(): void {
    this.initForm();
    this.route.queryParams.subscribe(params => {
      const configId = +params['configId'];
      if (configId) {
        this.loadConfiguration(configId);
      }
    });
  }

  initForm(): void {
    this.configForm = this.fb.group({
      id: [0],
      nameAr: ['', Validators.required],
      nameEn: [''],
      description: [''],
      printerType: ['A4', Validators.required],
      paperFormat: ['A4'],
      paperWidth: [210, [Validators.required, Validators.min(1)]],
      paperHeight: [297, [Validators.min(0)]],
      orientation: ['Portrait'],
      printInColor: [true],
      printBackground: [true],
      scale: [1.0, [Validators.required, Validators.min(0.1), Validators.max(2.0)]],
      isActive: [true],
      // Margins (stored as JSON)
      marginTop: ['20mm'],
      marginRight: ['20mm'],
      marginBottom: ['20mm'],
      marginLeft: ['20mm'],
      // Font settings (stored as JSON)
      fontFamily: ['Arial'],
      baseFontSize: [12, [Validators.required, Validators.min(6), Validators.max(72)]],
      headerFontSize: [16, [Validators.required, Validators.min(6), Validators.max(72)]],
      footerFontSize: [10, [Validators.required, Validators.min(6), Validators.max(72)]],
      tableFontSize: [11, [Validators.required, Validators.min(6), Validators.max(72)]],
      // POS settings (stored as JSON)
      posEncoding: ['UTF-8'],
      posCopies: [1, [Validators.min(1), Validators.max(10)]],
      posAutoCut: [true],
      posOpenCashDrawer: [false],
      posPrintDensity: [8, [Validators.min(0), Validators.max(15)]],
      posPrintSpeed: [3, [Validators.min(1), Validators.max(5)]],
      posUseEscPos: [true],
      posConnectionType: ['USB'],
      posConnectionString: ['']
    });

    // Update form when printer type changes
    this.configForm.get('printerType')?.valueChanges.subscribe(type => {
      this.onPrinterTypeChange(type);
    });
  }

  onPrinterTypeChange(printerType: string): void {
    this.showPosSettings = printerType === 'POS' || printerType === 'Thermal';
    
    if (this.showPosSettings) {
      // Apply POS defaults
      this.configForm.patchValue({
        paperFormat: 'Thermal',
        paperWidth: 80,
        paperHeight: 0,
        printInColor: false,
        printBackground: false,
        marginTop: '5mm',
        marginRight: '5mm',
        marginBottom: '5mm',
        marginLeft: '5mm',
        fontFamily: 'Courier',
        baseFontSize: 10,
        headerFontSize: 12,
        footerFontSize: 8,
        tableFontSize: 9
      });
    } else {
      // Apply A4 defaults
      this.configForm.patchValue({
        paperFormat: 'A4',
        paperWidth: 210,
        paperHeight: 297,
        printInColor: true,
        printBackground: true,
        marginTop: '20mm',
        marginRight: '20mm',
        marginBottom: '20mm',
        marginLeft: '20mm',
        fontFamily: 'Arial',
        baseFontSize: 12,
        headerFontSize: 16,
        footerFontSize: 10,
        tableFontSize: 11
      });
    }
  }

  loadConfiguration(configId: number): void {
    this.printerConfigService.GetPrinterConfigurationById(configId).subscribe({
      next: (config) => {
        this.isDefault = config.isDefault;
        
        // Parse JSON fields
        let margins: any = {};
        let fontSettings: any = {};
        let posSettings: any = {};

        if (config.margins) {
          try {
            margins = JSON.parse(config.margins);
          } catch (e) {
            console.error('Error parsing margins:', e);
          }
        }

        if (config.fontSettings) {
          try {
            fontSettings = JSON.parse(config.fontSettings);
          } catch (e) {
            console.error('Error parsing font settings:', e);
          }
        }

        if (config.posSettings) {
          try {
            posSettings = JSON.parse(config.posSettings);
          } catch (e) {
            console.error('Error parsing POS settings:', e);
          }
        }

        this.configForm.patchValue({
          id: config.id,
          nameAr: config.nameAr,
          nameEn: config.nameEn || '',
          description: config.description || '',
          printerType: config.printerType,
          paperFormat: config.paperFormat,
          paperWidth: config.paperWidth,
          paperHeight: config.paperHeight,
          orientation: config.orientation,
          printInColor: config.printInColor,
          printBackground: config.printBackground,
          scale: config.scale,
          isActive: config.isActive,
          marginTop: margins.top || '20mm',
          marginRight: margins.right || '20mm',
          marginBottom: margins.bottom || '20mm',
          marginLeft: margins.left || '20mm',
          fontFamily: fontSettings.fontFamily || 'Arial',
          baseFontSize: fontSettings.baseFontSize || 12,
          headerFontSize: fontSettings.headerFontSize || 16,
          footerFontSize: fontSettings.footerFontSize || 10,
          tableFontSize: fontSettings.tableFontSize || 11,
          posEncoding: posSettings.encoding || 'UTF-8',
          posCopies: posSettings.copies || 1,
          posAutoCut: posSettings.autoCut ?? true,
          posOpenCashDrawer: posSettings.openCashDrawer ?? false,
          posPrintDensity: posSettings.printDensity || 8,
          posPrintSpeed: posSettings.printSpeed || 3,
          posUseEscPos: posSettings.useEscPos ?? true,
          posConnectionType: posSettings.connectionType || 'USB',
          posConnectionString: posSettings.connectionString || ''
        });

        this.showPosSettings = config.printerType === 'POS' || config.printerType === 'Thermal';
      },
      error: (err) => {
        console.error('Error loading printer configuration:', err);
        this.toastr.error('حدث خطأ أثناء تحميل إعدادات الطابعة', 'خطأ');
      }
    });
  }

  save(): void {
    if (this.configForm.valid) {
      const formValue = this.configForm.value;

      // Build JSON strings for complex fields
      const margins = JSON.stringify({
        top: formValue.marginTop,
        right: formValue.marginRight,
        bottom: formValue.marginBottom,
        left: formValue.marginLeft
      });

      const fontSettings = JSON.stringify({
        fontFamily: formValue.fontFamily,
        baseFontSize: formValue.baseFontSize,
        headerFontSize: formValue.headerFontSize,
        footerFontSize: formValue.footerFontSize,
        tableFontSize: formValue.tableFontSize
      });

      let posSettings: string | undefined = undefined;
      if (this.showPosSettings) {
        posSettings = JSON.stringify({
          encoding: formValue.posEncoding,
          copies: formValue.posCopies,
          autoCut: formValue.posAutoCut,
          openCashDrawer: formValue.posOpenCashDrawer,
          printDensity: formValue.posPrintDensity,
          printSpeed: formValue.posPrintSpeed,
          useEscPos: formValue.posUseEscPos,
          connectionType: formValue.posConnectionType,
          connectionString: formValue.posConnectionString || null
        });
      }

      const configDto: PrinterConfigurationDto = {
        id: formValue.id,
        nameAr: formValue.nameAr,
        nameEn: formValue.nameEn,
        description: formValue.description,
        printerType: formValue.printerType,
        paperFormat: formValue.paperFormat,
        paperWidth: formValue.paperWidth,
        paperHeight: formValue.paperHeight,
        margins: margins,
        fontSettings: fontSettings,
        posSettings: posSettings,
        printInColor: formValue.printInColor,
        printBackground: formValue.printBackground,
        orientation: formValue.orientation,
        scale: formValue.scale,
        isActive: formValue.isActive,
        isDefault: this.isDefault // Preserve default status
      };

      this.printerConfigService.SavePrinterConfiguration(configDto).subscribe({
        next: () => {
          this.toastr.success('تم حفظ إعدادات الطابعة بنجاح', 'نجاح');
          this.router.navigate(['../printer-configurations'], { relativeTo: this.route });
        },
        error: (err) => {
          console.error('Error saving printer configuration:', err);
          this.toastr.error(err.error || 'حدث خطأ أثناء حفظ إعدادات الطابعة', 'خطأ');
        }
      });
    } else {
      this.toastr.error('يرجى ملء جميع الحقول المطلوبة', 'خطأ');
    }
  }

  cancel(): void {
    this.router.navigate(['../printer-configurations'], { relativeTo: this.route });
  }

  get isPosPrinter(): boolean {
    return this.showPosSettings;
  }
}

