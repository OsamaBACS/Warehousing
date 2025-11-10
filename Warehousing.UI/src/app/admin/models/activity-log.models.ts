export interface UserActivityLog {
  id: number;
  userId: number;
  username: string;
  action: string;
  module: string;
  timestamp: string;
  ipAddress: string;
  details: string;
  createdAt: string;
  createdBy: string;
}

export interface ActivityLogResponse {
  data: UserActivityLog[];
  totalCount: number;
}

export interface ActivityLogFilters {
  userId?: number;
  startDate?: string;
  endDate?: string;
  action?: string;
  module?: string;
  page?: number;
  pageSize?: number;
}

export interface ActivityLogStats {
  todayLogs: number;
  weekLogs: number;
  monthLogs: number;
  totalLogs: number;
}




