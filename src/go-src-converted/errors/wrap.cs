// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package errors -- go2cs converted at 2020 October 08 03:45:36 UTC
// import "errors" ==> using errors = go.errors_package
// Original source: C:\Go\src\errors\wrap.go
using reflectlite = go.@internal.reflectlite_package;
using static go.builtin;

namespace go
{
    public static partial class errors_package
    {
        // Unwrap returns the result of calling the Unwrap method on err, if err's
        // type contains an Unwrap method returning error.
        // Otherwise, Unwrap returns nil.
        public static error Unwrap(error err)
        {
            if (!ok)
            {
                return error.As(null!)!;
            }
            return error.As(u.Unwrap())!;

        }

        // Is reports whether any error in err's chain matches target.
        //
        // The chain consists of err itself followed by the sequence of errors obtained by
        // repeatedly calling Unwrap.
        //
        // An error is considered to match a target if it is equal to that target or if
        // it implements a method Is(error) bool such that Is(target) returns true.
        //
        // An error type might provide an Is method so it can be treated as equivalent
        // to an existing error. For example, if MyError defines
        //
        //    func (m MyError) Is(target error) bool { return target == os.ErrExist }
        //
        // then Is(MyError{}, os.ErrExist) returns true. See syscall.Errno.Is for
        // an example in the standard library.
        public static bool Is(error err, error target)
        {
            if (target == null)
            {
                return err == target;
            }

            var isComparable = reflectlite.TypeOf(target).Comparable();
            while (true)
            {
                if (isComparable && err == target)
                {
                    return true;
                }

                {

                    if (ok && x.Is(target))
                    {
                        return true;
                    } 
                    // TODO: consider supporting target.Is(err). This would allow
                    // user-definable predicates, but also may allow for coping with sloppy
                    // APIs, thereby making it easier to get away with them.

                } 
                // TODO: consider supporting target.Is(err). This would allow
                // user-definable predicates, but also may allow for coping with sloppy
                // APIs, thereby making it easier to get away with them.
                err = Unwrap(err);

                if (err == null)
                {
                    return false;
                }

            }


        }

        // As finds the first error in err's chain that matches target, and if so, sets
        // target to that error value and returns true. Otherwise, it returns false.
        //
        // The chain consists of err itself followed by the sequence of errors obtained by
        // repeatedly calling Unwrap.
        //
        // An error matches target if the error's concrete value is assignable to the value
        // pointed to by target, or if the error has a method As(interface{}) bool such that
        // As(target) returns true. In the latter case, the As method is responsible for
        // setting target.
        //
        // An error type might provide an As method so it can be treated as if it were a
        // different error type.
        //
        // As panics if target is not a non-nil pointer to either a type that implements
        // error, or to any interface type.
        public static bool As(error err, object target) => func((_, panic, __) =>
        {
            if (target == null)
            {
                panic("errors: target cannot be nil");
            }

            var val = reflectlite.ValueOf(target);
            var typ = val.Type();
            if (typ.Kind() != reflectlite.Ptr || val.IsNil())
            {
                panic("errors: target must be a non-nil pointer");
            }

            {
                var e = typ.Elem();

                if (e.Kind() != reflectlite.Interface && !e.Implements(errorType))
                {
                    panic("errors: *target must be interface or implement error");
                }

            }

            var targetType = typ.Elem();
            while (err != null)
            {
                if (reflectlite.TypeOf(err).AssignableTo(targetType))
                {
                    val.Elem().Set(reflectlite.ValueOf(err));
                    return true;
                }

                {

                    if (ok && x.As(target))
                    {
                        return true;
                    }

                }

                err = Unwrap(err);

            }

            return false;

        });

        private static var errorType = reflectlite.TypeOf((error.val)(null)).Elem();
    }
}
