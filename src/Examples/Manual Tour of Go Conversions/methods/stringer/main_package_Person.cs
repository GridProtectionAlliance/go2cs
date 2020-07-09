using System.Runtime.CompilerServices;
using go;

public static partial class main_package
{
    public partial struct Person
    {
        public Person((@string, int) i) :
            this(i.Item1, i.Item2) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Person(@string Name = default, int Age = default) {
            this.Name = Name;
            this.Age = Age;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"{{{Name}}} {{{Age}}}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Person((@string, int) value) => new Person(value);

        // Person to nil comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Person obj, NilType _) => obj.Equals( default(Person));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Person obj, NilType nil) => !(obj == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, Person obj) => obj == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, Person obj) => obj != nil;
    }
}
