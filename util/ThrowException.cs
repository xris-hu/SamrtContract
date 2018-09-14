using Ont.SmartContract.Framework;
using Ont.SmartContract.Framework.Services.Ont;
using Ont.SmartContract.Framework.Services.System;
using System;
using System.ComponentModel;
using System.Numerics;

namespace ExceptionTest 
{
    public class ExceptionTest : SmartContract
    {
       
		public static Object Main(string operation, params object[] args)
        {
        	if (operation == "transferMulti")
            {
            	return TransferMulti(args);
            }
            return false;
        }

      	// smart contract throw excepiton example in multiple transfer 
        public static bool TransferMulti(object[] args)
        {
            for(int i=0;i<args.Length;i++)
            {
                State state = (State)args[i];
                if(!Transfer(state.From, state.To, state.Amount)) throw new Exception("Transfer failed.");
            }
            return true;
        }

        public static bool Transfer(byte[] from, byte[] to, BigInteger value)
        {
            if (value <= 0) return false;
            if (!Runtime.CheckWitness(from)) return false;
            if (to.Length != 20) return false;
            if (from == to) return true;

            BigInteger from_value = Storage.Get(Storage.CurrentContext, from).AsBigInteger();
            if (from_value < value) return false;
            if (from_value == value)
                Storage.Delete(Storage.CurrentContext, from);
            else
                Storage.Put(Storage.CurrentContext, from, from_value - value);

            BigInteger to_value = Storage.Get(Storage.CurrentContext, to).AsBigInteger();
            Storage.Put(Storage.CurrentContext, to, to_value + value);
            Runtime.Notify(from, to, value);
            return true;
        }

         public struct State
        {
            public byte[] From;
            public byte[] To;
            public BigInteger Amount;
        }
        
    }
}