import type { ChatMessage, ChatResponse, InvestorSummary } from '../types/api'

const BASE = '/api'

export async function fetchInvestors(): Promise<InvestorSummary[]> {
  const res = await fetch(`${BASE}/investors`)
  if (!res.ok) throw new Error(`Failed to load investors: ${res.statusText}`)
  return res.json()
}

export async function sendChat(
  investorId: string,
  messages: ChatMessage[]
): Promise<string> {
  const res = await fetch(`${BASE}/chat`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ investorId, messages }),
  })
  if (!res.ok) {
    const text = await res.text()
    throw new Error(`Chat error ${res.status}: ${text}`)
  }
  const data: ChatResponse = await res.json()
  return data.reply
}
