using EquiTie.Api.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EquiTie.Api;

public class PortfolioService(CsvDataRepository repo)
{
    public IReadOnlyList<InvestorSummaryDto> GetAllInvestors() =>
        repo.Investors
            .Select(i => new InvestorSummaryDto(
                i.InvestorId, i.InvestorName, i.InvestorType,
                i.Country, i.ReportingCurrency, i.TechSavviness, i.Age,
                i.KycStatus, BuildSignals(i.InvestorId)))
            .ToList()
            .AsReadOnly();

    private PortfolioSignals BuildSignals(string investorId)
    {
        var allocs = repo.Allocations.Where(a => a.InvestorId == investorId).ToList();
        var fees = repo.Fees.Where(f => f.InvestorId == investorId).ToList();
        var calls = repo.CapitalCalls.Where(c => c.InvestorId == investorId).ToList();
        var dists = repo.Distributions.Where(d => d.InvestorId == investorId).ToList();

        var topSector = allocs
            .GroupBy(a => repo.GetDeal(a.DealId)?.CompanyId ?? "")
            .Select(g => repo.GetCompany(g.Key)?.Sector)
            .Where(s => s is not null)
            .GroupBy(s => s!)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key;

        var firstCompany = allocs
            .Select(a => repo.GetDeal(a.DealId))
            .Where(d => d is not null)
            .Select(d => d!.CompanyName)
            .FirstOrDefault();

        var hasExited = allocs.Any(a => a.AllocationStatus == "Exited"
                                         || repo.GetDeal(a.DealId)?.Status == "Exited");
        var hasWrittenOff = allocs.Any(a => repo.GetDeal(a.DealId)?.Status == "Written Off");

        return new PortfolioSignals(
            OverdueFeeCount: fees.Count(f => f.Status == "Overdue"),
            UpcomingCallCount: calls.Count(c => c.Status == "Upcoming"),
            UpcomingFeeCount: fees.Count(f => f.Status == "Upcoming"),
            DistributionCount: dists.Count,
            PositionCount: allocs.Select(a => repo.GetDeal(a.DealId)?.CompanyId).Distinct().Count(),
            TopSector: topSector,
            HasExitedDeals: hasExited,
            HasWrittenOffDeals: hasWrittenOff,
            FirstCompanyName: firstCompany
        );
    }

    public InvestorContext? BuildContext(string investorId)
    {
        var investor = repo.GetInvestor(investorId);
        if (investor is null) return null;

        var allocations = repo.Allocations.Where(a => a.InvestorId == investorId).ToList();
        var fees = repo.Fees.Where(f => f.InvestorId == investorId).ToList();
        var calls = repo.CapitalCalls.Where(c => c.InvestorId == investorId).ToList();
        var distributions = repo.Distributions.Where(d => d.InvestorId == investorId).ToList();
        var statementLines = repo.StatementLines
                                 .Where(s => s.InvestorId == investorId)
                                 .OrderBy(s => s.Date)
                                 .ToList()
                                 .AsReadOnly();

        var positionsByCompany = allocations
            .GroupBy(a => repo.GetDeal(a.DealId)?.CompanyId ?? "UNKNOWN")
            .Select(grp =>
            {
                var company = repo.GetCompany(grp.Key);
                var rounds = grp
                    .Select(BuildRoundContext)
                    .Where(r => r is not null)
                    .Select(r => r!)
                    .OrderBy(r => r.Deal.DealDate)
                    .ToList()
                    .AsReadOnly();

                return new PositionContext(company ?? UnknownCompany(grp.Key), rounds);
            })
            .ToList()
            .AsReadOnly();

        var allRounds = positionsByCompany.SelectMany(p => p.Rounds).ToList();
        var totalCommittedUsd = allRounds.Sum(r => repo.ToUsd(r.Allocation.CommitmentAmount, r.Allocation.DealCurrency));
        var totalContributedUsd = allRounds.Sum(r => repo.ToUsd(r.Allocation.ContributedAmount, r.Allocation.DealCurrency));
        var totalCurrentValueUsd = allRounds.Sum(r => r.CurrentValueUsd);
        decimal? portfolioMoic = totalContributedUsd > 0 ? totalCurrentValueUsd / totalContributedUsd : null;

        var totalFeesPaidUsd = fees.Where(f => f.Status == "Paid").Sum(f => repo.ToUsd(f.Amount, f.Currency));
        var totalDistributedUsd = distributions.Sum(d => repo.ToUsd(d.NetAmount, d.Currency));

        var totals = new PortfolioTotals(
            totalCommittedUsd, totalContributedUsd, totalCurrentValueUsd,
            portfolioMoic, totalFeesPaidUsd, totalDistributedUsd);

        var sectorExposure = positionsByCompany
            .GroupBy(p => p.Company.Sector)
            .ToDictionary(
                g => g.Key,
                g => g.SelectMany(p => p.Rounds).Sum(r => r.CurrentValueUsd));

        return new InvestorContext(
            investor,
            positionsByCompany,
            totals,
            fees.Where(f => f.Status == "Upcoming").ToList().AsReadOnly(),
            fees.Where(f => f.Status == "Overdue").ToList().AsReadOnly(),
            calls.Where(c => c.Status == "Upcoming").ToList().AsReadOnly(),
            distributions,
            sectorExposure,
            statementLines,   // arg 9  — closes question type #7 (account statement)
            repo.FxRates      // arg 10 — live rates so PromptBuilder never hardcodes FX
        );
    }

