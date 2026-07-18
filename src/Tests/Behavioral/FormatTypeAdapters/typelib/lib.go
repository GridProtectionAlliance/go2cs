// typelib supplies a FOREIGN struct implementing the consumer's interface by VALUE, so the
// consumer-side conversion generates the ᴠ value adapter (the os.File→io.Writer shape).
package typelib

// Mark is the foreign value implementer.
type Mark struct {
	Tag string
}

// NewMark constructs a Mark value.
func NewMark(tag string) Mark {
	return Mark{Tag: tag}
}

// Stamp implements the consumer's stamper interface by value.
func (m Mark) Stamp() string {
	return "mark:" + m.Tag
}
