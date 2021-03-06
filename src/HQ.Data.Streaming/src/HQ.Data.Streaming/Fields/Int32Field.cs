#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Diagnostics;
using System.Text;

namespace HQ.Data.Streaming.Fields
{
    [DebuggerDisplay("Int32: {Value} ({RawValue}:{_encoding.BodyName})")]
    public readonly ref struct Int32Field
    {
        public bool Initialized => _buffer != null;
        public int? Value => Initialized ? !_encoding.TryParse(_buffer, out int value) ? default(int?) : value : default;
        public string RawValue => Initialized ? _encoding.GetString(_buffer) : default;

        private readonly Encoding _encoding;
        private readonly ReadOnlySpan<byte> _buffer;

        public Int32Field(ReadOnlySpan<byte> buffer, Encoding encoding)
        {
            _buffer = buffer;
            _encoding = encoding;
        }

        public unsafe Int32Field(byte* start, int length, Encoding encoding)
        {
            _buffer = new ReadOnlySpan<byte>(start, length);
            _encoding = encoding;
        }
    }
}
