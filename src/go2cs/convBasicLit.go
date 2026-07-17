package main

import (
	"fmt"
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"math"
	"regexp"
	"strconv"
	"strings"
)

var octalCharRegex *regexp.Regexp

func getOctalCharRegex() *regexp.Regexp {
	if octalCharRegex == nil {
		octalCharRegex = regexp.MustCompile(`\\[0-7][0-7][0-7]`)
	}

	return octalCharRegex
}

// stringLiteralNeedsByteArray reports whether a Go interpreted (double-quoted) string literal token
// must be emitted as a byte-array-backed @string rather than a C# string/u8 literal. It scans the
// token's `\xHH` escapes — Go's `\x` is EXACTLY two hex digits denoting one raw byte; C#'s `\x` is a
// GREEDY 1-to-4-hex-digit code-UNIT escape, and a C# `"…"u8` literal UTF-8-re-encodes its content.
// An escape forces the byte-array form when EITHER:
//
//	(1) its byte value is >= 0x80 — a C# string/u8 literal UTF-8-re-encodes such a byte to a
//	    multi-byte sequence (or, when the escape greedily forms a lone surrogate, fails to encode at
//	    all — CS9026), so @string byte indexing / len would not match Go; or
//	(2) it is immediately followed by a third hex digit — C# greedily folds e.g. `\xdb50` into the
//	    single code unit U+DB50, changing the decoded content even if every resulting byte is ASCII.
//
// Only `\xHH` ESCAPES are inspected: a literal written with actual UTF-8 characters (`"Michał"`,
// `"白鵬翔"`) round-trips exactly through C#'s `"…"u8` encoding and keeps the readable string form, as
// does an all-ASCII escape sequence with no greedy extension (image/jpeg's `"\x00\x10\x01\x11"u8[i]`).
// Only raw-byte data expressed with high/greedy `\x` escapes (zip blobs, embedded tzdata) is routed
// to the byte array. A raw (backtick) literal has no escapes and never trips this (checked by caller).
func stringLiteralNeedsByteArray(token string) bool {
	if _, err := strconv.Unquote(token); err != nil {
		return false
	}

	backslashes := 0

	for i := 0; i < len(token); i++ {
		c := token[i]

		if c == '\\' {
			backslashes++
			continue
		}

		if (c == 'x' || c == 'X') && backslashes%2 == 1 && i+2 < len(token) && isHexDigit(token[i+1]) && isHexDigit(token[i+2]) {
			b := hexValue(token[i+1])<<4 | hexValue(token[i+2])

			if b >= 0x80 || (i+3 < len(token) && isHexDigit(token[i+3])) {
				return true
			}
		}

		backslashes = 0
	}

	return false
}

// isHexDigit reports whether b is an ASCII hexadecimal digit.
func isHexDigit(b byte) bool {
	return (b >= '0' && b <= '9') || (b >= 'a' && b <= 'f') || (b >= 'A' && b <= 'F')
}

// hexValue returns the numeric value (0-15) of an ASCII hex digit; callers guard with isHexDigit.
func hexValue(b byte) int {
	switch {
	case b >= '0' && b <= '9':
		return int(b - '0')
	case b >= 'a' && b <= 'f':
		return int(b-'a') + 10
	default:
		return int(b-'A') + 10
	}
}

// emitByteArrayString decodes a Go interpreted string literal token to its exact byte sequence and
// emits a PARENTHESIZED byte-array-backed C# @string — `((@string)(new byte[]{ 0xNN, ... }))`. This
// is the faithful representation for a Go string holding raw bytes (see stringLiteralNeedsByteArray):
// it preserves the exact bytes that C#'s UTF-16 string literal / greedy `\x` escape would otherwise
// mangle, and the @string's byte indexing (`s[i]`) then matches Go. The outer parentheses are load-
// bearing: an INLINE-indexed literal (`"…"[i]`) would otherwise bind `[i]` to the inner `byte[]`
// (postfix `[]` outranks the cast), indexing the raw array instead of the @string. Returns
// ("", false) if the token cannot be decoded as a Go string literal (caller falls back to the
// ordinary string-literal path).
func emitByteArrayString(token string) (string, bool) {
	decoded, err := strconv.Unquote(token)

	if err != nil {
		return "", false
	}

	builder := &strings.Builder{}
	builder.WriteString("((@string)(new byte[]{")

	for i := 0; i < len(decoded); i++ {
		if i > 0 {
			builder.WriteString(", ")
		}

		fmt.Fprintf(builder, "0x%02x", decoded[i])
	}

	builder.WriteString("}))")

	return builder.String(), true
}

