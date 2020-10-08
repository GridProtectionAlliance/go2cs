// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package export -- go2cs converted at 2020 October 08 04:54:55 UTC
// import "golang.org/x/tools/internal/event/export" ==> using export = go.golang.org.x.tools.@internal.@event.export_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\export\printer.go
using io = go.io_package;

using core = go.golang.org.x.tools.@internal.@event.core_package;
using keys = go.golang.org.x.tools.@internal.@event.keys_package;
using label = go.golang.org.x.tools.@internal.@event.label_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal {
namespace @event
{
    public static partial class export_package
    {
        public partial struct Printer
        {
            public array<byte> buffer;
        }

        private static void WriteEvent(this ptr<Printer> _addr_p, io.Writer w, core.Event ev, label.Map lm)
        {
            ref Printer p = ref _addr_p.val;

            var buf = p.buffer[..0L];
            if (!ev.At().IsZero())
            {
                w.Write(ev.At().AppendFormat(buf, "2006/01/02 15:04:05 "));
            }

            var msg = keys.Msg.Get(lm);
            io.WriteString(w, msg);
            {
                var err = keys.Err.Get(lm);

                if (err != null)
                {
                    if (msg != "")
                    {
                        io.WriteString(w, ": ");
                    }

                    io.WriteString(w, err.Error());

                }

            }

            for (long index = 0L; ev.Valid(index); index++)
            {
                var l = ev.Label(index);
                if (!l.Valid() || l.Key() == keys.Msg || l.Key() == keys.Err)
                {
                    continue;
                }

                io.WriteString(w, "\n\t");
                io.WriteString(w, l.Key().Name());
                io.WriteString(w, "=");
                l.Key().Format(w, buf, l);

            }

            io.WriteString(w, "\n");

        }
    }
}}}}}}
