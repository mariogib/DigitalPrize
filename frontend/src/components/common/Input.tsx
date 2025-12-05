import React from 'react';
import styles from './Input.module.css';

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
}

export const Input: React.FC<InputProps> = ({ label, error, id, className, ...props }) => {
  const inputId = id ?? (label ? label.toLowerCase().replace(/\s+/g, '-') : undefined);
  const errorClass = error ? ` ${String(styles.error)}` : '';
  const extraClass = className ? ` ${className}` : '';

  return (
    <div className={styles.container}>
      {label && (
        <label htmlFor={inputId} className={styles.label}>
          {label}
        </label>
      )}
      <input id={inputId} className={[styles.input, errorClass, extraClass].join('')} {...props} />
      {error && <span className={styles.errorMessage}>{error}</span>}
    </div>
  );
};
