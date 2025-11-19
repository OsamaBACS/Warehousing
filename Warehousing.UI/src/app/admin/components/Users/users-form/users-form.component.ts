import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Role } from '../../../models/role';
import { Store } from '../../../models/store';
import { UsersService } from '../../../services/users.service';
import { RoleService } from '../../../services/role.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LanguageService } from '../../../../core/services/language.service';
import { ToastrService } from 'ngx-toastr';
import { User } from '../../../models/users';
import { PrintHeaderSettings, PrintFooterSettings, defaultPrintHeaderVisibility, defaultPrintFooterVisibility } from '../../../models/PrintSettings';

@Component({
  selector: 'app-users-form',
  standalone: false,
  templateUrl: './users-form.component.html',
  styleUrl: './users-form.component.scss'
})
export class UsersFormComponent implements OnInit {

  userForm!: FormGroup;
  roles: Role[] = [];
  stores: Store[] = [];
  printSettingsExpanded: boolean = false;

  constructor(
    private usersService: UsersService,
    private roleService: RoleService,
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    public lang: LanguageService,
    private toastr: ToastrService
  ) {
    this.stores = this.route.snapshot.data['StoresResolver'];
  }

  ngOnInit(): void {
    this.initializingForm(null);
    this.route.queryParams.subscribe(params => {
      const userId = +params['userId'];
      if (userId) {
        this.usersService.GetUserById(userId)
          .subscribe({
            next: (res) => {
              this.initializingForm(res)
            },
            error: (err) => {
            }
          });
      }
    });
    this.roleService.GetRoles().subscribe(res => {
      this.roles = res;
    });
  }

  initializingForm(user: User | null) {
    // Parse existing print settings (JSON or plain text)
    const headerSettings = this.parsePrintSettings(user?.printHeader || '');
    const footerSettings = this.parsePrintFooterSettings(user?.printFooter || '');

    this.userForm = this.fb.group({
      id: [user?.id || 0],
      nameEn: [user?.nameEn || '', Validators.required],
      nameAr: [user?.nameAr || '', Validators.required],
      phone: [user?.phone || ''],
      email: [user?.email || '', [Validators.required, Validators.email]],
      address: [user?.address || ''],
      username: [user?.username || '', Validators.required],
      passwordHash: [user?.passwordHash || '', user ? [] : [Validators.required]], // Only required on create
      roles: [user?.userRoles?.map(ur => ur.roleId) || []],
      
      // Simple text fields (for backward compatibility)
      printHeader: [user?.printHeader || ''],
      printFooter: [user?.printFooter || ''],
      
      // Advanced structured settings
      printHeaderCustomText: [headerSettings.customText || ''],
      printFooterCustomText: [footerSettings.customText || ''],
      printFooterCustomTerms: [footerSettings.customTerms || ''],
      printFooterCustomNotes: [footerSettings.customNotes || ''],
      
      // Header visibility toggles
      showCompanyName: [headerSettings.visibility?.showCompanyName ?? true],
      showCompanyLogo: [headerSettings.visibility?.showCompanyLogo ?? true],
      showCompanyAddress: [headerSettings.visibility?.showCompanyAddress ?? true],
      showCompanyPhone: [headerSettings.visibility?.showCompanyPhone ?? true],
      showCompanyFax: [headerSettings.visibility?.showCompanyFax ?? true],
      showCompanyEmail: [headerSettings.visibility?.showCompanyEmail ?? true],
      showRegistrationNumber: [headerSettings.visibility?.showRegistrationNumber ?? true],
      showCapital: [headerSettings.visibility?.showCapital ?? true],
      showTaxNumber: [headerSettings.visibility?.showTaxNumber ?? true],
      showSlogan: [headerSettings.visibility?.showSlogan ?? true],
      showDocumentTitle: [headerSettings.visibility?.showDocumentTitle ?? true],
      
      // Footer visibility toggles
      showTerms: [footerSettings.visibility?.showTerms ?? true],
      showNotes: [footerSettings.visibility?.showNotes ?? true],
      showCustomerSignature: [footerSettings.visibility?.showCustomerSignature ?? true],
      showAuthorizedSignature: [footerSettings.visibility?.showAuthorizedSignature ?? true],
      showCompanyFooterNote: [footerSettings.visibility?.showCompanyFooterNote ?? true],
      showDocumentGenerationDate: [footerSettings.visibility?.showDocumentGenerationDate ?? true],
      
      isActive: [user?.isActive ?? true]
    });

    // Detect if settings are JSON (expanded mode) or plain text (simple mode)
    this.printSettingsExpanded = this.isJsonString(user?.printHeader || '') || this.isJsonString(user?.printFooter || '');
  }

