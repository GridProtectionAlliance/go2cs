package main

import "fmt"

func pair(n int) (int, error) {
	if n < 0 {
		return 0, fmt.Errorf("negative %d", n)
	}
	return n * 2, nil
}

// Named results (value + error) captured by a deferred closure and written after
// capture: the deferred hook must observe the FINAL values. The mixed `v, err :=`
// reuse escape-marks the interface-typed result, routing it to a heap box — the
// declaration side must emit that box (was CS0103 on `Ꮡerr`, internal/poll
// SendFile's TestHookDidSendFile shape).
func send(n int) (written int64, err error) {
	defer func() {
		fmt.Println("hook:", written, err, written > 0)
	}()
	v, err := pair(n)
	if err != nil {
		written, err = 0, fmt.Errorf("send: %w", err)
		return
	}
	written = int64(v)
	return
}

// A VALUE-type named result whose address is taken: the write through the pointer
// must be visible to the bare return (needs the boxed declaration, not a copy-box).
func addrv() (x int) {
	y, x := 1, 2
	_ = y
	p := &x
	*p = 5
	return
}

// The func-LITERAL sibling: named results + defer + mixed reuse + later write.
func lit(n int) (int64, error) {
	f := func() (w int64, e error) {
		defer func() {
			fmt.Println("lit hook:", w, e)
		}()
		v, e := pair(n)
		if e != nil {
			return
		}
		w = int64(v)
		return
	}
	return f()
}

// A non-defer closure writing the named result, read after the call.
func cls(n int) (err error) {
	set := func() {
		err = fmt.Errorf("cls %d", n)
	}
	v, err := pair(n)
	fmt.Println("cls pair:", v, err)
	set()
	return
}

func main() {
	w, e := send(3)
	fmt.Println("send:", w, e)
	w, e = send(-1)
	fmt.Println("send:", w, e)
	fmt.Println("addrv:", addrv())
	w, e = lit(4)
	fmt.Println("lit:", w, e)
	w, e = lit(-2)
	fmt.Println("lit:", w, e)
	fmt.Println("cls:", cls(5))
}
