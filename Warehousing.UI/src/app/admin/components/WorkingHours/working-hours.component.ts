import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { WorkingHoursService, WorkingHours, WorkingHoursException, WorkingHoursStatus } from '../../services/working-hours.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { PermissionsEnum } from '../../constants/enums/permissions.enum';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-working-hours',
  templateUrl: './working-hours.component.html',
  styleUrls: ['./working-hours.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, TranslateModule]
})
export class WorkingHoursComponent implements OnInit {
  workingHoursForm!: FormGroup;
  exceptionForm!: FormGroup;
  workingHours: WorkingHours | null = null;
  workingHoursStatus: WorkingHoursStatus | null = null;
  loading = false;
  saving = false;
  isWorkingHoursActive: boolean = false;
  currentStatusMessage: string = '';
  currentStatusClass: string = '';
  currentTime: Date = new Date();

  // Form data
  formData: Partial<WorkingHours> = {};
  newException: Partial<WorkingHoursException> = {
    isWorkingDay: true,
    exceptionDate: '',
    reason: '',
    startTime: '',
    endTime: ''
  };

  // UI state
  showExceptionForm = false;
  editingException: WorkingHoursException | null = null;

  // Day options
  dayOfWeekOptions = [
    { value: 0, name: 'Sunday' },
    { value: 1, name: 'Monday' },
    { value: 2, name: 'Tuesday' },
    { value: 3, name: 'Wednesday' },
    { value: 4, name: 'Thursday' },
    { value: 5, name: 'Friday' },
    { value: 6, name: 'Saturday' }
  ];
  permissionsEnum = PermissionsEnum;

  constructor(
    private fb: FormBuilder,
    private workingHoursService: WorkingHoursService,
    private notificationService: NotificationService,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.initForms();
    this.loadWorkingHours();
    this.loadWorkingHoursStatus();
  }

  initForms(): void {
    this.workingHoursForm = this.fb.group({
      id: [0],
      name: ['', Validators.required],
      description: [''],
      startTime: ['', Validators.required],
      endTime: ['', Validators.required],
      startDay: [0, Validators.required], // Sunday
      endDay: [4, Validators.required], // Thursday
      allowWeekends: [false],
      allowHolidays: [false],
      isActive: [false]
    });

    this.exceptionForm = this.fb.group({
      workingHoursId: [0, Validators.required],
      exceptionDate: ['', Validators.required],
      startTime: [''],
      endTime: [''],
      reason: ['', Validators.required],
      isWorkingDay: [true]
    });
  }

  hasPermission(permission: string): boolean {
    return this.authService.hasPermission(permission);
  }

  loadWorkingHours(): void {
    this.loading = true;
    this.workingHoursService.getWorkingHours().subscribe({
      next: (data: WorkingHours) => {
        // Ensure exceptions array is always initialized
        if (!data.exceptions) {
          data.exceptions = [];
        }
        
        // Convert DayOfWeek enum strings to numbers for form handling
        const convertedData = {
          ...data,
          startDay: this.convertDayToNumber(data.startDay),
          endDay: this.convertDayToNumber(data.endDay)
        };
        
        this.workingHours = convertedData;
        this.formData = { ...convertedData };
        this.loading = false;
      },
      error: (error: unknown) => {
        console.error('Error loading working hours:', error);
        this.notificationService.error('Failed to load working hours configuration');
        this.loading = false;
      }
    });
  }

  loadWorkingHoursStatus(): void {
    this.workingHoursService.getWorkingHoursStatus().subscribe({
      next: (status: WorkingHoursStatus) => {
        this.workingHoursStatus = status;
      },
      error: (error: unknown) => {
        console.error('Error loading working hours status:', error);
      }
    });
  }

  saveWorkingHours(): void {
    if (this.workingHoursForm.invalid) {
      this.notificationService.error('Please fill in all required fields');
      return;
    }

    this.saving = true;
    const formValue = this.workingHoursForm.value;
    
    // Convert day numbers back to DayOfWeek enum strings for the API
    const updatedData: WorkingHours = {
      ...formValue,
      startDay: this.convertNumberToDayString(formValue.startDay),
      endDay: this.convertNumberToDayString(formValue.endDay),
      exceptions: this.workingHours?.exceptions || []
    };

    console.log('Saving working hours:', updatedData); // Debug log

    this.workingHoursService.updateWorkingHours(updatedData).subscribe({
      next: (data: WorkingHours) => {
        // Convert the response back to numbers for display
        const convertedData = {
          ...data,
          startDay: this.convertDayToNumber(data.startDay),
          endDay: this.convertDayToNumber(data.endDay)
        };
        
        this.workingHours = convertedData;
        this.formData = { ...convertedData };
        this.notificationService.success('Working hours updated successfully');
        this.saving = false;
        this.loadWorkingHoursStatus();
      },
      error: (error: unknown) => {
        console.error('Error updating working hours:', error);
        this.notificationService.error('Failed to update working hours');
        this.saving = false;
      }
    });
  }

