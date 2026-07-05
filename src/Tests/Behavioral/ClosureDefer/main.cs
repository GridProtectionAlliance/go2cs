namespace go;

using fmt = fmt_package;

partial class main_package {

internal static Action makeGreeter(@string name) {
    return () => func((defer, recover) => {
        deferǃ((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), "bye", name, defer);
        fmt.Println("hi", name);
    });
}

internal static void Main() {
    var f = () => func((defer, recover) => {
        deferǃ(ᴛ1 => fmt.Println(ᴛ1), "deferred", defer);
        fmt.Println("body");
    });
    f();
    var divPrint = (nint a, nint b) => func((defer, recover) => {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println("closure recovered:", r);
                }
            }
        });
        fmt.Println(a / b);
    });
    divPrint(20, 4);
    divPrint(1, 0);
    var safeDiv = (nint a, nint b) => {
        nint result = default!;
        func((defer, recover) => {
            defer(() => {
                {
                    var r = recover(); if (r != default!) {
                        result = -1;
                    }
                }
            });
            result = a / b; return;
        });
        return result;
    };
    fmt.Println(safeDiv(20, 4));
    fmt.Println(safeDiv(1, 0));
    var counted = () => {
        nint n = default!;
        func((defer, recover) => {
            defer(() => {
                n++;
            });
            n = 10;
            return;
        });
        return n;
    };
    fmt.Println(counted());
    var greet = makeGreeter("go2cs"u8);
    greet();
    var run = (Action fn) => {
        fn();
    };
    run(() => func((defer, recover) => {
        deferǃ(ᴛ1 => fmt.Println(ᴛ1), "arg-closure deferred", defer);
        fmt.Println("arg-closure body");
    }));
    ((Action)(() => func((defer, recover) => {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println("outer recovered:", r);
                }
            }
        });
        throw panic("from-iife");
    })))();
    var fetch = (nint, error) () => func<(nint, error)>((defer, recover) => {
        deferǃ(ᴛ1 => fmt.Println(ᴛ1), "fetch deferred", defer);
        return (42, default!);
    });
    var (v, err) = fetch();
    var tk = makeTask(5);
    fmt.Println("task:", (~tk).fn(), (~tk).name);
    fmt.Println("fetched:", v, err);
    fmt.Println("done");
}

[GoType] partial struct task {
    internal Func<nint> fn;
    internal @string name;
}

internal static ж<task> makeTask(nint @base) {
    nint bonus = @base * 2;
    return Ꮡ(new task(fn: () => bonus + 1, name: "t"u8));
}

} // end main_package
