import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getCatalogItems } from '../api/catalog';
import { getImageUrl } from '../api/client';
import { buttonVariants } from '@/components/ui/button';
import type { CatalogItemSummaryDto } from '../types';

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
    description: 'Привязывай вишлисты к праздникам и следи за предстоящими событиями в удобном календаре.',
  },
];

export default function LandingPage() {
  const [carouselItems, setCarouselItems] = useState<CatalogItemSummaryDto[]>([]);

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

      <section className="space-y-10">
        <div className="text-center space-y-3">
          <h2 className="text-3xl font-extrabold tracking-tight">Анонимные предложения подарка</h2>
          <p className="text-muted-foreground max-w-xl mx-auto text-base leading-relaxed">
            Хочешь узнать, понравится ли подарок — прежде чем его купить?
            Предложи идею анонимно и получи честную реакцию.
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="rounded-2xl border bg-card p-6 space-y-5">
            <div className="flex items-center gap-3">
              <span className="text-3xl">🕵️</span>
              <h3 className="font-bold text-lg">Ты даришь</h3>
            </div>
            <ol className="space-y-4">
              {[
                { step: '1', text: 'Выбери друга, которому хочешь сделать сюрприз' },
                { step: '2', text: 'Подбери подарок из каталога, своего вишлиста или придумай свою идею' },
                { step: '3', text: 'Прикрепи записку под псевдонимом — или оставь полную интригу' },
                { step: '4', text: 'Отправь анонимно. Твоё имя не раскрывается' },
              ].map(({ step, text }) => (
                <li key={step} className="flex gap-3 items-start">
                  <span className="flex-shrink-0 w-6 h-6 rounded-full bg-primary/10 text-primary text-xs font-bold flex items-center justify-center mt-0.5">{step}</span>
                  <span className="text-sm text-muted-foreground leading-relaxed">{text}</span>
                </li>
              ))}
            </ol>
          </div>

          <div className="rounded-2xl border bg-card p-6 space-y-5">
            <div className="flex items-center gap-3">
              <span className="text-3xl">🎀</span>
              <h3 className="font-bold text-lg">Ты получаешь</h3>
            </div>
            <ol className="space-y-4">
              {[
                { step: '1', text: 'Приходит уведомление: «Кто-то анонимно предлагает тебе подарок»' },
                { step: '2', text: 'Открываешь интригу постепенно — сначала название, потом описание, потом фото' },
                { step: '3', text: 'Нажимаешь «Хочу» или «Не моё» — и можешь оставить комментарий' },
                { step: '4', text: 'Отправитель узнаёт твою реакцию, ты по-прежнему не знаешь кто это' },
              ].map(({ step, text }) => (
                <li key={step} className="flex gap-3 items-start">
                  <span className="flex-shrink-0 w-6 h-6 rounded-full bg-primary/10 text-primary text-xs font-bold flex items-center justify-center mt-0.5">{step}</span>
                  <span className="text-sm text-muted-foreground leading-relaxed">{text}</span>
                </li>
              ))}
            </ol>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          {[
            { icon: '🎭', title: 'Полная анонимность', description: 'Имя отправителя никогда не раскрывается — даже после реакции. Только выбранный псевдоним.' },
            { icon: '🎁', title: 'Три источника идей', description: 'Предложи товар из каталога, желание из своего вишлиста или опиши подарок своими словами.' },
            { icon: '✨', title: 'Честная реакция', description: 'Именинник оценивает идею до покупки — без вежливости ради вежливости. Ты получаешь настоящий ответ.' },
          ].map(({ icon, title, description }) => (
            <div key={title} className="rounded-xl border bg-primary/3 p-5 space-y-2">
              <div className="text-2xl">{icon}</div>
              <h4 className="font-semibold text-sm">{title}</h4>
              <p className="text-xs text-muted-foreground leading-relaxed">{description}</p>
            </div>
          ))}
        </div>
      </section>

      <section className="space-y-10">
        <div className="text-center space-y-3">
          <h2 className="text-3xl font-extrabold tracking-tight">Стань мастером подарков</h2>
          <p className="text-muted-foreground max-w-xl mx-auto text-base leading-relaxed">
            Каждый подарок делает тебя лучше. Собирай бейджи, открывай достижения и прокачивай свой профиль дарителя.
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="rounded-2xl border bg-card p-6 space-y-4 md:col-span-1">
            <div className="text-3xl">🏅</div>
            <h3 className="font-bold text-lg">Бейджи дарителя</h3>
            <p className="text-sm text-muted-foreground leading-relaxed">
              Именинник оценивает подарок и выставляет тебе бейдж — «Читает мысли», «Самый щедрый», «Открыватель новинок» и другие.
              Каждый бейдж отражает твой стиль дарения.
            </p>
            <div className="flex flex-wrap gap-2 pt-1">
              {[
                { emoji: '🔮', label: 'Читает мысли' },
                { emoji: '💎', label: 'Самый щедрый' },
                { emoji: '🚀', label: 'Открыватель' },
                { emoji: '🎯', label: 'В яблочко' },
              ].map(({ emoji, label }) => (
                <span key={label} className="text-xs px-2.5 py-1 rounded-full bg-primary/8 text-primary border border-primary/20">
                  {emoji} {label}
                </span>
              ))}
            </div>
          </div>

          <div className="rounded-2xl border bg-card p-6 space-y-4 md:col-span-1">
            <div className="text-3xl">🏆</div>
            <h3 className="font-bold text-lg">Достижения</h3>
            <p className="text-sm text-muted-foreground leading-relaxed">
              Выполняй задания и открывай уникальные достижения. Прогресс отслеживается автоматически — просто дари.
            </p>
            <div className="space-y-2.5 pt-1">
              {[
                { icon: '🎁', label: 'Первый подарок', done: true },
                { icon: '✨', label: '5 исполненных желаний', done: true },
                { icon: '🌟', label: 'Коллекционер бейджей', done: false },
              ].map(({ icon, label, done }) => (
                <div key={label} className="flex items-center gap-2.5">
                  <span className={`text-lg ${done ? '' : 'grayscale opacity-40'}`}>{icon}</span>
                  <span className={`text-sm ${done ? 'text-foreground' : 'text-muted-foreground'}`}>{label}</span>
                  {done && <span className="ml-auto text-xs text-primary font-medium">✓</span>}
                </div>
              ))}
            </div>
          </div>

          <div className="rounded-2xl border bg-card p-6 space-y-4 md:col-span-1">
            <div className="text-3xl">⚡</div>
            <h3 className="font-bold text-lg">Уровень дарителя</h3>
            <p className="text-sm text-muted-foreground leading-relaxed">
              Чем больше желаний исполняешь и чем разнообразнее твои бейджи — тем выше твой уровень. Публичный профиль дарителя виден друзьям.
            </p>
            <div className="space-y-2 pt-1">
              <div className="flex items-center justify-between text-sm">
                <span className="font-medium">Уровень 3 · Добрый ангел</span>
                <span className="text-muted-foreground text-xs">→ Уровень 4</span>
              </div>
              <div className="h-2 rounded-full bg-muted overflow-hidden">
                <div className="h-full rounded-full bg-primary w-[62%]" />
              </div>
              <p className="text-xs text-muted-foreground">62% до следующего уровня</p>
            </div>
          </div>
        </div>
      </section>

      {carouselItems.length > 0 && (
        <section className="space-y-6">
          <div className="text-center space-y-3">
            <h2 className="text-3xl font-extrabold tracking-tight">Не знаешь, что подарить?</h2>
            <p className="text-muted-foreground max-w-xl mx-auto text-base leading-relaxed">
              В каталоге собраны сотни идей на любой повод и бюджет. А в подборках — готовые наборы для тех, кто хочет вдохновения, а не поиска.
            </p>
          </div>

          <div className="overflow-hidden relative">
            <div className="absolute left-0 top-0 bottom-0 w-12 z-10 bg-gradient-to-r from-background to-transparent pointer-events-none" />
            <div className="absolute right-0 top-0 bottom-0 w-12 z-10 bg-gradient-to-l from-background to-transparent pointer-events-none" />
            <div className="flex animate-marquee gap-4" style={{ width: 'max-content' }}>
              {[...carouselItems, ...carouselItems].map((item, index) => (
                <CarouselCard key={`${item.id}-${index}`} item={item} />
              ))}
            </div>
          </div>

          <div className="flex gap-3 justify-center flex-wrap">
            <Link to="/catalog" className={buttonVariants({ size: 'lg' })}>
              Смотреть каталог идей
            </Link>
            <Link to="/catalog/collections" className={buttonVariants({ variant: 'outline', size: 'lg' })}>
              Готовые подборки →
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

function CarouselCard({ item }: { item: CatalogItemSummaryDto }) {
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
