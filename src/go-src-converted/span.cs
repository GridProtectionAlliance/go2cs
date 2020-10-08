// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package span contains support for representing with positions and ranges in
// text files.
// package span -- go2cs converted at 2020 October 08 04:54:37 UTC
// import "golang.org/x/tools/internal/span" ==> using span = go.golang.org.x.tools.@internal.span_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\span\span.go
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using path = go.path_package;
using static go.builtin;
using System.ComponentModel;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal
{
    public static partial class span_package
    {
        // Span represents a source code range in standardized form.
        public partial struct Span
        {
            public span v;
        }

        // Point represents a single point within a file.
        // In general this should only be used as part of a Span, as on its own it
        // does not carry enough information.
        public partial struct Point
        {
            public point v;
        }

        private partial struct span
        {
            [Description("json:\"uri\"")]
            public URI URI;
            [Description("json:\"start\"")]
            public point Start;
            [Description("json:\"end\"")]
            public point End;
        }

        private partial struct point
        {
            [Description("json:\"line\"")]
            public long Line;
            [Description("json:\"column\"")]
            public long Column;
            [Description("json:\"offset\"")]
            public long Offset;
        }

        // Invalid is a span that reports false from IsValid
        public static Span Invalid = new Span(v:span{Start:invalidPoint.v,End:invalidPoint.v});

        private static Point invalidPoint = new Point(v:point{Line:0,Column:0,Offset:-1});

        // Converter is the interface to an object that can convert between line:column
        // and offset forms for a single file.
        public partial interface Converter
        {
            (long, error) ToPosition(long offset); //ToOffset converts from a line:column pair to an offset.
            (long, error) ToOffset(long line, long col);
        }

        public static Span New(URI uri, Point start, Point end)
        {
            Span s = new Span(v:span{URI:uri,Start:start.v,End:end.v});
            s.v.clean();
            return s;
        }

        public static Point NewPoint(long line, long col, long offset)
        {
            Point p = new Point(v:point{Line:line,Column:col,Offset:offset});
            p.v.clean();
            return p;
        }

        public static long Compare(Span a, Span b)
        {
            {
                var r__prev1 = r;

                var r = CompareURI(a.URI(), b.URI());

                if (r != 0L)
                {
                    return r;
                }

                r = r__prev1;

            }

            {
                var r__prev1 = r;

                r = comparePoint(a.v.Start, b.v.Start);

                if (r != 0L)
                {
                    return r;
                }

                r = r__prev1;

            }

            return comparePoint(a.v.End, b.v.End);

        }

        public static long ComparePoint(Point a, Point b)
        {
            return comparePoint(a.v, b.v);
        }

        private static long comparePoint(point a, point b)
        {
            if (!a.hasPosition())
            {
                if (a.Offset < b.Offset)
                {
                    return -1L;
                }

                if (a.Offset > b.Offset)
                {
                    return 1L;
                }

                return 0L;

            }

            if (a.Line < b.Line)
            {
                return -1L;
            }

            if (a.Line > b.Line)
            {
                return 1L;
            }

            if (a.Column < b.Column)
            {
                return -1L;
            }

            if (a.Column > b.Column)
            {
                return 1L;
            }

            return 0L;

        }

        public static bool HasPosition(this Span s)
        {
            return s.v.Start.hasPosition();
        }
        public static bool HasOffset(this Span s)
        {
            return s.v.Start.hasOffset();
        }
        public static bool IsValid(this Span s)
        {
            return s.v.Start.isValid();
        }
        public static bool IsPoint(this Span s)
        {
            return s.v.Start == s.v.End;
        }
        public static URI URI(this Span s)
        {
            return s.v.URI;
        }
        public static Point Start(this Span s)
        {
            return new Point(s.v.Start);
        }
        public static Point End(this Span s)
        {
            return new Point(s.v.End);
        }
        private static (slice<byte>, error) MarshalJSON(this ptr<Span> _addr_s)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref Span s = ref _addr_s.val;

            return json.Marshal(_addr_s.v);
        }
        private static error UnmarshalJSON(this ptr<Span> _addr_s, slice<byte> b)
        {
            ref Span s = ref _addr_s.val;

            return error.As(json.Unmarshal(b, _addr_s.v))!;
        }

        public static bool HasPosition(this Point p)
        {
            return p.v.hasPosition();
        }
        public static bool HasOffset(this Point p)
        {
            return p.v.hasOffset();
        }
        public static bool IsValid(this Point p)
        {
            return p.v.isValid();
        }
        private static (slice<byte>, error) MarshalJSON(this ptr<Point> _addr_p)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref Point p = ref _addr_p.val;

            return json.Marshal(_addr_p.v);
        }
        private static error UnmarshalJSON(this ptr<Point> _addr_p, slice<byte> b)
        {
            ref Point p = ref _addr_p.val;

            return error.As(json.Unmarshal(b, _addr_p.v))!;
        }
        public static long Line(this Point p) => func((_, panic, __) =>
        {
            if (!p.v.hasPosition())
            {
                panic(fmt.Errorf("position not set in %v", p.v));
            }

            return p.v.Line;

        });
        public static long Column(this Point p) => func((_, panic, __) =>
        {
            if (!p.v.hasPosition())
            {
                panic(fmt.Errorf("position not set in %v", p.v));
            }

            return p.v.Column;

        });
        public static long Offset(this Point p) => func((_, panic, __) =>
        {
            if (!p.v.hasOffset())
            {
                panic(fmt.Errorf("offset not set in %v", p.v));
            }

            return p.v.Offset;

        });

        private static bool hasPosition(this point p)
        {
            return p.Line > 0L;
        }
        private static bool hasOffset(this point p)
        {
            return p.Offset >= 0L;
        }
        private static bool isValid(this point p)
        {
            return p.hasPosition() || p.hasOffset();
        }
        private static bool isZero(this point p)
        {
            return (p.Line == 1L && p.Column == 1L) || (!p.hasPosition() && p.Offset == 0L);
        }

        private static void clean(this ptr<span> _addr_s)
        {
            ref span s = ref _addr_s.val;
 
            //this presumes the points are already clean
            if (!s.End.isValid() || (s.End == new point()))
            {
                s.End = s.Start;
            }

        }

        private static void clean(this ptr<point> _addr_p)
        {
            ref point p = ref _addr_p.val;

            if (p.Line < 0L)
            {
                p.Line = 0L;
            }

            if (p.Column <= 0L)
            {
                if (p.Line > 0L)
                {
                    p.Column = 1L;
                }
                else
                {
                    p.Column = 0L;
                }

            }

            if (p.Offset == 0L && (p.Line > 1L || p.Column > 1L))
            {
                p.Offset = -1L;
            }

        }

        // Format implements fmt.Formatter to print the Location in a standard form.
        // The format produced is one that can be read back in using Parse.
        public static void Format(this Span s, fmt.State f, int c)
        {
            var fullForm = f.Flag('+');
            var preferOffset = f.Flag('#'); 
            // we should always have a uri, simplify if it is file format
            //TODO: make sure the end of the uri is unambiguous
            var uri = string(s.v.URI);
            if (c == 'f')
            {
                uri = path.Base(uri);
            }
            else if (!fullForm)
            {
                uri = s.v.URI.Filename();
            }

            fmt.Fprint(f, uri);
            if (!s.IsValid() || (!fullForm && s.v.Start.isZero() && s.v.End.isZero()))
            {
                return ;
            } 
            // see which bits of start to write
            var printOffset = s.HasOffset() && (fullForm || preferOffset || !s.HasPosition());
            var printLine = s.HasPosition() && (fullForm || !printOffset);
            var printColumn = printLine && (fullForm || (s.v.Start.Column > 1L || s.v.End.Column > 1L));
            fmt.Fprint(f, ":");
            if (printLine)
            {
                fmt.Fprintf(f, "%d", s.v.Start.Line);
            }

            if (printColumn)
            {
                fmt.Fprintf(f, ":%d", s.v.Start.Column);
            }

            if (printOffset)
            {
                fmt.Fprintf(f, "#%d", s.v.Start.Offset);
            } 
            // start is written, do we need end?
            if (s.IsPoint())
            {
                return ;
            } 
            // we don't print the line if it did not change
            printLine = fullForm || (printLine && s.v.End.Line > s.v.Start.Line);
            fmt.Fprint(f, "-");
            if (printLine)
            {
                fmt.Fprintf(f, "%d", s.v.End.Line);
            }

            if (printColumn)
            {
                if (printLine)
                {
                    fmt.Fprint(f, ":");
                }

                fmt.Fprintf(f, "%d", s.v.End.Column);

            }

            if (printOffset)
            {
                fmt.Fprintf(f, "#%d", s.v.End.Offset);
            }

        }

        public static (Span, error) WithPosition(this Span s, Converter c)
        {
            Span _p0 = default;
            error _p0 = default!;

            {
                var err = s.update(c, true, false);

                if (err != null)
                {
                    return (new Span(), error.As(err)!);
                }

            }

            return (s, error.As(null!)!);

        }

        public static (Span, error) WithOffset(this Span s, Converter c)
        {
            Span _p0 = default;
            error _p0 = default!;

            {
                var err = s.update(c, false, true);

                if (err != null)
                {
                    return (new Span(), error.As(err)!);
                }

            }

            return (s, error.As(null!)!);

        }

        public static (Span, error) WithAll(this Span s, Converter c)
        {
            Span _p0 = default;
            error _p0 = default!;

            {
                var err = s.update(c, true, true);

                if (err != null)
                {
                    return (new Span(), error.As(err)!);
                }

            }

            return (s, error.As(null!)!);

        }

        private static error update(this ptr<Span> _addr_s, Converter c, bool withPos, bool withOffset)
        {
            ref Span s = ref _addr_s.val;

            if (!s.IsValid())
            {
                return error.As(fmt.Errorf("cannot add information to an invalid span"))!;
            }

            if (withPos && !s.HasPosition())
            {
                {
                    var err__prev2 = err;

                    var err = s.v.Start.updatePosition(c);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

                if (s.v.End.Offset == s.v.Start.Offset)
                {
                    s.v.End = s.v.Start;
                }                {
                    var err__prev3 = err;

                    err = s.v.End.updatePosition(c);


                    else if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev3;

                }

            }

            if (withOffset && (!s.HasOffset() || (s.v.End.hasPosition() && !s.v.End.hasOffset())))
            {
                {
                    var err__prev2 = err;

                    err = s.v.Start.updateOffset(c);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

                if (s.v.End.Line == s.v.Start.Line && s.v.End.Column == s.v.Start.Column)
                {
                    s.v.End.Offset = s.v.Start.Offset;
                }                {
                    var err__prev3 = err;

                    err = s.v.End.updateOffset(c);


                    else if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev3;

                }

            }

            return error.As(null!)!;

        }

        private static error updatePosition(this ptr<point> _addr_p, Converter c)
        {
            ref point p = ref _addr_p.val;

            var (line, col, err) = c.ToPosition(p.Offset);
            if (err != null)
            {
                return error.As(err)!;
            }

            p.Line = line;
            p.Column = col;
            return error.As(null!)!;

        }

        private static error updateOffset(this ptr<point> _addr_p, Converter c)
        {
            ref point p = ref _addr_p.val;

            var (offset, err) = c.ToOffset(p.Line, p.Column);
            if (err != null)
            {
                return error.As(err)!;
            }

            p.Offset = offset;
            return error.As(null!)!;

        }
    }
}}}}}
