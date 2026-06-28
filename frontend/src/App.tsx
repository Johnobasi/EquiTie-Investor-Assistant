import { useState, useEffect, useRef, useCallback, useMemo } from "react";
import type { InvestorSummary, ChatMessage } from "./types/api";
import { fetchInvestors, sendChat } from "./hooks/useApi";
import { buildChips, type Chip } from "./chips";
import { MessageBubble, TypingIndicator } from "./components/MessageBubble";

const ACCENT = "#15433a";
const ACCENT_SOFT = "#e6efe9";
const PAPER = "#f4f2ec";

const BADGE_COLOR: Record<string, string> = {
  Low: "#8a6d1f",
  Medium: "#1f5f8a",
  High: "#2c6e49",
};

// A chat message plus, for assistant answers, the deterministic source files
// that the specific question drew on (set when the question came from a chip).
type UiMessage = ChatMessage & { sources?: string[] };

export default function App() {
  const [investors, setInvestors] = useState<InvestorSummary[]>([]);
  const [investorId, setInvestorId] = useState<string>("");
  const [messages, setMessages] = useState<UiMessage[]>([]);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);
  const [booting, setBooting] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [overviewSources, setOverviewSources] = useState<string[]>([]);
  const [askedLabels, setAskedLabels] = useState<string[]>([]);

  const bottomRef = useRef<HTMLDivElement>(null);
  const taRef = useRef<HTMLTextAreaElement>(null);

  // Load investor list (with signals) once at startup
  useEffect(() => {
    fetchInvestors()
      .then((list) => {
        setInvestors(list);
        if (list.length) setInvestorId(list[0].investorId);
      })
      .catch((e) => setError(e.message))
      .finally(() => setBooting(false));
  }, []);

  const current = useMemo(
    () => investors.find((i) => i.investorId === investorId),
    [investors, investorId]
  );

  const chips: Chip[] = useMemo(
    () => (current ? buildChips(current.signals) : []),
    [current]
  );

  // Reset the conversation (and the asked-chip tracker) when the investor changes
  useEffect(() => {
    setMessages([]);
    setError(null);
    setAskedLabels([]);
  }, [investorId]);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, loading]);

  // Core send. `answerSources` is the deterministic per-question source list to
  // attach to the assistant's reply; when the user free-types, we fall back to
  // the investor-level overview source map.
  const send = useCallback(
    async (text?: string, answerSources?: string[]) => {
      const q = (text ?? input).trim();
      if (!q || loading || !investorId) return;
      setInput("");
      setError(null);
      const next: UiMessage[] = [...messages, { role: "user", content: q }];
      setMessages(next);
      setLoading(true);
      try {
        const reply = await sendChat(investorId, next);
        setMessages((p) => [
          ...p,
          { role: "assistant", content: reply, sources: answerSources ?? overviewSources },
        ]);
      } catch (e) {
        setError(e instanceof Error ? e.message : String(e));
      } finally {
        setLoading(false);
        taRef.current?.focus();
      }
    },
    [input, loading, messages, investorId, overviewSources]
  );

  // Send a question from a chip: remember the chip (so it leaves the suggestion
  // strip) and attach that chip's own deterministic source files to the answer.
  const sendChip = useCallback(
    (chip: Chip) => {
      setAskedLabels((prev) => (prev.includes(chip.label) ? prev : [...prev, chip.label]));
      send(chip.q, chip.sources);
    },
    [send]
  );

  const onKey = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      send();
    }
  };

  const fmtUsd = (v: number) =>
    "$" + v.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

  if (booting) {
    return (
      <div style={{ height: "100vh", display: "flex", alignItems: "center", justifyContent: "center", background: PAPER, color: "#5e635f", fontFamily: "ui-sans-serif, system-ui, sans-serif" }}>
        Loading your portfolio...
      </div>
    );
  }

  if (!current) {
    return (
      <div style={{ height: "100vh", display: "flex", alignItems: "center", justifyContent: "center", background: PAPER, color: "#b23b34", fontFamily: "ui-sans-serif, system-ui, sans-serif", padding: 24, textAlign: "center" }}>
        {error ?? "Could not load investors. Is the backend running on http://localhost:5000?"}
      </div>
    );
  }

  const firstName = current.investorName.split(" ")[0];

  // Suggestion chips that haven't been asked yet (drives the inline strip)
  const remainingChips = chips.filter((c) => !askedLabels.includes(c.label));

  return (
    <div style={{ display: "flex", flexDirection: "column", height: "100vh", background: PAPER, fontFamily: "ui-sans-serif, -apple-system, system-ui, sans-serif", color: "#1a1d1b" }}>
      {/* Header */}
      <header style={{ background: "#fff", borderBottom: "1px solid #e2e0d8", padding: "12px 24px", display: "flex", alignItems: "center", gap: 16, flexShrink: 0 }}>
        <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
          <div style={{ width: 30, height: 30, borderRadius: 8, background: ACCENT, display: "flex", alignItems: "center", justifyContent: "center", color: "#fff", fontSize: 13, fontWeight: 700, letterSpacing: 0.5 }}>ET</div>
          <span style={{ fontSize: 15, fontWeight: 600 }}>EquiTie Investor Assistant</span>
        </div>
        <div style={{ marginLeft: "auto", display: "flex", alignItems: "center", gap: 10 }}>
          <span style={{ fontSize: 11.5, color: "#8a8d87" }}>Logged in as</span>
          <span style={{ fontSize: 11, fontWeight: 600, color: BADGE_COLOR[current.techSavviness] ?? "#2c6e49", background: ACCENT_SOFT, padding: "3px 9px", borderRadius: 20 }}>
            {current.techSavviness} · {current.signals.positionCount} positions
          </span>
          <select
            value={investorId}
            onChange={(e) => setInvestorId(e.target.value)}
            aria-label="Select investor"
            style={{ fontSize: 13, padding: "6px 10px", borderRadius: 8, border: "1px solid #d4d2ca", background: "#fff", color: "#1a1d1b", cursor: "pointer", minWidth: 180 }}
          >
            {investors.map((i) => (
              <option key={i.investorId} value={i.investorId}>
                {i.investorName} ({i.investorId})
              </option>
            ))}
          </select>
        </div>
      </header>

      {/* Snapshot bar */}
      <SnapshotBar investorId={investorId} current={current} fmtUsd={fmtUsd} onSources={setOverviewSources} />

      {/* Investor profile strip — makes personalisation visible */}
      <div style={{ background: "#fbfaf6", borderBottom: "1px solid #e2e0d8", padding: "6px 24px", display: "flex", gap: 22, flexWrap: "wrap", flexShrink: 0, fontSize: 11.5, color: "#6b6f69" }}>
        <ProfileItem label="Currency" value={current.reportingCurrency} />
        {current.age != null && <ProfileItem label="Age" value={current.age} />}
        <ProfileItem label="Country" value={current.country} />
        <ProfileItem label="KYC" value={current.kycStatus} />
      </div>

      {/* Chat area */}
      <div style={{ flex: 1, overflowY: "auto", padding: "20px 16px", display: "flex", flexDirection: "column", gap: 14 }}>
        {messages.length === 0 && (
          <div style={{ maxWidth: 600, margin: "auto", textAlign: "center", paddingTop: 24 }}>
            <div style={{ width: 50, height: 50, borderRadius: 14, background: ACCENT_SOFT, display: "flex", alignItems: "center", justifyContent: "center", margin: "0 auto 14px", fontSize: 22, color: ACCENT }}>◆</div>
            <h2 style={{ fontSize: 19, fontWeight: 600, marginBottom: 6 }}>Hello, {firstName}</h2>
            {current.signals.overdueFeeCount > 0 && (
              <div style={{ margin: "12px auto", maxWidth: 460, padding: "8px 14px", borderRadius: 8, background: "#fdf6e6", border: "1px solid #e8cf86", color: "#7a5c00", fontSize: 13, textAlign: "left" }}>
                You have {current.signals.overdueFeeCount} overdue fee{current.signals.overdueFeeCount > 1 ? "s" : ""} - tap the chip below to review.
              </div>
            )}
            <p style={{ fontSize: 14, color: "#5e635f", lineHeight: 1.6, marginBottom: 20 }}>
              Ask about your portfolio - holdings, valuations, fees, distributions, or your account statement.
            </p>
            <div style={{ display: "flex", flexWrap: "wrap", gap: 8, justifyContent: "center" }}>
              {chips.map((c) => (
                <ChipButton key={c.label} chip={c} big onClick={() => sendChip(c)} />
              ))}
            </div>
          </div>
        )}
        {messages.map((m, i) => (
          <div key={i}>
            <MessageBubble message={m} />
            {m.role === "assistant" && m.sources && m.sources.length > 0 && <SourcesFooter files={m.sources} />}
          </div>
        ))}
        {loading && <TypingIndicator />}
        {error && (
          <div style={{ padding: "10px 14px", borderRadius: 8, background: "#fdeceb", border: "1px solid #f1c4c0", color: "#b23b34", fontSize: 13 }}>
            {error}
          </div>
        )}
        <div ref={bottomRef} />
      </div>

      {/* Inline chips once chatting — show the remaining question types,
          dropping any the investor has already asked. Hides when none remain. */}
      {messages.length > 0 && !loading && remainingChips.length > 0 && (
        <div style={{ padding: "0 16px 8px", display: "flex", gap: 6, flexWrap: "wrap", flexShrink: 0 }}>
          {remainingChips.map((c) => (
            <ChipButton key={c.label} chip={c} onClick={() => sendChip(c)} />
          ))}
        </div>
      )}

      {/* Input */}
      <div style={{ background: "#fff", borderTop: "1px solid #e2e0d8", padding: "12px 16px", display: "flex", gap: 10, alignItems: "flex-end", flexShrink: 0 }}>
        <textarea
          ref={taRef}
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={onKey}
          rows={1}
          placeholder="Ask about your portfolio..."
          style={{ flex: 1, resize: "none", fontSize: 14, padding: "9px 12px", borderRadius: 10, border: "1px solid #d4d2ca", background: "#faf9f5", color: "#1a1d1b", fontFamily: "inherit", lineHeight: 1.5, outline: "none" }}
        />
        <button
          onClick={() => send()}
          disabled={!input.trim() || loading}
          style={{ padding: "9px 18px", borderRadius: 10, background: input.trim() && !loading ? ACCENT : "#e2e0d8", color: input.trim() && !loading ? "#fff" : "#9a9a93", border: "none", fontSize: 14, fontWeight: 600, cursor: input.trim() && !loading ? "pointer" : "default" }}
        >
          Send
        </button>
      </div>
    </div>
  );
}

