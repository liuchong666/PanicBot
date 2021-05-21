using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ConsoleApp1
{
    public class PanicBot
    {
        private readonly ILogger<PanicBot> _logger;
        private IHttpClientFactory _httpClientFactory;
        public PanicBot(ILogger<PanicBot> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public string Login(string userName, string password)
        {
            var client = _httpClientFactory.CreateClient();
            var user = new { name = userName, password = password };

            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(user));
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var resultAuth = client.PostAsync($"http://api.muyunzhaig.com/user/login/?null", httpContent).Result;//.Content.ReadAsStringAsync().Result/*.JsonDeserialize<dynamic>()*/;
            if (resultAuth.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError(resultAuth.Content.ReadAsStringAsync().Result);
                Console.WriteLine($"登录请求失败{resultAuth.StatusCode}");
            }

            var resultStr = resultAuth.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<dynamic>(resultStr);
            if (result.code != 0)
            {
                _logger.LogError(resultStr);
                Console.WriteLine($"登录请求失败：{result.message}");
            }

            var token = result.data.token;
            httpContent.Dispose();

            Console.WriteLine($"用户：{userName}登录成功");
            return token;
        }
    }
}