  private parsePrintSettings(printHeader: string): PrintHeaderSettings {
    if (!printHeader) {
      return {
        customText: '',
        visibility: defaultPrintHeaderVisibility
      };
    }

    // Try to parse as JSON
    if (this.isJsonString(printHeader)) {
      try {
        const parsed = JSON.parse(printHeader);
        return {
          customText: parsed.customText || '',
          visibility: {
            showCompanyName: parsed.visibility?.showCompanyName ?? true,
            showCompanyLogo: parsed.visibility?.showCompanyLogo ?? true,
            showCompanyAddress: parsed.visibility?.showCompanyAddress ?? true,
            showCompanyPhone: parsed.visibility?.showCompanyPhone ?? true,
            showCompanyFax: parsed.visibility?.showCompanyFax ?? true,
            showCompanyEmail: parsed.visibility?.showCompanyEmail ?? true,
            showRegistrationNumber: parsed.visibility?.showRegistrationNumber ?? true,
            showCapital: parsed.visibility?.showCapital ?? true,
            showTaxNumber: parsed.visibility?.showTaxNumber ?? true,
            showSlogan: parsed.visibility?.showSlogan ?? true,
            showDocumentTitle: parsed.visibility?.showDocumentTitle ?? true
          }
        };
      } catch (e) {
        // If JSON parse fails, treat as plain text
        return {
          customText: printHeader,
          visibility: defaultPrintHeaderVisibility
        };
      }
    }

    // Plain text - treat as customText
    return {
      customText: printHeader,
      visibility: defaultPrintHeaderVisibility
    };
  }

  private parsePrintFooterSettings(printFooter: string): PrintFooterSettings {
    if (!printFooter) {
      return {
        customText: '',
        customTerms: '',
        customNotes: '',
        visibility: defaultPrintFooterVisibility
      };
    }

    // Try to parse as JSON
    if (this.isJsonString(printFooter)) {
      try {
        const parsed = JSON.parse(printFooter);
        return {
          customText: parsed.customText || '',
          customTerms: parsed.customTerms || '',
          customNotes: parsed.customNotes || '',
          visibility: {
            showTerms: parsed.visibility?.showTerms ?? true,
            showNotes: parsed.visibility?.showNotes ?? true,
            showCustomerSignature: parsed.visibility?.showCustomerSignature ?? true,
            showAuthorizedSignature: parsed.visibility?.showAuthorizedSignature ?? true,
            showCompanyFooterNote: parsed.visibility?.showCompanyFooterNote ?? true,
            showDocumentGenerationDate: parsed.visibility?.showDocumentGenerationDate ?? true
          }
        };
      } catch (e) {
        // If JSON parse fails, treat as plain text
        return {
          customText: printFooter,
          customTerms: '',
          customNotes: '',
          visibility: defaultPrintFooterVisibility
        };
      }
    }

    // Plain text - treat as customText
    return {
      customText: printFooter,
      customTerms: '',
      customNotes: '',
      visibility: defaultPrintFooterVisibility
    };
  }

  private isJsonString(str: string): boolean {
    if (!str || str.trim().length === 0) return false;
    const trimmed = str.trim();
    return trimmed.startsWith('{') && trimmed.endsWith('}');
  }

  togglePrintSettingsExpanded(): void {
    this.printSettingsExpanded = !this.printSettingsExpanded;
  }

