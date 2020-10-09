// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package context defines the Context type, which carries deadlines,
// cancellation signals, and other request-scoped values across API boundaries
// and between processes.
//
// Incoming requests to a server should create a Context, and outgoing
// calls to servers should accept a Context. The chain of function
// calls between them must propagate the Context, optionally replacing
// it with a derived Context created using WithCancel, WithDeadline,
// WithTimeout, or WithValue. When a Context is canceled, all
// Contexts derived from it are also canceled.
//
// The WithCancel, WithDeadline, and WithTimeout functions take a
// Context (the parent) and return a derived Context (the child) and a
// CancelFunc. Calling the CancelFunc cancels the child and its
// children, removes the parent's reference to the child, and stops
// any associated timers. Failing to call the CancelFunc leaks the
// child and its children until the parent is canceled or the timer
// fires. The go vet tool checks that CancelFuncs are used on all
// control-flow paths.
//
// Programs that use Contexts should follow these rules to keep interfaces
// consistent across packages and enable static analysis tools to check context
// propagation:
//
// Do not store Contexts inside a struct type; instead, pass a Context
// explicitly to each function that needs it. The Context should be the first
// parameter, typically named ctx:
//
//     func DoSomething(ctx context.Context, arg Arg) error {
//         // ... use ctx ...
//     }
//
// Do not pass a nil Context, even if a function permits it. Pass context.TODO
// if you are unsure about which Context to use.
//
// Use context Values only for request-scoped data that transits processes and
// APIs, not for passing optional parameters to functions.
//
// The same Context may be passed to functions running in different goroutines;
// Contexts are safe for simultaneous use by multiple goroutines.
//
// See https://blog.golang.org/context for example code for a server that uses
// Contexts.
// package context -- go2cs converted at 2020 October 09 04:49:50 UTC
// import "context" ==> using context = go.context_package
// Original source: C:\Go\src\context\context.go
using errors = go.errors_package;
using reflectlite = go.@internal.reflectlite_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class context_package
    {
        // A Context carries a deadline, a cancellation signal, and other values across
        // API boundaries.
        //
        // Context's methods may be called by multiple goroutines simultaneously.
        public partial interface Context
        {
            void Deadline(); // Done returns a channel that's closed when work done on behalf of this
// context should be canceled. Done may return nil if this context can
// never be canceled. Successive calls to Done return the same value.
// The close of the Done channel may happen asynchronously,
// after the cancel function returns.
//
// WithCancel arranges for Done to be closed when cancel is called;
// WithDeadline arranges for Done to be closed when the deadline
// expires; WithTimeout arranges for Done to be closed when the timeout
// elapses.
//
// Done is provided for use in select statements:
//
//  // Stream generates values with DoSomething and sends them to out
//  // until DoSomething returns an error or ctx.Done is closed.
//  func Stream(ctx context.Context, out chan<- Value) error {
//      for {
//          v, err := DoSomething(ctx)
//          if err != nil {
//              return err
//          }
//          select {
//          case <-ctx.Done():
//              return ctx.Err()
//          case out <- v:
//          }
//      }
//  }
//
// See https://blog.golang.org/pipelines for more examples of how to use
// a Done channel for cancellation.
            void Done(); // If Done is not yet closed, Err returns nil.
// If Done is closed, Err returns a non-nil error explaining why:
// Canceled if the context was canceled
// or DeadlineExceeded if the context's deadline passed.
// After Err returns a non-nil error, successive calls to Err return the same error.
            void Err(); // Value returns the value associated with this context for key, or nil
// if no value is associated with key. Successive calls to Value with
// the same key returns the same result.
//
// Use context values only for request-scoped data that transits
// processes and API boundaries, not for passing optional parameters to
// functions.
//
// A key identifies a specific value in a Context. Functions that wish
// to store values in Context typically allocate a key in a global
// variable then use that key as the argument to context.WithValue and
// Context.Value. A key can be any type that supports equality;
// packages should define keys as an unexported type to avoid
// collisions.
//
// Packages that define a Context key should provide type-safe accessors
// for the values stored using that key:
//
//     // Package user defines a User type that's stored in Contexts.
//     package user
//
//     import "context"
//
//     // User is the type of value stored in the Contexts.
//     type User struct {...}
//
//     // key is an unexported type for keys defined in this package.
//     // This prevents collisions with keys defined in other packages.
//     type key int
//
//     // userKey is the key for user.User values in Contexts. It is
//     // unexported; clients use user.NewContext and user.FromContext
//     // instead of using this key directly.
//     var userKey key
//
//     // NewContext returns a new Context that carries value u.
//     func NewContext(ctx context.Context, u *User) context.Context {
//         return context.WithValue(ctx, userKey, u)
//     }
//
//     // FromContext returns the User value stored in ctx, if any.
//     func FromContext(ctx context.Context) (*User, bool) {
//         u, ok := ctx.Value(userKey).(*User)
//         return u, ok
//     }
            void Value(object key);
        }

        // Canceled is the error returned by Context.Err when the context is canceled.
        public static var Canceled = errors.New("context canceled");

        // DeadlineExceeded is the error returned by Context.Err when the context's
        // deadline passes.
        public static error DeadlineExceeded = error.As(new deadlineExceededError())!;

        private partial struct deadlineExceededError
        {
        }

        private static @string Error(this deadlineExceededError _p0)
        {
            return "context deadline exceeded";
        }
        private static bool Timeout(this deadlineExceededError _p0)
        {
            return true;
        }
        private static bool Temporary(this deadlineExceededError _p0)
        {
            return true;
        }

        // An emptyCtx is never canceled, has no values, and has no deadline. It is not
        // struct{}, since vars of this type must have distinct addresses.
        private partial struct emptyCtx // : long
        {
        }

        private static (time.Time, bool) Deadline(this ptr<emptyCtx> _addr__p0)
        {
            time.Time deadline = default;
            bool ok = default;
            ref emptyCtx _p0 = ref _addr__p0.val;

            return ;
        }

        private static channel<object> Done(this ptr<emptyCtx> _addr__p0)
        {
            ref emptyCtx _p0 = ref _addr__p0.val;

            return null;
        }

        private static error Err(this ptr<emptyCtx> _addr__p0)
        {
            ref emptyCtx _p0 = ref _addr__p0.val;

            return error.As(null!)!;
        }

        private static void Value(this ptr<emptyCtx> _addr__p0, object key)
        {
            ref emptyCtx _p0 = ref _addr__p0.val;

            return null;
        }

        private static @string String(this ptr<emptyCtx> _addr_e)
        {
            ref emptyCtx e = ref _addr_e.val;


            if (e == background) 
                return "context.Background";
            else if (e == todo) 
                return "context.TODO";
                        return "unknown empty Context";

        }

        private static ptr<emptyCtx> background = @new<emptyCtx>();        private static ptr<emptyCtx> todo = @new<emptyCtx>();

        // Background returns a non-nil, empty Context. It is never canceled, has no
        // values, and has no deadline. It is typically used by the main function,
        // initialization, and tests, and as the top-level Context for incoming
        // requests.
        public static Context Background()
        {
            return background;
        }

        // TODO returns a non-nil, empty Context. Code should use context.TODO when
        // it's unclear which Context to use or it is not yet available (because the
        // surrounding function has not yet been extended to accept a Context
        // parameter).
        public static Context TODO()
        {
            return todo;
        }

        // A CancelFunc tells an operation to abandon its work.
        // A CancelFunc does not wait for the work to stop.
        // A CancelFunc may be called by multiple goroutines simultaneously.
        // After the first call, subsequent calls to a CancelFunc do nothing.
        public delegate void CancelFunc();

        // WithCancel returns a copy of parent with a new Done channel. The returned
        // context's Done channel is closed when the returned cancel function is called
        // or when the parent context's Done channel is closed, whichever happens first.
        //
        // Canceling this context releases resources associated with it, so code should
        // call cancel as soon as the operations running in this Context complete.
        public static (Context, CancelFunc) WithCancel(Context parent) => func((_, panic, __) =>
        {
            Context ctx = default;
            CancelFunc cancel = default;

            if (parent == null)
            {
                panic("cannot create context from nil parent");
            }

            ref var c = ref heap(newCancelCtx(parent), out ptr<var> _addr_c);
            propagateCancel(parent, _addr_c);
            return (_addr_c, () =>
            {
                c.cancel(true, Canceled);
            });

        });

        // newCancelCtx returns an initialized cancelCtx.
        private static cancelCtx newCancelCtx(Context parent)
        {
            return new cancelCtx(Context:parent);
        }

        // goroutines counts the number of goroutines ever created; for testing.
        private static int goroutines = default;

        // propagateCancel arranges for child to be canceled when parent is.
        private static void propagateCancel(Context parent, canceler child)
        {
            var done = parent.Done();
            if (done == null)
            {
                return ; // parent is never canceled
            }

            child.cancel(false, parent.Err());
            return ;
            {
                var (p, ok) = parentCancelCtx(parent);

                if (ok)
                {
                    p.mu.Lock();
                    if (p.err != null)
                    { 
                        // parent has already been canceled
                        child.cancel(false, p.err);

                    }
                    else
                    {
                        if (p.children == null)
                        {
                            p.children = make();
                        }

                        p.children[child] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};

                    }

                    p.mu.Unlock();

                }
                else
                {
                    atomic.AddInt32(_addr_goroutines, +1L);
                    go_(() => () =>
                    {
                        child.cancel(false, parent.Err());
                    }());

                }

            }

        }

        // &cancelCtxKey is the key that a cancelCtx returns itself for.
        private static long cancelCtxKey = default;

        // parentCancelCtx returns the underlying *cancelCtx for parent.
        // It does this by looking up parent.Value(&cancelCtxKey) to find
        // the innermost enclosing *cancelCtx and then checking whether
        // parent.Done() matches that *cancelCtx. (If not, the *cancelCtx
        // has been wrapped in a custom implementation providing a
        // different done channel, in which case we should not bypass it.)
        private static (ptr<cancelCtx>, bool) parentCancelCtx(Context parent)
        {
            ptr<cancelCtx> _p0 = default!;
            bool _p0 = default;

            var done = parent.Done();
            if (done == closedchan || done == null)
            {
                return (_addr_null!, false);
            }

            ptr<cancelCtx> (p, ok) = parent.Value(_addr_cancelCtxKey)._<ptr<cancelCtx>>();
            if (!ok)
            {
                return (_addr_null!, false);
            }

            p.mu.Lock();
            ok = p.done == done;
            p.mu.Unlock();
            if (!ok)
            {
                return (_addr_null!, false);
            }

            return (_addr_p!, true);

        }

        // removeChild removes a context from its parent.
        private static void removeChild(Context parent, canceler child)
        {
            var (p, ok) = parentCancelCtx(parent);
            if (!ok)
            {
                return ;
            }

            p.mu.Lock();
            if (p.children != null)
            {
                delete(p.children, child);
            }

            p.mu.Unlock();

        }

        // A canceler is a context type that can be canceled directly. The
        // implementations are *cancelCtx and *timerCtx.
        private partial interface canceler
        {
            channel<object> cancel(bool removeFromParent, error err);
            channel<object> Done();
        }

        // closedchan is a reusable closed channel.
        private static var closedchan = make_channel<object>();

        private static void init()
        {
            close(closedchan);
        }

        // A cancelCtx can be canceled. When canceled, it also cancels any children
        // that implement canceler.
        private partial struct cancelCtx : Context
        {
            public Context Context;
            public sync.Mutex mu; // protects following fields
            public channel<object> done; // created lazily, closed by first cancel call
            public error err; // set to non-nil by the first cancel call
        }

        private static void Value(this ptr<cancelCtx> _addr_c, object key)
        {
            ref cancelCtx c = ref _addr_c.val;

            if (key == _addr_cancelCtxKey)
            {
                return c;
            }

            return c.Context.Value(key);

        }

        private static channel<object> Done(this ptr<cancelCtx> _addr_c)
        {
            ref cancelCtx c = ref _addr_c.val;

            c.mu.Lock();
            if (c.done == null)
            {
                c.done = make_channel<object>();
            }

            var d = c.done;
            c.mu.Unlock();
            return d;

        }

        private static error Err(this ptr<cancelCtx> _addr_c)
        {
            ref cancelCtx c = ref _addr_c.val;

            c.mu.Lock();
            var err = c.err;
            c.mu.Unlock();
            return error.As(err)!;
        }

        private partial interface stringer
        {
            @string String();
        }

        private static @string contextName(Context c)
        {
            {
                stringer (s, ok) = stringer.As(c._<stringer>())!;

                if (ok)
                {
                    return s.String();
                }

            }

            return reflectlite.TypeOf(c).String();

        }

        private static @string String(this ptr<cancelCtx> _addr_c)
        {
            ref cancelCtx c = ref _addr_c.val;

            return contextName(c.Context) + ".WithCancel";
        }

        // cancel closes c.done, cancels each of c's children, and, if
        // removeFromParent is true, removes c from its parent's children.
        private static void cancel(this ptr<cancelCtx> _addr_c, bool removeFromParent, error err) => func((_, panic, __) =>
        {
            ref cancelCtx c = ref _addr_c.val;

            if (err == null)
            {
                panic("context: internal error: missing cancel error");
            }

            c.mu.Lock();
            if (c.err != null)
            {
                c.mu.Unlock();
                return ; // already canceled
            }

            c.err = err;
            if (c.done == null)
            {
                c.done = closedchan;
            }
            else
            {
                close(c.done);
            }

            foreach (var (child) in c.children)
            { 
                // NOTE: acquiring the child's lock while holding parent's lock.
                child.cancel(false, err);

            }
            c.children = null;
            c.mu.Unlock();

            if (removeFromParent)
            {
                removeChild(c.Context, c);
            }

        });

        // WithDeadline returns a copy of the parent context with the deadline adjusted
        // to be no later than d. If the parent's deadline is already earlier than d,
        // WithDeadline(parent, d) is semantically equivalent to parent. The returned
        // context's Done channel is closed when the deadline expires, when the returned
        // cancel function is called, or when the parent context's Done channel is
        // closed, whichever happens first.
        //
        // Canceling this context releases resources associated with it, so code should
        // call cancel as soon as the operations running in this Context complete.
        public static (Context, CancelFunc) WithDeadline(Context parent, time.Time d) => func((defer, panic, _) =>
        {
            Context _p0 = default;
            CancelFunc _p0 = default;

            if (parent == null)
            {
                panic("cannot create context from nil parent");
            }

            {
                var (cur, ok) = parent.Deadline();

                if (ok && cur.Before(d))
                { 
                    // The current deadline is already sooner than the new one.
                    return WithCancel(parent);

                }

            }

            ptr<timerCtx> c = addr(new timerCtx(cancelCtx:newCancelCtx(parent),deadline:d,));
            propagateCancel(parent, c);
            var dur = time.Until(d);
            if (dur <= 0L)
            {
                c.cancel(true, DeadlineExceeded); // deadline has already passed
                return (c, () =>
                {
                    c.cancel(false, Canceled);
                });

            }

            c.mu.Lock();
            defer(c.mu.Unlock());
            if (c.err == null)
            {
                c.timer = time.AfterFunc(dur, () =>
                {
                    c.cancel(true, DeadlineExceeded);
                });

            }

            return (c, () =>
            {
                c.cancel(true, Canceled);
            });

        });

        // A timerCtx carries a timer and a deadline. It embeds a cancelCtx to
        // implement Done and Err. It implements cancel by stopping its timer then
        // delegating to cancelCtx.cancel.
        private partial struct timerCtx
        {
            public ref cancelCtx cancelCtx => ref cancelCtx_val;
            public ptr<time.Timer> timer; // Under cancelCtx.mu.

            public time.Time deadline;
        }

        private static (time.Time, bool) Deadline(this ptr<timerCtx> _addr_c)
        {
            time.Time deadline = default;
            bool ok = default;
            ref timerCtx c = ref _addr_c.val;

            return (c.deadline, true);
        }

        private static @string String(this ptr<timerCtx> _addr_c)
        {
            ref timerCtx c = ref _addr_c.val;

            return contextName(c.cancelCtx.Context) + ".WithDeadline(" + c.deadline.String() + " [" + time.Until(c.deadline).String() + "])";
        }

        private static void cancel(this ptr<timerCtx> _addr_c, bool removeFromParent, error err)
        {
            ref timerCtx c = ref _addr_c.val;

            c.cancelCtx.cancel(false, err);
            if (removeFromParent)
            { 
                // Remove this timerCtx from its parent cancelCtx's children.
                removeChild(c.cancelCtx.Context, c);

            }

            c.mu.Lock();
            if (c.timer != null)
            {
                c.timer.Stop();
                c.timer = null;
            }

            c.mu.Unlock();

        }

        // WithTimeout returns WithDeadline(parent, time.Now().Add(timeout)).
        //
        // Canceling this context releases resources associated with it, so code should
        // call cancel as soon as the operations running in this Context complete:
        //
        //     func slowOperationWithTimeout(ctx context.Context) (Result, error) {
        //         ctx, cancel := context.WithTimeout(ctx, 100*time.Millisecond)
        //         defer cancel()  // releases resources if slowOperation completes before timeout elapses
        //         return slowOperation(ctx)
        //     }
        public static (Context, CancelFunc) WithTimeout(Context parent, time.Duration timeout)
        {
            Context _p0 = default;
            CancelFunc _p0 = default;

            return WithDeadline(parent, time.Now().Add(timeout));
        }

        // WithValue returns a copy of parent in which the value associated with key is
        // val.
        //
        // Use context Values only for request-scoped data that transits processes and
        // APIs, not for passing optional parameters to functions.
        //
        // The provided key must be comparable and should not be of type
        // string or any other built-in type to avoid collisions between
        // packages using context. Users of WithValue should define their own
        // types for keys. To avoid allocating when assigning to an
        // interface{}, context keys often have concrete type
        // struct{}. Alternatively, exported context key variables' static
        // type should be a pointer or interface.
        public static Context WithValue(Context parent, object key, object val) => func((_, panic, __) =>
        {
            if (parent == null)
            {
                panic("cannot create context from nil parent");
            }

            if (key == null)
            {
                panic("nil key");
            }

            if (!reflectlite.TypeOf(key).Comparable())
            {
                panic("key is not comparable");
            }

            return addr(new valueCtx(parent,key,val));

        });

        // A valueCtx carries a key-value pair. It implements Value for that key and
        // delegates all other calls to the embedded Context.
        private partial struct valueCtx : Context
        {
            public Context Context;
        }

        // stringify tries a bit to stringify v, without using fmt, since we don't
        // want context depending on the unicode tables. This is only used by
        // *valueCtx.String().
        private static @string stringify(object v)
        {
            switch (v.type())
            {
                case stringer s:
                    return s.String();
                    break;
                case @string s:
                    return s;
                    break;
            }
            return "<not Stringer>";

        }

        private static @string String(this ptr<valueCtx> _addr_c)
        {
            ref valueCtx c = ref _addr_c.val;

            return contextName(c.Context) + ".WithValue(type " + reflectlite.TypeOf(c.key).String() + ", val " + stringify(c.val) + ")";
        }

        private static void Value(this ptr<valueCtx> _addr_c, object key)
        {
            ref valueCtx c = ref _addr_c.val;

            if (c.key == key)
            {
                return c.val;
            }

            return c.Context.Value(key);

        }
    }
}
