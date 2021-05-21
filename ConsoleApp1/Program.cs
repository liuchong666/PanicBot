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

            var sessionId = panicBot.GetSessionList(user.Token);

            List<Goods> list = new List<Goods>();
            var allGoods = panicBot.GetList(user.Token, sessionId, 1, list, user);
            Console.WriteLine(allGoods.Item2);
            
            while (true)
            {
                int hour = DateTime.Now.Hour;
                int minute = DateTime.Now.Minute;
                int second = DateTime.Now.Second;
                if ((hour == 10 || hour == 14) && minute > 55 && second > 54)
                {
                    break;
                }

                Console.WriteLine($"当前时间为 {hour}时{minute}分{second}秒, 在{hour}:56:54开启抢购商品!\r\n");
                Thread.Sleep(1000);
            }

            bool resultF=false;
            int i = 0;
            do
            {
                if (i >= list.Count)
                {
                    Console.WriteLine("商品已抢购完，无可抢商品！");
                    break;
                }

                long goodsId = list[i].Id;
                Console.WriteLine($"开始抢购商品：{list[i].GoodName}");

                var goods = panicBot.GetGoods(user.Token, goodsId);
                if (goods.GoodsState != 0&&goods.IsSale)
                {
                    i++;
                    continue;
                }

                var result = panicBot.DoOrder(user.Token, goodsId);
                resultF = result.Item1;
                if (resultF)
                {
                    Console.WriteLine("抢购成功！");
                    break;
                }
                else
                {
                    Console.WriteLine($"抢购失败：{result.Item2}");
                    i++;
                }
                Thread.Sleep(300);
            } while (!resultF);

            Console.ReadKey();
        }
    }
}