func replaceOctalChars(value string) string {
	octals := getOctalCharRegex().FindAllString(value, -1)

	if len(octals) > 0 {
		for _, octal := range octals {
			decimal, err := strconv.ParseInt(octal[1:], 8, 64)

			if err == nil {
				if decimal <= 0xFFFF {
					value = strings.Replace(value, octal, fmt.Sprintf("\\u%04x", decimal), 1)
				} else {
					value = strings.Replace(value, octal, fmt.Sprintf("\\U%08x", decimal), 1)
				}
			} else {
				showWarning("Failed to parse octal literal \\%s: %s", octal, err)
			}
		}
	}

	return value
}

// floatLiteralSourceText returns the SOURCE text of a float-constant initializer that is a
// plain literal — an ast.BasicLit, optionally under a single unary sign (`-7.05306122448979611050e-01`
// parses as a UnaryExpr over the literal) — or "" when the initializer is a folded expression
// with no single literal form.
func floatLiteralSourceText(expr ast.Expr) string {
	sign := ""

	if unary, ok := expr.(*ast.UnaryExpr); ok {
		switch unary.Op {
		case token.SUB, token.ADD:
			sign = unary.Op.String()
			expr = unary.X
		default:
			return ""
		}
	}

	if lit, ok := expr.(*ast.BasicLit); ok && lit.Kind == token.FLOAT {
		return sign + lit.Value
	}

	return ""
}

// isValidCSharpRealLiteral reports whether a Go decimal float literal's source text is ALSO a
// valid C# real-literal, so it can be emitted verbatim. Go and C# share the decimal forms —
// digits with optional fraction, `e`/`E` exponent, and `_` digit separators (both languages
// restrict separators to between digits) — but Go additionally allows hex floats (`0x1p-2`)
// and a bare trailing dot (`5.`, `5.e2`), which C# does not.
func isValidCSharpRealLiteral(lit string) bool {
	body := lit

	if len(body) > 0 && (body[0] == '+' || body[0] == '-') {
		body = body[1:]
	}

	if len(body) == 0 {
		return false
	}

	if len(body) > 1 && body[0] == '0' && (body[1] == 'x' || body[1] == 'X') {
		return false
	}

	if i := strings.IndexByte(body, '.'); i != -1 && (i+1 >= len(body) || body[i+1] < '0' || body[i+1] > '9') {
		return false
	}

	return true
}

// exactFloatConstString returns the EXACT C# value text for a float-kind constant declaration.
// go/constant's Value.String() is a SHORTENED human-readable form (~6 significant digits), so
// emitting it silently truncated the COMPILED value (math cbrt's `C = 0.542857` — the exact
// 5.42857142857142815906e-01 survived only in the `/* … */` comment). Preference order:
//  1. The Go source literal VERBATIM when it is also valid C# syntax and parses to the same
//     value — the declaration then reads exactly like the Go source, and the comment-elision
//     check sees constVal == orgExpr and drops the now-redundant `/* original */` comment.
//  2. The shortest round-trip form (strconv.FormatFloat 'g'/-1) at the declaration's width —
//     bitSize 32 for a float32-typed const (the appended `f` suffix then parses with the same
//     single rounding Go applies converting the exact constant), 64 otherwise. This covers
//     folded const expressions and Go-only literal forms (hex floats, trailing-dot).
//
// A beyond-float64 value keeps the shortened String() form so the GoUntyped overflow path
// (strconv.ParseFloat fails → writeUntypedConst) still triggers exactly as before.
func exactFloatConstString(val constant.Value, source ast.Expr, isFloat32 bool) string {
	f64, _ := constant.Float64Val(val)

	if math.IsInf(f64, 0) {
		return val.String()
	}

	bitSize := 64
	target := f64

	if isFloat32 {
		bitSize = 32
		f32, _ := constant.Float32Val(val)
		target = float64(f32)
	}

	if source != nil {
		if lit := floatLiteralSourceText(source); lit != "" && isValidCSharpRealLiteral(lit) {
			if parsed, err := strconv.ParseFloat(lit, bitSize); err == nil && parsed == target {
				return lit
			}
		}
	}

	return strconv.FormatFloat(target, 'g', -1, bitSize)
}

