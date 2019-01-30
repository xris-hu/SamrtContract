from ontology.interop.Ontology.Native import Invoke
from ontology.interop.Ontology.Contract import Migrate
from ontology.interop.System.Action import RegisterAction
from ontology.interop.Ontology.Runtime import Base58ToAddress
from ontology.interop.System.App import RegisterAppCall, DynamicAppCall
from ontology.interop.System.Storage import Put, GetContext, Get, Delete
from ontology.interop.System.ExecutionEngine import GetExecutingScriptHash
from ontology.libont import AddressFromVmCode, bytes2hexstring, bytearray_reverse
from ontology.interop.System.Runtime import CheckWitness, Notify, Serialize, Deserialize

ONT_ADDRESS = bytearray(b'\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x01')
ONG_ADDRESS = bytearray(b'\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x02')
ZERO_ADDRESS = bytearray(b'\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00')
CONTRACT_ADDRESS = GetExecutingScriptHash()
ctx = GetContext()

NAME = 'PAX'
SYMBOL = 'PAX'
DECIMALS = 18
FACTOR = 1000000000000000000

TOKEN_SYMBOL_PREFIX = 'TOKEN_SYMBOL'
PAUSED = 'PAUSED'
INITIALIZED = "INITIALIZED"
TOTAL_SUPPLY_KEY = 'TOTAL_SUPPLY'
ENFORCEMENT_ROLE_KEY = 'LAW_ENFORCEMENT_ROLE'
SUPPLY_CONTROLLER_KEY = 'SUPPLY_CONTROLLER'
OWNER_KEY = 'OWNER'
BALANCE_PREFIX = 'BALANCE'
APPROVE_PREFIX = 'APPROVE'
FROZEN_PREFIX = 'FROZEN'

Owner = Base58ToAddress('AGjD4Mo25kzcStyh1stp7tXkUuMopD43NT')
SupplyController = Base58ToAddress('AGjD4Mo25kzcStyh1stp7tXkUuMopD43NT')
EnforcementRole = Base58ToAddress('AGjD4Mo25kzcStyh1stp7tXkUuMopD43NT')

TransferEvent = RegisterAction("TRANSFER", "from", "to", "amount")
TransferOwnerEvent = RegisterAction("TRANSFEROWNER", "oldowner", "newowner")
ApproveEvent = RegisterAction("Approve", "owner", "spender", "amount")
PauseEvent = RegisterAction("PAUSED")
UnpauseEvent = RegisterAction("UNPAUSED")
FrozenEvent = RegisterAction("FrozenAddress", "address")
UnfrozenEvent = RegisterAction("UnfrozenAddress", "address")
WipeFrozenEvent = RegisterAction("WipeFrozenAddress", "address")
SupplyIncreaseEvent = RegisterAction("SupplyIncreased", "address", "amount")
SupplyDecreaseEvent = RegisterAction("SupplyDecreased", "address", "amount")
SetSupplyControllerEvent = RegisterAction("SetSupplyController", "newController")
UpgradeContractEvent = RegisterAction("UpgradeContract")

def Main(operation, args):

    if operation == "init":
        return init()

    if operation == 'name':
        return name()

    if operation == 'symbol':
        return symbol()

    if operation == 'decimals':
        return decimals()

    if operation == 'totalSupply':
        return totalSupply()

    if operation == 'balanceOf':
        if len(args) != 1:
            return False
        acct = args[0]
        return balanceOf(acct)

    if operation == 'transfer':
        from_acct = args[0]
        to_acct = args[1]
        amount = args[2]
        return transfer(from_acct, to_acct, amount)

    if operation == 'transferMulti':
        return transferMulti(args)

    if operation == 'transferFrom':
        if len(args) != 4:
            return False
        spender = args[0]
        from_acct = args[1]
        to_acct = args[2]
        amount = args[3]
        return transferFrom(spender, from_acct, to_acct, amount)

    if operation == 'approve':
        if len(args) != 3:
            return False
        owner = args[0]
        spender = args[1]
        amount = args[2]
        return approve(owner, spender, amount)

    if operation == 'allowance':
        if len(args) != 2:
            return False
        owner = args[0]
        spender = args[1]
        return allowance(owner, spender)

    if operation == 'increaseSupply':
        amount = args[0]
        return increaseSupply(amount)

    if operation == 'decreaseSupply':
        amount = args[0]
        return decreaseSupply(amount)

    if operation == 'setLawEnforcementRole':
        newRole = args[0]
        return setLawEnforcementRole(newRole)

    if operation == 'getEnforcementRole':
        return getEnforcementRole()

    if operation == 'transferOwnership':
        newOwner = args[0]
        return transferOwnership(newOwner)

    if operation == 'getOwner':
        return getOwner()

    if operation == 'freeze':
        address = args[0]
        return freeze(address)

    if operation == 'unfreez':
        address = args[0]
        return unfreez(address)

    if operation == 'wipeFrozenAddress':
        address = args[0]
        return wipeFrozenAddress(address)

    if operation == 'setSupplyController':
        address = args[0]
        return setSupplyController(address)

    if operation == 'getSupplyController':
        return getSupplyController()

    if operation == 'pause':
        return pause()

    if operation == 'unpause':
        return unpause()

    if operation == 'isPaused':
        return isPaused()

    if operation == 'isInitialized':
        return isInitialized()

    if operation == 'isFrozen':
        address = args[0]
        return isFrozen(address)

