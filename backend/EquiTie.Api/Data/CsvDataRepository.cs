using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace EquiTie.Api.Data;

public class CsvDataRepository 
{
    public IReadOnlyList<Investor> Investors { get; }
    public IReadOnlyList<PortfolioCompany> Companies { get; }
    public IReadOnlyList<Deal> Deals { get; }
    public IReadOnlyList<Allocation> Allocations { get; }
    public IReadOnlyList<Valuation> Valuations { get; }
    public IReadOnlyList<CapitalCall> CapitalCalls { get; }
    public IReadOnlyList<Fee> Fees { get; }
    public IReadOnlyList<Distribution> Distributions { get; }
    public IReadOnlyList<StatementLine> StatementLines { get; }
    public IReadOnlyList<FxRate> FxRates { get; }

    private readonly Dictionary<string, Investor> _investorMap;
    private readonly Dictionary<string, Deal> _dealMap;
    private readonly Dictionary<string, PortfolioCompany> _companyMap;
    private readonly Dictionary<string, decimal> _fxMap;      // currency --- USD rate
    private readonly Dictionary<string, decimal> _latestPrices; // dealId --- latest share price

    public CsvDataRepository(IWebHostEnvironment env)
    {
        var csvDir = Path.Combine(env.ContentRootPath, "Data", "csv");
        Investors = Load<Investor>(Path.Combine(csvDir, "investors.csv"));
        Companies = Load<PortfolioCompany>(Path.Combine(csvDir, "portfolio_companies.csv"));
        Deals = Load<Deal>(Path.Combine(csvDir, "deals.csv"));

        Allocations = Load<Allocation>(Path.Combine(csvDir, "allocations.csv"));
        Valuations = Load<Valuation>(Path.Combine(csvDir, "valuations.csv"));
        CapitalCalls = Load<CapitalCall>(Path.Combine(csvDir, "capital_calls.csv"));
        Fees = Load<Fee>(Path.Combine(csvDir, "fees.csv"));
        Distributions = Load<Distribution>(Path.Combine(csvDir, "distributions.csv"));
        StatementLines = Load<StatementLine>(Path.Combine(csvDir, "statement_lines.csv"));
        FxRates = Load<FxRate>(Path.Combine(csvDir, "fx_rates.csv"));

        _investorMap = Investors.ToDictionary(i => i.InvestorId);
        _dealMap = Deals.ToDictionary(d => d.DealId);
        _companyMap = Companies.ToDictionary(c => c.CompanyId);
        _fxMap = FxRates.ToDictionary(f => f.Currency, f => f.ToUsd);

        _latestPrices = Valuations
            .GroupBy(v => v.DealId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(v => v.ValuationDate).First().SharePrice
            );
    }

    public Investor? GetInvestor(string id) => _investorMap.GetValueOrDefault(id);
    public Deal? GetDeal(string id) => _dealMap.GetValueOrDefault(id);
    public PortfolioCompany? GetCompany(string id) => _companyMap.GetValueOrDefault(id);

    public decimal ToUsd(decimal amount, string currency)
        => amount * (_fxMap.GetValueOrDefault(currency, 1m));

    public decimal CurrentSharePrice(string dealId)
        => _latestPrices.GetValueOrDefault(dealId, 0m);

    private static IReadOnlyList<TRecord> Load<TRecord>(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"CSV not found: {path}");

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            PrepareHeaderForMatch = args => args.Header.Replace("_", ""),
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, config);

        return csv.GetRecords<TRecord>().ToList().AsReadOnly();
    }
}


public class InvestorCsvMap : ClassMap<Investor>
{
    public InvestorCsvMap()
    {
        Map(m => m.InvestorId).Name("investor_id");
        Map(m => m.InvestorName).Name("investor_name");
        Map(m => m.InvestorType).Name("investor_type");
        Map(m => m.Country).Name("country");
        Map(m => m.ReportingCurrency).Name("reporting_currency");
        Map(m => m.Age).Name("age").TypeConverterOption.NullValues("", " ");
        Map(m => m.TechSavviness).Name("tech_savviness");
        Map(m => m.KycStatus).Name("kyc_status");
        Map(m => m.OnboardedDate).Name("onboarded_date");
        Map(m => m.Email).Name("email");
    }
}

public class CompanyCsvMap : ClassMap<PortfolioCompany>
{
    public CompanyCsvMap()
    {
        Map(m => m.CompanyId).Name("company_id");
        Map(m => m.CompanyName).Name("company_name");
        Map(m => m.Sector).Name("sector");
        Map(m => m.HqCountry).Name("hq_country");
        Map(m => m.Status).Name("status");
        Map(m => m.Website).Name("website");
    }
}

public class DealCsvMap : ClassMap<Deal>
{
    public DealCsvMap()
    {
        Map(m => m.DealId).Name("deal_id");
        Map(m => m.CompanyId).Name("company_id");
        Map(m => m.CompanyName).Name("company_name");
        Map(m => m.Round).Name("round");
        Map(m => m.Instrument).Name("instrument");
        Map(m => m.SpvName).Name("spv_name");
        Map(m => m.DealCurrency).Name("deal_currency");
        Map(m => m.DealDate).Name("deal_date");
        Map(m => m.PreMoneyValuationM).Name("pre_money_valuation_m");
        Map(m => m.PostMoneyValuationM).Name("post_money_valuation_m");
        Map(m => m.RoundSizeM).Name("round_size_m");
        Map(m => m.EquitieAllocationM).Name("equitie_allocation_m");
        Map(m => m.EntrySharePrice).Name("entry_share_price");
        Map(m => m.ContributedPct).Name("contributed_pct");
        Map(m => m.StdMgmtFeePct).Name("std_mgmt_fee_pct");
        Map(m => m.StdPerformanceFeePct).Name("std_performance_fee_pct");
        Map(m => m.StdStructuringFeePct).Name("std_structuring_fee_pct");
        Map(m => m.StdAdminFeeUsd).Name("std_admin_fee_usd");
        Map(m => m.Status).Name("status");
    }
}

