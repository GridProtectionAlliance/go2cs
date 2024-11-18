// _range_ iterates over elements in a variety of data
// structures. Let's see how to use `range` with some
// of the data structures we've already learned.

package main

import "fmt"

func main() {

	// Here we use `range` to sum the numbers in a slice.
	// Arrays work like this too.
	nums := []int{2, 3, 4}
	sum := 0

	var i, num, total int

	for i, num = range nums {
		sum += num
		total += i
	}
	fmt.Println("sum:", sum, "total:", total)

	// `range` on arrays and slices provides both the
	// index and value for each entry. Above we didn't
	// need the index, so we ignored it with the
	// blank identifier `_`. Sometimes we actually want
	// the indexes though.
	for i, num := range nums {
		if num == 3 {
			fmt.Println("index:", i)
		}
	}

	for _, num := range nums {
		fmt.Println("num:", num)
	}

	for i, _ := range nums {
		fmt.Println("index:", i)
	}

	total = 0
	for range nums {
		total++
	}
	fmt.Println("Total:", total)

	// `range` on map iterates over key/value pairs.
    /*
	kvs := map[string]string{"a": "apple", "b": "banana"}
	for k1, v1 := range kvs {
		fmt.Printf("%s -> %s\n", k1, v1)
	}

	// `range` can also iterate over just the keys of a map.
	for k1 := range kvs {
		fmt.Println("key:", k1)
	}

	// `range` can also iterate over just the keys of a map.
	for _, v1 := range kvs {
		fmt.Println("value:", v1)
	}
    */

	kvs := map[string]string{"a": "apple", "b": "banana"}
	for k, v := range kvs {
		fmt.Printf("%s -> %s\n", k, v)
	}

	for k := range kvs {
		fmt.Println("key:", k)
	}

	for v := range kvs {
		fmt.Println("value:", v)
	}

	var k, v string

	for k, v = range kvs {
		fmt.Printf("%s2 -> %s\n", k, v)

        for k, v := range kvs {
		    fmt.Printf("%s -> %s\n", k, v)
	    }

        str := "sub-test"
	    
        var i1 int
	    var c1 rune
        
        for i1, c1 = range str {
		    fmt.Println(i1, c1)
	    }
	}

	for k, _ = range kvs {
		fmt.Println("key:", k)
	}

	for _, v = range kvs {
		fmt.Println("val:", v)
	}

	total = 0
	for range kvs {
		total++
	}

	fmt.Println("Total:", total)

	// `range` on strings iterates over Unicode code
	// points. The first value is the starting byte index
	// of the `rune` and the second the `rune` itself.
	// See [Strings and Runes](strings-and-runes) for more
	// details.
	for i, c := range "go" {
		fmt.Println(i, c)
	}

	str := "test"

	var i1 int
	var c1 rune

	for i1, c1 = range str {
		fmt.Println(i1, c1)
	}

	arr := [...]int{2: 42, 4: 100}

	for i, v := range arr {
		fmt.Println(i, v)
	}

	// Creating a slice with indexed values
	slice := []int{2: 42, 4: 100}

	for i, v := range slice {
		fmt.Println(i, v)
	}

    var v1 int
	
    for i1, v1 = range slice {
		fmt.Println(i1, v1)
	}

    /*
    var v2 int
	
    for i1, v2 = range slice {
		fmt.Println(i1, v2)
	}
    */

    farr := [3]float32{1.1, 2.2, 3.3}

    for i, v := range farr {
        fmt.Println(i, v)
    }

    for i := 0; i < 10; i++ {
        var j int
        for j = 0; j < 5; j++ {
            fmt.Println(i, j)
        }
    }

    for x := 0; x < 10; x++ {
        for x := 0; x < 5; x++ {
            for x := 0; x < 3; x++ {
                for x := 0; x < 2; x++ {
                    fmt.Println(x)
                }
                fmt.Println(x)
            }
            fmt.Println(x)
        }
        fmt.Println(x)
    }
}

func calculate(x int) {
    var y = x * 2
    if x := y - 1; x > 0 {
        fmt.Println(x)
    }
}