def init():
    """
    Initialize smart contract.

    :return: True or raise exception.
    """
    assert (CheckWitness(Owner))
    assert (not isInitialized())

    Put(ctx, INITIALIZED, True)
    Put(ctx, TOTAL_SUPPLY_KEY, 0)
    Put(ctx, OWNER_KEY, Owner)
    Put(ctx, ENFORCEMENT_ROLE_KEY, EnforcementRole)
    Put(ctx, SUPPLY_CONTROLLER_KEY, SupplyController)
    Put(ctx, concat(BALANCE_PREFIX, SupplyController), 0)

    return True

def increaseSupply(amount):
    """
    Increase supply token to supply controller address.
    :param amount: Increase token amount.
    :return: True or raise exception.
    """
    assert(amount > 0)
    assert (CheckWitness(getSupplyController()))

    balance = balanceOf(getSupplyController())
    Put(ctx, concat(BALANCE_PREFIX, getSupplyController()), Add(balance, amount))
    Put(ctx, TOTAL_SUPPLY_KEY, Add(totalSupply(), amount))

    SupplyIncreaseEvent(getSupplyController(), amount)
    return True

def decreaseSupply(amount):
    """
    Decrease token supply from supply controller address.
    :param amount: decreased token amount.
    :return:
    """
    assert (amount > 0)
    assert (CheckWitness(getSupplyController()))

    balance = balanceOf(getSupplyController())
    Put(ctx, concat(BALANCE_PREFIX, getSupplyController()), Sub(balance, amount))
    Put(ctx, TOTAL_SUPPLY_KEY, Sub(totalSupply(), amount))

    SupplyDecreaseEvent(getSupplyController(), amount)
    return True

def setLawEnforcementRole(newEnforceRole):
    """
    Set Enforment role address.
    :param newEnforceRole: new enforcement role acccount.
    :return:
    """
    assert (isAddress(newEnforceRole))
    assert (CheckWitness(getEnforcementRole() or CheckWitness(getOwner())))

    Put(ctx, ENFORCEMENT_ROLE_KEY, newEnforceRole)
    return True

def getEnforcementRole():
    """
    Get current enforcement role account.
    :return: enforcement role.
    """
    enforcementRole = Get(ctx, ENFORCEMENT_ROLE_KEY)

    if not enforcementRole:
        return getOwner()

    return enforcementRole

def transferOwnership(newOwner):
    """
    transfer contract ownership from current owner to new owner account.
    :param newOwner: new smart contract owner.
    :return:True or raise exception.
    """
    assert(isAddress(newOwner))
    assert(CheckWitness(getOwner()))

    Put(ctx, OWNER_KEY, newOwner)
    TransferOwnerEvent(getOwner(), newOwner)
    return True

def getOwner():
    """
    Get contract owner.
    :return:smart contract owner.
    """
    return Get(ctx, OWNER_KEY)

def freeze(address):
    """
    Freeze specific acccount, it will not  be traded unless it will be unfreez.
    :param address: Frozen account.
    :return:True or raise exception.
    """
    assert(isAddress(address))
    assert (CheckWitness(getEnforcementRole()))

    Put(ctx, concat(FROZEN_PREFIX, address), True)
    FrozenEvent(address)
    return True

