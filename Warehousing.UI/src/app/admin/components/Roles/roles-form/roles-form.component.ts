import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Permission } from '../../../models/permission';
import { Category } from '../../../models/category';
import { Product } from '../../../models/product';
import { RoleService } from '../../../services/role.service';
import { PermissionService } from '../../../services/permission.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LanguageService } from '../../../../core/services/language.service';
import { ToastrService } from 'ngx-toastr';
import { RoleDtoForAdd } from '../../../models/role';

@Component({
  selector: 'app-roles-form',
  standalone: false,
  templateUrl: './roles-form.component.html',
  styleUrl: './roles-form.component.scss'
})
export class RolesFormComponent implements OnInit {

  roleForm!: FormGroup;
  permissions!: Permission[];
  categories!: Category[];
  products!: Product[];

  constructor(
    private fb: FormBuilder,
    private roleService: RoleService,
    private permissionService: PermissionService,
    private router: Router,
    private route: ActivatedRoute,
    public lang: LanguageService,
    private toastr: ToastrService
  ) {
    this.categories = this.route.snapshot.data['categoriesResolver'];
    this.products = this.route.snapshot.data['productsResolver'];
    this.permissions = this.route.snapshot.data['permissionsResolver'];
  }

  ngOnInit(): void {
    this.initializingForm(null);
    this.route.queryParams.subscribe(params => {
      const roleId = +params['roleId'];
      if (roleId) {
        this.roleService.GetRoleById(roleId)
          .subscribe({
            next: (res) => {
              const roleDtoForAdd: RoleDtoForAdd = {
                id: res.id,
                code: res.code,
                nameAr: res.nameAr,
                nameEn: res.nameEn,
                rolePermissionIds: res?.permissions.map(p => p.permissionId),
                categoryIds: res?.categoryIds || [],
                productIds: res?.productIds || []
              };
              this.initializingForm(roleDtoForAdd)
            },
            error: (err) => {
              console.log(err.error);
            }
          });
      }
    });
  }

  initializingForm(role: RoleDtoForAdd | null) {
    this.roleForm = this.fb.group({
      id: [role?.id || 0],
      code: [role?.code || '', Validators.required],
      nameEn: [role?.nameEn || ''],
      nameAr: [role?.nameAr || '', Validators.required],
      rolePermissionIds: [role?.rolePermissionIds || []],
      categoryIds: [role?.categoryIds || []],
      productIds: [role?.productIds || []]
    });
  }

  onPermissionChange(id: number, event: Event): void {
    const isChecked = (event.target as HTMLInputElement).checked;
    const currentPerms = this.roleForm.get('rolePermissionIds')?.value || [];

    if (isChecked) {
      this.roleForm.get('rolePermissionIds')?.setValue([...currentPerms, id]);
    } else {
      this.roleForm.get('rolePermissionIds')?.setValue(currentPerms.filter((c: number) => c !== id));
    }
  }

  onCategoryChange(id: number, event: Event): void {
    const isChecked = (event.target as HTMLInputElement).checked;
    const currentCats = this.roleForm.get('categoryIds')?.value || [];

    if (isChecked) {
      this.roleForm.get('categoryIds')?.setValue([...currentCats, id]);
    } else {
      this.roleForm.get('categoryIds')?.setValue(currentCats.filter((c: number) => c !== id));
    }
  }

  onProductChange(id: number, event: Event): void {
    const isChecked = (event.target as HTMLInputElement).checked;
    const currentProducts = this.roleForm.get('productIds')?.value || [];

    if (isChecked) {
      this.roleForm.get('productIds')?.setValue([...currentProducts, id]);
    } else {
      this.roleForm.get('productIds')?.setValue(currentProducts.filter((p: number) => p !== id));
    }
  }

  save(): void {
    if (this.roleForm.valid) {
      this.roleService.saveRole(this.roleForm.value).subscribe({
        next: (res) => {
          if (res) {
            this.toastr.success('Successfully saved', 'Roles');
            this.router.navigate(['../roles'], { relativeTo: this.route })
          }
          else {
            this.toastr.error('Error while saving', 'Role')
          }
        },
        error: (err) => {
          console.log(err.error);
          this.toastr.error(err.error, 'Role')
        }
      });
    }
    else {
      this.toastr.error('Please fill all required fields!', 'Roles');
    }
  }

  cancel(): void {
    this.router.navigate(['../roles'], { relativeTo: this.route })
  }
}
