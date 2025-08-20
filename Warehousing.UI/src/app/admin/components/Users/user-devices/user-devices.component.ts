import { Component, OnInit } from '@angular/core';
import { UsersService } from '../../../services/users.service';
import { LanguageService } from '../../../../core/services/language.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { UserDevice, UserDevicePagination } from '../../../models/UserDevice';
import { Observable, tap } from 'rxjs';
import { User } from '../../../models/users';
import { UpdateUserDeviceDto } from '../../../models/UpdateUserDeviceDto';

@Component({
  selector: 'app-user-devices',
  standalone: false,
  templateUrl: './user-devices.component.html',
  styleUrl: './user-devices.component.scss'
})
export class UserDevicesComponent implements OnInit {

  constructor(
    private usersService: UsersService,
    public lang: LanguageService,
    private router: Router,
    private route: ActivatedRoute,
    private toastr: ToastrService,
    private fb: FormBuilder,
  ) {
    this.users = this.route.snapshot.data['usersListResolver'];
    this.usersDropDowForm = this.fb.group({
      userId: [0]
    });
  }
  ngOnInit(): void {
    this.loadUserDevices();
  }

  loadUserDevices(userId: number = 0) {
    this.userDevices$ = this.usersService.GetUserDevicesPagination(this.pageIndex, this.pageSize, userId).pipe(
      tap(res => {
        this.totalPages = Math.ceil(res.totals / this.pageSize);
        this.totalPagesArray = Array.from({ length: this.totalPages }, (_, i) => i + 1);
      })
    );
  }

  toggleApproval(device: UserDevice): void {
    const newValue = !device.isApproved;
    this.updatingDevice = device.id; // Show loading state

    const updateUserDeviceDto: UpdateUserDeviceDto = { deviceId: device.id, isApproved: newValue };

    // Call your API to update approval status
    this.usersService.UpdateUserDevice(updateUserDeviceDto).subscribe({
      next: (updatedDevice) => {
        device.isApproved = updatedDevice.isApproved; // Reflect response
        this.toastr.success(
          `الجهاز ${device.isApproved ? 'موافق عليه' : 'محظور'}`,
          'تحديث الحالة'
        );
      },
      error: (err) => {
        this.toastr.error('فشل في تحديث الحالة', 'خطأ');
        // Optionally revert UI
      },
      complete: () => {
        this.updatingDevice = null; // Reset loading
      }
    });
  }

  userDevices$!: Observable<UserDevicePagination>;
  users!: User[];
  pageIndex: number = 1;
  pageSize: number = 10;
  totalPages = 1;
  totalPagesArray: number[] = [];
  usersDropDowForm!: FormGroup;
  updatingDevice: number | null = null;

  changePage(newPageIndex: number): void {
    if (newPageIndex > 0 && newPageIndex <= this.totalPages) {
      this.pageIndex = newPageIndex;
      this.loadUserDevices(+this.UserId.value);
    }
  }

  getPageRange(): number[] {
    const delta = 2; // how many pages to show before/after current
    const range: number[] = [];

    const left = Math.max(2, this.pageIndex - delta);
    const right = Math.min(this.totalPages - 1, this.pageIndex + delta);

    for (let i = left; i <= right; i++) {
      range.push(i);
    }

    return range;
  }

  get UserId(): FormControl {
    return this.usersDropDowForm.get('userId') as FormControl;
  }
}