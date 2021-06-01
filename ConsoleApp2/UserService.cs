using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public class UserService
    {
        public User UserInfo { get; set; }

        public string IP { get; set; }

        public string Token { get; set; }

        public int GroupId { get; set; }

        public Goods GoodsId { get; set; }

        public bool IsSuccess { get; set; }

        public string Login(IHttpClientFactory httpClientFactory, string name)
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
            UserInfo.Id = result.data.id;
            httpContent.Dispose();

            Console.WriteLine($"用户：{UserInfo.Name}登录成功{DateTime.Now.ToString("yyyy-MM-dd:hh mm ss fff")}\r\n");

            return token;
        }

        public int GetSessionList(IHttpClientFactory httpClientFactory)
        {
            var client = httpClientFactory.CreateClient("common");
            client.DefaultRequestHeaders.Add("Authorization", Token);
            var resultAuth = client.GetAsync($"http://api.muyunzhaig.com/rotation/rotationList").Result;
            if (resultAuth.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //_logger.LogError(resultAuth.Content.ReadAsStringAsync().Result);
                Console.WriteLine($"获取Session请求失败{resultAuth.StatusCode}");
            }

            var resultStr = resultAuth.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<dynamic>(resultStr);
            if (result.code != 0)
            {
                //_logger.LogError(resultStr);
                //_logger.LogInformation($"获取Session请求失败：{result.message.Value}");
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
                //_logger.LogInformation($"获取{startTime}Session请求成功SessionId：{result.data.sessionList[0].id}-{DateTime.Now}\r\n");
                return result.data.sessionList[0].id;
            }

            //_logger.LogInformation($"获取{startTime}Session请求成功SessionId：{result.data.sessionList[1].id}-{DateTime.Now}\r\n");
            return result.data.sessionList[1].id;
        }

        public string GetList(IHttpClientFactory httpClientFactory, int sessionId, int page, List<Goods> list)
        {
            //sessionId = 39;
            var msg = "当前共有{0}件商品\r\n";
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", Token);
            var resultAuth = client.GetAsync($"http://api.muyunzhaig.com/sg/getList/{sessionId}/{page}/12?null").Result;
            if (resultAuth.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //_logger.LogError(resultAuth.Content.ReadAsStringAsync().Result);
                Console.WriteLine($"获取商品列表请求失败{resultAuth.StatusCode}");
            }

            var resultStr = resultAuth.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<dynamic>(resultStr);
            if (result.code != 0)
            {
                //_logger.LogError(resultStr);
                Console.WriteLine($"获取商品列表请求失败：{result.message}");
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
                GetList(httpClientFactory, sessionId, page + 1, list);
            }

            //list = list.Where(c => c.GoodBuyPrice >= UserInfo.DlowP && c.GoodBuyPrice <= UserInfo.DupP && c.UserId != UserInfo.Id).ToList();
            msg = string.Format(msg, records/*, list.Count, UserInfo.DlowP, UserInfo.DupP*/);

            //_logger.LogInformation(msg);

            return msg;
        }

        public async Task DoOrder(List<Group> groups)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                Proxy = new WebProxy($"http://{IP}"),
                UseProxy = true,
            };

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("Authorization", Token);
            do
            {
                var group = groups.Where(c => c.GroupId == GroupId).FirstOrDefault();
                if (group.Queue.Count <= 0)
                {
                    Console.WriteLine($"用户：{UserInfo.Name}商品价格区间{group.DlowP}-{group.DupP}已经抢购完！{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}");
                    break;
                }
                Console.WriteLine($"用户：{UserInfo.Name}开始抢购{GoodsId.GoodName}{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}\r\n");
                var resultAuth =await client.GetAsync($"http://api.muyunzhaig.com/order/createOrder/{GoodsId.Id}?null");
                if (resultAuth.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    //_logger.LogError(resultAuth.Content.ReadAsStringAsync().Result);
                    Console.WriteLine($"用户：{UserInfo.Name}下单请求失败{resultAuth.StatusCode}{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}\r\n");
                    continue;
                }

                var resultStr = resultAuth.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<dynamic>(resultStr);
                if (result.code == 0)
                {
                    Console.WriteLine($"用户：{UserInfo.Name}抢购{GoodsId.GoodName}成功{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}\r\n");
                    IsSuccess = true;
                    break;
                }
                else if (result.code == -10000)
                {
                    Console.WriteLine($"用户：{UserInfo.Name}抢购{GoodsId.GoodName}失败：{result.message.Value}{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}\r\n");
                    Thread.Sleep(300);
                }
                else
                {
                    Console.WriteLine($"用户：{UserInfo.Name}抢购{GoodsId.GoodName}失败：{result.message.Value}切换商品{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}\r\n");
                    

                    Goods gds;
                    while (!group.Queue.TryDequeue(out gds)&&group.Queue.Count>0)
                    {

                    }
                    if (group.Queue.Count == 0) continue;
                    GoodsId = gds;
                    Thread.Sleep(300);
                }
            } while (true);

        }
    }
}
