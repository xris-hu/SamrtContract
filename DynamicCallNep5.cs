using Ont.SmartContract.Framework;
using System;
using System.Numerics;

namespace Contract
{
    public class Contract : SmartContract
    {
        public delegate object NEP5Contract(string method, object[] args);

        public static Object Main(string operation, params object[] args)
        {
            if (operation == "CallNep5Contract")
            {
                if (args.Length != 4) return false;
                byte[] from = (byte[])args[0];
                byte[] to = (byte[])args[1];
                ulong value = (ulong)args[2];
                byte[] hash = (byte[])args[3];
                return CallNep5Contract(from, to, value, hash);
            }
            return false;
        }
       
        public static bool CallNep5Contract(byte[] from, byte[] to, ulong value, byte[] contractHash)
        {
       		
            if (!TransferNEP5(from, to, contractHash, value)) throw new Exception();
            return true;
        }

        private static bool TransferNEP5(byte[] from, byte[] to, byte[] assetID, BigInteger amount)
        {
            var args = new object[] { from, to, amount };
            var contract = (NEP5Contract)assetID.ToDelegate();
            if (!(bool)contract("transfer", args)) return false;
            return true;
        }
    }
}

