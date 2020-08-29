// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:18:39 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\netpoll_windows.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly ulong _DWORD_MAX = 0xffffffffUL;



        private static readonly var _INVALID_HANDLE_VALUE = ~uintptr(0L);

        // net_op must be the same as beginning of internal/poll.operation.
        // Keep these in sync.


        // net_op must be the same as beginning of internal/poll.operation.
        // Keep these in sync.
        private partial struct net_op
        {
            public overlapped o; // used by netpoll
            public ptr<pollDesc> pd;
            public int mode;
            public int errno;
            public uint qty;
        }

        private partial struct overlappedEntry
        {
            public System.UIntPtr key;
            public ptr<net_op> op; // In reality it's *overlapped, but we cast it to *net_op anyway.
            public System.UIntPtr @internal;
            public uint qty;
        }

        private static System.UIntPtr iocphandle = _INVALID_HANDLE_VALUE; // completion port io handle

        private static void netpollinit()
        {
            iocphandle = stdcall4(_CreateIoCompletionPort, _INVALID_HANDLE_VALUE, 0L, 0L, _DWORD_MAX);
            if (iocphandle == 0L)
            {
                println("runtime: CreateIoCompletionPort failed (errno=", getlasterror(), ")");
                throw("runtime: netpollinit failed");
            }
        }

        private static System.UIntPtr netpolldescriptor()
        {
            return iocphandle;
        }

        private static int netpollopen(System.UIntPtr fd, ref pollDesc pd)
        {
            if (stdcall4(_CreateIoCompletionPort, fd, iocphandle, 0L, 0L) == 0L)
            {
                return int32(getlasterror());
            }
            return 0L;
        }

        private static int netpollclose(System.UIntPtr fd)
        { 
            // nothing to do
            return 0L;
        }

        private static void netpollarm(ref pollDesc pd, long mode)
        {
            throw("runtime: unused");
        }

        // Polls for completed network IO.
        // Returns list of goroutines that become runnable.
        private static ref g netpoll(bool block)
        {
            array<overlappedEntry> entries = new array<overlappedEntry>(64L);
            uint wait = default;            uint qty = default;            uint key = default;            uint flags = default;            uint n = default;            uint i = default;

            int errno = default;
            ref net_op op = default;
            guintptr gp = default;

            var mp = getg().m;

            if (iocphandle == _INVALID_HANDLE_VALUE)
            {
                return null;
            }
            wait = 0L;
            if (block)
            {
                wait = _INFINITE;
            }
retry:
            if (_GetQueuedCompletionStatusEx != null)
            {
                n = uint32(len(entries) / int(gomaxprocs));
                if (n < 8L)
                {
                    n = 8L;
                }
                if (block)
                {
                    mp.blocked = true;
                }
                if (stdcall6(_GetQueuedCompletionStatusEx, iocphandle, uintptr(@unsafe.Pointer(ref entries[0L])), uintptr(n), uintptr(@unsafe.Pointer(ref n)), uintptr(wait), 0L) == 0L)
                {
                    mp.blocked = false;
                    errno = int32(getlasterror());
                    if (!block && errno == _WAIT_TIMEOUT)
                    {
                        return null;
                    }
                    println("runtime: GetQueuedCompletionStatusEx failed (errno=", errno, ")");
                    throw("runtime: netpoll failed");
                }
                mp.blocked = false;
                for (i = 0L; i < n; i++)
                {
                    op = entries[i].op;
                    errno = 0L;
                    qty = 0L;
                    if (stdcall5(_WSAGetOverlappedResult, op.pd.fd, uintptr(@unsafe.Pointer(op)), uintptr(@unsafe.Pointer(ref qty)), 0L, uintptr(@unsafe.Pointer(ref flags))) == 0L)
                    {
                        errno = int32(getlasterror());
                    }
                    handlecompletion(ref gp, op, errno, qty);
                }
            else

            }            {
                op = null;
                errno = 0L;
                qty = 0L;
                if (block)
                {
                    mp.blocked = true;
                }
                if (stdcall5(_GetQueuedCompletionStatus, iocphandle, uintptr(@unsafe.Pointer(ref qty)), uintptr(@unsafe.Pointer(ref key)), uintptr(@unsafe.Pointer(ref op)), uintptr(wait)) == 0L)
                {
                    mp.blocked = false;
                    errno = int32(getlasterror());
                    if (!block && errno == _WAIT_TIMEOUT)
                    {
                        return null;
                    }
                    if (op == null)
                    {
                        println("runtime: GetQueuedCompletionStatus failed (errno=", errno, ")");
                        throw("runtime: netpoll failed");
                    } 
                    // dequeued failed IO packet, so report that
                }
                mp.blocked = false;
                handlecompletion(ref gp, op, errno, qty);
            }
            if (block && gp == 0L)
            {
                goto retry;
            }
            return gp.ptr();
        }

        private static void handlecompletion(ref guintptr gpp, ref net_op op, int errno, uint qty)
        {
            if (op == null)
            {
                println("runtime: GetQueuedCompletionStatus returned op == nil");
                throw("runtime: netpoll failed");
            }
            var mode = op.mode;
            if (mode != 'r' && mode != 'w')
            {
                println("runtime: GetQueuedCompletionStatus returned invalid mode=", mode);
                throw("runtime: netpoll failed");
            }
            op.errno = errno;
            op.qty = qty;
            netpollready(gpp, op.pd, mode);
        }
    }
}