/* ── Snapshot bar — fetches the investor's headline numbers ──────────────────
   Rather than a wasteful chat round-trip, the headline figures come from a
   dedicated deterministic overview endpoint, fetched once per investor. The
   overview's source map is used as the fallback citation for free-typed
   questions (chip-driven questions carry their own per-question sources). */
function SnapshotBar({
  current,
  fmtUsd,
  onSources,
}: {
  investorId: string;
  current: InvestorSummary;
  fmtUsd: (v: number) => string;
  onSources: (files: string[]) => void;
}) {
  const [totals, setTotals] = useState<{
    currentValueUsd: number;
    portfolioMoic: number | null;
    committedUsd: number;
    contributedUsd: number;
    feesPaidUsd: number;
    sources: string[];
  } | null>(null);

  useEffect(() => {
    let active = true;
    fetch(`/api/investors/${current.investorId}/overview`)
      .then((r) => (r.ok ? r.json() : null))
      .then((d) => {
        if (active && d) {
          setTotals(d);
          if (Array.isArray(d.sources)) onSources(d.sources);
        }
      })
      .catch(() => {});
    return () => {
      active = false;
    };
  }, [current.investorId, onSources]);

  const moic = totals?.portfolioMoic == null ? "—" : totals.portfolioMoic.toFixed(2) + "x";
  const calls = current.signals.upcomingCallCount;

  return (
    <div style={{ background: "#fff", borderBottom: "1px solid #e2e0d8", padding: "11px 24px", display: "flex", gap: 32, flexWrap: "wrap", flexShrink: 0, fontSize: 12.5 }}>
      <Stat label="Current value" value={totals ? fmtUsd(totals.currentValueUsd) : "—"} />
      <Stat label="Portfolio MOIC" value={moic} accent />
      <Stat label="Committed" value={totals ? fmtUsd(totals.committedUsd) : "—"} />
      <Stat label="Contributed" value={totals ? fmtUsd(totals.contributedUsd) : "—"} />
      <Stat label="Fees paid" value={totals ? fmtUsd(totals.feesPaidUsd) : "—"} />
      {calls > 0 && <Stat label="Upcoming calls" value={calls} warn />}
    </div>
  );
}

