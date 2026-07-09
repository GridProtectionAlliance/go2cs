package main

import (
	"fmt"

	"CrossPkgLib"
)

type localLabel interface {
	Label() string
}

func labelOf(l localLabel) string {
	return l.Label()
}

func main() {
	var foreign CrossPkgLib.Labeled = CrossPkgLib.Sensor{Name: "adapter", Temp: 21}
	var local localLabel = foreign

	fmt.Println(labelOf(foreign))
	fmt.Println(local.Label())
	fmt.Println(CrossPkgLib.Describe(foreign))
}
