import { Link } from 'react-router-dom';
import { useTheme } from '../../contexts';
import styles from './Header.module.css';

export const Header: React.FC = () => {
  const { logoUrl, tenantName } = useTheme();

  return (
    <header className={styles.header}>
      <div className={styles.container}>
        <Link to="/" className={styles.logo}>
          {logoUrl ? (
            <img src={logoUrl} alt={tenantName ?? 'Logo'} className={styles.logoImage} />
          ) : (
            <>
              <span className={styles.logoIcon}>🎯</span>
              <span className={styles.logoText}>
                <span className={styles.logoBrand}>{tenantName ?? 'WORLDPLAY'}</span>
                <span className={styles.logoSub}>Digital Prizes</span>
              </span>
            </>
          )}
        </Link>
        <nav className={styles.nav}>
          <Link to="/" className={styles.navLink}>
            Home
          </Link>
          <Link to="/prizes" className={styles.navLink}>
            Prizes
          </Link>
          <Link to="/status" className={styles.navLink}>
            Check Status
          </Link>
          <Link to="/admin" className={styles.adminLink}>
            Admin Portal
          </Link>
        </nav>
      </div>
    </header>
  );
};
