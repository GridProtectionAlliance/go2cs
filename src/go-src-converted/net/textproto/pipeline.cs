// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package textproto -- go2cs converted at 2020 October 08 03:38:27 UTC
// import "net/textproto" ==> using textproto = go.net.textproto_package
// Original source: C:\Go\src\net\textproto\pipeline.go
using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace net
{
    public static partial class textproto_package
    {
        // A Pipeline manages a pipelined in-order request/response sequence.
        //
        // To use a Pipeline p to manage multiple clients on a connection,
        // each client should run:
        //
        //    id := p.Next()    // take a number
        //
        //    p.StartRequest(id)    // wait for turn to send request
        //    «send request»
        //    p.EndRequest(id)    // notify Pipeline that request is sent
        //
        //    p.StartResponse(id)    // wait for turn to read response
        //    «read response»
        //    p.EndResponse(id)    // notify Pipeline that response is read
        //
        // A pipelined server can use the same calls to ensure that
        // responses computed in parallel are written in the correct order.
        public partial struct Pipeline
        {
            public sync.Mutex mu;
            public ulong id;
            public sequencer request;
            public sequencer response;
        }

        // Next returns the next id for a request/response pair.
        private static ulong Next(this ptr<Pipeline> _addr_p)
        {
            ref Pipeline p = ref _addr_p.val;

            p.mu.Lock();
            var id = p.id;
            p.id++;
            p.mu.Unlock();
            return id;
        }

        // StartRequest blocks until it is time to send (or, if this is a server, receive)
        // the request with the given id.
        private static void StartRequest(this ptr<Pipeline> _addr_p, ulong id)
        {
            ref Pipeline p = ref _addr_p.val;

            p.request.Start(id);
        }

        // EndRequest notifies p that the request with the given id has been sent
        // (or, if this is a server, received).
        private static void EndRequest(this ptr<Pipeline> _addr_p, ulong id)
        {
            ref Pipeline p = ref _addr_p.val;

            p.request.End(id);
        }

        // StartResponse blocks until it is time to receive (or, if this is a server, send)
        // the request with the given id.
        private static void StartResponse(this ptr<Pipeline> _addr_p, ulong id)
        {
            ref Pipeline p = ref _addr_p.val;

            p.response.Start(id);
        }

        // EndResponse notifies p that the response with the given id has been received
        // (or, if this is a server, sent).
        private static void EndResponse(this ptr<Pipeline> _addr_p, ulong id)
        {
            ref Pipeline p = ref _addr_p.val;

            p.response.End(id);
        }

        // A sequencer schedules a sequence of numbered events that must
        // happen in order, one after the other. The event numbering must start
        // at 0 and increment without skipping. The event number wraps around
        // safely as long as there are not 2^32 simultaneous events pending.
        private partial struct sequencer
        {
            public sync.Mutex mu;
            public ulong id;
            public map<ulong, channel<object>> wait;
        }

        // Start waits until it is time for the event numbered id to begin.
        // That is, except for the first event, it waits until End(id-1) has
        // been called.
        private static void Start(this ptr<sequencer> _addr_s, ulong id)
        {
            ref sequencer s = ref _addr_s.val;

            s.mu.Lock();
            if (s.id == id)
            {
                s.mu.Unlock();
                return ;
            }

            var c = make_channel<object>();
            if (s.wait == null)
            {
                s.wait = make_map<ulong, channel<object>>();
            }

            s.wait[id] = c;
            s.mu.Unlock().Send(c);

        }

        // End notifies the sequencer that the event numbered id has completed,
        // allowing it to schedule the event numbered id+1.  It is a run-time error
        // to call End with an id that is not the number of the active event.
        private static void End(this ptr<sequencer> _addr_s, ulong id) => func((_, panic, __) =>
        {
            ref sequencer s = ref _addr_s.val;

            s.mu.Lock();
            if (s.id != id)
            {
                s.mu.Unlock();
                panic("out of sync");
            }

            id++;
            s.id = id;
            if (s.wait == null)
            {
                s.wait = make_map<ulong, channel<object>>();
            }

            var (c, ok) = s.wait[id];
            if (ok)
            {
                delete(s.wait, id);
            }

            s.mu.Unlock();
            if (ok)
            {
                close(c);
            }

        });
    }
}}
