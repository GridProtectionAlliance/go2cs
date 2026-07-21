// FuncLitNumericTupleReturn guards the explicit tuple return type on a multi-result func
// literal whose return arm carries an untyped NUMERIC constant element against a
// differently-SIZED declared result element. The literal emits bare, so without the explicit
// type the arm infers the literal's natural C# type — an INT literal is C# `int`, a FLOAT
// literal C# `double` — where the Go result is e.g. int64: net/http ServeContent's
// `sizeFunc := func() (int64, error) { …; return 0, errSeeker }` inferred
// `Func<(int, error errSeeker)>` and was rejected at the serveContent call site (delegate
// types are invariant — CS1662/CS0029/CS1503; the tuple-element sibling of the
// FuncLitStringConcatReturn string-literal arm). The converter now marks such an arm
// not-fully-typed so the existing generateResultSignature prefix fires:
//
//	var sizeFunc = (int64, error) () => { …; return (0, errSeeker); };
//
// A declared element the literal's natural type already matches (int32 for INT, float64 for
// FLOAT) keeps inferred typing, and Go `int` (C# nint) is deliberately exempt — the
// `return 0, err` shape against (int, error) results is pervasive and green today — so the
// intControl arm pins the unchanged inferred emission. That exemption is NARROWED, not removed:
// when a declared-int position carries BOTH a bare-`0` (C# int) arm AND a non-literal `i+1`
// (C# nint) arm, the mix defeats inference (the mixedIntArms bufio-`onComma` shape) and the
// explicit `(nint, …)` return type is forced. Every literal here is invoked and its values
// printed, comparing behavior vs Go; the sized literals are also PASSED to typed function
// parameters, the exact use the wrong inferred delegate type rejects.
package main

import (
	"errors"
	"fmt"
)

var errSeek = errors.New("cannot seek")

func take64(f func() (int64, error)) string {
	n, err := f()
	if err != nil {
		return "err:" + err.Error()
	}
	return fmt.Sprintf("ok:%d", n)
}

func takeF64(f func() (float64, error)) string {
	x, err := f()
	if err != nil {
		return "err:" + err.Error()
	}
	return fmt.Sprintf("ok:%g", x)
}

func seekEnd(ok bool) (int64, error) {
	if !ok {
		return 0, errSeek
	}
	return 4096, nil
}

// sizeFuncShape is the ServeContent shape: untyped-0 error arms plus a variable success arm,
// declared (int64, error), assigned to a local and passed to a typed parameter.
func sizeFuncShape(ok bool) string {
	sizeFunc := func() (int64, error) {
		size, err := seekEnd(ok)
		if err != nil {
			return 0, errSeek
		}
		return size, nil
	}
	return take64(sizeFunc)
}

// negatedArm covers the SUB-negated literal (`return -1, …`), same declared (int64, error).
func negatedArm(ok bool) string {
	probe := func() (int64, error) {
		size, err := seekEnd(ok)
		if err != nil {
			return -1, nil
		}
		return size, nil
	}
	return take64(probe)
}

// floatShape: an untyped INT literal against a declared float64 element marks the arm, while
// the 2.5 FLOAT literal's natural C# double already matches float64.
func floatShape(ok bool) string {
	ratio := func() (float64, error) {
		if !ok {
			return 0, errSeek
		}
		return 2.5, nil
	}
	return takeF64(ratio)
}

// floatControl: a fully FLOAT-literal arm against float64 keeps inferred typing and still
// binds the typed parameter (natural double == declared float64).
func floatControl() string {
	half := func() (float64, error) {
		return 1.5, errSeek
	}
	return takeF64(half)
}

// intControl: the pervasive stdlib shape — untyped 0 against declared Go int — keeps its
// inferred emission (the deliberate exemption) and is invoked locally.
func intControl(ok bool) string {
	count := func() (int, error) {
		if !ok {
			return 0, errSeek
		}
		return 21, nil
	}
	n, err := count()
	if err != nil {
		return "err:" + err.Error()
	}
	return fmt.Sprintf("ok:%d", n)
}

func takeSplit(f func(data []byte, atEOF bool) (advance int, token []byte, err error), data []byte, atEOF bool) string {
	a, t, err := f(data, atEOF)
	if err != nil {
		return fmt.Sprintf("adv:%d tok:[%s] err:%s", a, string(t), err.Error())
	}
	return fmt.Sprintf("adv:%d tok:[%s]", a, string(t))
}

// mixedIntArms is the bufio ExampleScanner_* `onComma` shape and the case where the Go-`int`
// exemption above (intControl) must NOT fire: a multi-result closure whose declared first result
// is Go `int` (C# nint) and whose arms MIX a bare `0` (naturally C# `int`) with a non-literal
// `i + 1` (C# `nint`). One arm is FULLY typed (`return 0, data, errSeek` — no nil element), so it
// alone drove C# lambda return-type inference to a C# `int` first element, and the `nint` arm
// then failed (CS0029/CS1662; the assignment-inferred delegate is also rejected at the invariant
// use site, CS0407). The int-literal + nint-expression MIX at a declared-int position now forces
// the explicit `(nint, ...)` return type; each arm converts in place. intControl (all-literal, no
// nint-expression arm) still keeps its inferred emission — the exemption is narrowed, not removed.
func mixedIntArms(data []byte, atEOF bool) string {
	onComma := func(data []byte, atEOF bool) (advance int, token []byte, err error) {
		for i := 0; i < len(data); i++ {
			if data[i] == ',' {
				return i + 1, data[:i], nil
			}
		}
		if !atEOF {
			return 0, nil, nil
		}
		return 0, data, errSeek
	}
	return takeSplit(onComma, data, atEOF)
}

func main() {
	fmt.Println("sizeFuncShape(true):", sizeFuncShape(true))
	fmt.Println("sizeFuncShape(false):", sizeFuncShape(false))
	fmt.Println("negatedArm(true):", negatedArm(true))
	fmt.Println("negatedArm(false):", negatedArm(false))
	fmt.Println("floatShape(true):", floatShape(true))
	fmt.Println("floatShape(false):", floatShape(false))
	fmt.Println("floatControl:", floatControl())
	fmt.Println("intControl(true):", intControl(true))
	fmt.Println("intControl(false):", intControl(false))
	fmt.Println("mixedIntArms(hi,bye/true):", mixedIntArms([]byte("hi,bye"), true))
	fmt.Println("mixedIntArms(tail/true):", mixedIntArms([]byte("tail"), true))
	fmt.Println("mixedIntArms(tail/false):", mixedIntArms([]byte("tail"), false))
}
