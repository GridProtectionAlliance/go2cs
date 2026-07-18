package main

import "fmt"

// Exercises len(string) over multi-byte literals: Go counts UTF-8 BYTES, so the values
// below differ from the UTF-16 char counts a naive C# string.Length would report. The
// table shape keeps the literals as plain C# strings in the emission (the u8-suppressed
// argument path), which is exactly the overload under guard. Printed output stays ASCII.
var tests = []struct {
	name string
	n    int
}{
	{"ascii", len("abc")},
	{"bmp2", len("aΩb")},        // Ω: 2-byte UTF-8, 1 UTF-16 char
	{"bmp3", len("a☺b☻c")}, // ☺☻: 3-byte UTF-8, 1 UTF-16 char each
	{"astral", len("z\U0001d56b")},   // 𝕫: 4-byte UTF-8, UTF-16 surrogate pair
}

func main() {
	for _, t := range tests {
		fmt.Println(t.name, t.n)
	}
	fmt.Println(len("a☺b☻"), len("héllo"), len("日本語"))
}
