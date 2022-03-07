// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2022 March 06 22:26:15 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\needmdeadlock.go
// This is for issue #42207.
// During a call to needm we could get a SIGCHLD signal
// which would itself call needm, causing a deadlock.

/*
#include <signal.h>
#include <pthread.h>
#include <sched.h>
#include <unistd.h>

extern void GoNeedM();

#define SIGNALERS 10

static void* needmSignalThread(void* p) {
    pthread_t* pt = (pthread_t*)(p);
    int i;

    for (i = 0; i < 100; i++) {
        if (pthread_kill(*pt, SIGCHLD) < 0) {
            return NULL;
        }
        usleep(1);
    }
    return NULL;
}

// We don't need many calls, as the deadlock is only likely
// to occur the first couple of times that needm is called.
// After that there will likely be an extra M available.
#define CALLS 10

static void* needmCallbackThread(void* p) {
    int i;

    for (i = 0; i < SIGNALERS; i++) {
        sched_yield(); // Help the signal threads get started.
    }
    for (i = 0; i < CALLS; i++) {
        GoNeedM();
    }
    return NULL;
}

static void runNeedmSignalThread() {
    int i;
    pthread_t caller;
    pthread_t s[SIGNALERS];

    pthread_create(&caller, NULL, needmCallbackThread, NULL);
    for (i = 0; i < SIGNALERS; i++) {
        pthread_create(&s[i], NULL, needmSignalThread, &caller);
    }
    for (i = 0; i < SIGNALERS; i++) {
        pthread_join(s[i], NULL);
    }
    pthread_join(caller, NULL);
}
*/
using C = go.C_package;// This is for issue #42207.
// During a call to needm we could get a SIGCHLD signal
// which would itself call needm, causing a deadlock.

/*
#include <signal.h>
#include <pthread.h>
#include <sched.h>
#include <unistd.h>

extern void GoNeedM();

#define SIGNALERS 10

static void* needmSignalThread(void* p) {
    pthread_t* pt = (pthread_t*)(p);
    int i;

    for (i = 0; i < 100; i++) {
        if (pthread_kill(*pt, SIGCHLD) < 0) {
            return NULL;
        }
        usleep(1);
    }
    return NULL;
}

// We don't need many calls, as the deadlock is only likely
// to occur the first couple of times that needm is called.
// After that there will likely be an extra M available.
#define CALLS 10

static void* needmCallbackThread(void* p) {
    int i;

    for (i = 0; i < SIGNALERS; i++) {
        sched_yield(); // Help the signal threads get started.
    }
    for (i = 0; i < CALLS; i++) {
        GoNeedM();
    }
    return NULL;
}

static void runNeedmSignalThread() {
    int i;
    pthread_t caller;
    pthread_t s[SIGNALERS];

    pthread_create(&caller, NULL, needmCallbackThread, NULL);
    for (i = 0; i < SIGNALERS; i++) {
        pthread_create(&s[i], NULL, needmSignalThread, &caller);
    }
    for (i = 0; i < SIGNALERS; i++) {
        pthread_join(s[i], NULL);
    }
    pthread_join(caller, NULL);
}
*/


using fmt = go.fmt_package;
using os = go.os_package;
using time = go.time_package;
using System;
using System.Threading;


namespace go;

public static partial class main_package {

private static void init() {
    register("NeedmDeadlock", NeedmDeadlock);
}

//export GoNeedM
public static void GoNeedM() {
}

public static void NeedmDeadlock() { 
    // The failure symptom is that the program hangs because of a
    // deadlock in needm, so set an alarm.
    go_(() => () => {
        time.Sleep(5 * time.Second);
        fmt.Println("Hung for 5 seconds");
        os.Exit(1);
    }());

    C.runNeedmSignalThread();
    fmt.Println("OK");

}

} // end main_package
