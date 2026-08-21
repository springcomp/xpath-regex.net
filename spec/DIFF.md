# XPath Regex Syntax Differences: XSD → XPath 2.0 → 3.0 → 3.1

## Metacharacters

- **XSD 1.0**: `. \ ? * + ( ) [ ]` (had bug: missing `{}`)
- **XPath 2.0+**: `. \ ? * + { } ( ) [ ] ^ $` (fixed; added anchors)

## Quantifiers

- **XSD 1.0**: Greedy only: `?` `*` `+` `{n}` `{n,m}` `{n,}`
- **XPath 2.0+**: Add reluctant: `??` `*?` `+?` `{n}?` `{n,}?` `{n,m}?` (match shortest)

## Grouping & Capture (major XPath 2.0 jump)

- **XSD**: `(...)` no special semantics
- **XPath 2.0+**: Capturing groups `(...)` + back-references `\1`-`\N`
- **XPath 3.0+**: Non-capturing groups `(?:...)` support

## Flags (all versions 2.0+)

| Flag | Meaning |
|------|---------|
| `s` | `.` matches newline + carriage return |
| `m` | `^`/`$` match line boundaries |
| `i` | Case-insensitive (Unicode codepoint) |
| `x` | Strip whitespace from pattern (except in `[...]`) |
| `q` | **XPath 3.0+**: Literal mode — all chars literal, no metacharacters |

## Dot (`.`) Behavior

- **XSD/XPath 2.0**: `[^\n]` (not newline)
- **XPath 3.0+**: `[^\n\r]` (not newline or carriage return)

### Dot with flags

- Default: excludes line breaks
- With `s` flag: matches **everything** including breaks
- With `m` flag: `^`/`$` work per-line (dot unchanged)

## Functions

| Function | XPath 2.0 | XPath 3.0+ |
|----------|-----------|-----------|
| `fn:matches()` | ✓ | ✓ |
| `fn:replace()` | ✓ | ✓ |
| `fn:tokenize()` | ✓ | ✓ |
| `fn:analyze-string()` | ✗ | ✓ (NEW) |

## Anchoring Model

- **XSD**: Implicit `^...$` (whole-string match only)
- **XPath 2.0+**: NO implicit anchors (use explicit `^`/`$` for anchoring)

## Error Codes (all 2.0+)

- `FORX0001`: Bad flags
- `FORX0002`: Bad pattern
- `FORX0003`: Pattern matches zero-length string
- `FORX0004`: Bad replacement string

## Unicode Blocks

- **XPath 2.0**: Invalid block name → implementation-defined
- **XPath 3.0+**: Invalid block name → `FORX0002` error

## Key Architecture

```
XSD:      Base (implicit anchoring, greedy, no captures)
XPath 2.0: + explicit anchors, reluctant quantifiers, captures, back-refs
XPath 3.0: + non-capturing groups, q flag, clarified . behavior
XPath 3.1: Modularized specs, edge-case documentation, same features
```

## Core Difference

XSD regex validates literals; XPath regex processes substrings. XPath 3.0+ adds literal-quote mode (`q` flag) for escaping-free matching.

## Detailed Grammar Evolution

### XSD 1.0 (Base)

```
Char ::= [^.\?*+()|#x5B#x5D]           [10] ERROR: missing { }
quantifier ::= [?*+] | ( '{' quantity '}' )
atom ::= Char | charClass | ( '(' regExp ')' )
charClass ::= charClassEsc | charClassExpr | WildcardEsc
```

**Characteristics:**
- Implicit `^...$` anchoring
- No `^` or `$` support in pattern
- Greedy quantifiers only
- Basic grouping (no capture semantics)
- Definition of `Char` missing `{` and `}`

### XPath 2.0 Extensions (Over XSD)

```
Char ::= [^.\?*+{}()|^$#x5B#x5D]       [10] CORRECTED
charClass ::= charClassEsc | charClassExpr | WildcardEsc | "^" | "$"
quantifier ::= ( [?*+] | ( '{' quantity '}' ) ) '?'?    [4] RELUCTANT
atom ::= Char | charClass | ( '(' regExp ')' ) | backReference
SingleCharEsc ::= '\' [nrt\|.?*+(){}$#x2D#x5B#x5D#x5E]  [24] Added $
```

**Changes:**
- Corrected `Char` definition to include `{` and `}`
- Added explicit `^` and `$` metacharacters
- Reluctant quantifiers via `?` suffix
- Capturing groups with back-references `\1`-`\N`
- Escape for `$` character

**Anchoring:** NOT implicitly anchored (must use explicit `^`/`$`)

### XPath 3.0 Refinement (Over 2.0)

```
Same as XPath 2.0, but:
- References XSD 1.1 baseline (clarified grammar)
- Adds 'q' flag (quote/literal mode)
- Clarifies . to exclude \r (both \n and \r now excluded by default)
- Invalid Unicode block names → FORX0002
- Non-capturing groups (?:...) formalized
```

