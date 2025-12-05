export interface Prize {
  id: string;
  name: string;
  description: string;
  value: number;
  status: PrizeStatus;
  createdAt: string;
  updatedAt: string;
}

export type PrizeStatus = 'active' | 'inactive' | 'redeemed';
