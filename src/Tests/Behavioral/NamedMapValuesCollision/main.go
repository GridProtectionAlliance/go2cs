// Regression test: a NAMED map type whose name is `Values` (or `Keys`) collides with the
// IDictionary<K,V>.Values / .Keys members that the TypeGenerator's IMap wrapper forwards.
// Those two forwarders were emitted as PUBLIC properties, so `public ... Values` inside the
// `Values` struct had the same name as its enclosing type (CS0542 — net/url's
// `type Values map[string][]string`). Emitting them as EXPLICIT interface implementations
// (as golib's own map<K,V> does) removes the collision without changing observable behavior:
// Go code never calls .Keys/.Values on a map; they exist only for IDictionary conformance.
package main

import "fmt"

type Values map[string][]string

func (v Values) Get(key string) string {
	if vs := v[key]; len(vs) > 0 {
		return vs[0]
	}
	return ""
}

func (v Values) Add(key, value string) {
	v[key] = append(v[key], value)
}

func main() {
	v := Values{}
	v.Add("color", "red")
	v.Add("color", "blue")
	fmt.Println(v.Get("color"))  // red
	fmt.Println(len(v["color"])) // 2
	fmt.Println(len(v))          // 1
}
