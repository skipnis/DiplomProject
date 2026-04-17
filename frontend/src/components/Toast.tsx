import { createContext, useContext, useCallback, type ReactNode } from 'react';
import { toast } from 'sonner';

interface ToastContextValue {
  showToast: (message: string, type?: 'error' | 'success' | 'info' | 'warning') => void;
  error: (message: string) => void;
  success: (message: string) => void;
  warning: (message: string) => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

export function ToastProvider({ children }: { children: ReactNode }) {
  const showToast = useCallback((message: string, type: 'error' | 'success' | 'info' | 'warning' = 'info') => {
    if (type === 'error') toast.error(message);
    else if (type === 'success') toast.success(message);
    else if (type === 'warning') toast.warning(message);
    else toast(message);
  }, []);

  const error = useCallback((message: string) => showToast(message, 'error'), [showToast]);
  const success = useCallback((message: string) => showToast(message, 'success'), [showToast]);
  const warning = useCallback((message: string) => showToast(message, 'warning'), [showToast]);

  return (
    <ToastContext.Provider value={{ showToast, error, success, warning }}>
      {children}
    </ToastContext.Provider>
  );
}

export function useToast(): ToastContextValue {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error('useToast must be used inside ToastProvider');
  return ctx;
}
