using Microsoft.AspNetCore.Mvc;

namespace EquiTie.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvestorsController(PortfolioService portfolio) : ControllerBase
{
    /// <summary>Returns the list of investors for the investor-switcher dropdown.</summary>
    [HttpGet]
    public IActionResult GetAll() => Ok(portfolio.GetAllInvestors());

    /// <summary>
    /// Returns the headline portfolio figures for the snapshot bar
    /// (current value, MOIC, committed, contributed). Computed deterministically
    /// server-side from the same context the chat assistant uses.
    /// </summary>
    [HttpGet("{investorId}/overview")]
    public IActionResult GetOverview(string investorId)
    {
        var ctx = portfolio.BuildContext(investorId);
        if (ctx is null)
            return NotFound($"Investor '{investorId}' not found.");

        return Ok(new
        {
            currentValueUsd = ctx.Totals.TotalCurrentValueUsd,
            portfolioMoic = ctx.Totals.PortfolioMoic,
            committedUsd = ctx.Totals.TotalCommittedUsd,
            contributedUsd = ctx.Totals.TotalContributedUsd,
            feesPaidUsd = ctx.Totals.TotalFeesPaidUsd,
            upcomingCallCount = ctx.UpcomingCalls.Count,
            // Deterministic source map: which CSV files this investor's answers
            // are derived from, based on what data they actually have. Computed
            // in code, never by the model — this is the verification guarantee.
            sources = BuildSourceMap(ctx),
        });
    }

    /// <summary>
    /// Returns the set of source CSV files that back this investor's data,
    /// derived deterministically from what the investor actually holds.
    /// The UI renders this as a persistent "Sources" footer so the provenance
    /// of every figure is always visible and always correct.
    /// </summary>
    private static IReadOnlyList<string> BuildSourceMap(InvestorContext ctx)
    {
        var files = new List<string>
        {
            // Always present: every investor has holdings valued in USD
            "allocations.csv",
            "valuations.csv",
            "fx_rates.csv",
            "deals.csv",
            "portfolio_companies.csv",
        };

        if (ctx.UpcomingFees.Count > 0 || ctx.OverdueFees.Count > 0 || ctx.Totals.TotalFeesPaidUsd > 0)
            files.Add("fees.csv");
        if (ctx.UpcomingCalls.Count > 0)
            files.Add("capital_calls.csv");
        if (ctx.Distributions.Count > 0)
            files.Add("distributions.csv");
        if (ctx.StatementLines.Count > 0)
            files.Add("statement_lines.csv");

        return files;
    }
}

[ApiController]
[Route("api/[controller]")]
public class ChatController(
    PortfolioService portfolio,
    PromptBuilder promptBuilder,
    AnthropicChatService chatService,
    ILogger<ChatController> logger) : ControllerBase
{
    /// <summary>
    /// Accepts a conversation history for a given investor and returns the
    /// assistant's next reply. Context isolation is enforced server-side:
    /// the system prompt contains only the authenticated investor's data.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.InvestorId))
            return BadRequest("investor_id is required.");

        if (request.Messages is null || request.Messages.Count == 0)
            return BadRequest("At least one message is required.");

        var ctx = portfolio.BuildContext(request.InvestorId);
        if (ctx is null)
            return NotFound($"Investor '{request.InvestorId}' not found.");

        logger.LogInformation("Chat request for investor {InvestorId} ({TechLevel}, {MessageCount} messages)",
            request.InvestorId, ctx.Investor.TechSavviness, request.Messages.Count);

        var systemPrompt = promptBuilder.Build(ctx);
        var reply = await chatService.SendAsync(systemPrompt, request.Messages, ct);

        return Ok(new ChatResponse(reply));
    }
}