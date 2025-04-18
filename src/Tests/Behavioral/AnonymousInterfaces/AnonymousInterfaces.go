package main

import (
	"fmt"
	"io"
)

// 1. Type Switch using inline interface
func testTypeSwitch(err error) {
	switch x := err.(type) {
	case interface{ Unwrap() error }:
		fmt.Println("TypeSwitch: Unwrap =", x.Unwrap())
	default:
		fmt.Println("TypeSwitch: No match")
	}
}

// 2. Type Assertion using inline interface
func testTypeAssertion(err error) {
	if x, ok := err.(interface{ Is(error) bool }); ok {
		fmt.Println("TypeAssertion: Is(nil) =", x.Is(nil))
	} else {
		fmt.Println("TypeAssertion: No match")
	}
}

// 3. Function parameter using inline interface
func takesReader(r interface{ Read([]byte) (int, error) }) {
	buf := make([]byte, 4)
	n, _ := r.Read(buf)
	fmt.Println("FuncParam: Read =", string(buf[:n]))
}

// 4. Composite literal with inline interface
func testCompositeLiteral() {
	readers := []interface{ Read([]byte) (int, error) }{fakeReader{}}
	buf := make([]byte, 4)
	n, _ := readers[0].Read(buf)
	fmt.Println("CompositeLiteral: Read =", string(buf[:n]))
}

// 5. Struct field with inline interface type
type WithInlineField struct {
	R interface{ Read([]byte) (int, error) }
}

func testInlineField() {
	s := WithInlineField{R: fakeReader{}}
	buf := make([]byte, 4)
	n, _ := s.R.Read(buf)
	fmt.Println("InlineField: Read =", string(buf[:n]))
}

/*
// 6. Interface embedding inline interface
type InlineEmbed interface {
	interface{ Close() error }
	Flush() error
}

type embeddedImpl struct{}

func (embeddedImpl) Close() error { return nil }
func (embeddedImpl) Flush() error { return nil }

func testInterfaceEmbedding(x InlineEmbed) {
	_ = x.Close()
	_ = x.Flush()
	fmt.Println("InterfaceEmbed: Close and Flush OK")
}
*/

// Supporting types

type fakeReader struct{}

func (fakeReader) Read(b []byte) (int, error) {
	copy(b, "DATA")
	return 4, nil
}

type fakeError struct{}

func (fakeError) Error() string     { return "fake error" }
func (fakeError) Unwrap() error     { return io.EOF }
func (fakeError) Is(err error) bool { return err == io.EOF }

func main() {
	testTypeSwitch(fakeError{})
	testTypeAssertion(fakeError{})
	takesReader(fakeReader{})
	testCompositeLiteral()
	testInlineField()
	//testInterfaceEmbedding(embeddedImpl{})
}
