# VM Baseline

Collected 2026-08-17 with the instrumented `net8.0` build.

Run the metrics report with:

```powershell
dotnet run --project benchmarks/XPath.Regex.NET.Benchmarks/XPath.Regex.NET.Benchmarks.csproj -c Release -p:RegexInstrumentation=true -- --metrics
```

The timing benchmarks must be run without instrumentation:

```powershell
dotnet run --project benchmarks/XPath.Regex.NET.Benchmarks/XPath.Regex.NET.Benchmarks.csproj -c Release -- --filter *MatchingBenchmarks*
```

## Character-test counts

| Scenario | 256 | 512 | 1024 | 2048 | Scaling |
| --- | ---: | ---: | ---: | ---: | --- |
| `a+z` on `a^n` | 66,045 | 263,165 | 1,050,621 | 4,198,397 | Quadratic |
| `^a+z$` on `a^n` | 513 | 1,025 | 2,049 | 4,097 | Linear |
| `a+z` on `b^n` | 255 | 511 | 1,023 | 2,047 | Linear |
| `a+z` on `a^(n-1)z` | 511 | 1,023 | 2,047 | 4,095 | Linear |

For the unanchored no-match case, doubling input length produces approximately four times as many character tests. The normalized values were `1.0078`, `1.0039`, `1.0020`, and `1.0010` character tests per input character squared.

`CandidateStarts` is linear in all cases because the current implementation still advances `startPos` one position at a time. The quadratic cost comes from rerunning a long failed VM scan at each candidate start.

## Timing smoke run

BenchmarkDotNet dry job, one cold-start sample per input, Release without instrumentation:

| Input length | Mean |
| ---: | ---: |
| 256 | 254.0 ms |
| 512 | 637.9 ms |
| 1024 | 1.846 s |
| 2048 | 5.727 s |

These timings are directional only because each size has one sample and includes cold-start effects. Use the instrumented counters for the scaling claim.

## Post-fix instrumented run

The unanchored search now uses a compiler-generated reluctant `.*?` prefix for regular XPath programs. The VM runs one scan per requested match instead of restarting at every candidate position.

| Scenario | 256 | 512 | 1024 | 2048 | Scaling |
| --- | ---: | ---: | ---: | ---: | --- |
| `a+z` on `a^n` | 1,026 | 2,050 | 4,098 | 8,194 | Linear |
| `^a+z$` on `a^n` | 770 | 1,538 | 3,074 | 6,146 | Linear |
| `a+z` on `b^n` | 514 | 1,026 | 2,050 | 4,098 | Linear |
| `a+z` on `a^(n-1)z` | 1,022 | 2,046 | 4,094 | 8,190 | Linear |

The `UnanchoredNoMatch` character-test counts are now approximately `4n` instead of `n^2`: 1,026, 2,050, 4,098, and 8,194. The normalized values fall from `0.0156555` to `0.0019536` character tests per input character squared as the input doubles. All benchmark results remained correct: the three no-match scenarios returned `False`, and `MatchAtEnd` returned `True`.

## Prefix-hint baseline (before wiring)

Collected 2026-08-17 with current unwired metadata. Instrumented metrics use restart-only backreference patterns so prefix skipping is observable.

| Scenario | 256 | 512 | 1024 | 2048 | Scaling |
| --- | ---: | ---: | ---: | ---: | --- |
| `(foobar)\\d+\\1`, no prefix occurrence: candidate starts | 244 | 500 | 1,012 | 2,036 | Linear |
| `(foobar)\\d+\\1`, no prefix occurrence: character tests | 244 | 500 | 1,012 | 2,036 | Linear |
| `.*abc` control: character tests | 771 | 1,539 | 3,075 | 6,147 | Linear |

Timing smoke run before wiring: `MatchingBenchmarks.UnanchoredNoMatch` measured 24.211 ms, 50.964 ms, 131.124 ms, and 223.701 ms at 256, 512, 1024, and 2048 characters respectively (one cold-start sample each).

## Prefix-hint results (after wiring)

Collected 2026-08-17 with `PikeVm` prefix fast-forward enabled. Case-insensitive searches deliberately skip this path because `string.IndexOf` OrdinalIgnoreCase does not guarantee VM Unicode SimpleFold equivalence.

| Scenario | 256 | 512 | 1024 | 2048 | Scaling |
| --- | ---: | ---: | ---: | ---: | --- |
| `(foobar)\\d+\\1`, no prefix occurrence: candidate starts | 0 | 0 | 0 | 0 | Constant |
| `(foobar)\\d+\\1`, no prefix occurrence: character tests | 0 | 0 | 0 | 0 | Constant |
| `.*abc` control: character tests | 771 | 1,539 | 3,075 | 6,147 | Linear |

Prefix miss work drops from 2,036 candidate VM starts and character tests at length 2048 to zero; control metrics are unchanged. This is measurable improvement with no-prefix control regression.
