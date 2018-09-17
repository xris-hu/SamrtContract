using Ont.SmartContract.Framework;
using Ont.SmartContract.Framework.Services.Ont;
using Ont.SmartContract.Framework.Services.System;
using System.Numerics;

public class Contract : SmartContract
{
    public static bool Main(string operation, params object[] args)
    {
        return false;
    }

    private byte[] bytesConcat(byte[] a, byte[] b)
    {
        return a.Concat(b);
    }

    private byte[] stringConcat(string a, string b)
    {
        return a.AsByteArray().Concat(b.AsByteArray());
    }

    private byte[] stringToBytes(string s)
    {
        return s.AsByteArray();
    }

    private string bytesToString(byte[] arr)
    {
        return arr.AsString();
    }

    private BigInteger bytestoBiginteger(byte[] arr)
    {
        return arr.AsBigInteger();
    }

    private byte[] bigintegerToBytes(BigInteger b)
    {
        return b.AsByteArray();
    }

}