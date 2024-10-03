package main

import (
	"fmt"
	"go/ast"
	"go/token"
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
				if decimal <= math.MaxUint16 {
					value = strings.Replace(value, octal, fmt.Sprintf("\\u%04x", decimal), 1)
				} else {
					value = strings.Replace(value, octal, fmt.Sprintf("\\U%08x", decimal), 1)
				}
			} else {
				println(fmt.Sprintf("WARNING: Failed to parse octal literal \\%s: %s", octal, err))
			}
		}
	}

	return value
}

func (v *Visitor) convBasicLit(basicLit *ast.BasicLit, u8StringOK bool) string {
	result := &strings.Builder{}
	value := basicLit.Value

	switch basicLit.Kind {
	case token.INT:
		// Parse literal octal, binary, etc as a decimal integer
		if intval, err := strconv.ParseInt(value, 0, 64); err == nil {
			// Convert the signed integer to a string
			value = strconv.FormatInt(intval, 10)

			if intval > math.MaxInt32 || intval < math.MinInt32 {
				result.WriteString("(nint)")
				result.WriteString(value)
				result.WriteRune('L')
			} else {
				result.WriteString(value)
			}
		} else if uintval, err := strconv.ParseUint(value, 0, 64); err == nil {
			// Convert the unsigned integer to a string
			value = strconv.FormatUint(uintval, 10)

			if uintval > math.MaxUint32 {
				result.WriteString("(nuint)")
				result.WriteString(value)
				result.WriteString("UL")
			} else {
				result.WriteString(value)
				result.WriteRune('U')
			}
		} else {
			println(fmt.Sprintf("WARNING: Failed to parse integer literal as a 64-bit signed or unsigned int: %s", value))
			result.WriteString(value)
		}
	case token.FLOAT:
		result.WriteString(value)

		if _, err := strconv.ParseFloat(value, 32); err == nil {
			result.WriteRune('F')
		} else {
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
		}

		if endsWith_i {
			result.WriteString(fmt.Sprintf("i(%sD)", value))
		} else {
			result.WriteString(value)
		}
	case token.CHAR:
		result.WriteString(replaceOctalChars(value))
	case token.STRING:
		strVal, isRawStr := v.getStringLiteral(value)

		if !isRawStr {
			strVal = replaceOctalChars(strVal)
		}

		result.WriteString(strVal)

		if u8StringOK {
			result.WriteString("u8")
		}
	}

	return result.String()
}
