import type { InvestorSummary } from '../types/api'

interface Props {
  investors: InvestorSummary[]
  selected: string
  onChange: (id: string) => void
}

const TECH_BADGE: Record<string, { bg: string; color: string; label: string }> = {
  Low:    { bg: '#fef3c7', color: '#92400e', label: 'Essentials' },
  Medium: { bg: '#dbeafe', color: '#1e40af', label: 'Informed'   },
  High:   { bg: '#d1fae5', color: '#065f46', label: 'Sophisticated' },
}

export function InvestorSelector({ investors, selected, onChange }: Props) {
  const current = investors.find(i => i.investorId === selected)
  const badge   = TECH_BADGE[current?.techSavviness ?? 'High']

  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
      <label style={{ fontSize: 12, color: '#666', whiteSpace: 'nowrap' }}>
        Logged in as
      </label>
      <select
        value={selected}
        onChange={e => onChange(e.target.value)}
        style={{
          fontSize: 13,
          padding: '4px 8px',
          borderRadius: 6,
          border: '1px solid #d0cec8',
          background: '#fff',
          color: '#1a1a1a',
          cursor: 'pointer',
        }}
      >
        {investors.map(inv => (
          <option key={inv.investorId} value={inv.investorId}>
            {inv.investorName}
          </option>
        ))}
      </select>

      {current && (
        <span
          style={{
            fontSize: 11,
            padding: '2px 8px',
            borderRadius: 20,
            background: badge.bg,
            color: badge.color,
            fontWeight: 500,
            whiteSpace: 'nowrap',
          }}
        >
          {badge.label}
        </span>
      )}
    </div>
  )
}
