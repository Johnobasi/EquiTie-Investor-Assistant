import type { PortfolioSignals } from "./types/api";

export interface Chip {
  label: string;
  q: string;
  urgent?: boolean;
  /**
   * The CSV files this question type draws on. Defined here in code — not by the
   * model — so the citation footer stays deterministic and specific to the
   * question, never a blanket list. Every position/value question needs the
   * common base (allocations + valuations + fx + deals + companies); the rest
   * add only what their question type actually reads.
   */
  sources: string[];
}

// Common base: every figure that involves holdings or value needs these.
const BASE = [
  "allocations.csv",
  "valuations.csv",
  "fx_rates.csv",
  "deals.csv",
  "portfolio_companies.csv",
];

// Investor-aware suggestion chips covering all 7 question types.
export function buildChips(s: PortfolioSignals): Chip[] {
  const chips: Chip[] = [];

  if (s.overdueFeeCount > 0)
    chips.push({
      label: `${s.overdueFeeCount} overdue fee${s.overdueFeeCount > 1 ? "s" : ""}`,
      q: "Do I have any overdue fees? List them with amounts and due dates.",
      urgent: true,
      sources: ["fees.csv", "deals.csv", "fx_rates.csv"],
    });
  if (s.upcomingCallCount > 0)
    chips.push({
      label: `${s.upcomingCallCount} upcoming capital call${s.upcomingCallCount > 1 ? "s" : ""}`,
      q: "What capital calls are coming up - how much and when?",
      urgent: true,
      sources: ["capital_calls.csv", "deals.csv", "fx_rates.csv"],
    });

  chips.push({
    label: "Portfolio overview",
    q: "Give me a full portfolio overview - holdings, current value, committed vs contributed, and overall MOIC.",
    sources: BASE,
  });

  if (s.firstCompanyName)
    chips.push({
      label: `My ${s.firstCompanyName} position`,
      q: `Tell me about my ${s.firstCompanyName} position - current value, cost basis, the share price I paid, and MOIC.`,
      sources: BASE,
    });

  if (s.distributionCount > 0)
    chips.push({
      label: "My distributions",
      q: "What distributions have I received? Show gross, the carry taken, and what I actually received net.",
      sources: ["distributions.csv", "deals.csv", "fx_rates.csv"],
    });
  else if (s.hasExitedDeals)
    chips.push({
      label: "Exited deals",
      q: "Which of my deals have exited and what did I receive after the performance fee?",
      sources: ["distributions.csv", "deals.csv", "portfolio_companies.csv", "fx_rates.csv"],
    });

  chips.push({
    label: "Fee breakdown",
    q: "What fees do I pay across my deals, including any discounts on my allocations?",
    sources: ["allocations.csv", "fees.csv", "deals.csv", "fx_rates.csv"],
  });

  if (s.topSector)
    chips.push({
      label: `${s.topSector} valuations`,
      q: `How have the valuations moved for my ${s.topSector} holdings, and what has that done to my MOIC?`,
      sources: BASE,
    });

  if (s.hasWrittenOffDeals)
    chips.push({
      label: "Write-offs",
      q: "Which of my investments have been written off, and what was my loss?",
      sources: ["allocations.csv", "valuations.csv", "deals.csv", "portfolio_companies.csv", "fx_rates.csv"],
    });

  chips.push({
    label: "Account statement",
    q: "Give me a plain-language account statement - all my contributions, fees paid, and distributions received.",
    sources: ["statement_lines.csv", "deals.csv", "fx_rates.csv"],
  });

  return chips;
}