using Ont.SmartContract.Framework;
using Ont.SmartContract.Framework.Services.Ont;
using Helper = Ont.SmartContract.Framework.Helper;

public class MapExample : SmartContract
{

    public static void Main()
    {
        Map<string, int> m = new Map<string, int>();
        m["k1"] = 100;
        m["k2"] = 200;
        Runtime.Notify(m["k1"]);
        Runtime.Notify(m["k2"]);

        // seriallize
        byte[] b = Helper.Serialize(m);
        Storage.Put(Storage.CurrentContext, "mapexample", b);
        byte[] v = Storage.Get(Storage.CurrentContext, "mapexample");

        // deserialize
        Map<string, int> m2 = (Map<string, int>)Helper.Deserialize(v);
        Runtime.Notify(m2["k1"]);
        Runtime.Notify(m2["k2"]);
    }
    
}