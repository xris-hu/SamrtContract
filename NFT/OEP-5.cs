using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.ComponentModel;
using System.Numerics;

namespace Ontology
{
    public class Ontology : SmartContract
    {
        //Token Settings
        public static string Name() => "My Non-Fungibles";
        public static string Symbol() => "MNFT";
        public static readonly byte[] admin = "ATrzHaicmhRj15C3Vv6e6gLfLqhSD2PtTr".ToScriptHash();

        //Store Key Prefix
        public static readonly byte[] owner_balance_prefix = "Balance".AsByteArray();
        public static readonly byte[] owner_of_token_prefix = "OwnerOf".AsByteArray();
        public static readonly byte[] approve_prefix = "Approve".AsByteArray();
        public static readonly byte[] token_index_prefix = "Index".AsByteArray();
        public static readonly byte[] token_cir_key = "in_circulation".AsByteArray();
        public static readonly byte[] token_property_prefix = "token_property".AsByteArray();
        public static readonly byte[] token_url_prefix = "token_url".AsByteArray();


        public delegate void deleTransfer(byte[] from, byte[] to, byte[] tokenId);
        [DisplayName("transfer")]
        public static event deleTransfer Transferred;

        public delegate void deleApprove(byte[] onwer, byte[] spender, byte[] tokenId);
        [DisplayName("approval")]
        public static event deleApprove Approval;

        public struct State
        {
            public byte[] To;
            public byte[] TokenId;
        }

        public static Object Main(string operation, params object[] args)
        {
            if (operation == "name") return Name();
            if (operation == "symbol") return Symbol();
            if (operation == "balanceOf")
            {
                if (args.Length != 1) return 0;
                byte[] address = (byte[])args[0];
                return BalanceOf(address);
            }
            if (operation == "ownerOf")
            {
               if (args.Length != 1) return false;
                byte[] tokenId = (byte[])args[0];
                return ownerOf(tokenId);
            }
            if (operation == "transfer")
            {
                if (args.Length != 2) return false;
                byte[] to = (byte[])args[0];
                byte[] tokenId = (byte[])args[1];
                return Transfer(to, tokenId);
            }
            if (operation == "transferMulti")
            {
                return TransferMulti(args);
            }
            if (operation == "approve")
            {
                if (args.Length != 2) return false;
                byte[] to = (byte[])args[0];
                byte[] tokenId = (byte[])args[1];
                return Approve(to, tokenId);
            }
            if (operation == "takeOwnership")
            {
                if (args.Length != 2) return false;
                byte[] to = (byte[])args[0];
                byte[] tokenId = (byte[])args[1];
                return TakeOwnership(to, tokenId);
            }

            if (operation == "mintToken")
            {
                if (args.Length != 4) return false;
                byte[] owner = (byte[])args[0];
                byte[] tokenId = (byte[])args[1];
                byte[] property = (byte[])args[2];
                string url = (string)args[3];
                return MintToken(owner, tokenId, property, url);
            }

            if (operation == "totalSupply")
            {
                return TotalSupply();
            }

            return false;
        }

        /// <summary>ransfers an NFTs of tokens from the from account to the to account.
        ///         to SHOULD be 20-byte address. If not, throw an exception.
        /// </summary>
        /// <returns>return transfer result, success or fail. </returns>
        /// <param name="to">token receiver address</param>
        /// <param name="tokenId">NFT token id</param>
        public static bool Transfer(byte[] to, byte[] tokenId)
        {
            Require(validateAddress(to), "invalid to address");
            byte[] owner = ownerOf(tokenId);
            Require(Runtime.CheckWitness(owner), "invalid owner");
            StorageContext context = Storage.CurrentContext;

            Storage.Put(context, owner_balance_prefix.Concat(owner), BalanceOf(to) - 1);
            Storage.Put(context, owner_of_token_prefix.Concat(tokenId), to);
            Storage.Put(context, owner_balance_prefix.Concat(to), BalanceOf(to) + 1);
            return true;
        }

        /// <summary>transfer multiple amount of NFT token from  multiple sender to multiple receiver</summary>
        /// <returns>return transfer result, if any transfer fail, all of transfers should fail. </returns>
        /// <param name="args">state struct</param>
        ///  public struct State
        /// {
        ///    public byte[] TokenId; // NFT token id
        ///    public byte[] To; // transfer receiver
        ///}
        public static bool TransferMulti(object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                State state = (State)args[i];
                if (!Transfer(state.To, state.TokenId)) throw new Exception();
            }
            return true;
        }