**Changes:**
- `q` flag: all characters literal, no metacharacter interpretation
- When `q` used with `fn:replace()`: `$` and `\` lose special meaning in replacement
- `.` now excludes both `\n` and `\r` (not just `\n`)
- Unicode block validation: undefined blocks now error instead of implementation-defined

### XPath 3.1 Modularization (Over 3.0)

```
Same productions as XPath 3.0, but reorganized:
- 5.6.1.1: Matching start/end (^, $)
- 5.6.1.2: Reluctant quantifiers
- 5.6.1.3: Captured subexpressions (formal definition)
- 5.6.1.4: Back-references (formal definition)
- 5.6.1.5: Unicode block names
- 5.6.2: Flags section formally defined separately
```

**Changes:**
- Better documentation of edge cases
- Formal definitions for nested repeating constructs
- Clarified behavior with multiple capturing groups
- Enhanced examples for complex patterns

## Flag Interaction Matrix

| Combination | Effect | Notes |
|-------------|--------|-------|
| `i` alone | Case-insensitive matching | Unicode codepoint-based |
| `s` alone | `.` matches everything | Enables newline dotting |
| `m` alone | `^`/`$` match line boundaries | Per-line anchoring |
| `x` alone | Whitespace removed from pattern | Except in `[...]` |
| `q` alone | All literal (no metacharacters) | Replaces all metacharacter meaning |
| `i` + `q` | Literal + case-insensitive | `q` doesn't cancel `i` |
| `q` + `m/s/x` | `m/s/x` ignored | `q` takes precedence |
| `s` + `m` | Both active | `.` matches multiline, `^/$` at line boundaries |
| No flags | Greedy, anchoring-off, case-sensitive | Default mode |

## Character Class Escape Changes

| Escape | XSD 1.0 | XPath 2.0+ | Notes |
|--------|---------|-----------|-------|
| `\n` `\r` `\t` | ✓ | ✓ | Single char escapes |
| `\|` `\.` `\-` `\^` | ✓ | ✓ | Escaped metacharacters |
| `\?` `\*` `\+` `\{` `\}` | ✓ | ✓ | Escaped metacharacters |
| `\(` `\)` `\[` `\]` | ✓ | ✓ | Escaped metacharacters |
| `\$` | ✗ (error) | ✓ (added) | NEW in XPath 2.0 |
| `\s` `\S` `\d` `\D` `\w` `\W` | ✓ | ✓ | Multi-char escapes |
| `\i` `\I` `\c` `\C` | ✓ | ✓ | Name character escapes |
| `\p{X}` `\P{X}` | ✓ | ✓ | Unicode category escapes |

## Implementation Recommendations

1. **Base Implementation** (minimum): Support XPath 3.1 (superset of all)
2. **Compatibility modes**: Provide XSD, XPath 2.0, XPath 3.0, XPath 3.1 options as needed
3. **Key feature set**:
   - Reluctant quantifiers + greedy (default)
   - Capturing + non-capturing groups
   - Back-references through `\1` to `\9`+
   - All flags: `s`, `m`, `i`, `x`, `q`
   - Unicode categories and blocks
   - Explicit anchoring control (`^`/`$`)
4. **Error handling**: Ensure FORX0001-0004 errors properly raised
5. **Unicode**: Support current Unicode version; document version explicitly

## Key Behavioral Differences

### Capturing Groups Numbering

Groups numbered 1, 2, 3... left-to-right by opening `(` position.

**XSD**: No capture semantics
**XPath 2.0+**: 
- Group 0 = entire match
- Group N = Nth captured substring (1-indexed)
- Non-capturing group `(?:...)` does not consume number

### Back-references

**XSD**: Not supported
**XPath 2.0+**: 
- Syntax: `\1` through `\N` (or `\10` through `\NN`)
- Invalid if N > number of capturing groups before reference
- NOT valid inside character class `[...]`
- If group not matched, back-reference matches zero-length string

### Quote Mode (`q` flag)

**XSD**: Not applicable
**XPath 2.0**: Not applicable
**XPath 3.0+**: 
- All characters treated as literals
- Pattern has no metacharacters
- In `fn:replace()`: `$` and `\` lose special meaning
- Works with `i` flag but cancels `m`, `s`, `x` effects

### Case-Insensitive Matching (`i` flag)

All versions 2.0+ use Unicode codepoint collation:
```
fn:lower-case(C1) eq fn:lower-case(C2) 
    or 
fn:upper-case(C1) eq fn:upper-case(C2)
```

Character ranges in `i` mode include case-variants (e.g., `[A-Z]` matches a-z plus special case-folding chars).

### Verbose Mode (`x` flag)

Removes whitespace `#x9`, `#xA`, `#xD`, `#x20` from pattern EXCEPT:
- Inside character class `[...]`
- Before escaped space `\ ` (treated as literal space)

## Error Behavior

### FORX0001: Invalid flags argument

Raised when:
- Flag string contains unknown flag letter
- Flag used with incompatible function version

### FORX0002: Invalid regular expression pattern

Raised when:
- Syntax error in pattern
- Invalid character range (e.g., `z-a` where z > a codepoint)
- Invalid Unicode block name (XPath 3.0+)
- Invalid category escape

### FORX0003: Regular expression matches zero-length string

Raised when:
- Pattern can match empty string in `fn:tokenize()` or `fn:replace()`
- Would cause infinite loop in split operation

### FORX0004: Invalid replacement string

Raised when:
- `fn:replace()` replacement contains invalid `$` or `\` usage
- Not raised if `q` flag present (quote mode makes all literals)

## Semantic Summary Table

| Aspect | XSD 1.0 | XPath 2.0 | XPath 3.0 | XPath 3.1 |
|--------|---------|-----------|-----------|-----------|
| Implicit anchoring | ✓ | ✗ | ✗ | ✗ |
| `^` and `$` support | ✗ | ✓ | ✓ | ✓ |
| Reluctant quantifiers | ✗ | ✓ | ✓ | ✓ |
| Capturing groups | No semantics | ✓ | ✓ | ✓ |
| Non-capturing groups | N/A | ✗ | ✓ | ✓ |
| Back-references | ✗ | ✓ | ✓ | ✓ |
| `q` flag | N/A | ✗ | ✓ | ✓ |
| `.` excludes `\r` | ✗ | ✗ | ✓ | ✓ |
| `fn:analyze-string()` | N/A | ✗ | ✓ | ✓ |
| Unicode block validation | N/A | Impl-def | ✓ (error) | ✓ (error) |
