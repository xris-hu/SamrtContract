using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Helper = Neo.SmartContract.Framework.Helper;

public class StructExmple : SmartContract
{

    public static void Main()
    {
        // create struct instance
        Person p = new Person();
        p.Name = "bob";
        p.Age = 20;
        Runtime.Notify(p.Name);
        Runtime.Notify(p.Age);

        // serialize struct instance to byte[]
        byte[] b = Helper.Serialize(p);

        // deserialize byte[] to struct
        Person p2 = (Person)Helper.Deserialize(b);
        Runtime.Notify(p2.Name);
        Runtime.Notify(p2.Age);

        // create struct array
        Person[] array = new Person[] { new Person { Name = "tom", Age = 18 }, new Person { Name = "jack", Age = 20 } };
    }

    struct Person
    {
        public string Name;
        public int Age;
    }
}
