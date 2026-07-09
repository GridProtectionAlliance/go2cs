package main

import "fmt"

type version string

func (v version) isValid() bool { return v != "" }
func (v version) tag() string   { return string(v) + "!" }

func asVersion(s string) version { return version(s) }

func mutate(s *string) {
	*s = *s + "-mutated"
}

func main() {
	// A `:=` from a func returning a NAMED string type must declare the local at
	// the named type (methods stay callable), not the underlying @string.
	fileVersion := asVersion("go1.21")
	if fileVersion.isValid() {
		fmt.Println(fileVersion.tag())
	}

	// A `:=`-declared string local whose address is taken must be heap-boxed so
	// writes through the pointer are visible.
	cause := ""
	mutate(&cause)
	fmt.Println("cause:" + cause)

	// A plain string local keeps the ordinary declaration.
	label := "x"
	for i := 0; i < 2; i++ {
		label = label + "y"
	}
	fmt.Println(label)
}
