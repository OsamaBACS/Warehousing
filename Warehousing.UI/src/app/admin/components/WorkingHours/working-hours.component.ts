import { Component, OnInit, DoCheck } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { WorkingHoursService, WorkingHours, WorkingHoursException, WorkingHoursStatus } from '../../services/working-hours.service';
import { WorkingHoursDay } from '../../models/working-hours.models';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { PermissionsEnum } from '../../constants/enums/permissions.enum';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-working-hours',
  templateUrl: './working-hours.component.html',
  styleUrls: ['./working-hours.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, TranslateModule]
})
export class WorkingHoursComponent implements OnInit, DoCheck {
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
  isRTL = false;

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

  // Day options with translations
  dayOfWeekOptions = [
    { value: 0, nameKey: 'COMMON.SUNDAY', name: 'Sunday' },
    { value: 1, nameKey: 'COMMON.MONDAY', name: 'Monday' },
    { value: 2, nameKey: 'COMMON.TUESDAY', name: 'Tuesday' },
    { value: 3, nameKey: 'COMMON.WEDNESDAY', name: 'Wednesday' },
    { value: 4, nameKey: 'COMMON.THURSDAY', name: 'Thursday' },
    { value: 5, nameKey: 'COMMON.FRIDAY', name: 'Friday' },
    { value: 6, nameKey: 'COMMON.SATURDAY', name: 'Saturday' }
  ];

  getDayNameKey(dayNumber: number | string | null | undefined): string {
    if (dayNumber === null || dayNumber === undefined) return '';
    
    // Convert to number if it's a string
    const dayNum = typeof dayNumber === 'string' ? parseInt(dayNumber, 10) : dayNumber;
    
    // Validate it's a valid day (0-6)
    if (isNaN(dayNum) || dayNum < 0 || dayNum > 6) {
      return '';
    }
    
    const dayOption = this.dayOfWeekOptions.find(d => d.value === dayNum);
    return dayOption?.nameKey || '';
  }
  permissionsEnum = PermissionsEnum;

  constructor(
    private fb: FormBuilder,
    private workingHoursService: WorkingHoursService,
    private notificationService: NotificationService,
    private authService: AuthService,
    public languageService: LanguageService,
    private translate: TranslateService
  ) { }

  ngOnInit(): void {
    this.updateRTL();
    this.initForms();
    this.initExceptionForm();
    this.loadWorkingHours();
    this.loadWorkingHoursStatus();
    // Update current time every second
    setInterval(() => {
      this.currentTime = new Date();
    }, 1000);
  }

  ngDoCheck(): void {
    // Check if language changed on every change detection cycle
    const newRTL = this.languageService.currentLang === 'ar';
    if (newRTL !== this.isRTL) {
      this.updateRTL();
    }
  }

  updateRTL(): void {
    this.isRTL = this.languageService.currentLang === 'ar';
  }

  initForms(): void {
    this.workingHoursForm = this.fb.group({
      id: [0],
      name: ['', Validators.required],
      description: [''],
      allowWeekends: [false],
      allowHolidays: [false],
      isActive: [false],
      days: this.fb.array([]) // Will be populated with 7 days
    });

    // Initialize days array with all 7 days
    this.initializeDays();
  }

  initializeDays(): void {
    const daysArray = this.workingHoursForm.get('days') as any;
    daysArray.clear();
    
    for (let i = 0; i < 7; i++) {
      daysArray.push(this.fb.group({
        id: [null],
        dayOfWeek: [i],
        startTime: [''],
        endTime: [''],
        isEnabled: [false]
      }));
    }
  }

