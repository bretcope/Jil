using System;

namespace StringInterningJil.Deserialize
{
    internal class StringInternHash
    {
        internal static readonly StringInternHash Instance;

        static StringInternHash()
        {
            // explicit static constructor so that Instance only gets instantiated when it's actually accessed the first time
            Instance = new StringInternHash(64);
        }

        private const uint FNV_PRIME = 16777619;
        private const uint FNV_OFFSET_BASIS = 2166136261;

        private int[] _buckets;
        private Slot[] _slots;
        private int _nextAvailableSlotIndex;

        private readonly object _writeLock = new object();

        public int Count => _nextAvailableSlotIndex;
        public int MaxSize => _slots.Length;

        private StringInternHash(int initialSize)
        {
            _buckets = new int[initialSize];
            _slots = new Slot[initialSize];
        }

        internal string GetString(char[] buffer, int length)
        {
            var hash = GetHashCode(buffer, length);
            var str = GetExistingString(buffer, length, hash);

            if (str != null)
                return str;

            // an existing string wasn't found, we need to add it to the hash
            lock (_writeLock)
            {
                // first, check one more time to see if it exists
                str = GetExistingString(buffer, length, hash);

                if (str == null)
                {
                    // it definitely doesn't exist. Let's add it
                    str = new string(buffer, 0, length);
                    Add(str, hash);
                }

                return str;
            }
        }

        private string GetExistingString(char[] buffer, int length, uint hash)
        {
            int[] buckets;
            Slot[] slots;
            do
            {
                buckets = _buckets;
                slots = _slots;

            } while (buckets.Length != slots.Length); // mismatch means we read right in the middle of a Grow(). Keep trying until we get a match.

            var bucket = hash % buckets.Length;
            var slotIndex = buckets[bucket] - 1;

            while (slotIndex >= 0)
            {
                var value = slots[slotIndex].Value;
                if (slots[slotIndex].HashCode == hash && IsMatchingString(value, buffer, length))
                {
                    return value;
                }

                slotIndex = slots[slotIndex].Next;
            }

            return null;
        }

        private void Add(string s, uint hash)
        {
            if (_nextAvailableSlotIndex == _slots.Length)
                Grow();

            var bucket = hash % _slots.Length;
            var slotIndex = _nextAvailableSlotIndex;
            _nextAvailableSlotIndex++;

            _slots[slotIndex].Value = s;
            _slots[slotIndex].HashCode = hash;
            _slots[slotIndex].Next = _buckets[bucket] - 1;

            _buckets[bucket] = slotIndex + 1;
        }

        private void Grow()
        {
            var oldSize = _slots.Length;
            var newSize = oldSize * 2;

            var newSlots = new Slot[newSize];
            Array.Copy(_slots, newSlots, oldSize);

            var newBuckets = new int[newSize];
            for (var i = 0; i < oldSize; i++)
            {
                var bucket = newSlots[i].HashCode % newSize;
                newSlots[i].Next = newBuckets[bucket] - 1;
                newBuckets[bucket] = i + 1;
            }

            _buckets = newBuckets;
            _slots = newSlots;
        }

        private static uint GetHashCode(char[] buffer, int length)
        {
            var hash = FNV_OFFSET_BASIS;
            for (var i = 0; i < length; i++)
            {
                hash = unchecked((buffer[i] ^ hash) * FNV_PRIME);
            }

            return hash;
        }

        private static bool IsMatchingString(string s, char[] buffer, int length)
        {
            if (s.Length != length)
                return false;

            for (var i = 0; i < length; i++)
            {
                if (s[i] != buffer[i])
                    return false;
            }

            return true;
        }

        private struct Slot
        {
            internal string Value;
            internal uint HashCode;
            internal int Next;
        }
    }
}