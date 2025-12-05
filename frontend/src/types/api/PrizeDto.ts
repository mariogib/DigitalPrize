export interface CreatePrizeDto {
  name: string;
  description: string;
  value: number;
}

export type UpdatePrizeDto = Partial<CreatePrizeDto>;
