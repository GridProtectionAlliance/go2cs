using fmt = go.fmt_package;
using sort = go.sort_package;

namespace go
{
    public static partial class main_package
    {
        public partial struct Person
        {
            public @string Name;
            public nint Age;
            public float ShoeSize;
        }

        public partial struct PeopleByShoeSize // : slice<Person>
        {
        }

        public partial struct PeopleByAge // : slice<Person>
        {
        }

        public static nint Len(this PeopleByShoeSize p)
        {
            return len(p);
        }

        public static void Swap(this PeopleByShoeSize p, nint i, nint j)
        {
            (p[i], p[j]) = (p[j], p[i]);
        }

        public static bool Less(this PeopleByShoeSize p, nint i, nint j)
        {
            return (p[i].ShoeSize < p[j].ShoeSize);
        }

        public static nint Len(this PeopleByAge p)
        {
            return len(p);
        }

        public static void Swap(this PeopleByAge p, nint i, nint j)
        {
            (p[i], p[j]) = (p[j], p[i]);
        }

        public static bool Less(this PeopleByAge p, nint i, nint j)
        {
            return (p[i].Age < p[j].Age);
        }

        private static void Main()
        {
            Person[] persons = new[] { new Person(Name:"Person1",Age: 26,ShoeSize: 8 ), new Person(Name: "Person2", Age: 21, ShoeSize: 4), new Person(Name:"Person3",Age: 15,ShoeSize: 9) };
            var people = new slice<Person>(persons);

            fmt.Println(people);

            sort.Sort(new PeopleByShoeSize(people));
            fmt.Println(people);

            sort.Sort(new PeopleByAge(people));
            fmt.Println(people);
        }
    }
}
