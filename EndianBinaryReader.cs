using System;
using System.IO;

public class EndianBinaryReader : BinaryReader
{
    private bool isLittleEndian;

    public EndianBinaryReader(Stream input, bool isLittleEndian)
        : base(input)
    {
        this.isLittleEndian = isLittleEndian;
    }

    public bool IsLittleEndian
    {
        get { return isLittleEndian; }
        set { isLittleEndian = value; }
    }

    public override short ReadInt16()
    {
        return isLittleEndian ? base.ReadInt16() : BitConverter.ToInt16(ReverseBytes(base.ReadBytes(2)), 0);
    }

    public override int ReadInt32()
    {
        return isLittleEndian ? base.ReadInt32() : BitConverter.ToInt32(ReverseBytes(base.ReadBytes(4)), 0);
    }

    public override long ReadInt64()
    {
        return isLittleEndian ? base.ReadInt64() : BitConverter.ToInt64(ReverseBytes(base.ReadBytes(8)), 0);
    }

    private byte[] ReverseBytes(byte[] bytes)
    {
        Array.Reverse(bytes);
        return bytes;
    }
}