    private RoundContext? BuildRoundContext(Allocation alloc)
    {
        var deal = repo.GetDeal(alloc.DealId);
        if (deal is null) return null;

        var currentPrice = repo.CurrentSharePrice(alloc.DealId);
        var currentValueDealCcy = alloc.Units * currentPrice;
        var currentValueUsd = repo.ToUsd(currentValueDealCcy, alloc.DealCurrency);
        decimal? moic = alloc.ContributedAmount > 0
                                      ? currentValueDealCcy / alloc.ContributedAmount
                                      : null;

        var history = repo.Valuations
            .Where(v => v.DealId == alloc.DealId)
            .OrderBy(v => v.ValuationDate)
            .ToList()
            .AsReadOnly();

        return new RoundContext(alloc, deal, currentPrice, currentValueDealCcy, currentValueUsd, moic, history);
    }

    private static PortfolioCompany UnknownCompany(string id) => new()
    {
        CompanyId = id,
        CompanyName = "Unknown",
        Sector = "Unknown",
        HqCountry = "Unknown",
        Status = "Unknown",
        Website = "",
    };
}

public class AnthropicChatService(IHttpClientFactory httpFactory, IConfiguration config, ILogger<AnthropicChatService> logger)
{
    private const string Model = "claude-sonnet-4-5";
    private const int MaxTokens = 1024;
    private const string ApiVersion = "2023-06-01";

    public async Task<string> SendAsync(string systemPrompt, IEnumerable<ChatMessage> history, CancellationToken ct = default)
    {
        var apiKey = config["Anthropic:ApiKey"]
            ?? throw new InvalidOperationException("Anthropic:ApiKey is not configured.");

        var body = new AnthropicRequest
        {
            Model = Model,
            MaxTokens = MaxTokens,
            System = systemPrompt,
            Messages = history.Select(m => new AnthropicMessage(m.Role, m.Content)).ToList(),
        };

        var json = JsonSerializer.Serialize(body, JsonOptions.Default);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var client = httpFactory.CreateClient("anthropic");
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", ApiVersion);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        logger.LogInformation("Sending chat request to Anthropic ({MessageCount} messages)", body.Messages.Count);

        var response = await client.PostAsync("https://api.anthropic.com/v1/messages", content, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Anthropic API error {Status}: {Body}", response.StatusCode, raw);
            throw new HttpRequestException($"Anthropic API returned {response.StatusCode}: {raw}");
        }

        var result = JsonSerializer.Deserialize<AnthropicResponse>(raw, JsonOptions.Default)
            ?? throw new InvalidOperationException("Empty response from Anthropic API.");

        return result.Content.FirstOrDefault(c => c.Type == "text")?.Text
            ?? throw new InvalidOperationException("No text block in Anthropic response.");
    }
}

file record AnthropicRequest
{
    [JsonPropertyName("model")]
    public string Model { get; init; } = "";
    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; init; }
    [JsonPropertyName("system")]
    public string System { get; init; } = "";
    [JsonPropertyName("messages")]
    public List<AnthropicMessage> Messages { get; init; } = [];
}

