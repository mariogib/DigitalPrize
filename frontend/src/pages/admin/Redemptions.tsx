/**
 * Admin Redemptions List Page
 * Lists all redemptions with filtering and approve/reject workflow
 */

import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { competitionService } from '../../services';
import {
  redemptionService,
  type RedemptionFilterParameters,
} from '../../services/redemptionService';
import type { CompetitionResponse, PrizeRedemptionResponse } from '../../types';
import styles from './Redemptions.module.css';

export const Redemptions: React.FC = () => {
  const [redemptions, setRedemptions] = useState<PrizeRedemptionResponse[]>([]);
  const [competitions, setCompetitions] = useState<CompetitionResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Pagination state
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(20);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // Filter state
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCompetition, setSelectedCompetition] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');

  // Action state
  const [actionLoading, setActionLoading] = useState<number | null>(null);

  const loadCompetitions = async (): Promise<void> => {
    try {
      const response = await competitionService.getCompetitions({ pageSize: 100 });
      setCompetitions(response.items);
    } catch {
      console.error('Failed to load competitions');
    }
  };

  const loadRedemptions = useCallback(async (): Promise<void> => {
    try {
      setLoading(true);
      setError(null);

      const params: RedemptionFilterParameters = {
        pageNumber,
        pageSize,
        sortBy: 'redeemedAt',
        sortDescending: true,
      };

      if (searchTerm) params.searchTerm = searchTerm;
      if (selectedCompetition) params.competitionId = parseInt(selectedCompetition, 10);
      if (statusFilter) params.status = statusFilter;
      if (fromDate) params.fromDate = fromDate;
      if (toDate) params.toDate = toDate;

      const response = await redemptionService.getRedemptions(params);
      setRedemptions(response.items);
      setTotalPages(response.totalPages);
      setTotalCount(response.totalCount);
    } catch {
      setError('Failed to load redemptions');
    } finally {
      setLoading(false);
    }
  }, [pageNumber, pageSize, searchTerm, selectedCompetition, statusFilter, fromDate, toDate]);

  useEffect(() => {
    void loadCompetitions();
  }, []);

  useEffect(() => {
    void loadRedemptions();
  }, [loadRedemptions]);

  const handleSearch = (e: React.FormEvent): void => {
    e.preventDefault();
    setPageNumber(1);
    void loadRedemptions();
  };

  const handleApprove = async (redemptionId: number): Promise<void> => {
    const notes = window.prompt('Add notes (optional):');

    try {
      setActionLoading(redemptionId);
      await redemptionService.approveRedemption(redemptionId, notes ?? undefined);
      void loadRedemptions();
    } catch {
      setError('Failed to approve redemption');
    } finally {
      setActionLoading(null);
    }
  };

  const handleReject = async (redemptionId: number): Promise<void> => {
    const reason = window.prompt('Enter rejection reason:');
    if (!reason) {
      window.alert('Rejection reason is required');
      return;
    }

    try {
      setActionLoading(redemptionId);
      await redemptionService.rejectRedemption(redemptionId, reason);
      void loadRedemptions();
    } catch {
      setError('Failed to reject redemption');
    } finally {
      setActionLoading(null);
    }
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getStatusBadgeClass = (status: string): string => {
    const statusLower = status.toLowerCase();
    if (statusLower === 'completed' || statusLower === 'approved') {
      return styles.statusCompleted ?? '';
    }
    if (statusLower === 'pending') {
      return styles.statusPending ?? '';
    }
    if (statusLower === 'rejected' || statusLower === 'failed') {
      return styles.statusRejected ?? '';
    }
    return '';
  };

  const clearFilters = (): void => {
    setSearchTerm('');
    setSelectedCompetition('');
    setStatusFilter('');
    setFromDate('');
    setToDate('');
    setPageNumber(1);
  };

  const isPending = (status: string): boolean => {
    return status.toLowerCase() === 'pending';
  };

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div className={styles.headerLeft}>
          <h1>Redemptions</h1>
          <p className={styles.subtitle}>{totalCount.toLocaleString()} total redemptions</p>
        </div>
      </div>

      {/* Filters */}
      <div className={styles.filters}>
        <form onSubmit={handleSearch} className={styles.searchForm}>
          <input
            type="text"
            placeholder="Search by cell number or code..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className={styles.searchInput}
          />
          <button type="submit" className={styles.searchButton}>
            Search
          </button>
        </form>

        <div className={styles.filterRow}>
          <select
            value={selectedCompetition}
            onChange={(e) => {
              setSelectedCompetition(e.target.value);
              setPageNumber(1);
            }}
            className={styles.filterSelect}
            aria-label="Filter by competition"
          >
            <option value="">All Competitions</option>
            {competitions.map((comp) => (
              <option key={comp.competitionId} value={String(comp.competitionId)}>
                {comp.name}
              </option>
            ))}
          </select>

          <select
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(e.target.value);
              setPageNumber(1);
            }}
            className={styles.filterSelect}
            aria-label="Filter by status"
          >
            <option value="">All Status</option>
            <option value="pending">Pending</option>
            <option value="completed">Completed</option>
            <option value="approved">Approved</option>
            <option value="rejected">Rejected</option>
          </select>

          <div className={styles.dateRange}>
            <input
              type="date"
              value={fromDate}
              onChange={(e) => {
                setFromDate(e.target.value);
                setPageNumber(1);
              }}
              className={styles.dateInput}
              aria-label="From date"
            />
            <span className={styles.dateSeparator}>to</span>
            <input
              type="date"
              value={toDate}
              onChange={(e) => {
                setToDate(e.target.value);
                setPageNumber(1);
              }}
              className={styles.dateInput}
              aria-label="To date"
            />
          </div>

          {(searchTerm || selectedCompetition || statusFilter || fromDate || toDate) && (
            <button onClick={clearFilters} className={styles.clearButton}>
              Clear Filters
            </button>
          )}
        </div>
      </div>

      {/* Error State */}
      {error && (
        <div className={styles.error}>
          {error}
          <button onClick={() => setError(null)}>×</button>
        </div>
      )}

      {/* Loading State */}
      {loading ? (
        <div className={styles.loading}>Loading redemptions...</div>
      ) : redemptions.length === 0 ? (
        <div className={styles.emptyState}>
          <h3>No redemptions found</h3>
          <p>
            {searchTerm || selectedCompetition || statusFilter || fromDate || toDate
              ? 'Try adjusting your filters'
              : 'No redemptions have been processed yet'}
          </p>
        </div>
      ) : (
        <>
          {/* Table */}
          <div className={styles.tableContainer}>
            <table className={styles.table}>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Redemption Code</th>
                  <th>Prize Award</th>
                  <th>Redeemed By</th>
                  <th>Channel</th>
                  <th>Status</th>
                  <th>Date</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {redemptions.map((redemption) => (
                  <tr key={redemption.prizeRedemptionId}>
                    <td className={styles.idCell}>#{redemption.prizeRedemptionId}</td>
                    <td className={styles.codeCell}>{redemption.redemptionCode ?? '-'}</td>
                    <td>
                      <Link
                        to={`/admin/awards/${String(redemption.prizeAwardId)}`}
                        className={styles.awardLink}
                      >
                        Award #{redemption.prizeAwardId}
                      </Link>
                    </td>
                    <td className={styles.cellNumber}>{redemption.redeemedBy ?? '-'}</td>
                    <td>
                      <span className={styles.channelBadge}>
                        {redemption.redemptionChannel ?? 'N/A'}
                      </span>
                    </td>
                    <td>
                      <span
                        className={[styles.statusBadge, getStatusBadgeClass('completed')]
                          .filter(Boolean)
                          .join(' ')}
                      >
                        Completed
                      </span>
                    </td>
                    <td className={styles.dateCell}>{formatDate(redemption.redeemedAt)}</td>
                    <td>
                      <div className={styles.actions}>
                        {isPending('completed') ? (
                          <>
                            <button
                              className={styles.approveButton}
                              onClick={() => void handleApprove(redemption.prizeRedemptionId)}
                              disabled={actionLoading === redemption.prizeRedemptionId}
                              title="Approve redemption"
                            >
                              {actionLoading === redemption.prizeRedemptionId ? '...' : 'Approve'}
                            </button>
                            <button
                              className={styles.rejectButton}
                              onClick={() => void handleReject(redemption.prizeRedemptionId)}
                              disabled={actionLoading === redemption.prizeRedemptionId}
                              title="Reject redemption"
                            >
                              Reject
                            </button>
                          </>
                        ) : (
                          <Link
                            to={`/admin/awards/${String(redemption.prizeAwardId)}`}
                            className={styles.viewButton}
                          >
                            View Award
                          </Link>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className={styles.pagination}>
              <button
                onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                disabled={pageNumber === 1}
                className={styles.pageButton}
              >
                Previous
              </button>
              <span className={styles.pageInfo}>
                Page {pageNumber} of {totalPages}
              </span>
              <button
                onClick={() => setPageNumber((p) => Math.min(totalPages, p + 1))}
                disabled={pageNumber === totalPages}
                className={styles.pageButton}
              >
                Next
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
};
