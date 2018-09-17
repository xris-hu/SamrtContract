using Ont.SmartContract.Framework;
using Ont.SmartContract.Framework.Services.Ont;
using System;

namespace Asset{

    public class NativeAsset
    {
        public static readonly byte[] OntContract = "AFmseVrdL9f9oyCzZefL9tG6UbvhUMqNMV".ToScriptHash();
        public static readonly byte[] OngContract = "AFmseVrdL9f9oyCzZefL9tG6UbvhfRZMHJ".ToScriptHash();

        public static bool RecycleAsset(byte[] from, byte[] to, ulong ont, ulong ong)
        {
            byte[] ret;
            Transfer transfer = new Transfer { From = from, To = to, Value = ont };
            ret = Native.Invoke(0, OntContract, "transfer", new object[1] { transfer });
            if (ret[0] != 1) throw new Exception("tansfer ont error.") ;

            transfer.Value = ong;
            ret = Native.Invoke(0, OngContract, "transfer", new object[1] { transfer });
            if (ret[0] != 1) throw new Exception("transfer ong error.");
            return true;
        }

        struct Transfer
        {
            public byte[] From;
            public byte[] To;
            public ulong Value;
        }
    }
}
