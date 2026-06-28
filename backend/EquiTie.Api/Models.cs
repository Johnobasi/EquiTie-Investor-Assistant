using CsvHelper.Configuration.Attributes;

namespace EquiTie.Api;
public class Investor
{
    [Name("investor_id")] public string InvestorId { get; set; } = "";
    [Name("investor_name")] public string InvestorName { get; set; } = "";
    [Name("investor_type")] public string InvestorType { get; set; } = "";
    [Name("country")] public string Country { get; set; } = "";
    [Name("reporting_currency")] public string ReportingCurrency { get; set; } = "";
    [Name("age")] public int? Age { get; set; }
    [Name("tech_savviness")] public string TechSavviness { get; set; } = "";
    [Name("kyc_status")] public string KycStatus { get; set; } = "";
    [Name("onboarded_date")] public DateTime OnboardedDate { get; set; }
    [Name("email")] public string Email { get; set; } = "";
}

public class PortfolioCompany
{
    [Name("company_id")] public string CompanyId { get; set; } = "";
    [Name("company_name")] public string CompanyName { get; set; } = "";
    [Name("sector")] public string Sector { get; set; } = "";
    [Name("hq_country")] public string HqCountry { get; set; } = "";
    [Name("status")] public string Status { get; set; } = "";
    [Name("website")] public string Website { get; set; } = "";
}

public class Deal
{
    [Name("deal_id")] public string DealId { get; set; } = "";
    [Name("company_id")] public string CompanyId { get; set; } = "";
    [Name("company_name")] public string CompanyName { get; set; } = "";
    [Name("round")] public string Round { get; set; } = "";
    [Name("instrument")] public string Instrument { get; set; } = "";
    [Name("spv_name")] public string SpvName { get; set; } = "";
    [Name("deal_currency")] public string DealCurrency { get; set; } = "";
    [Name("deal_date")] public DateTime DealDate { get; set; }
    [Name("pre_money_valuation_m")] public decimal PreMoneyValuationM { get; set; }
    [Name("post_money_valuation_m")] public decimal PostMoneyValuationM { get; set; }
    [Name("round_size_m")] public decimal RoundSizeM { get; set; }
    [Name("equitie_allocation_m")] public decimal EquitieAllocationM { get; set; }
    [Name("entry_share_price")] public decimal EntrySharePrice { get; set; }
    [Name("contributed_pct")] public decimal ContributedPct { get; set; }
    [Name("std_mgmt_fee_pct")] public decimal StdMgmtFeePct { get; set; }
    [Name("std_performance_fee_pct")] public decimal StdPerformanceFeePct { get; set; }
    [Name("std_structuring_fee_pct")] public decimal StdStructuringFeePct { get; set; }
    [Name("std_admin_fee_usd")] public decimal StdAdminFeeUsd { get; set; }
    [Name("status")] public string Status { get; set; } = "";
}

public class Allocation
{
    [Name("allocation_id")] public string AllocationId { get; set; } = "";
    [Name("deal_id")] public string DealId { get; set; } = "";
    [Name("investor_id")] public string InvestorId { get; set; } = "";
    [Name("deal_currency")] public string DealCurrency { get; set; } = "";
    [Name("commitment_amount")] public decimal CommitmentAmount { get; set; }
    [Name("price_discount_pct")] public decimal PriceDiscountPct { get; set; }
    [Name("effective_share_price")] public decimal EffectiveSharePrice { get; set; }
    [Name("units")] public decimal Units { get; set; }
    [Name("contributed_amount")] public decimal ContributedAmount { get; set; }
    [Name("outstanding_commitment")] public decimal OutstandingCommitment { get; set; }
    [Name("mgmt_fee_pct")] public decimal MgmtFeePct { get; set; }
    [Name("performance_fee_pct")] public decimal PerformanceFeePct { get; set; }
    [Name("structuring_fee_pct")] public decimal StructuringFeePct { get; set; }
    [Name("admin_fee_usd")] public decimal AdminFeeUsd { get; set; }
    [Name("fee_discount")] public string FeeDiscount { get; set; } = "";
    [Name("allocation_status")] public string AllocationStatus { get; set; } = "";
    [Name("allocation_date")] public DateTime AllocationDate { get; set; }
}

public class Valuation
{
    [Name("valuation_id")] public string ValuationId { get; set; } = "";
    [Name("deal_id")] public string DealId { get; set; } = "";
    [Name("valuation_date")] public DateTime ValuationDate { get; set; }
    [Name("share_price")] public decimal SharePrice { get; set; }
    [Name("company_valuation_m")] public decimal CompanyValuationM { get; set; }
    [Name("mark_source")] public string MarkSource { get; set; } = "";
    [Name("multiple_vs_entry")] public decimal MultipleVsEntry { get; set; }
}

