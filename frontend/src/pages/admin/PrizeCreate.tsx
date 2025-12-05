/**
 * Admin Prize Create Page
 * Create a new prize
 */

import React, { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { prizeService } from '../../services';
import { apiClient } from '../../services/api/apiClient';
import type { PrizeTypeResponse } from '../../types';
import styles from './PrizeCreate.module.css';

export const PrizeCreate: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const prizePoolId = searchParams.get('prizePoolId');

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [prizeTypes, setPrizeTypes] = useState<PrizeTypeResponse[]>([]);

  const [formData, setFormData] = useState({
    name: '',
    description: '',
    prizeTypeId: '',
    monetaryValue: '',
    totalQuantity: '1',
    imageUrl: '',
    expiryDate: '',
  });

  const loadPrizeTypes = useCallback(async () => {
    try {
      const types = await apiClient.get<PrizeTypeResponse[]>('/api/prizes/types');
      setPrizeTypes(types);
      const firstType = types[0];
      if (firstType) {
        setFormData((prev) => ({ ...prev, prizeTypeId: String(firstType.prizeTypeId) }));
      }
    } catch (err) {
      console.error('Failed to load prize types:', err);
    }
  }, []);

  useEffect(() => {
    void loadPrizeTypes();
  }, [loadPrizeTypes]);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
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

    if (!prizePoolId) {
      setError('Prize Pool is required');
      return;
    }

    if (!formData.prizeTypeId) {
      setError('Prize Type is required');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const request: {
        prizePoolId: number;
        prizeTypeId: number;
        name: string;
        description?: string;
        monetaryValue?: number;
        totalQuantity: number;
        imageUrl?: string;
        expiryDate?: string;
      } = {
        prizePoolId: Number(prizePoolId),
        prizeTypeId: Number(formData.prizeTypeId),
        name: formData.name.trim(),
        totalQuantity: Number(formData.totalQuantity) || 1,
      };

      // Only add optional fields if they have values
      if (formData.description.trim()) {
        request.description = formData.description.trim();
      }
      if (formData.monetaryValue) {
        request.monetaryValue = Number(formData.monetaryValue);
      }
      if (formData.imageUrl.trim()) {
        request.imageUrl = formData.imageUrl.trim();
      }
      if (formData.expiryDate) {
        request.expiryDate = formData.expiryDate;
      }

      await prizeService.create(request);
      navigate(`/admin/prize-pools/${prizePoolId}`);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to create prize';
      setError(errorMessage);
      console.error('Create prize error:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    if (prizePoolId) {
      navigate(`/admin/prize-pools/${prizePoolId}`);
    } else {
      navigate('/admin/prize-pools');
    }
  };

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div className={styles.headerLeft}>
          <Link
            to={prizePoolId ? `/admin/prize-pools/${prizePoolId}` : '/admin/prize-pools'}
            className={styles.backLink}
          >
            ← {prizePoolId ? 'Back to Prize Pool' : 'Back to Prize Pools'}
          </Link>
          <h1>Create Prize</h1>
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
          <h2>Prize Details</h2>

          <div className={styles.formRow}>
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
                placeholder="Enter prize name"
                disabled={loading}
                autoFocus
              />
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="prizeTypeId" className={styles.label}>
                Prize Type <span className={styles.required}>*</span>
              </label>
              <select
                id="prizeTypeId"
                name="prizeTypeId"
                value={formData.prizeTypeId}
                onChange={handleChange}
                className={styles.select}
                disabled={loading}
              >
                {prizeTypes.map((type) => (
                  <option key={type.prizeTypeId} value={type.prizeTypeId}>
                    {type.name}
                  </option>
                ))}
              </select>
            </div>
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
              placeholder="Enter prize description (optional)"
              rows={3}
              disabled={loading}
            />
          </div>

          <div className={styles.formRow}>
            <div className={styles.formGroup}>
              <label htmlFor="monetaryValue" className={styles.label}>
                Monetary Value
              </label>
              <input
                type="number"
                id="monetaryValue"
                name="monetaryValue"
                value={formData.monetaryValue}
                onChange={handleChange}
                className={styles.input}
                placeholder="0.00"
                min="0"
                step="0.01"
                disabled={loading}
              />
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="totalQuantity" className={styles.label}>
                Quantity <span className={styles.required}>*</span>
              </label>
              <input
                type="number"
                id="totalQuantity"
                name="totalQuantity"
                value={formData.totalQuantity}
                onChange={handleChange}
                className={styles.input}
                placeholder="1"
                min="1"
                disabled={loading}
              />
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="expiryDate" className={styles.label}>
                Expiry Date
              </label>
              <input
                type="date"
                id="expiryDate"
                name="expiryDate"
                value={formData.expiryDate}
                onChange={handleChange}
                className={styles.input}
                disabled={loading}
              />
            </div>
          </div>

          <div className={styles.formGroup}>
            <label htmlFor="imageUrl" className={styles.label}>
              Image URL
            </label>
            <input
              type="url"
              id="imageUrl"
              name="imageUrl"
              value={formData.imageUrl}
              onChange={handleChange}
              className={styles.input}
              placeholder="https://example.com/image.jpg"
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
            {loading ? 'Creating...' : 'Create Prize'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default PrizeCreate;
