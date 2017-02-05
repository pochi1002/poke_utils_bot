using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace PokeUtils.Utils
{
    public static class CollectionExtensions
    {
        public static T2 GetValueOrDefault<T1, T2>(this Dictionary<T1, T2> obj, T1 key)
        {
            T2 value;
            if (obj == null || key == null) return default(T2);
            return obj.TryGetValue(key, out value) ? value : default(T2);
        }
        public static T2 ForceGetValue<T1, T2>(this IDictionary<T1, T2> obj, T1 key, Func<T2> generateFunc)
        {
            T2 value;
            if (obj == null || key == null) throw new ArgumentException();
            if (obj.TryGetValue(key, out value)) return value;
            lock (obj)
            {
                if (obj.TryGetValue(key, out value)) return value;
                obj[key] = generateFunc();
                return obj[key];
            }
        }
        /// <summary>
        /// スレッドセーフじゃないので気を付けて
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public static async Task<T2> ForceGetValueAsync<T1, T2>(this IDictionary<T1, T2> obj, T1 key, Func<Task<T2>> generateFunc)
        {
            T2 value;
            if (obj == null || key == null) throw new ArgumentException();
            if (obj.TryGetValue(key, out value)) return value;
            obj[key] = await generateFunc();
            return obj[key];
        }
        public static T2 ForceGetValue<T1, T2>(this ConcurrentDictionary<T1, T2> obj, T1 key, Func<T2> generateFunc)
        {
            T2 value;
            if (obj == null || key == null) throw new ArgumentException();
            if (obj.TryGetValue(key, out value)) return value;
            lock (obj)
            {
                if (obj.TryGetValue(key, out value)) return value;
                obj[key] = generateFunc();
                return obj[key];
            }
        }
        public static T2 GetValueOrDefault<T1, T2>(this IDictionary<T1, T2> obj, T1 key)
        {
            T2 value;
            if (obj == null || key == null) return default(T2);
            return obj.TryGetValue(key, out value) ? value : default(T2);
        }
        public static T2 GetValueOrDefault<T1, T2>(this IReadOnlyDictionary<T1, T2> obj, T1 key)
        {
            T2 value;
            if (obj == null || key == null) return default(T2);
            return obj.TryGetValue(key, out value) ? value : default(T2);
        }

        public static T2 GetValueOrDefault<T1, T2>(this IReadOnlyDictionary<T1, T2> obj, T1 key, T2 defaultValue)
        {
            T2 value;
            if (obj == null || key == null) return default(T2);
            return obj.TryGetValue(key, out value) ? value : defaultValue;
        }
    }
}