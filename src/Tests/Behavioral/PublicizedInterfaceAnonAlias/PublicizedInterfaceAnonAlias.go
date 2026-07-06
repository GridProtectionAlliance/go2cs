package main

import "fmt"

// entry is a type ALIAS to an anonymous struct — exactly the shape of testing's
// `type corpusEntry = struct{…}`. go2cs LIFTS the anonymous struct to a synthesized
// `entryᴛ1` named type (a lift has no *types.Object of its own).
type entry = struct {
	ID   int
	Name string
}

// deps is an UNEXPORTED interface whose method both TAKES and RETURNS the aliased
// anonymous struct — mirroring testing's unexported `testDeps` with its corpusEntry-typed
// fuzzing methods (CoordinateFuzzing/RunFuzzWorker/ReadCorpus).
type deps interface {
	Process(e entry) entry
}

type impl struct{}

func (impl) Process(e entry) entry {
	e.ID++
	e.Name = e.Name + "!"
	return e
}

// Run is EXPORTED and holds the unexported `deps` interface, so go2cs PUBLICIZES `deps`
// (a Go consumer can legally hold the value) and emits it as a `public` C# interface. Its
// method's lifted `entryᴛ1` parameter/result must then be publicized too, or the public
// interface member is CS0051 (parameter) / CS0050 (result). Without that cascade this file
// fails to compile — the regression this test guards.
func Run(d deps) entry {
	return d.Process(entry{ID: 1, Name: "seed"})
}

func main() {
	r := Run(impl{})
	fmt.Println(r.ID, r.Name)
}
