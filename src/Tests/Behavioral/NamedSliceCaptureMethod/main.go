package main

import "fmt"

// A named slice whose pointer-receiver methods are CAPTURE-MODE because they pass the receiver
// to a helper taking *stack (the internal/trace/internal/oldtrace orderEventList Push/Pop
// heap.Interface shape). Such methods emit a ж<stack> receiver, so a VALUE local they are called
// on must be heap-boxed or the ж-receiver extension does not apply to the value (CS1929).
type stack []int

func growTo(s *stack, v int) { *s = append(*s, v) }
func shrink(s *stack)        { *s = (*s)[:len(*s)-1] }

func (s *stack) push(v int) { growTo(s, v) }

func (s *stack) pop() int {
	v := (*s)[len(*s)-1]
	shrink(s)
	return v
}

func main() {
	// `st` is a named-slice VALUE local; its address is never syntactically taken and it is not
	// closed over — only the capture-mode method calls justify the box.
	var st stack
	st.push(10)
	st.push(20)
	st.push(30)
	fmt.Println(len(st)) // 3
	fmt.Println(st.pop()) // 30
	fmt.Println(st.pop()) // 20
	fmt.Println(len(st))  // 1
}
