// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This program outputs a CPU profile that includes
// both Go and Cgo stacks. This is used by the mapping info
// tests in runtime/pprof.
//
// If SETCGOTRACEBACK=1 is set, the CPU profile will includes
// PCs from C side but they will not be symbolized.

// package main -- go2cs converted at 2022 March 13 05:29:17 UTC
// Original source: C:\Program Files\Go\src\runtime\pprof\testdata\mappingtest\main.go
namespace go;
/*
#include <stdint.h>
#include <stdlib.h>

int cpuHogCSalt1 = 0;
int cpuHogCSalt2 = 0;

void CPUHogCFunction0(int foo) {
    int i;
    for (i = 0; i < 100000; i++) {
        if (foo > 0) {
            foo *= foo;
        } else {
            foo *= foo + 1;
        }
        cpuHogCSalt2 = foo;
    }
}

void CPUHogCFunction() {
    CPUHogCFunction0(cpuHogCSalt1);
}

struct CgoTracebackArg {
    uintptr_t context;
        uintptr_t sigContext;
    uintptr_t *buf;
        uintptr_t max;
};

void CollectCgoTraceback(void* parg) {
        struct CgoTracebackArg* arg = (struct CgoTracebackArg*)(parg);
    arg->buf[0] = (uintptr_t)(CPUHogCFunction0);
    arg->buf[1] = (uintptr_t)(CPUHogCFunction);
    arg->buf[2] = 0;
};
*/



} // end main_package
