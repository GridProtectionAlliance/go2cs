package main

import "fmt"

func main() {
	// Uses symbols declared in lookup_tables.go. If the converter wrongly excluded
	// that file (treating "_tables" as a platform constraint), these references would
	// be undefined and the C# would not compile.
	fmt.Printf("%s:", tableName)
	for _, v := range lookupTable {
		fmt.Printf(" %d", v)
	}
	fmt.Println()
}
