# Regular Expression Specification Reference

**Primary Sources**:
- **XSD**: XML Schema Part 2: Datatypes Second Edition (W3C Recommendation 28 October 2004), Appendix F
- **XPath 3.1**: XPath and XQuery Functions and Operators 3.1 (W3C Recommendation 21 March 2017), Section 5.6

**Target Implementations**: `fn:matches()`, `fn:replace()`, `fn:tokenize()`, `fn:analyze-string()`

---

## Overview

A **regular expression** is a sequence of characters that denotes a **set of strings** L(R).  
When used to constrain a lexical space, a regular expression R asserts that only strings in L(R) are valid literals for values of that type.

### Key Characteristics

- **Implicit Anchoring**: Regular expressions are implicitly anchored at head and tail
- **Intended for Literal Matching**: Designed to match entire literals, not substrings
- **Level 1 Support**: Targets "Level 1" features from Unicode Regular Expression Guidelines

### Anchoring Behavior

Unlike Perl and standard Unix utilities, XSD regular expressions are **implicitly anchored**:
- Patterns automatically match from start to end of string
- No need for `^` (head anchor) or `$` (tail anchor)
- To achieve unanchored matching, use `.*` prefix/suffix

**Example - Anchored (XSD)**:
```xml
<pattern value='A.*Z'/>  <!-- Matches strings starting with A and ending with Z -->
```

**Equivalent in other languages**:
```
^A.*Z$
```

**Example - Unanchored (XSD)**:
```xml
<pattern value='.*AAA.*'/>  <!-- Matches strings containing AAA anywhere -->
```

---

## Grammar

### Regular Expression

```
[1] regExp ::= branch ( '|' branch )*
```

**Semantics**:
- Empty string: the set containing just the empty string
- S: all strings in L(S)
- S|T: all strings in L(S) and all strings in L(T)

### Branch

```
[2] branch ::= piece*
```

**Semantics**:
- S: all strings in L(S)
- ST: all strings st with s in L(S) and t in L(T) (concatenation)

### Piece

```
[3] piece ::= atom quantifier?
```

**Semantics**: An atom possibly followed by a quantifier

### Quantifiers

```
[4] quantifier   ::= [?*+] | ( '{' quantity '}' )
[5] quantity     ::= quantRange | quantMin | QuantExact
[6] quantRange   ::= QuantExact ',' QuantExact
[7] quantMin     ::= QuantExact ','
[8] QuantExact   ::= [0-9]+
```

**Quantifier Meanings**:

| Quantifier | Meaning |
|------------|---------|
| `S?` | Empty string and all strings in L(S) (zero or one) |
| `S*` | All concatenations of zero or more strings from L(S) |
| `S+` | All concatenations of one or more strings from L(S) |
| `S{n,m}` | All sequences of at least n, and at most m, strings from L(S) |
| `S{n}` | All sequences of exactly n strings from L(S) |
| `S{n,}` | All sequences of at least n strings from L(S) |
| `S{0,m}` | All sequences of at most m strings from L(S) |
| `S{0,0}` | The set containing only the empty string |

**Note**: The form `S{,m}` is NOT supported (unlike some implementations)

### Atom

```
[9] atom ::= Char | charClass | ( '(' regExp ')' )
```

**Semantics**:
- `c`: single string consisting only of c (normal character)
- `C`: all strings in L(C) (character class)
- `(S)`: all strings in L(S) (grouped expression)

### Characters

```
[10] Char ::= [^.\?*+()|#x5B#x5D]
```

