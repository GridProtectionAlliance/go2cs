// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package textproto -- go2cs converted at 2022 March 13 05:36:17 UTC
// import "net/textproto" ==> using textproto = go.net.textproto_package
// Original source: C:\Program Files\Go\src\net\textproto\header.go
namespace go.net;

public static partial class textproto_package {

// A MIMEHeader represents a MIME-style header mapping
// keys to sets of values.
public partial struct MIMEHeader { // : map<@string, slice<@string>>
}

// Add adds the key, value pair to the header.
// It appends to any existing values associated with key.
public static void Add(this MIMEHeader h, @string key, @string value) {
    key = CanonicalMIMEHeaderKey(key);
    h[key] = append(h[key], value);
}

// Set sets the header entries associated with key to
// the single element value. It replaces any existing
// values associated with key.
public static void Set(this MIMEHeader h, @string key, @string value) {
    h[CanonicalMIMEHeaderKey(key)] = new slice<@string>(new @string[] { value });
}

// Get gets the first value associated with the given key.
// It is case insensitive; CanonicalMIMEHeaderKey is used
// to canonicalize the provided key.
// If there are no values associated with the key, Get returns "".
// To use non-canonical keys, access the map directly.
public static @string Get(this MIMEHeader h, @string key) {
    if (h == null) {
        return "";
    }
    var v = h[CanonicalMIMEHeaderKey(key)];
    if (len(v) == 0) {
        return "";
    }
    return v[0];
}

// Values returns all values associated with the given key.
// It is case insensitive; CanonicalMIMEHeaderKey is
// used to canonicalize the provided key. To use non-canonical
// keys, access the map directly.
// The returned slice is not a copy.
public static slice<@string> Values(this MIMEHeader h, @string key) {
    if (h == null) {
        return null;
    }
    return h[CanonicalMIMEHeaderKey(key)];
}

// Del deletes the values associated with key.
public static void Del(this MIMEHeader h, @string key) {
    delete(h, CanonicalMIMEHeaderKey(key));
}

} // end textproto_package
