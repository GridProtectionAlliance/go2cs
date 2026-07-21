package main

import "fmt"

// A struct whose fields are float64 / float32 targets for integer-FORM constants.
type sample struct {
	name  string
	value float64 // integer-form constant assigned to a float64 FIELD
	small float32 // integer-form constant assigned to a float32 FIELD
}

// integer-form constant assigned to a float64 RETURN (a value that overflows every
// C# integral type: unfixed, the bare literal is CS1021 "Integral constant is too large")
func bigReturn() float64 {
	return 123456789123456789123456789
}

// small integer-form constant assigned to a float64 RETURN
func smallReturn() float64 {
	return 33909
}

// integer-form constant assigned to a float32 RETURN
func f32Return() float32 {
	return 5
}

// integer-form constants assigned to package-level float VARs
var directBig float64 = 123456789123456789123456789
var directSmall float64 = 42

// decimal-FORM control constants that must stay byte-identical (already float literals)
var decimalControl float64 = 339.7784

func main() {
	samples := []sample{
		{"big", 123456789123456789123456789, 5}, // integer-form float64 field + float32 field
		{"decimal", 339.7784, 3.5},               // decimal-form control (unchanged)
		{"small", 33909, 8},
	}
	for _, s := range samples {
		fmt.Println(s.name, s.value, s.small)
	}
	fmt.Println(bigReturn(), smallReturn(), f32Return())
	fmt.Println(directBig, directSmall, decimalControl)
}
