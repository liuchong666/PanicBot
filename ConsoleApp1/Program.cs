using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddHttpClient()
                .AddLogging()
                .AddScoped(typeof(PanicBot))
                //.AddSingleton<IFooService, FooService>()
                //.AddSingleton<IBarService, BarService>()
                .BuildServiceProvider();

            Console.WriteLine("请输入登录用户");
            var name = Console.ReadLine();

            Console.WriteLine("请输入登录密码");
            var pwd = Console.ReadLine();

            var panicBot = serviceProvider
                 .GetService<PanicBot>();

            var token = panicBot.Login(name, pwd);
            //var logger = serviceProvider.GetService<ILoggerFactory>()
            //    .CreateLogger<Program>();
            Console.WriteLine(token);
        }
    }
}
