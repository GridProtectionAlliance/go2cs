// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package xerrors -- go2cs converted at 2020 October 08 04:34:16 UTC
// import "golang.org/x/xerrors" ==> using xerrors = go.golang.org.x.xerrors_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\xerrors\wrap.go
using reflect = go.reflect_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x
{
    public static partial class xerrors_package
    {
        // A Wrapper provides context around another error.
        public partial interface Wrapper
        {
            error Unwrap();
        }

        // Opaque returns an error with the same error formatting as err
        // but that does not match err and cannot be unwrapped.
        public static error Opaque(error err)
        {
            return error.As(new noWrapper(err))!;
        }

        private partial struct noWrapper : error
        {
            public error error;
        }

        private static error FormatError(this noWrapper e, Printer p)
        {
            error next = default!;

            {
                Formatter (f, ok) = e.error._<Formatter>();

                if (ok)
                {
                    return error.As(f.FormatError(p))!;
                }

            }
            p.Print(e.error);
            return error.As(null!)!;
        }

        // Unwrap returns the result of calling the Unwrap method on err, if err implements
        // Unwrap. Otherwise, Unwrap returns nil.
        public static error Unwrap(error err)
        {
            Wrapper (u, ok) = Wrapper.As(err._<Wrapper>())!;
            if (!ok)
            {
                return error.As(null!)!;
            }
            return error.As(u.Unwrap())!;
        }

        // Is reports whether any error in err's chain matches target.
        //
        // An error is considered to match a target if it is equal to that target or if
        // it implements a method Is(error) bool such that Is(target) returns true.
        public static bool Is(error err, error target)
        {
            if (target == null)
            {
                return err == target;
            }
            var isComparable = reflect.TypeOf(target).Comparable();
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
                    // TODO: consider supporing target.Is(err). This would allow
                    // user-definable predicates, but also may allow for coping with sloppy
                    // APIs, thereby making it easier to get away with them.

                } 
                // TODO: consider supporing target.Is(err). This would allow
                // user-definable predicates, but also may allow for coping with sloppy
                // APIs, thereby making it easier to get away with them.
                err = Unwrap(err);

                if (err == null)
                {
                    return false;
                }
            }
        }

        // As finds the first error in err's chain that matches the type to which target
        // points, and if so, sets the target to its value and returns true. An error
        // matches a type if it is assignable to the target type, or if it has a method
        // As(interface{}) bool such that As(target) returns true. As will panic if target
        // is not a non-nil pointer to a type which implements error or is of interface type.
        //
        // The As method should set the target to its value and return true if err
        // matches the type to which target points.
        public static bool As(error err, object target) => func((_, panic, __) =>
        {
            if (target == null)
            {
                panic("errors: target cannot be nil");
            }
            var val = reflect.ValueOf(target);
            var typ = val.Type();
            if (typ.Kind() != reflect.Ptr || val.IsNil())
            {
                panic("errors: target must be a non-nil pointer");
            }
            {
                var e = typ.Elem();

                if (e.Kind() != reflect.Interface && !e.Implements(errorType))
                {
                    panic("errors: *target must be interface or implement error");
                }

            }
            var targetType = typ.Elem();
            while (err != null)
            {
                if (reflect.TypeOf(err).AssignableTo(targetType))
                {
                    val.Elem().Set(reflect.ValueOf(err));
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

        private static var errorType = reflect.TypeOf((error.val)(null)).Elem();
    }
}}}
