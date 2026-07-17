package main

import (
	"fmt"
	"unsafe"
)

type Type struct {
	Size_       uintptr
	PtrBytes    uintptr
	Hash        uint32
	TFlag       TFlag
	Align_      uint8
	FieldAlign_ uint8
	Kind_       Kind
	Equal       func(unsafe.Pointer, unsafe.Pointer) bool
	GCData      *byte
	Str         NameOff
	PtrToThis   TypeOff
}

type Kind uint8

const (
	Invalid Kind = iota
	Bool
	Int
	Int8
	Int16
	Int32
	Int64
	Uint
	Uint8
	Uint16
	Uint32
	Uint64
	Uintptr
	Float32
	Float64
	Complex64
	Complex128
	Array
	Chan
	Func
	Interface
	Map
	Pointer
	Slice
	String
	Struct
	UnsafePointer
)

// String returns the name of k.
func (k Kind) String() string {
	if int(k) < len(kindNames) {
		return kindNames[k]
	}
	return kindNames[0]
}

var kindNames = []string{
	Invalid:       "invalid",
	Bool:          "bool",
	Int:           "int",
	Int8:          "int8",
	Int16:         "int16",
	Int32:         "int32",
	Int64:         "int64",
	Uint:          "uint",
	Uint8:         "uint8",
	Uint16:        "uint16",
	Uint32:        "uint32",
	Uint64:        "uint64",
	Uintptr:       "uintptr",
	Float32:       "float32",
	Float64:       "float64",
	Complex64:     "complex64",
	Complex128:    "complex128",
	Array:         "array",
	Chan:          "chan",
	Func:          "func",
	Interface:     "interface",
	Map:           "map",
	Pointer:       "ptr",
	Slice:         "slice",
	String:        "string",
	Struct:        "struct",
	UnsafePointer: "unsafe.Pointer",
}

type NameOff int32
type TypeOff int32
type TextOff int32

const (
	KindDirectIface Kind = 1 << 5
	KindGCProg      Kind = 1 << 6
	KindMask        Kind = (1 << 5) - 1
)

type TFlag uint8

const (
	TFlagUncommon       TFlag = 1 << 0
	TFlagExtraStar      TFlag = 1 << 1
	TFlagNamed          TFlag = 1 << 2
	TFlagRegularMemory  TFlag = 1 << 3
	TFlagUnrolledBitmap TFlag = 1 << 4
)

func NoEscape(p unsafe.Pointer) unsafe.Pointer {
	x := uintptr(p)
	return unsafe.Pointer(x ^ 0)
}

type EmptyInterface struct {
	Type *Type
	Data unsafe.Pointer
}

func TypeOf(a any) *Type {
	eface := *(*EmptyInterface)(unsafe.Pointer(&a))
	return (*Type)(NoEscape(unsafe.Pointer(eface.Type)))
}

func TypeFor[T any]() *Type {
	var v T
	if t := TypeOf(v); t != nil {
		return t
	}
	return TypeOf((*T)(nil)).Elem()
}

func (t *Type) Kind() Kind { return t.Kind_ & KindMask }

func (t *Type) HasName() bool {
	return t.TFlag&TFlagNamed != 0
}

func (t *Type) Elem() *Type {
	switch t.Kind() {
	case Array:
		tt := (*ArrayType)(unsafe.Pointer(t))
		return tt.Elem
	case Map:
		tt := (*MapType)(unsafe.Pointer(t))
		return tt.Elem
	}
	return nil
}

type MapType struct {
	Type
	Key        *Type
	Elem       *Type
	Bucket     *Type
	Hasher     func(unsafe.Pointer, uintptr) uintptr
	KeySize    uint8
	ValueSize  uint8
	BucketSize uint16
	Flags      uint32
}

func (t *Type) MapType() *MapType {
	if t.Kind() != Map {
		return nil
	}
	return (*MapType)(unsafe.Pointer(t))
}

type ArrayType struct {
	Type
	Elem  *Type // array element type
	Slice *Type // slice type
	Len   uintptr
}

func (t *Type) ArrayType() *ArrayType {
	if t.Kind() != Array {
		return nil
	}
	return (*ArrayType)(unsafe.Pointer(t))
}

// Bare-iota emission rule: golib's builtin declares `const nint iota = 0`. A const
// initialized by exactly the builtin `iota` emits it bare only when the emitted C# type
// matches golib's constant (nint — Go int, explicit or tightened) AND the folded
// group-position value is 0; every other type (named wrappers like Kind above, other
// widths like int64) and every later group position keeps the folded `/* iota */ N` form.
const (
	seqFirst  int = iota // position 0 at int -> `const nint seqFirst = iota`
	seqSecond            // implicit repetition -> folded 1
	seqAgain  int = iota // later position, explicit iota -> `/* iota */ 2`
)

const (
	wideFirst int64 = iota // position 0 but int64 != nint -> `/* iota */ 0`
	wideNext
)

const (
	rawZero = iota // untyped at package level (never tightens) -> `UntypedInt rawZero = iota`
	rawOne  = iota // later position: value 1 is not golib's iota -> `/* iota */ 1`
)

// stateMachine mirrors compress/flate's function-local const group: every use of the
// state consts below is a concrete int context, so the group TIGHTENS to nint and
// position 0 emits bare `iota`; wideLocal's only use is int64, so it tightens to a
// non-nint type and keeps the folded form.
func stateMachine(step int) string {
	const (
		stateInit = iota // tightens to nint -> `const nint stateInit = iota`
		stateDict
	)

	const wideLocal = iota // tightens to int64 -> `const int64 wideLocal = /* iota */ 0`
	var offset int64 = wideLocal

	switch step {
	case stateInit:
		return fmt.Sprintf("init[%d]", offset)
	case stateDict:
		return fmt.Sprintf("dict[%d]", offset)
	}
	return "unknown"
}

func main() {
	fmt.Printf("Slice Kind Value: %s [%d]\n", Slice.String(), int(Slice))
	fmt.Println(seqFirst, seqSecond, seqAgain)
	fmt.Println(wideFirst, wideNext)
	fmt.Println(rawZero, rawOne)
	fmt.Println(stateMachine(0))
	fmt.Println(stateMachine(1))
	fmt.Println(stateMachine(2))
}
