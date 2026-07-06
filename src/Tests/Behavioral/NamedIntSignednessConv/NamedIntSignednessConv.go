// Guards a conversion between two named-numeric types of OPPOSITE signedness — internal/trace's
// public `type Time int64` and unexported `type timestamp uint64`, converted both ways via
// `Time(ts)` / `timestamp(t)` (oldtrace.go `time: Time(ev.Ts)`, batch.go `time: timestamp(ts)`).
//
// The converter records a GoImplicitConv between the two [GoType num:] wrappers; because Time is
// exported and timestamp is not, ImplicitConvGenerator hosts the operator in the less-accessible
// `timestamp` struct and casts `src.Value` (a ulong) straight to `Time` — but `ulong`→`Time`(int64)
// needs `ulong`→`long`, which is not an implicit C# conversion (CS0030). The generator now routes a
// non-implicit local numeric pair through the constructed type's underlying keyword: `new Time((long)src.Value)`.
package main

import "fmt"

type Time int64

type timestamp uint64

func toTime(ts timestamp) Time {
	return Time(ts)
}

func toTimestamp(t Time) timestamp {
	return timestamp(t)
}

func main() {
	var ts timestamp = 1234567890
	t := toTime(ts)
	fmt.Println(int64(t)) // 1234567890

	back := toTimestamp(t + 1)
	fmt.Println(uint64(back)) // 1234567891

	// a large uint64 value that would overflow into negative int64 — the cast must preserve the
	// bit pattern exactly (Go conversion is a reinterpret, matching C# unchecked (long)).
	var big timestamp = 18446744073709551615 // ^uint64(0)
	fmt.Println(int64(toTime(big)))          // -1
}
