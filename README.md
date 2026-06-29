# EquiTie Investor Assistant

An AI-powered investor assistant that combines deterministic financial calculations with large language models to deliver personalised, trustworthy portfolio insights.

---

# Executive Summary

This project is a working prototype of an AI-powered Investor Assistant for EquiTie.

The objective was not simply to build a chatbot, but to demonstrate how large language models can be combined with deterministic business logic to answer investor questions accurately while remaining grounded in the supplied portfolio data.

The prototype enables an authenticated investor to:

- View a personalised portfolio overview
- Ask questions about individual investments
- Review current valuations and MOIC
- View upcoming capital calls and fees
- Review distributions and realised returns
- Generate a plain-language account statement
- Receive responses tailored to their profile (age, technical proficiency and portfolio composition)

Rather than allowing the language model to perform financial calculations directly, all portfolio calculations are performed deterministically within the application. The AI model is responsible only for interpreting verified portfolio data and generating natural language responses.

This separation significantly reduces hallucinations while improving explainability, consistency and trust.

---

# Highlights

- Deterministic financial calculations (MOIC, valuations, FX, fees and portfolio totals).
- AI used exclusively for natural language generation and personalisation.
- Investor-specific prompts built from verified portfolio data.
- Clear separation between business logic, prompt construction and presentation.
- Architecture designed to evolve into a production solution with minimal changes.

---

# Time Spent

The assessment recommends spending approximately 2–3 hours.

I spent approximately 3 hours.
Given the financial nature of the problem, I considered correctness and explainability a better investment than implementing additional features.

---

# Scope

Rather than attempting to build a production-ready investment platform, I focused on delivering a complete end-to-end workflow demonstrating the core objectives of the assessment.

The implemented scope includes:

- Investor selection
- Portfolio aggregation
- Deterministic financial calculations
- Prompt construction
- AI-powered responses
- Personalised user experience

Features such as authentication, persistence, deployment and monitoring were intentionally deferred to keep the implementation focused on the core problem.

---

# Design Philosophy

Given the suggested timebox, I prioritised:

- Deterministic financial calculations
- Grounded AI responses
- Clear separation of responsibilities
- Investor-specific personalisation
- A maintainable architecture that can evolve into production

The emphasis throughout the solution was correctness over feature breadth.

---

# Solution Overview

The solution is organised into five logical layers, each with a single responsibility.

```
                 React Application

                 Investor Dashboard
                        │
                        ▼
                  Portfolio Service
         (Business Rules & Calculations)
                        │
                        ▼
                 CSV Data Repository
              (Investor Dataset Parsing)
                        │
                        ▼
                 Prompt Builder
      (Grounded Context Construction)
                        │
                        ▼
                  Claude Sonnet API
          (Natural Language Generation)
```

Each layer has a clearly defined responsibility, making the application easier to understand and evolve.

---

# Architecture

## Data Repository

The supplied CSV files are parsed once during application startup.

The repository exposes strongly typed access to:

- Investors
- Companies
- Deals
- Allocations
- Valuations
- Fees
- Capital Calls
- Distributions
- FX Rates

This keeps the implementation lightweight while avoiding repeated parsing during conversations.

---

## Portfolio Service

This layer contains all financial business logic.

It is responsible for calculating:

- Current portfolio value
- Cost basis
- MOIC
- Current share prices
- Commitment vs contribution
- Outstanding commitments
- Currency conversion
- Fee summaries
- Distribution summaries
- Sector exposure

Every number displayed to the investor originates from deterministic application code.

The AI model never performs arithmetic.

---

## Prompt Builder

Rather than exposing raw CSV data to the language model, the application builds a compact investor-specific context containing:

- Investor profile
- Holdings
- Valuation history
- Fee obligations
- Capital calls
- Distributions
- Account statement
- Portfolio metrics
- Sector exposure

Only verified information for the selected investor is included.

This reduces hallucinations while improving response quality and keeping prompts compact.

