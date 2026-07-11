package main

import "fmt"

type (
    User struct {
	    Id   int
	    Name string
   }
			
	MyFloat float64

	Account struct {
		User User
		MyFloat int
	}
)

func main() {
	var a User;
	a.Id = 12
	a.Name = "Me"
	fmt.Println("Hello, 世界 ", a)
}