def unfreez(address):
    """
    Unfreeze specific account, this account will be re-traded
    :param address:Unfrozen account.
    :return: True or raise exception.
    """

    assert (isAddress(address))
    assert (CheckWitness(getEnforcementRole()))

    Delete(ctx, concat(FROZEN_PREFIX, address))
    UnfrozenEvent(address)
    return True

def wipeFrozenAddress(address):
    """
    Deduct the balance of the frozen account to 0.
    :param address:frozen account.
    :return:True or raise exception.
    """
    assert(isAddress(address))
    assert(CheckWitness(getEnforcementRole()))
    balance = balanceOf(address)
    total = totalSupply()
    Put(ctx, TOTAL_SUPPLY_KEY, Sub(total, balance))
    Put(ctx, concat(BALANCE_PREFIX, address), 0)

    WipeFrozenEvent(address)
    return True

def setSupplyController(address):
    """
    Set new supply controller account.
    :param address: new supply controller account.
    :return:
    """
    assert (isAddress(address))
    assert(CheckWitness(getSupplyController()))

    Put(ctx, SUPPLY_CONTROLLER_KEY, address)
    SetSupplyControllerEvent(address)
    return True

def getSupplyController():
    """
    Get current contract supply controller account.
    :return: supply controller account.
    """
    return Get(ctx, SUPPLY_CONTROLLER_KEY)

def pause():
    """
    Set the smart contract to paused state, the token can not be transfered, approved.
    Just can invoke some get interface, like getOwner.
    :return:True or raise exception.
    """
    assert(CheckWitness(getOwner()))

    Put(ctx, PAUSED, True)

def unpause():
    """
    Resume the smart contract to normal state, all the function can be invoked.
    :return:True or raise exception.
    """
    assert(CheckWitness(getOwner()))

    Put(ctx, PAUSED, False)


def isPaused():
    """
    Confirm whether the contract is paused or not.
    :return: True or False
    """
    return Get(ctx, PAUSED)

def isInitialized():
    """
    Confir whether the contract is initialized or not.
    :return: True or False
    """
    return Get(ctx, INITIALIZED)

def isFrozen(address):
    """
    Confir whether specific account is frozen or not.
    :param address:confirmed account.
    :return:True or False.
    """
    return Get(ctx, concat(FROZEN_PREFIX, address))

def name():
    """
    :return: name of the token
    """
    return NAME


def symbol():
    """
    :return: symbol of the token
    """
    return SYMBOL


def decimals():
    """
    :return: the decimals of the token
    """
    return DECIMALS


def totalSupply():
    """
    :return: the total supply of the token
    """
    return Get(ctx, TOTAL_SUPPLY_KEY)


def balanceOf(account):
    """
    :param account:
    :return: the token balance of account
    """
    return Get(ctx, concat(BALANCE_PREFIX, account))


def transfer(from_acct, to_acct, amount):
    """
    Transfer amount of tokens from from_acct to to_acct
    :param from_acct: the account from which the amount of tokens will be transferred
    :param to_acct: the account to which the amount of tokens will be transferred
    :param amount: the amount of the tokens to be transferred, >= 0
    :return: True means success, False or raising exception means failure.
    """
    assert(not isPaused())
    assert(amount > 0)
    assert(isAddress(to_acct))
    assert(CheckWitness(from_acct))
    assert(not isFrozen(from_acct))
    assert(not isFrozen(to_acct))


    fromKey = concat(BALANCE_PREFIX, from_acct)
    fromBalance = balanceOf(from_acct)
    if amount > fromBalance:
        return False
    if amount == fromBalance:
        Delete(ctx, fromKey)
    else:
        Put(ctx, fromKey, Sub(fromBalance, amount))

    toKey = concat(BALANCE_PREFIX, to_acct)
    toBalance = balanceOf(to_acct)
    Put(ctx, toKey, Add(toBalance, amount))

    TransferEvent(from_acct, to_acct, amount)

    return True


def transferMulti(args):
    """
    :param args: the parameter is an array, containing element like [from, to, amount]
    :return: True means success, False or raising exception means failure.
    """
    assert(not isPaused())

    for p in args:
        if len(p) != 3:
            raise Exception("transferMulti params error.")
        if transfer(p[0], p[1], p[2]) == False:
            raise Exception("transferMulti failed.")
    return True


