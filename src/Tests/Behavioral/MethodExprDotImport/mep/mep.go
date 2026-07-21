// The FOREIGN package whose pointer-receiver methods the dot-importing main uses as method
// EXPRESSIONS. Each emits as a [GoRecv] extension static (plus a RecvGenerator ж-overload) in
// the mep_package C# class, so a method expression that binds them must QUALIFY the static form
// with this package's alias.
package mep

type Reader struct {
	Name string
}

func (r *Reader) Read(delim byte) (string, error) {
	return r.Name, nil
}

func (r *Reader) Peek(delim byte) (string, error) {
	return "peek:" + r.Name, nil
}
