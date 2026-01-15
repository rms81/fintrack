import {
  type User,
  type RegisterRequest,
  type LoginRequest,
  type Profile,
  type CreateProfileRequest,
  type UpdateProfileRequest,
  type Account,
  type CreateAccountRequest,
  type UpdateAccountRequest,
  type Category,
  type CreateCategoryRequest,
  type UpdateCategoryRequest,
  type TransactionPage,
  type TransactionFilter,
  type Transaction,
  type UpdateTransactionRequest,
  type UploadResponse,
  type PreviewResponse,
  type ConfirmImportResponse,
  type CsvFormatConfig,
  type ImportSession,
  type ImportFormat,
  type CreateImportFormatRequest,
  type UpdateImportFormatRequest,
  type Rule,
  type CreateRuleRequest,
  type UpdateRuleRequest,
  type TestRulesRequest,
  type TestRulesResponse,
  type ProblemDetails,
  ApiError,
} from './types';

const API_BASE = '/api';

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    let details: ProblemDetails | undefined;
    try {
      details = await response.json();
    } catch {
      // Response body is not JSON
    }
    throw new ApiError(
      details?.title ?? `HTTP ${response.status}`,
      response.status,
      details
    );
  }

  // Handle 204 No Content
  if (response.status === 204) {
    return undefined as T;
  }

  return response.json();
}

async function request<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const response = await fetch(`${API_BASE}${endpoint}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  });
  return handleResponse<T>(response);
}

// Auth API
export const authApi = {
  register: (data: RegisterRequest) =>
    request<User>('/auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  login: (data: LoginRequest) =>
    request<User>('/auth/login', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  logout: () =>
    request<void>('/auth/logout', {
      method: 'POST',
    }),

  getCurrentUser: () => request<User>('/auth/me'),
};

// Profile API
export const profilesApi = {
  getAll: () => request<Profile[]>('/profiles'),

  getById: (id: string) => request<Profile>(`/profiles/${id}`),

  create: (data: CreateProfileRequest) =>
    request<Profile>('/profiles', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (id: string, data: UpdateProfileRequest) =>
    request<Profile>(`/profiles/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  delete: (id: string) =>
    request<void>(`/profiles/${id}`, {
      method: 'DELETE',
    }),
};

// Account API
export const accountsApi = {
  getAll: (profileId: string) =>
    request<Account[]>(`/profiles/${profileId}/accounts`),

  getById: (profileId: string, id: string) =>
    request<Account>(`/profiles/${profileId}/accounts/${id}`),

  create: (profileId: string, data: CreateAccountRequest) =>
    request<Account>(`/profiles/${profileId}/accounts`, {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (profileId: string, id: string, data: UpdateAccountRequest) =>
    request<Account>(`/profiles/${profileId}/accounts/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  delete: (profileId: string, id: string) =>
    request<void>(`/profiles/${profileId}/accounts/${id}`, {
      method: 'DELETE',
    }),
};

// Category API
export const categoriesApi = {
  getAll: (profileId: string) =>
    request<Category[]>(`/profiles/${profileId}/categories`),

  getById: (profileId: string, id: string) =>
    request<Category>(`/profiles/${profileId}/categories/${id}`),

  create: (profileId: string, data: CreateCategoryRequest) =>
    request<Category>(`/profiles/${profileId}/categories`, {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (profileId: string, id: string, data: UpdateCategoryRequest) =>
    request<Category>(`/profiles/${profileId}/categories/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  delete: (profileId: string, id: string) =>
    request<void>(`/profiles/${profileId}/categories/${id}`, {
      method: 'DELETE',
    }),
};

// Transaction API
export const transactionsApi = {
  getAll: (profileId: string, filter: TransactionFilter = {}) => {
    const params = new URLSearchParams();
    if (filter.accountId) params.set('accountId', filter.accountId);
    if (filter.categoryId) params.set('categoryId', filter.categoryId);
    if (filter.fromDate) params.set('fromDate', filter.fromDate);
    if (filter.toDate) params.set('toDate', filter.toDate);
    if (filter.minAmount !== undefined) params.set('minAmount', filter.minAmount.toString());
    if (filter.maxAmount !== undefined) params.set('maxAmount', filter.maxAmount.toString());
    if (filter.search) params.set('search', filter.search);
    if (filter.uncategorized) params.set('uncategorized', 'true');
    if (filter.page) params.set('page', filter.page.toString());
    if (filter.pageSize) params.set('pageSize', filter.pageSize.toString());

    const query = params.toString();
    return request<TransactionPage>(`/profiles/${profileId}/transactions${query ? `?${query}` : ''}`);
  },

  getById: (id: string) =>
    request<Transaction>(`/transactions/${id}`),

  update: (id: string, data: UpdateTransactionRequest) =>
    request<Transaction>(`/transactions/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  delete: (id: string) =>
    request<void>(`/transactions/${id}`, {
      method: 'DELETE',
    }),
};

// Import API
export const importApi = {
  upload: async (accountId: string, file: File): Promise<UploadResponse> => {
    const formData = new FormData();
    formData.append('file', file);

    const response = await fetch(`${API_BASE}/accounts/${accountId}/import/upload`, {
      method: 'POST',
      body: formData,
    });

    return handleResponse<UploadResponse>(response);
  },

  preview: (sessionId: string, formatOverride?: CsvFormatConfig) =>
    request<PreviewResponse>(`/import/${sessionId}/preview`, {
      method: 'POST',
      body: JSON.stringify({ formatOverride }),
    }),

  confirm: (sessionId: string, formatOverride?: CsvFormatConfig, skipDuplicates = true) =>
    request<ConfirmImportResponse>(`/import/${sessionId}/confirm`, {
      method: 'POST',
      body: JSON.stringify({ formatOverride, skipDuplicates }),
    }),

  getSessions: (accountId: string) =>
    request<ImportSession[]>(`/accounts/${accountId}/import/sessions`),
};

// Import Format API
export const importFormatsApi = {
  getAll: (profileId: string) =>
    request<ImportFormat[]>(`/profiles/${profileId}/import-formats`),

  getById: (id: string) =>
    request<ImportFormat>(`/import-formats/${id}`),

  create: (profileId: string, data: CreateImportFormatRequest) =>
    request<ImportFormat>(`/profiles/${profileId}/import-formats`, {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (id: string, data: UpdateImportFormatRequest) =>
    request<ImportFormat>(`/import-formats/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  delete: (id: string) =>
    request<void>(`/import-formats/${id}`, {
      method: 'DELETE',
    }),
};

// Rules API
export const rulesApi = {
  getAll: (profileId: string) =>
    request<Rule[]>(`/profiles/${profileId}/rules`),

  getById: (id: string) =>
    request<Rule>(`/rules/${id}`),

  create: (profileId: string, data: CreateRuleRequest) =>
    request<Rule>(`/profiles/${profileId}/rules`, {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (id: string, data: UpdateRuleRequest) =>
    request<Rule>(`/rules/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  delete: (id: string) =>
    request<void>(`/rules/${id}`, {
      method: 'DELETE',
    }),

  test: (profileId: string, data: TestRulesRequest) =>
    request<TestRulesResponse>(`/profiles/${profileId}/rules/test`, {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  apply: (profileId: string, onlyUncategorized = true) =>
    request<{ transactionsUpdated: number }>(`/profiles/${profileId}/rules/apply`, {
      method: 'POST',
      body: JSON.stringify({ onlyUncategorized }),
    }),
};
