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
                users.AddRange(item.Users);
            }

            var us = new List<UserService>();
            foreach (var item in users)
            {
                var u = new UserService
                {
                    UserInfo = item
                };

                us.Add(u);
            }

            var proxyUrl = "http://api2.uuhttp.com:39002/index/api/return_data?mode=http&count=10&return_type=2&line_break=6&secert=MTM4MTM5MDA1NTI6NTdiYTE3MmE2YmUxMjVjY2EyZjQ0OTgyNmY5OTgwY2E=";
            var client = httpClientFactory.CreateClient("common");
            
            

            List<string> listip = new List<string>();
            while (listip.Count<us.Count+2)
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
                    catch (Exception)
                    {
                    }
                }
            }


            for (int i = 0; i < us.Count; i++)
            {
                us[i].IP = listip[i];
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
            Console.WriteLine("主线程执行！");
            Console.WriteLine("主线程结束！");
            Console.ReadKey();
        }
    }
}
