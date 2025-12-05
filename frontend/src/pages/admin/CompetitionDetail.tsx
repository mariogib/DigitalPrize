/**
 * Admin Competition Detail Page
 * View and edit competition details
 */

import React, { useCallback, useEffect, useState } from 'react';
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom';
import { competitionService, prizePoolService, registrationService } from '../../services';
import type {
  CompetitionDetailResponse,
  PagedResponse,
  PrizePoolSummaryResponse,
  RegistrationResponse,
  UpdateCompetitionRequest,
} from '../../types';
import styles from './CompetitionDetail.module.css';

type TabType = 'details' | 'registrations';

export const CompetitionDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const competitionId = parseInt(id ?? '0', 10);
  const isEditMode = location.pathname.endsWith('/edit');

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<TabType>('details');

  const [competition, setCompetition] = useState<CompetitionDetailResponse | null>(null);
  const [registrations, setRegistrations] = useState<PagedResponse<RegistrationResponse> | null>(
    null
  );

  const [registrationPage, setRegistrationPage] = useState(1);
  const [saving, setSaving] = useState(false);
  const [editForm, setEditForm] = useState<UpdateCompetitionRequest | null>(null);

  // Initialize edit form when entering edit mode
  useEffect(() => {
    if (isEditMode && competition && !editForm) {
      setEditForm({
        name: competition.name,
        description: competition.description ?? '',
        startDate: competition.startDate?.split('T')[0] ?? '',
        endDate: competition.endDate?.split('T')[0] ?? '',
        status: competition.status,
      });
    } else if (!isEditMode) {
      setEditForm(null);
    }
  }, [isEditMode, competition, editForm]);

  const handleEditChange = (field: keyof UpdateCompetitionRequest, value: string) => {
    if (editForm) {
      setEditForm({ ...editForm, [field]: value });
    }
  };

  const handleSave = async () => {
    if (!editForm || !competitionId) return;

    setSaving(true);
    try {
      await competitionService.updateCompetition(competitionId, editForm);
      await loadCompetition();
      navigate(`/admin/competitions/${String(competitionId)}`);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to save competition';
      setError(errorMessage);
    } finally {
      setSaving(false);
    }
  };

  const loadCompetition = useCallback(async () => {
    if (!competitionId) return;

    setLoading(true);
    setError(null);

    try {
      const data = await competitionService.getCompetition(competitionId);
      setCompetition(data);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to load competition';
      setError(errorMessage);
      console.error('Load competition error:', err);
    } finally {
      setLoading(false);
    }
  }, [competitionId]);

  const loadRegistrations = useCallback(async () => {
    if (!competitionId) return;

    try {
      const data = await registrationService.getRegistrationsForCompetition(competitionId, {
        pageNumber: registrationPage,
        pageSize: 10,
      });
      setRegistrations(data);
    } catch (err) {
      console.error('Load registrations error:', err);
    }
  }, [competitionId, registrationPage]);

  useEffect(() => {
    void loadCompetition();
  }, [loadCompetition]);

  useEffect(() => {
    if (activeTab === 'registrations') {
      void loadRegistrations();
    }
  }, [activeTab, loadRegistrations]);

  const formatDate = (date?: string): string => {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const formatDateTime = (date?: string): string => {
    if (!date) return '-';
    return new Date(date).toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getStatusBadgeClass = (status: string): string => {
    switch (status.toLowerCase()) {
      case 'active':
        return styles.statusActive ?? '';
      case 'upcoming':
        return styles.statusUpcoming ?? '';
      case 'completed':
        return styles.statusCompleted ?? '';
      case 'draft':
        return styles.statusDraft ?? '';
      default:
        return '';
    }
  };

  if (loading) {
    return (
      <div className={styles.page}>
        <div className={styles.loading}>Loading competition...</div>
      </div>
    );
  }

  if (error || !competition) {
    return (
      <div className={styles.page}>
        <div className={styles.errorState}>
          <h2>Error</h2>
          <p>{error ?? 'Competition not found'}</p>
          <Link to="/admin/competitions" className={styles.backLink}>
            ← Back to Competitions
          </Link>
        </div>
      </div>
    );
  }

  // Get the single prize pool for this competition (1:1 relationship)
  const prizePool: PrizePoolSummaryResponse | undefined = competition.prizePools?.[0];

  const handleDeletePrizePool = async () => {
    if (!prizePool) return;

    const confirmed = window.confirm(
      `Are you sure you want to remove the prize pool "${prizePool.name}" from this competition?`
    );
    if (!confirmed) return;

    try {
      await prizePoolService.deletePrizePool(prizePool.prizePoolId);
      await loadCompetition();
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to delete prize pool';
      setError(errorMessage);
    }
  };

  return (
    <div className={styles.page}>
      {/* Header */}
      <div className={styles.header}>
        <div className={styles.headerLeft}>
          <Link to="/admin/competitions" className={styles.backLink}>
            ← Back to Competitions
          </Link>
          <h1>
            {competition.name}
            {isEditMode && ' (Editing)'}
          </h1>
          <span
            className={`${styles.statusBadge ?? ''} ${getStatusBadgeClass(competition.status)}`}
          >
            {competition.status}
          </span>
        </div>
        <div className={styles.headerActions}>
          {isEditMode ? (
            <>
              <button
                className={styles.editButton}
                onClick={() => navigate(`/admin/competitions/${String(competitionId)}`)}
                disabled={saving}
              >
                Cancel
              </button>
              <button
                className={styles.saveButton}
                onClick={() => void handleSave()}
                disabled={saving}
              >
                {saving ? 'Saving...' : 'Save'}
              </button>
            </>
          ) : (
            <button
              className={styles.editButton}
              onClick={() => navigate(`/admin/competitions/${String(competitionId)}/edit`)}
            >
              Edit
            </button>
          )}
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
          className={`${styles.tab ?? ''} ${activeTab === 'registrations' ? (styles.activeTab ?? '') : ''}`}
          onClick={() => setActiveTab('registrations')}
        >
          Registrations ({registrations?.totalCount ?? competition.registrationCount})
        </button>
      </div>

      {/* Tab Content */}
      <div className={styles.tabContent}>
        {activeTab === 'details' && (
          <div className={styles.detailsGrid}>
            <div className={styles.detailCard}>
              <h3>Competition Information</h3>
              {isEditMode && editForm !== null ? (
                <>
                  <div className={styles.detailRow}>
                    <span className={styles.detailLabel}>Name</span>
                    <input
                      type="text"
                      className={styles.editInput}
                      value={editForm.name ?? ''}
                      onChange={(e) => handleEditChange('name', e.target.value)}
                      aria-label="Competition name"
                    />
                  </div>
                  <div className={styles.detailRow}>
                    <span className={styles.detailLabel}>Description</span>
                    <textarea
                      className={styles.editTextarea}
                      value={editForm.description ?? ''}
                      onChange={(e) => handleEditChange('description', e.target.value)}
                      rows={3}
                      aria-label="Competition description"
                    />
                  </div>
                  <div className={styles.detailRow}>
                    <span className={styles.detailLabel}>Start Date</span>
                    <input
                      type="date"
                      className={styles.editInput}
                      value={editForm.startDate ?? ''}
                      onChange={(e) => handleEditChange('startDate', e.target.value)}
                      aria-label="Start date"
                    />
                  </div>
                  <div className={styles.detailRow}>
                    <span className={styles.detailLabel}>End Date</span>
                    <input
                      type="date"
                      className={styles.editInput}
                      value={editForm.endDate ?? ''}
                      onChange={(e) => handleEditChange('endDate', e.target.value)}
                      aria-label="End date"
                    />
                  </div>
                </>
              ) : (
                <>
                  <div className={styles.detailRow}>
                    <span className={styles.detailLabel}>Description</span>
                    <span className={styles.detailValue}>{competition.description ?? '-'}</span>
                  </div>
                  <div className={styles.detailRow}>
                    <span className={styles.detailLabel}>Start Date</span>
                    <span className={styles.detailValue}>{formatDate(competition.startDate)}</span>
                  </div>
                  <div className={styles.detailRow}>
                    <span className={styles.detailLabel}>End Date</span>
                    <span className={styles.detailValue}>{formatDate(competition.endDate)}</span>
                  </div>
                </>
              )}
              <div className={styles.detailRow}>
                <span className={styles.detailLabel}>Created</span>
                <span className={styles.detailValue}>{formatDateTime(competition.createdAt)}</span>
              </div>
            </div>

            <div className={styles.statsCard}>
              <h3>Statistics</h3>
              <div className={styles.statsGrid}>
                <div className={styles.statItem}>
                  <span className={styles.statValue}>{competition.registrationCount}</span>
                  <span className={styles.statLabel}>Registrations</span>
                </div>
                <div className={styles.statItem}>
                  <span className={styles.statValue}>{competition.awardedPrizesCount}</span>
                  <span className={styles.statLabel}>Awards</span>
                </div>
              </div>
            </div>

            <div className={styles.detailCard}>
              <h3>Prize Pool</h3>
              {prizePool ? (
                <div className={styles.prizePoolInfo}>
                  <div className={styles.detailRow}>
                    <span className={styles.detailLabel}>Name</span>
                    <span className={styles.detailValue}>
                      <Link to={`/admin/prize-pools/${String(prizePool.prizePoolId)}`}>
                        {prizePool.name}
                      </Link>
                    </span>
                  </div>
                  <div className={styles.detailRow}>
                    <span className={styles.detailLabel}>Status</span>
                    <span className={styles.detailValue}>
                      {prizePool.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </div>
                  <div className={styles.detailRow}>
                    <span className={styles.detailLabel}>Prizes</span>
                    <span className={styles.detailValue}>
                      {prizePool.totalPrizes} total, {prizePool.availablePrizes} available
                    </span>
                  </div>
                  <div className={styles.prizePoolActions}>
                    <Link
                      to={`/admin/prize-pools/${String(prizePool.prizePoolId)}`}
                      className={styles.linkButton}
                    >
                      View Details
                    </Link>
                    <button
                      className={styles.deleteButton}
                      onClick={() => void handleDeletePrizePool()}
                    >
                      Remove
                    </button>
                  </div>
                </div>
              ) : (
                <div className={styles.noPrizePool}>
                  <p>No prize pool assigned to this competition.</p>
                  <Link
                    to={`/admin/prize-pools/new?competitionId=${String(competitionId)}`}
                    className={styles.addButton}
                  >
                    + Add Prize Pool
                  </Link>
                </div>
              )}
            </div>

            {competition.registrationFields.length > 0 && (
              <div className={styles.detailCard}>
                <h3>Registration Fields</h3>
                {competition.registrationFields.map((field) => (
                  <div key={field.registrationFieldId} className={styles.detailRow}>
                    <span className={styles.detailLabel}>
                      {field.fieldName}{' '}
                      {field.isRequired && <span className={styles.required}>*</span>}
                    </span>
                    <span className={styles.detailValue}>{field.fieldType}</span>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {activeTab === 'registrations' && (
          <div className={styles.registrationsTab}>
            <div className={styles.tabHeader}>
              <h3>Registrations</h3>
            </div>
            {!registrations || registrations.items.length === 0 ? (
              <div className={styles.emptyState}>
                <p>No registrations yet for this competition.</p>
              </div>
            ) : (
              <>
                <table className={styles.table}>
                  <thead>
                    <tr>
                      <th>Cell Number</th>
                      <th>Registered</th>
                      <th>Verified</th>
                    </tr>
                  </thead>
                  <tbody>
                    {registrations.items.map((reg) => (
                      <tr key={reg.registrationId}>
                        <td className={styles.codeCell}>{reg.cellNumber}</td>
                        <td>{formatDateTime(reg.registeredAt)}</td>
                        <td>{reg.isVerified ? '✓ Yes' : 'No'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                {registrations.totalPages > 1 && (
                  <div className={styles.pagination}>
                    <button
                      disabled={registrationPage === 1}
                      onClick={() => setRegistrationPage((p) => p - 1)}
                    >
                      Previous
                    </button>
                    <span>
                      Page {registrationPage} of {registrations.totalPages}
                    </span>
                    <button
                      disabled={registrationPage >= registrations.totalPages}
                      onClick={() => setRegistrationPage((p) => p + 1)}
                    >
                      Next
                    </button>
                  </div>
                )}
              </>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
