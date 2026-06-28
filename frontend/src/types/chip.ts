export interface PortfolioSignals {
  overdueFeeCount: number;
  upcomingCallCount: number;
  upcomingFeeCount: number;
  distributionCount: number;
  positionCount: number;
  topSector: string | null;
  hasExitedDeals: boolean;
  hasWrittenOffDeals: boolean;
  firstCompanyName: string | null;
}

export interface InvestorSummary {
  investorId: string;
  investorName: string;
  investorType: string;
  country: string;
  reportingCurrency: string;
  techSavviness: "Low" | "Medium" | "High";
  age: number | null;
  kycStatus: string;
  signals: PortfolioSignals;
}

export interface ChatMessage {
  role: "user" | "assistant";
  content: string;
}

export interface ChatResponse {
  reply: string;
}

export interface ApiError {
  error: string;
  detail?: string | null;
}