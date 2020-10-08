// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package textproto implements generic support for text-based request/response
// protocols in the style of HTTP, NNTP, and SMTP.
//
// The package provides:
//
// Error, which represents a numeric error response from
// a server.
//
// Pipeline, to manage pipelined requests and responses
// in a client.
//
// Reader, to read numeric response code lines,
// key: value headers, lines wrapped with leading spaces
// on continuation lines, and whole text blocks ending
// with a dot on a line by itself.
//
// Writer, to write dot-encoded text blocks.
//
// Conn, a convenient packaging of Reader, Writer, and Pipeline for use
// with a single network connection.
//
// package textproto -- go2cs converted at 2020 October 08 03:38:29 UTC
// import "net/textproto" ==> using textproto = go.net.textproto_package
// Original source: C:\Go\src\net\textproto\textproto.go
using bufio = go.bufio_package;
using fmt = go.fmt_package;
using io = go.io_package;
using net = go.net_package;
using static go.builtin;

namespace go {
namespace net
{
    public static partial class textproto_package
    {
        // An Error represents a numeric error response from a server.
        public partial struct Error
        {
            public long Code;
            public @string Msg;
        }

        private static @string Error(this ptr<Error> _addr_e)
        {
            ref Error e = ref _addr_e.val;

            return fmt.Sprintf("%03d %s", e.Code, e.Msg);
        }

        // A ProtocolError describes a protocol violation such
        // as an invalid response or a hung-up connection.
        public partial struct ProtocolError // : @string
        {
        }

        public static @string Error(this ProtocolError p)
        {
            return string(p);
        }

        // A Conn represents a textual network protocol connection.
        // It consists of a Reader and Writer to manage I/O
        // and a Pipeline to sequence concurrent requests on the connection.
        // These embedded types carry methods with them;
        // see the documentation of those types for details.
        public partial struct Conn : Reader, Writer
        {
            public Reader Reader;
            public Writer Writer;
            public ref Pipeline Pipeline => ref Pipeline_val;
            public io.ReadWriteCloser conn;
        }

        // NewConn returns a new Conn using conn for I/O.
        public static ptr<Conn> NewConn(io.ReadWriteCloser conn)
        {
            return addr(new Conn(Reader:Reader{R:bufio.NewReader(conn)},Writer:Writer{W:bufio.NewWriter(conn)},conn:conn,));
        }

        // Close closes the connection.
        private static error Close(this ptr<Conn> _addr_c)
        {
            ref Conn c = ref _addr_c.val;

            return error.As(c.conn.Close())!;
        }

        // Dial connects to the given address on the given network using net.Dial
        // and then returns a new Conn for the connection.
        public static (ptr<Conn>, error) Dial(@string network, @string addr)
        {
            ptr<Conn> _p0 = default!;
            error _p0 = default!;

            var (c, err) = net.Dial(network, addr);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (_addr_NewConn(c)!, error.As(null!)!);

        }

        // Cmd is a convenience method that sends a command after
        // waiting its turn in the pipeline. The command text is the
        // result of formatting format with args and appending \r\n.
        // Cmd returns the id of the command, for use with StartResponse and EndResponse.
        //
        // For example, a client might run a HELP command that returns a dot-body
        // by using:
        //
        //    id, err := c.Cmd("HELP")
        //    if err != nil {
        //        return nil, err
        //    }
        //
        //    c.StartResponse(id)
        //    defer c.EndResponse(id)
        //
        //    if _, _, err = c.ReadCodeLine(110); err != nil {
        //        return nil, err
        //    }
        //    text, err := c.ReadDotBytes()
        //    if err != nil {
        //        return nil, err
        //    }
        //    return c.ReadCodeLine(250)
        //
        private static (ulong, error) Cmd(this ptr<Conn> _addr_c, @string format, params object[] args)
        {
            ulong id = default;
            error err = default!;
            args = args.Clone();
            ref Conn c = ref _addr_c.val;

            id = c.Next();
            c.StartRequest(id);
            err = c.PrintfLine(format, args);
            c.EndRequest(id);
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            return (id, error.As(null!)!);

        }

        // TrimString returns s without leading and trailing ASCII space.
        public static @string TrimString(@string s)
        {
            while (len(s) > 0L && isASCIISpace(s[0L]))
            {
                s = s[1L..];
            }

            while (len(s) > 0L && isASCIISpace(s[len(s) - 1L]))
            {
                s = s[..len(s) - 1L];
            }

            return s;

        }

        // TrimBytes returns b without leading and trailing ASCII space.
        public static slice<byte> TrimBytes(slice<byte> b)
        {
            while (len(b) > 0L && isASCIISpace(b[0L]))
            {
                b = b[1L..];
            }

            while (len(b) > 0L && isASCIISpace(b[len(b) - 1L]))
            {
                b = b[..len(b) - 1L];
            }

            return b;

        }

        private static bool isASCIISpace(byte b)
        {
            return b == ' ' || b == '\t' || b == '\n' || b == '\r';
        }

        private static bool isASCIILetter(byte b)
        {
            b |= 0x20UL; // make lower case
            return 'a' <= b && b <= 'z';

        }
    }
}}
