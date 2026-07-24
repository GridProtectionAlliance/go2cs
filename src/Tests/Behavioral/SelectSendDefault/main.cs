namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

internal static void fullBuffered() {
    var ch = new channel<nint>(1);
    ch.ᐸꟷ(1);
    switch (ᐧ) {
    case ᐧ when ch.ᐸꟷ(2, ꟷ): {
        fmt.Println("full buffered: sent");
        break;
    }
    default: {
        fmt.Println("full buffered: default");
        break;
    }}
    fmt.Println("full buffered: held =", ᐸꟷ(ch));
    switch (ᐧ) {
    case ᐧ when ch.ᐸꟷ(2, ꟷ): {
        fmt.Println("drained buffered: sent");
        break;
    }
    default: {
        fmt.Println("drained buffered: default");
        break;
    }}
    fmt.Println("drained buffered: held =", ᐸꟷ(ch));
}

internal static void freeBuffered() {
    var ch = new channel<nint>(2);
    ch.ᐸꟷ(1);
    switch (ᐧ) {
    case ᐧ when ch.ᐸꟷ(2, ꟷ): {
        fmt.Println("free buffered: sent");
        break;
    }
    default: {
        fmt.Println("free buffered: default");
        break;
    }}
    fmt.Println("free buffered: len =", len(ch));
    fmt.Println("free buffered: drained", ᐸꟷ(ch), ᐸꟷ(ch));
}

internal static void unbufferedWithReceiver() {
    var ch = new channel<nint>(0);
    var got = new channel<nint>(1);
    var chʗ1 = ch;
    var gotʗ1 = got;
    goǃ(() => {
        gotʗ1.ᐸꟷ(ᐸꟷ(chʗ1));
    });
    var sent = false;
    for (nint i = 0; i < 1000 && !sent; i++) {
        switch (ᐧ) {
        case ᐧ when ch.ᐸꟷ(7, ꟷ): {
            sent = true;
            break;
        }
        default: {
            time.Sleep(time.Millisecond);
            break;
        }}
    }
    fmt.Println("unbuffered with receiver: sent =", sent, "received =", ᐸꟷ(got));
}

internal static void nilChannel() {
    channel<nint> ch = default!;
    switch (ᐧ) {
    case ᐧ when ch.ᐸꟷ(1, ꟷ): {
        fmt.Println("nil channel: sent");
        break;
    }
    default: {
        fmt.Println("nil channel: default");
        break;
    }}
}

internal static void closedChannel() => func((defer, recover) => {
    defer(() => {
        {
            var r = recover(); if (r != default!) {
                fmt.Println("closed channel: recovered:", r);
            }
        }
    });
    var ch = new channel<nint>(1);
    close(ch);
    switch (ᐧ) {
    case ᐧ when ch.ᐸꟷ(1, ꟷ): {
        fmt.Println("closed channel: sent");
        break;
    }
    default: {
        fmt.Println("closed channel: default");
        break;
    }}
    fmt.Println("closed channel: NOT REACHED");
});

internal static void oneOfManyReady() {
    var full = new channel<nint>(1);
    full.ᐸꟷ(1);
    var open = new channel<nint>(1);
    switch (ᐧ) {
    case ᐧ when full.ᐸꟷ(2, ꟷ): {
        fmt.Println("one of many: chose full");
        break;
    }
    case ᐧ when open.ᐸꟷ(3, ꟷ): {
        fmt.Println("one of many: chose open");
        break;
    }
    default: {
        fmt.Println("one of many: default");
        break;
    }}
    fmt.Println("one of many: full held", ᐸꟷ(full), "open got", ᐸꟷ(open));
}

internal static void neitherReady() {
    var full = new channel<nint>(1);
    full.ᐸꟷ(1);
    var empty = new channel<nint>(1);
    switch (ᐧ) {
    case ᐧ when full.ᐸꟷ(2, ꟷ): {
        fmt.Println("neither ready: sent");
        break;
    }
    case ᐧ when empty.ꟷᐳ(out var v): {
        fmt.Println("neither ready: received", v);
        break;
    }
    default: {
        fmt.Println("neither ready: default");
        break;
    }}
}

internal static void exactlyOneSend() {
    var a = new channel<nint>(2);
    var b = new channel<nint>(2);
    switch (ᐧ) {
    case ᐧ when a.ᐸꟷ(1, ꟷ): {
        break;
    }
    case ᐧ when b.ᐸꟷ(2, ꟷ): {
        break;
    }
    default: {
        fmt.Println("exactly one send: default");
        break;
    }}
    fmt.Println("exactly one send: total =", len(a) + len(b));
}

internal static void blockingSendStillBlocks() {
    var ch = new channel<nint>(0);
    var got = new channel<nint>(1);
    var chʗ1 = ch;
    var gotʗ1 = got;
    goǃ(() => {
        time.Sleep(20 * time.Millisecond);
        gotʗ1.ᐸꟷ(ᐸꟷ(chʗ1));
    });
    switch (select(ch.ᐸꟷ(42, ꓸꓸꓸ))) {
    case 0: {
        fmt.Println("blocking send: sent");
        break;
    }}
    fmt.Println("blocking send: received", ᐸꟷ(got));
}

[GoType("chan nint")] partial struct queue;

internal static void namedChannelType() {
    var q = new queue(1);
    switch (ᐧ) {
    case ᐧ when q.ᐸꟷ(1, ꟷ): {
        fmt.Println("named channel: sent");
        break;
    }
    default: {
        fmt.Println("named channel: default");
        break;
    }}
    switch (ᐧ) {
    case ᐧ when q.ᐸꟷ(2, ꟷ): {
        fmt.Println("named channel full: sent");
        break;
    }
    default: {
        fmt.Println("named channel full: default");
        break;
    }}
    fmt.Println("named channel: held =", ᐸꟷ<nint>(q));
}

internal static void Main() {
    fullBuffered();
    freeBuffered();
    unbufferedWithReceiver();
    nilChannel();
    closedChannel();
    oneOfManyReady();
    neitherReady();
    exactlyOneSend();
    blockingSendStillBlocks();
    namedChannelType();
}

} // end main_package
