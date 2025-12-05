/**
 * Awards List Page
 * Displays a paginated list of all prize awards with filtering options
 */

import React, { useCallback, useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { awardService } from '../../services';
import type { PagedResponse, PrizeAwardResponse } from '../../types';
import styles from './Awards.module.css';

const PAGE_SIZE = 10;

export const Awards: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [awards, setAwards] = useState<PagedResponse<PrizeAwardResponse> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const currentPage = Number(searchParams.get('page')) || 1;
  const statusFilter = searchParams.get('status') ?? '';
  const searchTerm = searchParams.get('search') ?? '';

  const fetchAwards = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const params: {
        pageNumber: number;
        pageSize: number;
        status?: string;
        searchTerm?: string;
      } = {
        pageNumber: currentPage,
        pageSize: PAGE_SIZE,
      };

      if (statusFilter) {
        params.status = statusFilter;
      }
      if (searchTerm) {
        params.searchTerm = searchTerm;
      }

      const data = await awardService.getAwards(params);
      setAwards(data);
    } catch (err) {
      setError('Failed to load awards');
      console.error('Awards error:', err);
    } finally {
      setLoading(false);
    }
  }, [currentPage, statusFilter, searchTerm]);

  useEffect(() => {
    void fetchAwards();
  }, [fetchAwards]);

  const handleSearch = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    const search = formData.get('search') as string;
    setSearchParams((prev) => {
      prev.set('search', search);
      prev.set('page', '1');
      return prev;
    });
  };

  const handleStatusFilter = (status: string) => {
    setSearchParams((prev) => {
      if (status) {
        prev.set('status', status);
      } else {
        prev.delete('status');
      }
      prev.set('page', '1');
      return prev;
    });
  };

  const handlePageChange = (page: number) => {
    setSearchParams((prev) => {
      prev.set('page', String(page));
      return prev;
    });
  };

  const getStatusClass = (status: string): string => {
    const statusMap: Record<string, string> = {
      awarded: styles.statusAwarded ?? '',
      redeemed: styles.statusRedeemed ?? '',
      expired: styles.statusExpired ?? '',
      cancelled: styles.statusCancelled ?? '',
    };
    return statusMap[status.toLowerCase()] ?? '';
  };

  const formatCurrency = (value?: number): string => {
    if (value === undefined) return '-';
    return new Intl.NumberFormat('en-ZA', {
      style: 'currency',
      currency: 'ZAR',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(value);
  };

  if (loading && !awards) {
    return (
      <div className={styles.loading}>
        <div className={styles.spinner} />
        <p>Loading awards...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className={styles.error}>
        <p>{error}</p>
        <button onClick={() => void fetchAwards()}>Retry</button>
      </div>
    );
  }

  return (
    <div className={styles.awards}>
      <div className={styles.header}>
        <h1>Prize Awards</h1>
        <Link to="/admin/awards/new" className={styles.createButton}>
          + Award Prize
        </Link>
      </div>

      {/* Filters */}
      <div className={styles.filters}>
        <form onSubmit={handleSearch} className={styles.searchForm}>
          <input
            type="text"
            name="search"
            placeholder="Search by cell number..."
            defaultValue={searchTerm}
            className={styles.searchInput}
          />
          <button type="submit" className={styles.searchButton}>
            Search
          </button>
        </form>

        <div className={styles.statusFilters}>
          <button
            className={[styles.filterButton, !statusFilter ? styles.active : '']
              .filter(Boolean)
              .join(' ')}
            onClick={() => handleStatusFilter('')}
          >
            All
          </button>
          <button
            className={[styles.filterButton, statusFilter === 'Awarded' ? styles.active : '']
              .filter(Boolean)
              .join(' ')}
            onClick={() => handleStatusFilter('Awarded')}
          >
            Awarded
          </button>
          <button
            className={[styles.filterButton, statusFilter === 'Redeemed' ? styles.active : '']
              .filter(Boolean)
              .join(' ')}
            onClick={() => handleStatusFilter('Redeemed')}
          >
            Redeemed
          </button>
          <button
            className={[styles.filterButton, statusFilter === 'Expired' ? styles.active : '']
              .filter(Boolean)
              .join(' ')}
            onClick={() => handleStatusFilter('Expired')}
          >
            Expired
          </button>
          <button
            className={[styles.filterButton, statusFilter === 'Cancelled' ? styles.active : '']
              .filter(Boolean)
              .join(' ')}
            onClick={() => handleStatusFilter('Cancelled')}
          >
            Cancelled
          </button>
        </div>
      </div>

      {/* Table */}
      {awards && awards.items.length > 0 ? (
        <>
          <div className={styles.tableContainer}>
            <table className={styles.table}>
              <thead>
                <tr>
                  <th>Prize</th>
                  <th>Cell Number</th>
                  <th>Competition</th>
                  <th>Value</th>
                  <th>Status</th>
                  <th>Awarded At</th>
                  <th>Expiry</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {awards.items.map((award) => (
                  <tr key={award.prizeAwardId}>
                    <td>
                      <div className={styles.prizeInfo}>
                        <span className={styles.prizeName}>{award.prizeName}</span>
                        <span className={styles.prizeType}>{award.prizeTypeName}</span>
                      </div>
                    </td>
                    <td>{award.cellNumber}</td>
                    <td>{award.competitionName}</td>
                    <td>{formatCurrency(award.monetaryValue)}</td>
                    <td>
                      <span
                        className={[styles.status, getStatusClass(award.status)]
                          .filter(Boolean)
                          .join(' ')}
                      >
                        {award.status}
                      </span>
                    </td>
                    <td>{new Date(award.awardedAt).toLocaleDateString()}</td>
                    <td>
                      {award.expiryDate ? new Date(award.expiryDate).toLocaleDateString() : '-'}
                    </td>
                    <td>
                      <div className={styles.actions}>
                        <Link
                          to={`/admin/awards/${String(award.prizeAwardId)}`}
                          className={styles.actionButton}
                          title="View"
                        >
                          👁️
                        </Link>
                        {award.status === 'Awarded' && award.isRedeemable && (
                          <button className={styles.actionButton} title="Redeem">
                            ✅
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {awards.totalPages > 1 && (
            <div className={styles.pagination}>
              <button
                className={styles.pageButton}
                disabled={!awards.hasPreviousPage}
                onClick={() => handlePageChange(currentPage - 1)}
              >
                ← Previous
              </button>
              <span className={styles.pageInfo}>
                Page {awards.pageNumber} of {awards.totalPages}
              </span>
              <button
                className={styles.pageButton}
                disabled={!awards.hasNextPage}
                onClick={() => handlePageChange(currentPage + 1)}
              >
                Next →
              </button>
            </div>
          )}
        </>
      ) : (
        <div className={styles.empty}>
          <p>No awards found.</p>
          <Link to="/admin/awards/new" className={styles.createButton}>
            Award your first prize
          </Link>
        </div>
      )}
    </div>
  );
};
