package convertedtestharness

import "fmt"

func add(left, right int) int {
	return left + right
}

func Double(value int) int {
	return add(value, value)
}

func Label(value int) string {
	return fmt.Sprintf("value=%d", value)
}
