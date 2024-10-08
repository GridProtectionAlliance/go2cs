// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 05:29:21 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\deadlock.go
namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using debug = runtime.debug_package;
using time = time_package;
using System;
using System.Threading;

public static partial class main_package {

private static void init() {
    registerInit("InitDeadlock", InitDeadlock);
    registerInit("NoHelperGoroutines", NoHelperGoroutines);

    register("SimpleDeadlock", SimpleDeadlock);
    register("LockedDeadlock", LockedDeadlock);
    register("LockedDeadlock2", LockedDeadlock2);
    register("GoexitDeadlock", GoexitDeadlock);
    register("StackOverflow", StackOverflow);
    register("ThreadExhaustion", ThreadExhaustion);
    register("RecursivePanic", RecursivePanic);
    register("RecursivePanic2", RecursivePanic2);
    register("RecursivePanic3", RecursivePanic3);
    register("RecursivePanic4", RecursivePanic4);
    register("RecursivePanic5", RecursivePanic5);
    register("GoexitExit", GoexitExit);
    register("GoNil", GoNil);
    register("MainGoroutineID", MainGoroutineID);
    register("Breakpoint", Breakpoint);
    register("GoexitInPanic", GoexitInPanic);
    register("PanicAfterGoexit", PanicAfterGoexit);
    register("RecoveredPanicAfterGoexit", RecoveredPanicAfterGoexit);
    register("RecoverBeforePanicAfterGoexit", RecoverBeforePanicAfterGoexit);
    register("RecoverBeforePanicAfterGoexit2", RecoverBeforePanicAfterGoexit2);
    register("PanicTraceback", PanicTraceback);
    register("GoschedInPanic", GoschedInPanic);
    register("SyscallInPanic", SyscallInPanic);
    register("PanicLoop", PanicLoop);
}

public static void SimpleDeadlock() => func((_, panic, _) => {
    panic("not reached");
});

public static void InitDeadlock() => func((_, panic, _) => {
    panic("not reached");
});

public static void LockedDeadlock() {
    runtime.LockOSThread();
}

public static void LockedDeadlock2() {
    go_(() => () => {
        runtime.LockOSThread();
    }());
    time.Sleep(time.Millisecond);
}

public static void GoexitDeadlock() {
    Action F = () => {
        for (nint i = 0; i < 10; i++) {
        }
    };

    go_(() => F());
    go_(() => F());
    runtime.Goexit();
}

public static void StackOverflow() {
    Func<byte> f = default;
    f = () => {
        array<byte> buf = new array<byte>(64 << 10);
        return buf[0] + f();
    };
    debug.SetMaxStack(1474560);
    f();
}

public static void ThreadExhaustion() {
    debug.SetMaxThreads(10);
    var c = make_channel<nint>();
    for (nint i = 0; i < 100; i++) {
        go_(() => () => {
            runtime.LockOSThread();
            c.Send(0);
        }());
        c.Receive();
    }
}

public static void RecursivePanic() => func((defer, panic, _) => {
    () => {
        defer(() => {
            fmt.Println(recover());
        }());
        array<byte> x = new array<byte>(8192);
        x => {
            defer(() => {
                {
                    var err = recover();

                    if (err != null) {
                        panic("wrap: " + err._<@string>());
                    }

                }
            }());
            panic("bad");
        }(x);
    }();
    panic("again");
});

// Same as RecursivePanic, but do the first recover and the second panic in
// separate defers, and make sure they are executed in the correct order.
public static void RecursivePanic2() => func((defer, panic, _) => {
    () => {
        defer(() => {
            fmt.Println(recover());
        }());
        array<byte> x = new array<byte>(8192);
        x => {
            defer(() => {
                panic("second panic");
            }());
            defer(() => {
                fmt.Println(recover());
            }());
            panic("first panic");
        }(x);
    }();
    panic("third panic");
});

// Make sure that the first panic finished as a panic, even though the second
// panic was recovered
public static void RecursivePanic3() => func((defer, panic, recover) => {
    defer(() => {
        defer(() => {
            recover();
        }());
        panic("second panic");
    }());
    panic("first panic");
});

// Test case where a single defer recovers one panic but starts another panic. If
// the second panic is never recovered, then the recovered first panic will still
// appear on the panic stack (labeled '[recovered]') and the runtime stack.
public static void RecursivePanic4() => func((defer, panic, recover) => {
    defer(() => {
        recover();
        panic("second panic");
    }());
    panic("first panic");
});

// Test case where we have an open-coded defer higher up the stack (in two), and
// in the current function (three) we recover in a defer while we still have
// another defer to be processed.
public static void RecursivePanic5() => func((_, panic, _) => {
    one();
    panic("third panic");
});

//go:noinline
private static void one() {
    two();
}

//go:noinline
private static void two() => func((defer, _, _) => {
    defer(() => {
    }());

    three();
});

//go:noinline
private static void three() => func((defer, panic, _) => {
    defer(() => {
    }());

    defer(() => {
        fmt.Println(recover());
    }());

    defer(() => {
        fmt.Println(recover());
        panic("second panic");
    }());

    panic("first panic");
});

public static void GoexitExit() {
    println("t1");
    go_(() => () => {
        time.Sleep(time.Millisecond);
    }());
    ref nint i = ref heap(0, out ptr<nint> _addr_i);
    println("t2");
    runtime.SetFinalizer(_addr_i, p => {
    });
    println("t3");
    runtime.GC();
    println("t4");
    runtime.Goexit();
}

public static void GoNil() => func((defer, _, recover) => {
    defer(() => {
        recover();
    }());
    Action f = default;
    go_(() => f());
});

public static void MainGoroutineID() => func((_, panic, _) => {
    panic("test");
});

public static void NoHelperGoroutines() => func((_, panic, _) => {
    ref nint i = ref heap(0, out ptr<nint> _addr_i);
    runtime.SetFinalizer(_addr_i, p => {
    });
    time.AfterFunc(time.Hour, () => {
    });
    panic("oops");
});

public static void Breakpoint() {
    runtime.Breakpoint();
}

public static void GoexitInPanic() => func((defer, panic, _) => {
    go_(() => () => {
        defer(() => {
            runtime.Goexit();
        }());
        panic("hello");
    }());
    runtime.Goexit();
});

private partial struct errorThatGosched {
}

private static @string Error(this errorThatGosched _p0) {
    runtime.Gosched();
    return "errorThatGosched";
}

public static void GoschedInPanic() => func((_, panic, _) => {
    panic(new errorThatGosched());
});

private partial struct errorThatPrint {
}

private static @string Error(this errorThatPrint _p0) {
    fmt.Println("1");
    fmt.Println("2");
    return "3";
}

public static void SyscallInPanic() => func((_, panic, _) => {
    panic(new errorThatPrint());
});

public static void PanicAfterGoexit() => func((defer, panic, _) => {
    defer(() => {
        panic("hello");
    }());
    runtime.Goexit();
});

public static void RecoveredPanicAfterGoexit() => func((defer, panic, _) => {
    defer(() => {
        defer(() => {
            var r = recover();
            if (r == null) {
                panic("bad recover");
            }
        }());
        panic("hello");
    }());
    runtime.Goexit();
});

public static void RecoverBeforePanicAfterGoexit() => func((defer, panic, _) => { 
    // 1. defer a function that recovers
    // 2. defer a function that panics
    // 3. call goexit
    // Goexit runs the #2 defer. Its panic
    // is caught by the #1 defer.  For Goexit, we explicitly
    // resume execution in the Goexit loop, instead of resuming
    // execution in the caller (which would make the Goexit disappear!)
    defer(() => {
        var r = recover();
        if (r == null) {
            panic("bad recover");
        }
    }());
    defer(() => {
        panic("hello");
    }());
    runtime.Goexit();
});

public static void RecoverBeforePanicAfterGoexit2() => func((defer, panic, _) => {
    for (nint i = 0; i < 2; i++) {
        defer(() => {
        }());
    } 
    // 1. defer a function that recovers
    // 2. defer a function that panics
    // 3. call goexit
    // Goexit runs the #2 defer. Its panic
    // is caught by the #1 defer.  For Goexit, we explicitly
    // resume execution in the Goexit loop, instead of resuming
    // execution in the caller (which would make the Goexit disappear!)
    defer(() => {
        var r = recover();
        if (r == null) {
            panic("bad recover");
        }
    }());
    defer(() => {
        panic("hello");
    }());
    runtime.Goexit();
});

public static void PanicTraceback() {
    pt1();
}

private static void pt1() => func((defer, panic, _) => {
    defer(() => {
        panic("panic pt1");
    }());
    pt2();
});

private static void pt2() => func((defer, panic, _) => {
    defer(() => {
        panic("panic pt2");
    }());
    panic("hello");
});

private partial struct panicError {
}

private static @string Error(this ptr<panicError> _addr__p0) => func((_, panic, _) => {
    ref panicError _p0 = ref _addr__p0.val;

    panic("double error");
});

public static void PanicLoop() => func((_, panic, _) => {
    panic(addr(new panicError()));
});

} // end main_package
