// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.net;

using bytes = bytes_package;
using binary = encoding.binary_package;
using io = io_package;
using rand = math.rand_package;
using net = net_package;
using runtime = runtime_package;
using sync = sync_package;
using testing = testing_package;
using time = time_package;
using encoding;
using math;

partial class nettest_package {

public delegate (net.Conn c1, net.Conn c2, Action stop, error err) MakePipe();

// TestConn tests that a net.Conn implementation properly satisfies the interface.
// The tests should not produce any false positives, but may experience
// false negatives. Thus, some issues may only be detected when the test is
// run multiple times. For maximal effectiveness, run the tests under the
// race detector.
public static void TestConn(ж<testing.T> Ꮡt, MakePipe mp) {
    ref var t = ref Ꮡt.val;

    t.Run("BasicIO"u8, (ж<testing.T> t) => {
        timeoutWrapper(ᏑtΔ1, mp, testBasicIO);
    });
    t.Run("PingPong"u8, (ж<testing.T> t) => {
        timeoutWrapper(ᏑtΔ2, mp, testPingPong);
    });
    t.Run("RacyRead"u8, (ж<testing.T> t) => {
        timeoutWrapper(ᏑtΔ3, mp, testRacyRead);
    });
    t.Run("RacyWrite"u8, (ж<testing.T> t) => {
        timeoutWrapper(ᏑtΔ4, mp, testRacyWrite);
    });
    t.Run("ReadTimeout"u8, (ж<testing.T> t) => {
        timeoutWrapper(ᏑtΔ5, mp, testReadTimeout);
    });
    t.Run("WriteTimeout"u8, (ж<testing.T> t) => {
        timeoutWrapper(ᏑtΔ6, mp, testWriteTimeout);
    });
    t.Run("PastTimeout"u8, (ж<testing.T> t) => {
        timeoutWrapper(ᏑtΔ7, mp, testPastTimeout);
    });
    t.Run("PresentTimeout"u8, (ж<testing.T> t) => {
        timeoutWrapper(ᏑtΔ8, mp, testPresentTimeout);
    });
    t.Run("FutureTimeout"u8, (ж<testing.T> t) => {
        timeoutWrapper(ᏑtΔ9, mp, testFutureTimeout);
    });
    t.Run("CloseTimeout"u8, (ж<testing.T> t) => {
        timeoutWrapper(ᏑtΔ10, mp, testCloseTimeout);
    });
    t.Run("ConcurrentMethods"u8, (ж<testing.T> t) => {
        timeoutWrapper(ᏑtΔ11, mp, testConcurrentMethods);
    });
}

internal delegate void connTester(ж<testing.T> t, net.Conn c1, net.Conn c2);

internal static void timeoutWrapper(ж<testing.T> Ꮡt, MakePipe mp, connTester f) => func((defer, _) => {
    ref var t = ref Ꮡt.val;

    t.Helper();
    var c1 = mp();
    var c2 = mp();
    var stop = mp();
    var err = mp();
    if (err != default!) {
        t.Fatalf("unable to make pipe: %v"u8, err);
    }
    ref var once = ref heap(new sync_package.Once(), out var Ꮡonce);
    deferǃ(onceʗ1.Do, 
    var stopʗ2 = stop;
    () => {
        stopʗ2();
    }, defer);
    var timer = time.AfterFunc(time.ΔMinute, 
    var onceʗ2 = once;
    var stopʗ4 = stop;
    () => {
        onceʗ2.Do(
        var stopʗ6 = stop;
        () => {
            t.Error("test timed out; terminating pipe");
            stopʗ6();
        });
    });
    var timerʗ1 = timer;
    defer(timerʗ1.Stop);
    f(Ꮡt, c1, c2);
});

// testBasicIO tests that the data sent on c1 is properly received on c2.
internal static void testBasicIO(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) {
    ref var t = ref Ꮡt.val;

    var want = new slice<byte>(1 << (int)(20));
    rand.New(rand.NewSource(0)).Read(want);
    var dataCh = new channel<slice<byte>>(1);
    var wantʗ1 = want;
    goǃ(() => {
        var rd = bytes.NewReader(wantʗ1);
        {
            var err = chunkedCopy(c1, ~rd); if (err != default!) {
                t.Errorf("unexpected c1.Write error: %v"u8, err);
            }
        }
        {
            var err = c1.Close(); if (err != default!) {
                t.Errorf("unexpected c1.Close error: %v"u8, err);
            }
        }
    });
    var dataChʗ1 = dataCh;
    goǃ(() => {
        var wr = @new<bytes.Buffer>();
        {
            var err = chunkedCopy(~wr, c2); if (err != default!) {
                t.Errorf("unexpected c2.Read error: %v"u8, err);
            }
        }
        {
            var err = c2.Close(); if (err != default!) {
                t.Errorf("unexpected c2.Close error: %v"u8, err);
            }
        }
        dataChʗ1.ᐸꟷ(wr.Bytes());
    });
    {
        var got = ᐸꟷ(dataCh); if (!bytes.Equal(got, want)) {
            t.Error("transmitted data differs");
        }
    }
}

// testPingPong tests that the two endpoints can synchronously send data to
// each other in a typical request-response pattern.
internal static void testPingPong(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) => func((defer, _) => {
    ref var t = ref Ꮡt.val;

    ref var wg = ref heap(new sync_package.WaitGroup(), out var Ꮡwg);
    var wgʗ1 = wg;
    defer(wgʗ1.Wait);
    var pingPonger = 
    var wgʗ2 = wg;
    (net.Conn c) => {
        var wgʗ3 = wg;
        defer(wgʗ3.Done);
        var buf = new slice<byte>(8);
        uint64 prev = default!;
        while (ᐧ) {
            {
                var (_, err) = io.ReadFull(c, buf); if (err != default!) {
                    if (AreEqual(err, io.EOF)) {
                        break;
                    }
                    t.Errorf("unexpected Read error: %v"u8, err);
                }
            }
            var v = binary.LittleEndian.Uint64(buf);
            binary.LittleEndian.PutUint64(buf, v + 1);
            if (prev != 0 && prev + 2 != v) {
                t.Errorf("mismatching value: got %d, want %d"u8, v, prev + 2);
            }
            prev = v;
            if (v == 1000) {
                break;
            }
            {
                var (_, err) = c.Write(buf); if (err != default!) {
                    t.Errorf("unexpected Write error: %v"u8, err);
                    break;
                }
            }
        }
        {
            var err = c.Close(); if (err != default!) {
                t.Errorf("unexpected Close error: %v"u8, err);
            }
        }
    };
    wg.Add(2);
    var pingPongerʗ1 = pingPonger;
    goǃ(pingPongerʗ1, c1);
    var pingPongerʗ2 = pingPonger;
    goǃ(pingPongerʗ2, c2);
    // Start off the chain reaction.
    {
        var (_, err) = c1.Write(new slice<byte>(8)); if (err != default!) {
            t.Errorf("unexpected c1.Write error: %v"u8, err);
        }
    }
});

// testRacyRead tests that it is safe to mutate the input Read buffer
// immediately after cancelation has occurred.
internal static void testRacyRead(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) => func((defer, _) => {
    ref var t = ref Ꮡt.val;

    goǃ(chunkedCopy, c2, ~rand.New(rand.NewSource(0)));
    ref var wg = ref heap(new sync_package.WaitGroup(), out var Ꮡwg);
    var wgʗ1 = wg;
    defer(wgʗ1.Wait);
    c1.SetReadDeadline(time.Now().Add(time.Millisecond));
    for (nint i = 0; i < 10; i++) {
        wg.Add(1);
        var wgʗ2 = wg;
        goǃ(() => {
            var wgʗ3 = wg;
            defer(wgʗ3.Done);
            var b1 = new slice<byte>(1024);
            var b2 = new slice<byte>(1024);
            for (nint j = 0; j < 100; j++) {
                var (_, err) = c1.Read(b1);
                copy(b1, b2);
                // Mutate b1 to trigger potential race
                if (err != default!) {
                    checkForTimeoutError(Ꮡt, err);
                    c1.SetReadDeadline(time.Now().Add(time.Millisecond));
                }
            }
        });
    }
});

// testRacyWrite tests that it is safe to mutate the input Write buffer
// immediately after cancelation has occurred.
internal static void testRacyWrite(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) => func((defer, _) => {
    ref var t = ref Ꮡt.val;

    goǃ(chunkedCopy, io.Discard, c2);
    ref var wg = ref heap(new sync_package.WaitGroup(), out var Ꮡwg);
    var wgʗ1 = wg;
    defer(wgʗ1.Wait);
    c1.SetWriteDeadline(time.Now().Add(time.Millisecond));
    for (nint i = 0; i < 10; i++) {
        wg.Add(1);
        var wgʗ2 = wg;
        goǃ(() => {
            var wgʗ3 = wg;
            defer(wgʗ3.Done);
            var b1 = new slice<byte>(1024);
            var b2 = new slice<byte>(1024);
            for (nint j = 0; j < 100; j++) {
                var (_, err) = c1.Write(b1);
                copy(b1, b2);
                // Mutate b1 to trigger potential race
                if (err != default!) {
                    checkForTimeoutError(Ꮡt, err);
                    c1.SetWriteDeadline(time.Now().Add(time.Millisecond));
                }
            }
        });
    }
});

// testReadTimeout tests that Read timeouts do not affect Write.
internal static void testReadTimeout(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) {
    ref var t = ref Ꮡt.val;

    goǃ(chunkedCopy, io.Discard, c2);
    c1.SetReadDeadline(aLongTimeAgo);
    var (_, err) = c1.Read(new slice<byte>(1024));
    checkForTimeoutError(Ꮡt, err);
    {
        var (_, errΔ1) = c1.Write(new slice<byte>(1024)); if (errΔ1 != default!) {
            t.Errorf("unexpected Write error: %v"u8, errΔ1);
        }
    }
}

// testWriteTimeout tests that Write timeouts do not affect Read.
internal static void testWriteTimeout(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) {
    ref var t = ref Ꮡt.val;

    goǃ(chunkedCopy, c2, ~rand.New(rand.NewSource(0)));
    c1.SetWriteDeadline(aLongTimeAgo);
    var (_, err) = c1.Write(new slice<byte>(1024));
    checkForTimeoutError(Ꮡt, err);
    {
        var (_, errΔ1) = c1.Read(new slice<byte>(1024)); if (errΔ1 != default!) {
            t.Errorf("unexpected Read error: %v"u8, errΔ1);
        }
    }
}

// testPastTimeout tests that a deadline set in the past immediately times out
// Read and Write requests.
internal static void testPastTimeout(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) {
    ref var t = ref Ꮡt.val;

    goǃ(chunkedCopy, c2, c2);
    testRoundtrip(Ꮡt, c1);
    c1.SetDeadline(aLongTimeAgo);
    var (n, err) = c1.Write(new slice<byte>(1024));
    if (n != 0) {
        t.Errorf("unexpected Write count: got %d, want 0"u8, n);
    }
    checkForTimeoutError(Ꮡt, err);
    (n, err) = c1.Read(new slice<byte>(1024));
    if (n != 0) {
        t.Errorf("unexpected Read count: got %d, want 0"u8, n);
    }
    checkForTimeoutError(Ꮡt, err);
    testRoundtrip(Ꮡt, c1);
}

// testPresentTimeout tests that a past deadline set while there are pending
// Read and Write operations immediately times out those operations.
internal static void testPresentTimeout(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) => func((defer, _) => {
    ref var t = ref Ꮡt.val;

    ref var wg = ref heap(new sync_package.WaitGroup(), out var Ꮡwg);
    var wgʗ1 = wg;
    defer(wgʗ1.Wait);
    wg.Add(3);
    var deadlineSet = new channel<bool>(1);
    var aLongTimeAgoʗ1 = aLongTimeAgo;
    var deadlineSetʗ1 = deadlineSet;
    var wgʗ2 = wg;
    goǃ(() => {
        var wgʗ3 = wg;
        defer(wgʗ3.Done);
        time.Sleep(100 * time.Millisecond);
        deadlineSet.ᐸꟷ(true);
        c1.SetReadDeadline(aLongTimeAgo);
        c1.SetWriteDeadline(aLongTimeAgo);
    });
    var deadlineSetʗ2 = deadlineSet;
    var wgʗ4 = wg;
    goǃ(() => {
        var wgʗ5 = wg;
        defer(wgʗ5.Done);
        var (n, err) = c1.Read(new slice<byte>(1024));
        if (n != 0) {
            t.Errorf("unexpected Read count: got %d, want 0"u8, n);
        }
        checkForTimeoutError(Ꮡt, err);
        if (len(deadlineSet) == 0) {
            t.Error("Read timed out before deadline is set");
        }
    });
    var deadlineSetʗ3 = deadlineSet;
    var wgʗ6 = wg;
    goǃ(() => {
        var wgʗ7 = wg;
        defer(wgʗ7.Done);
        error err = default!;
        while (err == default!) {
            (_, err) = c1.Write(new slice<byte>(1024));
        }
        checkForTimeoutError(Ꮡt, err);
        if (len(deadlineSet) == 0) {
            t.Error("Write timed out before deadline is set");
        }
    });
});

// testFutureTimeout tests that a future deadline will eventually time out
// Read and Write operations.
internal static void testFutureTimeout(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) => func((defer, _) => {
    ref var t = ref Ꮡt.val;

    ref var wg = ref heap(new sync_package.WaitGroup(), out var Ꮡwg);
    wg.Add(2);
    c1.SetDeadline(time.Now().Add(100 * time.Millisecond));
    var wgʗ1 = wg;
    goǃ(() => {
        var wgʗ2 = wg;
        defer(wgʗ2.Done);
        var (_, err) = c1.Read(new slice<byte>(1024));
        checkForTimeoutError(Ꮡt, err);
    });
    var wgʗ3 = wg;
    goǃ(() => {
        var wgʗ4 = wg;
        defer(wgʗ4.Done);
        error err = default!;
        while (err == default!) {
            (_, err) = c1.Write(new slice<byte>(1024));
        }
        checkForTimeoutError(Ꮡt, err);
    });
    wg.Wait();
    goǃ(chunkedCopy, c2, c2);
    resyncConn(Ꮡt, c1);
    testRoundtrip(Ꮡt, c1);
});

// testCloseTimeout tests that calling Close immediately times out pending
// Read and Write operations.
internal static void testCloseTimeout(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) => func((defer, _) => {
    ref var t = ref Ꮡt.val;

    goǃ(chunkedCopy, c2, c2);
    ref var wg = ref heap(new sync_package.WaitGroup(), out var Ꮡwg);
    var wgʗ1 = wg;
    defer(wgʗ1.Wait);
    wg.Add(3);
    // Test for cancelation upon connection closure.
    c1.SetDeadline(neverTimeout);
    var wgʗ2 = wg;
    goǃ(() => {
        var wgʗ3 = wg;
        defer(wgʗ3.Done);
        time.Sleep(100 * time.Millisecond);
        c1.Close();
    });
    var wgʗ4 = wg;
    goǃ(() => {
        var wgʗ5 = wg;
        defer(wgʗ5.Done);
        error err = default!;
        var buf = new slice<byte>(1024);
        while (err == default!) {
            (_, err) = c1.Read(buf);
        }
    });
    var wgʗ6 = wg;
    goǃ(() => {
        var wgʗ7 = wg;
        defer(wgʗ7.Done);
        error err = default!;
        var buf = new slice<byte>(1024);
        while (err == default!) {
            (_, err) = c1.Write(buf);
        }
    });
});

// testConcurrentMethods tests that the methods of net.Conn can safely
// be called concurrently.
internal static void testConcurrentMethods(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) => func((defer, _) => {
    ref var t = ref Ꮡt.val;

    if (runtime.GOOS == "plan9"u8) {
        t.Skip("skipping on plan9; see https://golang.org/issue/20489");
    }
    goǃ(chunkedCopy, c2, c2);
    // The results of the calls may be nonsensical, but this should
    // not trigger a race detector warning.
    ref var wg = ref heap(new sync_package.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < 100; i++) {
        wg.Add(7);
        var wgʗ1 = wg;
        goǃ(() => {
            var wgʗ2 = wg;
            defer(wgʗ2.Done);
            c1.Read(new slice<byte>(1024));
        });
        var wgʗ3 = wg;
        goǃ(() => {
            var wgʗ4 = wg;
            defer(wgʗ4.Done);
            c1.Write(new slice<byte>(1024));
        });
        var wgʗ5 = wg;
        goǃ(() => {
            var wgʗ6 = wg;
            defer(wgʗ6.Done);
            c1.SetDeadline(time.Now().Add(10 * time.Millisecond));
        });
        var aLongTimeAgoʗ1 = aLongTimeAgo;
        var wgʗ7 = wg;
        goǃ(() => {
            var wgʗ8 = wg;
            defer(wgʗ8.Done);
            c1.SetReadDeadline(aLongTimeAgo);
        });
        var aLongTimeAgoʗ2 = aLongTimeAgo;
        var wgʗ9 = wg;
        goǃ(() => {
            var wgʗ10 = wg;
            defer(wgʗ10.Done);
            c1.SetWriteDeadline(aLongTimeAgo);
        });
        var wgʗ11 = wg;
        goǃ(() => {
            var wgʗ12 = wg;
            defer(wgʗ12.Done);
            c1.LocalAddr();
        });
        var wgʗ13 = wg;
        goǃ(() => {
            var wgʗ14 = wg;
            defer(wgʗ14.Done);
            c1.RemoteAddr();
        });
    }
    wg.Wait();
    // At worst, the deadline is set 10ms into the future
    resyncConn(Ꮡt, c1);
    testRoundtrip(Ꮡt, c1);
});

// checkForTimeoutError checks that the error satisfies the Error interface
// and that Timeout returns true.
internal static void checkForTimeoutError(ж<testing.T> Ꮡt, error err) {
    ref var t = ref Ꮡt.val;

    t.Helper();
    {
        var (nerr, ok) = err._<netꓸError>(ᐧ); if (ok){
            if (!nerr.Timeout()) {
                if (runtime.GOOS == "windows"u8 && runtime.GOARCH == "arm64"u8 && t.Name() == "TestTestConn/TCP/RacyRead"u8){
                    t.Logf("ignoring known failure mode on windows/arm64; see https://go.dev/issue/52893"u8);
                } else {
                    t.Errorf("got error: %v, want err.Timeout() = true"u8, nerr);
                }
            }
        } else {
            t.Errorf("got %T: %v, want net.Error"u8, err, err);
        }
    }
}

// testRoundtrip writes something into c and reads it back.
// It assumes that everything written into c is echoed back to itself.
internal static void testRoundtrip(ж<testing.T> Ꮡt, net.Conn c) {
    ref var t = ref Ꮡt.val;

    t.Helper();
    {
        var err = c.SetDeadline(neverTimeout); if (err != default!) {
            t.Errorf("roundtrip SetDeadline error: %v"u8, err);
        }
    }
    @string s = "Hello, world!"u8;
    var buf = slice<byte>(s);
    {
        var (_, err) = c.Write(buf); if (err != default!) {
            t.Errorf("roundtrip Write error: %v"u8, err);
        }
    }
    {
        var (_, err) = io.ReadFull(c, buf); if (err != default!) {
            t.Errorf("roundtrip Read error: %v"u8, err);
        }
    }
    if (((@string)buf) != s) {
        t.Errorf("roundtrip data mismatch: got %q, want %q"u8, buf, s);
    }
}

// resyncConn resynchronizes the connection into a sane state.
// It assumes that everything written into c is echoed back to itself.
// It assumes that 0xff is not currently on the wire or in the read buffer.
internal static void resyncConn(ж<testing.T> Ꮡt, net.Conn c) {
    ref var t = ref Ꮡt.val;

    t.Helper();
    c.SetDeadline(neverTimeout);
    var errCh = new channel<error>(1);
    var errChʗ1 = errCh;
    goǃ(() => {
        var (_, err) = c.Write(new byte[]{255}.slice());
        errChʗ1.ᐸꟷ(err);
    });
    var buf = new slice<byte>(1024);
    while (ᐧ) {
        var (n, err) = c.Read(buf);
        if (n > 0 && bytes.IndexByte(buf[..(int)(n)], 255) == n - 1) {
            break;
        }
        if (err != default!) {
            t.Errorf("unexpected Read error: %v"u8, err);
            break;
        }
    }
    {
        var err = ᐸꟷ(errCh); if (err != default!) {
            t.Errorf("unexpected Write error: %v"u8, err);
        }
    }
}

[GoType("dyn")] partial struct chunkedCopy_dst {
    public partial ref io_package.Writer Writer { get; }
}

[GoType("dyn")] partial struct chunkedCopy_src {
    public partial ref io_package.Reader Reader { get; }
}

// chunkedCopy copies from r to w in fixed-width chunks to avoid
// causing a Write that exceeds the maximum packet size for packet-based
// connections like "unixpacket".
// We assume that the maximum packet size is at least 1024.
internal static error chunkedCopy(io.Writer w, io.Reader r) {
    var b = new slice<byte>(1024);
    var (_, err) = io.CopyBuffer(new chunkedCopy_dst(w), new chunkedCopy_src(r), b);
    return err;
}

} // end nettest_package
