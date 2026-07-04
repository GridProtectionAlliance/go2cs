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

type tally struct{ total int }

func (t *tally) Write(p []byte) (int, error) {
	t.total += len(p)
	return len(p), nil
}

// fill mirrors archive/tar's ReadFrom-hiding shape: a LIFTED anonymous struct embedding
// io.Writer, built from the POINTER receiver - the embed emits as a real interface field
// (not a promoted-struct property), the receiver goes direct-ж so its box feeds the
// pointer adapter, and the ctor routes the element through the interface conversion
// (archive/tar CS0144/CS1929/CS1503).
func (t *tally) fill(r io.Reader) (int64, error) {
	return io.Copy(struct{ io.Writer }{t}, r)
}

type byteRepeat struct{ left int }

func (b *byteRepeat) Read(p []byte) (int, error) {
	if b.left == 0 {
		return 0, io.EOF
	}
	n := 0
	for i := range p {
		if b.left == 0 {
			break
		}
		p[i] = 'x'
		b.left--
		n++
	}
	return n, nil
}

// quad/frame: slicing a NAMED-array field (f.data[:]) needs the wrapper's Range indexer
// (archive/tar's tr.blk[..], CS1503).
type quad [4]byte

type frame struct{ data quad }

func checksum(f *frame) int {
	s := 0
	for _, v := range f.data[:] {
		s += int(v)
	}
	return s
}

func main() {
	testTypeSwitch(fakeError{})
	testTypeAssertion(fakeError{})
	takesReader(fakeReader{})
	testCompositeLiteral()
	testInlineField()
	testInterfaceEmbedding(embeddedImpl{})

	tl := &tally{}
	n, err := tl.fill(&byteRepeat{left: 10})
	fmt.Println(n, err == nil, tl.total) // 10 true 10

	fr := frame{data: quad{1, 2, 3, 4}}
	fmt.Println(checksum(&fr)) // 10
}
