export interface WorkingHoursException {
  id: number;
  workingHoursId: number;
  exceptionDate: string;
  startTime?: string;
  endTime?: string;
  reason: string;
  isWorkingDay: boolean;
  createdAt: string;
  createdBy: string;
}

export interface WorkingHoursDay {
  id?: number;
  workingHoursId?: number;
  dayOfWeek: number; // 0 = Sunday, 1 = Monday, etc.
  startTime?: string;
  endTime?: string;
  isEnabled: boolean;
}

export interface WorkingHours {
  id: number;
  name: string;
  description: string;
  startTime?: string; // Deprecated: kept for backward compatibility
  endTime?: string; // Deprecated: kept for backward compatibility
  startDay?: number | string; // Deprecated: kept for backward compatibility
  endDay?: number | string; // Deprecated: kept for backward compatibility
  isActive: boolean;
  allowWeekends: boolean;
  allowHolidays: boolean;
  days: WorkingHoursDay[]; // Per-day configuration
  exceptions: WorkingHoursException[];
  createdAt: string;
  createdBy: string;
}

export interface WorkingHoursStatus {
  isWithinWorkingHours: boolean;
  currentTime: string;
  nextWorkingDay?: string;
  message: string;
}

export interface WorkingHoursExceptionDto {
  workingHoursId: number;
  exceptionDate: string;
  startTime?: string;
  endTime?: string;
  reason: string;
  isWorkingDay: boolean;
}


