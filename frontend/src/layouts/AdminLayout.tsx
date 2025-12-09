/**
 * Admin Dashboard Layout
 * Main layout for admin pages with sidebar navigation
 */

import React, { useState } from 'react';
import { Link, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../auth';
import { useTheme } from '../contexts';
import styles from './AdminLayout.module.css';

interface NavItem {
  path: string;
  label: string;
  icon: string;
}

const navItems: NavItem[] = [
  { path: '/admin', label: 'Dashboard', icon: '📊' },
  { path: '/admin/competitions', label: 'Competitions', icon: '🏆' },
  { path: '/admin/prize-pools', label: 'Prize Pools', icon: '🎁' },
  { path: '/admin/awards', label: 'Awards', icon: '🏅' },
  { path: '/admin/registrations', label: 'Registrations', icon: '📝' },
  { path: '/admin/redemptions', label: 'Redemptions', icon: '💰' },
  { path: '/admin/reports', label: 'Reports', icon: '📈' },
];

export const AdminLayout: React.FC = () => {
  const location = useLocation();
  const { claims, signOut, databaseInfo } = useAuth();
  const { logoUrl, tenantName } = useTheme();
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);

  const isActive = (path: string): boolean => {
    if (path === '/admin') {
      return location.pathname === '/admin';
    }
    return location.pathname.startsWith(path);
  };

  const handleSignOut = () => {
    void signOut();
  };

  return (
    <div className={styles.layout}>
      <aside
        className={[styles.sidebar, sidebarCollapsed ? styles.collapsed : '']
          .filter(Boolean)
          .join(' ')}
      >
        <div className={styles.sidebarHeader}>
          <Link to="/admin" className={styles.logo}>
            {logoUrl ? (
              <img src={logoUrl} alt={tenantName ?? 'Logo'} className={styles.logoImage} />
            ) : (
              <span className={styles.logoIcon}>🎯</span>
            )}
            {!sidebarCollapsed && <span className={styles.logoText}>{tenantName ?? 'WORLDPLAY'}</span>}
          </Link>
          <button
            className={styles.collapseButton}
            onClick={() => setSidebarCollapsed(!sidebarCollapsed)}
            aria-label={sidebarCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
          >
            {sidebarCollapsed ? '→' : '←'}
          </button>
        </div>

        <nav className={styles.nav}>
          {navItems.map((item) => (
            <Link
              key={item.path}
              to={item.path}
              className={[styles.navItem, isActive(item.path) ? styles.active : '']
                .filter(Boolean)
                .join(' ')}
              title={sidebarCollapsed ? item.label : undefined}
            >
              <span className={styles.navIcon}>{item.icon}</span>
              {!sidebarCollapsed && <span className={styles.navLabel}>{item.label}</span>}
            </Link>
          ))}
        </nav>

        <div className={styles.sidebarFooter}>
          {!sidebarCollapsed && (
            <Link to="/" className={styles.publicLink}>
              ← Back to Public Site
            </Link>
          )}
        </div>
      </aside>

      <main className={styles.main}>
        <header className={styles.header}>
          <h1 className={styles.pageTitle}>Admin Dashboard</h1>
          <div className={styles.headerActions}>
            {databaseInfo && (
              <span className={styles.dbInfo} title={`Server: ${databaseInfo.Server}`}>
                📁 {databaseInfo.Database}
              </span>
            )}
            <span className={styles.user}>👤 {claims?.name ?? claims?.email ?? 'User'}</span>
            <button className={styles.signOutButton} onClick={handleSignOut}>
              Sign Out
            </button>
          </div>
        </header>
        <div className={styles.content}>
          <Outlet />
        </div>
      </main>
    </div>
  );
};
