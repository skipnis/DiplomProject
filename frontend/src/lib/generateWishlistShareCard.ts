import QRCode from 'qrcode';
import { QUOTES, pickRandom } from './quotes';

interface WishlistShareCardOptions {
  name: string;
  emoji: string;
  ownerDisplayName: string;
  wishlistId: string;
  wishCount: number;
  eventTitle?: string;
  eventDate?: string;
}

const TAGLINE = 'Подари что-нибудь из моего вишлиста ✨';

function wrapText(ctx: CanvasRenderingContext2D, text: string, maxWidth: number): string[] {
  const words = text.split(' ');
  const lines: string[] = [];
  let currentLine = '';
  for (const word of words) {
    const testLine = currentLine ? `${currentLine} ${word}` : word;
    if (ctx.measureText(testLine).width > maxWidth && currentLine) {
      lines.push(currentLine);
      currentLine = word;
    } else {
      currentLine = testLine;
    }
  }
  if (currentLine) lines.push(currentLine);
  return lines;
}

function loadImage(url: string): Promise<HTMLImageElement | null> {
  return new Promise((resolve) => {
    const img = new Image();
    img.crossOrigin = 'anonymous';
    img.onload = () => resolve(img);
    img.onerror = () => resolve(null);
    img.src = url;
  });
}

function formatEventDate(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long' });
}

export async function generateWishlistShareCard(options: WishlistShareCardOptions): Promise<Blob> {
  const { name, emoji, ownerDisplayName, wishlistId, wishCount, eventTitle, eventDate } = options;

  const WIDTH = 600;
  const PADDING = 40;
  const FONT = '-apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif';

  const canvas = document.createElement('canvas');
  const ctx = canvas.getContext('2d')!;

  const DECORATOR_HEIGHT = 200;
  const FOOTER_HEIGHT = 230;
  const CONTENT_HEIGHT =
    40 +                              // top padding
    28 + 16 +                         // owner label
    60 + 16 +                         // emoji
    40 + 12 +                         // name
    24 + 12 +                         // wish count
    (eventTitle ? 28 + 12 : 0) +      // event
    20 + 32;                          // tagline + gap before footer

  const HEIGHT = DECORATOR_HEIGHT + CONTENT_HEIGHT + FOOTER_HEIGHT;
  canvas.width = WIDTH;
  canvas.height = HEIGHT;

  // Background
  const bgGradient = ctx.createLinearGradient(0, 0, WIDTH, HEIGHT);
  bgGradient.addColorStop(0, '#0d0d1f');
  bgGradient.addColorStop(1, '#130f2e');
  ctx.fillStyle = bgGradient;
  ctx.fillRect(0, 0, WIDTH, HEIGHT);

  // Subtle dot grid in decorator area
  ctx.fillStyle = 'rgba(255,255,255,0.03)';
  for (let row = 0; row < 4; row++) {
    for (let col = 0; col < 8; col++) {
      ctx.beginPath();
      ctx.arc(col * 80 + 40, row * 55 + 28, 1.5, 0, Math.PI * 2);
      ctx.fill();
    }
  }

  // Large emoji centered in decorator area
  ctx.fillStyle = '#ffffff';
  ctx.font = '90px serif';
  ctx.textAlign = 'center';
  ctx.textBaseline = 'middle';
  ctx.fillText(emoji, WIDTH / 2, DECORATOR_HEIGHT / 2);
  ctx.textBaseline = 'alphabetic';

  let currentY = DECORATOR_HEIGHT + 40;

  // Owner label
  ctx.font = `500 15px ${FONT}`;
  ctx.textAlign = 'left';
  ctx.fillStyle = 'rgba(255, 255, 255, 0.4)';
  ctx.fillText(`Автор: ${ownerDisplayName}`, PADDING, currentY + 15);
  currentY += 28 + 16;

  // Wishlist name
  ctx.font = `bold 38px ${FONT}`;
  ctx.fillStyle = '#ffffff';
  const nameMetrics = ctx.measureText(name);
  const maxNameWidth = WIDTH - PADDING * 2;
  if (nameMetrics.width > maxNameWidth) {
    ctx.font = `bold 28px ${FONT}`;
  }
  ctx.fillText(name, PADDING, currentY + 32);
  currentY += 40 + 12;

  // Wish count
  ctx.font = `15px ${FONT}`;
  ctx.fillStyle = 'rgba(255, 255, 255, 0.45)';
  const wishWord = wishCount === 1 ? 'желание' : wishCount < 5 ? 'желания' : 'желаний';
  ctx.fillText(`${wishCount} ${wishWord}`, PADDING, currentY + 15);
  currentY += 24 + 12;

  // Event (if present)
  if (eventTitle && eventDate) {
    const eventText = `🗓 ${eventTitle} · ${formatEventDate(eventDate)}`;
    ctx.font = `500 15px ${FONT}`;
    ctx.fillStyle = '#818cf8';
    ctx.fillText(eventText, PADDING, currentY + 15);
    currentY += 28 + 12;
  }

  // Tagline
  ctx.font = `italic 14px ${FONT}`;
  ctx.fillStyle = 'rgba(255, 255, 255, 0.3)';
  ctx.fillText(TAGLINE, PADDING, currentY + 15);
  currentY += 20 + 32;

  // Footer separator
  ctx.strokeStyle = 'rgba(255, 255, 255, 0.1)';
  ctx.lineWidth = 1;
  ctx.beginPath();
  ctx.moveTo(PADDING, currentY);
  ctx.lineTo(WIDTH - PADDING, currentY);
  ctx.stroke();
  currentY += 28;

  // QR code
  const wishlistUrl = `${window.location.origin}/wishlists/${wishlistId}`;
  const QR_SIZE = 148;

  const qrDataUrl = await QRCode.toDataURL(wishlistUrl, {
    width: QR_SIZE,
    margin: 1,
    color: { dark: '#ffffff', light: '#00000000' },
  });
  const qrImage = await loadImage(qrDataUrl);
  if (qrImage) {
    ctx.fillStyle = 'rgba(255, 255, 255, 0.06)';
    ctx.beginPath();
    ctx.roundRect(PADDING - 10, currentY - 10, QR_SIZE + 20, QR_SIZE + 20, 12);
    ctx.fill();
    ctx.drawImage(qrImage, PADDING, currentY, QR_SIZE, QR_SIZE);
  }

  // Branding
  const brandX = PADDING + QR_SIZE + 30;
  const brandY = currentY + 28;
  ctx.font = `bold 30px ${FONT}`;
  ctx.fillStyle = '#818cf8';
  ctx.textAlign = 'left';
  ctx.fillText('Wishapp', brandX, brandY);
  ctx.font = `15px ${FONT}`;
  ctx.fillStyle = 'rgba(255, 255, 255, 0.5)';
  ctx.fillText('Открой и исполни', brandX, brandY + 34);
  ctx.fillText('желание друга', brandX, brandY + 56);
  ctx.font = `italic 12px ${FONT}`;
  ctx.fillStyle = 'rgba(255, 255, 255, 0.25)';
  const quote = `«${pickRandom(QUOTES)}»`;
  const maxQuoteWidth = WIDTH - brandX - PADDING;
  const quoteLines = wrapText(ctx, quote, maxQuoteWidth).slice(0, 3);
  quoteLines.forEach((line, index) => {
    ctx.fillText(line, brandX, brandY + 88 + index * 16);
  });

  return new Promise((resolve, reject) => {
    canvas.toBlob((blob) => {
      if (blob) resolve(blob);
      else reject(new Error('toBlob failed'));
    }, 'image/png');
  });
}
