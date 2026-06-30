package main

import (
	"fmt"
	"unsafe"
)

// A (*[N]T)(ptr)[:n] pointer-cast slice is emitted as a golib slice<T> (not a bare Span<T>), so it
// ranges as (index, element) and supports element-address Ꮡ(s, i) — mirrors runtime's printDebugLog
// (range + &state[i]) and os_windows (range over an unsafe []byte). This is a COMPILE + target guard:
// the values flow through the unsafe.Pointer(&arr) round-trip (a transient fixed address → stale, the
// known unsafe.Pointer=nuint limitation), so the runtime values are not output-compared — only that the
// range and element-address forms over a pointer-cast slice compile.
func main() {
	arr := [4]int{10, 20, 30, 40}
	s := (*[4]int)(unsafe.Pointer(&arr))[:4]

	sum := 0
	for i := range s { // index-only range over a pointer-cast slice
		sum += i
	}

	total := 0
	for _, v := range s { // value range
		total += v
	}

	for i := range s { // element-address through &s[i]
		ps := &s[i]
		*ps += 1
	}

	fmt.Println(sum, total, s[0])
}
