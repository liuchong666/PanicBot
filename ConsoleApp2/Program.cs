using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

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

            var result= Parallel.ForEach(us, async u =>
             {
                 Console.WriteLine(string.Format("{0}:线程ID{1}", DateTime.Now.ToString("yyyy-MM-dd:hh mm ss fff"),  Thread.CurrentThread.ManagedThreadId));
                 u.Token=u.Login(httpClientFactory);
                 await Task.Delay(300);
                 Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd:hh mm ss fff")}-------------------");
             });

            if (result.IsCompleted)
            {
                Console.WriteLine($"登录成功用户数：{us.Where(c=>!string.IsNullOrEmpty(c.Token)).Count()}");
            }
            Console.WriteLine("主线程执行！");
            Console.WriteLine("主线程结束！");
            Console.ReadKey();
        }
    }
}
