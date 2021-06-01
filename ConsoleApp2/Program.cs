using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            Config.ConfigInit();
            IServiceCollection services = Config.ServiceInit();

            var serviceProvider = services.BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            var groups = serviceProvider.GetService<IOptions<List<Group>>>().Value;

            var users = new List<User>();
            foreach (var item in groups)
            {
                foreach (var user in item.Users)
                {
                    user.GroupId = item.GroupId;
                    users.Add(user);
                }
            }

            var us = new List<UserService>();
            foreach (var item in users)
            {
                var u = new UserService
                {
                    GroupId = item.GroupId,
                    UserInfo = item
                };

                us.Add(u);
            }

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

            var proxyUrl = "http://api2.uuhttp.com:39002/index/api/return_data?mode=http&count=10&return_type=2&line_break=6&secert=MTM4MTM5MDA1NTI6NTdiYTE3MmE2YmUxMjVjY2EyZjQ0OTgyNmY5OTgwY2E=";
            var client = httpClientFactory.CreateClient("common");



            List<string> listip = new List<string>();
            while (listip.Count < us.Count + 2)
            {
                var ipStr = client.GetAsync(proxyUrl).Result.Content.ReadAsStringAsync().Result;
                var resultIPs = JsonConvert.DeserializeObject<dynamic>(ipStr);

                foreach (var item in resultIPs)
                {
                    try
                    {
                        var ip = $"{item.ip.Value}:{item.port.Value}";
                        HttpClientHandler handler = new HttpClientHandler()
                        {
                            Proxy = new WebProxy($"http://{ip}"),
                            UseProxy = true,
                        };

                        var client1 = new HttpClient(handler);
                        client1.Timeout = TimeSpan.FromMilliseconds(300);

                        var watch = new Stopwatch();
                        watch.Start();
                        var task = client1.GetAsync("http://api.muyunzhaig.com").Result.Content.ReadAsStringAsync().Result;
                        watch.Stop();
                        var responseTimeForCompleteRequest = watch.ElapsedMilliseconds;

                        if (responseTimeForCompleteRequest < 300)
                        {
                            listip.Add(ip);
                        }

                    }
                    catch (Exception ex)
                    {
                    }
                }
            }


            for (int i = 0; i < us.Count; i++)
            {
                us[i].IP = listip[i];
                listip.RemoveAt(i);
            }

            var result = Parallel.ForEach(us, async u =>
              {
                  Console.WriteLine(string.Format("{0}:线程ID{1}", DateTime.Now.ToString("yyyy-MM-dd:hh mm ss fff"), Thread.CurrentThread.ManagedThreadId));
                  u.Token = u.Login(httpClientFactory, "common");
                  await Task.Delay(300);
                  Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd:hh mm ss fff")}-------------------");
              });

            if (result.IsCompleted)
            {
                Console.WriteLine($"登录成功用户数：{us.Where(c => !string.IsNullOrEmpty(c.Token)).Count()}");
            }

            WaitForGetAllGoods();
            //获取去商品列表
            int sessionId = us[0].GetSessionList(httpClientFactory);
            List<Goods> goods = new List<Goods>();
            var msg = us[0].GetList(httpClientFactory, sessionId, 1, goods);
            Console.WriteLine(msg);

            var queue = new ConcurrentBag<Goods>();
            foreach (var item in goods)
            {
                queue.Add(item);
            }

            //商品分组
            foreach (var group in groups)
            {
                var groupGoods = goods.Where(c => c.GoodBuyPrice >= group.DlowP && c.GoodBuyPrice < group.DupP).ToList();
                if (groupGoods.Count < group.Users.Count)
                {
                    Console.WriteLine($"商品价格区间在{group.DlowP}-{group.DupP}共有{groupGoods.Count}件，共有{group.Users.Count}用户抢购！");
                    return;
                }

                foreach (var item in groupGoods)
                {
                    group.Queue.Enqueue(item);
                }

                var usgroup = us.Where(c => c.GroupId == group.GroupId).ToList();
                foreach (var item in usgroup)
                {
                    Goods gds;
                    while (!group.Queue.TryDequeue(out gds))
                    {
                        
                    }
                    item.GoodsId = gds;
                }
            }

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

            var result1 = Parallel.ForEach(us, async u =>
            {
                //Console.WriteLine(string.Format("{0}:线程ID{1}", DateTime.Now.ToString("yyyy-MM-dd:hh mm ss fff"), Thread.CurrentThread.ManagedThreadId));
               await u.DoOrder(groups);
            });

            if (result1.IsCompleted)
            {
                var fails = us.Where(c => !c.IsSuccess).ToList();
                foreach (var item in fails)
                {
                    Console.WriteLine($"用户：{item.UserInfo.Name}抢购失败");
                }
                Console.WriteLine($"抢购完成");
            }
            Console.WriteLine("主线程执行！");
            Console.WriteLine("主线程结束！");
            Console.ReadKey();
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
