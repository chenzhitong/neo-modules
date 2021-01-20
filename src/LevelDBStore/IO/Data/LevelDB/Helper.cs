using Neo.IO.Caching;
using Neo.Persistence;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Neo.IO.Data.LevelDB
{
    public static class Helper
    {
        public static IEnumerable<T> Seek<T>(this DB db, ReadOptions options, byte[] prefix, SeekDirection direction, Func<byte[], byte[], T> resultSelector)
        {
            using Iterator it = db.NewIterator(options);
            if (direction == SeekDirection.Forward)
            {
                for (it.Seek(prefix); it.Valid(); it.Next())
                {
                    var key = it.Key();
                    if (key.Length < 1) break;
                    yield return resultSelector(it.Key(), it.Value());
                }
            }
            else
            {
                // SeekForPrev

                it.Seek(prefix);
                if (!it.Valid())
                    it.SeekToLast();
                else if (it.Key().AsSpan().SequenceCompareTo(prefix) > 0)
                    it.Prev();

                for (; it.Valid(); it.Prev())
                {
                    var key = it.Key();
                    if (key.Length < 1) break;
                    yield return resultSelector(it.Key(), it.Value());
                }
            }
        }

        public static IEnumerable<T> FindRange<T>(this DB db, ReadOptions options, byte[] startKey, byte[] endKey, Func<byte[], byte[], T> resultSelector)
        {
            using Iterator it = db.NewIterator(options);
            for (it.Seek(startKey); it.Valid(); it.Next())
            {
                byte[] key = it.Key();
                if (key.AsSpan().SequenceCompareTo(endKey) > 0) break;
                yield return resultSelector(key, it.Value());
            }
        }

        internal static byte[] ToByteArray(this IntPtr data, UIntPtr length)
        {
            if (data == IntPtr.Zero) return null;
            byte[] buffer = new byte[(int)length];
            Marshal.Copy(data, buffer, 0, (int)length);
            return buffer;
        }

        public static byte[] CreateKey(byte[] key = null)
        {
            if (key is null || key.Length == 0) return Array.Empty<byte>();
            byte[] buffer = new byte[1 + key.Length];
            Buffer.BlockCopy(key, 0, buffer, 1, key.Length);
            return buffer;
        }

        public static byte[] CreateKey(byte key) => CreateKey(new byte[] { key });
    }
}
