import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { CompaniesService } from '../../../services/companies.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { Company } from '../../../models/Company';
import { PrinterConfiguration, parsePrinterConfiguration, serializePrinterConfiguration, defaultA4PrinterConfig, defaultPosPrinterConfig } from '../../../models/PrinterConfiguration';
import { ImageUrlService } from '../../../../shared/services/image-url.service';

@Component({
  selector: 'app-company-form.component',
  standalone: false,
  templateUrl: './company-form.component.html',
  styleUrl: './company-form.component.scss'
})
export class CompanyFormComponent implements OnInit {

  companyForm!: FormGroup;
  printerConfigForm!: FormGroup;
  imageToShow: any;
  initialImageUrl: string | null = null;
  url = environment.resourcesUrl;
  showPrinterSettings = false;
  printerConfig: PrinterConfiguration = defaultA4PrinterConfig;

  constructor(
    private fb: FormBuilder,
    private companiesService: CompaniesService,
    private notification: NotificationService,
    private router: Router,
    private route: ActivatedRoute,
    private imageUrlService: ImageUrlService
  ) {}

  ngOnInit(): void {
    this.initForm(null);

    this.route.queryParams.subscribe(params => {
      const companyId = +params['companyId'];
      if (companyId) {
        this.companiesService.GetCompanyById(companyId).subscribe({
          next: (res) => {
            this.initForm(res);
          },
          error: (err) => {
            console.error(err.error);
          }
        });
      }
    });
  }

  initForm(company: Company | null) {
    this.companyForm = this.fb.group({
      id: [company?.id || 0],
      nameEn: [company?.nameEn || '', Validators.required],
      nameAr: [company?.nameAr || '', Validators.required],
      addressEn: [company?.addressEn || '', Validators.required],
      addressAr: [company?.addressAr || '', Validators.required],
      phone: [company?.phone || '', [Validators.required, Validators.pattern(/^\+?[0-9\s\-]{7,15}$/)]],
      email: [company?.email || '', [Validators.required, Validators.email]],
      website: [company?.website || ''],
      taxNumber: [company?.taxNumber || ''],
      fax: [company?.fax || ''],
      registrationNumber: [company?.registrationNumber || ''],
      capital: [company?.capital || null],
      sloganEn: [company?.sloganEn || ''],
      sloganAr: [company?.sloganAr || ''],
      currencyCode: [company?.currencyCode || '', Validators.required],
      footerNoteEn: [company?.footerNoteEn || ''],
      footerNoteAr: [company?.footerNoteAr || ''],
      termsEn: [company?.termsEn || ''],
      termsAr: [company?.termsAr || ''],
      image: [null],
      printTemplateId: [company?.printTemplateId || null],
      isActive: [company?.isActive ?? true]
    });

    // Initialize printer configuration
    if (company?.printerConfiguration) {
      this.printerConfig = parsePrinterConfiguration(company.printerConfiguration);
    } else {
      this.printerConfig = { ...defaultA4PrinterConfig };
    }
    this.initPrinterConfigForm();

    if (company && company.logoUrl) {
      // Use ImageUrlService to handle both full URLs and relative paths
      this.initialImageUrl = this.imageUrlService.getImageUrl(company.logoUrl, this.url);
      this.detectFiles(this.initialImageUrl);
    }
  }

  initPrinterConfigForm() {
    this.printerConfigForm = this.fb.group({
      printerType: [this.printerConfig.printerType, Validators.required],
      paperFormat: [this.printerConfig.paperFormat],
      paperWidth: [this.printerConfig.paperWidth, [Validators.required, Validators.min(1)]],
      paperHeight: [this.printerConfig.paperHeight, [Validators.min(0)]],
      orientation: [this.printerConfig.orientation],
      printInColor: [this.printerConfig.printInColor],
      printBackground: [this.printerConfig.printBackground],
      scale: [this.printerConfig.scale, [Validators.required, Validators.min(0.1), Validators.max(2.0)]],
      // Margins
      marginTop: [this.printerConfig.margins.top],
      marginRight: [this.printerConfig.margins.right],
      marginBottom: [this.printerConfig.margins.bottom],
      marginLeft: [this.printerConfig.margins.left],
      // Font settings
      fontFamily: [this.printerConfig.fontSettings.fontFamily],
      baseFontSize: [this.printerConfig.fontSettings.baseFontSize, [Validators.required, Validators.min(6), Validators.max(72)]],
      headerFontSize: [this.printerConfig.fontSettings.headerFontSize, [Validators.required, Validators.min(6), Validators.max(72)]],
      footerFontSize: [this.printerConfig.fontSettings.footerFontSize, [Validators.required, Validators.min(6), Validators.max(72)]],
      tableFontSize: [this.printerConfig.fontSettings.tableFontSize, [Validators.required, Validators.min(6), Validators.max(72)]],
      // POS settings
      posEncoding: [this.printerConfig.posSettings?.encoding || 'UTF-8'],
      posCopies: [this.printerConfig.posSettings?.copies || 1, [Validators.min(1), Validators.max(10)]],
      posAutoCut: [this.printerConfig.posSettings?.autoCut ?? true],
      posOpenCashDrawer: [this.printerConfig.posSettings?.openCashDrawer ?? false],
      posPrintDensity: [this.printerConfig.posSettings?.printDensity || 8, [Validators.min(0), Validators.max(15)]],
      posPrintSpeed: [this.printerConfig.posSettings?.printSpeed || 3, [Validators.min(1), Validators.max(5)]],
      posUseEscPos: [this.printerConfig.posSettings?.useEscPos ?? true],
      posConnectionType: [this.printerConfig.posSettings?.connectionType || 'USB'],
      posConnectionString: [this.printerConfig.posSettings?.connectionString || '']
    });

    // Update form when printer type changes
    this.printerConfigForm.get('printerType')?.valueChanges.subscribe(type => {
      this.onPrinterTypeChange(type);
    });
  }

