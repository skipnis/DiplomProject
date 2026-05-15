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
  'Reservations.NotFound': 'Бронирование не найдено',
  'Friendships.AlreadyFriends': 'Вы уже друзья',
  'Friendships.RequestAlreadySent': 'Заявка уже отправлена',
  'Reservations.AlreadyReserved': 'Желание уже забронировано',
  'Reservations.CannotReserveOwn': 'Нельзя забронировать собственное желание',
  'Users.UsernameAlreadyTaken': 'Имя пользователя уже занято',
  'Proposals.NotFound': 'Предложение не найдено',
  'Proposals.RecipientNotFound': 'Получатель не найден или не является другом',
  'Proposals.SelfProposal': 'Нельзя отправить предложение самому себе',
  'Proposals.AlreadyReacted': 'Вы уже ответили на это предложение',
  'Proposals.CatalogItemNotFound': 'Товар каталога не найден',
  'Proposals.WishNotFound': 'Желание не найдено',
  'Request.TooLarge': 'Файл слишком большой, выберите файл меньшего размера',
  'Avatar.TooLarge': 'Аватар должен быть не больше 5 МБ',
  'Image.TooLarge': 'Изображение должно быть не больше 10 МБ',
};

const STATUS_MESSAGES: Record<number, string> = {
  400: 'Некорректные данные',
  401: 'Необходимо войти в аккаунт',
  403: 'Нет прав доступа',
  404: 'Не найдено',
  409: 'Запись уже существует',
  413: 'Файл слишком большой, выберите файл меньшего размера',
  422: 'Ошибка валидации',
  429: 'Слишком много запросов, подождите немного',
  500: 'Ошибка сервера, попробуйте позже',
  503: 'Сервер временно недоступен',
};

export function parseApiFieldErrors(error: unknown): Record<string, string> | null {
  if (!(error instanceof ApiError) || error.status !== 422) return null;
  try {
    const body = JSON.parse(error.message);
    if (!body.errors) return null;
    const errs: Record<string, string> = {};
    for (const [key, msgs] of Object.entries(body.errors as Record<string, string[]>)) {
      const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
      if (Array.isArray(msgs) && msgs[0]) errs[camelKey] = msgs[0];
    }
    return Object.keys(errs).length > 0 ? errs : null;
  } catch {
    return null;
  }
}

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

    return STATUS_MESSAGES[error.status] ?? 'Произошла ошибка, попробуйте позже';
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
    return STATUS_MESSAGES[status] ?? 'Произошла ошибка, попробуйте позже';
  }

  return error.message || 'Произошла ошибка';
}
