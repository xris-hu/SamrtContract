using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;

namespace CGAS
{
    public class StorageExample : SmartContract
    {
        public static void Main(string method, object[] args)
        {
            StorageMap asset = Storage.CurrentContext.CreateMap(nameof(asset));

            asset.Put("key", 100);
            Runtime.Notify(asset.Get("key").AsBigInteger());

            asset.Get("key");
            Runtime.Notify(asset.Get("key").AsBigInteger());

            asset.Delete("key");
            Runtime.Notify(asset.Get("key").AsBigInteger());
        }
    }
}