// preserveGoIntLiteral returns the literal's SOURCE text when it is also a valid C# integer
// literal denoting the same value — hex `0x…`, binary `0b…`, and decimal, each with optional
// `_` digit separators — keeping the developer's own formatting (the visually-similar goal:
// `0x4000` must not flatten to `16384`). Go-only forms re-render as the decimal fallback:
// `0o…` octal has no C# syntax, and a LEGACY leading-zero octal (`0755`) would silently
// re-bind as decimal 755 in C#.
func preserveGoIntLiteral(source string, decimal string) string {
	if len(source) > 1 && source[0] == '0' {
		if c := source[1]; c == 'x' || c == 'X' || c == 'b' || c == 'B' {
			return source
		}

		return decimal
	}

	return source
}

func (v *Visitor) convBasicLit(basicLit *ast.BasicLit, context BasicLitContext) string {
	result := &strings.Builder{}
	value := basicLit.Value

	switch basicLit.Kind {
	case token.INT:
		// Parse literal octal, binary, etc as a decimal integer (the VALUE classifies the
		// emitted form below; the TEXT keeps the Go source formatting where C# supports it)
		if intval, err := strconv.ParseInt(value, 0, 64); err == nil {
			value = preserveGoIntLiteral(value, strconv.FormatInt(intval, 10))

			if intval > math.MaxInt32 || intval < math.MinInt32 {
				// A value outside int32 used in an unsigned context (e.g. 0x80000000
				// passed to a uint32 parameter) must emit an unsigned C# literal — a
				// signed (nint)…L does not convert to uint/uint32.
				if intval >= 0 && v.isUnsignedType(basicLit) {
					result.WriteString(value)

					if intval > math.MaxUint32 {
						result.WriteString("UL")
					} else {
						result.WriteRune('U')
					}
				} else {
					result.WriteString("(nint)")
					result.WriteString(value)
					result.WriteRune('L')
				}
			} else {
				result.WriteString(value)
			}
		} else if uintval, err := strconv.ParseUint(value, 0, 64); err == nil {
			value = preserveGoIntLiteral(value, strconv.FormatUint(uintval, 10))

			if uintval > math.MaxUint32 {
				result.WriteString("(nuint)")
				result.WriteString(value)
				result.WriteString("UL")
			} else {
				result.WriteString(value)
				result.WriteRune('U')
			}
		} else {
			v.showWarning("Failed to parse integer literal as a 64-bit signed or unsigned int: %s", value)
			result.WriteString(value)
		}
	case token.FLOAT:
		// The C# suffix must reflect the literal's resolved type, not merely whether the value
		// fits in float32. A Go untyped float constant defaults to float64, so emitting `F`
		// (float32) whenever it fits corrupts inferred types — `z := 1.0` would become a float32,
		// and subsequent float64 arithmetic on it fails (CS0266). Emit `F` only when go/types
		// resolves the literal as float32; otherwise `D` (double, matching Go's float64 default).
		// And when an integer-valued float constant is used in an integer context — `math.Inf(1.0)`,
		// where Inf takes an int — emit the integer form (`1`), not `1.0D` (which is CS1503).
		// A literal INSIDE a constant expression (`var b float32 = -3.5`, `complex(2.5, -3.5)`
		// in a complex64 context) stays recorded UNTYPED — go/types resolves the context on the
		// outermost expression only — so its float32-ness comes from the propagated context (see
		// markUntypedConstContexts); a complex64 context makes the literal a float32 operand too.
		isFloat32 := false
		intForm := ""

		if tv, ok := v.info.Types[basicLit]; ok && tv.Type != nil {
			if basic, ok := tv.Type.Underlying().(*types.Basic); ok {
				if basic.Info()&types.IsInteger != 0 && tv.Value != nil {
					intForm = tv.Value.ExactString()
				} else if basic.Kind() == types.Float32 {
					isFloat32 = true
				} else if basic.Info()&types.IsUntyped != 0 {
					if constContext := v.untypedConstContext(basicLit); constContext != nil {
						switch constContext.Kind() {
						case types.Float32, types.Complex64:
							isFloat32 = true
						}
					}
				}
			}
		}

		if intForm != "" {
			result.WriteString(intForm)
		} else if isFloat32 {
			result.WriteString(value)
			result.WriteRune('F')
		} else {
			result.WriteString(value)
			result.WriteRune('D')
		}
	case token.IMAG:
		endsWith_i := strings.HasSuffix(value, "i")

		if endsWith_i {
			value = strings.TrimSuffix(value, "i")
		}

		// For complex literals, we use the golib `builtin.i()` helper. Qualify it with the
		// class name (never a bare `i(…)`): `i` is the single most common Go loop/receiver
		// variable, and a local named `i` in scope shadows the using-static import so the bare
		// call binds the variable and fails (encoding/gob encComplex's `i *encInstr` parameter,
		// `c != 0+0i` → CS0149 "Method name expected").
		// The F/D argument suffix selects the golib OVERLOAD — i(float) returns complex64,
		// i(double) returns complex128 — so it must reflect the literal's RESOLVED complex type,
		// not whether the value happens to fit in float32 (the old heuristic routed `0.1i` in a
		// complex128 context through complex64, silently losing precision). Like the FLOAT case
		// above, a literal inside a constant expression stays recorded untyped and takes its
		// complex64-ness from the propagated context (see markUntypedConstContexts); the untyped
		// default (complex128) emits D.
		isComplex64 := false

		if tv, ok := v.info.Types[basicLit]; ok && tv.Type != nil {
			if basic, ok := tv.Type.Underlying().(*types.Basic); ok {
				if basic.Kind() == types.Complex64 {
					isComplex64 = true
				} else if basic.Info()&types.IsUntyped != 0 {
					if constContext := v.untypedConstContext(basicLit); constContext != nil {
						switch constContext.Kind() {
						case types.Complex64, types.Float32:
							isComplex64 = true
						}
					}
				}
			}
		}

		if !endsWith_i {
			result.WriteString(value)
		} else if isComplex64 {
			result.WriteString(fmt.Sprintf("builtin.i(%sF)", value))
		} else {
			result.WriteString(fmt.Sprintf("builtin.i(%sD)", value))
		}
	case token.CHAR:
		value = replaceOctalChars(value)
		intVal, err := strconv.Atoi(value)

		if err == nil {
			if intVal <= 0xFFFF {
				// Character can be represented as a char in C# Rune. QuoteRune escapes control
				// and special characters (`'\t'`, `'\n'`, `'\\'` — Go's escapes are all valid C#
				// char escapes for BMP runes); the raw `%c` form emitted literal control bytes,
				// and a raw newline inside a char literal does not even parse (CS1010).
				result.WriteString(fmt.Sprintf("(rune)%s", strconv.QuoteRune(rune(intVal))))
			} else {
				// For characters beyond BMP, we can use the direct code point
				result.WriteString(fmt.Sprintf("0x%X", intVal))
			}
		} else {
			// A QUOTED rune literal beyond the BMP ('\U0001D504') cannot be a C# char
			// literal (html's entity table, CS1012 ×133) — emit the code point instead.
			// BMP literals keep their source text verbatim (zero churn).
			emitted := false

			if len(value) >= 2 && value[0] == '\'' && value[len(value)-1] == '\'' {
				if r, _, _, uerr := strconv.UnquoteChar(value[1:len(value)-1], '\''); uerr == nil && r > 0xFFFF {
					result.WriteString(fmt.Sprintf("(rune)0x%X", r))
					emitted = true
				}
			}

			if !emitted {
				result.WriteString(fmt.Sprintf("(rune)%s", value))
			}
		}
	case token.STRING:
		// A Go interpreted string literal that carries a `\xHH` raw-byte escape is binary data
		// (zip blobs, embedded tzdata). C#'s `\x` escape is greedy (1-4 hex digits) and a C# UTF-16
		// string re-encodes bytes >= 0x80 to two UTF-8 bytes, so re-emitting the token as a C#
		// string literal both mis-parses (`\xdb50` -> lone surrogate U+DB50, CS9026) and corrupts
		// the bytes. Emit these as a byte-array-backed @string so the exact bytes are preserved and
		// @string byte indexing matches Go. Text-only literals keep the readable string form.
		if !strings.HasPrefix(value, "`") && stringLiteralNeedsByteArray(value) {
			if byteArray, ok := emitByteArrayString(value); ok {
				result.WriteString(byteArray)
				break
			}
		}

		strVal, isRawStr := v.getStringLiteral(value)

		if !isRawStr {
			strVal = replaceOctalChars(strVal)
		}

		if context.sourceIsRuneArray || context.castToGoString {
			result.WriteString("(@string)")
		}

		result.WriteString(strVal)

		if context.u8StringOK {
			result.WriteString("u8")
		}
	}

	return result.String()
}
