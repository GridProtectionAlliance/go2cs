//******************************************************************************************************
//  sync_package.cs - Gbtc
//
//  Copyright © 2026, J. Ritchie Carroll.  All Rights Reserved.
//
//  Licensed under the MIT License (MIT), the "License"; you may not use this file except in compliance
//  with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/30/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
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
