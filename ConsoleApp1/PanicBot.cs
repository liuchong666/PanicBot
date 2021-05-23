using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;

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

        public (string, long) Login(string userName, string password)
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
                _logger.LogInformation($"用户：{userName}登录失败，{result.message.Value}{DateTime.Now}\r\n");
                Console.WriteLine($"登录请求失败：{result.message}");
            }

            var token = result.data.token.Value;
            httpContent.Dispose();

            _logger.LogInformation($"用户：{userName}登录成功{DateTime.Now}\r\n");
            Console.WriteLine($"用户：{userName}登录成功{DateTime.Now}\r\n");
            return (token, result.data.id.Value);
        }

        public int GetSessionList(string token)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", token);
            var resultAuth = client.GetAsync($"http://api.muyunzhaig.com/rotation/rotationList").Result;
            if (resultAuth.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError(resultAuth.Content.ReadAsStringAsync().Result);
                Console.WriteLine($"获取Session请求失败{resultAuth.StatusCode}");
            }

            var resultStr = resultAuth.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<dynamic>(resultStr);
            if (result.code != 0)
            {
                _logger.LogError(resultStr);
                _logger.LogInformation($"获取Session请求失败：{result.message.Value}");
                Console.WriteLine($"获取Session请求失败：{result.message.Value}");
            }

            var startTimeValue = "";
            if ((DateTime.Now.Hour > 11) && (DateTime.Now.Hour < 15))
                startTimeValue = "15:00";
            else
            {
                startTimeValue = "11:00";
            }

            var startTime = result.data.sessionList[0].startTime.Value;
            if (startTimeValue == startTime)
            {
                _logger.LogInformation($"获取{startTime}Session请求成功SessionId：{result.data.sessionList[0].id}-{DateTime.Now}\r\n");
                return result.data.sessionList[0].id;
            }
            
            _logger.LogInformation($"获取{startTime}Session请求成功SessionId：{result.data.sessionList[1].id}-{DateTime.Now}\r\n");
            return result.data.sessionList[1].id;
        }

        public (List<Goods>, string) GetList(string token, int sessionId, int page, List<Goods> list, User user)
        {
            //sessionId = 39;
            var msg = "当前共有{0}件商品，筛选后可抢商品为：{1}件\r\n";
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", token);
            var resultAuth = client.GetAsync($"http://api.muyunzhaig.com/sg/getList/{sessionId}/{page}/12?null").Result;
            if (resultAuth.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError(resultAuth.Content.ReadAsStringAsync().Result);
                Console.WriteLine($"获取商品列表请求失败{resultAuth.StatusCode}");
            }

            var resultStr = resultAuth.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<dynamic>(resultStr);
            if (result.code != 0)
            {
                _logger.LogError(resultStr);
                Console.WriteLine($"获取Session请求失败：{result.message}");
            }

            var records = result.data.totalRecords.Value;
            var pages = result.data.totalPages.Value;


            foreach (var item in result.data.currentList)
            {
                var id = item.id.Value;
                var goodsName = item.goodName.Value;
                var goodsPrice = item.goodPrice;
                var userId = item.userId.Value;

                Goods goods = new Goods()
                {
                    GoodBuyPrice = goodsPrice,
                    GoodName = goodsName,
                    Id = id,
                    UserId = userId
                };
                list.Add(goods);
            }

            if (page < pages)
            {
                GetList(token, sessionId, page + 1, list, user);
            }

            list = list.Where(c => c.GoodBuyPrice > user.DlowP && c.GoodBuyPrice <= user.DupP && c.UserId != user.Id).ToList();
            msg = string.Format(msg, records, list.Count);
            
            _logger.LogInformation($"共获取{records}件商品，获取可抢商品{list.Count}件-{DateTime.Now}\r\n");

            return (list, msg);
        }

        public Goods GetGoods(string token, long goodsId)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", token);
            var resultAuth = client.GetAsync($"http://api.muyunzhaig.com/sg/getSgInfo/{goodsId}?nul").Result;
            if (resultAuth.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError(resultAuth.Content.ReadAsStringAsync().Result);
                Console.WriteLine($"获取商品详情请求失败{resultAuth.StatusCode}");
            }

            var resultStr = resultAuth.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<dynamic>(resultStr);
            if (result.code == 0)
            {
                Goods goods = new Goods
                {
                    IsSale = result.data.isSale,
                    GoodsState = result.data.goodState,
                };

                return goods;
            }

            return null;
        }

        public (bool, string) DoOrder(string token, long goodsId)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", token);
            var resultAuth = client.GetAsync($"http://api.muyunzhaig.com/order/createOrder/{goodsId}?null").Result;
            if (resultAuth.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError(resultAuth.Content.ReadAsStringAsync().Result);
                Console.WriteLine($"下单请求失败{resultAuth.StatusCode}");
            }

            var resultStr = resultAuth.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<dynamic>(resultStr);
            if (result.code == 0)
            {
                Console.WriteLine($"抢购成功");
                return (true, "");
            }

            return (false, result.message.Value);
        }
    }
}
