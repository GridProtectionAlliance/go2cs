package main

import "fmt"

func main() {	
	type Person struct {
		Name    string
		Address *struct {
			Street string
			City   string
		}
	}

	var data *struct {
		Name    string `json:"name"`
		Address *struct {
			Street string `json:"street"`
			City   string `json:"city"`
		} `json:"address"`
	}
	
	var mine Person

	var person = (*Person)(data)  // ignoring tags, the underlying types are identical
	
	person = &mine
	
	fmt.Println(mine == *person)
    
    fmt.Println([]rune(string("白鵬翔"))) 
}