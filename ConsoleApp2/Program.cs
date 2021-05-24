using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            //ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(100, 100);

            ThreadPool.GetAvailableThreads(out int at, out int aot);
            //ThreadPool.GetMaxThreads(out int mt, out int mot);

            Parallel.For(1, 60, async i =>
            {
                Console.WriteLine(string.Format("{0}:第{1}个线程{2}", DateTime.Now.ToString("yyyy-MM-dd:hh mm ss fff"), i.ToString(), Thread.CurrentThread.ManagedThreadId));
                await Task.Delay(5000);
                Console.WriteLine("-------------------");
            });

            //for (int i = 1; i <= 99; i++)
            //{
            //    ThreadPool.QueueUserWorkItem(new WaitCallback(testFun), i.ToString());
            //}
            Console.WriteLine("主线程执行！");
            Console.WriteLine("主线程结束！");
            Console.ReadKey();
        }

        public static void testFun(object obj)
        {
            Console.WriteLine(string.Format("{0}:第{1}个线程", DateTime.Now.ToString("yyyy-MM-dd:hh mm ss fff"), obj.ToString()));
            Thread.Sleep(300);
        }

    }
}
