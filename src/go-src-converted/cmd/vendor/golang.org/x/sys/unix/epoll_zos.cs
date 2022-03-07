// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build zos && s390x
// +build zos,s390x

// package unix -- go2cs converted at 2022 March 06 23:26:32 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\epoll_zos.go
using sync = go.sync_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // This file simulates epoll on z/OS using poll.

    // Analogous to epoll_event on Linux.
    // TODO(neeilan): Pad is because the Linux kernel expects a 96-bit struct. We never pass this to the kernel; remove?
public partial struct EpollEvent {
    public uint Events;
    public int Fd;
    public int Pad;
}

public static readonly nuint EPOLLERR = 0x8;
public static readonly nuint EPOLLHUP = 0x10;
public static readonly nuint EPOLLIN = 0x1;
public static readonly nuint EPOLLMSG = 0x400;
public static readonly nuint EPOLLOUT = 0x4;
public static readonly nuint EPOLLPRI = 0x2;
public static readonly nuint EPOLLRDBAND = 0x80;
public static readonly nuint EPOLLRDNORM = 0x40;
public static readonly nuint EPOLLWRBAND = 0x200;
public static readonly nuint EPOLLWRNORM = 0x100;
public static readonly nuint EPOLL_CTL_ADD = 0x1;
public static readonly nuint EPOLL_CTL_DEL = 0x2;
public static readonly nuint EPOLL_CTL_MOD = 0x3; 
// The following constants are part of the epoll API, but represent
// currently unsupported functionality on z/OS.
// EPOLL_CLOEXEC  = 0x80000
// EPOLLET        = 0x80000000
// EPOLLONESHOT   = 0x40000000
// EPOLLRDHUP     = 0x2000     // Typically used with edge-triggered notis
// EPOLLEXCLUSIVE = 0x10000000 // Exclusive wake-up mode
// EPOLLWAKEUP    = 0x20000000 // Relies on Linux's BLOCK_SUSPEND capability

// TODO(neeilan): We can eliminate these epToPoll / pToEpoll calls by using identical mask values for POLL/EPOLL
// constants where possible The lower 16 bits of epoll events (uint32) can fit any system poll event (int16).

// epToPollEvt converts epoll event field to poll equivalent.
// In epoll, Events is a 32-bit field, while poll uses 16 bits.
private static short epToPollEvt(uint events) {
    map ep2p = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<uint, short>{EPOLLIN:POLLIN,EPOLLOUT:POLLOUT,EPOLLHUP:POLLHUP,EPOLLPRI:POLLPRI,EPOLLERR:POLLERR,};

    short pollEvts = 0;
    foreach (var (epEvt, pEvt) in ep2p) {
        if ((events & epEvt) != 0) {
            pollEvts |= pEvt;
        }
    }    return pollEvts;

}

// pToEpollEvt converts 16 bit poll event bitfields to 32-bit epoll event fields.
private static uint pToEpollEvt(short revents) {
    map p2ep = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<short, uint>{POLLIN:EPOLLIN,POLLOUT:EPOLLOUT,POLLHUP:EPOLLHUP,POLLPRI:EPOLLPRI,POLLERR:EPOLLERR,};

    uint epollEvts = 0;
    foreach (var (pEvt, epEvt) in p2ep) {
        if ((revents & pEvt) != 0) {
            epollEvts |= epEvt;
        }
    }    return epollEvts;

}

// Per-process epoll implementation.
private partial struct epollImpl {
    public sync.Mutex mu;
    public map<nint, ptr<eventPoll>> epfd2ep;
    public nint nextEpfd;
}

// eventPoll holds a set of file descriptors being watched by the process. A process can have multiple epoll instances.
// On Linux, this is an in-kernel data structure accessed through a fd.
private partial struct eventPoll {
    public sync.Mutex mu;
    public map<nint, ptr<EpollEvent>> fds;
}

// epoll impl for this process.
private static epollImpl impl = new epollImpl(epfd2ep:make(map[int]*eventPoll),nextEpfd:0,);

