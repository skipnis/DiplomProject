import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { API_URL } from '../api/client';
import type { NotificationDto } from '../types';

const BACKOFF_DELAYS_MS = [1000, 2000, 4000, 8000, 16000, 30000];

async function tryRefreshToken(): Promise<void> {
  await fetch(`${API_URL}/auth/refresh`, { method: 'POST', credentials: 'include' });
}

export function useNotificationsHub(
  isAuthenticated: boolean,
  onNotification: (n: NotificationDto) => void,
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

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/notifications`, { withCredentials: true })
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on('ReceiveNotification', (n: NotificationDto) => {
      onNotificationRef.current(n);
    });

    const connect = async () => {
      while (!stopped) {
        try {
          await connection.start();
          attempt = 0;
          onConnectedRef.current?.();
          return;
        } catch {
          if (stopped) return;
          try {
            await tryRefreshToken();
          } catch {
            // refresh token expired — keep retrying
          }
          const delay = BACKOFF_DELAYS_MS[Math.min(attempt, BACKOFF_DELAYS_MS.length - 1)];
          attempt++;
          await new Promise((resolve) => setTimeout(resolve, delay));
        }
      }
    };

    connection.onclose(async () => {
      if (stopped) return;
      try {
        await tryRefreshToken();
      } catch {
        // ignore
      }
      await connect();
    });

    connect();

    return () => {
      stopped = true;
      connection.stop();
    };
  }, [isAuthenticated]);
}
