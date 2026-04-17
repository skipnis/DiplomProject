import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getCatalogItems } from '../api/catalog';
import { getImageUrl } from '../api/client';
import { buttonVariants } from '@/components/ui/button';
import type { CatalogItemDto } from '../types';

const FEATURES = [
  {
    icon: '📝',
    title: 'Создавай вишлисты',
    description: 'Добавляй желания вручную или парси ссылки из любого магазина — цена и фото подтянутся автоматически.',
  },
  {
    icon: '🤝',
    title: 'Делись с друзьями',
    description: 'Настраивай уровень доступа: для всех, только для друзей или конкретных людей.',
  },
  {
    icon: '🎁',
    title: 'Бронируй подарки',
    description: 'Резервируй желания друзей, чтобы никто не подарил одно и то же. Именинник не узнает.',
  },
  {
    icon: '🗓️',
    title: 'События и напоминания',
    description: 'Привязывай вишлисты к праздникам и синхронизируй события с Google Calendar.',
  },
];

export default function LandingPage() {
  const [carouselItems, setCarouselItems] = useState<CatalogItemDto[]>([]);

  useEffect(() => {
    getCatalogItems({ page: 1, pageSize: 16 })
      .then((response) => setCarouselItems(response.items))
      .catch(() => {});
  }, []);

  return (
    <div className="space-y-24 pb-24">
      <section className="text-center pt-16 pb-4 space-y-6 max-w-2xl mx-auto">
        <div className="text-6xl">🎁</div>
        <h1 className="text-4xl sm:text-5xl font-extrabold tracking-tight leading-tight">
          Делитесь желаниями,{' '}
          <span className="text-primary">дарите радость</span>
        </h1>
        <p className="text-lg text-muted-foreground max-w-xl mx-auto">
          Wishapp — сервис для создания вишлистов и совместного выбора подарков.
          Больше никаких одинаковых презентов и мучительных вопросов «что подарить?»
        </p>
        <div className="flex gap-3 justify-center flex-wrap">
          <Link to="/login" className={buttonVariants({ size: 'lg', className: 'text-base px-8' })}>
            Начать бесплатно
          </Link>
          <Link to="/catalog" className={buttonVariants({ variant: 'outline', size: 'lg', className: 'text-base px-8' })}>
            Смотреть каталог
          </Link>
        </div>
      </section>

      <section className="space-y-8">
        <h2 className="text-2xl font-bold text-center">Как это работает</h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {FEATURES.map((feature) => (
            <div
              key={feature.title}
              className="rounded-xl border bg-card p-5 space-y-2 hover:shadow-md transition-shadow"
            >
              <div className="text-3xl">{feature.icon}</div>
              <h3 className="font-semibold text-base">{feature.title}</h3>
              <p className="text-sm text-muted-foreground leading-relaxed">{feature.description}</p>
            </div>
          ))}
        </div>
      </section>

      {carouselItems.length > 0 && (
        <section className="space-y-6">
          <h2 className="text-2xl font-bold text-center">Популярные товары в каталоге</h2>
          <div className="overflow-hidden relative">
            <div className="absolute left-0 top-0 bottom-0 w-12 z-10 bg-gradient-to-r from-background to-transparent pointer-events-none" />
            <div className="absolute right-0 top-0 bottom-0 w-12 z-10 bg-gradient-to-l from-background to-transparent pointer-events-none" />
            <div className="flex animate-marquee gap-4" style={{ width: 'max-content' }}>
              {[...carouselItems, ...carouselItems].map((item, index) => (
                <CarouselCard key={`${item.id}-${index}`} item={item} />
              ))}
            </div>
          </div>
          <div className="text-center">
            <Link to="/catalog" className={buttonVariants({ variant: 'outline' })}>
              Перейти в каталог →
            </Link>
          </div>
        </section>
      )}

      <section className="rounded-2xl bg-primary/5 border border-primary/20 p-10 text-center space-y-5">
        <h2 className="text-2xl font-bold">Готовы попробовать?</h2>
        <p className="text-muted-foreground max-w-md mx-auto">
          Зарегистрируйтесь за 30 секунд через Google или по email — никакого пароля.
        </p>
        <Link to="/login" className={buttonVariants({ size: 'lg', className: 'text-base px-10' })}>
          Войти / Зарегистрироваться
        </Link>
      </section>
    </div>
  );
}

function CarouselCard({ item }: { item: CatalogItemDto }) {
  const imageUrl = getImageUrl(item.imagePath);
  return (
    <div className="w-44 flex-shrink-0 rounded-xl border bg-card overflow-hidden hover:shadow-md transition-shadow">
      <div className="h-36 bg-muted flex items-center justify-center overflow-hidden">
        {imageUrl ? (
          <img src={imageUrl} alt={item.name} className="w-full h-full object-cover" />
        ) : (
          <span className="text-3xl">🎁</span>
        )}
      </div>
      <div className="p-3 space-y-1">
        <p className="text-sm font-medium leading-tight line-clamp-2">{item.name}</p>
        {item.price != null && (
          <p className="text-sm font-bold text-primary">
            {item.price.toLocaleString('ru-RU')} {item.currency}
          </p>
        )}
      </div>
    </div>
  );
}
