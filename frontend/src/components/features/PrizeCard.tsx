import { type PrizeSummaryResponse } from '../../types';
import styles from './PrizeCard.module.css';

interface PrizeCardProps {
  prize: PrizeSummaryResponse;
  onSelect?: (id: number) => void;
}

export const PrizeCard: React.FC<PrizeCardProps> = ({ prize, onSelect }) => {
  const handleClick = () => {
    onSelect?.(prize.prizeId);
  };

  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('en-ZA', {
      style: 'currency',
      currency: 'ZAR',
    }).format(value);
  };

  const getStatusClass = (isActive: boolean): string => {
    return isActive ? (styles.active ?? '') : (styles.inactive ?? '');
  };

  return (
    <div className={styles.card} onClick={handleClick}>
      <h3 className={styles.name}>{prize.name}</h3>
      <p className={styles.description}>{prize.prizeTypeName}</p>
      <div className={styles.footer}>
        <span className={styles.value}>
          {prize.monetaryValue != null ? formatCurrency(prize.monetaryValue) : 'N/A'}
        </span>
        <span className={[styles.status, getStatusClass(prize.isActive)].filter(Boolean).join(' ')}>
          {prize.isActive ? 'Active' : 'Inactive'}
        </span>
      </div>
      <div className={styles.quantity}>
        {prize.remainingQuantity} / {prize.totalQuantity} available
      </div>
    </div>
  );
};
