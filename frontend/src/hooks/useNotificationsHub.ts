import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { API_URL } from '../api/client';
import type { NotificationDto } from '../types';

export function useNotificationsHub(
  isAuthenticated: boolean,
  onNotification: (n: NotificationDto) => void,
) {
  const onNotificationRef = useRef(onNotification);
  onNotificationRef.current = onNotification;

  useEffect(() => {
    if (!isAuthenticated) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/notifications`, { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    connection.on('ReceiveNotification', (n: NotificationDto) => {
      onNotificationRef.current(n);
    });

    connection.start().catch(console.error);

    return () => {
      connection.stop();
    };
  }, [isAuthenticated]);
}
