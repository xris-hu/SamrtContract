using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;

public class RedEnvelope : SmartContract
{
    private static readonly byte[] ongAddr = "AFmseVrdL9f9oyCzZefL9tG6UbvhfRZMHJ".ToScriptHash();

    private const ulong factor = 1000000000; // ONG Decimal
    private const ulong divider = 10000000; //
    private const ulong minOng = 1;

    private static readonly byte[] moneyPrefix = "Luck_".AsByteArray();
    private static readonly byte[] sizePrefix = "LuckSize_".AsByteArray();
    private static readonly byte[] luckerPrefix = "Lucker_".AsByteArray();

    public static Object Main(string operation, params object[] args)
    {
        if (operation == "SendLuckyMoney")
        {
            if (args.Length != 3) return false;
            byte[] account = (byte[])args[0];
            ulong value = (ulong)args[1];
            ulong size = (ulong)args[2];
            return SendLuckyMoney(account, value, size);
        }

        if (operation == "GetLuckyMoney")
        {
            if (args.Length != 2) return false;
            byte[] hash = (byte[])args[0];
            byte[] account = (byte[])args[1];
            return GetLuckyMoney(hash, account);
        }

        if (operation == "GetLuckyList")
        {
            if (args.Length != 1) return false;
            byte[] hash = (byte[])args[0];
            return GetLuckyList(hash);
        }
        return false;
    }


    public static byte[] SendLuckyMoney(byte[] account, ulong value, ulong size)
    {
        if (value <= minOng || !validateAddress(account) || !Runtime.CheckWitness(account)) return null;
        BigInteger ongValue = value * factor;
        byte[] hash = ExecutionEngine.ExecutingScriptHash;
        byte[] ret = Native.Invoke(0, ongAddr, "transfer", new object[1] { new Transfer { From = account, To = hash, Value = (ulong)ongValue } });
        if (ret[0] != 1) return null;

        BigInteger t = Runtime.Time;
        byte[]  envelopHash = hash.Concat(t.AsByteArray()).Concat(account);
        Storage.Put(Storage.CurrentContext, moneyPrefix.Concat(envelopHash), ongValue);
        Storage.Put(Storage.CurrentContext, sizePrefix.Concat(envelopHash), size);
        Runtime.Notify("Account:", account, " send a red envelope: ", envelopHash);
        return envelopHash;
    }

    public static bool GetLuckyMoney(byte[] envelopHash, byte[] account)
    {
        if (!validateAddress(account) || !Runtime.CheckWitness(account)) return false;
        StorageContext context = Storage.CurrentContext;
        BigInteger remainMoney = Storage.Get(context, moneyPrefix.Concat(envelopHash)).AsBigInteger();
        BigInteger remainSize = Storage.Get(context,  sizePrefix.Concat(envelopHash)).AsBigInteger();
        // 红包已发完
        if (remainMoney <= 0 || remainSize < 1 ) return false;
        // 已经领取红包
        if (Storage.Get(context, luckerPrefix.Concat(envelopHash).Concat(account)).AsBigInteger() != 0) {
            Runtime.Log("You have open the envelope.");
            return false;
        }

        byte[] ret;
        if (remainSize == 1)
        {
            ret = Native.Invoke(0, ongAddr, "transfer", new object[1] { new Transfer { From = ExecutionEngine.ExecutingScriptHash, To = account, Value = (ulong)remainMoney } });
            if (ret[0] != 1) return false;
            Storage.Put(context, moneyPrefix.Concat(envelopHash), 0);
            Storage.Put(context, sizePrefix.Concat(envelopHash), 0);
            Storage.Put(context, luckerPrefix.Concat(envelopHash).Concat(account), remainMoney);
            Runtime.Notify("account:", account, " get luck money: ", remainMoney);
            return true;
        }
        BigInteger money;
        BigInteger range = (remainMoney / divider) / remainSize * 2; // 最大值：剩余平均值 * 2
        BigInteger quota = (BigInteger)(Runtime.Time % range);
        if (quota == 0) { money = 1 * divider; } else { money = quota * divider; } // 最小值：0.01 ONG

        ret = Native.Invoke(0, ongAddr, "transfer", new object[1] { new Transfer { From = ExecutionEngine.ExecutingScriptHash, To = account, Value = (ulong)money } });
        if (ret[0] != 1) return false;
        Storage.Put(context, moneyPrefix.Concat(envelopHash), remainMoney - money);
        Storage.Put(context, sizePrefix.Concat(envelopHash), remainSize - 1);
        Storage.Put(context, luckerPrefix.Concat(envelopHash).Concat(account), money);
        Runtime.Notify("account:", account, " get luck money: ", money);
        return true;
    }

    private static byte[] GetLuckyList(byte[] envelopHash)
    {
        //TODO
        return null;
    }

    private static bool validateAddress(byte[] address)
    {
        if (address.Length != 20) return false;
        if (address.AsBigInteger() == 0) return false;
        return true;
    }

    struct Transfer
    {
        public byte[] From;
        public byte[] To;
        public ulong Value;
    }
}
