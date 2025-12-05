import { Link } from 'react-router-dom';
import styles from './Footer.module.css';

export const Footer: React.FC = () => {
  const currentYear = new Date().getFullYear();

  return (
    <footer className={styles.footer}>
      <div className={styles.container}>
        <div className={styles.footerContent}>
          <div className={styles.brand}>
            <div className={styles.logo}>
              <span className={styles.logoIcon}>🎯</span>
              <span className={styles.logoText}>WORLDPLAY</span>
            </div>
            <p className={styles.tagline}>Empowering your business through the power of mobile.</p>
          </div>

          <div className={styles.links}>
            <div className={styles.linkGroup}>
              <h4>Quick Links</h4>
              <Link to="/">Home</Link>
              <Link to="/prizes">Prizes</Link>
              <Link to="/status">Check Status</Link>
            </div>
            <div className={styles.linkGroup}>
              <h4>Support</h4>
              <a
                href="https://www.worldplay.co.za/privacy-policy/"
                target="_blank"
                rel="noopener noreferrer"
              >
                Privacy Policy
              </a>
              <a href="https://www.worldplay.co.za/" target="_blank" rel="noopener noreferrer">
                WorldPlay.co.za
              </a>
            </div>
          </div>
        </div>

        <div className={styles.copyright}>
          <p>
            Developed by{' '}
            <a href="https://www.WorldPlay.co.za/" target="_blank" rel="noopener noreferrer">
              WorldPlay
            </a>
            . All rights reserved © {currentYear}
          </p>
        </div>
      </div>
    </footer>
  );
};
