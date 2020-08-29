// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package nettest provides utilities for network testing.
// package nettest -- go2cs converted at 2020 August 29 10:12:23 UTC
// import "vendor/golang_org/x/net/nettest" ==> using nettest = go.vendor.golang_org.x.net.nettest_package
// Original source: C:\Go\src\vendor\golang_org\x\net\nettest\conntest.go
using bytes = go.bytes_package;
using binary = go.encoding.binary_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using rand = go.math.rand_package;
using net = go.net_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using testing = go.testing_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class nettest_package
    {
        private static var aLongTimeAgo = time.Unix(233431200L, 0L);        private static time.Time neverTimeout = new time.Time();

        // MakePipe creates a connection between two endpoints and returns the pair
        // as c1 and c2, such that anything written to c1 is read by c2 and vice-versa.
        // The stop function closes all resources, including c1, c2, and the underlying
        // net.Listener (if there is one), and should not be nil.
        public delegate  error) MakePipe((net.Conn,  net.Conn,  Action);

        // TestConn tests that a net.Conn implementation properly satisfies the interface.
        // The tests should not produce any false positives, but may experience
        // false negatives. Thus, some issues may only be detected when the test is
        // run multiple times. For maximal effectiveness, run the tests under the
        // race detector.
        public static void TestConn(ref testing.T t, MakePipe mp)
        {
            testConn(t, mp);
        }

        public delegate void connTester(ref testing.T, net.Conn, net.Conn);

        private static void timeoutWrapper(ref testing.T _t, MakePipe mp, connTester f) => func(_t, (ref testing.T t, Defer defer, Panic _, Recover __) =>
        {
            var (c1, c2, stop, err) = mp();
            if (err != null)
            {
                t.Fatalf("unable to make pipe: %v", err);
            }
            sync.Once once = default;
            defer(once.Do(() =>
            {
                stop();

            }));
            var timer = time.AfterFunc(time.Minute, () =>
            {
                once.Do(() =>
                {
                    t.Error("test timed out; terminating pipe");
                    stop();
                });
            });
            defer(timer.Stop());
            f(t, c1, c2);
        });

        // testBasicIO tests that the data sent on c1 is properly received on c2.
        private static void testBasicIO(ref testing.T t, net.Conn c1, net.Conn c2)
        {
            var want = make_slice<byte>(1L << (int)(20L));
            rand.New(rand.NewSource(0L)).Read(want);

            var dataCh = make_channel<slice<byte>>();
            go_(() => () =>
            {
                var rd = bytes.NewReader(want);
                {
                    var err__prev1 = err;

                    var err = chunkedCopy(c1, rd);

                    if (err != null)
                    {
                        t.Errorf("unexpected c1.Write error: %v", err);
                    }

                    err = err__prev1;

                }
                {
                    var err__prev1 = err;

                    err = c1.Close();

                    if (err != null)
                    {
                        t.Errorf("unexpected c1.Close error: %v", err);
                    }

                    err = err__prev1;

                }
            }());

            go_(() => () =>
            {
                ptr<object> wr = @new<bytes.Buffer>();
                {
                    var err__prev1 = err;

                    err = chunkedCopy(wr, c2);

                    if (err != null)
                    {
                        t.Errorf("unexpected c2.Read error: %v", err);
                    }

                    err = err__prev1;

                }
                {
                    var err__prev1 = err;

                    err = c2.Close();

                    if (err != null)
                    {
                        t.Errorf("unexpected c2.Close error: %v", err);
                    }

                    err = err__prev1;

                }
                dataCh.Send(wr.Bytes());
            }());

            {
                var got = dataCh.Receive();

                if (!bytes.Equal(got, want))
                {
                    t.Errorf("transmitted data differs");
                }

            }
        }

        // testPingPong tests that the two endpoints can synchronously send data to
        // each other in a typical request-response pattern.
        private static void testPingPong(ref testing.T _t, net.Conn c1, net.Conn c2) => func(_t, (ref testing.T t, Defer defer, Panic _, Recover __) =>
        {
            sync.WaitGroup wg = default;
            defer(wg.Wait());

            Action<net.Conn> pingPonger = c =>
            {
                defer(wg.Done());
                var buf = make_slice<byte>(8L);
                ulong prev = default;
                while (true)
                {
                    {
                        var (_, err) = io.ReadFull(c, buf);

                        if (err != null)
                        {
                            if (err == io.EOF)
                            {
                                break;
                            }
                            t.Errorf("unexpected Read error: %v", err);
                        }

                    }

                    var v = binary.LittleEndian.Uint64(buf);
                    binary.LittleEndian.PutUint64(buf, v + 1L);
                    if (prev != 0L && prev + 2L != v)
                    {
                        t.Errorf("mismatching value: got %d, want %d", v, prev + 2L);
                    }
                    prev = v;
                    if (v == 1000L)
                    {
                        break;
                    }
                    {
                        (_, err) = c.Write(buf);

                        if (err != null)
                        {
                            t.Errorf("unexpected Write error: %v", err);
                            break;
                        }

                    }
                }

                {
                    var err = c.Close();

                    if (err != null)
                    {
                        t.Errorf("unexpected Close error: %v", err);
                    }

                }
            }
;

            wg.Add(2L);
            go_(() => pingPonger(c1));
            go_(() => pingPonger(c2)); 

            // Start off the chain reaction.
            {
                (_, err) = c1.Write(make_slice<byte>(8L));

                if (err != null)
                {
                    t.Errorf("unexpected c1.Write error: %v", err);
                }

            }
        });

        // testRacyRead tests that it is safe to mutate the input Read buffer
        // immediately after cancelation has occurred.
        private static void testRacyRead(ref testing.T _t, net.Conn c1, net.Conn c2) => func(_t, (ref testing.T t, Defer defer, Panic _, Recover __) =>
        {
            go_(() => chunkedCopy(c2, rand.New(rand.NewSource(0L))));

            sync.WaitGroup wg = default;
            defer(wg.Wait());

            c1.SetReadDeadline(time.Now().Add(time.Millisecond));
            for (long i = 0L; i < 10L; i++)
            {
                wg.Add(1L);
                go_(() => () =>
                {
                    defer(wg.Done());

                    var b1 = make_slice<byte>(1024L);
                    var b2 = make_slice<byte>(1024L);
                    for (long j = 0L; j < 100L; j++)
                    {
                        var (_, err) = c1.Read(b1);
                        copy(b1, b2); // Mutate b1 to trigger potential race
                        if (err != null)
                        {
                            checkForTimeoutError(t, err);
                            c1.SetReadDeadline(time.Now().Add(time.Millisecond));
                        }
                    }

                }());
            }

        });

        // testRacyWrite tests that it is safe to mutate the input Write buffer
        // immediately after cancelation has occurred.
        private static void testRacyWrite(ref testing.T _t, net.Conn c1, net.Conn c2) => func(_t, (ref testing.T t, Defer defer, Panic _, Recover __) =>
        {
            go_(() => chunkedCopy(ioutil.Discard, c2));

            sync.WaitGroup wg = default;
            defer(wg.Wait());

            c1.SetWriteDeadline(time.Now().Add(time.Millisecond));
            for (long i = 0L; i < 10L; i++)
            {
                wg.Add(1L);
                go_(() => () =>
                {
                    defer(wg.Done());

                    var b1 = make_slice<byte>(1024L);
                    var b2 = make_slice<byte>(1024L);
                    for (long j = 0L; j < 100L; j++)
                    {
                        var (_, err) = c1.Write(b1);
                        copy(b1, b2); // Mutate b1 to trigger potential race
                        if (err != null)
                        {
                            checkForTimeoutError(t, err);
                            c1.SetWriteDeadline(time.Now().Add(time.Millisecond));
                        }
                    }

                }());
            }

        });

        // testReadTimeout tests that Read timeouts do not affect Write.
        private static void testReadTimeout(ref testing.T t, net.Conn c1, net.Conn c2)
        {
            go_(() => chunkedCopy(ioutil.Discard, c2));

            c1.SetReadDeadline(aLongTimeAgo);
            var (_, err) = c1.Read(make_slice<byte>(1024L));
            checkForTimeoutError(t, err);
            {
                (_, err) = c1.Write(make_slice<byte>(1024L));

                if (err != null)
                {
                    t.Errorf("unexpected Write error: %v", err);
                }

            }
        }

        // testWriteTimeout tests that Write timeouts do not affect Read.
        private static void testWriteTimeout(ref testing.T t, net.Conn c1, net.Conn c2)
        {
            go_(() => chunkedCopy(c2, rand.New(rand.NewSource(0L))));

            c1.SetWriteDeadline(aLongTimeAgo);
            var (_, err) = c1.Write(make_slice<byte>(1024L));
            checkForTimeoutError(t, err);
            {
                (_, err) = c1.Read(make_slice<byte>(1024L));

                if (err != null)
                {
                    t.Errorf("unexpected Read error: %v", err);
                }

            }
        }

        // testPastTimeout tests that a deadline set in the past immediately times out
        // Read and Write requests.
        private static void testPastTimeout(ref testing.T t, net.Conn c1, net.Conn c2)
        {
            go_(() => chunkedCopy(c2, c2));

            testRoundtrip(t, c1);

            c1.SetDeadline(aLongTimeAgo);
            var (n, err) = c1.Write(make_slice<byte>(1024L));
            if (n != 0L)
            {
                t.Errorf("unexpected Write count: got %d, want 0", n);
            }
            checkForTimeoutError(t, err);
            n, err = c1.Read(make_slice<byte>(1024L));
            if (n != 0L)
            {
                t.Errorf("unexpected Read count: got %d, want 0", n);
            }
            checkForTimeoutError(t, err);

            testRoundtrip(t, c1);
        }

        // testPresentTimeout tests that a deadline set while there are pending
        // Read and Write operations immediately times out those operations.
        private static void testPresentTimeout(ref testing.T _t, net.Conn c1, net.Conn c2) => func(_t, (ref testing.T t, Defer defer, Panic _, Recover __) =>
        {
            sync.WaitGroup wg = default;
            defer(wg.Wait());
            wg.Add(3L);

            var deadlineSet = make_channel<bool>(1L);
            go_(() => () =>
            {
                defer(wg.Done());
                time.Sleep(100L * time.Millisecond);
                deadlineSet.Send(true);
                c1.SetReadDeadline(aLongTimeAgo);
                c1.SetWriteDeadline(aLongTimeAgo);
            }());
            go_(() => () =>
            {
                defer(wg.Done());
                var (n, err) = c1.Read(make_slice<byte>(1024L));
                if (n != 0L)
                {
                    t.Errorf("unexpected Read count: got %d, want 0", n);
                }
                checkForTimeoutError(t, err);
                if (len(deadlineSet) == 0L)
                {
                    t.Error("Read timed out before deadline is set");
                }
            }());
            go_(() => () =>
            {
                defer(wg.Done());
                error err = default;
                while (err == null)
                {
                    _, err = c1.Write(make_slice<byte>(1024L));
                }

                checkForTimeoutError(t, err);
                if (len(deadlineSet) == 0L)
                {
                    t.Error("Write timed out before deadline is set");
                }
            }());
        });

        // testFutureTimeout tests that a future deadline will eventually time out
        // Read and Write operations.
        private static void testFutureTimeout(ref testing.T _t, net.Conn c1, net.Conn c2) => func(_t, (ref testing.T t, Defer defer, Panic _, Recover __) =>
        {
            sync.WaitGroup wg = default;
            wg.Add(2L);

            c1.SetDeadline(time.Now().Add(100L * time.Millisecond));
            go_(() => () =>
            {
                defer(wg.Done());
                var (_, err) = c1.Read(make_slice<byte>(1024L));
                checkForTimeoutError(t, err);
            }());
            go_(() => () =>
            {
                defer(wg.Done());
                error err = default;
                while (err == null)
                {
                    _, err = c1.Write(make_slice<byte>(1024L));
                }

                checkForTimeoutError(t, err);
            }());
            wg.Wait();

            go_(() => chunkedCopy(c2, c2));
            resyncConn(t, c1);
            testRoundtrip(t, c1);
        });

        // testCloseTimeout tests that calling Close immediately times out pending
        // Read and Write operations.
        private static void testCloseTimeout(ref testing.T _t, net.Conn c1, net.Conn c2) => func(_t, (ref testing.T t, Defer defer, Panic _, Recover __) =>
        {
            go_(() => chunkedCopy(c2, c2));

            sync.WaitGroup wg = default;
            defer(wg.Wait());
            wg.Add(3L); 

            // Test for cancelation upon connection closure.
            c1.SetDeadline(neverTimeout);
            go_(() => () =>
            {
                defer(wg.Done());
                time.Sleep(100L * time.Millisecond);
                c1.Close();
            }());
            go_(() => () =>
            {
                defer(wg.Done());
                error err = default;
                var buf = make_slice<byte>(1024L);
                while (err == null)
                {
                    _, err = c1.Read(buf);
                }

            }());
            go_(() => () =>
            {
                defer(wg.Done());
                err = default;
                buf = make_slice<byte>(1024L);
                while (err == null)
                {
                    _, err = c1.Write(buf);
                }

            }());
        });

        // testConcurrentMethods tests that the methods of net.Conn can safely
        // be called concurrently.
        private static void testConcurrentMethods(ref testing.T _t, net.Conn c1, net.Conn c2) => func(_t, (ref testing.T t, Defer defer, Panic _, Recover __) =>
        {
            if (runtime.GOOS == "plan9")
            {
                t.Skip("skipping on plan9; see https://golang.org/issue/20489");
            }
            go_(() => chunkedCopy(c2, c2)); 

            // The results of the calls may be nonsensical, but this should
            // not trigger a race detector warning.
            sync.WaitGroup wg = default;
            for (long i = 0L; i < 100L; i++)
            {
                wg.Add(7L);
                go_(() => () =>
                {
                    defer(wg.Done());
                    c1.Read(make_slice<byte>(1024L));
                }());
                go_(() => () =>
                {
                    defer(wg.Done());
                    c1.Write(make_slice<byte>(1024L));
                }());
                go_(() => () =>
                {
                    defer(wg.Done());
                    c1.SetDeadline(time.Now().Add(10L * time.Millisecond));
                }());
                go_(() => () =>
                {
                    defer(wg.Done());
                    c1.SetReadDeadline(aLongTimeAgo);
                }());
                go_(() => () =>
                {
                    defer(wg.Done());
                    c1.SetWriteDeadline(aLongTimeAgo);
                }());
                go_(() => () =>
                {
                    defer(wg.Done());
                    c1.LocalAddr();
                }());
                go_(() => () =>
                {
                    defer(wg.Done());
                    c1.RemoteAddr();
                }());
            }

            wg.Wait(); // At worst, the deadline is set 10ms into the future

            resyncConn(t, c1);
            testRoundtrip(t, c1);
        });

        // checkForTimeoutError checks that the error satisfies the Error interface
        // and that Timeout returns true.
        private static void checkForTimeoutError(ref testing.T t, error err)
        {
            {
                net.Error (nerr, ok) = err._<net.Error>();

                if (ok)
                {
                    if (!nerr.Timeout())
                    {
                        t.Errorf("err.Timeout() = false, want true");
                    }
                }
                else
                {
                    t.Errorf("got %T, want net.Error", err);
                }

            }
        }

        // testRoundtrip writes something into c and reads it back.
        // It assumes that everything written into c is echoed back to itself.
        private static void testRoundtrip(ref testing.T t, net.Conn c)
        {
            {
                var err = c.SetDeadline(neverTimeout);

                if (err != null)
                {
                    t.Errorf("roundtrip SetDeadline error: %v", err);
                }

            }

            const @string s = "Hello, world!";

            slice<byte> buf = (slice<byte>)s;
            {
                var (_, err) = c.Write(buf);

                if (err != null)
                {
                    t.Errorf("roundtrip Write error: %v", err);
                }

            }
            {
                (_, err) = io.ReadFull(c, buf);

                if (err != null)
                {
                    t.Errorf("roundtrip Read error: %v", err);
                }

            }
            if (string(buf) != s)
            {
                t.Errorf("roundtrip data mismatch: got %q, want %q", buf, s);
            }
        }

        // resyncConn resynchronizes the connection into a sane state.
        // It assumes that everything written into c is echoed back to itself.
        // It assumes that 0xff is not currently on the wire or in the read buffer.
        private static void resyncConn(ref testing.T t, net.Conn c)
        {
            c.SetDeadline(neverTimeout);
            var errCh = make_channel<error>();
            go_(() => () =>
            {
                var (_, err) = c.Write(new slice<byte>(new byte[] { 0xff }));
                errCh.Send(err);
            }());
            var buf = make_slice<byte>(1024L);
            while (true)
            {
                var (n, err) = c.Read(buf);
                if (n > 0L && bytes.IndexByte(buf[..n], 0xffUL) == n - 1L)
                {
                    break;
                }
                if (err != null)
                {
                    t.Errorf("unexpected Read error: %v", err);
                    break;
                }
            }

            {
                var err = errCh.Receive();

                if (err != null)
                {
                    t.Errorf("unexpected Write error: %v", err);
                }

            }
        }

        // chunkedCopy copies from r to w in fixed-width chunks to avoid
        // causing a Write that exceeds the maximum packet size for packet-based
        // connections like "unixpacket".
        // We assume that the maximum packet size is at least 1024.
        private static error chunkedCopy(io.Writer w, io.Reader r)
        {
            var b = make_slice<byte>(1024L);
            var (_, err) = io.CopyBuffer(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{io.Writer}{w}, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{io.Reader}{r}, b);
            return error.As(err);
        }
    }
}}}}}
