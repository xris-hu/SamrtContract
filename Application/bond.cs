using Ont.SmartContract.Framework;
using Ont.SmartContract.Framework.Services.Ont;
using Ont.SmartContract.Framework.Services.System;
using Helper = Ont.SmartContract.Framework.Helper;
using System;
using System.Numerics;
using System.ComponentModel;

public class Bond : SmartContract
{
    // address setting
    public static readonly byte[] ontAddr = "AFmseVrdL9f9oyCzZefL9tG6UbvhUMqNMV".ToScriptHash();
    public static readonly byte[] admin = "AeS7aUsTmf7egcGQGS88LZAGD8gNFmCJnD".ToScriptHash();

    public static readonly byte[] bondPrefix = "Bond_".AsByteArray();
    public static readonly byte[] bondInvestorPrefix = "BondInvestor_".AsByteArray();
    public static readonly byte[] bondPaidPrefix = "BondPaid_".AsByteArray();

    // bond setting
    private const int minInterval = 259200; // 30 day;
    private const int minIssueCap = 100000;
    private const int minInvestCap = 1000; 
    private const int minRound = 6;

    public static Object Main(string operation, params object[] args)
    {
        // only admin can issue bond
        if (operation == "IssueBond")
        {
            if (args.Length != 8) return false;
            string bondName = (string)args[0];
            uint parValue = (uint)args[1];
            uint startTime = (uint)args[2];
            uint maturity = (uint)args[3];
            uint interval = (uint)args[4];
            uint couponRate = (uint)args[5];
            ulong totalAmount = (ulong)args[6];
            byte[] account = (byte[])args[7];

            return IssueBond(bondName, parValue, startTime, maturity, interval, couponRate, totalAmount, account);
        }
        if (operation == "InvestBond")
        {
            if (args.Length != 3) return false;
            string bondName = (string)args[0];
            byte[] account = (byte[])args[1];
            uint count = (uint)args[2];
            return InvestBond(bondName, account, count);
        }
        if (operation == "PayInterstOrPrincipal")
        {
            if (args.Length != 2) return false;
            string bondName = (string)args[0];
            byte[] account = (byte[])args[1];
            return PayInterstOrPrincipal(bondName, account);
        }
        if (operation == "GetBond")
        {
            if (args.Length != 1) return false;
            string bondName = (string)args[0];
            return GetBond(bondName);
        }
        return false;
    }


    public static bool IssueBond(string bondName, uint parValue, uint purchaseEndTime, uint interval, uint round, uint couponRate, ulong totalCap, byte[] Account)
    {
        if (!Runtime.CheckWitness(admin)) return false;
        if (purchaseEndTime <= Runtime.Time) return false;
        if (totalCap < minIssueCap || round < minRound || couponRate <= 0 || interval < minInterval) return false;
        if (!validateAddress(Account)) return false;

        BondItem bond = new BondItem();
        bond.purchaseEndTime = purchaseEndTime;
        bond.CouponRate = couponRate;
        bond.Interval = interval;
        bond.TotalCap = totalCap;
        bond.remainCap = totalCap;
        bond.Round = round;
        bond.Maturity = purchaseEndTime + round * interval;

        byte[] b = Helper.Serialize(bond);
        Storage.Put(Storage.CurrentContext, bondPrefix.Concat(bondName.AsByteArray()), b);

        return true;
    }

    public static bool InvestBond(string bondName, byte[] account, uint bondNumber)
    {
        if (!Runtime.CheckWitness(account)) return false;
        if (bondNumber <= 0 || !validateAddress(account) || !validateBond(bondName)) return false;
        BondItem bond = (BondItem)Helper.Deserialize(GetBond(bondName));

        if (Runtime.Time > bond.purchaseEndTime) {
            Runtime.Notify("bond subscription has been ended.");
            return false;
        }

        uint investValue = bondNumber * bond.ParValue;
        if (bond.remainCap < investValue) {
            Runtime.Notify("bond remain invest capacity not enough.");
            return false;
        }

        byte[] ret = Native.Invoke(0, ontAddr, "transfer", new object[1] { new Transfer { From = account, To = bond.Account, Value = investValue } });
        if (ret[0] != 1) return false;
        bond.remainCap = bond.TotalCap - investValue;

        byte[] investorKey = bondInvestorPrefix.Concat(bondName.AsByteArray()).Concat(account);
        BigInteger balance = Storage.Get(Storage.CurrentContext, investorKey).AsBigInteger();
        Storage.Put(Storage.CurrentContext, investorKey,balance + investValue);

        return true;
    }

    public static bool PayInterstOrPrincipal(string bondName, byte[] account)
    {
        if (!validateBond(bondName)) return false;

        byte[] investorKey = bondInvestorPrefix.Concat(bondName.AsByteArray()).Concat(account);
        BigInteger balance = Storage.Get(Storage.CurrentContext, investorKey).AsBigInteger();
        if (balance < minInvestCap) return false;

        BondItem bond = (BondItem)Helper.Deserialize(GetBond(bondName));

        byte[] paidKey = bondPaidPrefix.Concat(bondName.AsByteArray()).Concat(account);
        BigInteger paidRound = Storage.Get(Storage.CurrentContext, paidKey).AsBigInteger();
        BigInteger currentRound = (Runtime.Time - bond.purchaseEndTime) / bond.Interval;

        if (paidRound > bond.Round) return false;
        if (currentRound > bond.Round) currentRound = bond.Round;

        BigInteger investValue = Storage.Get(Storage.CurrentContext, investorKey).AsBigInteger();
        BigInteger interst = (currentRound - paidRound) * (investValue * bond.CouponRate / 100);

        byte[] ret;
        if (currentRound == bond.Round)
        {
            ret = Native.Invoke(0, ontAddr, "transfer", new object[1] { new Transfer { From = bond.Account, To = account, Value = (ulong)(interst + investValue) } });
        }
        else{
            ret = Native.Invoke(0, ontAddr, "transfer", new object[1] { new Transfer { From = bond.Account, To = account, Value = (ulong)interst } });
        }
        
        if (ret[0] != 1) return false;
        Storage.Put(Storage.CurrentContext, paidKey, paidRound + 1);

        return true;
    }


    public static byte[] GetBond(string bondName)
    {
        return Storage.Get(Storage.CurrentContext, bondPrefix.Concat(bondName.AsByteArray()));
    }

    private static bool validateAddress(byte[] address)
    {
        if (address.Length != 20) return false;
        if (address.AsBigInteger() == 0) return false;
        return true;
    }

    private static bool validateBond(string bondName)
    {
        byte[] v = Storage.Get(Storage.CurrentContext, bondPrefix.Concat(bondName.AsByteArray()));
        if (v == null || v.Length == 0) return false;
        return true;
    }

    struct BondItem
    {
        public uint ParValue;
        public uint purchaseEndTime;
        public uint Maturity;
        public uint Interval;
        public uint CouponRate;
        public uint Round;
        public ulong TotalCap;
        public ulong remainCap;
        public byte[] Account;
    }

    struct Transfer
    {
        public byte[] From;
        public byte[] To;
        public ulong Value;
    }
}
