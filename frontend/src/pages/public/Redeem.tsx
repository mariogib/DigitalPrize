/**
 * Public Prize Redemption Page
 * Allows users to redeem their won prizes
 */

import React, { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { redemptionService } from '../../services';
import type { RedeemablePrizeResponse } from '../../types';
import styles from './Redeem.module.css';

type RedemptionStep = 'lookup' | 'select' | 'confirm' | 'success';

interface RedemptionState {
  cellNumber: string;
  otp: string;
  requiresVerification: boolean;
  redeemablePrizes: RedeemablePrizeResponse[];
  selectedPrize: RedeemablePrizeResponse | null;
}

export const Redeem: React.FC = () => {
  const [searchParams] = useSearchParams();
  const initialCode = searchParams.get('code') ?? '';

  const [step, setStep] = useState<RedemptionStep>('lookup');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [state, setState] = useState<RedemptionState>({
    cellNumber: '',
    otp: '',
    requiresVerification: false,
    redeemablePrizes: [],
    selectedPrize: null,
  });

  const [redemptionCode, setRedemptionCode] = useState(initialCode);

  const handleLookup = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!state.cellNumber.trim()) {
      setError('Please enter your cell number');
      return;
    }

    setLoading(true);
    try {
      const result = await redemptionService.initiateRedemption({
        cellNumber: state.cellNumber.trim(),
      });

      if (!result.success) {
        setError(result.message ?? 'No prizes available for redemption');
        return;
      }

      setState((prev) => ({
        ...prev,
        requiresVerification: result.requiresVerification,
        redeemablePrizes: result.redeemablePrizes,
      }));

      if (result.redeemablePrizes.length === 1) {
        const firstPrize = result.redeemablePrizes[0];
        if (firstPrize) {
          setState((prev) => ({
            ...prev,
            selectedPrize: firstPrize,
          }));
        }
        setStep('confirm');
      } else {
        setStep('select');
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Lookup failed';
      setError(errorMessage);
      console.error('Lookup error:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSelectPrize = (prize: RedeemablePrizeResponse) => {
    setState((prev) => ({
      ...prev,
      selectedPrize: prize,
    }));
    setStep('confirm');
  };

  const handleRedeem = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!state.selectedPrize) {
      setError('No prize selected');
      return;
    }

    if (state.requiresVerification && !state.otp.trim()) {
      setError('Please enter the OTP sent to your phone');
      return;
    }

    setError(null);
    setLoading(true);

    try {
      const result = await redemptionService.completeRedemption({
        cellNumber: state.cellNumber.trim(),
        otpCode: state.otp.trim(),
        prizeAwardId: state.selectedPrize.prizeAwardId,
        redemptionChannel: 'web',
      });

      if (!result.success) {
        setError(result.message ?? 'Redemption failed');
        return;
      }

      if (result.redemptionCode) {
        setRedemptionCode(result.redemptionCode);
      }
      setStep('success');
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Redemption failed';
      setError(errorMessage);
      console.error('Redemption error:', err);
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (value?: number): string => {
    if (value === undefined) return '-';
    return new Intl.NumberFormat('en-ZA', {
      style: 'currency',
      currency: 'ZAR',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(value);
  };

  const renderLookupStep = () => (
    <div className={styles.stepContent}>
      <div className={styles.icon}>🎁</div>
      <h1>Redeem Your Prize</h1>
      <p className={styles.subtitle}>
        Enter your cell number to check if you have any prizes to redeem.
      </p>

      <form onSubmit={(e) => void handleLookup(e)} className={styles.form}>
        {error && <div className={styles.errorBanner}>{error}</div>}

        <div className={styles.formGroup}>
          <label htmlFor="cellNumber" className={styles.label}>
            Cell Number
          </label>
          <input
            type="tel"
            id="cellNumber"
            value={state.cellNumber}
            onChange={(e) => setState((prev) => ({ ...prev, cellNumber: e.target.value }))}
            className={styles.input}
            placeholder="+27 00 000 0000"
            required
          />
        </div>

        <button type="submit" className={styles.primaryButton} disabled={loading}>
          {loading ? 'Checking...' : 'Check for Prizes'}
        </button>
      </form>
    </div>
  );

  const renderSelectStep = () => (
    <div className={styles.stepContent}>
      <div className={styles.icon}>🏆</div>
      <h1>Select a Prize</h1>
      <p className={styles.subtitle}>You have {state.redeemablePrizes.length} prizes available!</p>

      <div className={styles.prizeList}>
        {state.redeemablePrizes.map((prize) => (
          <button
            key={prize.prizeAwardId}
            className={styles.prizeSelectCard}
            onClick={() => handleSelectPrize(prize)}
          >
            <div className={styles.prizeHeader}>
              <span className={styles.prizeType}>{prize.prizeTypeName}</span>
              <span className={styles.prizeName}>{prize.prizeName}</span>
            </div>
            {prize.monetaryValue !== undefined && (
              <div className={styles.prizeValue}>{formatCurrency(prize.monetaryValue)}</div>
            )}
            {prize.expiryDate && (
              <div className={styles.prizeExpiry}>
                Expires: {new Date(prize.expiryDate).toLocaleDateString()}
              </div>
            )}
          </button>
        ))}
      </div>

      <button type="button" className={styles.linkButton} onClick={() => setStep('lookup')}>
        ← Back
      </button>
    </div>
  );

  const renderConfirmStep = () => (
    <div className={styles.stepContent}>
      <div className={styles.icon}>🎊</div>
      <h1>Confirm Redemption</h1>
      <p className={styles.subtitle}>You&apos;re about to redeem this prize!</p>

      {state.selectedPrize && (
        <div className={styles.prizeCard}>
          {state.selectedPrize.prizeTypeName && (
            <div className={styles.prizeHeader}>
              <span className={styles.prizeType}>{state.selectedPrize.prizeTypeName}</span>
              <span className={styles.prizeName}>{state.selectedPrize.prizeName}</span>
            </div>
          )}
          {state.selectedPrize.monetaryValue !== undefined && (
            <div className={styles.prizeValue}>
              {formatCurrency(state.selectedPrize.monetaryValue)}
            </div>
          )}
          {state.selectedPrize.expiryDate && (
            <div className={styles.prizeExpiry}>
              Expires: {new Date(state.selectedPrize.expiryDate).toLocaleDateString()}
            </div>
          )}
        </div>
      )}

      <form onSubmit={(e) => void handleRedeem(e)} className={styles.form}>
        {error && <div className={styles.errorBanner}>{error}</div>}

        {state.requiresVerification && (
          <>
            <div className={styles.otpSection}>
              <p className={styles.otpInfo}>
                An OTP has been sent to your phone. Enter it below to verify your identity.
              </p>
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="otp" className={styles.label}>
                Enter OTP
              </label>
              <input
                type="text"
                id="otp"
                value={state.otp}
                onChange={(e) => setState((prev) => ({ ...prev, otp: e.target.value }))}
                className={styles.otpInput}
                placeholder="000000"
                maxLength={6}
                required
              />
            </div>
          </>
        )}

        <button type="submit" className={styles.primaryButton} disabled={loading}>
          {loading ? 'Redeeming...' : 'Confirm Redemption'}
        </button>

        <button
          type="button"
          className={styles.linkButton}
          onClick={() => {
            if (state.redeemablePrizes.length > 1) {
              setStep('select');
            } else {
              setStep('lookup');
            }
          }}
        >
          ← Back
        </button>
      </form>
    </div>
  );

  const renderSuccessStep = () => (
    <div className={styles.stepContent}>
      <div className={styles.successIcon}>✅</div>
      <h1>Prize Redeemed!</h1>
      <p className={styles.subtitle}>Your prize has been successfully redeemed.</p>

      {redemptionCode && (
        <div className={styles.redemptionCode}>
          <span className={styles.codeLabel}>Your Redemption Code</span>
          <span className={styles.codeValue}>{redemptionCode}</span>
          <button
            className={styles.copyButton}
            onClick={() => void navigator.clipboard.writeText(redemptionCode)}
          >
            Copy
          </button>
        </div>
      )}

      {state.selectedPrize && (
        <>
          <div className={styles.prizeCard}>
            <div className={styles.prizeHeader}>
              <span className={styles.prizeType}>{state.selectedPrize.prizeTypeName}</span>
              <span className={styles.prizeName}>{state.selectedPrize.prizeName}</span>
            </div>
            {state.selectedPrize.monetaryValue !== undefined && (
              <div className={styles.prizeValue}>
                {formatCurrency(state.selectedPrize.monetaryValue)}
              </div>
            )}
          </div>
        </>
      )}

      <button
        className={styles.primaryButton}
        onClick={() => {
          setState({
            cellNumber: '',
            otp: '',
            requiresVerification: false,
            redeemablePrizes: [],
            selectedPrize: null,
          });
          setStep('lookup');
          setRedemptionCode('');
        }}
      >
        Redeem Another Prize
      </button>
    </div>
  );

  return (
    <div className={styles.redeemPage}>
      <div className={styles.container}>
        {step === 'lookup' && renderLookupStep()}
        {step === 'select' && renderSelectStep()}
        {step === 'confirm' && renderConfirmStep()}
        {step === 'success' && renderSuccessStep()}
      </div>
    </div>
  );
};
