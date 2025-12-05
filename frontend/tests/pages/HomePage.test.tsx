import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { HomePage } from '../../src/pages/HomePage';

describe('HomePage', () => {
  it('should render welcome message', () => {
    render(
      <BrowserRouter>
        <HomePage />
      </BrowserRouter>
    );

    expect(screen.getByText('Welcome to DigitalPrizes')).toBeInTheDocument();
  });

  it('should render call to action link', () => {
    render(
      <BrowserRouter>
        <HomePage />
      </BrowserRouter>
    );

    const ctaLink = screen.getByRole('link', { name: /view prizes/i });
    expect(ctaLink).toBeInTheDocument();
    expect(ctaLink).toHaveAttribute('href', '/prizes');
  });
});
