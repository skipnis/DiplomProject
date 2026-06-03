import QRCode from 'qrcode';

interface WishShareCardOptions {
  name: string;
  priority: number;
  imagePath: string | null;
  shareToken: string;
  wishlistName: string;
  ownerDisplayName: string;
  storageUrl: string;
}

const PRIORITY_COLORS: Record<number, string> = {
  0: '#6b7280',
  1: '#16a34a',
  2: '#ca8a04',
  3: '#dc2626',
  4: '#9333ea',
};

const PRIORITY_LABELS: Record<number, string> = {
  0: '—',
  1: 'Неплохо бы',
  2: 'Хочу',
  3: 'Очень хочу',
  4: 'Мечта',
};

const TAGLINE = 'Подари мечту — сделай день особенным ✨';

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

export async function generateWishShareCard(options: WishShareCardOptions): Promise<Blob> {
  const { name, priority, imagePath, shareToken, wishlistName, ownerDisplayName, storageUrl } = options;

  const WIDTH = 600;
  const PADDING = 40;
  const FONT = '-apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif';

  const canvas = document.createElement('canvas');
  const ctx = canvas.getContext('2d')!;

  let wishImage: HTMLImageElement | null = null;
  if (imagePath) {
    const imageUrl = imagePath.startsWith('http') ? imagePath : `${storageUrl}/${imagePath}`;
    wishImage = await loadImage(imageUrl);
  }

  const IMAGE_SECTION_HEIGHT = wishImage ? 360 : 0;
  const DECORATOR_HEIGHT = wishImage ? 0 : 140;
  const FOOTER_HEIGHT = 230;

  ctx.font = `bold 34px ${FONT}`;
  const nameLines = wrapText(ctx, name, WIDTH - PADDING * 2);
  const nameLineCount = Math.min(nameLines.length, 3);

  const CONTENT_HEIGHT =
    40 +                                  // top padding
    28 +                                  // owner label
    16 +                                  // gap after owner
    (priority > 0 ? 26 + 20 : 0) +       // badge + gap
    28 + (nameLineCount - 1) * 46 + 24 + // name + gap
    (wishlistName ? 20 + 12 : 0) +        // wishlist name + gap
    20 +                                  // tagline
    32;                                   // gap before footer

  const HEIGHT = IMAGE_SECTION_HEIGHT + DECORATOR_HEIGHT + CONTENT_HEIGHT + FOOTER_HEIGHT;
  canvas.width = WIDTH;
  canvas.height = HEIGHT;

  // Background
  const bgGradient = ctx.createLinearGradient(0, 0, WIDTH, HEIGHT);
  bgGradient.addColorStop(0, '#0d0d1f');
  bgGradient.addColorStop(1, '#130f2e');
  ctx.fillStyle = bgGradient;
  ctx.fillRect(0, 0, WIDTH, HEIGHT);

  let currentY = 0;

  if (wishImage) {
    const imgAspect = wishImage.width / wishImage.height;
    const targetAspect = WIDTH / IMAGE_SECTION_HEIGHT;
    let srcX = 0, srcY = 0, srcW = wishImage.width, srcH = wishImage.height;
    if (imgAspect > targetAspect) {
      srcW = wishImage.height * targetAspect;
      srcX = (wishImage.width - srcW) / 2;
    } else {
      srcH = wishImage.width / targetAspect;
      srcY = (wishImage.height - srcH) / 2;
    }
    ctx.drawImage(wishImage, srcX, srcY, srcW, srcH, 0, 0, WIDTH, IMAGE_SECTION_HEIGHT);
    const overlayGradient = ctx.createLinearGradient(0, IMAGE_SECTION_HEIGHT * 0.4, 0, IMAGE_SECTION_HEIGHT);
    overlayGradient.addColorStop(0, 'rgba(13, 13, 31, 0)');
    overlayGradient.addColorStop(1, 'rgba(13, 13, 31, 1)');
    ctx.fillStyle = overlayGradient;
    ctx.fillRect(0, 0, WIDTH, IMAGE_SECTION_HEIGHT);
    currentY = IMAGE_SECTION_HEIGHT;
  } else {
    ctx.font = '72px serif';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText('🎁', WIDTH / 2, DECORATOR_HEIGHT / 2);
    ctx.textBaseline = 'alphabetic';
    currentY = DECORATOR_HEIGHT;
  }

  currentY += 40;

  // Owner label: "Желание Алексея" (genitive isn't available in JS, so use nominative)
  ctx.font = `500 15px ${FONT}`;
  ctx.textAlign = 'left';
  ctx.fillStyle = 'rgba(255, 255, 255, 0.4)';
  ctx.fillText(`Автор: ${ownerDisplayName}`, PADDING, currentY + 15);
  currentY += 28 + 16;

  // Priority badge
  if (priority > 0) {
    const priorityColor = PRIORITY_COLORS[priority];
    const priorityLabel = PRIORITY_LABELS[priority];
    ctx.font = `bold 13px ${FONT}`;
    ctx.textAlign = 'left';
    const badgeWidth = ctx.measureText(priorityLabel).width + 24;
    const badgeHeight = 26;
    ctx.fillStyle = priorityColor + '30';
    ctx.beginPath();
    ctx.roundRect(PADDING, currentY, badgeWidth, badgeHeight, 13);
    ctx.fill();
    ctx.strokeStyle = priorityColor + '80';
    ctx.lineWidth = 1;
    ctx.stroke();
    ctx.fillStyle = priorityColor;
    ctx.fillText(priorityLabel, PADDING + 12, currentY + 18);
    currentY += badgeHeight + 20;
  }

  // Wish name
  ctx.font = `bold 34px ${FONT}`;
  ctx.fillStyle = '#ffffff';
  ctx.textAlign = 'left';
  nameLines.slice(0, 3).forEach((line, index) => {
    ctx.fillText(line, PADDING, currentY + 28 + index * 46);
  });
  currentY += 28 + (nameLineCount - 1) * 46 + 24;

  // Wishlist name
  if (wishlistName) {
    ctx.font = `15px ${FONT}`;
    ctx.fillStyle = 'rgba(255, 255, 255, 0.4)';
    ctx.fillText(wishlistName, PADDING, currentY + 15);
    currentY += 20 + 12;
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
  const shareUrl = `${window.location.origin}/share/${shareToken}`;
  const QR_SIZE = 148;
  const qrDataUrl = await QRCode.toDataURL(shareUrl, {
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
  ctx.font = `13px ${FONT}`;
  ctx.fillStyle = 'rgba(255, 255, 255, 0.25)';
  ctx.fillText(window.location.host, brandX, brandY + 94);

  return new Promise((resolve, reject) => {
    canvas.toBlob((blob) => {
      if (blob) resolve(blob);
      else reject(new Error('toBlob failed'));
    }, 'image/png');
  });
}
