// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using sync = sync_package;

partial class textproto_package {

// A Pipeline manages a pipelined in-order request/response sequence.
//
// To use a Pipeline p to manage multiple clients on a connection,
// each client should run:
//
//	id := p.Next()	// take a number
//
//	p.StartRequest(id)	// wait for turn to send request
//	«send request»
//	p.EndRequest(id)	// notify Pipeline that request is sent
//
//	p.StartResponse(id)	// wait for turn to read response
//	«read response»
//	p.EndResponse(id)	// notify Pipeline that response is read
//
// A pipelined server can use the same calls to ensure that
// responses computed in parallel are written in the correct order.
[GoType] partial struct Pipeline {
    internal sync_package.Mutex mu;
    internal nuint id;
    internal sequencer request;
    internal sequencer response;
}

// Next returns the next id for a request/response pair.
[GoRecv] public static nuint Next(this ref Pipeline p) {
    p.mu.Lock();
    nuint id = p.id;
    p.id++;
    p.mu.Unlock();
    return id;
}

// StartRequest blocks until it is time to send (or, if this is a server, receive)
// the request with the given id.
[GoRecv] public static void StartRequest(this ref Pipeline p, nuint id) {
    p.request.Start(id);
}

// EndRequest notifies p that the request with the given id has been sent
// (or, if this is a server, received).
[GoRecv] public static void EndRequest(this ref Pipeline p, nuint id) {
    p.request.End(id);
}

// StartResponse blocks until it is time to receive (or, if this is a server, send)
// the request with the given id.
[GoRecv] public static void StartResponse(this ref Pipeline p, nuint id) {
    p.response.Start(id);
}

// EndResponse notifies p that the response with the given id has been received
// (or, if this is a server, sent).
[GoRecv] public static void EndResponse(this ref Pipeline p, nuint id) {
    p.response.End(id);
}

// A sequencer schedules a sequence of numbered events that must
// happen in order, one after the other. The event numbering must start
// at 0 and increment without skipping. The event number wraps around
// safely as long as there are not 2^32 simultaneous events pending.
[GoType] partial struct sequencer {
    internal sync_package.Mutex mu;
    internal nuint id;
    internal map<nuint, channel<EmptyStruct>> wait;
}

// Start waits until it is time for the event numbered id to begin.
// That is, except for the first event, it waits until End(id-1) has
// been called.
[GoRecv] internal static void Start(this ref sequencer s, nuint id) {
    s.mu.Lock();
    if (s.id == id) {
        s.mu.Unlock();
        return;
    }
    var c = new channel<EmptyStruct>(1);
    if (s.wait == default!) {
        s.wait = new map<nuint, channel<EmptyStruct>>();
    }
    s.wait[id] = c;
    s.mu.Unlock();
    ᐸꟷ(c);
}

// End notifies the sequencer that the event numbered id has completed,
// allowing it to schedule the event numbered id+1.  It is a run-time error
// to call End with an id that is not the number of the active event.
[GoRecv] internal static void End(this ref sequencer s, nuint id) {
    s.mu.Lock();
    if (s.id != id) {
        s.mu.Unlock();
        throw panic("out of sync");
    }
    id++;
    s.id = id;
    if (s.wait == default!) {
        s.wait = new map<nuint, channel<EmptyStruct>>();
    }
    var c = s.wait[id];
    var ok = s.wait[id];
    if (ok) {
        delete(s.wait, id);
    }
    s.mu.Unlock();
    if (ok) {
        close(c);
    }
}

} // end textproto_package
