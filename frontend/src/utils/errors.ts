export class ApiError extends Error {
  readonly status: number;
  constructor(status: number, message: string) {
    super(message);
    this.status = status;
    this.name = 'ApiError';
  }
}

// Maps backend error codes to Russian user-friendly messages
const CODE_MESSAGES: Record<string, string> = {
  'Wishlists.NotFound': 'Вишлист не найден',
  'Wishes.NotFound': 'Желание не найдено',
  'Users.NotFound': 'Пользователь не найден',
  'Friendships.NotFound': 'Дружба не найдена',
  'Reservations.NotFound': 'Резервация не найдена',
  'Friendships.AlreadyFriends': 'Вы уже друзья',
  'Friendships.RequestAlreadySent': 'Заявка уже отправлена',
  'Reservations.AlreadyReserved': 'Желание уже зарезервировано',
  'Reservations.CannotReserveOwn': 'Нельзя зарезервировать собственное желание',
  'Users.UsernameAlreadyTaken': 'Имя пользователя уже занято',
};

const STATUS_MESSAGES: Record<number, string> = {
  400: 'Некорректные данные',
  401: 'Необходимо войти в аккаунт',
  403: 'Нет прав доступа',
  404: 'Не найдено',
  409: 'Запись уже существует',
  422: 'Ошибка валидации',
  500: 'Ошибка сервера, попробуйте позже',
  503: 'Сервер временно недоступен',
};

export function parseError(error: unknown): string {
  if (!(error instanceof Error)) return 'Произошла ошибка';

  if (error instanceof ApiError) {
    // Try to extract code from body
    try {
      const body = JSON.parse(error.message);
      if (body.code && CODE_MESSAGES[body.code]) return CODE_MESSAGES[body.code];
      if (body.errors) {
        // Validation errors: { errors: { field: ["msg"] } }
        const messages = Object.values(body.errors).flat() as string[];
        if (messages.length) return messages[0];
      }
    } catch { /* not JSON */ }

    return STATUS_MESSAGES[error.status] ?? `Ошибка ${error.status}`;
  }

  // Plain Error — try to parse message as JSON
  try {
    const body = JSON.parse(error.message);
    if (body.code && CODE_MESSAGES[body.code]) return CODE_MESSAGES[body.code];
    if (body.status && STATUS_MESSAGES[body.status]) return STATUS_MESSAGES[body.status];
    if (body.title) return STATUS_MESSAGES[body.status] ?? body.title;
  } catch { /* not JSON */ }

  // Check for HTTP status in plain message
  const match = error.message.match(/HTTP (\d{3})/);
  if (match) {
    const status = Number(match[1]);
    return STATUS_MESSAGES[status] ?? `Ошибка ${status}`;
  }

  return error.message || 'Произошла ошибка';
}