  onRoleChange(roleId: number, event: Event): void {
    const isChecked = (event.target as HTMLInputElement).checked;
    const currentRoles = [...this.userForm.get('roles')?.value];

    if (isChecked) {
      currentRoles.push(roleId);
    } else {
      const index = currentRoles.indexOf(roleId);
      if (index > -1) {
        currentRoles.splice(index, 1);
      }
    }

    this.userForm.get('roles')?.setValue(currentRoles);
  }

  get isEditMode(): boolean {
    return this.userForm.get('id')?.value > 0;
  }

  save(): void {
    if (this.userForm.valid) {
      // Prepare the data for saving
      const formValue = { ...this.userForm.value };

      // If expanded mode, convert structured settings to JSON
      if (this.printSettingsExpanded) {
        // Build PrintHeaderSettings JSON
        const headerSettings: PrintHeaderSettings = {
          customText: formValue.printHeaderCustomText || null,
          visibility: {
            showCompanyName: formValue.showCompanyName ?? true,
            showCompanyLogo: formValue.showCompanyLogo ?? true,
            showCompanyAddress: formValue.showCompanyAddress ?? true,
            showCompanyPhone: formValue.showCompanyPhone ?? true,
            showCompanyFax: formValue.showCompanyFax ?? true,
            showCompanyEmail: formValue.showCompanyEmail ?? true,
            showRegistrationNumber: formValue.showRegistrationNumber ?? true,
            showCapital: formValue.showCapital ?? true,
            showTaxNumber: formValue.showTaxNumber ?? true,
            showSlogan: formValue.showSlogan ?? true,
            showDocumentTitle: formValue.showDocumentTitle ?? true
          }
        };
        formValue.printHeader = JSON.stringify(headerSettings);

        // Build PrintFooterSettings JSON
        const footerSettings: PrintFooterSettings = {
          customText: formValue.printFooterCustomText || null,
          customTerms: formValue.printFooterCustomTerms || null,
          customNotes: formValue.printFooterCustomNotes || null,
          visibility: {
            showTerms: formValue.showTerms ?? true,
            showNotes: formValue.showNotes ?? true,
            showCustomerSignature: formValue.showCustomerSignature ?? true,
            showAuthorizedSignature: formValue.showAuthorizedSignature ?? true,
            showCompanyFooterNote: formValue.showCompanyFooterNote ?? true,
            showDocumentGenerationDate: formValue.showDocumentGenerationDate ?? true
          }
        };
        formValue.printFooter = JSON.stringify(footerSettings);
      }
      // If simple mode, use the plain text values directly (already in formValue.printHeader and formValue.printFooter)

      // Remove structured fields before sending to API
      delete formValue.printHeaderCustomText;
      delete formValue.printFooterCustomText;
      delete formValue.printFooterCustomTerms;
      delete formValue.printFooterCustomNotes;
      delete formValue.showCompanyName;
      delete formValue.showCompanyLogo;
      delete formValue.showCompanyAddress;
      delete formValue.showCompanyPhone;
      delete formValue.showCompanyFax;
      delete formValue.showCompanyEmail;
      delete formValue.showRegistrationNumber;
      delete formValue.showCapital;
      delete formValue.showTaxNumber;
      delete formValue.showSlogan;
      delete formValue.showDocumentTitle;
      delete formValue.showTerms;
      delete formValue.showNotes;
      delete formValue.showCustomerSignature;
      delete formValue.showAuthorizedSignature;
      delete formValue.showCompanyFooterNote;
      delete formValue.showDocumentGenerationDate;

      this.usersService.SaveUser(formValue).subscribe({
        next: (res) => {
          if (res) {
            this.toastr.success('تم حفظ المستخدم بنجاح', 'المستخدم');
            this.router.navigate(['../users'], { relativeTo: this.route })
          }
          else {
            this.toastr.error('حدث خطأ أثناء الحفظ', 'المستخدم')
          }
        },
        error: (err) => {
          this.toastr.error(err.error || 'حدث خطأ أثناء الحفظ', 'المستخدم')
        }
      });
    }
    else {
      this.toastr.error('يرجى ملء جميع الحقول المطلوبة!', 'المستخدم');
    }
  }

  cancel(): void {
    this.router.navigate(['../users'], { relativeTo: this.route })
  }
}
