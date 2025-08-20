import { Component, OnInit } from '@angular/core';
import { UsersService } from '../../../services/users.service';
import { LanguageService } from '../../../../core/services/language.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Observable, tap } from 'rxjs';
import { UserPagination } from '../../../models/users';

@Component({
  selector: 'app-users',
  standalone: false,
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent implements OnInit {

  constructor(
    private usersService: UsersService,
    public lang: LanguageService,
    private router: Router,
    private route: ActivatedRoute,
    private toastr: ToastrService,
    private fb: FormBuilder,
  ) {
    this.searchForm = this.fb.group({
      keyword: ['', Validators.required],
    });
  }
  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(loadType: string = 'GET') {
    if (loadType === 'GET') {
      this.searchForm.reset();
      this.users$ = this.usersService.GetUsersPagination(this.pageIndex, this.pageSize).pipe(
        tap(res => {
          this.totalPages = Math.ceil(res.totals / this.pageSize);
          this.totalPagesArray = Array.from({ length: this.totalPages }, (_, i) => i + 1);
        })
      );
    }
    else {
      if (this.keyword?.value) {
        this.users$ = this.usersService.SearchUsersPagination(this.pageIndex, this.pageSize, this.keyword?.value?.toString()).pipe(
          tap(res => {
            this.totalPages = Math.ceil(res.totals / this.pageSize);
            this.totalPagesArray = Array.from({ length: this.totalPages }, (_, i) => i + 1);
          })
        );
      }
      else {
        this.toastr.info('please specify the search term!', 'Search');
      }
    }
  }

  openForm(userId: number | null) {
    if (userId) {
      this.router.navigate(['../users-form'], { relativeTo: this.route, queryParams: { userId: userId } });
    }
    else {
      this.router.navigate(['../users-form'], { relativeTo: this.route });
    }
  }

  users$!: Observable<UserPagination>;
  users!: UserPagination;
  pageIndex: number = 1;
  pageSize: number = 10;
  totalPages = 1;
  totalPagesArray: number[] = [];
  searchForm!: FormGroup;

  changePage(newPageIndex: number): void {
    if (newPageIndex > 0 && newPageIndex <= this.totalPages) {
      this.pageIndex = newPageIndex;
      this.loadUsers();
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


  changePassword(id: number) {
    this.usersService.ChangePasswordForAdmin(id).subscribe({
      next: (res) => {
        if (res) {
          this.toastr.success('Successfully password changed', 'User');
        }
        else {
          this.toastr.error('Error while change password', 'User');
        }
      },
      error: (err) => {
        this.toastr.error(err.error, 'User');
      }
    });
  }

  get keyword(): FormControl {
    return this.searchForm.get('keyword') as FormControl;
  }
}
