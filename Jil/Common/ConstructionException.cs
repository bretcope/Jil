using System;

namespace StringInterningJil.Common
{
    // Just a common type to act as a "couldn't build this junk!" catch-all
    sealed class ConstructionException : Exception
    {
        public ConstructionException(string msg) : base(msg) { }
    }
}
