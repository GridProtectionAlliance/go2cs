// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package par -- go2cs converted at 2022 March 06 23:16:43 UTC
// import "cmd/go/internal/par" ==> using par = go.cmd.go.@internal.par_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\par\queue.go
using fmt = go.fmt_package;
using System;
using System.Threading;


namespace go.cmd.go.@internal;

public static partial class par_package {

    // Queue manages a set of work items to be executed in parallel. The number of
    // active work items is limited, and excess items are queued sequentially.
public partial struct Queue {
    public nint maxActive;
    public channel<queueState> st;
}

private partial struct queueState {
    public nint active; // number of goroutines processing work; always nonzero when len(backlog) > 0
    public slice<Action> backlog;
    public channel<object> idle; // if non-nil, closed when active becomes 0
}

// NewQueue returns a Queue that executes up to maxActive items in parallel.
//
// maxActive must be positive.
public static ptr<Queue> NewQueue(nint maxActive) => func((_, panic, _) => {
    if (maxActive < 1) {
        panic(fmt.Sprintf("par.NewQueue called with nonpositive limit (%d)", maxActive));
    }
    ptr<Queue> q = addr(new Queue(maxActive:maxActive,st:make(chanqueueState,1),));
    q.st.Send(new queueState());
    return _addr_q!;

});

// Add adds f as a work item in the queue.
//
// Add returns immediately, but the queue will be marked as non-idle until after
// f (and any subsequently-added work) has completed.
private static void Add(this ptr<Queue> _addr_q, Action f) {
    ref Queue q = ref _addr_q.val;

    var st = q.st.Receive();
    if (st.active == q.maxActive) {
        st.backlog = append(st.backlog, f);
        q.st.Send(st);
        return ;
    }
    if (st.active == 0) { 
        // Mark q as non-idle.
        st.idle = null;

    }
    st.active++;
    q.st.Send(st);

    go_(() => () => {
        while (true) {
            f();

            st = q.st.Receive();
            if (len(st.backlog) == 0) {
                st.active--;

                if (st.active == 0 && st.idle != null) {
                    close(st.idle);
                }

                q.st.Send(st);
                return ;

            }

            (f, st.backlog) = (st.backlog[0], st.backlog[(int)1..]);            q.st.Send(st);

        }

    }());

}

// Idle returns a channel that will be closed when q has no (active or enqueued)
// work outstanding.
private static channel<object> Idle(this ptr<Queue> _addr_q) => func((defer, _, _) => {
    ref Queue q = ref _addr_q.val;

    var st = q.st.Receive();
    defer(() => {
        q.st.Send(st);
    }());

    if (st.idle == null) {
        st.idle = make_channel<object>();
        if (st.active == 0) {
            close(st.idle);
        }
    }
    return st.idle;

});

} // end par_package
