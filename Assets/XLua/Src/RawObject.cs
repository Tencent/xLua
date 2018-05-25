namespace XLua
{
    public interface RawObject
    {
        object Target { get; }
    }
}

namespace XLua.Cast
{
    public class Any<T> : RawObject
    {
        T mTarget;

        public Any(T i)
        {
            mTarget = i;
        }

        public object Target
        {
            get
            {
                return mTarget;
            }
        }
    }

    public class Byte : Any<byte>
    {
        public Byte(byte i) : base(i)
        {
        }
    }

    public class SByte : Any<sbyte>
    {
        public SByte(sbyte i) : base(i)
        {
        }
    }

    public class Char : Any<char>
    {
        public Char(char i) : base(i)
        {
        }
    }

    public class Int16 : Any<short>
    {
        public Int16(short i) : base(i)
        {
        }
    }

    public class UInt16 : Any<ushort>
    {
        public UInt16(ushort i) : base(i)
        {
        }
    }

    public class Int32 : Any<int>
    {
        public Int32(int i) : base(i)
        {
        }
    }

    public class UInt32 : Any<uint>
    {
        public UInt32(uint i) : base(i)
        {
        }
    }

    public class Int64 : Any<long>
    {
        public Int64(long i) : base(i)
        {
        }
    }

    public class UInt64 : Any<ulong>
    {
        public UInt64(ulong i) : base(i)
        {
        }
    }

    public class Float : Any<float>
    {
        public Float(float i) : base(i)
        {
        }
    }
}
