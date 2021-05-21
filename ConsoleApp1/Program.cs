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
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());
            build.AddJsonFile("config.json", true, true);
            var config = build.Build();
            var name = config["name"];
            var password = config["password"];
            var dprice = decimal.Parse(config["DlowP"]);
            var uprice = decimal.Parse(config["DupP"]);
            var count = int.Parse(config["Count"]);

            User user = new User
            {
                Name = name,
                Password = password,
                DlowP = dprice,
                DupP = uprice,
                Count = count
            };

            var serviceProvider = new ServiceCollection()
                .AddHttpClient()
                .AddLogging()
                .AddScoped(typeof(PanicBot))
                .BuildServiceProvider();

            var panicBot = serviceProvider
                 .GetService<PanicBot>();

            var token = panicBot.Login(user.Name, user.Password);
            user.Token = token.Item1;
            user.Id = token.Item2;

            var sessionId = panicBot.GetSessionList(user.Token);

            List<Goods> list = new List<Goods>();
            var allGoods=panicBot.GetList(user.Token, sessionId, 1, list,user);
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