file record AnthropicMessage(
    [property: JsonPropertyName("role")]
    string Role,
    [property: JsonPropertyName("content")]
    string Content
);

file record AnthropicResponse
{
    [JsonPropertyName("content")] public List<ContentBlock> Content { get; init; } = [];
}

file record ContentBlock
{
    [JsonPropertyName("type")] public string Type { get; init; } = "";
    [JsonPropertyName("text")] public string Text { get; init; } = "";
}

file static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
public class PromptBuilder
{
    private static readonly DateOnly ReportDate = new(2026, 6, 25);

    public string Build(InvestorContext ctx)
    {
        var sb = new StringBuilder();
        var inv = ctx.Investor;

        // FIX: FX rates read from loaded CSV data, not hardcoded literals
        var fxLines = ctx.FxRates
            .Where(f => f.Currency != "USD")
            .Select(f => $"{f.Currency}={f.ToUsd} to USD")
            .ToList();
        var fxSummary = fxLines.Count > 0
            ? string.Join(", ", fxLines) + " (as of report date)"
            : "USD baseline only";

        sb.AppendLine("You are the EquiTie Investor Assistant — a knowledgeable, trustworthy assistant for EquiTie investors.");
        sb.AppendLine("You answer questions about the authenticated investor's own portfolio only.");
        sb.AppendLine("You must NEVER reveal or reference data belonging to other investors.");
        sb.AppendLine($"The report date is {ReportDate:dd MMMM yyyy}.");
        sb.AppendLine();
        sb.AppendLine("ACCURACY RULES:");
        sb.AppendLine("- Every number you quote must come from the data below. Never invent or estimate.");
        sb.AppendLine("- Always cite the source (deal ID, allocation ID, or statement line ID) when quoting a specific figure.");
        sb.AppendLine("- If you are uncertain, say so. Do not guess.");
        sb.AppendLine($"- FX rates in use: {fxSummary}");
        sb.AppendLine("- NEVER give investment advice or recommendations. If asked, explain you can only provide factual portfolio data.");
        sb.AppendLine("- Do NOT append a 'Sources:' line or list source files. The application displays a deterministic source citation beneath each answer, so you must not produce one yourself.");
        sb.AppendLine();

        sb.AppendLine(BuildPersonalisation(inv));
        sb.AppendLine();

        // ── Investor profile ─────────────────────────────────────────────────
        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine("INVESTOR PROFILE");
        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine($"Name:                {inv.InvestorName}");
        sb.AppendLine($"Type:                {inv.InvestorType}");
        sb.AppendLine($"Country:             {inv.Country}");
        sb.AppendLine($"Reporting currency:  {inv.ReportingCurrency}");
        sb.AppendLine($"KYC status:          {inv.KycStatus}");
        if (inv.Age.HasValue) sb.AppendLine($"Age:                 {inv.Age}");
        sb.AppendLine();

        // ── Portfolio summary ─────────────────────────────────────────────────
        var t = ctx.Totals;
        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine("PORTFOLIO SUMMARY (all values USD)");
        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine($"Total committed:       {Usd(t.TotalCommittedUsd)}");
        sb.AppendLine($"Total contributed:     {Usd(t.TotalContributedUsd)}");
        sb.AppendLine($"Total current value:   {Usd(t.TotalCurrentValueUsd)}");
        sb.AppendLine($"Portfolio MOIC:        {Moic(t.PortfolioMoic)}");
        sb.AppendLine($"Total fees paid:       {Usd(t.TotalFeesPaidUsd)}");
        sb.AppendLine($"Total distributions:   {Usd(t.TotalDistributedUsd)} (net of carry)");
        sb.AppendLine();

        // ── Holdings ─────────────────────────────────────────────────────────
        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine("HOLDINGS");
        sb.AppendLine("═══════════════════════════════════════");
        foreach (var pos in ctx.Positions)
        {
            sb.AppendLine($"Company: {pos.Company.CompanyName} | Sector: {pos.Company.Sector} | Status: {pos.Company.Status}");
            foreach (var r in pos.Rounds)
            {
                var a = r.Allocation;
                var d = r.Deal;
                sb.AppendLine($"  Round: {d.Round} (deal {d.DealId})");
                sb.AppendLine($"    Allocation:    {a.AllocationId}");
                sb.AppendLine($"    Currency:      {a.DealCurrency}");
                sb.AppendLine($"    Entry price:   {a.DealCurrency} {a.EffectiveSharePrice:N4}{(a.PriceDiscountPct > 0 ? $" ({a.PriceDiscountPct}% discount from standard {d.EntrySharePrice:N4})" : "")}");
                sb.AppendLine($"    Units held:    {a.Units:N4}");
                sb.AppendLine($"    Committed:     {a.DealCurrency} {a.CommitmentAmount:N2}");
                sb.AppendLine($"    Contributed:   {a.DealCurrency} {a.ContributedAmount:N2}");
                if (a.OutstandingCommitment > 0)
                    sb.AppendLine($"    Outstanding:   {a.DealCurrency} {a.OutstandingCommitment:N2}");
                sb.AppendLine($"    Current price: {a.DealCurrency} {r.CurrentSharePrice:N4}");
                sb.AppendLine($"    Current value: {a.DealCurrency} {r.CurrentValueDealCcy:N2} (USD {r.CurrentValueUsd:N2})");
                sb.AppendLine($"    MOIC:          {Moic(r.Moic)}");
                sb.AppendLine($"    Fees:          mgmt {a.MgmtFeePct}% | perf {a.PerformanceFeePct}% | struct {a.StructuringFeePct}% | admin USD {a.AdminFeeUsd}{(a.FeeDiscount == "Yes" ? " [FEE DISCOUNT APPLIED]" : "")}");
                sb.AppendLine($"    Status:        {a.AllocationStatus}");

                if (r.ValuationHistory.Count > 0)
                {
                    sb.AppendLine($"    Valuation history:");
                    foreach (var v in r.ValuationHistory)
                        sb.AppendLine($"      {v.ValuationDate:yyyy-MM-dd}  price {a.DealCurrency} {v.SharePrice:N4}  co.val {v.CompanyValuationM:N1}M  source: {v.MarkSource}  {v.MultipleVsEntry:N2}x vs entry");
                }
            }
            sb.AppendLine();
        }

        // ── Sector exposure ───────────────────────────────────────────────────
        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine("SECTOR EXPOSURE (current value, USD)");
        sb.AppendLine("═══════════════════════════════════════");
        var totalVal = ctx.SectorExposureUsd.Values.Sum();
        foreach (var (sector, valueUsd) in ctx.SectorExposureUsd.OrderByDescending(x => x.Value))
        {
            var pct = totalVal > 0 ? valueUsd / totalVal * 100 : 0;
            sb.AppendLine($"  {sector,-35} {Usd(valueUsd)}  ({pct:N1}% of portfolio)");
        }
        sb.AppendLine();

        // ── Upcoming fees ─────────────────────────────────────────────────────
        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine("UPCOMING FEES (due after report date)");
        sb.AppendLine("═══════════════════════════════════════");
        if (!ctx.UpcomingFees.Any())
            sb.AppendLine("  None.");
        else
            foreach (var f in ctx.UpcomingFees)
                sb.AppendLine($"  {f.FeeId}  {f.FeeType,-20}  deal {f.DealId}  {f.Currency} {f.Amount:N2}  due {f.DueDate:yyyy-MM-dd}");
        sb.AppendLine();

        // ── Overdue fees ──────────────────────────────────────────────────────
        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine("OVERDUE FEES");
        sb.AppendLine("═══════════════════════════════════════");
        if (!ctx.OverdueFees.Any())
            sb.AppendLine("  None.");
        else
            foreach (var f in ctx.OverdueFees)
                sb.AppendLine($"  OVERDUE {f.FeeId}  {f.FeeType,-20}  deal {f.DealId}  {f.Currency} {f.Amount:N2}  was due {f.DueDate:yyyy-MM-dd}");
        sb.AppendLine();

        // ── Upcoming capital calls ────────────────────────────────────────────
        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine("UPCOMING CAPITAL CALLS");
        sb.AppendLine("═══════════════════════════════════════");
        if (!ctx.UpcomingCalls.Any())
            sb.AppendLine("  None.");
        else
            foreach (var c in ctx.UpcomingCalls)
                sb.AppendLine($"  {c.CallId}  deal {c.DealId}  call #{c.CallNumber}  {c.Currency} {c.Amount:N2}  due {c.DueDate:yyyy-MM-dd}");
        sb.AppendLine();

        // ── Distributions ─────────────────────────────────────────────────────
        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine("DISTRIBUTIONS RECEIVED");
        sb.AppendLine("═══════════════════════════════════════");
        if (!ctx.Distributions.Any())
            sb.AppendLine("  None.");
        else
            foreach (var d in ctx.Distributions)
            {
                sb.AppendLine($"  {d.DistributionId}  {d.DistributionDate:yyyy-MM-dd}  {d.DistributionType}");
                sb.AppendLine($"    Deal {d.DealId}  Gross: {d.Currency} {d.GrossAmount:N2}  Carry ({d.PerformanceFeePct}%): {d.Currency} {d.PerformanceFeeAmount:N2}  Net received: {d.Currency} {d.NetAmount:N2}");
            }
        sb.AppendLine();

        // FIX: ACCOUNT STATEMENT section — was entirely missing, closes question type #7
        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine("ACCOUNT STATEMENT (chronological ledger)");
        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine("Note: negative amounts = money paid out by investor; positive = money received.");
        if (!ctx.StatementLines.Any())
        {
            sb.AppendLine("  No statement lines on record.");
        }
        else
        {
            // Group by type for summary
            var contributions = ctx.StatementLines.Where(s => s.Type == "Capital Contribution").ToList();
            var feeLines = ctx.StatementLines.Where(s => s.Type.Contains("Fee")).ToList();
            var distLines = ctx.StatementLines.Where(s => s.Type == "Distribution").ToList();

            sb.AppendLine($"  Total lines: {ctx.StatementLines.Count}");
            sb.AppendLine($"  Capital contributions: {contributions.Count} lines");
            sb.AppendLine($"  Fee charges:           {feeLines.Count} lines");
            sb.AppendLine($"  Distributions:         {distLines.Count} lines");
            sb.AppendLine();
            sb.AppendLine("  Chronological detail:");
            foreach (var s in ctx.StatementLines)
                sb.AppendLine($"  {s.LineId}  {s.Date:yyyy-MM-dd}  {s.Type,-25}  deal {s.DealId,-8}  {s.Currency} {s.Amount:N2}  ref {s.ReferenceId}");
        }

        return sb.ToString();
    }

