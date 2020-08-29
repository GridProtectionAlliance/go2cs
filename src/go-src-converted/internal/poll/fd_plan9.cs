// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2020 August 29 08:25:20 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd_plan9.go
using errors = go.errors_package;
using io = go.io_package;
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

        private static bool isSet(this ref atomicBool b)
        {
            return atomic.LoadInt32((int32.Value)(b)) != 0L;
        }
        private static void setFalse(this ref atomicBool b)
        {
            atomic.StoreInt32((int32.Value)(b), 0L);

        }
        private static void setTrue(this ref atomicBool b)
        {
            atomic.StoreInt32((int32.Value)(b), 1L);

        }

        public partial struct FD
        {
            public fdMutex fdmu;
            public Action Destroy; // deadlines
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
        private static error destroy(this ref FD fd)
        {
            if (fd.Destroy != null)
            {
                fd.Destroy();
            }
            return error.As(null);
        }

        // Close handles the locking for closing an FD. The real operation
        // is in the net package.
        private static error Close(this ref FD fd)
        {
            if (!fd.fdmu.increfAndClose())
            {
                return error.As(errClosing(fd.isFile));
            }
            return error.As(null);
        }

        // Read implements io.Reader.
        private static (long, error) Read(this ref FD _fd, Func<slice<byte>, (long, error)> fn, slice<byte> b) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            if (fd.rtimedout.isSet())
            {
                return (0L, ErrTimeout);
            }
            {
                var err = fd.readLock();

                if (err != null)
                {
                    return (0L, err);
                }

            }
            defer(fd.readUnlock());
            if (len(b) == 0L)
            {
                return (0L, null);
            }
            fd.raio = newAsyncIO(fn, b);
            var (n, err) = fd.raio.Wait();
            fd.raio = null;
            if (isHangup(err))
            {
                err = io.EOF;
            }
            if (isInterrupted(err))
            {
                err = ErrTimeout;
            }
            return (n, err);
        });

        // Write implements io.Writer.
        private static (long, error) Write(this ref FD _fd, Func<slice<byte>, (long, error)> fn, slice<byte> b) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            if (fd.wtimedout.isSet())
            {
                return (0L, ErrTimeout);
            }
            {
                var err = fd.writeLock();

                if (err != null)
                {
                    return (0L, err);
                }

            }
            defer(fd.writeUnlock());
            fd.waio = newAsyncIO(fn, b);
            var (n, err) = fd.waio.Wait();
            fd.waio = null;
            if (isInterrupted(err))
            {
                err = ErrTimeout;
            }
            return (n, err);
        });

        // SetDeadline sets the read and write deadlines associated with fd.
        private static error SetDeadline(this ref FD fd, time.Time t)
        {
            return error.As(setDeadlineImpl(fd, t, 'r' + 'w'));
        }

        // SetReadDeadline sets the read deadline associated with fd.
        private static error SetReadDeadline(this ref FD fd, time.Time t)
        {
            return error.As(setDeadlineImpl(fd, t, 'r'));
        }

        // SetWriteDeadline sets the write deadline associated with fd.
        private static error SetWriteDeadline(this ref FD fd, time.Time t)
        {
            return error.As(setDeadlineImpl(fd, t, 'w'));
        }

        private static error setDeadlineImpl(ref FD fd, time.Time t, long mode)
        {
            var d = t.Sub(time.Now());
            if (mode == 'r' || mode == 'r' + 'w')
            {
                fd.rtimedout.setFalse();
            }
            if (mode == 'w' || mode == 'r' + 'w')
            {
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
                        fd.rtimedout.setTrue();
                        if (fd.raio != null)
                        {
                            fd.raio.Cancel();
                        }
                    });
                }
                if (mode == 'w' || mode == 'r' + 'w')
                {
                    fd.wtimer = time.AfterFunc(d, () =>
                    {
                        fd.wtimedout.setTrue();
                        if (fd.waio != null)
                        {
                            fd.waio.Cancel();
                        }
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
            return error.As(null);
        }

        // On Plan 9 only, expose the locking for the net code.

        // ReadLock wraps FD.readLock.
        private static error ReadLock(this ref FD fd)
        {
            return error.As(fd.readLock());
        }

        // ReadUnlock wraps FD.readUnlock.
        private static void ReadUnlock(this ref FD fd)
        {
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

        // PollDescriptor returns the descriptor being used by the poller,
        // or ^uintptr(0) if there isn't one. This is only used for testing.
        public static System.UIntPtr PollDescriptor()
        {
            return ~uintptr(0L);
        }

        // RawControl invokes the user-defined function f for a non-IO
        // operation.
        private static error RawControl(this ref FD fd, Action<System.UIntPtr> f)
        {
            return error.As(errors.New("not implemented"));
        }

        // RawRead invokes the user-defined function f for a read operation.
        private static error RawRead(this ref FD fd, Func<System.UIntPtr, bool> f)
        {
            return error.As(errors.New("not implemented"));
        }

        // RawWrite invokes the user-defined function f for a write operation.
        private static error RawWrite(this ref FD fd, Func<System.UIntPtr, bool> f)
        {
            return error.As(errors.New("not implemented"));
        }
    }
}}
