using System;
using System.Text;

namespace HQ.Evolve.Fields
{
    public readonly ref struct DecimalField
    {
        public decimal? Value => !_encoding.TryParse(_buffer, out decimal value) ? default(decimal?) : value;
        public string RawValue => _encoding.GetString(_buffer);

        private readonly Encoding _encoding;
        private readonly ReadOnlySpan<byte> _buffer;

        public DecimalField(ReadOnlySpan<byte> buffer, Encoding encoding)
        {
            _buffer = buffer;
            _encoding = encoding;
        }

        public unsafe DecimalField(byte* start, int length, Encoding encoding)
        {
            _buffer = new ReadOnlySpan<byte>(start, length);
            _encoding = encoding;
        }
    }
}
