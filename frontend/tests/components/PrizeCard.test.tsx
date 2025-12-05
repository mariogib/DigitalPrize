import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { PrizeCard } from '../../src/components/features/PrizeCard';
import { type PrizeSummaryResponse } from '../../src/types';

const mockPrize: PrizeSummaryResponse = {
  prizeId: 1,
  prizePoolId: 1,
  prizePoolName: 'Test Pool',
  prizeTypeId: 1,
  prizeTypeName: 'Gift Card',
  name: 'Test Prize',
  monetaryValue: 50,
  totalQuantity: 100,
  remainingQuantity: 75,
  awardedQuantity: 20,
  redeemedQuantity: 5,
  isActive: true,
  expiryDate: '2025-12-31T00:00:00Z',
};

describe('PrizeCard', () => {
  it('should render prize name', () => {
    render(<PrizeCard prize={mockPrize} />);

    expect(screen.getByText('Test Prize')).toBeInTheDocument();
  });

  it('should render prize type name', () => {
    render(<PrizeCard prize={mockPrize} />);

    expect(screen.getByText('Gift Card')).toBeInTheDocument();
  });

  it('should render formatted prize value', () => {
    render(<PrizeCard prize={mockPrize} />);

    expect(screen.getByText('$50.00')).toBeInTheDocument();
  });

  it('should render active status when prize is active', () => {
    render(<PrizeCard prize={mockPrize} />);

    expect(screen.getByText('Active')).toBeInTheDocument();
  });

  it('should call onSelect with prizeId when clicked', () => {
    const onSelect = vi.fn();
    render(<PrizeCard prize={mockPrize} onSelect={onSelect} />);

    fireEvent.click(screen.getByText('Test Prize'));

    expect(onSelect).toHaveBeenCalledWith(1);
  });

  it('should not throw when onSelect is undefined', () => {
    render(<PrizeCard prize={mockPrize} />);

    expect(() => {
      fireEvent.click(screen.getByText('Test Prize'));
    }).not.toThrow();
  });

  it('should render quantity information', () => {
    render(<PrizeCard prize={mockPrize} />);

    expect(screen.getByText('75 / 100 available')).toBeInTheDocument();
  });
});
