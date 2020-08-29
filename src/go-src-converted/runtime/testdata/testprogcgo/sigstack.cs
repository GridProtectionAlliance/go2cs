// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// Test handling of Go-allocated signal stacks when calling from
// C-created threads with and without signal stacks. (See issue
// #22930.)

// package main -- go2cs converted at 2020 August 29 08:24:59 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\sigstack.go
/*
#include <pthread.h>
#include <signal.h>
#include <stdio.h>
#include <stdlib.h>
#include <sys/mman.h>

extern void SigStackCallback();

static void* WithSigStack(void* arg __attribute__((unused))) {
    // Set up an alternate system stack.
    void* base = mmap(0, SIGSTKSZ, PROT_READ|PROT_WRITE, MAP_PRIVATE|MAP_ANON, -1, 0);
    if (base == MAP_FAILED) {
        perror("mmap failed");
        abort();
    }
    stack_t st = {}, ost = {};
    st.ss_sp = (char*)base;
    st.ss_flags = 0;
    st.ss_size = SIGSTKSZ;
    if (sigaltstack(&st, &ost) < 0) {
        perror("sigaltstack failed");
        abort();
    }

    // Call Go.
    SigStackCallback();

    // Disable signal stack and protect it so we can detect reuse.
    if (ost.ss_flags & SS_DISABLE) {
        // Darwin libsystem has a bug where it checks ss_size
        // even if SS_DISABLE is set. (The kernel gets it right.)
        ost.ss_size = SIGSTKSZ;
    }
    if (sigaltstack(&ost, NULL) < 0) {
        perror("sigaltstack restore failed");
        abort();
    }
    mprotect(base, SIGSTKSZ, PROT_NONE);
    return NULL;
}

static void* WithoutSigStack(void* arg __attribute__((unused))) {
    SigStackCallback();
    return NULL;
}

static void DoThread(int sigstack) {
    pthread_t tid;
    if (sigstack) {
        pthread_create(&tid, NULL, WithSigStack, NULL);
    } else {
        pthread_create(&tid, NULL, WithoutSigStack, NULL);
    }
    pthread_join(tid, NULL);
}
*/
using C = go.C_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("SigStack", SigStack);
        }

        public static void SigStack()
        {
            C.DoThread(0L);
            C.DoThread(1L);
            C.DoThread(0L);
            C.DoThread(1L);
            println("OK");
        }

        public static ref long BadPtr = default;

        //export SigStackCallback
        public static void SigStackCallback() => func((defer, _, recover) =>
        { 
            // Cause the Go signal handler to run.
            defer(() =>
            {
                recover();

            }());
            BadPtr.Value = 42L;
        });
    }
}
