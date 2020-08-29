// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements objsets.
//
// An objset is similar to a Scope but objset elements
// are identified by their unique id, instead of their
// object name.

// package types -- go2cs converted at 2020 August 29 08:47:46 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\objset.go

using static go.builtin;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // An objset is a set of objects identified by their unique id.
        // The zero value for objset is a ready-to-use empty objset.
        private partial struct objset // : map<@string, Object>
        {
        } // initialized lazily

        // insert attempts to insert an object obj into objset s.
        // If s already contains an alternative object alt with
        // the same name, insert leaves s unchanged and returns alt.
        // Otherwise it inserts obj and returns nil.
        private static Object insert(this ref objset s, Object obj)
        {
            var id = obj.Id();
            {
                var alt = (s.Value)[id];

                if (alt != null)
                {
                    return alt;
                }

            }
            if (s == null.Value)
            {
                s.Value = make_map<@string, Object>();
            }
            (s.Value)[id] = obj;
            return null;
        }
    }
}}
