// NarrowByteArithFirstOperandCast guards the narrow-integer-arithmetic cast when the RHS's FIRST
// operand is itself a conversion — `buf[i] = byte(e/100) + '0'` (runtime print.go). Go evaluates byte
// arithmetic at byte width (wrapping); C# promotes `byte + int` (the rune `'0'`) to `int`, so the
// result must be cast back to `byte` (CS0266) both to compile and to preserve the wrap. The converter's
// "already narrowed" guard skipped the cast whenever the converted RHS merely STARTED with `(byte)(` —
// but there that is only the FIRST operand's own conversion (`(byte)(e/100) + (rune)'0'`), not a
// whole-expression narrowing, so the binary result stayed `int`. The guard now skips only when the
// ENTIRE RHS is `(byte)(…)`. (The narrow-cast on a RETURN of such arithmetic is a separate gap, not
// covered here — this test stays on the assignment path the fix addresses.) Verified vs Go incl. a wrap.
package main

import "fmt"

//go:noinline
func encode(e int) [3]byte {
	var buf [3]byte
	buf[0] = byte(e/100) + '0' // (byte)(e/100) + (rune)'0' -> needs the whole-expr (byte) cast
	buf[1] = byte(e/10)%10 + '0'
	buf[2] = byte(e%10) + '0'
	return buf
}

//go:noinline
func wrapCase(x byte) byte {
	var buf [1]byte
	buf[0] = byte(x) + byte(x) + '0' // wrapping, indexed assignment (like print.go): first operand is a conv
	return buf[0]
}

func main() {
	b := encode(305)
	fmt.Println(b[0], b[1], b[2]) // 51 48 53  ('3' '0' '5')
	fmt.Println(wrapCase(200))    // byte(200)+byte(200)=144, +'0'(48)=192
}
