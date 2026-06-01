import EmojiPicker, { type EmojiClickData, Theme } from 'emoji-picker-react';
import { useTheme } from '../hooks/useTheme';
import { Dialog, DialogContent } from '@/components/ui/dialog';

interface EmojiPickerPopoverProps {
  open: boolean;
  onClose: () => void;
  onSelect: (emoji: string) => void;
}

export function EmojiPickerPopover({ open, onClose, onSelect }: EmojiPickerPopoverProps) {
  const { theme } = useTheme();

  return (
    <Dialog open={open} onOpenChange={(isOpen) => { if (!isOpen) onClose(); }}>
      <DialogContent showCloseButton={false} className="p-0 max-w-fit border-none bg-transparent ring-0 shadow-none">
        <EmojiPicker
          onEmojiClick={(data: EmojiClickData) => { onSelect(data.emoji); onClose(); }}
          theme={theme === 'dark' ? Theme.DARK : Theme.LIGHT}
          lazyLoadEmojis
        />
      </DialogContent>
    </Dialog>
  );
}
