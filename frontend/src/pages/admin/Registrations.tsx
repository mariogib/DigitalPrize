/**
 * Admin Registrations List Page
 * Lists all registrations with search, filtering, and management actions
 */

import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { competitionService } from '../../services';
import {
  registrationService,
  type RegistrationFilterParameters,
} from '../../services/registrationService';
import type { CompetitionResponse, RegistrationResponse } from '../../types';
import styles from './Registrations.module.css';

export const Registrations: React.FC = () => {
  const [registrations, setRegistrations] = useState<RegistrationResponse[]>([]);
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
  const [verificationStatus, setVerificationStatus] = useState<string>('');
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

  const loadRegistrations = useCallback(async (): Promise<void> => {
    try {
      setLoading(true);
      setError(null);

      const params: RegistrationFilterParameters = {
        pageNumber,
        pageSize,
        sortBy: 'registeredAt',
        sortDescending: true,
      };

      if (searchTerm) params.searchTerm = searchTerm;
      if (selectedCompetition) params.competitionId = parseInt(selectedCompetition, 10);
      if (verificationStatus) params.status = verificationStatus;
      if (fromDate) params.fromDate = fromDate;
      if (toDate) params.toDate = toDate;

      const response = await registrationService.getRegistrations(params);
      setRegistrations(response.items);
      setTotalPages(response.totalPages);
      setTotalCount(response.totalCount);
    } catch {
      setError('Failed to load registrations');
    } finally {
      setLoading(false);
    }
  }, [pageNumber, pageSize, searchTerm, selectedCompetition, verificationStatus, fromDate, toDate]);

  useEffect(() => {
    void loadCompetitions();
  }, []);

  useEffect(() => {
    void loadRegistrations();
  }, [loadRegistrations]);

  const handleSearch = (e: React.FormEvent): void => {
    e.preventDefault();
    setPageNumber(1);
    void loadRegistrations();
  };

  const handleVerify = async (registrationId: number): Promise<void> => {
    try {
      setActionLoading(registrationId);
      await registrationService.verifyRegistration(registrationId);
      void loadRegistrations();
    } catch {
      setError('Failed to verify registration');
    } finally {
      setActionLoading(null);
    }
  };

  const handleFlag = async (registrationId: number): Promise<void> => {
    const reason = window.prompt('Enter reason for flagging:');
    if (!reason) return;

    try {
      setActionLoading(registrationId);
      await registrationService.flagRegistration(registrationId, reason);
      void loadRegistrations();
    } catch {
      setError('Failed to flag registration');
    } finally {
      setActionLoading(null);
    }
  };

  const handleDelete = async (registrationId: number): Promise<void> => {
    if (!window.confirm('Are you sure you want to delete this registration?')) return;

    try {
      setActionLoading(registrationId);
      await registrationService.deleteRegistration(registrationId);
      void loadRegistrations();
    } catch {
      setError('Failed to delete registration');
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

  const clearFilters = (): void => {
    setSearchTerm('');
    setSelectedCompetition('');
    setVerificationStatus('');
    setFromDate('');
    setToDate('');
    setPageNumber(1);
  };

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div className={styles.headerLeft}>
          <h1>Registrations</h1>
          <p className={styles.subtitle}>{totalCount.toLocaleString()} total registrations</p>
        </div>
      </div>

      {/* Filters */}
      <div className={styles.filters}>
        <form onSubmit={handleSearch} className={styles.searchForm}>
          <input
            type="text"
            placeholder="Search by cell number..."
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
            value={verificationStatus}
            onChange={(e) => {
              setVerificationStatus(e.target.value);
              setPageNumber(1);
            }}
            className={styles.filterSelect}
            aria-label="Filter by verification status"
          >
            <option value="">All Status</option>
            <option value="verified">Verified</option>
            <option value="pending">Pending</option>
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
              placeholder="From date"
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
              placeholder="To date"
            />
          </div>

          {(searchTerm || selectedCompetition || verificationStatus || fromDate || toDate) && (
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
        <div className={styles.loading}>Loading registrations...</div>
      ) : registrations.length === 0 ? (
        <div className={styles.emptyState}>
          <h3>No registrations found</h3>
          <p>
            {searchTerm || selectedCompetition || verificationStatus || fromDate || toDate
              ? 'Try adjusting your filters'
              : 'No registrations have been submitted yet'}
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
                  <th>Cell Number</th>
                  <th>Competition</th>
                  <th>Status</th>
                  <th>Registered</th>
                  <th>Answers</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {registrations.map((registration) => (
                  <tr key={registration.registrationId}>
                    <td className={styles.idCell}>#{registration.registrationId}</td>
                    <td className={styles.cellNumber}>{registration.cellNumber}</td>
                    <td>
                      <Link
                        to={`/admin/competitions/${String(registration.competitionId)}`}
                        className={styles.competitionLink}
                      >
                        {registration.competitionName}
                      </Link>
                    </td>
                    <td>
                      <span
                        className={[
                          styles.statusBadge,
                          registration.isVerified ? styles.statusVerified : styles.statusPending,
                        ]
                          .filter(Boolean)
                          .join(' ')}
                      >
                        {registration.isVerified ? 'Verified' : 'Pending'}
                      </span>
                    </td>
                    <td className={styles.dateCell}>{formatDate(registration.registeredAt)}</td>
                    <td>
                      {registration.answers.length > 0 ? (
                        <button
                          className={styles.viewAnswersButton}
                          onClick={() => {
                            const answersText = registration.answers
                              .map((a) => `${a.fieldName}: ${a.value ?? 'N/A'}`)
                              .join('\n');
                            window.alert(answersText || 'No answers');
                          }}
                          title="View answers"
                        >
                          {registration.answers.length} answer
                          {registration.answers.length !== 1 ? 's' : ''}
                        </button>
                      ) : (
                        <span className={styles.noAnswers}>-</span>
                      )}
                    </td>
                    <td>
                      <div className={styles.actions}>
                        {!registration.isVerified && (
                          <button
                            className={styles.verifyButton}
                            onClick={() => void handleVerify(registration.registrationId)}
                            disabled={actionLoading === registration.registrationId}
                            title="Verify registration"
                          >
                            {actionLoading === registration.registrationId ? '...' : 'Verify'}
                          </button>
                        )}
                        <button
                          className={styles.flagButton}
                          onClick={() => void handleFlag(registration.registrationId)}
                          disabled={actionLoading === registration.registrationId}
                          title="Flag registration"
                        >
                          Flag
                        </button>
                        <button
                          className={styles.deleteButton}
                          onClick={() => void handleDelete(registration.registrationId)}
                          disabled={actionLoading === registration.registrationId}
                          title="Delete registration"
                        >
                          Delete
                        </button>
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
