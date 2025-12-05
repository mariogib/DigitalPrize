/**
 * Prize Pools List Page
 * Displays a paginated list of all prize pools with filtering options
 */

import React, { useCallback, useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { prizePoolService } from '../../services';
import type { PagedResponse, PrizePoolSummaryResponse } from '../../types';
import styles from './PrizePools.module.css';

const PAGE_SIZE = 10;

export const PrizePools: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [prizePools, setPrizePools] = useState<PagedResponse<PrizePoolSummaryResponse> | null>(
    null
  );
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const currentPage = Number(searchParams.get('page')) || 1;
  const competitionFilter = searchParams.get('competition') ?? '';
  const searchTerm = searchParams.get('search') ?? '';

  const fetchPrizePools = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const params: {
        pageNumber: number;
        pageSize: number;
        searchTerm?: string;
      } = {
        pageNumber: currentPage,
        pageSize: PAGE_SIZE,
      };

      if (searchTerm) {
        params.searchTerm = searchTerm;
      }

      // If a competition filter is provided, use the dedicated method
      const data = competitionFilter
        ? await prizePoolService.getPrizePoolsForCompetition(Number(competitionFilter), params)
        : await prizePoolService.getPrizePools(params);

      setPrizePools(data);
    } catch (err) {
      setError('Failed to load prize pools');
      console.error('Prize pools error:', err);
    } finally {
      setLoading(false);
    }
  }, [currentPage, competitionFilter, searchTerm]);

  useEffect(() => {
    void fetchPrizePools();
  }, [fetchPrizePools]);

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

  const handlePageChange = (page: number) => {
    setSearchParams((prev) => {
      prev.set('page', String(page));
      return prev;
    });
  };

  const getAvailabilityClass = (available: number, total: number): string => {
    const ratio = available / total;
    if (ratio <= 0) return styles.availabilityNone ?? '';
    if (ratio <= 0.25) return styles.availabilityLow ?? '';
    if (ratio <= 0.5) return styles.availabilityMedium ?? '';
    return styles.availabilityHigh ?? '';
  };

  const handleDelete = async (pool: PrizePoolSummaryResponse) => {
    const confirmed = window.confirm(
      `Are you sure you want to delete "${pool.name}"? This action cannot be undone.`
    );
    if (!confirmed) return;

    try {
      await prizePoolService.deletePrizePool(pool.prizePoolId);
      await fetchPrizePools();
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to delete prize pool';
      setError(errorMessage);
    }
  };

  if (loading && !prizePools) {
    return (
      <div className={styles.loading}>
        <div className={styles.spinner} />
        <p>Loading prize pools...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className={styles.error}>
        <p>{error}</p>
        <button onClick={() => void fetchPrizePools()}>Retry</button>
      </div>
    );
  }

  return (
    <div className={styles.prizePools}>
      <div className={styles.header}>
        <h1>Prize Pools</h1>
        <Link to="/admin/prize-pools/new" className={styles.createButton}>
          + New Prize Pool
        </Link>
      </div>

      {/* Filters */}
      <div className={styles.filters}>
        <form onSubmit={handleSearch} className={styles.searchForm}>
          <input
            type="text"
            name="search"
            placeholder="Search prize pools..."
            defaultValue={searchTerm}
            className={styles.searchInput}
          />
          <button type="submit" className={styles.searchButton}>
            Search
          </button>
        </form>
      </div>

      {/* Table */}
      {prizePools && prizePools.items.length > 0 ? (
        <>
          <div className={styles.tableContainer}>
            <table className={styles.table}>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Competition</th>
                  <th>Availability</th>
                  <th>Awarded</th>
                  <th>Redeemed</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {prizePools.items.map((pool) => (
                  <tr key={pool.prizePoolId}>
                    <td>
                      <Link
                        to={`/admin/prize-pools/${String(pool.prizePoolId)}`}
                        className={styles.poolName}
                      >
                        {pool.name}
                      </Link>
                      {pool.description && (
                        <span className={styles.description}>{pool.description}</span>
                      )}
                    </td>
                    <td>{pool.competitionName}</td>
                    <td>
                      <div className={styles.availability}>
                        <span
                          className={[
                            styles.availabilityBadge,
                            getAvailabilityClass(pool.availablePrizes, pool.totalPrizes),
                          ]
                            .filter(Boolean)
                            .join(' ')}
                        >
                          {pool.availablePrizes} / {pool.totalPrizes}
                        </span>
                      </div>
                    </td>
                    <td>{pool.awardedPrizes}</td>
                    <td>{pool.redeemedPrizes}</td>
                    <td>
                      <span
                        className={[
                          styles.status,
                          pool.isActive ? styles.statusActive : styles.statusInactive,
                        ]
                          .filter(Boolean)
                          .join(' ')}
                      >
                        {pool.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td>
                      <div className={styles.actions}>
                        <Link
                          to={`/admin/prize-pools/${String(pool.prizePoolId)}`}
                          className={styles.actionButton}
                          title="View"
                        >
                          👁️
                        </Link>
                        <Link
                          to={`/admin/prize-pools/${String(pool.prizePoolId)}/edit`}
                          className={styles.actionButton}
                          title="Edit"
                        >
                          ✏️
                        </Link>
                        <button
                          className={styles.deleteActionButton}
                          title="Delete"
                          onClick={() => void handleDelete(pool)}
                        >
                          🗑️
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {prizePools.totalPages > 1 && (
            <div className={styles.pagination}>
              <button
                className={styles.pageButton}
                disabled={!prizePools.hasPreviousPage}
                onClick={() => handlePageChange(currentPage - 1)}
              >
                ← Previous
              </button>
              <span className={styles.pageInfo}>
                Page {prizePools.pageNumber} of {prizePools.totalPages}
              </span>
              <button
                className={styles.pageButton}
                disabled={!prizePools.hasNextPage}
                onClick={() => handlePageChange(currentPage + 1)}
              >
                Next →
              </button>
            </div>
          )}
        </>
      ) : (
        <div className={styles.empty}>
          <p>No prize pools found.</p>
          <Link to="/admin/prize-pools/new" className={styles.createButton}>
            Create your first prize pool
          </Link>
        </div>
      )}
    </div>
  );
};
