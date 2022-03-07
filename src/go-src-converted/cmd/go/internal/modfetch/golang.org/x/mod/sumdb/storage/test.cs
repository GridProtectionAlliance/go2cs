// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package storage -- go2cs converted at 2022 March 06 23:19:11 UTC
// import "golang.org/x/mod/sumdb/storage" ==> using storage = go.golang.org.x.mod.sumdb.storage_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\mod\sumdb\storage\test.go
using context = go.context_package;
using fmt = go.fmt_package;
using io = go.io_package;
using testing = go.testing_package;
using System;


namespace go.golang.org.x.mod.sumdb;

public static partial class storage_package {

    // TestStorage tests a Storage implementation.
public static void TestStorage(ptr<testing.T> _addr_t, context.Context ctx, Storage storage) {
    ref testing.T t = ref _addr_t.val;

    var s = storage; 

    // Insert records.
    err = s.ReadWrite(ctx, (ctx, tx) => {
        {
            nint i__prev1 = i;

            for (nint i = 0; i < 10; i++) {
                var err = tx.BufferWrites(new slice<Write>(new Write[] { {Key:fmt.Sprint(i),Value:fmt.Sprint(-i)}, {Key:fmt.Sprint(1000+i),Value:fmt.Sprint(-1000-i)} }));
                if (err != null) {
                    t.Fatal(err);
                }
            }

            i = i__prev1;
        }
        return null;
    });
    if (err != null) {
        t.Fatal(err);
    }
    Action testRead = () => {
        err = s.ReadOnly(ctx, (ctx, tx) => {
            {
                nint i__prev1 = i;

                for (i = int64(0); i < 1010; i++) {
                    if (i == 10) {
                        i = 1000;
                    }
                    var (val, err) = tx.ReadValue(ctx, fmt.Sprint(i));
                    if (err != null) {
                        t.Fatalf("reading %v: %v", i, err);
                    }
                    {
                        var want = fmt.Sprint(-i);

                        if (val != want) {
                            t.Fatalf("ReadValue %v = %q, want %v", i, val, want);
                        }
                    }
                }

                i = i__prev1;
            }
            return null;
        });
        if (err != null) {
            t.Fatal(err);
        }
    };
    testRead(); 

    // Buffered writes in failed transaction should not be applied.
    err = s.ReadWrite(ctx, (ctx, tx) => {
        tx.BufferWrites(new slice<Write>(new Write[] { {Key:fmt.Sprint(0),Value:""}, {Key:fmt.Sprint(1),Value:"overwrite"} }));
        if (err != null) {
            t.Fatal(err);
        }
        return io.ErrUnexpectedEOF;
    });
    if (err != io.ErrUnexpectedEOF) {
        t.Fatalf("ReadWrite returned %v, want ErrUnexpectedEOF", err);
    }
    testRead();
}

} // end storage_package