function ProfileItem({ label, value }: { label: string; value: string | number }) {
  return (
    <span>
      <span style={{ color: "#a3a69f", textTransform: "uppercase", letterSpacing: 0.3, fontSize: 10 }}>{label} </span>
      <span style={{ color: "#3c4039", fontWeight: 500 }}>{value}</span>
    </span>
  );
}

/* Deterministic source footer — the files come from code (the chip's own source
   list, or the overview source map for free-typed questions), never parsed from
   the model's text. Rendered under each assistant answer so provenance is
   always visible and specific to that question. */
function SourcesFooter({ files }: { files: string[] }) {
  return (
    <div style={{ display: "flex", alignItems: "center", gap: 6, flexWrap: "wrap", margin: "6px 0 2px 2px" }}>
      <span style={{ fontSize: 10.5, color: "#9a9d96", textTransform: "uppercase", letterSpacing: 0.4 }}>Sources</span>
      {files.map((f) => (
        <span
          key={f}
          style={{
            fontSize: 11,
            fontFamily: "ui-monospace, 'SF Mono', Menlo, monospace",
            color: "#5e7d72",
            background: "#eef3f0",
            border: "1px solid #dde8e3",
            padding: "1px 7px",
            borderRadius: 6,
          }}
        >
          {f}
        </span>
      ))}
      <span style={{ fontSize: 10.5, color: "#a3a69f" }}>· calculated using latest share prices</span>
    </div>
  );
}

