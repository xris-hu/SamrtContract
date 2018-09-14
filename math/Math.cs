using Neo.SmartContract.Framework;
using System;
using System.Numerics;

namespace Math{

    public class ArithMath
    {
        public static BigInteger Max(BigInteger a, BigInteger b)
        {
            return a >= b ? a : b;
        }

        public static BigInteger Min(BigInteger a, BigInteger b)
        {
            return a >= b ? a : b;
        }

        public static BigInteger Average(BigInteger a, BigInteger b)
        {
            return (a / 2) + (b / 2) + ((a % 2 + b % 2) / 2);
        }
    }
}
