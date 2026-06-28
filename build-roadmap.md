# EquiTie Relationship Manager Bot – Production Roadmap

Vision

Evolve the prototype Investor Assistant into a production-grade, AI-powered relationship manager that helps investors access portfolio information, complete operational tasks and communicate more effectively with their Relationship Manager (RM).

The bot complements—not replaces—the human RM by automating routine requests while ensuring regulated activities remain under human oversight.

Timeframe: Six months  
Assumption:Dedicated delivery team with sufficient engineering budget.

---

# 1. Product Scope

## Core Capabilities

| Capability | Description |
|------------|-------------|
| Portfolio Q&A | Answer questions about holdings, valuations, MOIC, fees, distributions, capital calls and account activity. |
| Proactive Notifications | Capital call reminders, overdue fee alerts, document expiry reminders and important portfolio events. |
| Investor Onboarding | KYC/AML document collection, suitability questionnaires and onboarding guidance. |
| Document Requests | Request missing documentation, monitor completion and escalate outstanding items. |
| Investor Reporting | Generate account statements, NAV summaries and tax pack summaries on demand. |
| RM Communication | Draft investor emails for Relationship Manager review before sending. |
| Operational Queries | Explain fees, capital calls, distributions and portfolio activity in plain language. |

## Out of Scope

The assistant will not:

- Provide investment advice or recommendations.
- Execute investment instructions.
- Handle formal complaints (immediately escalates to an RM).
- Replace the investor relationship.

Its purpose is to improve investor service while allowing Relationship Managers to focus on higher-value interactions.

---

# 2. Solution Architecture

```text
                     iOS Application (SwiftUI)
        ┌────────────────────────────────────────────┐
        │ Chat │ Notifications │ Document Uploads    │
        └────────────────────────────────────────────┘
                           │
                    HTTPS / WebSockets
                           │
                 API Gateway / Authentication
                           │
        ┌────────────────────────────────────────────┐
        │          ASP.NET Core (.NET 8)             │
        │                                            │
        │  Chat Orchestrator                         │
        │    • Context Builder                       │
        │    • Prompt Builder                        │
        │    • AI Client                             │
        │    • Audit Logger                          │
        │                                            │
        │  Background Services                       │
        │    • Notification Engine                   │
        │    • Document Processing                   │
        └────────────────────────────────────────────┘
                 │                      │
         PostgreSQL              External Systems
```

The backend remains responsible for all business logic, portfolio calculations and integrations.

The language model is responsible only for natural language understanding and response generation.

---

# 3. Technology Stack

## Mobile

- Swift
- SwiftUI
- Push Notifications (APNs)

## Backend

- ASP.NET Core (.NET 8)
- PostgreSQL
- Entity Framework Core
- Background Workers
- REST APIs

## AI

Use a production-grade Large Language Model (for example Claude Sonnet or GPT-4.1) for conversational interactions.

For large documents (PPMs, subscription agreements and reports), implement Retrieval-Augmented Generation (RAG) using vector search.

Structured financial information continues to use deterministic backend calculations.

---

# 4. Data Architecture

The Fund Administration Platform remains the single source of truth for financial data.

Typical integrations include:

| System | Purpose |
|--------|---------|
| Fund Administration Platform | Allocations, fees, distributions, valuations |
| CRM | Investor profile and relationship information |
| KYC Provider | Identity verification and document status |
| E-signature Platform | Subscription documents and agreements |
| Document Store | Statements, reports and legal documentation |
| Notification Services | Email and push notifications |

Financial data is synchronised into PostgreSQL.

The backend queries PostgreSQL when constructing investor context.

The AI layer never queries operational systems directly.

---

# 5. AI Architecture

The solution deliberately separates deterministic computation from AI.

## Deterministic Backend

Responsible for:

- Portfolio calculations
- MOIC
- FX conversion
- Fees
- Capital calls
- Distributions
- Current valuations
- Investor filtering
- Permission checks

## Language Model

Responsible for:

- Explaining financial information
- Summarising documents
- Answering investor questions
- Adapting language to investor sophistication
- Drafting communications

The language model never performs financial calculations.

It receives a verified context assembled by the backend.

This architecture significantly reduces hallucinations while improving auditability and consistency.

---

# 6. AI Safety and Governance

The production system should include several safeguards.

## Guardrails

- No investment advice.
- Escalate complaints directly to a Relationship Manager.
- Clearly disclose when responses are AI-generated.
- Never fabricate missing data.
- Explain only verified portfolio information.

