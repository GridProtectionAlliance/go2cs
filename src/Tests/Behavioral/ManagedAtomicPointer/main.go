package main

import (
	"fmt"
	"sync/atomic"
	"unsafe"
)

type proc struct{ addr int }

type lazyProc struct {
	p *proc
}

// find mirrors x/sys/windows's LazyProc.Find: a lock-free managed-pointer-field cache.
func (l *lazyProc) find() bool {
	if atomic.LoadPointer((*unsafe.Pointer)(unsafe.Pointer(&l.p))) == nil {
		atomic.StorePointer((*unsafe.Pointer)(unsafe.Pointer(&l.p)), unsafe.Pointer(&proc{addr: 42}))
		return true
	}
	return false
}

func main() {
	var l lazyProc
	fmt.Println(l.find()) // true — field was nil
	fmt.Println(l.find()) // false — already set
	fmt.Println(l.p.addr) // 42 — the stored pointer is usable
}
