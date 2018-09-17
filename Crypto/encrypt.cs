using Ont.SmartContract.Framework;
using Ont.SmartContract.Framework.Services.Ont;
using System;

namespace Crypto
{
    public class CryptoExample : SmartContract
    {
        public static void Main()
        {
             /// The input is hashed using SHA-1.
            Sha1("123456789".AsByteArray());
            
            /// The input is hashed using SHA-256.
            Sha256("123456789".AsByteArray());
             
            /// The input is hashed using Hash160: first with SHA-256 and then with RIPEMD-160.
            Hash160("123456789".AsByteArray());
             
            /// The input is hashed using Hash256: twice with SHA-256.  
            Hash256("123456789".AsByteArray());
        }
    }
}