private static (nint, error) epollcreate(this ptr<epollImpl> _addr_e, nint size) => func((defer, _, _) => {
    nint epfd = default;
    error err = default!;
    ref epollImpl e = ref _addr_e.val;

    e.mu.Lock();
    defer(e.mu.Unlock());
    epfd = e.nextEpfd;
    e.nextEpfd++;

    e.epfd2ep[epfd] = addr(new eventPoll(fds:make(map[int]*EpollEvent),));
    return (epfd, error.As(null!)!);
});

private static (nint, error) epollcreate1(this ptr<epollImpl> _addr_e, nint flag) {
    nint fd = default;
    error err = default!;
    ref epollImpl e = ref _addr_e.val;

    return e.epollcreate(4);
}

private static error epollctl(this ptr<epollImpl> _addr_e, nint epfd, nint op, nint fd, ptr<EpollEvent> _addr_@event) => func((defer, _, _) => {
    error err = default!;
    ref epollImpl e = ref _addr_e.val;
    ref EpollEvent @event = ref _addr_@event.val;

    e.mu.Lock();
    defer(e.mu.Unlock());

    var (ep, ok) = e.epfd2ep[epfd];
    if (!ok) {
        return error.As(EBADF)!;
    }

    if (op == EPOLL_CTL_ADD) 
        // TODO(neeilan): When we make epfds and fds disjoint, detect epoll
        // loops here (instances watching each other) and return ELOOP.
        {
            var (_, ok) = ep.fds[fd];

            if (ok) {
                return error.As(EEXIST)!;
            }

        }

        ep.fds[fd] = event;
    else if (op == EPOLL_CTL_MOD) 
        {
            (_, ok) = ep.fds[fd];

            if (!ok) {
                return error.As(ENOENT)!;
            }

        }

        ep.fds[fd] = event;
    else if (op == EPOLL_CTL_DEL) 
        {
            (_, ok) = ep.fds[fd];

            if (!ok) {
                return error.As(ENOENT)!;
            }

        }

        delete(ep.fds, fd);
        return error.As(null!)!;

});

// Must be called while holding ep.mu
private static slice<nint> getFds(this ptr<eventPoll> _addr_ep) {
    ref eventPoll ep = ref _addr_ep.val;

    var fds = make_slice<nint>(len(ep.fds));
    foreach (var (fd) in ep.fds) {
        fds = append(fds, fd);
    }    return fds;
}

private static (nint, error) epollwait(this ptr<epollImpl> _addr_e, nint epfd, slice<EpollEvent> events, nint msec) {
    nint n = default;
    error err = default!;
    ref epollImpl e = ref _addr_e.val;

    e.mu.Lock(); // in [rare] case of concurrent epollcreate + epollwait
    var (ep, ok) = e.epfd2ep[epfd];

    if (!ok) {
        e.mu.Unlock();
        return (0, error.As(EBADF)!);
    }
    var pollfds = make_slice<PollFd>(4);
    foreach (var (fd, epollevt) in ep.fds) {
        pollfds = append(pollfds, new PollFd(Fd:int32(fd),Events:epToPollEvt(epollevt.Events)));
    }    e.mu.Unlock();

    n, err = Poll(pollfds, msec);
    if (err != null) {
        return (n, error.As(err)!);
    }
    nint i = 0;
    foreach (var (_, pFd) in pollfds) {
        if (pFd.Revents != 0) {
            events[i] = new EpollEvent(Fd:pFd.Fd,Events:pToEpollEvt(pFd.Revents));
            i++;
        }
        if (i == n) {
            break;
        }
    }    return (n, error.As(null!)!);

}

public static (nint, error) EpollCreate(nint size) {
    nint fd = default;
    error err = default!;

    return impl.epollcreate(size);
}

public static (nint, error) EpollCreate1(nint flag) {
    nint fd = default;
    error err = default!;

    return impl.epollcreate1(flag);
}

public static error EpollCtl(nint epfd, nint op, nint fd, ptr<EpollEvent> _addr_@event) {
    error err = default!;
    ref EpollEvent @event = ref _addr_@event.val;

    return error.As(impl.epollctl(epfd, op, fd, event))!;
}

// Because EpollWait mutates events, the caller is expected to coordinate
// concurrent access if calling with the same epfd from multiple goroutines.
public static (nint, error) EpollWait(nint epfd, slice<EpollEvent> events, nint msec) {
    nint n = default;
    error err = default!;

    return impl.epollwait(epfd, events, msec);
}

} // end unix_package
