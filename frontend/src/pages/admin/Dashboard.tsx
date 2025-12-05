/**
 * Admin Dashboard Page
 * Shows overview statistics and quick actions
 */

import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { reportService } from '../../services';
import type { DashboardSummary } from '../../types';
import styles from './Dashboard.module.css';

interface StatCardProps {
  title: string;
  value: string | number;
  icon: string;
  trend?: {
    value: number;
    isPositive: boolean;
  };
  link?: string;
}

const StatCard: React.FC<StatCardProps> = ({ title, value, icon, trend, link }) => {
  const content = (
    <div className={styles.statCard}>
      <div className={styles.statIcon}>{icon}</div>
      <div className={styles.statContent}>
        <span className={styles.statValue}>{value}</span>
        <span className={styles.statTitle}>{title}</span>
        {trend && (
          <span className={trend.isPositive ? styles.trendPositive : styles.trendNegative}>
            {trend.isPositive ? '↑' : '↓'} {Math.abs(trend.value)}%
          </span>
        )}
      </div>
    </div>
  );

  if (link) {
    return (
      <Link to={link} className={styles.statLink}>
        {content}
      </Link>
    );
  }

  return content;
};

export const Dashboard: React.FC = () => {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchDashboard = async () => {
      try {
        const data = await reportService.getDashboardSummary();
        setSummary(data);
      } catch (err) {
        setError('Failed to load dashboard data');
        console.error('Dashboard error:', err);
      } finally {
        setLoading(false);
      }
    };

    void fetchDashboard();
  }, []);

  if (loading) {
    return (
      <div className={styles.loading}>
        <div className={styles.spinner} />
        <p>Loading dashboard...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className={styles.error}>
        <p>{error}</p>
        <button onClick={() => window.location.reload()}>Retry</button>
      </div>
    );
  }

  if (!summary) {
    return null;
  }

  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('en-ZA', {
      style: 'currency',
      currency: 'ZAR',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(value);
  };

  return (
    <div className={styles.dashboard}>
      {/* Stats Grid */}
      <div className={styles.statsGrid}>
        <StatCard
          title="Active Competitions"
          value={summary.activeCompetitions}
          icon="🏆"
          link="/admin/competitions?status=active"
        />
        <StatCard
          title="Total Registrations"
          value={summary.totalRegistrations.toLocaleString()}
          icon="📝"
          link="/admin/registrations"
        />
        <StatCard
          title="Prizes Awarded"
          value={summary.totalPrizesAwarded.toLocaleString()}
          icon="🏅"
          link="/admin/awards"
        />
        <StatCard
          title="Prizes Redeemed"
          value={summary.totalPrizesRedeemed.toLocaleString()}
          icon="✅"
          link="/admin/redemptions"
        />
        <StatCard
          title="Pending Redemptions"
          value={summary.pendingRedemptions}
          icon="⏳"
          link="/admin/redemptions?status=pending"
        />
        <StatCard
          title="Total Value Awarded"
          value={formatCurrency(summary.totalPrizeValueAwarded)}
          icon="💵"
        />
        <StatCard
          title="Total Value Redeemed"
          value={formatCurrency(summary.totalPrizeValueRedeemed)}
          icon="💰"
        />
      </div>

      {/* Quick Actions */}
      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>Quick Actions</h2>
        <div className={styles.quickActions}>
          <Link to="/admin/competitions/new" className={styles.actionButton}>
            <span className={styles.actionIcon}>➕</span>
            <span>New Competition</span>
          </Link>
          <Link to="/admin/prize-pools/new" className={styles.actionButton}>
            <span className={styles.actionIcon}>🎁</span>
            <span>New Prize Pool</span>
          </Link>
          <Link to="/admin/awards/new" className={styles.actionButton}>
            <span className={styles.actionIcon}>🏅</span>
            <span>Award Prize</span>
          </Link>
          <Link to="/admin/reports" className={styles.actionButton}>
            <span className={styles.actionIcon}>📊</span>
            <span>View Reports</span>
          </Link>
        </div>
      </section>

      {/* Recent Competitions */}
      <section className={styles.section}>
        <div className={styles.sectionHeader}>
          <h2 className={styles.sectionTitle}>Recent Competitions</h2>
          <Link to="/admin/competitions" className={styles.viewAll}>
            View All →
          </Link>
        </div>
        <div className={styles.tableContainer}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th>Name</th>
                <th>Status</th>
                <th>Registrations</th>
                <th>Awards</th>
                <th>End Date</th>
              </tr>
            </thead>
            <tbody>
              {summary.recentCompetitions.map((comp) => (
                <tr key={comp.competitionId}>
                  <td>
                    <Link to={`/admin/competitions/${String(comp.competitionId)}`}>
                      {comp.name}
                    </Link>
                  </td>
                  <td>
                    <span
                      className={[styles.status, styles[comp.status.toLowerCase()]]
                        .filter(Boolean)
                        .join(' ')}
                    >
                      {comp.status}
                    </span>
                  </td>
                  <td>{comp.registrationCount.toLocaleString()}</td>
                  <td>{comp.awardedCount.toLocaleString()}</td>
                  <td>{new Date(comp.endDate).toLocaleDateString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
};
