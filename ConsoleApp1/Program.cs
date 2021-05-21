using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var user = Config.ConfigInit();

            var panicBot = Config.ServiceInit();

            var token = panicBot.Login(user.Name, user.Password);
            user.Token = token.Item1;
            user.Id = token.Item2;

            var sessionId = panicBot.GetSessionList(user.Token);

            List<Goods> list = new List<Goods>();
            var allGoods = panicBot.GetList(user.Token, sessionId, 1, list, user);
            Console.WriteLine(allGoods.Item2);

            int minuteValue = 50;
            int secondValue = 50;
            while (true)
            {
                int hour = DateTime.Now.Hour;
                int minute = DateTime.Now.Minute;
                if ((hour == 10 || hour == 14) && minute > minuteValue)
                {
                    break;
                }

                Console.WriteLine($"当前时间为 {DateTime.Now.Hour}时{DateTime.Now.Minute}分{DateTime.Now.Second}秒, 在10:{minuteValue + 1}或14:{minuteValue + 1}开启拉取商品!\r\n");
                if ((hour == 10 || hour == 14) && minute >= minuteValue && DateTime.Now.Second >= secondValue)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    Thread.Sleep(10000);
                }
            }



            Console.ReadKey();
        }
    }
}
