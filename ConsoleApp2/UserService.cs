using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ConsoleApp2
{
    public class UserService
    {
        public User UserInfo { get; set; }

        public string IP { get; set; }

        public string Token { get; set; }

        public List<long> GoodsIds { get; set; }

        public string Login(IHttpClientFactory httpClientFactory,string name)
        {
            //var client = httpClientFactory.CreateClient(name);
            HttpClientHandler handler = new HttpClientHandler()
            {
                Proxy = new WebProxy($"http://{IP}"),
                UseProxy = true,
            };

            var client = new HttpClient(handler);
            var user = new { name = UserInfo.Name, password = UserInfo.Password };

            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(user));
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var resultAuth = client.PostAsync($"http://api.muyunzhaig.com/user/login/?null", httpContent).Result;//.Content.ReadAsStringAsync().Result/*.JsonDeserialize<dynamic>()*/;
            if (resultAuth.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"用户：{UserInfo.Name}登录请求失败{resultAuth.StatusCode}{DateTime.Now.ToString("yyyy-MM-dd:hh mm ss fff")}\r\n");
                return null;
            }

            var resultStr = resultAuth.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<dynamic>(resultStr);
            if (result.code != 0)
            {
                Console.WriteLine($"用户：{UserInfo.Name}登录请求失败：{result.message}{DateTime.Now.ToString("yyyy-MM-dd:hh mm ss fff")}\r\n");
                return null;
            }

            var token = result.data.token.Value;
            httpContent.Dispose();

            Console.WriteLine($"用户：{UserInfo.Name}登录成功{DateTime.Now.ToString("yyyy-MM-dd:hh mm ss fff")}\r\n");

            return token;
        }
    }
}