def approve(owner, spender, amount):
    """
    owner allow spender to spend amount of token from owner account
    Note here, the amount should be less than the balance of owner right now.
    :param owner:
    :param spender:
    :param amount: amount>=0
    :return: True means success, False or raising exception means failure.
    """
    assert (amount > 0)
    assert (not isPaused())
    assert (not isFrozen(owner))
    assert (not isFrozen(spender))

    assert (isAddress(owner) and isAddress(spender))
    assert (CheckWitness(owner))
    assert (balanceOf(owner) >= amount)

    Put(ctx, concat(concat(APPROVE_PREFIX, owner), spender), amount)

    ApproveEvent(owner, spender, amount)

    return True


def transferFrom(spender, from_acct, to_acct, amount):
    """
    spender spends amount of tokens on the behalf of from_acct, spender makes a transaction of amount of tokens
    from from_acct to to_acct
    :param spender:
    :param from_acct:
    :param to_acct:
    :param amount:
    :return:
    """
    assert (amount > 0 )
    assert (isAddress(spender) and isAddress(from_acct) and isAddress(to_acct))
    assert (CheckWitness(spender))

    fromKey = concat(BALANCE_PREFIX, from_acct)
    fromBalance = balanceOf(from_acct)
    assert (fromBalance >= amount)

    approveKey = concat(concat(APPROVE_PREFIX, from_acct), spender)
    approvedAmount = Get(ctx, approveKey)

    if amount > approvedAmount:
        return False
    elif amount == approvedAmount:
        Delete(ctx, approveKey)
        Put(ctx, fromKey, Sub(fromBalance, amount))
    else:
        Put(ctx, approveKey, Sub(approvedAmount, amount))
        Put(ctx, fromKey, Sub(fromBalance, amount))

    toBalance = balanceOf(to_acct)
    Put(ctx, concat(BALANCE_PREFIX, to_acct), Add(toBalance, amount))

    TransferEvent(from_acct, to_acct, amount)

    return True


def allowance(owner, spender):
    """
    check how many token the spender is allowed to spend from owner account
    :param owner: token owner
    :param spender:  token spender
    :return: the allowed amount of tokens
    """
    key = concat(concat(APPROVE_PREFIX, owner), spender)
    return Get(ctx, key)

def UpgradeContract(code):
    """
    Upgrade current smart contract to new smart contract.
    :param code:new smart contract avm code.
    :return: True or raise exception.
    """
    owner = getOwner()
    assert(CheckWitness(owner))

    ongBalance = Invoke(0, ONG_ADDRESS, 'balanceOf', state(CONTRACT_ADDRESS))
    res = Invoke(0, ONG_ADDRESS, "transfer", [state(CONTRACT_ADDRESS, owner, ongBalance)])
    if res != b'\x01':
        assert(False)

    ontBalance = Invoke(0, ONT_ADDRESS, 'balanceOf', state(CONTRACT_ADDRESS))
    res = Invoke(0, ONT_ADDRESS, "transfer", [state(CONTRACT_ADDRESS, owner, ontBalance)])
    if res != b'\x01':
        assert (False)

    #upgrade smart contract
    res = Migrate(code, "", "", "", "", "", "")
    if not res:
        assert (False)

    UpgradeContractEvent()

    return True

def Add(a, b):
    """
    Adds two numbers, throws on overflow.
    :param a:operand a
    :param b:operand b
    :return:
	"""
    c = a + b
    assert (c >= a)
    return c

def Sub(a, b):
	"""
	Substracts two numbers, throws on overflow (i.e. if subtrahend is greater than minuend).
    :param a: operand a
    :param b: operand b
    :return: a - b if a - b > 0 or revert the transaction.
	"""
	assert(a>=b)
	return a-b

def Mul(a, b):
    """
    Multiplies two numbers, throws on overflow.
    :param a: operand a
    :param b: operand b
    :return:
    """

    if a == 0:
        return 0
    c = a * b
    assert(c / a == b)
    return c

def Div(a, b):
    """
    Integer division of two numbers, truncating the quotient.
    :param a: operand a
    :param b: operand b
    :return: 
	"""

    assert (b > 0)
    c = a / b
    return c

def isAddress(address):
    """
    check the address is legal address.
    :param address:
    :return:True or raise exception.
    """
    assert (len(address) == 20 and address != ZERO_ADDRESS)
    return True
