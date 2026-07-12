package main

type reg struct {
	entries []string
	count   int
}

func newReg() *reg {
	return &reg{}
}

func (r *reg) add(name string) string {
	r.entries = append(r.entries, name)
	r.count++
	return name + "-added"
}

var registry = newReg()

var names = []string{"stdin", "stdout"}
