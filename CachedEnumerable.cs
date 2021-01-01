using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CachedIEnumerable {

    public static class CachedEnumerable {
        public static CachedEnumerable<T> Create<T> (IEnumerable<T> enumerable) {
            return new CachedEnumerable<T> (enumerable);
        }
    }

    public sealed class CachedEnumerable<T> : IEnumerable<T>, IDisposable {
        private readonly List<T> cache = new List<T> ();
        private readonly IEnumerable<T> _enumerable;
        private IEnumerator<T> _enumerator;
        private bool _enumerated = false;

        public CachedEnumerable (IEnumerable<T> enumerable) {
            _enumerable = enumerable ??
                throw new ArgumentNullException (nameof (enumerable));
        }

        public void Dispose () {
            if (_enumerator != null) {
                _enumerator.Dispose ();
                _enumerator = null;
            }
        }

        public IEnumerator<T> GetEnumerator () {
            var index = 0;
            while (true) {
                if (TryGetItem (index, out var Result)) {
                    yield return Result;
                    index++;
                } else {
                    // There are no items

                    yield break;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator () => GetEnumerator ();
        private bool TryGetItem (int index, out T Result) {
            //if the item is in the cache, use it
            if (index < cache.Count) {
                Result = cache[index];
                return true;
            }

            lock (cache) {
                if (_enumerator == null && !_enumerated) {
                    _enumerator = _enumerable.GetEnumerator ();
                }
                // Başka bir thread aldıysa yapacack bişi yok
                if (index < cache.Count) {
                    Result = cache[index];
                    return true;
                }
                // If we have already enumerate the whole stream, there is nothing else to do
                if (_enumerated) {
                    Result = default;
                    return false;
                }

                // Get the next item and store it to the cache,
                if (_enumerator.MoveNext ()) {
                    Result = _enumerator.Current;
                    cache.Add (Result);
                    return true;
                } else {
                    //There are no more items , we can dispose the underlying enumerator
                    _enumerator.Dispose ();
                    _enumerator = null;
                    _enumerated = true;
                    Result = default;
                    return false;
                }

            }
        }
    }
}