public class AllocationCsvMap : ClassMap<Allocation>
{
    public AllocationCsvMap()
    {
        Map(m => m.AllocationId).Name("allocation_id");
        Map(m => m.DealId).Name("deal_id");
        Map(m => m.InvestorId).Name("investor_id");
        Map(m => m.DealCurrency).Name("deal_currency");
        Map(m => m.CommitmentAmount).Name("commitment_amount");
        Map(m => m.PriceDiscountPct).Name("price_discount_pct");
        Map(m => m.EffectiveSharePrice).Name("effective_share_price");
        Map(m => m.Units).Name("units");
        Map(m => m.ContributedAmount).Name("contributed_amount");
        Map(m => m.OutstandingCommitment).Name("outstanding_commitment");
        Map(m => m.MgmtFeePct).Name("mgmt_fee_pct");
        Map(m => m.PerformanceFeePct).Name("performance_fee_pct");
        Map(m => m.StructuringFeePct).Name("structuring_fee_pct");
        Map(m => m.AdminFeeUsd).Name("admin_fee_usd");
        Map(m => m.FeeDiscount).Name("fee_discount");
        Map(m => m.AllocationStatus).Name("allocation_status");
        Map(m => m.AllocationDate).Name("allocation_date");
    }
}

public class ValuationCsvMap : ClassMap<Valuation>
{
    public ValuationCsvMap()
    {
        Map(m => m.ValuationId).Name("valuation_id");
        Map(m => m.DealId).Name("deal_id");
        Map(m => m.ValuationDate).Name("valuation_date");
        Map(m => m.SharePrice).Name("share_price");
        Map(m => m.CompanyValuationM).Name("company_valuation_m");
        Map(m => m.MarkSource).Name("mark_source");
        Map(m => m.MultipleVsEntry).Name("multiple_vs_entry");
    }
}

public class CapitalCallCsvMap : ClassMap<CapitalCall>
{
    public CapitalCallCsvMap()
    {
        Map(m => m.CallId).Name("call_id");
        Map(m => m.AllocationId).Name("allocation_id");
        Map(m => m.InvestorId).Name("investor_id");
        Map(m => m.DealId).Name("deal_id");
        Map(m => m.CallNumber).Name("call_number");
        Map(m => m.CallDate).Name("call_date");
        Map(m => m.Amount).Name("amount");
        Map(m => m.Currency).Name("currency");
        Map(m => m.DueDate).Name("due_date");
        Map(m => m.Status).Name("status");
    }
}

public class FeeCsvMap : ClassMap<Fee>
{
    public FeeCsvMap()
    {
        Map(m => m.FeeId).Name("fee_id");
        Map(m => m.AllocationId).Name("allocation_id");
        Map(m => m.InvestorId).Name("investor_id");
        Map(m => m.DealId).Name("deal_id");
        Map(m => m.FeeType).Name("fee_type");
        Map(m => m.Period).Name("period");
        Map(m => m.FeeRatePct).Name("fee_rate_pct").TypeConverterOption.NullValues("", " ");
        Map(m => m.Basis).Name("basis");
        Map(m => m.Amount).Name("amount");
        Map(m => m.Currency).Name("currency");
        Map(m => m.DueDate).Name("due_date");
        Map(m => m.Status).Name("status");
    }
}

public class DistributionCsvMap : ClassMap<Distribution>
{
    public DistributionCsvMap()
    {
        Map(m => m.DistributionId).Name("distribution_id");
        Map(m => m.DealId).Name("deal_id");
        Map(m => m.AllocationId).Name("allocation_id");
        Map(m => m.InvestorId).Name("investor_id");
        Map(m => m.DistributionDate).Name("distribution_date");
        Map(m => m.DistributionType).Name("distribution_type");
        Map(m => m.GrossAmount).Name("gross_amount");
        Map(m => m.PerformanceFeePct).Name("performance_fee_pct");
        Map(m => m.PerformanceFeeAmount).Name("performance_fee_amount");
        Map(m => m.NetAmount).Name("net_amount");
        Map(m => m.Currency).Name("currency");
        Map(m => m.FractionOfUnits).Name("fraction_of_units");
    }
}

public class StatementLineCsvMap : ClassMap<StatementLine>
{
    public StatementLineCsvMap()
    {
        Map(m => m.LineId).Name("line_id");
        Map(m => m.InvestorId).Name("investor_id");
        Map(m => m.Date).Name("date");
        Map(m => m.Type).Name("type");
        Map(m => m.DealId).Name("deal_id");
        Map(m => m.Amount).Name("amount");
        Map(m => m.Currency).Name("currency");
        Map(m => m.ReferenceId).Name("reference_id");
    }
}

public class FxRateCsvMap : ClassMap<FxRate>
{
    public FxRateCsvMap()
    {
        Map(m => m.Currency).Name("currency");
        Map(m => m.ToUsd).Name("to_usd");
        Map(m => m.AsOf).Name("as_of");
    }
}
