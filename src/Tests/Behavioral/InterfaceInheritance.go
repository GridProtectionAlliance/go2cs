package main

import "fmt"

type T1 struct {
    name string
}

func (t T1) M() {}
func (t T1) N() {}

type T2 struct {
    name string
}

func (t T2) M() {}
func (t T2) N() {}

type I interface {
    M()
}

type V interface {
    I
    N()
}

func main() {
    m := make(map[I]int)
    var i1 I = T1{"foo"}
    var i2 I = T2{"bar"}
    m[i1] = 1
    m[i2] = 2
    fmt.Println(m)

    n := make(map[V]int)
    var v1 V = T1{"foo"}
    var v2 V = T2{"bar"}
    n[v1] = 3
    n[v2] = 4
    fmt.Println(n)
}
