package main

import "fmt"

// takesUint32 mirrors internal/cpu.cpuid, which takes a uint32. An untyped hex
// constant like 0x80000000 (> int32 max, fits uint32) must convert to an unsigned
// C# literal (2147483648U); a signed (nint)…L would not convert to uint.
func takesUint32(v uint32) uint32 { return v }

func main() {
	// 1. Large unsigned constant adopting its parameter type.
	fmt.Println(takesUint32(0x80000000))

	env := "cpu.feature=on"

	// 2. Slicing a string yields a string, so a sliced value compares directly to a
	//    string literal (this previously became `slice<byte> != string`). Mirrors
	//    internal/cpu.processOptions' `field[:4] != "cpu."`.
	fmt.Println(env[:4] == "cpu.")

	// 3. Tuple (multi-value) assignment of string slices, then of a bare "" literal.
	//    A u8 string literal is a ReadOnlySpan<byte> (ref struct) and cannot be a
	//    ValueTuple element, so "" must not use the u8 form here.
	var field string
	field, env = env[:4], env[5:]
	fmt.Println(field, env)

	field, env = env, ""
	fmt.Printf("[%s][%s]\n", field, env)
}
