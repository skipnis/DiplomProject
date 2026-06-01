import { useMemo } from 'react';
import { Link } from 'react-router-dom';
import { QUOTES, pickRandom } from '../lib/quotes';

export default function Footer() {
  const quote = useMemo(() => pickRandom(QUOTES), []);
  const year = new Date().getFullYear();

  return (
    <footer className="border-t bg-muted/30 mt-24">
      <div className="max-w-[1100px] mx-auto px-6 py-10">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-8 mb-8">
          <div className="space-y-2">
            <div className="font-bold text-lg">🎁 Wishapp</div>
            <p className="text-sm text-muted-foreground leading-relaxed">
              Создавай вишлисты, бронируй подарки друзей и предлагай идеи анонимно — чтобы каждый праздник был в радость.
            </p>
          </div>

          <div className="space-y-2">
            <div className="text-xs font-semibold uppercase tracking-wide text-muted-foreground mb-3">Навигация</div>
            <nav className="flex flex-col gap-1.5">
              <Link to="/about" className="text-sm text-muted-foreground hover:text-foreground transition-colors">О приложении</Link>
              <Link to="/catalog" className="text-sm text-muted-foreground hover:text-foreground transition-colors">Идеи подарков</Link>
              <Link to="/catalog/collections" className="text-sm text-muted-foreground hover:text-foreground transition-colors">Подборки</Link>
              <Link to="/login" className="text-sm text-muted-foreground hover:text-foreground transition-colors">Войти / Зарегистрироваться</Link>
            </nav>
          </div>
        </div>

        <div className="border-t pt-6 flex flex-col sm:flex-row items-center justify-between gap-4">
          <p className="text-xs text-muted-foreground">© {year} Wishapp. Все права защищены.</p>
          <blockquote className="text-xs text-muted-foreground italic text-center sm:text-right max-w-sm">
            «{quote}»
          </blockquote>
        </div>
      </div>
    </footer>
  );
}