    private static string BuildPersonalisation(Investor inv)
    {
        var sb = new StringBuilder();
        sb.AppendLine("PERSONALISATION INSTRUCTIONS:");

        switch (inv.TechSavviness)
        {
            case "Low":
                sb.AppendLine($"- {inv.InvestorName} has low tech/finance savviness{(inv.Age.HasValue ? $" and is {inv.Age} years old" : "")}.");
                sb.AppendLine("- Use plain, friendly language. Always explain jargon on first use:");
                sb.AppendLine("    MOIC = Multiple on Invested Capital — how many times they've multiplied their money");
                sb.AppendLine("    Carry / performance fee = the firm's share of profits");
                sb.AppendLine("    Capital call = a request to send a portion of committed funds");
                sb.AppendLine("    Committed vs contributed = total pledged vs amount actually sent so far");
                sb.AppendLine("    SPV = Special Purpose Vehicle, the legal entity holding the investment");
                sb.AppendLine("- Keep answers short and warm. Avoid acronyms unless explained. Use analogies.");
                sb.AppendLine("- Never be condescending. Assume they are intelligent but not finance professionals.");
                break;

            case "Medium":
                sb.AppendLine($"- {inv.InvestorName} has medium finance savviness{(inv.Age.HasValue ? $", age {inv.Age}" : "")}.");
                sb.AppendLine("- Use clear language. Briefly define VC-specific terms on first use.");
                sb.AppendLine("- Answers can be moderately detailed with supporting numbers.");
                break;

            default:
                sb.AppendLine($"- {inv.InvestorName} is a sophisticated investor with high finance savviness{(inv.Age.HasValue ? $", age {inv.Age}" : "")}.");
                sb.AppendLine("- Be concise and data-dense. Assume full fluency with MOIC, carry, SPVs, DPI, TVPI, capital calls.");
                sb.AppendLine("- Skip jargon definitions unless asked. Lead with the number, follow with context.");
                break;
        }

        return sb.ToString();
    }

    private static string Usd(decimal v) => $"${v:N2}";
    private static string Moic(decimal? m) => m.HasValue ? $"{m.Value:N2}x" : "N/A";
}