  onPrinterTypeChange(printerType: string) {
    if (printerType === 'POS' || printerType === 'Thermal') {
      // Apply POS defaults
      const posDefaults = defaultPosPrinterConfig;
      this.printerConfigForm.patchValue({
        paperWidth: posDefaults.paperWidth,
        paperHeight: posDefaults.paperHeight,
        printInColor: false,
        printBackground: false,
        marginTop: posDefaults.margins.top,
        marginRight: posDefaults.margins.right,
        marginBottom: posDefaults.margins.bottom,
        marginLeft: posDefaults.margins.left,
        fontFamily: posDefaults.fontSettings.fontFamily,
        baseFontSize: posDefaults.fontSettings.baseFontSize,
        headerFontSize: posDefaults.fontSettings.headerFontSize,
        footerFontSize: posDefaults.fontSettings.footerFontSize,
        tableFontSize: posDefaults.fontSettings.tableFontSize
      });
    } else {
      // Apply A4 defaults
      const a4Defaults = defaultA4PrinterConfig;
      this.printerConfigForm.patchValue({
        paperFormat: a4Defaults.paperFormat,
        paperWidth: a4Defaults.paperWidth,
        paperHeight: a4Defaults.paperHeight,
        printInColor: a4Defaults.printInColor,
        printBackground: a4Defaults.printBackground,
        marginTop: a4Defaults.margins.top,
        marginRight: a4Defaults.margins.right,
        marginBottom: a4Defaults.margins.bottom,
        marginLeft: a4Defaults.margins.left,
        fontFamily: a4Defaults.fontSettings.fontFamily,
        baseFontSize: a4Defaults.fontSettings.baseFontSize,
        headerFontSize: a4Defaults.fontSettings.headerFontSize,
        footerFontSize: a4Defaults.fontSettings.footerFontSize,
        tableFontSize: a4Defaults.fontSettings.tableFontSize
      });
    }
  }

  togglePrinterSettings() {
    this.showPrinterSettings = !this.showPrinterSettings;
  }

  get isPosPrinter(): boolean {
    const type = this.printerConfigForm?.get('printerType')?.value;
    return type === 'POS' || type === 'Thermal';
  }

