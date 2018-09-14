using Ont.SmartContract.Framework;
using Ont.SmartContract.Framework.Services.Ont;
using Ont.SmartContract.Framework.Services.System;
using System.Numerics;
using Helper = Ont.SmartContract.Framework.Helper;

public class RuntimeExample : SmartContract
{
    public static void Main()
    {
        // get current block time 
        Runtime.Notify(Runtime.Time);

        // check the caller authority, it's very useful function for check user authentication
        byte[] user = { 248, 142, 51, 220, 214, 177, 110, 235, 27, 218, 59, 86, 23, 47, 140, 20, 114, 119, 159, 152 };
        Runtime.CheckWitness(user);

        // Notify the message to the blockchain,the arguments is a object array
        Runtime.Notify("Hi blockchain");
        Runtime.Notify("s1", "s2", 1, 2, 3);

        // Runtime.Log() write the log message to the blockchain, the arguments can only be string
        Runtime.Log("log message.");

    }
}