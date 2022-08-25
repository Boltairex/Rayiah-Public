using Rayiah.Objects.Attributes;
using Rayiah.Objects.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rayiah.Tests
{
    public class TestMain : ISave
    {
        [Save]
        public Dictionary<int, string> dictionary = new Dictionary<int, string>();
        [Save]
        public int[] array = new int[] { 1, 2, 3, 4, 7};

        public TestMain()
        {
            ISave.AddObject(this);
            Console.WriteLine("[TESTS ACTIVATED]");
            OnStart();
        }

        public void OnStart()
        {
            byte[] bytes = new byte[] {0, 23, 55};

            dictionary.Add(1, "test");
            ISave.SaveClassesValues();
        }

        public class CustomIEnumerator<T>
        {
            int index = 0;
            readonly T[] variables;

            public T Current { get => variables[index]; }

            public CustomIEnumerator(T[] variables)
            {
                this.index = 0;
                this.variables = variables;
            }

            public bool MoveForward()
            {
                if (index < variables.Length)
                {
                    index++;
                    return true;
                }
                return false;
            }
        }

        public delegate (bool, string) GetInfo(string t);
        public event GetInfo InfoHandler;

        public async Task Cycle() => await Task.Delay(1000);

        public void Test()
        {
            InfoHandler += Test2;
            InfoHandler += Test3;
            InfoHandler += Test4;

            do
            {
                var x = InfoHandler.GetInvocationList();
                foreach (Delegate d in x)
                {
                    var (X, Y) = (d as GetInfo).Invoke("T");
                    Console.WriteLine(X + "/" + Y);
                }

                Cycle().GetAwaiter().GetResult();
                Console.WriteLine("////////////////////");
            } while (true);
        }

        public (bool, string) Test2(string t) => (true, "Bruh");

        public (bool, string) Test3(string t) => (false, t);

        public (bool, string) Test4(string t) => (true, "Works");
    }
}