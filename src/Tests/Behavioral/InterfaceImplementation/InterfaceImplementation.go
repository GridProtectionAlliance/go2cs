package main

import "fmt"

type Animal interface {
    Type() string
    Swim() string
}

type Dog struct {
    Name string
    Breed string
}

type Frog struct {
    Name string
    Color string
}

func main() {
    f := new(Frog)
    d := new(Dog)
    zoo := [...]Animal{f, d}
 
    for _, a := range zoo {
        fmt.Println(a.Type(), "can", a.Swim())
    }
}

func (f *Frog) Type() string {
    return "Frog"
}

func (f *Frog) Swim() string {
    return "Kick"
}

func (d *Dog) Swim() string {
    return "Paddle"
}

func (d *Dog) Type() string {
    return "Doggie"
}