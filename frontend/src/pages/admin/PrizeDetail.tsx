/**
 * Admin Prize Detail Page
 * View and edit prize details
 */

import React, { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { prizeService } from '../../services';
import { apiClient } from '../../services/api/apiClient';
import type { PrizeDetailResponse, PrizeTypeResponse, UpdatePrizeRequest } from '../../types';
import styles from './PrizeDetail.module.css';

export const PrizeDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const prizeId = id ?? '0';

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isEditMode, setIsEditMode] = useState(false);
  const [prizeTypes, setPrizeTypes] = useState<PrizeTypeResponse[]>([]);

  const [prize, setPrize] = useState<PrizeDetailResponse | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    prizeTypeId: '',
    monetaryValue: '',
    totalQuantity: '',
    imageUrl: '',
    expiryDate: '',
    isActive: true,
  });

  const loadPrize = useCallback(async () => {
    if (!prizeId || prizeId === '0') return;

    setLoading(true);
    setError(null);

    try {
      const data = await prizeService.getById(prizeId);
      setPrize(data);
      setFormData({
        name: data.name,
        description: data.description || '',
        prizeTypeId: String(data.prizeTypeId),
        monetaryValue: data.monetaryValue ? String(data.monetaryValue) : '',
        totalQuantity: String(data.totalQuantity),
        imageUrl: data.imageUrl || '',
        expiryDate: data.expiryDate ? (String(data.expiryDate).split('T')[0] ?? '') : '',
        isActive: data.isActive,
      });
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to load prize';
      setError(errorMessage);
      console.error('Load prize error:', err);
    } finally {
      setLoading(false);
    }
  }, [prizeId]);

  const loadPrizeTypes = useCallback(async () => {
    try {
      const types = await apiClient.get<PrizeTypeResponse[]>('/api/prizes/types');
      setPrizeTypes(types);
    } catch (err) {
      console.error('Failed to load prize types:', err);
    }
  }, []);

  useEffect(() => {
    void loadPrize();
    void loadPrizeTypes();
  }, [loadPrize, loadPrizeTypes]);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target;
    if (type === 'checkbox') {
      const checked = (e.target as HTMLInputElement).checked;
      setFormData((prev) => ({
        ...prev,
        [name]: checked,
      }));
    } else {
      setFormData((prev) => ({
        ...prev,
        [name]: value,
      }));
    }
  };

  const handleSave = async () => {
    if (!formData.name.trim()) {
      setError('Name is required');
      return;
    }

    setSaving(true);
    setError(null);

    try {
      const request: UpdatePrizeRequest = {
        prizeTypeId: Number(formData.prizeTypeId),
        name: formData.name.trim(),
        totalQuantity: Number(formData.totalQuantity) || 1,
        isActive: formData.isActive,
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

      await prizeService.update(prizeId, request);
      setIsEditMode(false);
      void loadPrize();
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to save prize';
      setError(errorMessage);
      console.error('Save prize error:', err);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!prize) return;

    const confirmed = window.confirm(
      `Are you sure you want to delete "${prize.name}"? This action cannot be undone.`
    );

    if (!confirmed) return;

    try {
      await prizeService.delete(prizeId);
      navigate(`/admin/prize-pools/${String(prize.prizePoolId)}`);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to delete prize';
      setError(errorMessage);
      console.error('Delete prize error:', err);
    }
  };

  const handleCancel = () => {
    if (prize) {
      setFormData({
        name: prize.name,
        description: prize.description || '',
        prizeTypeId: String(prize.prizeTypeId),
        monetaryValue: prize.monetaryValue ? String(prize.monetaryValue) : '',
        totalQuantity: String(prize.totalQuantity),
        imageUrl: prize.imageUrl || '',
        expiryDate: prize.expiryDate ? (String(prize.expiryDate).split('T')[0] ?? '') : '',
        isActive: prize.isActive,
      });
    }
    setIsEditMode(false);
    setError(null);
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

  if (loading) {
    return (
      <div className={styles.page}>
        <div className={styles.loading}>Loading prize...</div>
      </div>
    );
  }

  if (error && !prize) {
    return (
      <div className={styles.page}>
        <div className={styles.errorState}>
          <h2>Error</h2>
          <p>{error}</p>
          <Link to="/admin/prize-pools" className={styles.backLink}>
            ← Back to Prize Pools
          </Link>
        </div>
      </div>
    );
  }

  if (!prize) {
    return (
      <div className={styles.page}>
        <div className={styles.errorState}>
          <h2>Prize not found</h2>
          <Link to="/admin/prize-pools" className={styles.backLink}>
            ← Back to Prize Pools
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.page}>
      {/* Header */}
      <div className={styles.header}>
        <div className={styles.headerLeft}>
          <Link to={`/admin/prize-pools/${String(prize.prizePoolId)}`} className={styles.backLink}>
            ← Back to {prize.prizePoolName}
          </Link>
          {isEditMode ? (
            <input
              type="text"
              name="name"
              value={formData.name}
              onChange={handleChange}
              className={styles.titleInput}
              placeholder="Prize name"
            />
          ) : (
            <h1>{prize.name}</h1>
          )}
          <div className={styles.headerMeta}>
            <span
              className={`${styles.statusBadge ?? ''} ${prize.isActive ? (styles.statusActive ?? '') : (styles.statusInactive ?? '')}`}
            >
              {prize.isActive ? 'Active' : 'Inactive'}
            </span>
            <span className={styles.typeBadge}>{prize.prizeTypeName}</span>
          </div>
        </div>
        <div className={styles.headerActions}>
          {isEditMode ? (
            <>
              <button className={styles.cancelButton} onClick={handleCancel} disabled={saving}>
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
            <>
              <button className={styles.editButton} onClick={() => setIsEditMode(true)}>
                Edit
              </button>
              <button className={styles.deleteButton} onClick={() => void handleDelete()}>
                Delete
              </button>
            </>
          )}
        </div>
      </div>

      {/* Error Banner */}
      {error && (
        <div className={styles.errorBanner}>
          <p>{error}</p>
          <button className={styles.dismissButton} onClick={() => setError(null)}>
            ×
          </button>
        </div>
      )}

      {/* Stats Cards */}
      <div className={styles.statsRow}>
        <div className={styles.statCard}>
          <span className={styles.statValue}>{prize.totalQuantity}</span>
          <span className={styles.statLabel}>Total</span>
        </div>
        <div className={`${styles.statCard ?? ''} ${styles.available ?? ''}`}>
          <span className={styles.statValue}>{prize.remainingQuantity}</span>
          <span className={styles.statLabel}>Available</span>
        </div>
        <div className={`${styles.statCard ?? ''} ${styles.awarded ?? ''}`}>
          <span className={styles.statValue}>{prize.awardedQuantity}</span>
          <span className={styles.statLabel}>Awarded</span>
        </div>
        <div className={`${styles.statCard ?? ''} ${styles.redeemed ?? ''}`}>
          <span className={styles.statValue}>{prize.redeemedQuantity}</span>
          <span className={styles.statLabel}>Redeemed</span>
        </div>
      </div>

      {/* Details */}
      <div className={styles.detailsGrid}>
        <div className={styles.detailCard}>
          <h3>Prize Information</h3>
          {isEditMode ? (
            <>
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
                >
                  {prizeTypes.map((type) => (
                    <option key={type.prizeTypeId} value={type.prizeTypeId}>
                      {type.name}
                    </option>
                  ))}
                </select>
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
                  rows={3}
                  placeholder="Prize description"
                />
              </div>
              <div className={styles.formRow}>
                <div className={styles.formGroup}>
                  <label htmlFor="monetaryValue" className={styles.label}>
                    Monetary Value
                  </label>
                  <input
                    id="monetaryValue"
                    type="number"
                    name="monetaryValue"
                    value={formData.monetaryValue}
                    onChange={handleChange}
                    className={styles.input}
                    min="0"
                    step="0.01"
                    placeholder="0.00"
                  />
                </div>
                <div className={styles.formGroup}>
                  <label htmlFor="totalQuantity" className={styles.label}>
                    Total Quantity <span className={styles.required}>*</span>
                  </label>
                  <input
                    id="totalQuantity"
                    type="number"
                    name="totalQuantity"
                    value={formData.totalQuantity}
                    onChange={handleChange}
                    className={styles.input}
                    min="1"
                    placeholder="1"
                  />
                </div>
              </div>
              <div className={styles.formGroup}>
                <label htmlFor="expiryDate" className={styles.label}>
                  Expiry Date
                </label>
                <input
                  id="expiryDate"
                  type="date"
                  name="expiryDate"
                  value={formData.expiryDate}
                  onChange={handleChange}
                  className={styles.input}
                  title="Expiry date"
                />
              </div>
              <div className={styles.formGroup}>
                <label htmlFor="imageUrl" className={styles.label}>
                  Image URL
                </label>
                <input
                  id="imageUrl"
                  type="url"
                  name="imageUrl"
                  value={formData.imageUrl}
                  onChange={handleChange}
                  className={styles.input}
                  placeholder="https://example.com/image.jpg"
                />
              </div>
              <div className={styles.formGroup}>
                <label className={styles.checkboxLabel}>
                  <input
                    type="checkbox"
                    name="isActive"
                    checked={formData.isActive}
                    onChange={handleChange}
                  />
                  Active
                </label>
              </div>
            </>
          ) : (
            <>
              <div className={styles.detailRow}>
                <span className={styles.detailLabel}>Type</span>
                <span className={styles.detailValue}>{prize.prizeTypeName}</span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.detailLabel}>Description</span>
                <span className={styles.detailValue}>{prize.description || '-'}</span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.detailLabel}>Value</span>
                <span className={styles.detailValue}>
                  {prize.monetaryValue !== undefined ? formatCurrency(prize.monetaryValue) : '-'}
                </span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.detailLabel}>Expires</span>
                <span className={styles.detailValue}>
                  {prize.expiryDate ? formatDate(prize.expiryDate) : '-'}
                </span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.detailLabel}>Created</span>
                <span className={styles.detailValue}>{formatDate(prize.createdAt)}</span>
              </div>
            </>
          )}
        </div>

        <div className={styles.detailCard}>
          <h3>Prize Pool</h3>
          <div className={styles.detailRow}>
            <span className={styles.detailLabel}>Pool</span>
            <span className={styles.detailValue}>
              <Link to={`/admin/prize-pools/${String(prize.prizePoolId)}`}>
                {prize.prizePoolName}
              </Link>
            </span>
          </div>
          <div className={styles.detailRow}>
            <span className={styles.detailLabel}>Availability</span>
            <span className={styles.detailValue}>
              {prize.remainingQuantity} / {prize.totalQuantity} remaining
            </span>
          </div>
        </div>

        {prize.imageUrl && (
          <div className={styles.detailCard}>
            <h3>Image Preview</h3>
            <img src={prize.imageUrl} alt={prize.name} className={styles.prizeImage} />
          </div>
        )}
      </div>
    </div>
  );
};
