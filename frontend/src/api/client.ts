import { ApiError } from '../utils/errors';

export const API_URL = import.meta.env.VITE_API_URL || '/api';
export const STORAGE_URL = import.meta.env.VITE_STORAGE_URL || 'http://localhost:9000/wishes';

export function getImageUrl(imagePath: string | null | undefined): string | null {
  if (!imagePath) return null;
  if (imagePath.startsWith('http')) return imagePath;
  return `${STORAGE_URL}/${imagePath}`;
}

export function getQrImageUrl(path: string): string {
  return `${API_URL}${path}`;
}

let isRefreshing = false;
let pendingQueue: Array<{ resolve: () => void; reject: (err: unknown) => void }> = [];

function processPendingQueue(error: unknown) {
  pendingQueue.forEach(({ resolve, reject }) => {
    if (error) reject(error);
    else resolve();
  });
  pendingQueue = [];
}

async function request<T>(
  method: string,
  path: string,
  body?: unknown,
  isFormData = false,
): Promise<T> {
  const headers: Record<string, string> = {};

  if (body && !isFormData) {
    headers['Content-Type'] = 'application/json';
  }

  const res = await fetch(`${API_URL}${path}`, {
    method,
    headers,
    credentials: 'include',
    body: isFormData
      ? (body as FormData)
      : body !== undefined
        ? JSON.stringify(body)
        : undefined,
  });

  if (res.status === 401 && path !== '/auth/refresh') {
    if (isRefreshing) {
      return new Promise<T>((resolve, reject) => {
        pendingQueue.push({
          resolve: () => resolve(request<T>(method, path, body, isFormData)),
          reject,
        });
      });
    }

    isRefreshing = true;

    try {
      const refreshRes = await fetch(`${API_URL}/auth/refresh`, {
        method: 'POST',
        credentials: 'include',
      });

      if (!refreshRes.ok) {
        const err = new ApiError(401, 'Unauthorized');
        processPendingQueue(err);
        isRefreshing = false;
        throw err;
      }

      processPendingQueue(null);
      isRefreshing = false;

      return request<T>(method, path, body, isFormData);
    } catch (err) {
      isRefreshing = false;
      processPendingQueue(err);
      throw err;
    }
  }

  if (res.status === 401) {
    throw new ApiError(401, 'Unauthorized');
  }

  if (!res.ok) {
    const text = await res.text();
    throw new ApiError(res.status, text || `HTTP ${res.status}`);
  }

  if (res.status === 204) return undefined as T;

  const ct = res.headers.get('content-type');
  if (ct?.includes('application/json')) {
    return res.json() as Promise<T>;
  }

  return undefined as T;
}

export function apiGet<T>(path: string): Promise<T> {
  return request<T>('GET', path);
}

export function apiPost<T>(path: string, body?: unknown): Promise<T> {
  return request<T>('POST', path, body);
}

export function apiPut<T>(path: string, body?: unknown): Promise<T> {
  return request<T>('PUT', path, body);
}

export function apiPatch<T>(path: string, body?: unknown): Promise<T> {
  return request<T>('PATCH', path, body);
}

export function apiDelete<T = void>(path: string, body?: unknown): Promise<T> {
  return request<T>('DELETE', path, body);
}

export function apiPostForm<T>(path: string, data: FormData): Promise<T> {
  return request<T>('POST', path, data, true);
}
