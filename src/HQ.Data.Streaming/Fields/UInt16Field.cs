using System;
using System.Text;

namespace HQ.Data.Streaming.Fields
{
    public readonly ref struct UInt16Field
    {
        public ushort? Value => !_encoding.TryParse(_buffer, out ushort value) ? default(ushort?) : value;
        public string RawValue => _encoding.GetString(_buffer);

        private readonly Encoding _encoding;
        private readonly ReadOnlySpan<byte> _buffer;

        public UInt16Field(ReadOnlySpan<byte> buffer, Encoding encoding)
        {
            _buffer = buffer;
            _encoding = encoding;
        }

        public unsafe UInt16Field(byte* start, int length, Encoding encoding)
        {
            _buffer = new ReadOnlySpan<byte>(start, length);
            _encoding = encoding;
        }
    }
}
