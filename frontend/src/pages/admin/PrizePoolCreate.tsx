/**
 * Admin Prize Pool Create Page
 * Create a new prize pool
 */

import React, { useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { prizePoolService } from '../../services';
import styles from './PrizePoolCreate.module.css';

export const PrizePoolCreate: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const competitionId = searchParams.get('competitionId');

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [formData, setFormData] = useState({
    name: '',
    description: '',
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.name.trim()) {
      setError('Name is required');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const description = formData.description.trim();
      const request = {
        name: formData.name.trim(),
        ...(competitionId ? { competitionId: Number(competitionId) } : {}),
        ...(description ? { description } : {}),
      };

      const result = await prizePoolService.createPrizePool(request);

      // Navigate to the new prize pool or back to competition if from there
      if (competitionId) {
        navigate(`/admin/competitions/${competitionId}`);
      } else {
        navigate(`/admin/prize-pools/${String(result.prizePoolId)}`);
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to create prize pool';
      setError(errorMessage);
      console.error('Create prize pool error:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    if (competitionId) {
      navigate(`/admin/competitions/${competitionId}`);
    } else {
      navigate('/admin/prize-pools');
    }
  };

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div className={styles.headerLeft}>
          <Link
            to={competitionId ? `/admin/competitions/${competitionId}` : '/admin/prize-pools'}
            className={styles.backLink}
          >
            ← {competitionId ? 'Back to Competition' : 'Back to Prize Pools'}
          </Link>
          <h1>Create Prize Pool</h1>
        </div>
      </div>

      {error && (
        <div className={styles.errorBanner}>
          <p>{error}</p>
          <button onClick={() => setError(null)} className={styles.dismissButton}>
            ×
          </button>
        </div>
      )}

      <form onSubmit={(e) => void handleSubmit(e)} className={styles.form}>
        <div className={styles.formCard}>
          <h2>Prize Pool Details</h2>

          <div className={styles.formGroup}>
            <label htmlFor="name" className={styles.label}>
              Name <span className={styles.required}>*</span>
            </label>
            <input
              type="text"
              id="name"
              name="name"
              value={formData.name}
              onChange={handleChange}
              className={styles.input}
              placeholder="Enter prize pool name"
              disabled={loading}
              autoFocus
            />
          </div>

          <div className={styles.formGroup}>
            <label htmlFor="description" className={styles.label}>
              Description
            </label>
            <textarea
              id="description"
              name="description"
              value={formData.description}
              onChange={handleChange}
              className={styles.textarea}
              placeholder="Enter prize pool description (optional)"
              rows={4}
              disabled={loading}
            />
          </div>
        </div>

        <div className={styles.formActions}>
          <button
            type="button"
            onClick={handleCancel}
            className={styles.cancelButton}
            disabled={loading}
          >
            Cancel
          </button>
          <button type="submit" className={styles.submitButton} disabled={loading}>
            {loading ? 'Creating...' : 'Create Prize Pool'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default PrizePoolCreate;
