package main

import "fmt"

type ColorList struct {
    Total int
    Color string
    Next *ColorList
    NextNext **ColorList
}

func main() {
   red := ColorList{2, "red", nil, nil}
   blue := ColorList{2, "blue", nil, nil}

   red.Next = &blue

   fmt.Printf("Value of red = %v\n", red)
   fmt.Printf("Value of blue = %v\n", blue)
}