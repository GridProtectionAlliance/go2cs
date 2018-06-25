// package main -- go2cs converted at 2018 June 25 16:32:20 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\SortArrayType.go

using fmt = go.fmt_package;
using sort = go.sort_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {


        public static long Len(this PeopleByShoeSize p)
        {
            returnlen(p)
        }

        public static void Swap(this PeopleByShoeSize p, long i, long j
        {
            p[i],p[j]=p[j],p[i]
        }

        public static bool Less(this PeopleByShoeSize p, long i, long j
        {
            return(p[i].ShoeSize<p[j].ShoeSize)
        }

        private static void Main()
        {
            people:=[]Person{{Name:"Person1",Age:25,ShoeSize:8,},{Name:"Person2",Age:21,ShoeSize:4,},{Name:"Person3",Age:15,ShoeSize:9,},{Name:"Person4",Age:45,ShoeSize:15,},{Name:"Person5",Age:25,ShoeSize:8.5,}}fmt.Println(people)sort.Sort(PeopleByShoeSize(people))fmt.Println(people)
        }
    }
}