  initExceptionForm(): void {
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
        
        // Ensure days array exists and has all 7 days, sorted by dayOfWeek
        if (!data.days || data.days.length === 0) {
          // Initialize with all 7 days disabled
          data.days = this.dayOfWeekOptions.map(day => ({
            dayOfWeek: day.value,
            startTime: '',
            endTime: '',
            isEnabled: false
          }));
        } else {
          // Normalize dayOfWeek values to numbers and ensure all 7 days are present
          const normalizedDays = data.days.map(day => ({
            ...day,
            dayOfWeek: typeof day.dayOfWeek === 'string' 
              ? this.workingHoursService.convertDayToNumber(day.dayOfWeek)
              : day.dayOfWeek
          }));
          
          // Ensure all 7 days are present (fill missing days)
          const allDays: WorkingHoursDay[] = [];
          for (let i = 0; i < 7; i++) {
            const existingDay = normalizedDays.find(d => d.dayOfWeek === i);
            if (existingDay) {
              allDays.push(existingDay);
            } else {
              allDays.push({
                dayOfWeek: i,
                startTime: '',
                endTime: '',
                isEnabled: false
              });
            }
          }
          // Sort by dayOfWeek
          data.days = allDays.sort((a, b) => a.dayOfWeek - b.dayOfWeek);
        }
        
        this.workingHours = data;
        this.formData = { ...data };
        
        // Populate form with days (this will replace any existing days in the form array)
        this.populateDaysForm(data.days);
        
        // Populate basic info
        this.workingHoursForm.patchValue({
          id: data.id,
          name: data.name,
          description: data.description,
          allowWeekends: data.allowWeekends,
          allowHolidays: data.allowHolidays,
          isActive: data.isActive
        });
        
        this.loading = false;
      },
      error: (error: unknown) => {
        this.notificationService.error('Failed to load working hours configuration');
        this.loading = false;
      }
    });
  }

  populateDaysForm(days: WorkingHoursDay[]): void {
    const daysArray = this.workingHoursForm.get('days') as any;
    daysArray.clear();
    
    // Ensure we have exactly 7 days, sorted by dayOfWeek (0-6)
    const sortedDays = [...days].sort((a, b) => a.dayOfWeek - b.dayOfWeek);
    
    // Fill missing days
    for (let i = 0; i < 7; i++) {
      const existingDay = sortedDays.find(d => {
        // Handle both number and DayOfWeek enum string values
        const dayNum = typeof d.dayOfWeek === 'string' 
          ? this.workingHoursService.convertDayToNumber(d.dayOfWeek) 
          : d.dayOfWeek;
        return dayNum === i;
      });
      
      if (existingDay) {
        // Convert dayOfWeek to number if it's a string
        const dayOfWeekNum = typeof existingDay.dayOfWeek === 'string'
          ? this.workingHoursService.convertDayToNumber(existingDay.dayOfWeek)
          : existingDay.dayOfWeek;
          
        daysArray.push(this.fb.group({
          id: [existingDay.id || null],
          dayOfWeek: [dayOfWeekNum],
          startTime: [existingDay.startTime || ''],
          endTime: [existingDay.endTime || ''],
          isEnabled: [existingDay.isEnabled || false]
        }));
      } else {
        // Create a new day entry for missing day
        daysArray.push(this.fb.group({
          id: [null],
          dayOfWeek: [i],
          startTime: [''],
          endTime: [''],
          isEnabled: [false]
        }));
      }
    }
  }

  getDaysFormArray(): any {
    return this.workingHoursForm.get('days') as any;
  }

  loadWorkingHoursStatus(): void {
    this.workingHoursService.getWorkingHoursStatus().subscribe({
      next: (status: WorkingHoursStatus) => {
        this.workingHoursStatus = status;
      },
      error: (error: unknown) => {
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
    
    // Get days from form array
    const daysArray = this.getDaysFormArray();
    const days: WorkingHoursDay[] = daysArray.controls.map((control: any) => ({
      id: control.value.id || undefined,
      workingHoursId: this.workingHours?.id,
      dayOfWeek: control.value.dayOfWeek,
      startTime: control.value.isEnabled && control.value.startTime ? control.value.startTime : undefined,
      endTime: control.value.isEnabled && control.value.endTime ? control.value.endTime : undefined,
      isEnabled: control.value.isEnabled
    }));
    
    const updatedData: WorkingHours = {
      id: formValue.id,
      name: formValue.name,
      description: formValue.description,
      allowWeekends: formValue.allowWeekends,
      allowHolidays: formValue.allowHolidays,
      isActive: formValue.isActive,
      days: days,
      exceptions: this.workingHours?.exceptions || [],
      createdAt: this.workingHours?.createdAt || new Date().toISOString(),
      createdBy: this.workingHours?.createdBy || ''
    };


    this.workingHoursService.updateWorkingHours(updatedData).subscribe({
      next: (data: WorkingHours) => {
        this.workingHours = data;
        this.formData = { ...data };
        this.notificationService.success('Working hours updated successfully');
        this.saving = false;
        this.loadWorkingHoursStatus();
        // Reload to get updated days
        this.loadWorkingHours();
      },
      error: (error: unknown) => {
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
      // Populate basic info (days are already loaded in loadWorkingHours)
      this.workingHoursForm.patchValue({
        id: this.workingHours.id,
        name: this.workingHours.name,
        description: this.workingHours.description,
        allowWeekends: this.workingHours.allowWeekends,
        allowHolidays: this.workingHours.allowHolidays,
        isActive: this.workingHours.isActive
      });
      
      // Scroll to the form section
      const formElement = document.querySelector('.config-section');
      if (formElement) {
        formElement.scrollIntoView({ behavior: 'smooth' });
      }
    }
  }

  getDayName(dayNumber: number | string | null | undefined): string {
    const nameKey = this.getDayNameKey(dayNumber);
    return nameKey ? this.translate.instant(nameKey) : '';
  }

  getEnabledDaysCount(): number {
    if (!this.workingHours?.days) return 0;
    return this.workingHours.days.filter(d => d.isEnabled).length;
  }

  getEnabledDaysNames(): string {
    if (!this.workingHours?.days) return '';
    const enabledDays = this.workingHours.days.filter(d => d.isEnabled);
    if (enabledDays.length === 0) return '';
    return enabledDays.map(d => this.getDayName(d.dayOfWeek)).join(', ');
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
