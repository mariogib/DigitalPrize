/**
 * Public Competition Registration Page
 * Allows users to register for active competitions
 */

import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { competitionService, registrationService } from '../../services';
import type { CompetitionDetailResponse, RegistrationFieldResponse } from '../../types';
import styles from './Register.module.css';

interface FormData {
  cellNumber: string;
  fieldValues: Record<string, string>;
}

interface FormErrors {
  cellNumber?: string;
  fieldValues?: Record<string, string>;
  general?: string;
}

export const Register: React.FC = () => {
  const { competitionId } = useParams<{ competitionId: string }>();
  const navigate = useNavigate();

  const [competition, setCompetition] = useState<CompetitionDetailResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const [formData, setFormData] = useState<FormData>({
    cellNumber: '',
    fieldValues: {},
  });
  const [formErrors, setFormErrors] = useState<FormErrors>({});

  useEffect(() => {
    const fetchCompetition = async () => {
      if (!competitionId) {
        setError('Invalid competition ID');
        setLoading(false);
        return;
      }

      try {
        const data = await competitionService.getCompetition(Number(competitionId));
        setCompetition(data);

        // Initialize field values
        const initialFieldValues: Record<string, string> = {};
        data.registrationFields.forEach((field) => {
          initialFieldValues[String(field.registrationFieldId)] = '';
        });
        setFormData((prev) => ({ ...prev, fieldValues: initialFieldValues }));
      } catch (err) {
        setError('Competition not found or no longer active');
        console.error('Competition error:', err);
      } finally {
        setLoading(false);
      }
    };

    void fetchCompetition();
  }, [competitionId]);

  const validateForm = (): boolean => {
    const errors: FormErrors = {};
    let isValid = true;

    // Validate cell number
    if (!formData.cellNumber.trim()) {
      errors.cellNumber = 'Cell number is required';
      isValid = false;
    } else if (!/^\+?[\d\s-]{10,}$/.test(formData.cellNumber.trim())) {
      errors.cellNumber = 'Please enter a valid cell number';
      isValid = false;
    }

    // Validate required fields
    if (competition?.registrationFields) {
      const fieldErrors: Record<string, string> = {};
      competition.registrationFields.forEach((field) => {
        if (field.isRequired && !formData.fieldValues[String(field.registrationFieldId)]?.trim()) {
          fieldErrors[String(field.registrationFieldId)] = `${field.fieldName} is required`;
          isValid = false;
        }
      });
      if (Object.keys(fieldErrors).length > 0) {
        errors.fieldValues = fieldErrors;
      }
    }

    setFormErrors(errors);
    return isValid;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm() || !competition) {
      return;
    }

    setSubmitting(true);
    setFormErrors({});

    try {
      // Convert fieldValues to answers format
      const answers = Object.entries(formData.fieldValues)
        .filter(([, value]) => value.trim() !== '')
        .map(([fieldId, value]) => ({
          registrationFieldId: Number(fieldId),
          value: value.trim(),
        }));

      await registrationService.submitRegistration({
        competitionId: competition.competitionId,
        cellNumber: formData.cellNumber.trim(),
        answers,
      });
      setSuccess(true);
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : 'Registration failed. Please try again.';
      setFormErrors({ general: errorMessage });
      console.error('Registration error:', err);
    } finally {
      setSubmitting(false);
    }
  };

  const handleFieldChange = (fieldId: string, value: string) => {
    setFormData((prev) => ({
      ...prev,
      fieldValues: {
        ...prev.fieldValues,
        [fieldId]: value,
      },
    }));
  };

  const renderField = (field: RegistrationFieldResponse) => {
    const fieldId = String(field.registrationFieldId);
    const hasError = formErrors.fieldValues?.[fieldId];
    const value = formData.fieldValues[fieldId] ?? '';

    switch (field.fieldType.toLowerCase()) {
      case 'select':
      case 'dropdown': {
        const options = field.options ? (JSON.parse(field.options) as string[]) : [];
        return (
          <select
            id={fieldId}
            value={value}
            onChange={(e) => handleFieldChange(fieldId, e.target.value)}
            className={[styles.input, hasError ? styles.inputError : ''].filter(Boolean).join(' ')}
            required={field.isRequired}
          >
            <option value="">Select...</option>
            {options.map((opt) => (
              <option key={opt} value={opt}>
                {opt}
              </option>
            ))}
          </select>
        );
      }

      case 'textarea':
        return (
          <textarea
            id={fieldId}
            value={value}
            onChange={(e) => handleFieldChange(fieldId, e.target.value)}
            className={[styles.textarea, hasError ? styles.inputError : '']
              .filter(Boolean)
              .join(' ')}
            required={field.isRequired}
            rows={3}
          />
        );

      case 'checkbox':
        return (
          <label className={styles.checkboxLabel}>
            <input
              type="checkbox"
              checked={value === 'true'}
              onChange={(e) => handleFieldChange(fieldId, e.target.checked ? 'true' : 'false')}
              className={styles.checkbox}
            />
            <span>{field.fieldName}</span>
          </label>
        );

      default:
        return (
          <input
            type={
              field.fieldType === 'email'
                ? 'email'
                : field.fieldType === 'number'
                  ? 'number'
                  : 'text'
            }
            id={fieldId}
            value={value}
            onChange={(e) => handleFieldChange(fieldId, e.target.value)}
            className={[styles.input, hasError ? styles.inputError : ''].filter(Boolean).join(' ')}
            required={field.isRequired}
          />
        );
    }
  };

  if (loading) {
    return (
      <div className={styles.loading}>
        <div className={styles.spinner} />
        <p>Loading...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className={styles.errorPage}>
        <h1>Oops!</h1>
        <p>{error}</p>
        <button onClick={() => navigate('/')} className={styles.button}>
          Back to Home
        </button>
      </div>
    );
  }

  if (success) {
    return (
      <div className={styles.successPage}>
        <div className={styles.successIcon}>✅</div>
        <h1>Registration Successful!</h1>
        <p>
          You have been registered for <strong>{competition?.name}</strong>
        </p>
        <p className={styles.successNote}>
          If you win a prize, you will be notified at {formData.cellNumber}
        </p>
        <button onClick={() => navigate('/')} className={styles.button}>
          Back to Home
        </button>
      </div>
    );
  }

  if (!competition) {
    return null;
  }

  return (
    <div className={styles.registerPage}>
      <div className={styles.container}>
        <div className={styles.header}>
          <h1>Register for Competition</h1>
          <h2>{competition.name}</h2>
          {competition.description && (
            <p className={styles.description}>{competition.description}</p>
          )}
        </div>

        <form onSubmit={(e) => void handleSubmit(e)} className={styles.form}>
          {formErrors.general && <div className={styles.errorBanner}>{formErrors.general}</div>}

          <div className={styles.formGroup}>
            <label htmlFor="cellNumber" className={styles.label}>
              Cell Number <span className={styles.required}>*</span>
            </label>
            <input
              type="tel"
              id="cellNumber"
              value={formData.cellNumber}
              onChange={(e) => setFormData((prev) => ({ ...prev, cellNumber: e.target.value }))}
              className={[styles.input, formErrors.cellNumber ? styles.inputError : '']
                .filter(Boolean)
                .join(' ')}
              placeholder="+27 00 000 0000"
              required
            />
            {formErrors.cellNumber && (
              <span className={styles.fieldError}>{formErrors.cellNumber}</span>
            )}
          </div>

          {competition.registrationFields
            .sort((a, b) => a.displayOrder - b.displayOrder)
            .map((field) => (
              <div key={field.registrationFieldId} className={styles.formGroup}>
                {field.fieldType !== 'checkbox' && (
                  <label htmlFor={String(field.registrationFieldId)} className={styles.label}>
                    {field.fieldName}
                    {field.isRequired && <span className={styles.required}> *</span>}
                  </label>
                )}
                {renderField(field)}
                {formErrors.fieldValues?.[String(field.registrationFieldId)] && (
                  <span className={styles.fieldError}>
                    {formErrors.fieldValues[String(field.registrationFieldId)]}
                  </span>
                )}
              </div>
            ))}

          <button type="submit" className={styles.submitButton} disabled={submitting}>
            {submitting ? 'Registering...' : 'Register'}
          </button>
        </form>

        <div className={styles.competitionInfo}>
          <h3>Competition Details</h3>
          <div className={styles.infoGrid}>
            <div className={styles.infoItem}>
              <span className={styles.infoLabel}>Start Date</span>
              <span className={styles.infoValue}>
                {new Date(competition.startDate).toLocaleDateString()}
              </span>
            </div>
            <div className={styles.infoItem}>
              <span className={styles.infoLabel}>End Date</span>
              <span className={styles.infoValue}>
                {new Date(competition.endDate).toLocaleDateString()}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
