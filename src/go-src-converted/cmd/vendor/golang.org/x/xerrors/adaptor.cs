// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package xerrors -- go2cs converted at 2020 October 08 04:58:34 UTC
// import "cmd/vendor/golang.org/x/xerrors" ==> using xerrors = go.cmd.vendor.golang.org.x.xerrors_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\xerrors\adaptor.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using reflect = go.reflect_package;
using strconv = go.strconv_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x
{
    public static partial class xerrors_package
    {
        // FormatError calls the FormatError method of f with an errors.Printer
        // configured according to s and verb, and writes the result to s.
        public static void FormatError(Formatter f, fmt.State s, int verb)
        { 
            // Assuming this function is only called from the Format method, and given
            // that FormatError takes precedence over Format, it cannot be called from
            // any package that supports errors.Formatter. It is therefore safe to
            // disregard that State may be a specific printer implementation and use one
            // of our choice instead.

            // limitations: does not support printing error as Go struct.

            @string sep = " ";            ptr<state> p = addr(new state(State:s));            var direct = true;

            error err = error.As(f)!;

            switch (verb)
            { 
            // Note that this switch must match the preference order
            // for ordinary string printing (%#v before %+v, and so on).

                case 'v': 
                    if (s.Flag('#'))
                    {
                        {
                            fmt.GoStringer (stringer, ok) = err._<fmt.GoStringer>();

                            if (ok)
                            {
                                io.WriteString(_addr_p.buf, stringer.GoString());
                                goto exit;
                            }
                        } 
                        // proceed as if it were %v
                    }
                    else if (s.Flag('+'))
                    {
                        p.printDetail = true;
                        sep = "\n  - ";
                    }
                    break;
                case 's': 
                    break;
                case 'q': 
                    // Use an intermediate buffer in the rare cases that precision,
                    // truncation, or one of the alternative verbs (q, x, and X) are
                    // specified.

                case 'x': 
                    // Use an intermediate buffer in the rare cases that precision,
                    // truncation, or one of the alternative verbs (q, x, and X) are
                    // specified.

                case 'X': 
                    // Use an intermediate buffer in the rare cases that precision,
                    // truncation, or one of the alternative verbs (q, x, and X) are
                    // specified.
                    direct = false;
                    break;
                default: 
                    p.buf.WriteString("%!");
                    p.buf.WriteRune(verb);
                    p.buf.WriteByte('(');

                    if (err != null) 
                        p.buf.WriteString(reflect.TypeOf(f).String());
                    else 
                        p.buf.WriteString("<nil>");
                                    p.buf.WriteByte(')');
                    io.Copy(s, _addr_p.buf);
                    return ;
                    break;
            }

loop:

            while (true)
            {
                switch (err.type())
                {
                    case Formatter v:
                        err = error.As(v.FormatError((printer.val)(p)))!;
                        break;
                    case fmt.Formatter v:
                        v.Format(p, 'v');
                        _breakloop = true;
                        break;
                        break;
                    default:
                    {
                        var v = err.type();
                        io.WriteString(_addr_p.buf, v.Error());
                        _breakloop = true;
                        break;
                        break;
                    }
                }
                if (err == null)
                {
                    break;
                }
                if (p.needColon || !p.printDetail)
                {
                    p.buf.WriteByte(':');
                    p.needColon = false;
                }
                p.buf.WriteString(sep);
                p.inDetail = false;
                p.needNewline = false;

            }
exit:
            var (width, okW) = s.Width();
            var (prec, okP) = s.Precision();

            if (!direct || (okW && width > 0L) || okP)
            { 
                // Construct format string from State s.
                byte format = new slice<byte>(new byte[] { '%' });
                if (s.Flag('-'))
                {
                    format = append(format, '-');
                }
                if (s.Flag('+'))
                {
                    format = append(format, '+');
                }
                if (s.Flag(' '))
                {
                    format = append(format, ' ');
                }
                if (okW)
                {
                    format = strconv.AppendInt(format, int64(width), 10L);
                }
                if (okP)
                {
                    format = append(format, '.');
                    format = strconv.AppendInt(format, int64(prec), 10L);
                }
                format = append(format, string(verb));
                fmt.Fprintf(s, string(format), p.buf.String());

            }
            else
            {
                io.Copy(s, _addr_p.buf);
            }
        }

        private static slice<byte> detailSep = (slice<byte>)"\n    ";

        // state tracks error printing state. It implements fmt.State.
        private partial struct state : fmt.State
        {
            public ref fmt.State State => ref State_val;
            public bytes.Buffer buf;
            public bool printDetail;
            public bool inDetail;
            public bool needColon;
            public bool needNewline;
        }

        private static (long, error) Write(this ptr<state> _addr_s, slice<byte> b)
        {
            long n = default;
            error err = default!;
            ref state s = ref _addr_s.val;

            if (s.printDetail)
            {
                if (len(b) == 0L)
                {
                    return (0L, error.As(null!)!);
                }

                if (s.inDetail && s.needColon)
                {
                    s.needNewline = true;
                    if (b[0L] == '\n')
                    {
                        b = b[1L..];
                    }

                }

                long k = 0L;
                foreach (var (i, c) in b)
                {
                    if (s.needNewline)
                    {
                        if (s.inDetail && s.needColon)
                        {
                            s.buf.WriteByte(':');
                            s.needColon = false;
                        }

                        s.buf.Write(detailSep);
                        s.needNewline = false;

                    }

                    if (c == '\n')
                    {
                        s.buf.Write(b[k..i]);
                        k = i + 1L;
                        s.needNewline = true;
                    }

                }
                s.buf.Write(b[k..]);
                if (!s.inDetail)
                {
                    s.needColon = true;
                }

            }
            else if (!s.inDetail)
            {
                s.buf.Write(b);
            }

            return (len(b), error.As(null!)!);

        }

        // printer wraps a state to implement an xerrors.Printer.
        private partial struct printer // : state
        {
        }

        private static void Print(this ptr<printer> _addr_s, params object[] args)
        {
            args = args.Clone();
            ref printer s = ref _addr_s.val;

            if (!s.inDetail || s.printDetail)
            {
                fmt.Fprint((state.val)(s), args);
            }

        }

        private static void Printf(this ptr<printer> _addr_s, @string format, params object[] args)
        {
            args = args.Clone();
            ref printer s = ref _addr_s.val;

            if (!s.inDetail || s.printDetail)
            {
                fmt.Fprintf((state.val)(s), format, args);
            }

        }

        private static bool Detail(this ptr<printer> _addr_s)
        {
            ref printer s = ref _addr_s.val;

            s.inDetail = true;
            return s.printDetail;
        }
    }
}}}}}
