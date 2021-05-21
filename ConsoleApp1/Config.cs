using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConsoleApp1
{
    public class Config
    {
        public static User ConfigInit()
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

            return user;
        }

        public static PanicBot ServiceInit()
        {
            var serviceProvider = new ServiceCollection()
                .AddHttpClient()
                .AddLogging()
                .AddScoped(typeof(PanicBot))
                .BuildServiceProvider();

            var panicBot = serviceProvider
                 .GetService<PanicBot>();

            return panicBot;
        }
    }
}
