package main

func main() {
	var c chan string
	c <- "let's get started" // deadlock
}