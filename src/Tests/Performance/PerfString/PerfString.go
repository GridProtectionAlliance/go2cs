package main

import (
	"fmt"
	"time"
)

// String building and conversion: measures byte-slice append, []byte->string
// conversion, string indexing, and concatenation (@string emulation in C#).

func run(n int) int {
	total := 0
	buf := make([]byte, 0, 32)

	for i := 0; i < n; i++ {
		buf = buf[:0]
		v := i
		if v == 0 {
			buf = append(buf, '0')
		}
		for v > 0 {
			buf = append(buf, byte('0'+v%10))
			v /= 10
		}

		s := string(buf)
		total += len(s)
		total += int(s[0])

		if i%1000 == 0 {
			t := s + "-" + s
			total += len(t)
			total += int(t[len(t)-1])
		}
	}

	return total
}

func main() {
	start := time.Now().UnixNano()

	total := run(10000000)

	elapsed := time.Now().UnixNano() - start
	fmt.Println("checksum:", total)
	fmt.Println("elapsed_ns:", elapsed)
}
