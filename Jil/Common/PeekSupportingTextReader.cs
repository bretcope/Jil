using System.IO;

namespace StringInterningJil.Common
{
    class PeekSupportingTextReader : TextReader
    {
        TextReader Inner;

        int? Peeked;

        public PeekSupportingTextReader(TextReader inner)
        {
            Inner = inner;
        }

        public override int Peek()
        {
            if (Peeked != null) return Peeked.Value;

            Peeked = Inner.Read();

            return Peeked.Value;
        }

        public override int Read()
        {
            if (Peeked != null)
            {
                var ret = Peeked.Value;
                Peeked = null;

                return ret;
            }

            return Inner.Read();
        }
    }
}
