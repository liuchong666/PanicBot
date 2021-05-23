using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace ConsoleApp1
{
    public class Config
    {
        private static IConfiguration configuration;
        public static User ConfigInit()
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());
            build.AddJsonFile("config.json", true, true);
            var config = build.Build();
            configuration = config;
            var name = config["name"];
            var password = config["password"];
            var dprice = decimal.Parse(config["DlowP"]);
            var uprice = decimal.Parse(config["DupP"]);
            var count = int.Parse(config["Count"]);
            ConfigModel.MinuteValue = int.Parse(config["MinuteValue"]);

            User user = new User
            {
                Name = name,
                Password = password,
                DlowP = dprice,
                DupP = uprice,
                Count = count,
            };

            return user;
        }

        public static PanicBot ServiceInit()
        {
            var serviceProvider = new ServiceCollection()
                .AddHttpClient()
                .AddLogging(loggingBuilder => {
                    loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
                    //loggingBuilder.AddConsole(); // 将日志输出到控制台
                    loggingBuilder.ClearProviders();
                    //loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                    loggingBuilder.AddNLog(configuration);
                })
                .AddScoped(typeof(PanicBot))
                .BuildServiceProvider();

            var panicBot = serviceProvider
                 .GetService<PanicBot>();

            return panicBot;
        }
    }

    public class ConfigModel
    {
        public static int MinuteValue { get; set; }
    }
}
