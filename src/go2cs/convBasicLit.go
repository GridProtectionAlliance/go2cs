package main

import (
	"fmt"
	"go/ast"
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
		isFloat32 := false
		intForm := ""

		if tv, ok := v.info.Types[basicLit]; ok && tv.Type != nil {
			if basic, ok := tv.Type.Underlying().(*types.Basic); ok {
				if basic.Info()&types.IsInteger != 0 && tv.Value != nil {
					intForm = tv.Value.ExactString()
				} else if basic.Kind() == types.Float32 {
					isFloat32 = true
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

		// For complex literals, we use the "i()" helper function (see bulitin in golib)
		if _, err := strconv.ParseFloat(value, 32); err == nil {
			if endsWith_i {
				result.WriteString(fmt.Sprintf("i(%sF)", value))
			} else {
				result.WriteString(value)
			}
		} else {
			if endsWith_i {
				result.WriteString(fmt.Sprintf("i(%sD)", value))
			} else {
				result.WriteString(value)
			}
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
