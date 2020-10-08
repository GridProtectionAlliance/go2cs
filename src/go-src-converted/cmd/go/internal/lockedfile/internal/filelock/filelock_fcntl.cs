// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix solaris

// This code implements the filelock API using POSIX 'fcntl' locks, which attach
// to an (inode, process) pair rather than a file descriptor. To avoid unlocking
// files prematurely when the same file is opened through different descriptors,
// we allow only one read-lock at a time.
//
// Most platforms provide some alternative API, such as an 'flock' system call
// or an F_OFD_SETLK command for 'fcntl', that allows for better concurrency and
// does not require per-inode bookkeeping in the application.
//
// TODO(golang.org/issue/35618): add a syscall.Flock binding for Illumos and
// switch it over to use filelock_unix.go.

// package filelock -- go2cs converted at 2020 October 08 04:34:18 UTC
// import "cmd/go/internal/lockedfile/internal/filelock" ==> using filelock = go.cmd.go.@internal.lockedfile.@internal.filelock_package
// Original source: C:\Go\src\cmd\go\internal\lockedfile\internal\filelock\filelock_fcntl.go
using errors = go.errors_package;
using io = go.io_package;
using rand = go.math.rand_package;
using os = go.os_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal {
namespace lockedfile {
namespace @internal
{
    public static partial class filelock_package
    {
        private partial struct lockType // : short
        {
        }

        private static readonly lockType readLock = (lockType)syscall.F_RDLCK;
        private static readonly lockType writeLock = (lockType)syscall.F_WRLCK;


        private partial struct inode // : ulong
        {
        } // type of syscall.Stat_t.Ino

        private partial struct inodeLock
        {
            public File owner;
            public slice<channel<File>> queue;
        }

        private partial struct token
        {
        }

        private static sync.Mutex mu = default;        private static map inodes = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<File, inode>{};        private static map locks = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<inode, inodeLock>{};

        private static error @lock(File f, lockType lt)
        {
            error err = default!;
 
            // POSIX locks apply per inode and process, and the lock for an inode is
            // released when *any* descriptor for that inode is closed. So we need to
            // synchronize access to each inode internally, and must serialize lock and
            // unlock calls that refer to the same inode through different descriptors.
            var (fi, err) = f.Stat();
            if (err != null)
            {
                return error.As(err)!;
            }

            ptr<syscall.Stat_t> ino = fi.Sys()._<ptr<syscall.Stat_t>>().Ino;

            mu.Lock();
            {
                var (i, dup) = inodes[f];

                if (dup && i != ino)
                {
                    mu.Unlock();
                    return error.As(addr(new os.PathError(Op:lt.String(),Path:f.Name(),Err:errors.New("inode for file changed since last Lock or RLock"),))!)!;
                }

            }

            inodes[f] = ino;

            channel<File> wait = default;
            var l = locks[ino];
            if (l.owner == f)
            { 
                // This file already owns the lock, but the call may change its lock type.
            }
            else if (l.owner == null)
            { 
                // No owner: it's ours now.
                l.owner = f;

            }
            else
            { 
                // Already owned: add a channel to wait on.
                wait = make_channel<File>();
                l.queue = append(l.queue, wait);

            }

            locks[ino] = l;
            mu.Unlock();

            if (wait != null)
            {
                wait.Send(f);
            } 

            // Spurious EDEADLK errors arise on platforms that compute deadlock graphs at
            // the process, rather than thread, level. Consider processes P and Q, with
            // threads P.1, P.2, and Q.3. The following trace is NOT a deadlock, but will be
            // reported as a deadlock on systems that consider only process granularity:
            //
            //     P.1 locks file A.
            //     Q.3 locks file B.
            //     Q.3 blocks on file A.
            //     P.2 blocks on file B. (This is erroneously reported as a deadlock.)
            //     P.1 unlocks file A.
            //     Q.3 unblocks and locks file A.
            //     Q.3 unlocks files A and B.
            //     P.2 unblocks and locks file B.
            //     P.2 unlocks file B.
            //
            // These spurious errors were observed in practice on AIX and Solaris in
            // cmd/go: see https://golang.org/issue/32817.
            //
            // We work around this bug by treating EDEADLK as always spurious. If there
            // really is a lock-ordering bug between the interacting processes, it will
            // become a livelock instead, but that's not appreciably worse than if we had
            // a proper flock implementation (which generally does not even attempt to
            // diagnose deadlocks).
            //
            // In the above example, that changes the trace to:
            //
            //     P.1 locks file A.
            //     Q.3 locks file B.
            //     Q.3 blocks on file A.
            //     P.2 spuriously fails to lock file B and goes to sleep.
            //     P.1 unlocks file A.
            //     Q.3 unblocks and locks file A.
            //     Q.3 unlocks files A and B.
            //     P.2 wakes up and locks file B.
            //     P.2 unlocks file B.
            //
            // We know that the retry loop will not introduce a *spurious* livelock
            // because, according to the POSIX specification, EDEADLK is only to be
            // returned when “the lock is blocked by a lock from another process”.
            // If that process is blocked on some lock that we are holding, then the
            // resulting livelock is due to a real deadlock (and would manifest as such
            // when using, for example, the flock implementation of this package).
            // If the other process is *not* blocked on some other lock that we are
            // holding, then it will eventually release the requested lock.
            long nextSleep = 1L * time.Millisecond;
            const long maxSleep = (long)500L * time.Millisecond;

            while (true)
            {
                err = setlkw(f.Fd(), lt);
                if (err != syscall.EDEADLK)
                {
                    break;
                }

                time.Sleep(nextSleep);

                nextSleep += nextSleep;
                if (nextSleep > maxSleep)
                {
                    nextSleep = maxSleep;
                } 
                // Apply 10% jitter to avoid synchronizing collisions when we finally unblock.
                nextSleep += time.Duration((0.1F * rand.Float64() - 0.05F) * float64(nextSleep));

            }


            if (err != null)
            {
                unlock(f);
                return error.As(addr(new os.PathError(Op:lt.String(),Path:f.Name(),Err:err,))!)!;
            }

            return error.As(null!)!;

        }

        private static error unlock(File f) => func((_, panic, __) =>
        {
            File owner = default;

            mu.Lock();
            var (ino, ok) = inodes[f];
            if (ok)
            {
                owner = locks[ino].owner;
            }

            mu.Unlock();

            if (owner != f)
            {
                panic("unlock called on a file that is not locked");
            }

            var err = setlkw(f.Fd(), syscall.F_UNLCK);

            mu.Lock();
            var l = locks[ino];
            if (len(l.queue) == 0L)
            { 
                // No waiters: remove the map entry.
                delete(locks, ino);

            }
            else
            { 
                // The first waiter is sending us their file now.
                // Receive it and update the queue.
                l.owner = l.queue[0L].Receive();
                l.queue = l.queue[1L..];
                locks[ino] = l;

            }

            delete(inodes, f);
            mu.Unlock();

            return error.As(err)!;

        });

        // setlkw calls FcntlFlock with F_SETLKW for the entire file indicated by fd.
        private static error setlkw(System.UIntPtr fd, lockType lt)
        {
            while (true)
            {
                var err = syscall.FcntlFlock(fd, syscall.F_SETLKW, addr(new syscall.Flock_t(Type:int16(lt),Whence:io.SeekStart,Start:0,Len:0,)));
                if (err != syscall.EINTR)
                {
                    return error.As(err)!;
                }

            }


        }

        private static bool isNotSupported(error err)
        {
            return err == syscall.ENOSYS || err == syscall.ENOTSUP || err == syscall.EOPNOTSUPP || err == ErrNotSupported;
        }
    }
}}}}}}
