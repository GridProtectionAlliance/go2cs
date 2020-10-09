// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2020 October 09 04:51:05 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd_plan9.go
using errors = go.errors_package;
using io = go.io_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        private partial struct atomicBool // : int
        {
        }

        private static bool isSet(this ptr<atomicBool> _addr_b)
        {
            ref atomicBool b = ref _addr_b.val;

            return atomic.LoadInt32((int32.val)(b)) != 0L;
        }
        private static void setFalse(this ptr<atomicBool> _addr_b)
        {
            ref atomicBool b = ref _addr_b.val;

            atomic.StoreInt32((int32.val)(b), 0L);
        }
        private static void setTrue(this ptr<atomicBool> _addr_b)
        {
            ref atomicBool b = ref _addr_b.val;

            atomic.StoreInt32((int32.val)(b), 1L);
        }

        public partial struct FD
        {
            public fdMutex fdmu;
            public Action Destroy; // deadlines
            public sync.Mutex rmu;
            public sync.Mutex wmu;
            public ptr<asyncIO> raio;
            public ptr<asyncIO> waio;
            public ptr<time.Timer> rtimer;
            public ptr<time.Timer> wtimer;
            public atomicBool rtimedout; // set true when read deadline has been reached
            public atomicBool wtimedout; // set true when write deadline has been reached

// Whether this is a normal file.
// On Plan 9 we do not use this package for ordinary files,
// so this is always false, but the field is present because
// shared code in fd_mutex.go checks it.
            public bool isFile;
        }

        // We need this to close out a file descriptor when it is unlocked,
        // but the real implementation has to live in the net package because
        // it uses os.File's.
        private static error destroy(this ptr<FD> _addr_fd)
        {
            ref FD fd = ref _addr_fd.val;

            if (fd.Destroy != null)
            {
                fd.Destroy();
            }

            return error.As(null!)!;

        }

        // Close handles the locking for closing an FD. The real operation
        // is in the net package.
        private static error Close(this ptr<FD> _addr_fd)
        {
            ref FD fd = ref _addr_fd.val;

            if (!fd.fdmu.increfAndClose())
            {
                return error.As(errClosing(fd.isFile))!;
            }

            return error.As(null!)!;

        }

        // Read implements io.Reader.
        private static (long, error) Read(this ptr<FD> _addr_fd, Func<slice<byte>, (long, error)> fn, slice<byte> b) => func((defer, _, __) =>
        {
            long _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;

            {
                var err = fd.readLock();

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            defer(fd.readUnlock());
            if (len(b) == 0L)
            {
                return (0L, error.As(null!)!);
            }

            fd.rmu.Lock();
            if (fd.rtimedout.isSet())
            {
                fd.rmu.Unlock();
                return (0L, error.As(ErrDeadlineExceeded)!);
            }

            fd.raio = newAsyncIO(fn, b);
            fd.rmu.Unlock();
            var (n, err) = fd.raio.Wait();
            fd.raio = null;
            if (isHangup(err))
            {
                err = io.EOF;
            }

            if (isInterrupted(err))
            {
                err = ErrDeadlineExceeded;
            }

            return (n, error.As(err)!);

        });

        // Write implements io.Writer.
        private static (long, error) Write(this ptr<FD> _addr_fd, Func<slice<byte>, (long, error)> fn, slice<byte> b) => func((defer, _, __) =>
        {
            long _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;

            {
                var err = fd.writeLock();

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            defer(fd.writeUnlock());
            fd.wmu.Lock();
            if (fd.wtimedout.isSet())
            {
                fd.wmu.Unlock();
                return (0L, error.As(ErrDeadlineExceeded)!);
            }

            fd.waio = newAsyncIO(fn, b);
            fd.wmu.Unlock();
            var (n, err) = fd.waio.Wait();
            fd.waio = null;
            if (isInterrupted(err))
            {
                err = ErrDeadlineExceeded;
            }

            return (n, error.As(err)!);

        });

        // SetDeadline sets the read and write deadlines associated with fd.
        private static error SetDeadline(this ptr<FD> _addr_fd, time.Time t)
        {
            ref FD fd = ref _addr_fd.val;

            return error.As(setDeadlineImpl(_addr_fd, t, 'r' + 'w'))!;
        }

        // SetReadDeadline sets the read deadline associated with fd.
        private static error SetReadDeadline(this ptr<FD> _addr_fd, time.Time t)
        {
            ref FD fd = ref _addr_fd.val;

            return error.As(setDeadlineImpl(_addr_fd, t, 'r'))!;
        }

        // SetWriteDeadline sets the write deadline associated with fd.
        private static error SetWriteDeadline(this ptr<FD> _addr_fd, time.Time t)
        {
            ref FD fd = ref _addr_fd.val;

            return error.As(setDeadlineImpl(_addr_fd, t, 'w'))!;
        }

        private static error setDeadlineImpl(ptr<FD> _addr_fd, time.Time t, long mode) => func((defer, _, __) =>
        {
            ref FD fd = ref _addr_fd.val;

            var d = t.Sub(time.Now());
            if (mode == 'r' || mode == 'r' + 'w')
            {
                fd.rmu.Lock();
                defer(fd.rmu.Unlock());
                fd.rtimedout.setFalse();
            }

            if (mode == 'w' || mode == 'r' + 'w')
            {
                fd.wmu.Lock();
                defer(fd.wmu.Unlock());
                fd.wtimedout.setFalse();
            }

            if (t.IsZero() || d < 0L)
            { 
                // Stop timer
                if (mode == 'r' || mode == 'r' + 'w')
                {
                    if (fd.rtimer != null)
                    {
                        fd.rtimer.Stop();
                    }

                    fd.rtimer = null;

                }

                if (mode == 'w' || mode == 'r' + 'w')
                {
                    if (fd.wtimer != null)
                    {
                        fd.wtimer.Stop();
                    }

                    fd.wtimer = null;

                }

            }
            else
            { 
                // Interrupt I/O operation once timer has expired
                if (mode == 'r' || mode == 'r' + 'w')
                {
                    fd.rtimer = time.AfterFunc(d, () =>
                    {
                        fd.rmu.Lock();
                        fd.rtimedout.setTrue();
                        if (fd.raio != null)
                        {
                            fd.raio.Cancel();
                        }

                        fd.rmu.Unlock();

                    });

                }

                if (mode == 'w' || mode == 'r' + 'w')
                {
                    fd.wtimer = time.AfterFunc(d, () =>
                    {
                        fd.wmu.Lock();
                        fd.wtimedout.setTrue();
                        if (fd.waio != null)
                        {
                            fd.waio.Cancel();
                        }

                        fd.wmu.Unlock();

                    });

                }

            }

            if (!t.IsZero() && d < 0L)
            { 
                // Interrupt current I/O operation
                if (mode == 'r' || mode == 'r' + 'w')
                {
                    fd.rtimedout.setTrue();
                    if (fd.raio != null)
                    {
                        fd.raio.Cancel();
                    }

                }

                if (mode == 'w' || mode == 'r' + 'w')
                {
                    fd.wtimedout.setTrue();
                    if (fd.waio != null)
                    {
                        fd.waio.Cancel();
                    }

                }

            }

            return error.As(null!)!;

        });

        // On Plan 9 only, expose the locking for the net code.

        // ReadLock wraps FD.readLock.
        private static error ReadLock(this ptr<FD> _addr_fd)
        {
            ref FD fd = ref _addr_fd.val;

            return error.As(fd.readLock())!;
        }

        // ReadUnlock wraps FD.readUnlock.
        private static void ReadUnlock(this ptr<FD> _addr_fd)
        {
            ref FD fd = ref _addr_fd.val;

            fd.readUnlock();
        }

        private static bool isHangup(error err)
        {
            return err != null && stringsHasSuffix(err.Error(), "Hangup");
        }

        private static bool isInterrupted(error err)
        {
            return err != null && stringsHasSuffix(err.Error(), "interrupted");
        }

        // IsPollDescriptor reports whether fd is the descriptor being used by the poller.
        // This is only used for testing.
        public static bool IsPollDescriptor(System.UIntPtr fd)
        {
            return false;
        }

        // RawControl invokes the user-defined function f for a non-IO
        // operation.
        private static error RawControl(this ptr<FD> _addr_fd, Action<System.UIntPtr> f)
        {
            ref FD fd = ref _addr_fd.val;

            return error.As(errors.New("not implemented"))!;
        }

        // RawRead invokes the user-defined function f for a read operation.
        private static error RawRead(this ptr<FD> _addr_fd, Func<System.UIntPtr, bool> f)
        {
            ref FD fd = ref _addr_fd.val;

            return error.As(errors.New("not implemented"))!;
        }

        // RawWrite invokes the user-defined function f for a write operation.
        private static error RawWrite(this ptr<FD> _addr_fd, Func<System.UIntPtr, bool> f)
        {
            ref FD fd = ref _addr_fd.val;

            return error.As(errors.New("not implemented"))!;
        }
    }
}}
