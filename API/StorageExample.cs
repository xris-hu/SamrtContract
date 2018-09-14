using Ont.SmartContract.Framework;
using Ont.SmartContract.Framework.Services.Ont;
using Ont.SmartContract.Framework.Services.System;
using System.Numerics;
using Helper = Ont.SmartContract.Framework.Helper;

public class StorageExample : SmartContract
{
    public static void Main()
    {
        // get the storage context
        StorageContext context = Storage.CurrentContext;

        Storage.Put(context, "key", 100);
        BigInteger v = Storage.Get(context, "key").AsBigInteger();
        Runtime.Notify(v);

        Storage.Put(context, "key".AsByteArray(), "100");
        Runtime.Notify(Storage.Get(context,"key".AsByteArray()).AsString());

        Storage.Delete(context, "key");
        Runtime.Notify(Storage.Get(context, "key".AsByteArray()));
    }
}