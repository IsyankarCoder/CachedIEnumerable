using System;
using System.Collections;
using System.Collections.Generic;

using System.Threading.Tasks;

namespace CachedIEnumerable {
    class Program {
        static void Main (string[] args) {
            var enumerable = MyEnumerable ();
            using var cachedEnum = CachedEnumerable.Create (enumerable);
            Parallel.ForEach (cachedEnum, item => Console.WriteLine (item+"---"));

            foreach (var item in cachedEnum) {
                Console.WriteLine (item);
            }
        }

        static IEnumerable<int> MyEnumerable () {
            //Should be called only once
            yield return 1;
            yield return 2;
            yield return 3;
            yield return 4;
            yield return 5;

        }
    }
}