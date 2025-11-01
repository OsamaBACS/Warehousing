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

export interface WorkingHours {
  id: number;
  name: string;
  description: string;
  startTime: string;
  endTime: string;
  startDay: number; // 0 = Sunday, 1 = Monday, etc.
  endDay: number;
  isActive: boolean;
  allowWeekends: boolean;
  allowHolidays: boolean;
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


