### FAQ

1. Q: **How to calculate the amount of the red envelope？What is the maximum and minimum**

   A:  Random value between 0.01 and 2 times of the average of residual money. 

   For example, if you send 100 ONG，total 10 red envelope, so the average ONG amount of the red  envelope  is 10, so the available of the money range between 0.01 and 20. 

   If 3 red envelope has been opened and the residual ONG is 60,  next round,  the range is 0.01 -  60/7 *2 = 17.14.

    If the previous person is not lucky, the more residual ONG , the more available ONG  for next round.

2. Q: **How randomness is designed**

   A:  Currently, modulo the balance through the time of previous block

3. Q: **Whether randomness can be predicted**

   A: In the first round, it is theoretically possible to predict a reasonable time to modulate the balance and obtain a relatively large red envelope, but the greatest unpredictability is the behavior of others. If the red envelope is opened by anyone, The balance will change, that is, the variable that is modulo changes, the best time will change again, and so on, until the red envelope is finished, no one can guarantee the best time.

4. Q: **How to make a red envelope?**

   A: Call the **“SendLuckyMoney”** method in the contract, which accepts three parameters

   1：your wallet address

   2：the ONG amount，minimum 10 ONG

   3：the number of red envelopes available

   it will response a red envelope hash.

5. Q: **How to receive the red envelope?**

   A: Call the **“GetLuckyMoney”** method in the contract, which accepts 2 parameters

   1: the red envelope hash

   2: your wallet address
