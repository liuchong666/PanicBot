using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    public class User
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        public string Token { get; set; }

        public decimal DlowP { get; set; }

        public decimal DupP { get; set; }

        public int Count { get; set; }
    }

    //public class Session
    //{
    //    public int Code { get; set; }

    //}

    public class Goods
    {
        public long Id { get; set; }

        public decimal GoodBuyPrice { get; set; }

        public string GoodName { get; set; }

        public long UserId { get; set; }
    }
}
