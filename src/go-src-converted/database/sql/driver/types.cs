// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package driver -- go2cs converted at 2020 October 08 04:58:47 UTC
// import "database/sql/driver" ==> using driver = go.database.sql.driver_package
// Original source: C:\Go\src\database\sql\driver\types.go
using fmt = go.fmt_package;
using reflect = go.reflect_package;
using strconv = go.strconv_package;
using time = go.time_package;
using static go.builtin;

namespace go {
namespace database {
namespace sql
{
    public static partial class driver_package
    {
        // ValueConverter is the interface providing the ConvertValue method.
        //
        // Various implementations of ValueConverter are provided by the
        // driver package to provide consistent implementations of conversions
        // between drivers. The ValueConverters have several uses:
        //
        //  * converting from the Value types as provided by the sql package
        //    into a database table's specific column type and making sure it
        //    fits, such as making sure a particular int64 fits in a
        //    table's uint16 column.
        //
        //  * converting a value as given from the database into one of the
        //    driver Value types.
        //
        //  * by the sql package, for converting from a driver's Value type
        //    to a user's type in a scan.
        public partial interface ValueConverter
        {
            (Value, error) ConvertValue(object v);
        }

        // Valuer is the interface providing the Value method.
        //
        // Types implementing Valuer interface are able to convert
        // themselves to a driver Value.
        public partial interface Valuer
        {
            (Value, error) Value();
        }

        // Bool is a ValueConverter that converts input values to bools.
        //
        // The conversion rules are:
        //  - booleans are returned unchanged
        //  - for integer types,
        //       1 is true
        //       0 is false,
        //       other integers are an error
        //  - for strings and []byte, same rules as strconv.ParseBool
        //  - all other types are an error
        public static boolType Bool = default;

        private partial struct boolType
        {
        }

        private static ValueConverter _ = ValueConverter.As(new boolType())!;

        private static @string String(this boolType _p0)
        {
            return "Bool";
        }

        private static (Value, error) ConvertValue(this boolType _p0, object src)
        {
            Value _p0 = default;
            error _p0 = default!;

            switch (src.type())
            {
                case bool s:
                    return (s, error.As(null!)!);
                    break;
                case @string s:
                    var (b, err) = strconv.ParseBool(s);
                    if (err != null)
                    {
                        return (null, error.As(fmt.Errorf("sql/driver: couldn't convert %q into type bool", s))!);
                    }

                    return (b, error.As(null!)!);
                    break;
                case slice<byte> s:
                    (b, err) = strconv.ParseBool(string(s));
                    if (err != null)
                    {
                        return (null, error.As(fmt.Errorf("sql/driver: couldn't convert %q into type bool", s))!);
                    }

                    return (b, error.As(null!)!);
                    break;

            }

            var sv = reflect.ValueOf(src);

            if (sv.Kind() == reflect.Int || sv.Kind() == reflect.Int8 || sv.Kind() == reflect.Int16 || sv.Kind() == reflect.Int32 || sv.Kind() == reflect.Int64) 
                var iv = sv.Int();
                if (iv == 1L || iv == 0L)
                {
                    return (iv == 1L, error.As(null!)!);
                }

                return (null, error.As(fmt.Errorf("sql/driver: couldn't convert %d into type bool", iv))!);
            else if (sv.Kind() == reflect.Uint || sv.Kind() == reflect.Uint8 || sv.Kind() == reflect.Uint16 || sv.Kind() == reflect.Uint32 || sv.Kind() == reflect.Uint64) 
                var uv = sv.Uint();
                if (uv == 1L || uv == 0L)
                {
                    return (uv == 1L, error.As(null!)!);
                }

                return (null, error.As(fmt.Errorf("sql/driver: couldn't convert %d into type bool", uv))!);
                        return (null, error.As(fmt.Errorf("sql/driver: couldn't convert %v (%T) into type bool", src, src))!);

        }

        // Int32 is a ValueConverter that converts input values to int64,
        // respecting the limits of an int32 value.
        public static int32Type Int32 = default;