  addException(): void {
    if (!this.workingHours || !this.newException.exceptionDate || !this.newException.reason) {
      this.notificationService.error('Please fill in all required fields');
      return;
    }

    const exceptionData = {
      workingHoursId: this.workingHours.id,
      exceptionDate: this.newException.exceptionDate!,
      reason: this.newException.reason!,
      isWorkingDay: this.newException.isWorkingDay!,
      startTime: this.newException.startTime || undefined,
      endTime: this.newException.endTime || undefined
    };

    this.workingHoursService.addException(exceptionData).subscribe({
      next: (exception: WorkingHoursException) => {
        this.notificationService.success('Exception added successfully');
        this.loadWorkingHours();
        this.resetExceptionForm();
      },
      error: (error: unknown) => {
        console.error('Error adding exception:', error);
        this.notificationService.error('Failed to add exception');
      }
    });
  }

  editException(exception: WorkingHoursException): void {
    this.editingException = exception;
    this.newException = {
      exceptionDate: exception.exceptionDate,
      reason: exception.reason,
      isWorkingDay: exception.isWorkingDay,
      startTime: exception.startTime || '',
      endTime: exception.endTime || ''
    };
    this.showExceptionForm = true;
  }

  updateException(): void {
    if (!this.editingException || !this.newException.exceptionDate || !this.newException.reason) {
      this.notificationService.error('Please fill in all required fields');
      return;
    }

    const exceptionData = {
      workingHoursId: this.editingException.workingHoursId,
      exceptionDate: this.newException.exceptionDate!,
      reason: this.newException.reason!,
      isWorkingDay: this.newException.isWorkingDay!,
      startTime: this.newException.startTime || undefined,
      endTime: this.newException.endTime || undefined
    };

    this.workingHoursService.updateException(this.editingException.id, exceptionData).subscribe({
      next: () => {
        this.notificationService.success('Exception updated successfully');
        this.loadWorkingHours();
        this.resetExceptionForm();
      },
      error: (error: unknown) => {
        console.error('Error updating exception:', error);
        this.notificationService.error('Failed to update exception');
      }
    });
  }

  deleteException(exception: WorkingHoursException): void {
    if (confirm(`Are you sure you want to delete this exception?`)) {
      this.workingHoursService.deleteException(exception.id).subscribe({
        next: () => {
          this.notificationService.success('Exception deleted successfully');
          this.loadWorkingHours();
        },
        error: (error: unknown) => {
          console.error('Error deleting exception:', error);
          this.notificationService.error('Failed to delete exception');
        }
      });
    }
  }

  resetExceptionForm(): void {
    this.newException = {
      isWorkingDay: true,
      exceptionDate: '',
      reason: '',
      startTime: '',
      endTime: ''
    };
    this.showExceptionForm = false;
    this.editingException = null;
  }

  toggleExceptionForm(): void {
    this.showExceptionForm = !this.showExceptionForm;
    if (!this.showExceptionForm) {
      this.resetExceptionForm();
    }
  }

  getDayName(dayNumber: number | string | undefined): string {
    if (dayNumber === undefined) return 'Unknown';
    return this.workingHoursService.getDayName(dayNumber);
  }

  convertDayToNumber(day: number | string): number {
    if (typeof day === 'number') return day;
    
    const dayMap: { [key: string]: number } = {
      'Sunday': 0,
      'Monday': 1,
      'Tuesday': 2,
      'Wednesday': 3,
      'Thursday': 4,
      'Friday': 5,
      'Saturday': 6
    };
    
    return dayMap[day] ?? 0;
  }

  convertNumberToDayString(dayNumber: number): string {
    const dayMap: { [key: number]: string } = {
      0: 'Sunday',
      1: 'Monday',
      2: 'Tuesday',
      3: 'Wednesday',
      4: 'Thursday',
      5: 'Friday',
      6: 'Saturday'
    };
    
    return dayMap[dayNumber] ?? 'Sunday';
  }

  onStatusClick(): void {
    if (this.workingHours) {
      const formData = {
        id: this.workingHours.id,
        name: this.workingHours.name,
        description: this.workingHours.description,
        startTime: this.workingHours.startTime,
        endTime: this.workingHours.endTime,
        startDay: this.convertDayToNumber(this.workingHours.startDay),
        endDay: this.convertDayToNumber(this.workingHours.endDay),
        allowWeekends: this.workingHours.allowWeekends,
        allowHolidays: this.workingHours.allowHolidays,
        isActive: this.workingHours.isActive
      };
      
      console.log('Populating form with:', formData); // Debug log
      
      // Populate the form with current working hours data
      this.workingHoursForm.patchValue(formData);
      
      // Scroll to the form section
      const formElement = document.querySelector('.config-section');
      if (formElement) {
        formElement.scrollIntoView({ behavior: 'smooth' });
      }
    }
  }

  formatTime(timeString: string): string {
    return this.workingHoursService.formatTime(timeString);
  }

  isCurrentTimeWithinWorkingHours(): boolean {
    if (!this.workingHours) return false;
    return this.workingHoursService.isCurrentTimeWithinWorkingHours(this.workingHours);
  }

  getStatusClass(): string {
    if (!this.workingHoursStatus) return 'text-muted';
    return this.workingHoursStatus.isWithinWorkingHours ? 'text-success' : 'text-danger';
  }

  getStatusIcon(): string {
    if (!this.workingHoursStatus) return 'fas fa-question-circle';
    return this.workingHoursStatus.isWithinWorkingHours ? 'fas fa-check-circle' : 'fas fa-times-circle';
  }

  trackByExceptionId(index: number, exception: WorkingHoursException): number {
    return exception.id;
  }
}