## Audit Trail

Every interaction should record:

- Investor ID
- Timestamp
- Model version
- Prompt
- Retrieved context
- Response
- Token usage

This provides full traceability for compliance and debugging.

---

# 7. Retrieval Strategy

Portfolio calculations remain deterministic.

Large unstructured documents (PPMs, reports and legal agreements) should be indexed using vector search.

The retrieval process is:

1. Retrieve relevant document fragments.
2. Combine retrieved content with verified portfolio context.
3. Generate the response.

This avoids sending entire documents to the model while improving response quality.

---

# 8. Observability

Every AI interaction should be observable.

Capture:

- Prompt construction time
- Retrieval latency
- Model latency
- Token usage
- Response quality metrics
- Error rates

Every investor request should be traceable from the original question through prompt construction, retrieved context, model response and final output.

This significantly improves production support and troubleshooting.

---

# 9. Performance Optimisation

To improve responsiveness and reduce inference costs:

- Cache frequently requested portfolio summaries.
- Reuse deterministic portfolio calculations where appropriate.
- Stream AI responses to the client.
- Batch background notification processing.
- Apply API rate limiting and request throttling.

---

# 10. Security

Security should be designed into the platform.

Recommended controls include:

- Single Sign-On (SSO)
- Role-Based Access Control (RBAC)
- Encryption in transit and at rest
- Immutable audit logging
- Regional data residency
- Secret management
- Least-privilege service accounts

---

# 11. Compliance

The production solution should support:

- UK GDPR
- FCA guidance
- SOC 2
- Audit retention policies

The assistant should always disclose that it provides informational support and not regulated financial advice.

---

# 12. Team

| Role | Responsibility |
|------|----------------|
| Senior Backend Engineer | APIs, integrations and business logic |
| Senior iOS Engineer | Mobile application |
| AI Engineer | Prompting, evaluation and RAG |
| Frontend Engineer | RM web portal |
| QA Engineer | Automation and regression testing |
| Product Manager | Roadmap and stakeholder management |
| Compliance Consultant | Regulatory review |
| Security Consultant | Security assessment |

---

# 13. Delivery Roadmap

## Phase 1 – Foundation (Months 1–2)

- PostgreSQL integration
- Investor authentication
- Chat API
- Portfolio Q&A
- Audit logging
- Internal pilot

---

## Phase 2 – Operational Workflows (Months 3–4)

- Proactive notifications
- KYC workflows
- Document requests
- E-signature integration
- RM web portal
- Retrieval-Augmented Generation

---

## Phase 3 – Production Readiness (Months 5–6)

- Compliance review
- Security testing
- Performance optimisation
- Monitoring dashboards
- Investor feedback collection
- General availability

---

# 14. Risks

| Risk | Mitigation |
|------|------------|
| Delays integrating fund administration systems | Begin with scheduled data imports while API integration is completed. |
| Incorrect financial responses | Keep all calculations deterministic and validate with automated tests. |
| AI produces regulated advice | Prompt guardrails, output validation and RM escalation. |
| Low investor trust | Transparency, auditability and human oversight. |
| Increased AI costs | Cache deterministic results and monitor token usage. |

---

# 15. Build vs Buy

| Component | Decision |
|----------|----------|
| Large Language Model | Buy |
| Vector Search | Build on PostgreSQL (`pgvector`) |
| KYC | Buy |
| E-signature | Buy |
| Fund Administration | Integrate |
| Investor Experience | Build |

The focus should remain on building the capabilities that differentiate the platform while integrating established solutions for commodity services.

---

# 16. Estimated Operational Costs

At this scale, engineering effort is the primary investment.

Infrastructure costs remain relatively modest until investor adoption increases.

Typical monthly infrastructure costs include:

- AI inference
- Cloud hosting
- Managed PostgreSQL
- Monitoring
- Notification services

As adoption grows, model inference becomes the primary operational cost to optimise.

---

# Final Thoughts

The architecture intentionally preserves a clear separation between deterministic financial computation and AI-generated language.

The backend remains the authoritative source for calculations, permissions and business rules.

The AI layer focuses exclusively on understanding investor questions and communicating verified information in a clear, personalised manner.

This approach produces a solution that is reliable, explainable, auditable and significantly easier to maintain as the platform evolves.

Rather than replacing Relationship Managers, the assistant augments them—automating repetitive operational work while allowing human experts to focus on advice, relationships and investor outcomes.