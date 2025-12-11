import axios from 'axios';

const API_BASE_URL = 'http://localhost:44387/api';

const api = axios.create({
  baseURL: API_BASE_URL,
});

api.interceptors.request.use((config: any) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Типы
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
}

export interface User {
  id: string;
  email: string;
  name: string | null;
  roles: string[];
}

export interface Role {
  id: string;
  name: string;
  description: string | null;
  isSystem: boolean;
  permissionKeys: string[];
}

export interface Permission {
  key: string;
  resource: string;
  action: string;
  description: string | null;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  name?: string;
  roleIds: string[];
}

export interface UpdateUserRequest {
  name?: string;
  password?: string;
  roleIds?: string[];
}

export interface CreateRoleRequest {
  name: string;
  description?: string;
  permissionKeys: string[];
}

export interface UpdateRolePermissionsRequest {
  permissionKeys: string[];
}

export interface Game {
  id: number;
  title: string | null;
  path: string | null;
  provider: string | null;
  category: string | null;
  tags: string | null;
  isFeatured: boolean;
  display: boolean;
  createdAt: string | null;
  updatedAt: string | null;
  platformNames: string[];
}

export interface GameListResponse {
  games: Game[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface GameListQuery {
  search?: string;
  searchTitle?: string;
  searchPath?: string;
  provider?: string;
  category?: string;
  sortBy?: string;
  sortOrder?: string;
  page?: number;
  pageSize?: number;
}

export interface UpdateGameRequest {
  title?: string;
  path?: string;
  category?: string;
  tags?: string;
}

export interface GameConfigItemResponse {
  gameId: number;
  isEnabled: boolean;
}

export interface Config {
  id: string;
  name: string;
  createdAt: string;
  updatedAt: string | null;
  gameIds: number[];
  games?: GameConfigItemResponse[] | null;
  platformNames: string[];
}

export interface Platform {
  id: string;
  name: string;
  description: string | null;
  url: string | null;
  configId: string;
  configName: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateConfigRequest {
  name: string;
  gameIds: number[];
}

export interface GameConfigItem {
  gameId: number;
  isEnabled: boolean;
}

export interface UpdateConfigRequest {
  name?: string;
  games?: GameConfigItem[] | null;
}

export interface CreatePlatformRequest {
  name: string;
  description?: string;
  url?: string;
  configId: string;
}

export interface UpdatePlatformRequest {
  name?: string;
  description?: string;
  url?: string;
  configId?: string | null;
}

export const authApi = {
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>('/auth/login', data);
    return response.data;
  },
};

export const usersApi = {
  getAll: async (): Promise<User[]> => {
    const response = await api.get<User[]>('/users');
    return response.data;
  },
  create: async (data: CreateUserRequest): Promise<User> => {
    const response = await api.post<User>('/users', data);
    return response.data;
  },
  update: async (id: string, data: UpdateUserRequest): Promise<User> => {
    const response = await api.put<User>(`/users/${id}`, data);
    return response.data;
  },
};

export const rolesApi = {
  getAll: async (): Promise<Role[]> => {
    const response = await api.get<Role[]>('/roles');
    return response.data;
  },
  create: async (data: CreateRoleRequest): Promise<Role> => {
    const response = await api.post<Role>('/roles', data);
    return response.data;
  },
  updatePermissions: async (id: string, data: UpdateRolePermissionsRequest): Promise<Role> => {
    const response = await api.put<Role>(`/roles/${id}/permissions`, data);
    return response.data;
  },
};

export const permissionsApi = {
  getAll: async (): Promise<Permission[]> => {
    const response = await api.get<Permission[]>('/permissions');
    return response.data;
  },
};

export const gamesApi = {
  getAll: async (query?: GameListQuery): Promise<GameListResponse> => {
    const response = await api.get<GameListResponse>('/games', { params: query });
    return response.data;
  },
  getById: async (id: number): Promise<Game> => {
    const response = await api.get<Game>(`/games/${id}`);
    return response.data;
  },
  update: async (id: number, data: UpdateGameRequest): Promise<Game> => {
    const response = await api.put<Game>(`/games/${id}`, data);
    return response.data;
  },
};

export const configsApi = {
  getAll: async (): Promise<Config[]> => {
    const response = await api.get<Config[]>('/configs');
    return response.data;
  },
  getById: async (id: string): Promise<Config> => {
    const response = await api.get<Config>(`/configs/${id}`);
    return response.data;
  },
  create: async (data: CreateConfigRequest): Promise<Config> => {
    const response = await api.post<Config>('/configs', data);
    return response.data;
  },
  update: async (id: string, data: UpdateConfigRequest): Promise<Config> => {
    const response = await api.put<Config>(`/configs/${id}`, data);
    return response.data;
  },
  delete: async (id: string): Promise<void> => {
    await api.delete(`/configs/${id}`);
  },
  export: async (id: string): Promise<void> => {
    const token = localStorage.getItem('token');
    const response = await fetch(`${API_BASE_URL}/configs/${id}/export`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });
    
    if (!response.ok) {
      const errorData = await response.json().catch(() => ({ message: 'Export failed' }));
      throw new Error(errorData.message || 'Export failed');
    }
    
    const blob = await response.blob();
    const contentDisposition = response.headers.get('content-disposition');
    let fileName = `config_${id}.json`;
    
    if (contentDisposition) {
      const fileNameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/i);
      if (fileNameMatch && fileNameMatch[1]) {
        fileName = fileNameMatch[1].replace(/['"]/g, '');
      }
    }
    
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
  },
  import: async (file: File): Promise<Config> => {
    const token = localStorage.getItem('token');
    const formData = new FormData();
    formData.append('file', file);
    
    const response = await fetch(`${API_BASE_URL}/configs/import`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
      },
      body: formData,
    });
    
    if (!response.ok) {
      const errorData = await response.json().catch(() => ({ message: 'Import failed' }));
      throw new Error(errorData.message || 'Import failed');
    }
    
    return response.json();
  },
};

export const platformsApi = {
  getAll: async (): Promise<Platform[]> => {
    const response = await api.get<Platform[]>('/platforms');
    return response.data;
  },
  getById: async (id: string): Promise<Platform> => {
    const response = await api.get<Platform>(`/platforms/${id}`);
    return response.data;
  },
  create: async (data: CreatePlatformRequest): Promise<Platform> => {
    const response = await api.post<Platform>('/platforms', data);
    return response.data;
  },
  update: async (id: string, data: UpdatePlatformRequest): Promise<Platform> => {
    const response = await api.put<Platform>(`/platforms/${id}`, data);
    return response.data;
  },
  delete: async (id: string): Promise<void> => {
    await api.delete(`/platforms/${id}`);
  },
};