        /// <summary>return the number of NFTs assigned to address</summary>
        /// <returns>the owner's total NFT number</returns>
        /// <param name="address">address SHOULD be a 20-byte address</param>
        public static BigInteger BalanceOf(byte[] address)
        {
            if (!validateAddress(address)) throw new Exception("Invalid address");
            return Storage.Get(Storage.CurrentContext, owner_balance_prefix.Concat(address)).AsBigInteger();
        }

        /// <summary>return the address currently marked as the owner of tokenId</summary>
        /// <returns>owner address</returns>
        /// <param name="tokenId">NFT token id</param>
        public static byte[] ownerOf(byte[] tokenId)
        {
            byte[] owner = Storage.Get(Storage.CurrentContext, owner_of_token_prefix.Concat(tokenId));
            if (owner == null || !validateAddress(owner)) throw new Exception();
            return owner;
        }

        /// <summary>
        ///    The approve allows to grant an NFTs toto account multiple times.
        ///    If this function is called again it overwrites to or tokenId
        /// </summary>
        /// <returns>transfer result, success or failure</returns>
        /// <param name="to">approve receiver address</param>
        /// <param name="tokenId">approved NFT token id</param>
        public static bool Approve(byte[] to, byte[] tokenId)
        {
            Require(validateAddress(to), "invalid to address");
            byte[] owner = ownerOf(tokenId);
            Require(Runtime.CheckWitness(owner), "invalid owner");

            Storage.Put(Storage.CurrentContext, approve_prefix.Concat(tokenId), to);
            Runtime.Notify("approve", owner, to, tokenId);
            return true;
        }

        /// <summary>
        ///    Assigns the ownership of the Non-fungible with ID tokenId to to address. 
        ///    to SHOULD be 20-byte addresses.
        /// </summary>
        /// <returns> success or failure</returns>
        /// <param name="to">receiver address</param>
        /// <param name="tokenId">NFT token id</param>
        public static bool TakeOwnership(byte[] to, byte[] tokenId)
        {
            Require(Runtime.CheckWitness(to), "not owner");
            byte[] owner = ownerOf(tokenId);
            Require(validateAddress(owner), "invalid address");
            byte[] approveAcc = Storage.Get(Storage.CurrentContext, approve_prefix.Concat(tokenId));
            Require(approveAcc == to, "not approved");

            Storage.Delete(Storage.CurrentContext, approve_prefix.Concat(tokenId));
            Storage.Put(Storage.CurrentContext, owner_of_token_prefix.Concat(tokenId), to);

            Storage.Put(Storage.CurrentContext, owner_balance_prefix.Concat(owner), BalanceOf(owner) - 1);
            Storage.Put(Storage.CurrentContext, owner_balance_prefix.Concat(to), BalanceOf(to) + 1);
            Runtime.Notify("take the owner ship", owner, to, tokenId);
            return true;
        }

        /// <summary>
        ///    Mint Token issue new NFT token id to a owner 
        /// </summary>
        /// <returns> success or failure</returns>
        /// <param name="owner">token receiver address</param>
        /// <param name="tokenId">NFT token id</param>
        /// <param name="property">the NFT token property, can't be empty.</param>
        /// <param name="URL">token url</param>
        public static bool MintToken(byte[] owner,byte[] tokenId, byte[] property, string URL)
        {
            Require(Runtime.CheckWitness(admin), "not admin");
            Require(validateAddress(owner), "invalid address");
            Require(property == null, "missing properties data string");
            Require(tokenId.Length <= 128, "token id too long");

            StorageContext context = Storage.CurrentContext;
            if (validateAddress(Storage.Get(context, owner_of_token_prefix.Concat(tokenId)))) {
                Runtime.Notify("token id already exist");
                return false;
            }
            BigInteger tokenNumber = Storage.Get(context, token_cir_key).AsBigInteger();
            tokenNumber += 1;

            Storage.Put(context, token_cir_key, tokenNumber);
            Storage.Put(context, owner_of_token_prefix.Concat(tokenId), owner);
            Storage.Put(context, token_property_prefix.Concat(tokenId), property);
            Storage.Put(context, token_url_prefix.Concat(tokenId), URL);
            Storage.Put(Storage.CurrentContext, owner_balance_prefix.Concat(owner), BalanceOf(owner) + 1);
            Runtime.Notify("mintToken", owner, tokenId);
            return true;
        }

        public static BigInteger TotalSupply()
        {
            return Storage.Get(Storage.CurrentContext, token_cir_key).AsBigInteger();
        }

        private static bool validateAddress(byte[] address)
        {
            if (address.Length != 20) return false;
            if (address.AsBigInteger() == 0) return false;
            return true;
        }

        private static void Require(bool cond, string msg)
        {
            if (!cond)
            {
                Runtime.Log(msg);
                throw new Exception();
            }
        }
    }
}
