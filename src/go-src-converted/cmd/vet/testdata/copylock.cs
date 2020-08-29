// package testdata -- go2cs converted at 2020 August 29 10:10:31 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\copylock.go
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using @unsafe = go.@unsafe_package;
using @unsafe = go.@unsafe_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        public static void OkFunc()
        {
            ref sync.Mutex x = default;
            var p = x;
            sync.Mutex y = default;
            p = ref y;

            sync.Mutex z = new sync.Mutex();
            sync.Mutex w = new sync.Mutex();

            w = new sync.Mutex();
            struct{Lsync.Mutex} q = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{Lsync.Mutex}{L:sync.Mutex{},};

            Tlock yy = new slice<Tlock>(new Tlock[] { Tlock{}, Tlock{once:sync.Once{},} });

            ptr<sync.Mutex> nl = @new<sync.Mutex>();
            var mx = make_slice<sync.Mutex>(10L);
            struct{L*sync.Mutex} xx = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{L*sync.Mutex}{L:new(sync.Mutex),};
        }

        public partial struct Tlock
        {
            public sync.Once once;
        }

        public static void BadFunc()
        {
            ref sync.Mutex x = default;
            var p = x;
            sync.Mutex y = default;
            p = ref y;
            p.Value = x.Value; // ERROR "assignment copies lock value to \*p: sync.Mutex"

            Tlock t = default;
            ref Tlock tp = default;
            tp = ref t;
            tp.Value = t; // ERROR "assignment copies lock value to \*tp: testdata.Tlock contains sync.Once contains sync.Mutex"
            t = tp.Value; // ERROR "assignment copies lock value to t: testdata.Tlock contains sync.Once contains sync.Mutex"

            y = x.Value; // ERROR "assignment copies lock value to y: sync.Mutex"
            var z = t; // ERROR "variable declaration copies lock value to z: testdata.Tlock contains sync.Once contains sync.Mutex"

            struct{Lsync.Mutex} w = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{Lsync.Mutex}{L:*x,};
            map q = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<long, Tlock>{1:t,2:*tp,};
            Tlock yy = new slice<Tlock>(new Tlock[] { t, *tp }); 

            // override 'new' keyword
            Action<object> @new = _p0 =>
            {
            }
;
            @new<t>(); // ERROR "call of new copies lock value: testdata.Tlock contains sync.Once contains sync.Mutex"

            // copy of array of locks
            array<sync.Mutex> muA = new array<sync.Mutex>(5L);
            var muB = muA; // ERROR "assignment copies lock value to muB: sync.Mutex"
            muA = muB; // ERROR "assignment copies lock value to muA: sync.Mutex"
            var muSlice = muA[..]; // OK

            // multidimensional array
            array<array<sync.Mutex>> mmuA = new array<array<sync.Mutex>>(5L);
            var mmuB = mmuA; // ERROR "assignment copies lock value to mmuB: sync.Mutex"
            mmuA = mmuB; // ERROR "assignment copies lock value to mmuA: sync.Mutex"
            var mmuSlice = mmuA[..]; // OK

            // slice copy is ok
            array<slice<array<sync.Mutex>>> fmuA = new array<slice<array<sync.Mutex>>>(5L);
            var fmuB = fmuA; // OK
            fmuA = fmuB; // OK
            var fmuSlice = fmuA[..]; // OK
        }

        public static void LenAndCapOnLockArrays()
        {
            array<sync.Mutex> a = new array<sync.Mutex>(5L);
            var aLen = len(a); // OK
            var aCap = cap(a); // OK

            // override 'len' and 'cap' keywords

            Action<object> len = _p0 =>
            {
            }
;
            len(a); // ERROR "call of len copies lock value: sync.Mutex"

            Action<object> cap = _p0 =>
            {
            }
;
            cap(a); // ERROR "call of cap copies lock value: sync.Mutex"
        }

        public static void SizeofMutex()
        {
            sync.Mutex mu = default;
            @unsafe.Sizeof(mu); // OK
            unsafe1.Sizeof(mu); // OK
            Sizeof(mu); // OK
            struct{Sizeoffunc(interface{})} @unsafe = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{Sizeoffunc(interface{})}{};
            @unsafe.Sizeof(mu); // ERROR "call of unsafe.Sizeof copies lock value: sync.Mutex"
            Action<object> Sizeof = _p0 =>
            {
            }
;
            Sizeof(mu); // ERROR "call of Sizeof copies lock value: sync.Mutex"
        }

        // SyncTypesCheck checks copying of sync.* types except sync.Mutex
        public static void SyncTypesCheck()
        { 
            // sync.RWMutex copying
            sync.RWMutex rwmuX = default;
            sync.RWMutex rwmuXX = new sync.RWMutex();
            ptr<sync.RWMutex> rwmuX1 = @new<sync.RWMutex>();
            var rwmuY = rwmuX; // ERROR "assignment copies lock value to rwmuY: sync.RWMutex"
            rwmuY = rwmuX; // ERROR "assignment copies lock value to rwmuY: sync.RWMutex"
            var rwmuYY = rwmuX; // ERROR "variable declaration copies lock value to rwmuYY: sync.RWMutex"
            var rwmuP = ref rwmuX;
            sync.RWMutex rwmuZ = ref new sync.RWMutex(); 

            // sync.Cond copying
            sync.Cond condX = default;
            sync.Cond condXX = new sync.Cond();
            ptr<sync.Cond> condX1 = @new<sync.Cond>();
            var condY = condX; // ERROR "assignment copies lock value to condY: sync.Cond contains sync.noCopy"
            condY = condX; // ERROR "assignment copies lock value to condY: sync.Cond contains sync.noCopy"
            var condYY = condX; // ERROR "variable declaration copies lock value to condYY: sync.Cond contains sync.noCopy"
            var condP = ref condX;
            sync.Cond condZ = ref new sync.Cond(L:&sync.Mutex{},);
            condZ = sync.NewCond(ref new sync.Mutex()); 

            // sync.WaitGroup copying
            sync.WaitGroup wgX = default;
            sync.WaitGroup wgXX = new sync.WaitGroup();
            ptr<sync.WaitGroup> wgX1 = @new<sync.WaitGroup>();
            var wgY = wgX; // ERROR "assignment copies lock value to wgY: sync.WaitGroup contains sync.noCopy"
            wgY = wgX; // ERROR "assignment copies lock value to wgY: sync.WaitGroup contains sync.noCopy"
            var wgYY = wgX; // ERROR "variable declaration copies lock value to wgYY: sync.WaitGroup contains sync.noCopy"
            var wgP = ref wgX;
            sync.WaitGroup wgZ = ref new sync.WaitGroup(); 

            // sync.Pool copying
            sync.Pool poolX = default;
            sync.Pool poolXX = new sync.Pool();
            ptr<sync.Pool> poolX1 = @new<sync.Pool>();
            var poolY = poolX; // ERROR "assignment copies lock value to poolY: sync.Pool contains sync.noCopy"
            poolY = poolX; // ERROR "assignment copies lock value to poolY: sync.Pool contains sync.noCopy"
            var poolYY = poolX; // ERROR "variable declaration copies lock value to poolYY: sync.Pool contains sync.noCopy"
            var poolP = ref poolX;
            sync.Pool poolZ = ref new sync.Pool(); 

            // sync.Once copying
            sync.Once onceX = default;
            sync.Once onceXX = new sync.Once();
            ptr<sync.Once> onceX1 = @new<sync.Once>();
            var onceY = onceX; // ERROR "assignment copies lock value to onceY: sync.Once contains sync.Mutex"
            onceY = onceX; // ERROR "assignment copies lock value to onceY: sync.Once contains sync.Mutex"
            var onceYY = onceX; // ERROR "variable declaration copies lock value to onceYY: sync.Once contains sync.Mutex"
            var onceP = ref onceX;
            sync.Once onceZ = ref new sync.Once();
        }

        // AtomicTypesCheck checks copying of sync/atomic types
        public static void AtomicTypesCheck()
        { 
            // atomic.Value copying
            atomic.Value vX = default;
            atomic.Value vXX = new atomic.Value();
            ptr<atomic.Value> vX1 = @new<atomic.Value>(); 
            // These are OK because the value has not been used yet.
            // (And vet can't tell whether it has been used, so they're always OK.)
            var vY = vX;
            vY = vX;
            var vYY = vX;
            var vP = ref vX;
            atomic.Value vZ = ref new atomic.Value();
        }
    }
}}}