---

## AI Layer

Claude Sonnet is used exclusively for natural language generation.

Responsibilities include:

- Explaining portfolio information
- Adapting language to investor sophistication
- Summarising financial data
- Answering investor questions

The AI model does not calculate financial values.

This mirrors the architecture commonly used in production financial systems where deterministic services remain the source of truth.

---

# Personalisation

One requirement of the assessment was tailoring responses to individual investors.

The assistant uses the `tech_savviness` field to classify investors into three explicit personalisation tiers (Low, Medium and High).

Responses are then adapted using:

- Technical proficiency
- Age
- Portfolio composition
- Investment sectors
- Outstanding obligations

For example:

- Investors in the Low tier receive shorter responses with financial terminology explained in plain language.
- Investors in the Medium tier receive balanced explanations with moderate technical detail.
- Investors in the High tier receive concise, data-rich responses that assume familiarity with venture capital terminology.

Only the presentation changes.

The underlying financial calculations remain identical for every investor.

---

# AI-Assisted Architecture

The application deliberately combines deterministic software engineering with AI.

```
Financial Calculations
        │
        ▼
Deterministic Code
        │
        ▼
Verified Portfolio Context
        │
        ▼
Large Language Model
        │
        ▼
Natural Language Response
```

This architecture provides:

- Repeatable calculations
- Reduced hallucinations
- Easier verification
- Simpler debugging
- Predictable behaviour

The AI model is responsible for communication.

The application remains responsible for correctness.

---

# Verification

Because the application operates on financial information, correctness was prioritised throughout the implementation.

Correctness in this prototype is achieved through deterministic architecture and manual ground-truth validation, not an automated test suite.

All financial calculations run in application code, so the same investor inputs always produce the same outputs — the model is structurally unable to influence the numbers. Against that baseline, I manually derived expected figures directly from the supplied CSV files for a reference investor (INV001), then cross-checked the running application against those figures.

In addition:

- Financial metrics were validated against the supplied dataset.
- FX conversions were cross-checked.
- Multi-round investments and partial capital calls were independently verified.
- Responses were reviewed to confirm they referenced only investor-specific data.
- Automated tests were deliberately omitted to keep the implementation aligned with prototype scope. Adding a regression test suite is the highest priority next step.

This ensures the AI explains verified financial information rather than generating financial calculations itself.

# Assumptions

This prototype assumes:

- The investor is already authenticated.
- Only one investor can be accessed at a time.
- The supplied CSV files are the source of truth.
- The dataset is internally consistent.
- The reporting date is 25 June 2026.

These assumptions align with the assessment brief.

---

# Trade-offs

Within the assessment timebox I deliberately chose not to implement:

- Authentication
- Persistent storage
- Conversation persistence
- Streaming responses
- Production monitoring
- Deployment infrastructure
- Row-level source citation UI

Instead I prioritised:

- Deterministic financial calculations
- Grounded AI responses
- Clean separation between business logic and AI
- Investor-specific personalisation
- Maintainable architecture

These decisions were intentional.

Given the available time, I believed demonstrating a reliable AI workflow and deterministic financial calculations provided greater value than implementing production infrastructure that would not materially change the core solution.

The application already includes file-level source citations, allowing investors to see which datasets were used when generating responses. Row-level citations (linking responses back to specific CSV records) were intentionally deferred as a future enhancement.

# Running the Project

## Prerequisites

Ensure the following are installed:

- .NET 8 SDK
- Node.js 20+ (or later)
- Visual Studio 2022 or Visual Studio Code with the C# Dev Kit
- Git

## Configuration

Before running the application, you must configure an Anthropic Claude API key.

1. Generate an API key from the Anthropic Console.
2. Open `backend/EquiTie.Api/appsettings.json`.
3. Add your API key to the `Anthropic` configuration section:

```json
{
  "Anthropic": {
    "ApiKey": "YOUR_CLAUDE_API_KEY"
  }
}
```

