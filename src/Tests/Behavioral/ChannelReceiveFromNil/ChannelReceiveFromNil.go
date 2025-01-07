package main

import "fmt"

func main() {
        var c chan string
        fmt.Println(<-c) // deadlock
}