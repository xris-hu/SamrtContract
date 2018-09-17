using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;

public class ContractExample : SmartContract
{
    public static bool Main(string operation, params object[] args)
    {
        if (operation == "Destroy")
        {
            return DestroyContract();
        }
        if (operation == "Migrate")
        {
            //smart contract avm code
            byte[] code = (byte[])args[0];
            return Migrate(code);
        }

        return false;
    }

    public static bool DestroyContract()
    {
        Contract.Destroy();
        return true;
    }

    public static bool Migrate(byte[] code)
    {
        Contract.Migrate(code, true, "", "", "", "", "");
        return true;
    }
}