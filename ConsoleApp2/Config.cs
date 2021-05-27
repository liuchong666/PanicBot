using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConsoleApp2
{
    public class Config
    {
        private static IConfiguration configuration;
        public static void ConfigInit()
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());
            build.AddJsonFile("config.json", true, true);
            var config = build.Build();
            configuration = config;
            //ConfigModel.MinuteValue = int.Parse(config["MinuteValue"]);
        }

        public static IServiceCollection ServiceInit()
        {
            IServiceCollection connection = new ServiceCollection()
                //.AddHttpClient()
                .AddLogging(loggingBuilder =>
                {
                    //loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
                    ////loggingBuilder.AddConsole(); // 将日志输出到控制台
                    //loggingBuilder.ClearProviders();
                    ////loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                    //loggingBuilder.AddNLog(configuration);
                })
                .Configure<List<Group>>(configuration.GetSection("Groups"));

            connection.AddHttpClient("common");
            return connection;
        }
    }

    public class ConfigModel
    {
        public static int MinuteValue { get; set; }
    }
}
