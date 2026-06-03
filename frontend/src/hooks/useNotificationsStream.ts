import { useEffect, useRef } from 'react';
import { API_URL } from '../api/client';
import type { NotificationDto } from '../types';

const BACKOFF_DELAYS_MS = [1000, 2000, 4000, 8000, 16000, 30000];

export function useNotificationsStream(
  isAuthenticated: boolean,
  onNotification: (notification: NotificationDto) => void,
  onConnected?: () => void,
) {
  const onNotificationRef = useRef(onNotification);
  const onConnectedRef = useRef(onConnected);
  onNotificationRef.current = onNotification;
  onConnectedRef.current = onConnected;

  useEffect(() => {
    if (!isAuthenticated) return;

    let stopped = false;
    let attempt = 0;
    let currentSource: EventSource | null = null;
    let timeoutId: ReturnType<typeof setTimeout>;

    const connect = () => {
      if (stopped) return;

      const source = new EventSource(`${API_URL}/notifications/stream`, { withCredentials: true });
      currentSource = source;

      source.onopen = () => {
        attempt = 0;
        onConnectedRef.current?.();
      };

      source.addEventListener('notification', (event) => {
        try {
          const notification = JSON.parse(event.data) as NotificationDto;
          onNotificationRef.current(notification);
        } catch {
          // ignore malformed events
        }
      });

      source.onerror = () => {
        source.close();
        if (stopped) return;
        const delay = BACKOFF_DELAYS_MS[Math.min(attempt, BACKOFF_DELAYS_MS.length - 1)];
        attempt++;
        timeoutId = setTimeout(connect, delay);
      };
    };

    connect();

    return () => {
      stopped = true;
      clearTimeout(timeoutId);
      currentSource?.close();
    };
  }, [isAuthenticated]);
}
