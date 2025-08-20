import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { CompaniesService } from '../../../services/companies.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { Company } from '../../../models/Company';

@Component({
  selector: 'app-company-form.component',
  standalone: false,
  templateUrl: './company-form.component.html',
  styleUrl: './company-form.component.scss'
})
export class CompanyFormComponent implements OnInit {

  companyForm!: FormGroup;
  imageToShow: any;
  initialImageUrl: string | null = null;
  url = environment.resourcesUrl;

  constructor(
    private fb: FormBuilder,
    private companiesService: CompaniesService,
    private notification: NotificationService,
    private router: Router,
    private route: ActivatedRoute
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
      currencyCode: [company?.currencyCode || '', Validators.required],
      footerNoteEn: [company?.footerNoteEn || ''],
      footerNoteAr: [company?.footerNoteAr || ''],
      termsEn: [company?.termsEn || ''],
      termsAr: [company?.termsAr || ''],
      image: [null],
      printTemplateId: [company?.printTemplateId || null],
      isActive: [company?.isActive ?? true]
    });

    if (company && company.logoUrl) {
      this.initialImageUrl = this.url + company.logoUrl;
      this.detectFiles(this.initialImageUrl);
    }
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
  get currencyCode(): FormControl { return this.companyForm.get('currencyCode') as FormControl; }
  get footerNoteEn(): FormControl { return this.companyForm.get('footerNoteEn') as FormControl; }
  get footerNoteAr(): FormControl { return this.companyForm.get('footerNoteAr') as FormControl; }
  get termsEn(): FormControl { return this.companyForm.get('termsEn') as FormControl; }
  get termsAr(): FormControl { return this.companyForm.get('termsAr') as FormControl; }
  get image(): FormControl { return this.companyForm.get('image') as FormControl; }
  get printTemplateId(): FormControl { return this.companyForm.get('printTemplateId') as FormControl; }
  get isActive(): FormControl { return this.companyForm.get('isActive') as FormControl; }
}
