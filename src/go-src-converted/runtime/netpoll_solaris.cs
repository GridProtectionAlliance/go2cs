// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:21:39 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\netpoll_solaris.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Solaris runtime-integrated network poller.
        //
        // Solaris uses event ports for scalable network I/O. Event
        // ports are level-triggered, unlike epoll and kqueue which
        // can be configured in both level-triggered and edge-triggered
        // mode. Level triggering means we have to keep track of a few things
        // ourselves. After we receive an event for a file descriptor,
        // it's our responsibility to ask again to be notified for future
        // events for that descriptor. When doing this we must keep track of
        // what kind of events the goroutines are currently interested in,
        // for example a fd may be open both for reading and writing.
        //
        // A description of the high level operation of this code
        // follows. Networking code will get a file descriptor by some means
        // and will register it with the netpolling mechanism by a code path
        // that eventually calls runtime·netpollopen. runtime·netpollopen
        // calls port_associate with an empty event set. That means that we
        // will not receive any events at this point. The association needs
        // to be done at this early point because we need to process the I/O
        // readiness notification at some point in the future. If I/O becomes
        // ready when nobody is listening, when we finally care about it,
        // nobody will tell us anymore.
        //
        // Beside calling runtime·netpollopen, the networking code paths
        // will call runtime·netpollarm each time goroutines are interested
        // in doing network I/O. Because now we know what kind of I/O we
        // are interested in (reading/writing), we can call port_associate
        // passing the correct type of event set (POLLIN/POLLOUT). As we made
        // sure to have already associated the file descriptor with the port,
        // when we now call port_associate, we will unblock the main poller
        // loop (in runtime·netpoll) right away if the socket is actually
        // ready for I/O.
        //
        // The main poller loop runs in its own thread waiting for events
        // using port_getn. When an event happens, it will tell the scheduler
        // about it using runtime·netpollready. Besides doing this, it must
        // also re-associate the events that were not part of this current
        // notification with the file descriptor. Failing to do this would
        // mean each notification will prevent concurrent code using the
        // same file descriptor in parallel.
        //
        // The logic dealing with re-associations is encapsulated in
        // runtime·netpollupdate. This function takes care to associate the
        // descriptor only with the subset of events that were previously
        // part of the association, except the one that just happened. We
        // can't re-associate with that right away, because event ports
        // are level triggered so it would cause a busy loop. Instead, that
        // association is effected only by the runtime·netpollarm code path,
        // when Go code actually asks for I/O.
        //
        // The open and arming mechanisms are serialized using the lock
        // inside PollDesc. This is required because the netpoll loop runs
        // asynchronously in respect to other Go code and by the time we get
        // to call port_associate to update the association in the loop, the
        // file descriptor might have been closed and reopened already. The
        // lock allows runtime·netpollupdate to be called synchronously from
        // the loop thread while preventing other threads operating to the
        // same PollDesc, so once we unblock in the main loop, until we loop
        // again we know for sure we are always talking about the same file
        // descriptor and can safely access the data we want (the event set).

        //go:cgo_import_dynamic libc_port_create port_create "libc.so"
        //go:cgo_import_dynamic libc_port_associate port_associate "libc.so"
        //go:cgo_import_dynamic libc_port_dissociate port_dissociate "libc.so"
        //go:cgo_import_dynamic libc_port_getn port_getn "libc.so"
        //go:cgo_import_dynamic libc_port_alert port_alert "libc.so"

        //go:linkname libc_port_create libc_port_create
        //go:linkname libc_port_associate libc_port_associate
        //go:linkname libc_port_dissociate libc_port_dissociate
        //go:linkname libc_port_getn libc_port_getn
        //go:linkname libc_port_alert libc_port_alert
        private static libcFunc libc_port_create = default;        private static libcFunc libc_port_associate = default;        private static libcFunc libc_port_dissociate = default;        private static libcFunc libc_port_getn = default;        private static libcFunc libc_port_alert = default;
        private static uint netpollWakeSig = default;

        private static int errno()
        {
            return getg().m.perrno.val;
        }

        private static int fcntl(int fd, int cmd, int arg)
        {
            return int32(sysvicall3(_addr_libc_fcntl, uintptr(fd), uintptr(cmd), uintptr(arg)));
        }

        private static int port_create()
        {
            return int32(sysvicall0(_addr_libc_port_create));
        }

        private static int port_associate(int port, int source, System.UIntPtr @object, uint events, System.UIntPtr user)
        {
            return int32(sysvicall5(_addr_libc_port_associate, uintptr(port), uintptr(source), object, uintptr(events), user));
        }

        private static int port_dissociate(int port, int source, System.UIntPtr @object)
        {
            return int32(sysvicall3(_addr_libc_port_dissociate, uintptr(port), uintptr(source), object));
        }

        private static int port_getn(int port, ptr<portevent> _addr_evs, uint max, ptr<uint> _addr_nget, ptr<timespec> _addr_timeout)
        {
            ref portevent evs = ref _addr_evs.val;
            ref uint nget = ref _addr_nget.val;
            ref timespec timeout = ref _addr_timeout.val;

            return int32(sysvicall5(_addr_libc_port_getn, uintptr(port), uintptr(@unsafe.Pointer(evs)), uintptr(max), uintptr(@unsafe.Pointer(nget)), uintptr(@unsafe.Pointer(timeout))));
        }

        private static int port_alert(int port, uint flags, uint events, System.UIntPtr user)
        {
            return int32(sysvicall4(_addr_libc_port_alert, uintptr(port), uintptr(flags), uintptr(events), user));
        }

        private static int portfd = -1L;

        private static void netpollinit()
        {
            portfd = port_create();
            if (portfd >= 0L)
            {
                fcntl(portfd, _F_SETFD, _FD_CLOEXEC);
                return ;
            }

            print("runtime: port_create failed (errno=", errno(), ")\n");
            throw("runtime: netpollinit failed");

        }

        private static bool netpollIsPollDescriptor(System.UIntPtr fd)
        {
            return fd == uintptr(portfd);
        }

        private static int netpollopen(System.UIntPtr fd, ptr<pollDesc> _addr_pd)
        {
            ref pollDesc pd = ref _addr_pd.val;

            lock(_addr_pd.@lock); 
            // We don't register for any specific type of events yet, that's
            // netpollarm's job. We merely ensure we call port_associate before
            // asynchronous connect/accept completes, so when we actually want
            // to do any I/O, the call to port_associate (from netpollarm,
            // with the interested event set) will unblock port_getn right away
            // because of the I/O readiness notification.
            pd.user = 0L;
            var r = port_associate(portfd, _PORT_SOURCE_FD, fd, 0L, uintptr(@unsafe.Pointer(pd)));
            unlock(_addr_pd.@lock);
            return r;

        }

        private static int netpollclose(System.UIntPtr fd)
        {
            return port_dissociate(portfd, _PORT_SOURCE_FD, fd);
        }

        // Updates the association with a new set of interested events. After
        // this call, port_getn will return one and only one event for that
        // particular descriptor, so this function needs to be called again.
        private static void netpollupdate(ptr<pollDesc> _addr_pd, uint set, uint clear)
        {
            ref pollDesc pd = ref _addr_pd.val;

            if (pd.closing)
            {
                return ;
            }

            var old = pd.user;
            var events = (old & ~clear) | set;
            if (old == events)
            {
                return ;
            }

            if (events != 0L && port_associate(portfd, _PORT_SOURCE_FD, pd.fd, events, uintptr(@unsafe.Pointer(pd))) != 0L)
            {
                print("runtime: port_associate failed (errno=", errno(), ")\n");
                throw("runtime: netpollupdate failed");
            }

            pd.user = events;

        }

        // subscribe the fd to the port such that port_getn will return one event.
        private static void netpollarm(ptr<pollDesc> _addr_pd, long mode)
        {
            ref pollDesc pd = ref _addr_pd.val;

            lock(_addr_pd.@lock);
            switch (mode)
            {
                case 'r': 
                    netpollupdate(_addr_pd, _POLLIN, 0L);
                    break;
                case 'w': 
                    netpollupdate(_addr_pd, _POLLOUT, 0L);
                    break;
                default: 
                    throw("runtime: bad mode");
                    break;
            }
            unlock(_addr_pd.@lock);

        }

        // netpollBreak interrupts a port_getn wait.
        private static void netpollBreak()
        {
            if (atomic.Cas(_addr_netpollWakeSig, 0L, 1L))
            { 
                // Use port_alert to put portfd into alert mode.
                // This will wake up all threads sleeping in port_getn on portfd,
                // and cause their calls to port_getn to return immediately.
                // Further, until portfd is taken out of alert mode,
                // all calls to port_getn will return immediately.
                if (port_alert(portfd, _PORT_ALERT_UPDATE, _POLLHUP, uintptr(@unsafe.Pointer(_addr_portfd))) < 0L)
                {
                    {
                        var e = errno();

                        if (e != _EBUSY)
                        {
                            println("runtime: port_alert failed with", e);
                            throw("runtime: netpoll: port_alert failed");
                        }

                    }

                }

            }

        }

        // netpoll checks for ready network connections.
        // Returns list of goroutines that become runnable.
        // delay < 0: blocks indefinitely
        // delay == 0: does not block, just polls
        // delay > 0: block for up to that many nanoseconds
        private static gList netpoll(long delay)
        {
            if (portfd == -1L)
            {
                return new gList();
            }

            ptr<timespec> wait;
            ref timespec ts = ref heap(out ptr<timespec> _addr_ts);
            if (delay < 0L)
            {
                wait = null;
            }
            else if (delay == 0L)
            {
                wait = _addr_ts;
            }
            else
            {
                ts.setNsec(delay);
                if (ts.tv_sec > 1e6F)
                { 
                    // An arbitrary cap on how long to wait for a timer.
                    // 1e6 s == ~11.5 days.
                    ts.tv_sec = 1e6F;

                }

                wait = _addr_ts;

            }

            array<portevent> events = new array<portevent>(128L);
retry:
            ref uint n = ref heap(1L, out ptr<uint> _addr_n);
            var r = port_getn(portfd, _addr_events[0L], uint32(len(events)), _addr_n, wait);
            var e = errno();
            if (r < 0L && e == _ETIME && n > 0L)
            { 
                // As per port_getn(3C), an ETIME failure does not preclude the
                // delivery of some number of events.  Treat a timeout failure
                // with delivered events as a success.
                r = 0L;

            }

            if (r < 0L)
            {
                if (e != _EINTR && e != _ETIME)
                {
                    print("runtime: port_getn on fd ", portfd, " failed (errno=", e, ")\n");
                    throw("runtime: netpoll failed");
                } 
                // If a timed sleep was interrupted and there are no events,
                // just return to recalculate how long we should sleep now.
                if (delay > 0L)
                {
                    return new gList();
                }

                goto retry;

            }

            ref gList toRun = ref heap(out ptr<gList> _addr_toRun);
            for (long i = 0L; i < int(n); i++)
            {
                var ev = _addr_events[i];

                if (ev.portev_source == _PORT_SOURCE_ALERT)
                {
                    if (ev.portev_events != _POLLHUP || @unsafe.Pointer(ev.portev_user) != @unsafe.Pointer(_addr_portfd))
                    {
                        throw("runtime: netpoll: bad port_alert wakeup");
                    }

                    if (delay != 0L)
                    { 
                        // Now that a blocking call to netpoll
                        // has seen the alert, take portfd
                        // back out of alert mode.
                        // See the comment in netpollBreak.
                        if (port_alert(portfd, 0L, 0L, 0L) < 0L)
                        {
                            e = errno();
                            println("runtime: port_alert failed with", e);
                            throw("runtime: netpoll: port_alert failed");
                        }

                        atomic.Store(_addr_netpollWakeSig, 0L);

                    }

                    continue;

                }

                if (ev.portev_events == 0L)
                {
                    continue;
                }

                var pd = (pollDesc.val)(@unsafe.Pointer(ev.portev_user));

                int mode = default;                int clear = default;

                if ((ev.portev_events & (_POLLIN | _POLLHUP | _POLLERR)) != 0L)
                {
                    mode += 'r';
                    clear |= _POLLIN;
                }

                if ((ev.portev_events & (_POLLOUT | _POLLHUP | _POLLERR)) != 0L)
                {
                    mode += 'w';
                    clear |= _POLLOUT;
                } 
                // To effect edge-triggered events, we need to be sure to
                // update our association with whatever events were not
                // set with the event. For example if we are registered
                // for POLLIN|POLLOUT, and we get POLLIN, besides waking
                // the goroutine interested in POLLIN we have to not forget
                // about the one interested in POLLOUT.
                if (clear != 0L)
                {
                    lock(_addr_pd.@lock);
                    netpollupdate(_addr_pd, 0L, uint32(clear));
                    unlock(_addr_pd.@lock);
                }

                if (mode != 0L)
                { 
                    // TODO(mikio): Consider implementing event
                    // scanning error reporting once we are sure
                    // about the event port on SmartOS.
                    //
                    // See golang.org/x/issue/30840.
                    netpollready(_addr_toRun, pd, mode);

                }

            }


            return toRun;

        }
    }
}
