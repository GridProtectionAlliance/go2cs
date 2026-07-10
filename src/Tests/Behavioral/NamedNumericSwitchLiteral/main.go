package main

import "fmt"

type reply int

const statusOK reply = 0x00

// A switch over a named numeric type mixing a NAMED-CONST label with untyped LITERAL
// labels: the literal labels must compare through a cast to the named type, never a
// constant pattern (net/http socksReply.String shape, CS9135).
func describe(code reply) string {
	switch code {
	case statusOK:
		return "ok"
	case 0x01:
		return "general failure"
	case 0x02, 0x03:
		return "blocked"
	default:
		return "unknown"
	}
}

// An ALL-literal switch over the named type takes the same cast form.
func kind(code reply) string {
	switch code {
	case 0x00:
		return "zero"
	case 0x04:
		return "four"
	}
	return "many"
}

func main() {
	for i := 0; i <= 4; i++ {
		fmt.Println(describe(reply(i)), kind(reply(i)))
	}
}
