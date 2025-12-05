/**
 * Public Status Check Page
 * Allows users to check their registration and prize status
 */

import React, { useState } from 'react';
import { redemptionService, registrationService } from '../../services';
import styles from './Status.module.css';

type StatusStep = 'lookup' | 'result';
type LookupType = 'registration' | 'redemption';

interface RegistrationStatus {
  type: 'registration';
  confirmationCode: string;
  status: string;
  message: string;
  awards: Array<{
    prizeType: string;
    prizeName: string;
    status: string;
  }>;
}

interface RedemptionStatus {
  type: 'redemption';
  status: string;
  message: string;
  completedAt?: string;
}

type StatusResult = RegistrationStatus | RedemptionStatus;

export const Status: React.FC = () => {
  const [step, setStep] = useState<StatusStep>('lookup');
  const [lookupType, setLookupType] = useState<LookupType>('registration');
  const [code, setCode] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<StatusResult | null>(null);

  const handleLookup = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!code.trim()) {
      setError('Please enter your confirmation or redemption code');
      return;
    }

    setLoading(true);
    try {
      if (lookupType === 'registration') {
        const status = await registrationService.checkStatus(code.trim());
        setResult({
          type: 'registration',
          confirmationCode: code.trim(),
          status: status.status,
          message: status.message,
          awards: status.awards ?? [],
        });
      } else {
        const status = await redemptionService.getRedemptionStatus(code.trim());
        const redemptionResult: RedemptionStatus = {
          type: 'redemption',
          status: status.status,
          message: status.message,
        };
        if (status.completedAt) {
          redemptionResult.completedAt = status.completedAt;
        }
        setResult(redemptionResult);
      }
      setStep('result');
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Lookup failed';
      setError(errorMessage);
      console.error('Status lookup error:', err);
    } finally {
      setLoading(false);
    }
  };

  const getStatusIcon = (status: string): string => {
    const statusLower = status.toLowerCase();
    if (
      statusLower.includes('complete') ||
      statusLower.includes('approved') ||
      statusLower.includes('confirmed')
    ) {
      return '✅';
    }
    if (statusLower.includes('pending') || statusLower.includes('processing')) {
      return '⏳';
    }
    if (
      statusLower.includes('reject') ||
      statusLower.includes('denied') ||
      statusLower.includes('expired')
    ) {
      return '❌';
    }
    return '📋';
  };

  const getStatusClass = (status: string): string => {
    const statusLower = status.toLowerCase();
    if (
      statusLower.includes('complete') ||
      statusLower.includes('approved') ||
      statusLower.includes('confirmed')
    ) {
      return styles.statusSuccess ?? '';
    }
    if (statusLower.includes('pending') || statusLower.includes('processing')) {
      return styles.statusPending ?? '';
    }
    if (
      statusLower.includes('reject') ||
      statusLower.includes('denied') ||
      statusLower.includes('expired')
    ) {
      return styles.statusError ?? '';
    }
    return styles.statusNeutral ?? '';
  };

  const renderLookupStep = () => (
    <div className={styles.stepContent}>
      <div className={styles.icon}>🔍</div>
      <h1>Check Your Status</h1>
      <p className={styles.subtitle}>
        Enter your code to check your registration or redemption status.
      </p>

      <div className={styles.typeSelector}>
        <button
          type="button"
          className={`${styles.typeButton ?? ''} ${lookupType === 'registration' ? (styles.active ?? '') : ''}`}
          onClick={() => setLookupType('registration')}
        >
          Registration
        </button>
        <button
          type="button"
          className={`${styles.typeButton ?? ''} ${lookupType === 'redemption' ? (styles.active ?? '') : ''}`}
          onClick={() => setLookupType('redemption')}
        >
          Redemption
        </button>
      </div>

      <form onSubmit={(e) => void handleLookup(e)} className={styles.form}>
        {error && <div className={styles.errorBanner}>{error}</div>}

        <div className={styles.formGroup}>
          <label htmlFor="code" className={styles.label}>
            {lookupType === 'registration' ? 'Confirmation Code' : 'Redemption Code'}
          </label>
          <input
            type="text"
            id="code"
            value={code}
            onChange={(e) => setCode(e.target.value)}
            className={styles.input}
            placeholder={
              lookupType === 'registration'
                ? 'Enter your confirmation code'
                : 'Enter your redemption code'
            }
            required
          />
        </div>

        <button type="submit" className={styles.primaryButton} disabled={loading}>
          {loading ? 'Checking...' : 'Check Status'}
        </button>
      </form>
    </div>
  );

  const renderRegistrationResult = (data: RegistrationStatus) => (
    <div className={styles.stepContent}>
      <div className={styles.icon}>{getStatusIcon(data.status)}</div>
      <h1>Registration Status</h1>

      <div className={styles.resultCard}>
        <div className={styles.resultHeader}>
          <span className={styles.resultLabel}>Confirmation Code</span>
          <span className={styles.resultCode}>{data.confirmationCode}</span>
        </div>

        <div
          className={`${styles.statusBadge ?? ''} ${getStatusClass(data.status)} ${styles.large ?? ''}`}
        >
          {data.status}
        </div>

        <p className={styles.statusMessage}>{data.message}</p>

        {data.awards.length > 0 && (
          <div className={styles.awardsSection}>
            <h3 className={styles.awardsSectionTitle}>🎉 Your Prizes</h3>
            <div className={styles.awardsList}>
              {data.awards.map((award, index) => (
                <div key={index} className={styles.awardItem}>
                  <div className={styles.awardHeader}>
                    <span className={styles.awardType}>{award.prizeType}</span>
                    <span className={`${styles.awardStatus ?? ''} ${getStatusClass(award.status)}`}>
                      {award.status}
                    </span>
                  </div>
                  <span className={styles.awardName}>{award.prizeName}</span>
                </div>
              ))}
            </div>
            <a href="/redeem" className={styles.redeemLink}>
              Redeem Your Prizes →
            </a>
          </div>
        )}
      </div>

      <button
        className={styles.secondaryButton}
        onClick={() => {
          setStep('lookup');
          setResult(null);
          setCode('');
        }}
      >
        Check Another Code
      </button>
    </div>
  );

  const renderRedemptionResult = (data: RedemptionStatus) => (
    <div className={styles.stepContent}>
      <div className={styles.icon}>{getStatusIcon(data.status)}</div>
      <h1>Redemption Status</h1>

      <div className={styles.resultCard}>
        <div
          className={`${styles.statusBadge ?? ''} ${getStatusClass(data.status)} ${styles.large ?? ''}`}
        >
          {data.status}
        </div>

        <p className={styles.statusMessage}>{data.message}</p>

        {data.completedAt && (
          <div className={styles.completedInfo}>
            <span className={styles.detailLabel}>Completed</span>
            <span className={styles.detailValue}>
              {new Date(data.completedAt).toLocaleDateString()} at{' '}
              {new Date(data.completedAt).toLocaleTimeString()}
            </span>
          </div>
        )}
      </div>

      <button
        className={styles.secondaryButton}
        onClick={() => {
          setStep('lookup');
          setResult(null);
          setCode('');
        }}
      >
        Check Another Code
      </button>
    </div>
  );

  const renderResultStep = () => {
    if (!result) return null;

    if (result.type === 'registration') {
      return renderRegistrationResult(result);
    }
    return renderRedemptionResult(result);
  };

  return (
    <div className={styles.statusPage}>
      <div className={styles.container}>
        {step === 'lookup' && renderLookupStep()}
        {step === 'result' && renderResultStep()}
      </div>
    </div>
  );
};