public class CapitalCall
{
    [Name("call_id")] public string CallId { get; set; } = "";
    [Name("allocation_id")] public string AllocationId { get; set; } = "";
    [Name("investor_id")] public string InvestorId { get; set; } = "";
    [Name("deal_id")] public string DealId { get; set; } = "";
    [Name("call_number")] public int CallNumber { get; set; }
    [Name("call_date")] public DateTime CallDate { get; set; }
    [Name("amount")] public decimal Amount { get; set; }
    [Name("currency")] public string Currency { get; set; } = "";
    [Name("due_date")] public DateTime DueDate { get; set; }
    [Name("status")] public string Status { get; set; } = "";
}

public class Fee
{
    [Name("fee_id")] public string FeeId { get; set; } = "";
    [Name("allocation_id")] public string AllocationId { get; set; } = "";
    [Name("investor_id")] public string InvestorId { get; set; } = "";
    [Name("deal_id")] public string DealId { get; set; } = "";
    [Name("fee_type")] public string FeeType { get; set; } = "";
    [Name("period")] public string Period { get; set; } = "";
    [Name("fee_rate_pct")] public decimal? FeeRatePct { get; set; }
    [Name("basis")] public string Basis { get; set; } = "";
    [Name("amount")] public decimal Amount { get; set; }
    [Name("currency")] public string Currency { get; set; } = "";
    [Name("due_date")] public DateTime DueDate { get; set; }
    [Name("status")] public string Status { get; set; } = "";
}

public class Distribution
{
    [Name("distribution_id")] public string DistributionId { get; set; } = "";
    [Name("deal_id")] public string DealId { get; set; } = "";
    [Name("allocation_id")] public string AllocationId { get; set; } = "";
    [Name("investor_id")] public string InvestorId { get; set; } = "";
    [Name("distribution_date")] public DateTime DistributionDate { get; set; }
    [Name("distribution_type")] public string DistributionType { get; set; } = "";
    [Name("gross_amount")] public decimal GrossAmount { get; set; }
    [Name("performance_fee_pct")] public decimal PerformanceFeePct { get; set; }
    [Name("performance_fee_amount")] public decimal PerformanceFeeAmount { get; set; }
    [Name("net_amount")] public decimal NetAmount { get; set; }
    [Name("currency")] public string Currency { get; set; } = "";
    [Name("fraction_of_units")] public decimal FractionOfUnits { get; set; }
}

public class StatementLine
{
    [Name("line_id")] public string LineId { get; set; } = "";
    [Name("investor_id")] public string InvestorId { get; set; } = "";
    [Name("date")] public DateTime Date { get; set; }
    [Name("type")] public string Type { get; set; } = "";
    [Name("deal_id")] public string DealId { get; set; } = "";
    [Name("amount")] public decimal Amount { get; set; }
    [Name("currency")] public string Currency { get; set; } = "";
    [Name("reference_id")] public string ReferenceId { get; set; } = "";
}

public class FxRate
{
    [Name("currency")] public string Currency { get; set; } = "";
    [Name("to_usd")] public decimal ToUsd { get; set; }
    [Name("as_of")] public DateTime AsOf { get; set; }
}

// ─── Computed / response models ───────────────────────────────────────────────

public record InvestorSummaryDto(
    string InvestorId,
    string InvestorName,
    string InvestorType,
    string Country,
    string ReportingCurrency,
    string TechSavviness,
    int? Age,
    string KycStatus,
    PortfolioSignals Signals
);

public record ChatRequest(
    string InvestorId,
    List<ChatMessage> Messages
);

public record ChatMessage(
    string Role,
    string Content
);

public record ChatResponse(string Reply);

public record InvestorContext(
    Investor Investor,
    IReadOnlyList<PositionContext> Positions,
    PortfolioTotals Totals,
    IReadOnlyList<Fee> UpcomingFees,
    IReadOnlyList<Fee> OverdueFees,
    IReadOnlyList<CapitalCall> UpcomingCalls,
    IReadOnlyList<Distribution> Distributions,
    IReadOnlyDictionary<string, decimal> SectorExposureUsd,
    IReadOnlyList<StatementLine> StatementLines,  // account statement — question type #7
    IReadOnlyList<FxRate> FxRates                 // live rates for PromptBuilder
);

public record PositionContext(
    PortfolioCompany Company,
    IReadOnlyList<RoundContext> Rounds
);

public record RoundContext(
    Allocation Allocation,
    Deal Deal,
    decimal CurrentSharePrice,
    decimal CurrentValueDealCcy,
    decimal CurrentValueUsd,
    decimal? Moic,
    IReadOnlyList<Valuation> ValuationHistory
);

public record PortfolioTotals(
    decimal TotalCommittedUsd,
    decimal TotalContributedUsd,
    decimal TotalCurrentValueUsd,
    decimal? PortfolioMoic,
    decimal TotalFeesPaidUsd,
    decimal TotalDistributedUsd
);

public record PortfolioSignals(
    int OverdueFeeCount,
    int UpcomingCallCount,
    int UpcomingFeeCount,
    int DistributionCount,
    int PositionCount,
    string? TopSector,
    bool HasExitedDeals,
    bool HasWrittenOffDeals,
    string? FirstCompanyName
);