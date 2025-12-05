import { Link } from 'react-router-dom';
import styles from './HomePage.module.css';

export const HomePage: React.FC = () => {
  return (
    <div className={styles.page}>
      {/* Hero Section */}
      <section className={styles.hero}>
        <div className={styles.heroContent}>
          <h1 className={styles.heroTitle}>
            Win Amazing <span className={styles.highlight}>Digital Prizes</span>
          </h1>
          <p className={styles.heroSubtitle}>
            Empowering your business through the power of mobile. Enter competitions, win rewards,
            and experience the future of digital prizes.
          </p>
          <div className={styles.heroActions}>
            <Link to="/register" className={styles.ctaPrimary}>
              Enter Now
            </Link>
            <Link to="/prizes" className={styles.ctaSecondary}>
              View Prizes
            </Link>
          </div>
        </div>
        <div className={styles.heroVisual}>
          <div className={styles.prizeCard}>
            <span className={styles.prizeIcon}>🎁</span>
            <span className={styles.prizeLabel}>Exclusive Rewards</span>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className={styles.features}>
        <div className={styles.container}>
          <h2 className={styles.sectionTitle}>How It Works</h2>
          <div className={styles.featureGrid}>
            <div className={styles.featureCard}>
              <div className={styles.featureIcon}>📱</div>
              <h3>Register</h3>
              <p>Enter your details and unique code to participate in exciting competitions.</p>
            </div>
            <div className={styles.featureCard}>
              <div className={styles.featureIcon}>🎯</div>
              <h3>Play</h3>
              <p>Your entry is automatically entered into the prize draw pool.</p>
            </div>
            <div className={styles.featureCard}>
              <div className={styles.featureIcon}>🏆</div>
              <h3>Win</h3>
              <p>Winners are selected and notified. Redeem your digital prizes instantly!</p>
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className={styles.ctaSection}>
        <div className={styles.container}>
          <h2>Ready to Win?</h2>
          <p>Check your competition status or register for new prizes today.</p>
          <div className={styles.ctaButtons}>
            <Link to="/status" className={styles.ctaPrimary}>
              Check Status
            </Link>
            <Link to="/redeem" className={styles.ctaSecondary}>
              Redeem Prize
            </Link>
          </div>
        </div>
      </section>
    </div>
  );
};
