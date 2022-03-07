// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package singleflight provides a duplicate function call suppression
// mechanism.
// package singleflight -- go2cs converted at 2022 March 06 22:16:17 UTC
// import "internal/singleflight" ==> using singleflight = go.@internal.singleflight_package
// Original source: C:\Program Files\Go\src\internal\singleflight\singleflight.go
using sync = go.sync_package;
using System;
using System.Threading;


namespace go.@internal;

public static partial class singleflight_package {

    // call is an in-flight or completed singleflight.Do call
private partial struct call {
    public sync.WaitGroup wg; // These fields are written once before the WaitGroup is done
// and are only read after the WaitGroup is done.
    public error err; // These fields are read and written with the singleflight
// mutex held before the WaitGroup is done, and are read but
// not written after the WaitGroup is done.
    public nint dups;
    public slice<channel<Result>> chans;
}

// Group represents a class of work and forms a namespace in
// which units of work can be executed with duplicate suppression.
public partial struct Group {
    public sync.Mutex mu; // protects m
    public map<@string, ptr<call>> m; // lazily initialized
}

// Result holds the results of Do, so they can be passed
// on a channel.
public partial struct Result {
    public error Err;
    public bool Shared;
}

// Do executes and returns the results of the given function, making
// sure that only one execution is in-flight for a given key at a
// time. If a duplicate comes in, the duplicate caller waits for the
// original to complete and receives the same results.
// The return value shared indicates whether v was given to multiple callers.
private static (object, error, bool) Do(this ptr<Group> _addr_g, @string key, Func<(object, error)> fn) {
    object v = default;
    error err = default!;
    bool shared = default;
    ref Group g = ref _addr_g.val;

    g.mu.Lock();
    if (g.m == null) {
        g.m = make_map<@string, ptr<call>>();
    }
    {
        var c__prev1 = c;

        var (c, ok) = g.m[key];

        if (ok) {
            c.dups++;
            g.mu.Unlock();
            c.wg.Wait();
            return (c.val, error.As(c.err)!, true);
        }
        c = c__prev1;

    }

    ptr<call> c = @new<call>();
    c.wg.Add(1);
    g.m[key] = c;
    g.mu.Unlock();

    g.doCall(c, key, fn);
    return (c.val, error.As(c.err)!, c.dups > 0);

}

// DoChan is like Do but returns a channel that will receive the
// results when they are ready. The second result is true if the function
// will eventually be called, false if it will not (because there is
// a pending request with this key).
private static (channel<Result>, bool) DoChan(this ptr<Group> _addr_g, @string key, Func<(object, error)> fn) {
    channel<Result> _p0 = default;
    bool _p0 = default;
    ref Group g = ref _addr_g.val;

    var ch = make_channel<Result>(1);
    g.mu.Lock();
    if (g.m == null) {
        g.m = make_map<@string, ptr<call>>();
    }
    {
        var c__prev1 = c;

        var (c, ok) = g.m[key];

        if (ok) {
            c.dups++;
            c.chans = append(c.chans, ch);
            g.mu.Unlock();
            return (ch, false);
        }
        c = c__prev1;

    }

    ptr<call> c = addr(new call(chans:[]chan<-Result{ch}));
    c.wg.Add(1);
    g.m[key] = c;
    g.mu.Unlock();

    go_(() => g.doCall(c, key, fn));

    return (ch, true);

}

// doCall handles the single call for a key.
private static (object, error) doCall(this ptr<Group> _addr_g, ptr<call> _addr_c, @string key, Func<(object, error)> fn) {
    object _p0 = default;
    error _p0 = default!;
    ref Group g = ref _addr_g.val;
    ref call c = ref _addr_c.val;

    c.val, c.err = fn();
    c.wg.Done();

    g.mu.Lock();
    delete(g.m, key);
    foreach (var (_, ch) in c.chans) {
        ch.Send(new Result(c.val,c.err,c.dups>0));
    }    g.mu.Unlock();
}

// ForgetUnshared tells the singleflight to forget about a key if it is not
// shared with any other goroutines. Future calls to Do for a forgotten key
// will call the function rather than waiting for an earlier call to complete.
// Returns whether the key was forgotten or unknown--that is, whether no
// other goroutines are waiting for the result.
private static bool ForgetUnshared(this ptr<Group> _addr_g, @string key) => func((defer, _, _) => {
    ref Group g = ref _addr_g.val;

    g.mu.Lock();
    defer(g.mu.Unlock());
    var (c, ok) = g.m[key];
    if (!ok) {
        return true;
    }
    if (c.dups == 0) {
        delete(g.m, key);
        return true;
    }
    return false;

});

} // end singleflight_package