        private partial struct int32Type
        {
        }

        private static ValueConverter _ = ValueConverter.As(new int32Type())!;

        private static (Value, error) ConvertValue(this int32Type _p0, object v)
        {
            Value _p0 = default;
            error _p0 = default!;

            var rv = reflect.ValueOf(v);

            if (rv.Kind() == reflect.Int || rv.Kind() == reflect.Int8 || rv.Kind() == reflect.Int16 || rv.Kind() == reflect.Int32 || rv.Kind() == reflect.Int64) 
                var i64 = rv.Int();
                if (i64 > (1L << (int)(31L)) - 1L || i64 < -(1L << (int)(31L)))
                {
                    return (null, error.As(fmt.Errorf("sql/driver: value %d overflows int32", v))!);
                }

                return (i64, error.As(null!)!);
            else if (rv.Kind() == reflect.Uint || rv.Kind() == reflect.Uint8 || rv.Kind() == reflect.Uint16 || rv.Kind() == reflect.Uint32 || rv.Kind() == reflect.Uint64) 
                var u64 = rv.Uint();
                if (u64 > (1L << (int)(31L)) - 1L)
                {
                    return (null, error.As(fmt.Errorf("sql/driver: value %d overflows int32", v))!);
                }

                return (int64(u64), error.As(null!)!);
            else if (rv.Kind() == reflect.String) 
                var (i, err) = strconv.Atoi(rv.String());
                if (err != null)
                {
                    return (null, error.As(fmt.Errorf("sql/driver: value %q can't be converted to int32", v))!);
                }

                return (int64(i), error.As(null!)!);
                        return (null, error.As(fmt.Errorf("sql/driver: unsupported value %v (type %T) converting to int32", v, v))!);

        }

        // String is a ValueConverter that converts its input to a string.
        // If the value is already a string or []byte, it's unchanged.
        // If the value is of another type, conversion to string is done
        // with fmt.Sprintf("%v", v).
        public static stringType String = default;

        private partial struct stringType
        {
        }

        private static (Value, error) ConvertValue(this stringType _p0, object v)
        {
            Value _p0 = default;
            error _p0 = default!;

            switch (v.type())
            {
                case @string _:
                    return (v, error.As(null!)!);
                    break;
                case slice<byte> _:
                    return (v, error.As(null!)!);
                    break;
            }
            return (fmt.Sprintf("%v", v), error.As(null!)!);

        }

        // Null is a type that implements ValueConverter by allowing nil
        // values but otherwise delegating to another ValueConverter.
        public partial struct Null
        {
            public ValueConverter Converter;
        }

        public static (Value, error) ConvertValue(this Null n, object v)
        {
            Value _p0 = default;
            error _p0 = default!;

            if (v == null)
            {
                return (null, error.As(null!)!);
            }

            return n.Converter.ConvertValue(v);

        }

        // NotNull is a type that implements ValueConverter by disallowing nil
        // values but otherwise delegating to another ValueConverter.
        public partial struct NotNull
        {
            public ValueConverter Converter;
        }

        public static (Value, error) ConvertValue(this NotNull n, object v)
        {
            Value _p0 = default;
            error _p0 = default!;

            if (v == null)
            {
                return (null, error.As(fmt.Errorf("nil value not allowed"))!);
            }

            return n.Converter.ConvertValue(v);

        }

        // IsValue reports whether v is a valid Value parameter type.
        public static bool IsValue(object v)
        {
            if (v == null)
            {
                return true;
            }

            switch (v.type())
            {
                case slice<byte> _:
                    return true;
                    break;
                case bool _:
                    return true;
                    break;
                case double _:
                    return true;
                    break;
                case long _:
                    return true;
                    break;
                case @string _:
                    return true;
                    break;
                case time.Time _:
                    return true;
                    break;
                case decimalDecompose _:
                    return true;
                    break;
            }
            return false;

        }

