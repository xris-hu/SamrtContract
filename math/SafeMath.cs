using Neo.SmartContract.Framework;
using System;
using System.Numerics;

namespace Math{

    public class SafeMath
    {
        public static BigInteger Mul(BigInteger a, BigInteger b)
        {
            if (a == 0)
            {
                return 0;
            }

            BigInteger c = a * b;
            if (c / a != b) throw new Exception();

            return c;
        }

        public static BigInteger Div(BigInteger a, BigInteger b)
        {
            if (b <= 0) throw new Exception();
            BigInteger c = a / b;

            return c;
        }

        public static BigInteger Sub(BigInteger a, BigInteger b)
        {
            if (b > a) throw new Exception();
            BigInteger c = a - b;

            return c;
        }

        public static BigInteger Add(BigInteger a, BigInteger b)
        {
            BigInteger c = a + b;
            if (c < a) throw new Exception();

            return c;
        }

        public static BigInteger Mod(BigInteger a, BigInteger b)
        {
            if (b == 0) throw new Exception();
            return a % b;
        }
    }
}
