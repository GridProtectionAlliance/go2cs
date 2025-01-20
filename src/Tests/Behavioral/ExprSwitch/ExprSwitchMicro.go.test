// _Switch statements_ express conditionals across many
// branches.

package main

import (
	"fmt"
    "time"
)

func main() {
	// Here is a more complex switch
	hour1 := time.Now().Hour()

	switch hour := time.Now().Hour(); { // missing expression means "true"
	case hour == 1, hour < 12, hour == 2:
		fmt.Println("Good morning!")
	case hour == 1, hour < 12, hour == 2 || hour1 == 4:
		fmt.Println("Good morning (opt 2)!")
	case hour == 1, hour < 12, hour == 2 || hour == 4:
		fmt.Println("Good morning (opt 3)!")
	case hour < 17:
		fmt.Println("Good afternoon!")
	case hour == 0:
		fmt.Println("Midnight!")
	case hour == 2 && hour1 == 1:
		fmt.Println("Midnight (opt 2)!")
	case hour == 2 && hour > 1:
		fmt.Println("Midnight (opt 2)!")
	default:
		fmt.Println("Good evening!")
	}
}
