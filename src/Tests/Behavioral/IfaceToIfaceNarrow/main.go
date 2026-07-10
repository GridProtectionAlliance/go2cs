package main

import "fmt"

type Reader interface{ Read() string }

type Writer interface{ Write(s string) }

type ReadWriteCloser interface {
	Reader
	Writer
	Close() string
}

type Conn interface {
	Read() string
	Write(s string)
	Close() string
}

type conn struct{ data string }

func (c *conn) Read() string   { return c.data }
func (c *conn) Write(s string) { c.data = s }
func (c *conn) Close() string  { return "closed:" + c.data }

func readFrom(r Reader) string { return r.Read() }

func writeTo(w Writer, s string) { w.Write(s) }

func asWriter(c Conn) Writer { return c }

func main() {
	var c Conn = &conn{data: "init"}

	// The SAME source interface also converts to the full-surface target: the
	// interface-inheritance prune must keep the NARROWER pairs' adapters alive
	// (net/http shape — Conn→ReadWriteCloser recorded, Conn→Reader/Writer pruned,
	// so every `new ConnᴠWriter(…)` use site failed CS0246).
	var rwc ReadWriteCloser = c

	writeTo(c, "hello")      // arg position, narrow target
	fmt.Println(readFrom(c)) // arg position, narrow target

	w := asWriter(c) // return position, narrow target
	w.Write("via-writer")

	fmt.Println(c.Read())
	fmt.Println(rwc.Close())
}
