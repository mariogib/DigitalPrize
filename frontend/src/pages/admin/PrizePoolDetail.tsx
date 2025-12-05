/**
 * Admin Prize Pool Detail Page
 * View and manage prize pool details and prizes
 */

import React, { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { prizePoolService } from '../../services';
import type { PrizePoolDetailResponse, PrizeSummaryResponse } from '../../types';
import styles from './PrizePoolDetail.module.css';

type TabType = 'details' | 'prizes';

export const PrizePoolDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const prizePoolId = parseInt(id ?? '0', 10);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<TabType>('details');

  const [prizePool, setPrizePool] = useState<PrizePoolDetailResponse | null>(null);

  const loadPrizePool = useCallback(async () => {
    if (!prizePoolId) return;

    setLoading(true);
    setError(null);

    try {
      const data = await prizePoolService.getPrizePool(prizePoolId);
      setPrizePool(data);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to load prize pool';
      setError(errorMessage);
      console.error('Load prize pool error:', err);
    } finally {
      setLoading(false);
    }
  }, [prizePoolId]);

  useEffect(() => {
    void loadPrizePool();
  }, [loadPrizePool]);

  const handleToggleStatus = async () => {
    if (!prizePool) return;

    try {
      await prizePoolService.updateStatus(prizePoolId, prizePool.isActive ? 'inactive' : 'active');
      void loadPrizePool();
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to update status';
      setError(errorMessage);
      console.error('Update status error:', err);
    }
  };

  const handleDelete = async () => {
    if (!prizePool) return;

    const confirmed = window.confirm(
      `Are you sure you want to delete "${prizePool.name}"? This action cannot be undone.`
    );

    if (!confirmed) return;

    try {
      await prizePoolService.deletePrizePool(prizePoolId);
      navigate('/admin/prize-pools');
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to delete prize pool';
      setError(errorMessage);
      console.error('Delete prize pool error:', err);
    }
  };

  const formatCurrency = (value?: number): string => {
    if (value === undefined) return '-';
    return new Intl.NumberFormat('en-ZA', {
      style: 'currency',
      currency: 'ZAR',
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
    }).format(value);
  };

  const formatDate = (date?: string): string => {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const getAvailabilityClass = (prize: PrizeSummaryResponse): string => {
    const percentAvailable =
      prize.totalQuantity > 0 ? (prize.remainingQuantity / prize.totalQuantity) * 100 : 0;

    if (percentAvailable === 0) return styles.availabilityNone ?? '';
    if (percentAvailable < 25) return styles.availabilityLow ?? '';
    if (percentAvailable < 50) return styles.availabilityMedium ?? '';
    return styles.availabilityHigh ?? '';
  };

  if (loading) {
    return (
      <div className={styles.page}>
        <div className={styles.loading}>Loading prize pool...</div>
      </div>
    );
  }

  if (error || !prizePool) {
    return (
      <div className={styles.page}>
        <div className={styles.errorState}>
          <h2>Error</h2>
          <p>{error ?? 'Prize pool not found'}</p>
          <Link to="/admin/prize-pools" className={styles.backLink}>
            ← Back to Prize Pools
          </Link>
        </div>
      </div>
    );
  }

  const prizes: PrizeSummaryResponse[] = prizePool.prizes;

  return (
    <div className={styles.page}>
      {/* Header */}
      <div className={styles.header}>
        <div className={styles.headerLeft}>
          <Link to="/admin/prize-pools" className={styles.backLink}>
            ← Back to Prize Pools
          </Link>
          <h1>{prizePool.name}</h1>
          <div className={styles.headerMeta}>
            <span
              className={`${styles.statusBadge ?? ''} ${prizePool.isActive ? (styles.statusActive ?? '') : (styles.statusInactive ?? '')}`}
            >
              {prizePool.isActive ? 'Active' : 'Inactive'}
            </span>
            {prizePool.competitionId && (
              <Link
                to={`/admin/competitions/${String(prizePool.competitionId)}`}
                className={styles.competitionLink}
              >
                {prizePool.competitionName}
              </Link>
            )}
          </div>
        </div>
        <div className={styles.headerActions}>
          <button
            className={prizePool.isActive ? styles.deactivateButton : styles.activateButton}
            onClick={() => void handleToggleStatus()}
          >
            {prizePool.isActive ? 'Deactivate' : 'Activate'}
          </button>
          <button
            className={styles.editButton}
            onClick={() => navigate(`/admin/prize-pools/${String(prizePoolId)}/edit`)}
          >
            Edit
          </button>
          <button className={styles.deleteButton} onClick={() => void handleDelete()}>
            Delete
          </button>
        </div>
      </div>

      {/* Stats Cards */}
      <div className={styles.statsRow}>
        <div className={styles.statCard}>
          <span className={styles.statValue}>{prizePool.totalPrizes}</span>
          <span className={styles.statLabel}>Total Prizes</span>
        </div>
        <div className={`${styles.statCard ?? ''} ${styles.available ?? ''}`}>
          <span className={styles.statValue}>{prizePool.availablePrizes}</span>
          <span className={styles.statLabel}>Available</span>
        </div>
        <div className={`${styles.statCard ?? ''} ${styles.awarded ?? ''}`}>
          <span className={styles.statValue}>{prizePool.awardedPrizes}</span>
          <span className={styles.statLabel}>Awarded</span>
        </div>
        <div className={`${styles.statCard ?? ''} ${styles.redeemed ?? ''}`}>
          <span className={styles.statValue}>{prizePool.redeemedPrizes}</span>
          <span className={styles.statLabel}>Redeemed</span>
        </div>
      </div>

      {/* Tabs */}
      <div className={styles.tabs}>
        <button
          className={`${styles.tab ?? ''} ${activeTab === 'details' ? (styles.activeTab ?? '') : ''}`}
          onClick={() => setActiveTab('details')}
        >
          Details
        </button>
        <button
          className={`${styles.tab ?? ''} ${activeTab === 'prizes' ? (styles.activeTab ?? '') : ''}`}
          onClick={() => setActiveTab('prizes')}
        >
          Prizes ({prizes.length})
        </button>
      </div>

      {/* Tab Content */}
      <div className={styles.tabContent}>
        {activeTab === 'details' && (
          <div className={styles.detailsGrid}>
            <div className={styles.detailCard}>
              <h3>Prize Pool Information</h3>
              <div className={styles.detailRow}>
                <span className={styles.detailLabel}>Description</span>
                <span className={styles.detailValue}>{prizePool.description ?? '-'}</span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.detailLabel}>Competition</span>
                <span className={styles.detailValue}>
                  {prizePool.competitionId ? (
                    <Link to={`/admin/competitions/${String(prizePool.competitionId)}`}>
                      {prizePool.competitionName}
                    </Link>
                  ) : (
                    '-'
                  )}
                </span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.detailLabel}>Created</span>
                <span className={styles.detailValue}>{formatDate(prizePool.createdAt)}</span>
              </div>
            </div>

            <div className={styles.detailCard}>
              <h3>Prize Breakdown</h3>
              <div className={styles.progressSection}>
                <div className={styles.progressBar}>
                  <div
                    className={styles.progressRedeemed}
                    style={{
                      width: `${String(prizePool.totalPrizes > 0 ? (prizePool.redeemedPrizes / prizePool.totalPrizes) * 100 : 0)}%`,
                    }}
                  />
                  <div
                    className={styles.progressAwarded}
                    style={{
                      width: `${String(prizePool.totalPrizes > 0 ? ((prizePool.awardedPrizes - prizePool.redeemedPrizes) / prizePool.totalPrizes) * 100 : 0)}%`,
                    }}
                  />
                </div>
                <div className={styles.progressLegend}>
                  <span className={styles.legendItem}>
                    <span className={`${styles.legendDot ?? ''} ${styles.redeemed ?? ''}`} />
                    Redeemed ({prizePool.redeemedPrizes})
                  </span>
                  <span className={styles.legendItem}>
                    <span className={`${styles.legendDot ?? ''} ${styles.awarded ?? ''}`} />
                    Awarded ({prizePool.awardedPrizes - prizePool.redeemedPrizes})
                  </span>
                  <span className={styles.legendItem}>
                    <span className={`${styles.legendDot ?? ''} ${styles.available ?? ''}`} />
                    Available ({prizePool.availablePrizes})
                  </span>
                </div>
              </div>
            </div>
          </div>
        )}

        {activeTab === 'prizes' && (
          <div className={styles.prizesTab}>
            <div className={styles.tabHeader}>
              <h3>Prizes</h3>
              <Link
                to={`/admin/prizes/new?prizePoolId=${String(prizePoolId)}`}
                className={styles.addButton}
              >
                + Add Prize
              </Link>
            </div>
            {prizes.length === 0 ? (
              <div className={styles.emptyState}>
                <p>No prizes configured for this prize pool.</p>
                <Link
                  to={`/admin/prizes/new?prizePoolId=${String(prizePoolId)}`}
                  className={styles.linkButton}
                >
                  Add Prize
                </Link>
              </div>
            ) : (
              <table className={styles.table}>
                <thead>
                  <tr>
                    <th>Prize</th>
                    <th>Type</th>
                    <th>Value</th>
                    <th>Quantity</th>
                    <th>Status</th>
                    <th>Valid</th>
                  </tr>
                </thead>
                <tbody>
                  {prizes.map((prize) => (
                    <tr key={prize.prizeId}>
                      <td>
                        <Link
                          to={`/admin/prizes/${String(prize.prizeId)}`}
                          className={styles.prizeLink}
                        >
                          {prize.name}
                        </Link>
                      </td>
                      <td>{prize.prizeTypeName}</td>
                      <td>{formatCurrency(prize.monetaryValue)}</td>
                      <td>
                        <div className={styles.quantityCell}>
                          <span
                            className={`${styles.availability ?? ''} ${getAvailabilityClass(prize)}`}
                          >
                            {prize.remainingQuantity}
                          </span>
                          <span className={styles.quantityTotal}>/ {prize.totalQuantity}</span>
                        </div>
                      </td>
                      <td>
                        <span
                          className={`${styles.statusBadge ?? ''} ${prize.isActive ? (styles.statusActive ?? '') : (styles.statusInactive ?? '')}`}
                        >
                          {prize.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td>
                        {prize.expiryDate ? (
                          <span className={styles.validDates}>{formatDate(prize.expiryDate)}</span>
                        ) : (
                          '-'
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