> **Important**
>
> A valid Claude API key is required for the AI assistant to generate responses. Without it, the backend will start successfully, but AI-powered features will not function.---

## Backend (.NET 8)

The backend is an ASP.NET Core (.NET 8) application.

Open the solution in Visual Studio 2022 or Visual Studio Code, restore the NuGet packages and run the API.

### Visual Studio

- Open the solution (`.sln`)
- Set the API project as the Startup Project
- Press F5 (or Ctrl + F5) to run(typically `http://localhost:5000`)

### .NET CLI

```bash
dotnet restore
dotnet build
dotnet run
```

The backend loads the supplied CSV files once during application startup via the `CsvDataRepository`. The parsed data is cached in memory for the lifetime of the application, so no database setup is required.

---

## Frontend (React + Vite)

Navigate to the frontend project and run:

```bash
npm install
npm run dev
```

The React application will start using the default Vite development server (typically `http://localhost:5173`).

---

## Using the Application

Once both services are running:

- Open the React application in your browser.
- Select an investor from the dropdown.
- Ask questions about the investor's portfolio.
- The frontend communicates with the .NET backend, which performs deterministic portfolio calculations before invoking the AI model to generate personalised natural language responses.

# Limitations

The prototype intentionally focuses on the core workflow rather than production completeness.

Current limitations include:

- In-memory CSV-backed store (no database)
- No authentication
- No audit logging
- No streaming responses
- No persistent conversation history (not stored between sessions)
- Simplified prompt management
- No automated evaluation framework

These limitations were accepted to keep the implementation aligned with the assessment scope.

---

# Failure Modes

While the application is designed to minimise hallucinations by grounding responses in deterministic portfolio data, there are scenarios where response quality or system behaviour may still be affected.

Current failure modes include:

- Ambiguous questions that lack sufficient context (for example, *"How is it doing?"* without specifying a company).
- Questions outside the supplied dataset, such as requests for market commentary, investment advice or information that does not exist in the provided CSV files.
- Requests requiring historical information that is not present in the supplied dataset.
- Very long conversations, as conversation history is intentionally limited in the prototype.
- Incorrect or inconsistent source data, since deterministic calculations can only be as accurate as the underlying dataset.
- External AI service failures, model unavailability or API rate limiting, which may temporarily prevent responses from being generated.
- The investor selector simulates authentication. In the current prototype, any user can select any `investor_id` from the dropdown. Data isolation is enforced by the backend when constructing the investor context, but the UI itself does not provide production-grade access control.
- Prompt or model changes may alter the quality, tone or completeness of responses without triggering any automated alert, as the prototype does not include an evaluation framework to validate AI responses against the deterministic portfolio context.

Where possible, the application is designed to fail safely. The backend always remains the source of truth for financial calculations and investor filtering, ensuring the AI is limited to explaining verified portfolio data rather than generating or inferring financial information.


# Path to Production

If developed beyond the assessment, the next priorities would be:

1. Replace the CSV repository with PostgreSQL.
2. Introduce authentication and role-based access control.
3. Add row-level source citations for complete traceability.
4. Move AI interactions behind a dedicated backend service.
5. Persist conversation history.
6. Stream AI responses.
7. Introduce telemetry and monitoring.
8. Add automated AI evaluation and prompt regression testing.

---

# Future Improvements

Additional enhancements would include:

- Vector search for supporting documentation
- Automated regression testing
- Evaluation framework for prompt quality
- Production observability
- Azure deployment using containerised services

---

# Closing Thoughts

This prototype demonstrates an architectural pattern rather than simply integrating an LLM into an application.

Deterministic application code is responsible for financial correctness.

AI is responsible for communication and personalisation.

Keeping those responsibilities independent improves reliability, simplifies testing and provides a clear path towards a production-grade investor assistant.

Throughout the assessment, every architectural decision was evaluated against one principle:

> Deterministic code should own correctness, while AI should improve communication.

That principle guided the final design of this solution.