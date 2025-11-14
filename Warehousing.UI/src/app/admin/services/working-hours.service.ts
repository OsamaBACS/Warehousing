import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

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
  dayOfWeek: number;
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
  days?: WorkingHoursDay[]; // Per-day configuration
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

@Injectable({
  providedIn: 'root'
})
export class WorkingHoursService {
  private apiUrl = `${environment.baseUrl}/WorkingHours`;

  constructor(private http: HttpClient) { }

  getWorkingHours(): Observable<WorkingHours> {
    return this.http.get<WorkingHours>(`${this.apiUrl}/GetWorkingHours`);
  }

  updateWorkingHours(workingHours: WorkingHours): Observable<WorkingHours> {
    return this.http.post<WorkingHours>(`${this.apiUrl}/UpdateWorkingHours`, workingHours);
  }

  getWorkingHoursStatus(): Observable<WorkingHoursStatus> {
    return this.http.get<WorkingHoursStatus>(`${this.apiUrl}/GetWorkingHoursStatus`);
  }

  addException(exception: Omit<WorkingHoursException, 'id' | 'createdAt' | 'createdBy'>): Observable<WorkingHoursException> {
    return this.http.post<WorkingHoursException>(`${this.apiUrl}/AddException`, exception);
  }

  updateException(id: number, exception: Omit<WorkingHoursException, 'id' | 'createdAt' | 'createdBy'>): Observable<WorkingHoursException> {
    return this.http.put<WorkingHoursException>(`${this.apiUrl}/UpdateException/${id}`, exception);
  }

  deleteException(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/DeleteException/${id}`);
  }

  // Helper methods for UI
  getDayName(dayNumber: number | string): string {
    // Handle both number and DayOfWeek enum string values
    let dayIndex: number;
    
    if (typeof dayNumber === 'string') {
      // Convert DayOfWeek enum string to number
      const dayMap: { [key: string]: number } = {
        'Sunday': 0,
        'Monday': 1,
        'Tuesday': 2,
        'Wednesday': 3,
        'Thursday': 4,
        'Friday': 5,
        'Saturday': 6
      };
      dayIndex = dayMap[dayNumber] ?? 0;
    } else {
      dayIndex = dayNumber;
    }
    
    const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    return days[dayIndex] || 'Unknown';
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

  formatTime(timeString: string): string {
    if (!timeString) return '';
    const time = new Date(`2000-01-01T${timeString}`);
    return time.toLocaleTimeString('en-US', { 
      hour: '2-digit', 
      minute: '2-digit',
      hour12: true 
    });
  }

  isCurrentTimeWithinWorkingHours(workingHours: WorkingHours): boolean {
    const now = new Date();
    const currentDay = now.getDay();
    const currentTime = now.getTime();

    // Check for exceptions first
    const today = now.toISOString().split('T')[0];
    const todayException = workingHours.exceptions.find(e => e.exceptionDate === today);
    
    if (todayException) {
      if (!todayException.isWorkingDay) {
        return false;
      }
      
      if (todayException.startTime && todayException.endTime) {
        const startTime = new Date(`2000-01-01T${todayException.startTime}`).getTime();
        const endTime = new Date(`2000-01-01T${todayException.endTime}`).getTime();
        return currentTime >= startTime && currentTime <= endTime;
      }
      // If exception doesn't specify times, fall through to check day configuration
    }

    // Check per-day configuration
    if (workingHours.days && workingHours.days.length > 0) {
      const dayConfig = workingHours.days.find(d => d.dayOfWeek === currentDay);
      
      if (dayConfig && dayConfig.isEnabled) {
        // If day is enabled but has no times set, allow access
        if (!dayConfig.startTime || !dayConfig.endTime) {
          return true;
        }
        
        const startTime = new Date(`2000-01-01T${dayConfig.startTime}`).getTime();
        const endTime = new Date(`2000-01-01T${dayConfig.endTime}`).getTime();
        return currentTime >= startTime && currentTime <= endTime;
      }
      
      // Day is not enabled or doesn't exist in configuration
      // Check if weekends are allowed for Friday/Saturday
      if (!workingHours.allowWeekends && (currentDay === 5 || currentDay === 6)) {
        return false;
      }
      
      return false; // Day is not configured as a working day
    }

    // Fallback to old period-based logic for backward compatibility
    if (workingHours.startDay && workingHours.endDay) {
      // Convert startDay and endDay to numbers if they are strings
      const startDay = typeof workingHours.startDay === 'string' ? this.convertDayToNumber(workingHours.startDay) : workingHours.startDay;
      const endDay = typeof workingHours.endDay === 'string' ? this.convertDayToNumber(workingHours.endDay) : workingHours.endDay;
      
      // Check if current day is within working days
      const isWithinWorkingDays = currentDay >= startDay && currentDay <= endDay;
      
      if (!isWithinWorkingDays && !workingHours.allowWeekends) {
        return false;
      }
    }

    // Use default working hours (old way)
    if (workingHours.startTime && workingHours.endTime) {
      const startTime = new Date(`2000-01-01T${workingHours.startTime}`).getTime();
      const endTime = new Date(`2000-01-01T${workingHours.endTime}`).getTime();
      return currentTime >= startTime && currentTime <= endTime;
    }

    return true; // If no time restrictions, allow access
  }
}