function Stat({ label, value, accent, warn }: { label: string; value: string | number; accent?: boolean; warn?: boolean }) {
  return (
    <div style={{ display: "flex", flexDirection: "column" }}>
      <span style={{ fontSize: 10.5, color: "#8a8d87", textTransform: "uppercase", letterSpacing: 0.4 }}>{label}</span>
      <span style={{ fontSize: 14, fontWeight: 600, color: warn ? "#b23b34" : accent ? ACCENT : "#1a1d1b" }}>{value}</span>
    </div>
  );
}

function ChipButton({ chip, big, onClick }: { chip: Chip; big?: boolean; onClick: () => void }) {
  const [h, setH] = useState(false);
  const urgent = chip.urgent;
  const border = urgent ? "1px solid #e8cf86" : "1px solid #d4d2ca";
  const bg = urgent ? (h ? "#fbeec8" : "#fdf6e6") : h ? "#ecebe3" : "#fff";
  const color = urgent ? "#7a5c00" : big ? "#2c302c" : "#565a54";
  return (
    <button
      onClick={onClick}
      onMouseEnter={() => setH(true)}
      onMouseLeave={() => setH(false)}
      style={{ fontSize: big ? 13 : 12, padding: big ? "7px 14px" : "4px 11px", borderRadius: 20, cursor: "pointer", fontFamily: "inherit", border, background: bg, color }}
    >
      {chip.label}
    </button>
  );
}