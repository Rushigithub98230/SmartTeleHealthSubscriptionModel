export interface ApplicationLog {
  id: number;
  timestamp: Date;
  logLevel: string;
  source: string;
  message: string;
  exception?: string;
  userId?: number;
  operation?: string;
  additionalData?: string;
  correlationId?: string;
}

export interface AuditLog {
  id: number;
  userId?: number;
  type: string;
  tableName: string;
  dateTime: Date;
  oldValues?: string;
  newValues?: string;
  affectedColumns?: string;
  primaryKey?: string;
}

export interface LogEntry {
  timestamp: Date;
  level: string;
  message: string;
  exception?: string;
  properties?: Record<string, any>;
}

export interface ApplicationLogFilterDto {
  logLevel?: string[];
  source?: string[];
  startDate?: Date;
  endDate?: Date;
  userId?: number;
  operation?: string;
  correlationId?: string;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

export interface AuditLogFilterDto {
  type?: string[];
  tableName?: string[];
  startDate?: Date;
  endDate?: Date;
  userId?: number;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

export interface LogStatistics {
  totalLogs: number;
  logsByLevel: Record<string, number>;
  logsBySource: Record<string, number>;
  errorRate: number;
}

