// Regression test: a struct that implements BOTH an interface AND a super-interface that EMBEDS
// it shares the inherited member. mime/multipart's sectionReadCloser embeds io.Closer, declares
// its own Close (overriding the embed), and is used as both io.Closer AND File (File embeds
// io.Closer). The ImplementGenerator emitted the inherited io.Closer.Close() explicit impl in
// BOTH the io.Closer partial and the File partial → CS0111 ("already defines a member") and
// CS8646 ("explicitly implemented more than once"). Each explicit interface member must be
// emitted once per struct across its partial-struct impls; the later partial skips it (the
// earlier partial's impl already satisfies it for the whole struct).
package main

import "fmt"

type Closer interface {
	Close() string
}

type Reader interface {
	Read() string
}

// ReadCloser EMBEDS Closer — its method set inherits Close, whose declaring interface is Closer.
type ReadCloser interface {
	Reader
	Closer
}

// myFile embeds the Closer interface (like sectionReadCloser embeds io.Closer) and declares its
// OWN Close, which overrides the embedded one (Go shadowing).
type myFile struct {
	Closer
	data string
}

func (f myFile) Read() string  { return "read:" + f.data }
func (f myFile) Close() string { return "closed:" + f.data }

func useCloser(c Closer) string          { return c.Close() }          // cast myFile -> Closer
func useReadCloser(rc ReadCloser) string { return rc.Read() + "," + rc.Close() } // cast -> ReadCloser

func main() {
	f := myFile{data: "x"}
	fmt.Println(useCloser(f))     // closed:x
	fmt.Println(useReadCloser(f)) // read:x,closed:x
}
