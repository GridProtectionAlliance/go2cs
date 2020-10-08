// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Type conversions for Scan.

// package sql -- go2cs converted at 2020 October 08 04:58:46 UTC
// import "database/sql" ==> using sql = go.database.sql_package
// Original source: C:\Go\src\database\sql\convert.go
using driver = go.database.sql.driver_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using reflect = go.reflect_package;
using strconv = go.strconv_package;
using time = go.time_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace database
{
    public static partial class sql_package
    {
        private static var errNilPtr = errors.New("destination pointer is nil"); // embedded in descriptive error

        private static @string describeNamedValue(ptr<driver.NamedValue> _addr_nv)
        {
            ref driver.NamedValue nv = ref _addr_nv.val;

            if (len(nv.Name) == 0L)
            {
                return fmt.Sprintf("$%d", nv.Ordinal);
            }

            return fmt.Sprintf("with name %q", nv.Name);

        }

        private static error validateNamedValueName(@string name)
        {
            if (len(name) == 0L)
            {
                return error.As(null!)!;
            }

            var (r, _) = utf8.DecodeRuneInString(name);
            if (unicode.IsLetter(r))
            {
                return error.As(null!)!;
            }

            return error.As(fmt.Errorf("name %q does not begin with a letter", name))!;

        }

        // ccChecker wraps the driver.ColumnConverter and allows it to be used
        // as if it were a NamedValueChecker. If the driver ColumnConverter
        // is not present then the NamedValueChecker will return driver.ErrSkip.
        private partial struct ccChecker
        {
            public driver.ColumnConverter cci;
            public long want;
        }

        private static error CheckNamedValue(this ccChecker c, ptr<driver.NamedValue> _addr_nv)
        {
            ref driver.NamedValue nv = ref _addr_nv.val;

            if (c.cci == null)
            {
                return error.As(driver.ErrSkip)!;
            } 
            // The column converter shouldn't be called on any index
            // it isn't expecting. The final error will be thrown
            // in the argument converter loop.
            var index = nv.Ordinal - 1L;
            if (c.want <= index)
            {
                return error.As(null!)!;
            } 

            // First, see if the value itself knows how to convert
            // itself to a driver type. For example, a NullString
            // struct changing into a string or nil.
            {
                driver.Valuer (vr, ok) = nv.Value._<driver.Valuer>();

                if (ok)
                {
                    var (sv, err) = callValuerValue(vr);
                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    if (!driver.IsValue(sv))
                    {
                        return error.As(fmt.Errorf("non-subset type %T returned from Value", sv))!;
                    }

                    nv.Value = sv;

                } 

                // Second, ask the column to sanity check itself. For
                // example, drivers might use this to make sure that
                // an int64 values being inserted into a 16-bit
                // integer field is in range (before getting
                // truncated), or that a nil can't go into a NOT NULL
                // column before going across the network to get the
                // same error.

            } 

            // Second, ask the column to sanity check itself. For
            // example, drivers might use this to make sure that
            // an int64 values being inserted into a 16-bit
            // integer field is in range (before getting
            // truncated), or that a nil can't go into a NOT NULL
            // column before going across the network to get the
            // same error.
            error err = default!;
            var arg = nv.Value;
            nv.Value, err = c.cci.ColumnConverter(index).ConvertValue(arg);
            if (err != null)
            {
                return error.As(err)!;
            }

            if (!driver.IsValue(nv.Value))
            {
                return error.As(fmt.Errorf("driver ColumnConverter error converted %T to unsupported type %T", arg, nv.Value))!;
            }

            return error.As(null!)!;

        }

        // defaultCheckNamedValue wraps the default ColumnConverter to have the same
        // function signature as the CheckNamedValue in the driver.NamedValueChecker
        // interface.
        private static error defaultCheckNamedValue(ptr<driver.NamedValue> _addr_nv)
        {
            error err = default!;
            ref driver.NamedValue nv = ref _addr_nv.val;

            nv.Value, err = driver.DefaultParameterConverter.ConvertValue(nv.Value);
            return error.As(err)!;
        }

        // driverArgsConnLocked converts arguments from callers of Stmt.Exec and
        // Stmt.Query into driver Values.
        //
        // The statement ds may be nil, if no statement is available.
        //
        // ci must be locked.
        private static (slice<driver.NamedValue>, error) driverArgsConnLocked(driver.Conn ci, ptr<driverStmt> _addr_ds, slice<object> args)
        {
            slice<driver.NamedValue> _p0 = default;
            error _p0 = default!;
            ref driverStmt ds = ref _addr_ds.val;

            var nvargs = make_slice<driver.NamedValue>(len(args)); 

            // -1 means the driver doesn't know how to count the number of
            // placeholders, so we won't sanity check input here and instead let the
            // driver deal with errors.
            long want = -1L;

            driver.Stmt si = default;
            ccChecker cc = default;
            if (ds != null)
            {
                si = ds.si;
                want = ds.si.NumInput();
                cc.want = want;
            } 

            // Check all types of interfaces from the start.
            // Drivers may opt to use the NamedValueChecker for special
            // argument types, then return driver.ErrSkip to pass it along
            // to the column converter.
            driver.NamedValueChecker (nvc, ok) = si._<driver.NamedValueChecker>();
            if (!ok)
            {
                nvc, ok = ci._<driver.NamedValueChecker>();
            }

            driver.ColumnConverter (cci, ok) = si._<driver.ColumnConverter>();
            if (ok)
            {
                cc.cci = cci;
            } 

            // Loop through all the arguments, checking each one.
            // If no error is returned simply increment the index
            // and continue. However if driver.ErrRemoveArgument
            // is returned the argument is not included in the query
            // argument list.
            error err = default!;
            long n = default;
            foreach (var (_, arg) in args)
            {
                var nv = _addr_nvargs[n];
                {
                    NamedArg (np, ok) = arg._<NamedArg>();

                    if (ok)
                    {
                        err = error.As(validateNamedValueName(np.Name))!;

                        if (err != null)
                        {
                            return (null, error.As(err)!);
                        }

                        arg = np.Value;
                        nv.Name = np.Name;

                    }

                }

                nv.Ordinal = n + 1L;
                nv.Value = arg; 

                // Checking sequence has four routes:
                // A: 1. Default
                // B: 1. NamedValueChecker 2. Column Converter 3. Default
                // C: 1. NamedValueChecker 3. Default
                // D: 1. Column Converter 2. Default
                //
                // The only time a Column Converter is called is first
                // or after NamedValueConverter. If first it is handled before
                // the nextCheck label. Thus for repeats tries only when the
                // NamedValueConverter is selected should the Column Converter
                // be used in the retry.
                var checker = defaultCheckNamedValue;
                var nextCC = false;

                if (nvc != null) 
                    nextCC = cci != null;
                    checker = nvc.CheckNamedValue;
                else if (cci != null) 
                    checker = cc.CheckNamedValue;
                nextCheck:
                err = error.As(checker(nv))!;

                if (err == null) 
                    n++;
                    continue;
                else if (err == driver.ErrRemoveArgument) 
                    nvargs = nvargs[..len(nvargs) - 1L];
                    continue;
                else if (err == driver.ErrSkip) 
                    if (nextCC)
                    {
                        nextCC = false;
                        checker = cc.CheckNamedValue;
                    }
                    else
                    {
                        checker = defaultCheckNamedValue;
                    }

                    goto nextCheck;
                else 
                    return (null, error.As(fmt.Errorf("sql: converting argument %s type: %v", describeNamedValue(_addr_nv), err))!);
                
            } 

            // Check the length of arguments after conversion to allow for omitted
            // arguments.
            if (want != -1L && len(nvargs) != want)
            {
                return (null, error.As(fmt.Errorf("sql: expected %d arguments, got %d", want, len(nvargs)))!);
            }

            return (nvargs, error.As(null!)!);


        }

        // convertAssign is the same as convertAssignRows, but without the optional
        // rows argument.
        private static error convertAssign(object dest, object src)
        {
            return error.As(convertAssignRows(dest, src, _addr_null))!;
        }

        // convertAssignRows copies to dest the value in src, converting it if possible.
        // An error is returned if the copy would result in loss of information.
        // dest should be a pointer type. If rows is passed in, the rows will
        // be used as the parent for any cursor values converted from a
        // driver.Rows to a *Rows.
        private static error convertAssignRows(object dest, object src, ptr<Rows> _addr_rows)
        {
            ref Rows rows = ref _addr_rows.val;
 
            // Common cases, without reflect.
            switch (src.type())
            {
                case @string s:
                    switch (dest.type())
                    {
                        case ptr<@string> d:
                            if (d == null)
                            {
                                return error.As(errNilPtr)!;
                            }

                            d.val = s;
                            return error.As(null!)!;
                            break;
                        case ptr<slice<byte>> d:
                            if (d == null)
                            {
                                return error.As(errNilPtr)!;
                            }

                            d.val = (slice<byte>)s;
                            return error.As(null!)!;
                            break;
                        case ptr<RawBytes> d:
                            if (d == null)
                            {
                                return error.As(errNilPtr)!;
                            }

                            d.val = append((d.val)[..0L], s);
                            return error.As(null!)!;
                            break;
                    }
                    break;
                case slice<byte> s:
                    switch (dest.type())
                    {
                        case ptr<@string> d:
                            if (d == null)
                            {
                                return error.As(errNilPtr)!;
                            }

                            d.val = string(s);
                            return error.As(null!)!;
                            break;
                        case 
                            if (d == null)
                            {
                                return error.As(errNilPtr)!;
                            }

                            d.val = cloneBytes(s);
                            return error.As(null!)!;
                            break;
                        case ptr<slice<byte>> d:
                            if (d == null)
                            {
                                return error.As(errNilPtr)!;
                            }

                            d.val = cloneBytes(s);
                            return error.As(null!)!;
                            break;
                        case ptr<RawBytes> d:
                            if (d == null)
                            {
                                return error.As(errNilPtr)!;
                            }

                            d.val = s;
                            return error.As(null!)!;
                            break;
                    }
                    break;
                case time.Time s:
                    switch (dest.type())
                    {
                        case ptr<time.Time> d:
                            d.val = s;
                            return error.As(null!)!;
                            break;
                        case ptr<@string> d:
                            d.val = s.Format(time.RFC3339Nano);
                            return error.As(null!)!;
                            break;
                        case ptr<slice<byte>> d:
                            if (d == null)
                            {
                                return error.As(errNilPtr)!;
                            }

                            d.val = (slice<byte>)s.Format(time.RFC3339Nano);
                            return error.As(null!)!;
                            break;
                        case ptr<RawBytes> d:
                            if (d == null)
                            {
                                return error.As(errNilPtr)!;
                            }

                            d.val = s.AppendFormat((d.val)[..0L], time.RFC3339Nano);
                            return error.As(null!)!;
                            break;
                    }
                    break;
                case decimalDecompose s:
                    switch (dest.type())
                    {
                        case decimalCompose d:
                            return error.As(d.Compose(s.Decompose(null)))!;
                            break;
                    }
                    break;
                case 
                    switch (dest.type())
                    {
                        case 
                            if (d == null)
                            {
                                return error.As(errNilPtr)!;
                            }

                            d.val = null;
                            return error.As(null!)!;
                            break;
                        case ptr<slice<byte>> d:
                            if (d == null)
                            {
                                return error.As(errNilPtr)!;
                            }

                            d.val = null;
                            return error.As(null!)!;
                            break;
                        case ptr<RawBytes> d:
                            if (d == null)
                            {
                                return error.As(errNilPtr)!;
                            }

                            d.val = null;
                            return error.As(null!)!;
                            break; 
                        // The driver is returning a cursor the client may iterate over.
                    } 
                    // The driver is returning a cursor the client may iterate over.
                    break;
                case driver.Rows s:
                    switch (dest.type())
                    {
                        case ptr<Rows> d:
                            if (d == null)
                            {
                                return error.As(errNilPtr)!;
                            }

                            if (rows == null)
                            {
                                return error.As(errors.New("invalid context to convert cursor rows, missing parent *Rows"))!;
                            }

                            rows.closemu.Lock();
                            d.val = new Rows(dc:rows.dc,releaseConn:func(error){},rowsi:s,); 
                            // Chain the cancel function.
                            var parentCancel = rows.cancel;
                            rows.cancel = () =>
                            { 
                                // When Rows.cancel is called, the closemu will be locked as well.
                                // So we can access rs.lasterr.
                                d.close(rows.lasterr);
                                if (parentCancel != null)
                                {
                                    parentCancel();
                                }

                            }
;
                            rows.closemu.Unlock();
                            return error.As(null!)!;
                            break;
                    }
                    break;

            }

            reflect.Value sv = default;

            switch (dest.type())
            {
                case ptr<@string> d:
                    sv = reflect.ValueOf(src);

                    if (sv.Kind() == reflect.Bool || sv.Kind() == reflect.Int || sv.Kind() == reflect.Int8 || sv.Kind() == reflect.Int16 || sv.Kind() == reflect.Int32 || sv.Kind() == reflect.Int64 || sv.Kind() == reflect.Uint || sv.Kind() == reflect.Uint8 || sv.Kind() == reflect.Uint16 || sv.Kind() == reflect.Uint32 || sv.Kind() == reflect.Uint64 || sv.Kind() == reflect.Float32 || sv.Kind() == reflect.Float64) 
                        d.val = asString(src);
                        return error.As(null!)!;
                                        break;
                case ptr<slice<byte>> d:
                    sv = reflect.ValueOf(src);
                    {
                        var b__prev1 = b;

                        var (b, ok) = asBytes(null, sv);

                        if (ok)
                        {
                            d.val = b;
                            return error.As(null!)!;
                        }

                        b = b__prev1;

                    }

                    break;
                case ptr<RawBytes> d:
                    sv = reflect.ValueOf(src);
                    {
                        var b__prev1 = b;

                        (b, ok) = asBytes((slice<byte>)d.val[..0L], sv);

                        if (ok)
                        {
                            d.val = RawBytes(b);
                            return error.As(null!)!;
                        }

                        b = b__prev1;

                    }

                    break;
                case ptr<bool> d:
                    var (bv, err) = driver.Bool.ConvertValue(src);
                    if (err == null)
                    {
                        d.val = bv._<bool>();
                    }

                    return error.As(err)!;
                    break;
                case 
                    d.val = src;
                    return error.As(null!)!;
                    break;

            }

            {
                Scanner (scanner, ok) = dest._<Scanner>();

                if (ok)
                {
                    return error.As(scanner.Scan(src))!;
                }

            }


            var dpv = reflect.ValueOf(dest);
            if (dpv.Kind() != reflect.Ptr)
            {
                return error.As(errors.New("destination not a pointer"))!;
            }

            if (dpv.IsNil())
            {
                return error.As(errNilPtr)!;
            }

            if (!sv.IsValid())
            {
                sv = reflect.ValueOf(src);
            }

            var dv = reflect.Indirect(dpv);
            if (sv.IsValid() && sv.Type().AssignableTo(dv.Type()))
            {
                switch (src.type())
                {
                    case slice<byte> b:
                        dv.Set(reflect.ValueOf(cloneBytes(b)));
                        break;
                    default:
                    {
                        var b = src.type();
                        dv.Set(sv);
                        break;
                    }
                }
                return error.As(null!)!;

            }

            if (dv.Kind() == sv.Kind() && sv.Type().ConvertibleTo(dv.Type()))
            {
                dv.Set(sv.Convert(dv.Type()));
                return error.As(null!)!;
            } 

            // The following conversions use a string value as an intermediate representation
            // to convert between various numeric types.
            //
            // This also allows scanning into user defined types such as "type Int int64".
            // For symmetry, also check for string destination types.

            if (dv.Kind() == reflect.Ptr) 
                if (src == null)
                {
                    dv.Set(reflect.Zero(dv.Type()));
                    return error.As(null!)!;
                }

                dv.Set(reflect.New(dv.Type().Elem()));
                return error.As(convertAssignRows(dv.Interface(), src, _addr_rows))!;
            else if (dv.Kind() == reflect.Int || dv.Kind() == reflect.Int8 || dv.Kind() == reflect.Int16 || dv.Kind() == reflect.Int32 || dv.Kind() == reflect.Int64) 
                if (src == null)
                {
                    return error.As(fmt.Errorf("converting NULL to %s is unsupported", dv.Kind()))!;
                }

                var s = asString(src);
                var (i64, err) = strconv.ParseInt(s, 10L, dv.Type().Bits());
                if (err != null)
                {
                    err = strconvErr(err);
                    return error.As(fmt.Errorf("converting driver.Value type %T (%q) to a %s: %v", src, s, dv.Kind(), err))!;
                }

                dv.SetInt(i64);
                return error.As(null!)!;
            else if (dv.Kind() == reflect.Uint || dv.Kind() == reflect.Uint8 || dv.Kind() == reflect.Uint16 || dv.Kind() == reflect.Uint32 || dv.Kind() == reflect.Uint64) 
                if (src == null)
                {
                    return error.As(fmt.Errorf("converting NULL to %s is unsupported", dv.Kind()))!;
                }

                s = asString(src);
                var (u64, err) = strconv.ParseUint(s, 10L, dv.Type().Bits());
                if (err != null)
                {
                    err = strconvErr(err);
                    return error.As(fmt.Errorf("converting driver.Value type %T (%q) to a %s: %v", src, s, dv.Kind(), err))!;
                }

                dv.SetUint(u64);
                return error.As(null!)!;
            else if (dv.Kind() == reflect.Float32 || dv.Kind() == reflect.Float64) 
                if (src == null)
                {
                    return error.As(fmt.Errorf("converting NULL to %s is unsupported", dv.Kind()))!;
                }

                s = asString(src);
                var (f64, err) = strconv.ParseFloat(s, dv.Type().Bits());
                if (err != null)
                {
                    err = strconvErr(err);
                    return error.As(fmt.Errorf("converting driver.Value type %T (%q) to a %s: %v", src, s, dv.Kind(), err))!;
                }

                dv.SetFloat(f64);
                return error.As(null!)!;
            else if (dv.Kind() == reflect.String) 
                if (src == null)
                {
                    return error.As(fmt.Errorf("converting NULL to %s is unsupported", dv.Kind()))!;
                }

                switch (src.type())
                {
                    case @string v:
                        dv.SetString(v);
                        return error.As(null!)!;
                        break;
                    case slice<byte> v:
                        dv.SetString(string(v));
                        return error.As(null!)!;
                        break;
                }
                        return error.As(fmt.Errorf("unsupported Scan, storing driver.Value type %T into type %T", src, dest))!;

        }

        private static error strconvErr(error err)
        {
            {
                ptr<strconv.NumError> (ne, ok) = err._<ptr<strconv.NumError>>();

                if (ok)
                {
                    return error.As(ne.Err)!;
                }

            }

            return error.As(err)!;

        }

        private static slice<byte> cloneBytes(slice<byte> b)
        {
            if (b == null)
            {
                return null;
            }

            var c = make_slice<byte>(len(b));
            copy(c, b);
            return c;

        }

        private static @string asString(object src)
        {
            switch (src.type())
            {
                case @string v:
                    return v;
                    break;
                case slice<byte> v:
                    return string(v);
                    break;
            }
            var rv = reflect.ValueOf(src);

            if (rv.Kind() == reflect.Int || rv.Kind() == reflect.Int8 || rv.Kind() == reflect.Int16 || rv.Kind() == reflect.Int32 || rv.Kind() == reflect.Int64) 
                return strconv.FormatInt(rv.Int(), 10L);
            else if (rv.Kind() == reflect.Uint || rv.Kind() == reflect.Uint8 || rv.Kind() == reflect.Uint16 || rv.Kind() == reflect.Uint32 || rv.Kind() == reflect.Uint64) 
                return strconv.FormatUint(rv.Uint(), 10L);
            else if (rv.Kind() == reflect.Float64) 
                return strconv.FormatFloat(rv.Float(), 'g', -1L, 64L);
            else if (rv.Kind() == reflect.Float32) 
                return strconv.FormatFloat(rv.Float(), 'g', -1L, 32L);
            else if (rv.Kind() == reflect.Bool) 
                return strconv.FormatBool(rv.Bool());
                        return fmt.Sprintf("%v", src);

        }

        private static (slice<byte>, bool) asBytes(slice<byte> buf, reflect.Value rv)
        {
            slice<byte> b = default;
            bool ok = default;


            if (rv.Kind() == reflect.Int || rv.Kind() == reflect.Int8 || rv.Kind() == reflect.Int16 || rv.Kind() == reflect.Int32 || rv.Kind() == reflect.Int64) 
                return (strconv.AppendInt(buf, rv.Int(), 10L), true);
            else if (rv.Kind() == reflect.Uint || rv.Kind() == reflect.Uint8 || rv.Kind() == reflect.Uint16 || rv.Kind() == reflect.Uint32 || rv.Kind() == reflect.Uint64) 
                return (strconv.AppendUint(buf, rv.Uint(), 10L), true);
            else if (rv.Kind() == reflect.Float32) 
                return (strconv.AppendFloat(buf, rv.Float(), 'g', -1L, 32L), true);
            else if (rv.Kind() == reflect.Float64) 
                return (strconv.AppendFloat(buf, rv.Float(), 'g', -1L, 64L), true);
            else if (rv.Kind() == reflect.Bool) 
                return (strconv.AppendBool(buf, rv.Bool()), true);
            else if (rv.Kind() == reflect.String) 
                var s = rv.String();
                return (append(buf, s), true);
                        return ;

        }

        private static var valuerReflectType = reflect.TypeOf((driver.Valuer.val)(null)).Elem();

        // callValuerValue returns vr.Value(), with one exception:
        // If vr.Value is an auto-generated method on a pointer type and the
        // pointer is nil, it would panic at runtime in the panicwrap
        // method. Treat it like nil instead.
        // Issue 8415.
        //
        // This is so people can implement driver.Value on value types and
        // still use nil pointers to those types to mean nil/NULL, just like
        // string/*string.
        //
        // This function is mirrored in the database/sql/driver package.
        private static (driver.Value, error) callValuerValue(driver.Valuer vr)
        {
            driver.Value v = default;
            error err = default!;

            {
                var rv = reflect.ValueOf(vr);

                if (rv.Kind() == reflect.Ptr && rv.IsNil() && rv.Type().Elem().Implements(valuerReflectType))
                {
                    return (null, error.As(null!)!);
                }

            }

            return vr.Value();

        }

        // decimal composes or decomposes a decimal value to and from individual parts.
        // There are four parts: a boolean negative flag, a form byte with three possible states
        // (finite=0, infinite=1, NaN=2), a base-2 big-endian integer
        // coefficient (also known as a significand) as a []byte, and an int32 exponent.
        // These are composed into a final value as "decimal = (neg) (form=finite) coefficient * 10 ^ exponent".
        // A zero length coefficient is a zero value.
        // The big-endian integer coefficient stores the most significant byte first (at coefficient[0]).
        // If the form is not finite the coefficient and exponent should be ignored.
        // The negative parameter may be set to true for any form, although implementations are not required
        // to respect the negative parameter in the non-finite form.
        //
        // Implementations may choose to set the negative parameter to true on a zero or NaN value,
        // but implementations that do not differentiate between negative and positive
        // zero or NaN values should ignore the negative parameter without error.
        // If an implementation does not support Infinity it may be converted into a NaN without error.
        // If a value is set that is larger than what is supported by an implementation,
        // an error must be returned.
        // Implementations must return an error if a NaN or Infinity is attempted to be set while neither
        // are supported.
        //
        // NOTE(kardianos): This is an experimental interface. See https://golang.org/issue/30870
        private partial interface @decimal : decimalDecompose, decimalCompose
        {
        }

        private partial interface decimalDecompose
        {
            (byte, bool, slice<byte>, int) Decompose(slice<byte> buf);
        }

        private partial interface decimalCompose
        {
            error Compose(byte form, bool negative, slice<byte> coefficient, int exponent);
        }
    }
}}
