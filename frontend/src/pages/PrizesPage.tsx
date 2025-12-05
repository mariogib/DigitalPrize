import { PrizeCard } from '../components/features/PrizeCard';
import { usePrizes } from '../hooks/usePrizes';
import styles from './PrizesPage.module.css';

export const PrizesPage: React.FC = () => {
  const { prizes, isLoading, error } = usePrizes();

  if (isLoading) {
    return <div className={styles.loading}>Loading prizes...</div>;
  }

  if (error) {
    return <div className={styles.error}>Error loading prizes: {error.message}</div>;
  }

  return (
    <div className={styles.container}>
      <h1 className={styles.title}>Available Prizes</h1>
      {prizes.length === 0 ? (
        <p className={styles.empty}>No prizes available at the moment.</p>
      ) : (
        <div className={styles.grid}>
          {prizes.map((prize) => (
            <PrizeCard key={prize.prizeId} prize={prize} />
          ))}
        </div>
      )}
    </div>
  );
};
