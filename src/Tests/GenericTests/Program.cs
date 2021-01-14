using System;
using go;
using static go.builtin;
using sort = go.sort_package;

namespace ConstraintTests
{
    public static partial class Program
    {
        // Currently Go interface with a type list constraint cannot be used as an ordinary interface type, so encountering one
        // of these need not apply normal interface handling. It may not need definition in the converted C# code at all:

        // For Go "comparable" constraint, this matches C# IEquatable<T>

        // For most any other contraint list, the target for C# may just need to be IComparable<T>

        // Since if Go code compiles, it will already have accounted for actual usage type constraints, all converted C# needs to do is handle comparisons

        // This works in C# because all native types already implement IEquatable<T> and IComparable<T>

        // Test based on following example:
        //      https: //go.googlesource.com/proposal/+/refs/heads/master/design/go2draft-type-parameters.md#sort
        
        //public interface Ordered<in T> : IComparable<T>
        //{
        //}

        // type Ordered interface {
        //    type int, int8, int16, int32, int64,
        //            uint, uint8, uint16, uint32, uint64, uintptr,
        //            float32, float64,
        //            string
        // }

        /*
            // orderedSlice is an internal type that implements sort.Interface.
            // The Less method uses the < operator. The Ordered type constraint
            // ensures that T has a < operator.
            type orderedSlice[T Ordered] []T

            func (s orderedSlice[T]) Len() int           { return len(s) }
            func (s orderedSlice[T]) Less(i, j int) bool { return s[i] < s[j] }
            func (s orderedSlice[T]) Swap(i, j int)      { s[i], s[j] = s[j], s[i] }

            // OrderedSlice sorts the slice s in ascending order.
            // The elements of s must be ordered using the < operator.
            func OrderedSlice[T Ordered](s []T) {
                // Convert s to the type orderedSlice[T].
                // As s is []T, and orderedSlice[T] is defined as []T,
                // this conversion is permitted.
                // orderedSlice[T] implements sort.Interface,
                // so can pass the result to sort.Sort.
                // The elements will be sorted using the < operator.
                sort.Sort(orderedSlice[T](s))
            }         
         */

        public partial struct orderedSlice<T> { /* : slice<T> where T : Ordered */ }

        public static nint Len<T>(this orderedSlice<T> s) where T : IComparable<T> => s.Length;
        
        // Comparisons on generics will need to use IEquatable / IComparable methods - operators will not work in C#
        public static bool Less<T>(this orderedSlice<T> s, nint i, nint j) where T : IComparable<T> => s[i].CompareTo(s[j]) < 0;

        public static void Swap<T>(this orderedSlice<T> s, nint i, nint j) where T : IComparable<T> => (s[i], s[j]) = (s[j], s[i]);

        public static void OrderedSlice<T>(slice<T> s) where T : IComparable<T>
        {
            orderedSlice<T> orderedSlice = new orderedSlice<T>(s);
            sort.Sort(orderedSlice);

            foreach (object value in orderedSlice.Array)
                Console.Write($"{value},");

            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            slice<nint> s1 = slice(new nint[] { 3, 5, 2 });
            OrderedSlice(s1);

            slice<@string> s2 = slice(new @string[] { "c", "a", "b" });
            OrderedSlice(s2);
        }
    }
}
