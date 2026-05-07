import { useMemo } from 'react';
import { Link } from 'react-router-dom';

const GIFT_QUOTES = [
  { text: 'Дарить — значит любить.', author: 'Народная мудрость' },
  { text: 'Маленький подарок освещает длинную дорогу.', author: 'Армянская пословица' },
  { text: 'Не дорог подарок — дорога любовь.', author: 'Русская пословица' },
  { text: 'Счастье не в том, чтобы получать подарки, а в том, чтобы их дарить.', author: 'Уильям Шекспир' },
  { text: 'Подарки — это язык любви, который понимают все.', author: 'Гэри Чепмен' },
  { text: 'Лучший подарок — это частица твоего сердца.', author: 'Ральф Уолдо Эмерсон' },
  { text: 'Дар без дарителя мал; умей отдавать себя вместе с подарком.', author: 'Джеймс Рассел Лоуэлл' },
  { text: 'Радость, которую мы дарим другим, возвращается к нам.', author: 'Жан де Лабрюйер' },
  { text: 'Истинный подарок — это нечто нужное, о чём человек не подумал бы купить для себя сам.', author: 'Ирис Мёрдок' },
  { text: 'Подарок — это связь между двумя сердцами.', author: 'Народная мудрость' },
  { text: 'Тот, кто получает подарок с любовью, возвращает половину долга.', author: 'Джордж Герберт' },
  { text: 'Чтобы сделать другого счастливым, не нужно много — лишь внимание и желание.', author: 'Аноним' },
  { text: 'Подарки говорят то, чего не скажут слова.', author: 'Народная мудрость' },
  { text: 'Давать, не ожидая ничего взамен, — вот настоящая щедрость.', author: 'Люк де Клапье' },
  { text: 'Самый ценный подарок — твоё время.', author: 'Народная мудрость' },
  { text: 'Нет ничего приятнее, чем получить то, чего долго желал.', author: 'Цицерон' },
  { text: 'Подарок — это мысль, которую кто-то воплотил ради тебя.', author: 'Аноним' },
  { text: 'Деньги тратятся, а добрый поступок остаётся.', author: 'Русская пословица' },
  { text: 'Радость дающего больше, чем радость принимающего.', author: 'Деяния апостолов 20:35' },
  { text: 'Каждый добрый подарок — это частица той радости, которую стоит разделить.', author: 'Аноним' },
  { text: 'Подари человеку день — и он будет счастлив весь день. Подари ему мечту — и он будет счастлив всю жизнь.', author: 'Аноним' },
  { text: 'Хороший подарок всегда в точку.', author: 'Народная мудрость' },
  { text: 'Не смотри в зубы дарёному коню.', author: 'Русская пословица' },
  { text: 'Лучшее, что можно подарить другу, — своё время и внимание.', author: 'Аноним' },
  { text: 'Дарить — значит видеть в другом человеке то, что он сам в себе не замечает.', author: 'Аноним' },
  { text: 'Бедный дарит от сердца, богатый — от кошелька.', author: 'Народная мудрость' },
  { text: 'Подарок, сделанный вовремя, стоит в сто раз дороже.', author: 'Народная мудрость' },
  { text: 'Когда даёшь — не считай. Когда берёшь — будь благодарен.', author: 'Арабская пословица' },
  { text: 'Настоящий подарок — это сюрприз даже для того, кто его делает.', author: 'Аноним' },
  { text: 'Есть только одно счастье в жизни — любить и быть любимым. Дарить — один из лучших способов это выразить.', author: 'Жорж Санд' },
  { text: 'Подарки — это способ показать, что ты думал о ком-то, даже когда тебя нет рядом.', author: 'Аноним' },
  { text: 'Лучший подарок приходит от души, а не от кошелька.', author: 'Народная мудрость' },
  { text: 'Тот, кто умеет дарить, умеет жить.', author: 'Аноним' },
  { text: 'Доброе слово и железные ворота открывает.', author: 'Русская пословица' },
  { text: 'Щедрость состоит не в том, чтобы давать много, а в том, чтобы давать вовремя.', author: 'Жан де Лабрюйер' },
  { text: 'Не важно, что ты даришь, важно — с каким чувством.', author: 'Аноним' },
  { text: 'Подарок без любви — просто вещь.', author: 'Народная мудрость' },
  { text: 'Тот, кто получает с благодарностью, платит щедрее того, кто даёт.', author: 'Уильям Блейк' },
  { text: 'Великодушие — это давать больше, чем можешь; гордость — это брать меньше, чем нужно.', author: 'Халиль Джебран' },
  { text: 'Доброта — это язык, который слышат глухие и видят слепые.', author: 'Марк Твен' },
];

export default function Footer() {
  const quote = useMemo(() => GIFT_QUOTES[Math.floor(Math.random() * GIFT_QUOTES.length)], []);
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
              <Link to="/" className="text-sm text-muted-foreground hover:text-foreground transition-colors">Главная</Link>
              <Link to="/catalog" className="text-sm text-muted-foreground hover:text-foreground transition-colors">Каталог подарков</Link>
              <Link to="/catalog/collections" className="text-sm text-muted-foreground hover:text-foreground transition-colors">Подборки</Link>
              <Link to="/login" className="text-sm text-muted-foreground hover:text-foreground transition-colors">Войти / Зарегистрироваться</Link>
            </nav>
          </div>
        </div>

        <div className="border-t pt-6 flex flex-col sm:flex-row items-center justify-between gap-4">
          <p className="text-xs text-muted-foreground">© {year} Wishapp. Все права защищены.</p>
          <blockquote className="text-xs text-muted-foreground italic text-center sm:text-right max-w-sm">
            «{quote.text}»
          </blockquote>
        </div>
      </div>
    </footer>
  );
}
