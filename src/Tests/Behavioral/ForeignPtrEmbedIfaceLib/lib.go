// ForeignPtrEmbedIfaceLib is the library half of the foreign-pointer-embed interface-adapter
// guard. It declares types whose POINTER-receiver methods are promoted through embedded POINTER
// fields and consumed from ANOTHER package/assembly — the net/http `http2timeTimer{*time.Timer}` /
// `FlushAfterChunkWriter{*bufio.Writer}` / `bufio.ReadWriter{*Reader; *Writer}` shapes. The
// pointer-receiver methods land as ж-extensions (direct-ж primaries or public RecvGenerator twins)
// visible to a consumer only through METADATA, so the ImplementGenerator's promoted forwarders must
// bind the embedded box FIELD (`this.Meter.Add(n)` / `m_box.Value.Gauge.Set(v)`) — the deref'd-value
// forward (`this.Meter.Value.Add(n)`) strands the extension receiver (CS1929/CS1061). See
// docs/ConversionStrategies.md "Promoted methods through a FOREIGN pointer embed".
package ForeignPtrEmbedIfaceLib

// Meter's methods write through the address of a receiver field so they are emitted as
// box-receiver (`this ж<Meter>`) primaries — the time.Timer Reset/Stop shape.
type Meter struct {
	total int
}

func (m *Meter) Add(delta int) int {
	p := &m.total
	*p += delta
	return *p
}

func (m *Meter) Total() int {
	p := &m.total
	return *p
}

// Gauge's methods use plain pointer-receiver mutation — the [GoRecv] ref-extension shape whose
// public RecvGenerator ж-twin is what the consumer's metadata probe must find (bufio.Writer.Write).
type Gauge struct {
	value int
}

func (g *Gauge) Set(v int) {
	g.value = v
}

func (g *Gauge) Get() int {
	return g.value
}

// Pair embeds TWO pointers — the bufio.ReadWriter shape. A consumer adapting *Pair to an interface
// must route each promoted member to the UNIQUE embed declaring it (Add/Total → Meter, Set/Get →
// Gauge).
type Pair struct {
	*Meter
	*Gauge
}

// NewMeter returns a heap-allocated Meter so adapter writes alias the same box.
func NewMeter() *Meter {
	return &Meter{}
}

// NewPair wires both embeds so every promoted member is callable.
func NewPair() *Pair {
	return &Pair{Meter: &Meter{}, Gauge: &Gauge{}}
}