        // IsScanValue is equivalent to IsValue.
        // It exists for compatibility.
        public static bool IsScanValue(object v)
        {
            return IsValue(v);
        }

        // DefaultParameterConverter is the default implementation of
        // ValueConverter that's used when a Stmt doesn't implement
        // ColumnConverter.
        //
        // DefaultParameterConverter returns its argument directly if
        // IsValue(arg). Otherwise, if the argument implements Valuer, its
        // Value method is used to return a Value. As a fallback, the provided
        // argument's underlying type is used to convert it to a Value:
        // underlying integer types are converted to int64, floats to float64,
        // bool, string, and []byte to themselves. If the argument is a nil
        // pointer, ConvertValue returns a nil Value. If the argument is a
        // non-nil pointer, it is dereferenced and ConvertValue is called
        // recursively. Other types are an error.
        public static defaultConverter DefaultParameterConverter = default;

        private partial struct defaultConverter
        {
        }

        private static ValueConverter _ = ValueConverter.As(new defaultConverter())!;

        private static var valuerReflectType = reflect.TypeOf((Valuer.val)(null)).Elem();

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
        // This function is mirrored in the database/sql package.
        private static (Value, error) callValuerValue(Valuer vr)
        {
            Value v = default;
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

        private static (Value, error) ConvertValue(this defaultConverter _p0, object v)
        {
            Value _p0 = default;
            error _p0 = default!;

            if (IsValue(v))
            {
                return (v, error.As(null!)!);
            }

            switch (v.type())
            {
                case Valuer vr:
                    var (sv, err) = callValuerValue(vr);
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    if (!IsValue(sv))
                    {
                        return (null, error.As(fmt.Errorf("non-Value type %T returned from Value", sv))!);
                    }

                    return (sv, error.As(null!)!); 

                    // For now, continue to prefer the Valuer interface over the decimal decompose interface.
                    break;
                case decimalDecompose vr:
                    return (vr, error.As(null!)!);
                    break;

            }

            var rv = reflect.ValueOf(v);

            if (rv.Kind() == reflect.Ptr) 
                // indirect pointers
                if (rv.IsNil())
                {
                    return (null, error.As(null!)!);
                }
                else
                {
                    return new defaultConverter().ConvertValue(rv.Elem().Interface());
                }

            else if (rv.Kind() == reflect.Int || rv.Kind() == reflect.Int8 || rv.Kind() == reflect.Int16 || rv.Kind() == reflect.Int32 || rv.Kind() == reflect.Int64) 
                return (rv.Int(), error.As(null!)!);
            else if (rv.Kind() == reflect.Uint || rv.Kind() == reflect.Uint8 || rv.Kind() == reflect.Uint16 || rv.Kind() == reflect.Uint32) 
                return (int64(rv.Uint()), error.As(null!)!);
            else if (rv.Kind() == reflect.Uint64) 
                var u64 = rv.Uint();
                if (u64 >= 1L << (int)(63L))
                {
                    return (null, error.As(fmt.Errorf("uint64 values with high bit set are not supported"))!);
                }

                return (int64(u64), error.As(null!)!);
            else if (rv.Kind() == reflect.Float32 || rv.Kind() == reflect.Float64) 
                return (rv.Float(), error.As(null!)!);
            else if (rv.Kind() == reflect.Bool) 
                return (rv.Bool(), error.As(null!)!);
            else if (rv.Kind() == reflect.Slice) 
                var ek = rv.Type().Elem().Kind();
                if (ek == reflect.Uint8)
                {
                    return (rv.Bytes(), error.As(null!)!);
                }

                return (null, error.As(fmt.Errorf("unsupported type %T, a slice of %s", v, ek))!);
            else if (rv.Kind() == reflect.String) 
                return (rv.String(), error.As(null!)!);
                        return (null, error.As(fmt.Errorf("unsupported type %T, a %s", v, rv.Kind()))!);

        }

        private partial interface decimalDecompose
        {
            (byte, bool, slice<byte>, int) Decompose(slice<byte> buf);
        }
    }
}}}
