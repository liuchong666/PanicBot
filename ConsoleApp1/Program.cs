using NLog;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("程序启动");
            while (true)
            {
                User user;
                PanicBot panicBot;
                while (true)
                {
                    if ((DateTime.Now.Hour == 10 || DateTime.Now.Hour == 14) && DateTime.Now.Minute > 45)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"程序等待中，在10点或14点46分用户将登录{DateTime.Now}\r\n");
                        if ((DateTime.Now.Hour == 10 || DateTime.Now.Hour == 14) && DateTime.Now.Minute >= 40)
                        {
                            Thread.Sleep(60 * 1000);
                        }
                        else
                        {
                            Thread.Sleep(60 * 1000 * 5);
                        }
                    }
                }

                user = Config.ConfigInit();

                panicBot = Config.ServiceInit();

                var token = panicBot.Login(user.Name, user.Password);
                user.Token = token.Item1;
                user.Id = token.Item2;

                //var sessionId = panicBot.GetSessionList(user.Token);

                //List<Goods> list = new List<Goods>();
                //var allGoods1 = panicBot.GetList(user.Token, sessionId, 1, list, user);
                //Console.WriteLine(allGoods1.Item2);

                WaitForGetAllGoods();

                (List<Goods>, string) allGoods = WaitForDoOrder(user, panicBot);
                logger.Info(allGoods.Item2);

                BeginDoOrder(user, panicBot, allGoods,logger);

                Thread.Sleep(5*60*1000);
            }
        }

        private static void BeginDoOrder(User user, PanicBot panicBot, (List<Goods>, string) allGoods,Logger logger)
        {
            Random random = new Random();
            bool resultF = false;
            int i = random.Next(0, allGoods.Item1.Count);
            do
            {
                if (allGoods.Item1.Count <= 0)
                {
                    Console.WriteLine("商品已抢购完，无可抢商品！\r\n");
                    break;
                }

                long goodsId = allGoods.Item1[i].Id;
                Console.WriteLine($"开始抢购商品：{allGoods.Item1[i].GoodName}-{DateTime.Now.ToString("yyyy-MM-dd hh mm ss fff")}\r\n");

                //var goods = panicBot.GetGoods(user.Token, goodsId);
                //if (goods.GoodsState != 0&&goods.IsSale)
                //{
                //    i++;
                //    continue;
                //}

                var result = panicBot.DoOrder(user.Token, goodsId);
                resultF = result.Item1;
                if (resultF)
                {
                    logger.Info($"用户：{user.Name}抢购商品：{allGoods.Item1[i].GoodName}成功");
                    Console.WriteLine("抢购成功！\r\n");
                    break;
                }
                else
                {
                    logger.Info($"用户：{user.Name}抢购商品：{allGoods.Item1[i].GoodName}失败,原因：{result.Item2}");
                    Console.WriteLine($"抢购失败：{result.Item2}\r\n");
                    if (result.Item2 != "未开场"&&result.Item2!= "请求超时")
                    {
                        allGoods.Item1.RemoveAt(i);
                        i = random.Next(0, allGoods.Item1.Count);
                    }
                }
                Thread.Sleep(300);
            } while (!resultF);
        }

        private static (List<Goods>, string) WaitForDoOrder(User user, PanicBot panicBot)
        {
            var sessionId = panicBot.GetSessionList(user.Token);

            List<Goods> list = new List<Goods>();
            var allGoods = panicBot.GetList(user.Token, sessionId, 1, list, user);
            Console.WriteLine(allGoods.Item2);

            while (true)
            {
                int hour = DateTime.Now.Hour;
                int minute = DateTime.Now.Minute;
                int second = DateTime.Now.Second;
                if ((hour == 10 || hour == 14) && minute > 55 && second > 58)
                {
                    break;
                }

                Console.WriteLine($"当前时间为 {hour}时{minute}分{second}秒, 在{hour}:56:59开启抢购商品!\r\n");
                Thread.Sleep(1000);
            }

            return allGoods;
        }

        private static void WaitForGetAllGoods()
        {
            int minuteValue = ConfigModel.MinuteValue - 1;
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
        }
    }
}
