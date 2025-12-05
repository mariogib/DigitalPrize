/**
 * Competitions List Page
 * Displays a paginated list of all competitions with filtering options
 */

import React, { useCallback, useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { competitionService } from '../../services';
import type { CompetitionResponse, PagedResponse } from '../../types';
import styles from './Competitions.module.css';

const PAGE_SIZE = 10;

export const Competitions: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [competitions, setCompetitions] = useState<PagedResponse<CompetitionResponse> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const currentPage = Number(searchParams.get('page')) || 1;
  const statusFilter = searchParams.get('status') ?? '';
  const searchTerm = searchParams.get('search') ?? '';

  const fetchCompetitions = useCallback(async () => {
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

      const data = await competitionService.getCompetitions(params);
      setCompetitions(data);
    } catch (err) {
      setError('Failed to load competitions');
      console.error('Competitions error:', err);
    } finally {
      setLoading(false);
    }
  }, [currentPage, statusFilter, searchTerm]);

  useEffect(() => {
    void fetchCompetitions();
  }, [fetchCompetitions]);

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
      active: styles.statusActive ?? '',
      upcoming: styles.statusUpcoming ?? '',
      ended: styles.statusEnded ?? '',
      draft: styles.statusDraft ?? '',
    };
    return statusMap[status.toLowerCase()] ?? '';
  };

  if (loading && !competitions) {
    return (
      <div className={styles.loading}>
        <div className={styles.spinner} />
        <p>Loading competitions...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className={styles.error}>
        <p>{error}</p>
        <button onClick={() => void fetchCompetitions()}>Retry</button>
      </div>
    );
  }

  return (
    <div className={styles.competitions}>
      <div className={styles.header}>
        <h1>Competitions</h1>
        <Link to="/admin/competitions/new" className={styles.createButton}>
          + New Competition
        </Link>
      </div>

      {/* Filters */}
      <div className={styles.filters}>
        <form onSubmit={handleSearch} className={styles.searchForm}>
          <input
            type="text"
            name="search"
            placeholder="Search competitions..."
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
            className={[styles.filterButton, statusFilter === 'Active' ? styles.active : '']
              .filter(Boolean)
              .join(' ')}
            onClick={() => handleStatusFilter('Active')}
          >
            Active
          </button>
          <button
            className={[styles.filterButton, statusFilter === 'Upcoming' ? styles.active : '']
              .filter(Boolean)
              .join(' ')}
            onClick={() => handleStatusFilter('Upcoming')}
          >
            Upcoming
          </button>
          <button
            className={[styles.filterButton, statusFilter === 'Ended' ? styles.active : '']
              .filter(Boolean)
              .join(' ')}
            onClick={() => handleStatusFilter('Ended')}
          >
            Ended
          </button>
          <button
            className={[styles.filterButton, statusFilter === 'Draft' ? styles.active : '']
              .filter(Boolean)
              .join(' ')}
            onClick={() => handleStatusFilter('Draft')}
          >
            Draft
          </button>
        </div>
      </div>

      {/* Table */}
      {competitions && competitions.items.length > 0 ? (
        <>
          <div className={styles.tableContainer}>
            <table className={styles.table}>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Status</th>
                  <th>Start Date</th>
                  <th>End Date</th>
                  <th>Registrations</th>
                  <th>Prizes Awarded</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {competitions.items.map((comp) => (
                  <tr key={comp.competitionId}>
                    <td>
                      <Link
                        to={`/admin/competitions/${String(comp.competitionId)}`}
                        className={styles.competitionName}
                      >
                        {comp.name}
                      </Link>
                      {comp.description && (
                        <span className={styles.description}>{comp.description}</span>
                      )}
                    </td>
                    <td>
                      <span
                        className={[styles.status, getStatusClass(comp.status)]
                          .filter(Boolean)
                          .join(' ')}
                      >
                        {comp.status}
                      </span>
                    </td>
                    <td>{new Date(comp.startDate).toLocaleDateString()}</td>
                    <td>{new Date(comp.endDate).toLocaleDateString()}</td>
                    <td>{comp.registrationCount.toLocaleString()}</td>
                    <td>{comp.awardedPrizesCount.toLocaleString()}</td>
                    <td>
                      <div className={styles.actions}>
                        <Link
                          to={`/admin/competitions/${String(comp.competitionId)}`}
                          className={styles.actionButton}
                          title="View"
                        >
                          👁️
                        </Link>
                        <Link
                          to={`/admin/competitions/${String(comp.competitionId)}/edit`}
                          className={styles.actionButton}
                          title="Edit"
                        >
                          ✏️
                        </Link>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {competitions.totalPages > 1 && (
            <div className={styles.pagination}>
              <button
                className={styles.pageButton}
                disabled={!competitions.hasPreviousPage}
                onClick={() => handlePageChange(currentPage - 1)}
              >
                ← Previous
              </button>
              <span className={styles.pageInfo}>
                Page {competitions.pageNumber} of {competitions.totalPages}
              </span>
              <button
                className={styles.pageButton}
                disabled={!competitions.hasNextPage}
                onClick={() => handlePageChange(currentPage + 1)}
              >
                Next →
              </button>
            </div>
          )}
        </>
      ) : (
        <div className={styles.empty}>
          <p>No competitions found.</p>
          <Link to="/admin/competitions/new" className={styles.createButton}>
            Create your first competition
          </Link>
        </div>
      )}
    </div>
  );
};
