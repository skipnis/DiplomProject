import { z } from 'zod';

const today = () => {
  const d = new Date();
  d.setHours(0, 0, 0, 0);
  return d;
};

export const profileSchema = z.object({
  username: z
    .string()
    .min(1, 'Обязательное поле')
    .max(50, 'Не более 50 символов')
    .regex(/^[a-zA-Z0-9_]+$/, 'Только буквы, цифры и символ _'),
  bio: z.string().max(500, 'Не более 500 символов').optional(),
  birthDate: z.string().optional(),
});

export const wishSchema = z.object({
  name: z.string().min(1, 'Обязательное поле').max(200, 'Не более 200 символов'),
  description: z.string().max(1000, 'Не более 1000 символов').optional(),
  url: z
    .string()
    .refine((v) => !v || URL.canParse(v), 'Введите корректный URL')
    .optional(),
  price: z
    .string()
    .refine((v) => !v || Number(v) > 0, 'Цена должна быть больше 0')
    .optional(),
});

export const wishlistSchema = z.object({
  name: z.string().min(1, 'Обязательное поле').max(100, 'Не более 100 символов'),
  description: z.string().max(500, 'Не более 500 символов').optional(),
});

export const eventSchema = z.object({
  title: z.string().min(1, 'Обязательное поле').max(200, 'Не более 200 символов'),
  description: z.string().max(2000, 'Не более 2000 символов').optional(),
  date: z
    .string()
    .min(1, 'Обязательное поле')
    .refine((v) => new Date(v) >= today(), 'Дата должна быть сегодня или позже'),
});

export const catalogItemSchema = z.object({
  name: z.string().min(1, 'Обязательное поле').max(200, 'Не более 200 символов'),
  description: z.string().max(2000, 'Не более 2000 символов').optional(),
  url: z
    .string()
    .refine((v) => !v || URL.canParse(v), 'Введите корректный URL')
    .optional(),
  price: z
    .string()
    .refine((v) => !v || Number(v) > 0, 'Цена должна быть больше 0')
    .optional(),
  categoryId: z.string().min(1, 'Выберите категорию'),
});

export const catalogCategorySchema = z.object({
  name: z.string().min(1, 'Обязательное поле').max(100, 'Не более 100 символов'),
  order: z
    .string()
    .refine((v) => v !== '' && Number.isInteger(Number(v)) && Number(v) >= 0, 'Порядок должен быть целым числом ≥ 0'),
});

export const catalogCollectionSchema = z.object({
  name: z.string().min(1, 'Обязательное поле').max(150, 'Не более 150 символов'),
  description: z.string().max(500, 'Не более 500 символов').optional(),
  order: z
    .string()
    .refine((v) => v !== '' && Number.isInteger(Number(v)) && Number(v) >= 0, 'Порядок должен быть целым числом ≥ 0'),
});

export type FormErrors = Record<string, string>;

export function parseZodErrors(error: z.ZodError): FormErrors {
  const errs: FormErrors = {};
  const fieldErrors = error.flatten().fieldErrors as Record<string, string[] | undefined>;
  for (const [field, msgs] of Object.entries(fieldErrors)) {
    if (msgs?.[0]) errs[field] = msgs[0];
  }
  return errs;
}
