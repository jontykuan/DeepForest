---
name: cross-cutting-concerns-standardizer
description: Standardizes logging, error handling, and configuration management across the codebase, ensuring consistent patterns, context preservation, and automated enforcement.
---

# Cross-Cutting Concerns Standardizer

This skill standardizes system-wide, non-functional requirements (cross-cutting concerns) to ensure code consistency, readability, and ease of maintenance.

---

## 1. Structured Logging Standard

Implement structured key-value logging rather than ad-hoc string formatting.

*   **Standard Library Usage**: Use standard structured logging libraries (e.g., `log/slog` in Go, or C#-specific structured loggers/BBCode tags in formatting systems).
*   **Log Levels**:
    *   `DEBUG`: Highly detailed operational info, tracing values, transient states.
    *   `INFO`: High-level system lifecycle events (startup, shutdown, major state transitions).
    *   `WARN`: Non-fatal issues that can be recovered from but indicate anomalies (retries, degraded state).
    *   `ERROR`: Critical failures requiring attention or indicating software bugs.
*   **Context Preservation**: Always append relevant key-value metadata to the log context (e.g., `SessionId`, `SeqNo`, `PlayerHP`) instead of embedding variables inside a single flat string.

---

## 2. Robust Error Handling Standard

Preserve error context during propagation and prevent unhandled failure states.

*   **Sentinel Errors**: Define package-level sentinel errors or unique constants for common domain-specific errors.
*   **Error Wrapping**: Always wrap lower-level errors when propagating them up the stack (e.g., using `fmt.Errorf("action failed: %w", err)` in Go or wrapping in C# custom exception classes) to maintain trace context.
*   **Rich Error Context**: Ensure every returned error contains the relevant structural values (e.g., action ID, item name) rather than just a descriptive message.
*   **Graceful Recovery**: Centralize exception catching to degrade gracefully (e.g., falling back to a safe default character state if card load fails) instead of throwing panics or crashing.

---

## 3. Centralized Configuration Standard

Centralize configuration variables to adhere to Twelve-Factor App principles.

*   **Centralized Config Struct**: Maintain a single source of truth for settings (e.g., a shared settings object or `GameState` variables).
*   **Centralized Validation**: Validate all configuration bounds at startup or loading phase, generating explicit errors if settings are out of bounds.
*   **Default Values**: Define sensible default values for every configuration parameter.
*   **Separation**: Keep code logic decoupled from raw configuration variables.

---

## Rationale and Application

Standardizing these concerns eliminates technical debt, reduces logging overhead, and ensures that errors are easily traceable in production environments. Use this skill when refactoring ad-hoc logging statements, error checks, or configuration structures.