**Metacharacters**: `. \ ? * + { } ( ) [ ]`
- Have special meanings in regular expressions
- Must be escaped with `\` to match literally
- Escaped metacharacters behave like normal characters

**Normal Characters**:
- Any XML character that is not a metacharacter
- Can be represented directly or with character reference

---

## Character Classes

A **character class** is an atom that identifies a **set of characters** C(R).  
The set of strings L(R) contains one single-character string "c" for each character c in C(R).

```
[11] charClass ::= charClassEsc | charClassExpr | WildcardEsc
```

### Character Class Expression

```
[12] charClassExpr ::= '[' charGroup ']'
[13] charGroup     ::= posCharGroup | negCharGroup | charClassSub
```

#### Positive Character Group

```
[14] posCharGroup ::= ( charRange | charClassEsc )+
```

Contains all characters from all constituent ranges or escapes.

#### Negative Character Group

```
[15] negCharGroup ::= '^' posCharGroup
```

Contains all XML characters that are NOT in the positive character group.

#### Character Class Subtraction

```
[16] charClassSub ::= ( posCharGroup | negCharGroup ) '-' charClassExpr
```

For G-C: all characters in C(G) that are not in C(C).

### Character Range

```
[17] charRange        ::= seRange | XmlCharIncDash
[18] seRange          ::= charOrEsc '-' charOrEsc
[20] charOrEsc        ::= XmlChar | SingleCharEsc
[21] XmlChar          ::= [^\#x2D#x5B#x5D]
[22] XmlCharIncDash   ::= [^\#x5B#x5D]
```

**Rules**:
- Single XML character identifies set containing only itself
- `[`, `]`, `-`, `\` are not valid character ranges (must be escaped)
- `^` only valid at beginning of positive character group in negative group
- `-` valid only at beginning or end of positive character group
- Form `s-e` identifies all characters with code points from s to e (inclusive)

**Validity Requirements for `s-e`**:
- s is single character escape or XML character (not `\`)
- If at start of expression, s is not `^`
- e is single character escape or XML character (not `\` or `[`)
- Code point of e >= code point of s

---

## Character Class Escapes

```
[23] charClassEsc ::= SingleCharEsc | MultiCharEsc | catEsc | complEsc
```

### Single Character Escapes

```
[24] SingleCharEsc ::= '\' [nrt\|.?*+(){}#x2D#x5B#x5D#x5E]
```

| Escape | Character |
|--------|-----------|
| `\n` | newline (#xA) |
| `\r` | return (#xD) |
| `\t` | tab (#x9) |
| `\\` | \ |
| `\|` | \| |
| `\.` | . |
| `\-` | - |
| `\^` | ^ |
| `\?` | ? |
| `\*` | * |
| `\+` | + |
| `\{` | { |
| `\}` | } |
| `\(` | ( |
| `\)` | ) |
| `\[` | [ |
| `\]` | ] |

### Multi-Character Escapes

```
[37]  MultiCharEsc ::= '\' [sSiIcCdDwW]
[37a] WildcardEsc  ::= '.'
```

| Escape | Equivalent Character Class | Description |
|--------|---------------------------|-------------|
| `.` | `[^\n\r]` | Any character except newline/return |
| `\s` | `[#x20\t\n\r]` | Whitespace characters |
| `\S` | `[^\s]` | Non-whitespace |
| `\i` | `Letter \| '_' \| ':'` | Initial name characters |
| `\I` | `[^\i]` | Non-initial name characters |
| `\c` | `NameChar` | Name characters |
| `\C` | `[^\c]` | Non-name characters |
| `\d` | `\p{Nd}` | Decimal digits |
| `\D` | `[^\d]` | Non-digits |
| `\w` | `[#x0000-#x10FFFF]-[\p{P}\p{Z}\p{C}]` | Word characters (all except punctuation, separator, other) |
| `\W` | `[^\w]` | Non-word characters |

### Category Escapes

```
[25] catEsc   ::= '\p{' charProp '}'
[26] complEsc ::= '\P{' charProp '}'
[27] charProp ::= IsCategory | IsBlock
```

**Unicode General Categories**:

```
[28] IsCategory  ::= Letters | Marks | Numbers | Punctuation | Separators | Symbols | Others
[29] Letters     ::= 'L' [ultmo]?
[30] Marks       ::= 'M' [nce]?
[31] Numbers     ::= 'N' [dlo]?
[32] Punctuation ::= 'P' [cdseifo]?
[33] Separators  ::= 'Z' [slp]?
[34] Symbols     ::= 'S' [mcko]?
[35] Others      ::= 'C' [cfon]?
```

**General Category Properties**:

| Category | Property | Meaning |
|----------|----------|---------|
| **Letters** | L | All Letters |
| | Lu | uppercase |
| | Ll | lowercase |
| | Lt | titlecase |
| | Lm | modifier |
| | Lo | other |
| **Marks** | M | All Marks |
| | Mn | nonspacing |
| | Mc | spacing combining |
| | Me | enclosing |
| **Numbers** | N | All Numbers |
| | Nd | decimal digit |
| | Nl | letter |
| | No | other |
| **Punctuation** | P | All Punctuation |
| | Pc | connector |
| | Pd | dash |
| | Ps | open |
| | Pe | close |
| | Pi | initial quote |
| | Pf | final quote |
| | Po | other |
| **Separators** | Z | All Separators |
| | Zs | space |
| | Zl | line |
| | Zp | paragraph |
| **Symbols** | S | All Symbols |
| | Sm | math |
| | Sc | currency |
| | Sk | modifier |
| | So | other |
| **Other** | C | All Others |
| | Cc | control |
| | Cf | format |
| | Co | private use |
| | Cn | not assigned |

**Note**: Excludes `Cs` (surrogate) property - surrogates don't occur at XML character abstraction level.

### Block Escapes

```
[36] IsBlock ::= 'Is' [a-zA-Z0-9#x2D]+
```

**Format**: `\p{IsBlockName}` or `\P{IsBlockName}` (complement)

**Unicode Block Names** (spaces stripped):

| Start Code | End Code | Block Name |
|------------|----------|------------|
| #x0000 | #x007F | BasicLatin |
| #x0080 | #x00FF | Latin-1Supplement |
| #x0100 | #x017F | LatinExtended-A |
| #x0180 | #x024F | LatinExtended-B |
| #x0250 | #x02AF | IPAExtensions |
| #x02B0 | #x02FF | SpacingModifierLetters |
| #x0300 | #x036F | CombiningDiacriticalMarks |
| #x0370 | #x03FF | Greek |
| #x0400 | #x04FF | Cyrillic |
| #x0530 | #x058F | Armenian |
| #x0590 | #x05FF | Hebrew |
| #x0600 | #x06FF | Arabic |
| #x0700 | #x074F | Syriac |
| #x0780 | #x07BF | Thaana |
| #x0900 | #x097F | Devanagari |
| #x0980 | #x09FF | Bengali |
| #x0A00 | #x0A7F | Gurmukhi |
| #x0A80 | #x0AFF | Gujarati |
| #x0B00 | #x0B7F | Oriya |
| #x0B80 | #x0BFF | Tamil |
| #x0C00 | #x0C7F | Telugu |
| #x0C80 | #x0CFF | Kannada |
| #x0D00 | #x0D7F | Malayalam |
| #x0D80 | #x0DFF | Sinhala |
| #x0E00 | #x0E7F | Thai |
| #x0E80 | #x0EFF | Lao |
| #x0F00 | #x0FFF | Tibetan |
| #x1000 | #x109F | Myanmar |
| #x10A0 | #x10FF | Georgian |
| #x1100 | #x11FF | HangulJamo |
| #x1200 | #x137F | Ethiopic |
| #x13A0 | #x13FF | Cherokee |
| #x1400 | #x167F | UnifiedCanadianAboriginalSyllabics |
| #x1680 | #x169F | Ogham |
| #x16A0 | #x16FF | Runic |
| #x1780 | #x17FF | Khmer |
| #x1800 | #x18AF | Mongolian |
| #x1E00 | #x1EFF | LatinExtendedAdditional |
| #x1F00 | #x1FFF | GreekExtended |
| #x2000 | #x206F | GeneralPunctuation |
| #x2070 | #x209F | SuperscriptsandSubscripts |
| #x20A0 | #x20CF | CurrencySymbols |
| #x20D0 | #x20FF | CombiningMarksforSymbols |
| #x2100 | #x214F | LetterlikeSymbols |
| #x2150 | #x218F | NumberForms |
| #x2190 | #x21FF | Arrows |
| #x2200 | #x22FF | MathematicalOperators |
| #x2300 | #x23FF | MiscellaneousTechnical |
| #x2400 | #x243F | ControlPictures |
| #x2440 | #x245F | OpticalCharacterRecognition |
| #x2460 | #x24FF | EnclosedAlphanumerics |
| #x2500 | #x257F | BoxDrawing |
| #x2580 | #x259F | BlockElements |
| #x25A0 | #x25FF | GeometricShapes |
| #x2600 | #x26FF | MiscellaneousSymbols |
| #x2700 | #x27BF | Dingbats |
| #x2800 | #x28FF | BraillePatterns |
| #x2E80 | #x2EFF | CJKRadicalsSupplement |
| #x2F00 | #x2FDF | KangxiRadicals |
| #x2FF0 | #x2FFF | IdeographicDescriptionCharacters |
| #x3000 | #x303F | CJKSymbolsandPunctuation |
| #x3040 | #x309F | Hiragana |
| #x30A0 | #x30FF | Katakana |
| #x3100 | #x312F | Bopomofo |
| #x3130 | #x318F | HangulCompatibilityJamo |
| #x3190 | #x319F | Kanbun |
| #x31A0 | #x31BF | BopomofoExtended |
| #x3200 | #x32FF | EnclosedCJKLettersandMonths |
| #x3300 | #x33FF | CJKCompatibility |
| #x3400 | #x4DB5 | CJKUnifiedIdeographsExtensionA |
| #x4E00 | #x9FFF | CJKUnifiedIdeographs |
| #xA000 | #xA48F | YiSyllables |
| #xA490 | #xA4CF | YiRadicals |
| #xAC00 | #xD7A3 | HangulSyllables |
| #xE000 | #xF8FF | PrivateUse |
| #xF900 | #xFAFF | CJKCompatibilityIdeographs |
| #xFB00 | #xFB4F | AlphabeticPresentationForms |
| #xFB50 | #xFDFF | ArabicPresentationForms-A |
| #xFE20 | #xFE2F | CombiningHalfMarks |
| #xFE30 | #xFE4F | CJKCompatibilityForms |
| #xFE50 | #xFE6F | SmallFormVariants |
| #xFE70 | #xFEFE | ArabicPresentationForms-B |
| #xFEFF | #xFEFF | Specials |
| #xFF00 | #xFFEF | HalfwidthandFullwidthForms |
| #xFFF0 | #xFFFD | Specials |

**Note**: Excludes `HighSurrogates`, `LowSurrogates`, `HighPrivateUseSurrogates` blocks.

**Example**: ASCII characters: `\p{IsBasicLatin}`

---

## Examples

### Basic Pattern Matching

```xml
<!-- Match strings starting with A and ending with Z -->
<simpleType name='myString'>
 <restriction base='string'>
  <pattern value='A.*Z'/>
 </restriction>
</simpleType>
```

### Unanchored Pattern

```xml
<!-- Match strings containing AAA anywhere -->
<simpleType name='myString'>
 <restriction base='string'>
  <pattern value='.*AAA.*'/>
 </restriction>
</simpleType>
```

### Character Classes

```
[0-9]+           # One or more digits
[a-zA-Z_]\w*     # C-style identifier
[\p{L}\p{N}]+    # Letters and numbers
[^\p{P}]+        # Not punctuation
```

### Quantifiers

```
\d{3}-\d{2}-\d{4}      # SSN format: 123-45-6789
[a-z]{1,5}             # 1 to 5 lowercase letters
\w+@\w+\.\w{2,4}       # Simple email pattern
```

---

## XPath 3.1 Extensions to XSD Regex

XPath 3.1 extends XSD regex syntax with features needed for text processing.

### 5.6.1.1 Start/End Anchors: `^` and `$`

**Default Behavior**:
- `^` matches start of entire string
- `$` matches end of entire string  
- Newline = `#x0A` only

**Multi-line Mode** (flag `m`):
- `^` matches: start of string OR position after `#x0A`
- `$` matches: end of string OR position before `#x0A` (except final `#x0A`)

**Grammar Changes**:
```
[10] Char ::= [^.\?*+{}()|^$#x5B#x5D]  // Added ^$ to metacharacters
[11] charClass ::= charClassEsc | charClassExpr | WildcardEsc | "^" | "$"
[24] SingleCharEsc ::= '\' [nrt\|.?*+(){}$#x2D#x5B#x5D#x5E]  // Added $
```

**Note**: XSD regex implicitly anchored. XPath regex NOT anchored unless `^`/`$` used.

### 5.6.1.2 Reluctant (Non-greedy) Quantifiers

**Syntax**: Add `?` after quantifier

| Greedy | Reluctant | Meaning |
|--------|-----------|----------|
| `X?` | `X??` | Zero or one, prefer zero |
| `X*` | `X*?` | Zero or more, prefer fewer |
| `X+` | `X+?` | One or more, prefer fewer |
| `X{n}` | `X{n}?` | Exactly n (no effect) |
| `X{n,}` | `X{n,}?` | At least n, prefer fewer |
| `X{n,m}` | `X{n,m}?` | n to m, prefer fewer |

**Behavior**: Match shortest substring consistent with overall success.

**Grammar Change**:
```
[4] quantifier ::= ( [?*+] | ( '{' quantity'}' ) ) '?'?
```

**Note**: No effect on `fn:matches()` (boolean result only).

### 5.6.1.3 Capturing Sub-expressions

**Capturing Group**: `(pattern)`  
**Non-capturing Group**: `(?:pattern)`

**Numbering**: Left-to-right by opening `(` position (1-indexed).  
**Substring 0**: Entire match.

**Example**: `A(BC(?:D(EF(GH[()]))))`
- Group 1: `BC(?:D(EF(GH[()])))`
- Group 2: `EF(GH[()])`  
- Group 3: `GH[()]`

**Repeated Captures**: Only last matched substring retained.

**Grammar Changes**:
```
[9] atom ::= Char | charClass | ( '(' '?:'? regExp ')' ) | backReference
```

### 5.6.1.4 Back-references

**Syntax**: `\N` where N = 1-9 followed by optional digits

**Rules**:
- `\N` matches same string as Nth captured substring
- Invalid if N > number of capturing groups before back-reference
- If group not matched, back-reference matches zero-length string
- NOT valid inside character class `[...]`

**Example**: `('|").*\1` matches strings with matching quote delimiters.

**Grammar Addition**:
```
[9a] backReference ::= "\" [1-9][0-9]*
```

### 5.6.1.5 Unicode Block Names

**Invalid Block Name**: Dynamic error [FORX0002]

**Example**: `\p{IsBadBlockName}` → error if block undefined

**Note**: XSD 1.0 unspecified; XSD 1.1 treats all characters as matching.

---

## XPath 3.1 Regex Flags

All regex functions (`fn:matches`, `fn:replace`, `fn:tokenize`, `fn:analyze-string`) accept optional `$flags` parameter.

**Format**: `xs:string` with option letters (any order, repeatable)  
**Invalid flag**: Dynamic error [FORX0001]

### Flag Definitions

#### `s` - Dot-all (Single-line) Mode

**Default**: `.` matches any character EXCEPT `#x0A` (newline) or `#x0D` (carriage return)  
**With `s`**: `.` matches ANY character including newlines

**Example**:
```xpath
fn:matches("hello\nworld", "hello.*world")      (: false :)
fn:matches("hello\nworld", "hello.*world", "s")  (: true :)
```

#### `m` - Multi-line Mode

**Default**: `^` = start of string, `$` = end of string  
**With `m`**: `^` = start of string OR after `#x0A`; `$` = before `#x0A` OR end of string

**Note**: Only `#x0A` treated as line separator (not `#x0D`).

#### `i` - Case-insensitive Mode

**Case Variant**: C2 is case-variant of C1 if:
```xpath
fn:lower-case(C1) eq fn:lower-case(C2) or fn:upper-case(C1) eq fn:upper-case(C2)
```
(using Unicode codepoint collation)

**Rules**:

1. **Normal character**: Matches itself + all case-variants  
   `"z"` matches `"z"` and `"Z"`

2. **Character range**: Includes all characters + their case-variants  
   `[A-Z]` matches A-Z, a-z, plus characters like `#x212A` (KELVIN SIGN, lower-case = "k")
   
   Also applies to:
   - Character class subtraction: `[A-Z-[IO]]` matches A,B,a,b but not I,O,i,o
   - Negative character group: `[^Q]` excludes Q and q

3. **Back-reference**: Case-blind comparison  
   `"([md])[aeiou]\1"` with `i` matches `"Mum"`, `"mom"`, `"Dad"`, `"DUD"`

4. **Other constructs**: Unaffected  
   `\p{Lu}` still matches only uppercase letters

#### `x` - Extended (Free-spacing) Mode

**Behavior**: Whitespace characters (`#x9`, `#xA`, `#xD`, `#x20`) removed EXCEPT inside `[...]`

**Use**: Break long regex into readable lines

**Examples**:
```xpath
fn:matches("helloworld", "hello world", "x")      (: true :)
fn:matches("helloworld", "hello[ ]world", "x")    (: false - space preserved in [] :)
fn:matches("hello world", "hello\ sworld", "x")  (: true - escaped space :)
fn:matches("hello world", "hello world", "x")     (: false :)
```

#### `q` - Literal (Quote) Mode

**Behavior**: ALL characters treated as literals (no metacharacter interpretation)

**Effect**:
- Every character implicitly escaped
- In `fn:replace()`: `$` and `\` lose special meaning in replacement string

**Combination**:
- OK with `i` flag
- If combined with `m`, `s`, or `x` → those flags have no effect

**Examples**:
```xpath
fn:tokenize("12.3.5.6", ".", "q")                  (: ("12","3","5","6") :)
fn:replace("a\b\c", "\", "\\", "q")           (: "a\\b\\c" :)
fn:replace("a/b/c", "/", "$", "q")                (: "a$b$c" :)
fn:matches("abcd", ".*", "q")                      (: false :)
fn:matches("Mr. B. Obama", "B. OBAMA", "iq")       (: true :)
```

---

## XPath 3.1 Regex Functions

### fn:matches

**Signature**:
```xpath
fn:matches($input as xs:string?, $pattern as xs:string) as xs:boolean
fn:matches($input as xs:string?, $pattern as xs:string, $flags as xs:string) as xs:boolean
```

**Behavior**:
- Returns `true` if `$input` or substring matches `$pattern`
- Empty sequence `$input` → empty string
- **NOT implicitly anchored** (unlike XSD validation)

**Errors**:
- [FORX0002]: Invalid `$pattern`
- [FORX0001]: Invalid `$flags`

**Examples**:
```xpath
fn:matches("abracadabra", "bra")           (: true :)
fn:matches("abracadabra", "^a.*a$")        (: true :)
fn:matches("abracadabra", "^bra")          (: false :)
fn:matches($poem, "Kaum.*krähen")          (: false - . doesn't match newline :)
fn:matches($poem, "Kaum.*krähen", "s")     (: true - dot-all mode :)
fn:matches($poem, "^Kaum.*gesehen,$", "m") (: true - multi-line mode :)
fn:matches($poem, "kiki", "i")             (: true - case-insensitive :)
```

### fn:replace

**Signature**:
```xpath
fn:replace($input as xs:string?, $pattern as xs:string, 
           $replacement as xs:string) as xs:string
fn:replace($input as xs:string?, $pattern as xs:string, 
           $replacement as xs:string, $flags as xs:string) as xs:string
```

**Behavior**:
- Replace each non-overlapping match with `$replacement`
- First match wins if overlapping
- Empty sequence `$input` → empty string

**Replacement String Variables** (unless `q` flag):
- `$0`: Entire match
- `$1` to `$S`: Captured substrings (S = number of capturing groups)
- `$N` where N > S and N > 9: Treat last digit as literal, re-evaluate
- Literal `$`: `\$`
- Literal `\`: `\\`

**Rules for `$N`**:
1. N=0 → entire match
2. 1 ≤ N ≤ S → Nth captured substring (or empty if group not matched)
3. S < N ≤ 9 → empty string
4. N > S and N > 9 → strip last digit, use as literal, re-apply rules

**Errors**:
- [FORX0002]: Invalid `$pattern`
- [FORX0001]: Invalid `$flags`
- [FORX0003]: Pattern matches zero-length string
- [FORX0004]: Invalid `$` or `\` in `$replacement` (without `q` flag)

**Examples**:
```xpath
fn:replace("abracadabra", "bra", "*")                (: "a*cada*" :)
fn:replace("abracadabra", "a.*a", "*")               (: "*" - greedy :)
fn:replace("abracadabra", "a.*?a", "*")              (: "*c*bra" - reluctant :)
fn:replace("abracadabra", "a", "")                   (: "brcdbr" :)
fn:replace("abracadabra", "a(.)", "a$1$1")           (: "abbraccaddabbra" :)
fn:replace("AAAA", "A+", "b")                        (: "b" - greedy :)
fn:replace("AAAA", "A+?", "b")                       (: "bbbb" - reluctant :)
fn:replace("darted", "^(.*?)d(.*)$", "$1c$2")        (: "carted" :)
fn:replace("abcd", "(ab)|(a)", "[1=$1][2=$2]")       (: "[1=ab][2=]cd" :)
```

### fn:tokenize

**Signature**:
```xpath
fn:tokenize($input as xs:string?) as xs:string*
fn:tokenize($input as xs:string?, $pattern as xs:string) as xs:string*
fn:tokenize($input as xs:string?, $pattern as xs:string, $flags as xs:string) as xs:string*
```

**Behavior**:
- One-argument form: Split on whitespace (after `fn:normalize-space()`), equivalent to `fn:tokenize(fn:normalize-space($input), ' ')`
- Multi-argument: Split on substrings matching `$pattern`
- Empty sequence or empty string → empty sequence
- Separators at start/end/adjacent → zero-length strings in result (except one-argument form)

**Errors**:
- [FORX0002]: Invalid `$pattern`
- [FORX0001]: Invalid `$flags`
- [FORX0003]: Pattern matches zero-length string

**Examples**:
```xpath
fn:tokenize(" red green blue ")                         (: ("red","green","blue") :)
fn:tokenize("The cat sat on the mat", "\s+")           (: ("The","cat","sat","on","the","mat") :)
fn:tokenize(" red green blue ", "\s+")                 (: ("","red","green","blue","") :)
fn:tokenize("1, 15, 24, 50", ",\s*")                  (: ("1","15","24","50") :)
fn:tokenize("1,15,,24,50,", ",")                        (: ("1","15","","24","50","") :)
fn:tokenize("Some unparsed <br> HTML", "\s*<br>\s*", "i")  (: ("Some unparsed","HTML","text") :)
fn:tokenize("abba", ".?")                               (: error FORX0003 :)
```

**Note**: One-argument separator = `[\t\n\r ]` (tab, newline, carriage return, space) - same as XSD list-valued attributes, NOT HTML5 (which includes form-feed `#x0C`).

### fn:analyze-string

**Signature**:
```xpath
fn:analyze-string($input as xs:string?, $pattern as xs:string) 
  as element(fn:analyze-string-result)
fn:analyze-string($input as xs:string?, $pattern as xs:string, $flags as xs:string) 
  as element(fn:analyze-string-result)
```

**Behavior**:
- Returns XML structure identifying matched/non-matched parts + captured groups
- Empty sequence `$input` → element with no children
- Namespace URI: `http://www.w3.org/2005/xpath-functions`
- Prefix: implementation-dependent
- **Non-deterministic** w.r.t. node identity (may return same or distinct nodes)
- Base URI: implementation-dependent
- Typed or untyped: implementation-defined

**Result Structure**:
```xml
<analyze-string-result>
  (<match> | <non-match>)*
</analyze-string-result>

<match>
  (text | <group nr="N">)*
</match>

<non-match>
  text
</non-match>

<group nr="N">
  (text | <group nr="M">)*
</group>
```

**Matching Algorithm**:
1. Find first match (leftmost starting position)
2. If multiple alternatives match at same position, choose first
3. Repeat from first character after previous match
4. Partition input into matched/non-matched substrings

**Schema**: See `analyze-string.xsd` (http://www.w3.org/2005/xpath-functions namespace)

**Errors**:
- [FORX0002]: Invalid `$pattern`
- [FORX0001]: Invalid `$flags`
- [FORX0003]: Pattern matches zero-length string

**Examples**:
```xpath
fn:analyze-string("The cat sat on the mat.", "\w+")
```
Returns:
```xml
<analyze-string-result xmlns="http://www.w3.org/2005/xpath-functions">
  <match>The</match>
  <non-match> </non-match>
  <match>cat</match>
  <non-match> </non-match>
  <match>sat</match>
  <non-match> </non-match>
  <match>on</match>
  <non-match> </non-match>
  <match>the</match>
  <non-match> </non-match>
  <match>mat</match>
  <non-match>.</non-match>
</analyze-string-result>
```

```xpath
fn:analyze-string("2008-12-03", "^(\d+)\-(\d+)\-(\d+)$")
```
Returns:
```xml
<analyze-string-result xmlns="http://www.w3.org/2005/xpath-functions">
  <match>
    <group nr="1">2008</group>-<group nr="2">12</group>-<group nr="3">03</group>
  </match>
</analyze-string-result>
```

```xpath
fn:analyze-string("A1,C15,,D24, X50,", "([A-Z])([0-9]+)")
```
Returns:
```xml
<analyze-string-result xmlns="http://www.w3.org/2005/xpath-functions">
  <match><group nr="1">A</group><group nr="2">1</group></match>
  <non-match>,</non-match>
  <match><group nr="1">C</group><group nr="2">15</group></match>
  <non-match>,,</non-match>
  <match><group nr="1">D</group><group nr="2">24</group></match>
  <non-match>, </non-match>
  <match><group nr="1">X</group><group nr="2">50</group></match>
  <non-match>,</non-match>
</analyze-string-result>
```

**Note**: Schema allows mixed content in `analyze-string-result` for atomization (returns original input string).

---

## Error Codes

| Code | Description |
|------|-------------|
| **FORX0001** | Invalid flags argument |
| **FORX0002** | Invalid regular expression pattern |
| **FORX0003** | Regular expression matches zero-length string |
| **FORX0004** | Invalid replacement string (invalid `$` or `\` usage) |

---

## Implementation Notes

### XSD vs XPath Differences

| Feature | XSD | XPath 3.1 |
|---------|-----|-----------|
| **Anchoring** | Implicit (`^...$`) | Explicit (add `^`/`$` if needed) |
| **Purpose** | Validation (entire lexical space) | Text processing (substring matching) |
| **Metacharacters** | `. \ ? * + { } ( ) [ ]` | `+ ^ $ . \ ? * + { } ( ) [ ]` |
| **Quantifiers** | Greedy only | Greedy + reluctant (`?` suffix) |
| **Groups** | No special meaning | Capturing + non-capturing (`(?:...)`) |
| **Back-references** | Not supported | Supported (`\1` to `\9...`) |
| **Functions** | N/A | `matches`, `replace`, `tokenize`, `analyze-string` |

### Compatibility Levels

This library supports multiple compatibility modes:
- **XSD**: Base regex syntax (validation-oriented)
- **XPath 3.0**: XSD + extensions (deprecated - use 3.1)
- **XPath 3.1**: Full feature set (current standard)

### Unicode Support

- **Collation**: Regular expressions use codepoint comparison (no collation)
- **Case folding**: Uses `fn:lower-case()` / `fn:upper-case()` on Unicode codepoints
- **Character properties**: Based on Unicode version (implementation-defined)
- **Level**: Targets Unicode Regular Expression Guidelines (UTS #18) "Level 1"
- **Limitations**: 
  - No general solution for combining character sequences
  - Base character + combining marks NOT easily matchable

### Schema Validation

The `fn:analyze-string` result conforms to `analyze-string.xsd` schema, but typing is implementation-defined:
- **Typed**: Elements have type annotations from schema validation
- **Untyped**: Elements are `xs:untyped`, attributes `xs:untypedAtomic`

**Recommendation**: Schema-aware processors SHOULD return typed nodes.

---
## Limitations

**Important Note**: This regex language does NOT provide:
- General solution for all Unicode character sequences
- Easy matching of combining character sequences
- Full support for base characters + combining marks

**Target**: Unicode Regular Expression Guidelines "Level 1" features only.

---

## References

- **XPath 3.1 Spec**: [XPath and XQuery Functions and Operators 3.1](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/)
- **XSD Spec**: [XML Schema Part 2: Datatypes Second Edition](https://www.w3.org/TR/2004/REC-xmlschema-2-20041028/)
- **XSD 1.1 Spec**: [W3C XML Schema Definition Language (XSD) 1.1 Part 2: Datatypes](https://www.w3.org/TR/xmlschema11-2/)
- **Unicode Database**: [Unicode Character Database](http://www.unicode.org/ucd/)
- **Unicode Regex Guidelines**: [UTS #18: Unicode Regular Expressions](http://www.unicode.org/reports/tr18/)
- **RFC 3986**: [Uniform Resource Identifier (URI): Generic Syntax](https://www.rfc-editor.org/rfc/rfc3986)
- **RFC 3987**: [Internationalized Resource Identifiers (IRIs)](https://www.rfc-editor.org/rfc/rfc3987)
