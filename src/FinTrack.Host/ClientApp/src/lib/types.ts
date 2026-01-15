// API Types - matching backend DTOs

// Auth types
export interface User {
  id: string;
  email: string;
  displayName: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

// ProfileType matches C# enum values
export const ProfileType = {
  Personal: 0,
  Business: 1,
} as const;

export type ProfileType = (typeof ProfileType)[keyof typeof ProfileType];

export interface Profile {
  id: string;
  name: string;
  type: ProfileType;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateProfileRequest {
  name: string;
  type?: ProfileType;
}

export interface UpdateProfileRequest {
  name: string;
  type: ProfileType;
}

export interface Account {
  id: string;
  profileId: string;
  name: string;
  bankName: string | null;
  currency: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateAccountRequest {
  name: string;
  bankName?: string | null;
  currency?: string;
}

export interface UpdateAccountRequest {
  name: string;
  bankName: string | null;
  currency: string;
}

// Category types
export interface Category {
  id: string;
  name: string;
  icon: string;
  color: string;
  sortOrder: number;
  parentId: string | null;
  transactionCount: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateCategoryRequest {
  name: string;
  icon?: string;
  color?: string;
  sortOrder?: number;
  parentId?: string | null;
}

export interface UpdateCategoryRequest {
  name: string;
  icon: string;
  color: string;
  sortOrder: number;
  parentId: string | null;
}

// Transaction types
export interface Transaction {
  id: string;
  accountId: string;
  categoryId: string | null;
  categoryName: string | null;
  date: string;
  amount: number;
  description: string;
  notes: string | null;
  tags: string[];
  createdAt: string;
  updatedAt: string | null;
}

export interface TransactionPage {
  items: Transaction[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface UpdateTransactionRequest {
  categoryId?: string | null;
  notes?: string | null;
  tags?: string[];
}

export interface TransactionFilter {
  accountId?: string;
  categoryId?: string;
  fromDate?: string;
  toDate?: string;
  minAmount?: number;
  maxAmount?: number;
  search?: string;
  uncategorized?: boolean;
  page?: number;
  pageSize?: number;
}

// Import types
export const ImportStatus = {
  Pending: 'Pending',
  Processing: 'Processing',
  Completed: 'Completed',
  Failed: 'Failed',
} as const;

export type ImportStatus = (typeof ImportStatus)[keyof typeof ImportStatus];

export interface CsvFormatConfig {
  delimiter: string;
  hasHeader: boolean;
  dateColumn: number;
  dateFormat: string;
  descriptionColumn: number;
  amountType: 'signed' | 'split';
  amountColumn: number | null;
  debitColumn: number | null;
  creditColumn: number | null;
  balanceColumn: number | null;
}

export interface ImportSession {
  id: string;
  accountId: string;
  filename: string;
  rowCount: number;
  status: ImportStatus;
  errorMessage: string | null;
  formatConfig: CsvFormatConfig | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface UploadResponse {
  sessionId: string;
  filename: string;
  rowCount: number;
  detectedFormat: CsvFormatConfig;
  sampleRows: string[];
}

export interface TransactionPreview {
  date: string;
  description: string;
  amount: number;
  isDuplicate: boolean;
}

export interface PreviewResponse {
  sessionId: string;
  transactions: TransactionPreview[];
  duplicateCount: number;
}

export interface ConfirmImportResponse {
  importedCount: number;
  skippedDuplicates: number;
}

// Import Format types
export interface ImportFormat {
  id: string;
  name: string;
  bankName: string | null;
  mapping: CsvFormatConfig;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateImportFormatRequest {
  name: string;
  bankName?: string | null;
  mapping: CsvFormatConfig;
}

export interface UpdateImportFormatRequest {
  name: string;
  bankName?: string | null;
  mapping: CsvFormatConfig;
}

// Rule types
export interface Rule {
  id: string;
  name: string;
  priority: number;
  ruleToml: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateRuleRequest {
  name: string;
  priority: number;
  ruleToml: string;
  isActive?: boolean;
}

export interface UpdateRuleRequest {
  name: string;
  priority: number;
  ruleToml: string;
  isActive: boolean;
}

export interface TestRulesRequest {
  description: string;
  amount: number;
  date: string;
}

export interface TestRulesResponse {
  matchedRuleName: string | null;
  category: string | null;
  tags: string[];
}

// API Error types
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  status: number;
  details?: ProblemDetails;

  constructor(message: string, status: number, details?: ProblemDetails) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.details = details;
  }
}
