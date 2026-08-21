# Status

## Regex VM design assessment

### Follows Russ Cox VM approach
- Yes. Compiler emits Thompson-style NFA bytecode (`Split`, `Jmp`, `Save`, `Match`, `Char`, `BackRef`): `src/XPath.Regex.NET/Internal/Compiler/RegexCompiler.cs`, `src/XPath.Regex.NET/Internal/Nfa/NfaInstruction.cs`.
- VM is Pike/Thompson lock-step thread simulation with two thread lists and epsilon expansion: `src/XPath.Regex.NET/Internal/Matcher/PikeVm.cs`.
- Leftmost-first priority preserved by split order and PC de-dup: `src/XPath.Regex.NET/Internal/Matcher/ThreadList.cs`.

### Catastrophic behavior risk
- Classic exponential backtracking: **No** for regex without backreferences. VM is NFA simulation with bounded thread count per step.
- Unanchored search: **Single-pass for regular XPath patterns** through a compiler-generated reluctant `.*?` prefix; backreference and XSD patterns retain the existing restart path.
- Backreferences: **Capture-aware thread state implemented**. Programs containing backreferences now de-duplicate by PC, input position, and logical capture values, preserving distinct capture paths while retaining epsilon-cycle protection. Programs without backreferences keep the existing PC-only fast path.

### Compile-time protections
- `RegexCompileOptions` defaults to a maximum quantifier bound of 1,000 and a
  maximum NFA program size of 100,000 instructions. Both limits have
  non-bypassable hard ceilings equal to their defaults; callers may only lower
  them.
- `RegexParser` validates quantifier bounds before `CompileRepeat` can unroll
  them. Bounds above the configured limit, including values that overflow the
  lexer integer representation, throw `RegexCompilationLimitExceededException`
  with `RegexCompilationLimit.MaxQuantifierBound`.
- `CompilerCursor.Emit` checks the instruction budget before appending each NFA
  instruction. Nested-repeat expansion therefore fails with
  `RegexCompilationLimitExceededException` and
  `RegexCompilationLimit.MaxProgramInstructions` before the budget is exceeded.

### Runtime protections
- Runtime safety limits in `PikeVm` are a 5,000 ms timeout, a 100,000-thread
  cap, and a 10,000-level epsilon-expansion stack cap. Each throws
  `RegexEngineLimitExceededException`; `FORX0002` remains reserved for invalid
  patterns detected during compilation.
- Epsilon expansion depth cap: `MaxStackDepth` in `PikeVm.AddThread` prevents
  pathological patterns from exhausting the call stack.
- Runtime safety limits in `PikeVm` are a 5,000 ms timeout, a 100,000-thread
  cap, and a 10,000-level epsilon-expansion stack cap. Each throws
  `RegexEngineLimitExceededException`; `FORX0002` remains reserved for invalid
  patterns at compile time.
- Epsilon expansion depth cap: `MaxStackDepth` in `PikeVm.AddThread` prevents
  pathological patterns from exhausting the call stack.
- Empty-match detection: `MetadataAnalyzer.CanMatchEmpty` used to block `replace/tokenize/analyze-string` with `FORX0003`.
- Minimum match length: `MinMatchLength` prunes start positions.
- Unanchored search: regular XPath programs scan once with a reluctant all-code-point prefix, preserving leftmost-first priority and actual match-start captures.

### Quality summary
- Strong alignment with Cox VM design for regular constructs.
- Good safeguards against infinite epsilon loops and empty-match operations.
- Unanchored regular-pattern search is linear in the measured benchmark; the `a+z`/`a^n` character-test count changed from approximately $n^2$ to linear scaling.
- Backreference-aware state can increase thread-list memory use for capture-heavy patterns; semantics are covered by capture-state regression tests.

### Instrumented baseline
- VM-work instrumentation and BenchmarkDotNet harness: `benchmarks/XPath.Regex.NET.Benchmarks/`.
- Measured `a+z` against `a^n`: character-test count scales approximately with $n^2$; see `benchmarks/BASELINE.md`.
- Post-fix measured `a+z` against `a^n`: 1,026, 2,050, 4,098, and 8,194 character tests at lengths 256, 512, 1024, and 2048; see `benchmarks/BASELINE.md`.
