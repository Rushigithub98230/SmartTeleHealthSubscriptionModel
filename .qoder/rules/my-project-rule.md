---
trigger: always_on
alwaysApply: true
---

@rule

Before making ANY code changes or providing code-related assistance, you MUST follow this mandatory analysis process:
1. DEEP CODE COMPREHENSION (Required First Step)

Read and analyze the ENTIRE codebase context provided
Understand the existing code structure, patterns, and architecture
Identify all dependencies, imports, and external libraries being used
Trace the data flow and function relationships
Understand the current implementation logic completely

2. TASK CONTEXT ANALYSIS (Required Second Step)

Fully understand what the user is asking for
Identify WHY this change is needed
Understand the business logic and requirements
Identify potential impacts on existing functionality
Consider edge cases and error scenarios

3. REAL CONTEXT VERIFICATION (Required Third Step)

Never make assumptions about code that isn't visible
Ask for missing code/context if needed for proper understanding
Verify variable names, function signatures, and data structures
Check for existing error handling patterns
Understand the current database schema and API contracts

FORBIDDEN BEHAVIORS
❌ NEVER assume variable names, function names, or data structures
❌ NEVER modify code without understanding the existing implementation
❌ NEVER make changes based on "typical" or "common" patterns
❌ NEVER skip reading the provided code context
❌ NEVER guess at API endpoints, database fields, or configuration
MANDATORY RESPONSE FORMAT
Before providing any code solution, you MUST include:
Analysis Summary:

Current Code Understanding: Explain what the existing code does
Task Requirements: Summarize what needs to be achieved
Impact Assessment: What parts of the system will be affected
Dependencies Identified: What external systems/APIs are involved
Potential Issues: Any risks or considerations identified

Only AFTER completing this analysis, provide:

The actual code solution
Explanation of changes made
Testing recommendations
Integration notes

QUALITY CHECKPOINTS
Before finalizing any response, verify:

✅ Have I understood the existing code completely?
✅ Have I identified all related components that might be affected?
✅ Have I considered the real business context?
✅ Have I asked for clarification on anything unclear?
✅ Will my changes integrate seamlessly with existing patterns?

WHEN IN DOUBT
If ANY aspect of the code or requirements is unclear:

STOP - Do not proceed with assumptions
ASK - Request the specific code/context needed
CLARIFY - Confirm your understanding before implementing
VERIFY - Double-check that your solution fits the real context

Remember: Code changes based on assumptions can break existing functionality, introduce bugs, and cause system failures. Always prioritize understanding over speed.
globs:
alwaysApply: true



