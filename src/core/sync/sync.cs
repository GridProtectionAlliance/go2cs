// sync.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Threading;

namespace go;

public static class sync_package
{
    public struct Mutex {
        private object _lock;
        public object Lock => _lock ??= new object();
    }

    public static void Lock(this ref Mutex mutex) => Monitor.Enter(mutex.Lock);

    public static void Unlock(this ref Mutex mutex) => Monitor.Exit(mutex.Lock);
}
