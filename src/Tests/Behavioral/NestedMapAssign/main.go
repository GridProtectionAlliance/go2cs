package main

import "fmt"

// addEdge mirrors internal/dag's Graph.AddEdge: a nested map `map[string]map[string]bool`.
// Assigning `g[from][to] = true` — where `g[from]` returns a map VALUE (golib map is a readonly
// struct) — cannot use a C# indexer SETTER on that rvalue (CS1612). The converter emits
// `g[from].Set(to, true)`: a method call runs on the rvalue and mutates the shared backing
// dictionary, so the write is visible through the original.
func addEdge(g map[string]map[string]bool, from, to string) {
	if g[from] == nil {
		g[from] = map[string]bool{}
	}
	g[from][to] = true // nested map assignment → g[from].Set(to, true)
}

func main() {
	g := map[string]map[string]bool{}
	addEdge(g, "a", "b")
	addEdge(g, "a", "c")
	addEdge(g, "b", "c")
	fmt.Println(g["a"]["b"], g["a"]["c"], g["b"]["c"], g["a"]["z"]) // true true true false
	fmt.Println(len(g["a"]))                                        // 2
}
