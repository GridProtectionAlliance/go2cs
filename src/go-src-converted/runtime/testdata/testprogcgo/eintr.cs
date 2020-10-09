// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2020 October 09 05:00:57 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\eintr.go
/*
#include <errno.h>
#include <signal.h>
#include <string.h>

static int clearRestart(int sig) {
    struct sigaction sa;

    memset(&sa, 0, sizeof sa);
    if (sigaction(sig, NULL, &sa) < 0) {
        return errno;
    }
    sa.sa_flags &=~ SA_RESTART;
    if (sigaction(sig, &sa, NULL) < 0) {
        return errno;
    }
    return 0;
}
*/
using C = go.C_package;/*
#include <errno.h>
#include <signal.h>
#include <string.h>

static int clearRestart(int sig) {
    struct sigaction sa;

    memset(&sa, 0, sizeof sa);
    if (sigaction(sig, NULL, &sa) < 0) {
        return errno;
    }
    sa.sa_flags &=~ SA_RESTART;
    if (sigaction(sig, &sa, NULL) < 0) {
        return errno;
    }
    return 0;
}
*/


using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using net = go.net_package;
using os = go.os_package;
using exec = go.os.exec_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("EINTR", EINTR);
            register("Block", Block);
        }

        // Test various operations when a signal handler is installed without
        // the SA_RESTART flag. This tests that the os and net APIs handle EINTR.
        public static void EINTR()
        {
            {
                var errno__prev1 = errno;

                var errno = C.clearRestart(C.@int(syscall.SIGURG));

                if (errno != 0L)
                {
                    log.Fatal(syscall.Errno(errno));
                }

                errno = errno__prev1;

            }

            {
                var errno__prev1 = errno;

                errno = C.clearRestart(C.@int(syscall.SIGWINCH));

                if (errno != 0L)
                {
                    log.Fatal(syscall.Errno(errno));
                }

                errno = errno__prev1;

            }

            {
                var errno__prev1 = errno;

                errno = C.clearRestart(C.@int(syscall.SIGCHLD));

                if (errno != 0L)
                {
                    log.Fatal(syscall.Errno(errno));
                }

                errno = errno__prev1;

            }


            ref sync.WaitGroup wg = ref heap(out ptr<sync.WaitGroup> _addr_wg);
            testPipe(_addr_wg);
            testNet(_addr_wg);
            testExec(_addr_wg);
            wg.Wait();
            fmt.Println("OK");

        }

        // spin does CPU bound spinning and allocating for a millisecond,
        // to get a SIGURG.
        //go:noinline
        private static (double, slice<byte>) spin()
        {
            double _p0 = default;
            slice<byte> _p0 = default;

            var stop = time.Now().Add(time.Millisecond);
            float r1 = 0.0F;
            var r2 = make_slice<byte>(200L);
            while (time.Now().Before(stop))
            {
                for (long i = 1L; i < 1e6F; i++)
                {
                    r1 += r1 / float64(i);
                    r2 = append(r2, bytes.Repeat(new slice<byte>(new byte[] { byte(i) }), 100L));
                    r2 = r2[100L..];
                }


            }

            return (r1, r2);

        }

        // winch sends a few SIGWINCH signals to the process.
        private static void winch() => func((defer, _, __) =>
        {
            var ticker = time.NewTicker(100L * time.Microsecond);
            defer(ticker.Stop());
            var pid = syscall.Getpid();
            for (long n = 10L; n > 0L; n--)
            {
                syscall.Kill(pid, syscall.SIGWINCH).Send(ticker.C);
            }


        });

        // sendSomeSignals triggers a few SIGURG and SIGWINCH signals.
        private static void sendSomeSignals()
        {
            var done = make_channel<object>();
            go_(() => () =>
            {
                spin();
                close(done);
            }());
            winch().Send(done);

        }

        // testPipe tests pipe operations.
        private static void testPipe(ptr<sync.WaitGroup> _addr_wg) => func((defer, _, __) =>
        {
            ref sync.WaitGroup wg = ref _addr_wg.val;

            var (r, w, err) = os.Pipe();
            if (err != null)
            {
                log.Fatal(err);
            }

            {
                var err__prev1 = err;

                var err = syscall.SetNonblock(int(r.Fd()), false);

                if (err != null)
                {
                    log.Fatal(err);
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = syscall.SetNonblock(int(w.Fd()), false);

                if (err != null)
                {
                    log.Fatal(err);
                }

                err = err__prev1;

            }

            wg.Add(2L);
            go_(() => () =>
            {
                defer(wg.Done());
                defer(w.Close()); 
                // Spin before calling Write so that the first ReadFull
                // in the other goroutine will likely be interrupted
                // by a signal.
                sendSomeSignals(); 
                // This Write will likely be interrupted by a signal
                // as the other goroutine spins in the middle of reading.
                // We write enough data that we should always fill the
                // pipe buffer and need multiple write system calls.
                {
                    var err__prev1 = err;

                    var (_, err) = w.Write(bytes.Repeat(new slice<byte>(new byte[] { 0 }), 2L << (int)(20L)));

                    if (err != null)
                    {
                        log.Fatal(err);
                    }

                    err = err__prev1;

                }

            }());
            go_(() => () =>
            {
                defer(wg.Done());
                defer(r.Close());
                var b = make_slice<byte>(1L << (int)(20L)); 
                // This ReadFull will likely be interrupted by a signal,
                // as the other goroutine spins before writing anything.
                {
                    var err__prev1 = err;

                    (_, err) = io.ReadFull(r, b);

                    if (err != null)
                    {
                        log.Fatal(err);
                    } 
                    // Spin after reading half the data so that the Write
                    // in the other goroutine will likely be interrupted
                    // before it completes.

                    err = err__prev1;

                } 
                // Spin after reading half the data so that the Write
                // in the other goroutine will likely be interrupted
                // before it completes.
                sendSomeSignals();
                {
                    var err__prev1 = err;

                    (_, err) = io.ReadFull(r, b);

                    if (err != null)
                    {
                        log.Fatal(err);
                    }

                    err = err__prev1;

                }

            }());

        });

        // testNet tests network operations.
        private static void testNet(ptr<sync.WaitGroup> _addr_wg) => func((defer, _, __) =>
        {
            ref sync.WaitGroup wg = ref _addr_wg.val;

            var (ln, err) = net.Listen("tcp4", "127.0.0.1:0");
            if (err != null)
            {
                if (errors.Is(err, syscall.EAFNOSUPPORT) || errors.Is(err, syscall.EPROTONOSUPPORT))
                {
                    return ;
                }

                log.Fatal(err);

            }

            wg.Add(2L);
            go_(() => () =>
            {
                defer(wg.Done());
                defer(ln.Close());
                var (c, err) = ln.Accept();
                if (err != null)
                {
                    log.Fatal(err);
                }

                defer(c.Close());
                ptr<net.TCPConn> (cf, err) = c._<ptr<net.TCPConn>>().File();
                if (err != null)
                {
                    log.Fatal(err);
                }

                defer(cf.Close());
                {
                    var err__prev1 = err;

                    var err = syscall.SetNonblock(int(cf.Fd()), false);

                    if (err != null)
                    {
                        log.Fatal(err);
                    } 
                    // See comments in testPipe.

                    err = err__prev1;

                } 
                // See comments in testPipe.
                sendSomeSignals();
                {
                    var err__prev1 = err;

                    var (_, err) = cf.Write(bytes.Repeat(new slice<byte>(new byte[] { 0 }), 2L << (int)(20L)));

                    if (err != null)
                    {
                        log.Fatal(err);
                    }

                    err = err__prev1;

                }

            }());
            go_(() => () =>
            {
                defer(wg.Done());
                sendSomeSignals();
                (c, err) = net.Dial("tcp", ln.Addr().String());
                if (err != null)
                {
                    log.Fatal(err);
                }

                defer(c.Close());
                (cf, err) = c._<ptr<net.TCPConn>>().File();
                if (err != null)
                {
                    log.Fatal(err);
                }

                defer(cf.Close());
                {
                    var err__prev1 = err;

                    err = syscall.SetNonblock(int(cf.Fd()), false);

                    if (err != null)
                    {
                        log.Fatal(err);
                    } 
                    // See comments in testPipe.

                    err = err__prev1;

                } 
                // See comments in testPipe.
                var b = make_slice<byte>(1L << (int)(20L));
                {
                    var err__prev1 = err;

                    (_, err) = io.ReadFull(cf, b);

                    if (err != null)
                    {
                        log.Fatal(err);
                    }

                    err = err__prev1;

                }

                sendSomeSignals();
                {
                    var err__prev1 = err;

                    (_, err) = io.ReadFull(cf, b);

                    if (err != null)
                    {
                        log.Fatal(err);
                    }

                    err = err__prev1;

                }

            }());

        });

        private static void testExec(ptr<sync.WaitGroup> _addr_wg) => func((defer, _, __) =>
        {
            ref sync.WaitGroup wg = ref _addr_wg.val;

            wg.Add(1L);
            go_(() => () =>
            {
                defer(wg.Done());
                var cmd = exec.Command(os.Args[0L], "Block");
                var (stdin, err) = cmd.StdinPipe();
                if (err != null)
                {
                    log.Fatal(err);
                }

                cmd.Stderr = @new<bytes.Buffer>();
                cmd.Stdout = cmd.Stderr;
                {
                    var err__prev1 = err;

                    var err = cmd.Start();

                    if (err != null)
                    {
                        log.Fatal(err);
                    }

                    err = err__prev1;

                }


                go_(() => () =>
                {
                    sendSomeSignals();
                    stdin.Close();
                }());

                {
                    var err__prev1 = err;

                    err = cmd.Wait();

                    if (err != null)
                    {
                        log.Fatalf("%v:\n%s", err, cmd.Stdout);
                    }

                    err = err__prev1;

                }

            }());

        });

        // Block blocks until stdin is closed.
        public static void Block()
        {
            io.Copy(ioutil.Discard, os.Stdin);
        }
    }
}