  detectFiles(fileOrEvent: Event | File | string) {
    let file: File | null = null;

    if (fileOrEvent instanceof File) {
      file = fileOrEvent;
    } else if (
      fileOrEvent instanceof Event &&
      (fileOrEvent.target as HTMLInputElement)?.files?.length
    ) {
      file = (fileOrEvent.target as HTMLInputElement).files![0];
    }

    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imageToShow = e.target.result;
        this.image.setValue(file);
      };
      reader.readAsDataURL(file);
    } else {
      this.imageToShow = fileOrEvent;
      this.image.setValue(fileOrEvent);
    }
  }

  save(): void {
    if (this.companyForm.valid) {
      const formData = new FormData();

      formData.append('id', this.id.value);
      formData.append('nameEn', this.nameEn.value);
      formData.append('nameAr', this.nameAr.value);
      formData.append('addressEn', this.addressEn.value);
      formData.append('addressAr', this.addressAr.value);
      formData.append('phone', this.phone.value);
      formData.append('email', this.email.value);
      formData.append('website', this.website.value);
      formData.append('taxNumber', this.taxNumber.value);
      formData.append('fax', this.fax.value || '');
      formData.append('registrationNumber', this.registrationNumber.value || '');
      formData.append('capital', this.capital.value || '');
      formData.append('sloganEn', this.sloganEn.value || '');
      formData.append('sloganAr', this.sloganAr.value || '');
      formData.append('currencyCode', this.currencyCode.value);
      formData.append('footerNoteEn', this.footerNoteEn.value);
      formData.append('footerNoteAr', this.footerNoteAr.value);
      formData.append('termsEn', this.termsEn.value);
      formData.append('termsAr', this.termsAr.value);

      const file = this.image.value;
      if (file instanceof File) {
        formData.append('image', file, file.name);
      }

      formData.append('printTemplateId', this.printTemplateId.value ? this.printTemplateId.value.toString() : '');
      formData.append('isActive', this.isActive.value);

      // Save printer configuration
      if (this.printerConfigForm.valid) {
        const printerConfig: PrinterConfiguration = {
          printerType: this.printerConfigForm.get('printerType')?.value,
          paperFormat: this.printerConfigForm.get('paperFormat')?.value,
          paperWidth: this.printerConfigForm.get('paperWidth')?.value,
          paperHeight: this.printerConfigForm.get('paperHeight')?.value,
          margins: {
            top: this.printerConfigForm.get('marginTop')?.value,
            right: this.printerConfigForm.get('marginRight')?.value,
            bottom: this.printerConfigForm.get('marginBottom')?.value,
            left: this.printerConfigForm.get('marginLeft')?.value
          },
          fontSettings: {
            fontFamily: this.printerConfigForm.get('fontFamily')?.value,
            baseFontSize: this.printerConfigForm.get('baseFontSize')?.value,
            headerFontSize: this.printerConfigForm.get('headerFontSize')?.value,
            footerFontSize: this.printerConfigForm.get('footerFontSize')?.value,
            tableFontSize: this.printerConfigForm.get('tableFontSize')?.value
          },
          posSettings: this.isPosPrinter ? {
            encoding: this.printerConfigForm.get('posEncoding')?.value,
            copies: this.printerConfigForm.get('posCopies')?.value,
            autoCut: this.printerConfigForm.get('posAutoCut')?.value,
            openCashDrawer: this.printerConfigForm.get('posOpenCashDrawer')?.value,
            printDensity: this.printerConfigForm.get('posPrintDensity')?.value,
            printSpeed: this.printerConfigForm.get('posPrintSpeed')?.value,
            useEscPos: this.printerConfigForm.get('posUseEscPos')?.value,
            connectionType: this.printerConfigForm.get('posConnectionType')?.value,
            connectionString: this.printerConfigForm.get('posConnectionString')?.value || null
          } : undefined,
          printInColor: this.printerConfigForm.get('printInColor')?.value,
          printBackground: this.printerConfigForm.get('printBackground')?.value,
          orientation: this.printerConfigForm.get('orientation')?.value,
          scale: this.printerConfigForm.get('scale')?.value
        };
        formData.append('printerConfiguration', serializePrinterConfiguration(printerConfig));
      }

      this.companiesService.SaveCompany(formData).subscribe({
        next: (res) => {
          this.notification.success('Successfully saved', 'Company');
          this.router.navigate(['../company'], { relativeTo: this.route });
        },
        error: (err) => {
          console.error(err.error);
          this.notification.error(err.error || 'Error while saving', 'Company');
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['../company'], { relativeTo: this.route });
  }

  // Getters for form controls
  get id(): FormControl { return this.companyForm.get('id') as FormControl; }
  get nameEn(): FormControl { return this.companyForm.get('nameEn') as FormControl; }
  get nameAr(): FormControl { return this.companyForm.get('nameAr') as FormControl; }
  get addressEn(): FormControl { return this.companyForm.get('addressEn') as FormControl; }
  get addressAr(): FormControl { return this.companyForm.get('addressAr') as FormControl; }
  get phone(): FormControl { return this.companyForm.get('phone') as FormControl; }
  get email(): FormControl { return this.companyForm.get('email') as FormControl; }
  get website(): FormControl { return this.companyForm.get('website') as FormControl; }
  get taxNumber(): FormControl { return this.companyForm.get('taxNumber') as FormControl; }
  get fax(): FormControl { return this.companyForm.get('fax') as FormControl; }
  get registrationNumber(): FormControl { return this.companyForm.get('registrationNumber') as FormControl; }
  get capital(): FormControl { return this.companyForm.get('capital') as FormControl; }
  get sloganEn(): FormControl { return this.companyForm.get('sloganEn') as FormControl; }
  get sloganAr(): FormControl { return this.companyForm.get('sloganAr') as FormControl; }
  get currencyCode(): FormControl { return this.companyForm.get('currencyCode') as FormControl; }
  get footerNoteEn(): FormControl { return this.companyForm.get('footerNoteEn') as FormControl; }
  get footerNoteAr(): FormControl { return this.companyForm.get('footerNoteAr') as FormControl; }
  get termsEn(): FormControl { return this.companyForm.get('termsEn') as FormControl; }
  get termsAr(): FormControl { return this.companyForm.get('termsAr') as FormControl; }
  get image(): FormControl { return this.companyForm.get('image') as FormControl; }
  get printTemplateId(): FormControl { return this.companyForm.get('printTemplateId') as FormControl; }
  get isActive(): FormControl { return this.companyForm.get('isActive') as FormControl; }
}
