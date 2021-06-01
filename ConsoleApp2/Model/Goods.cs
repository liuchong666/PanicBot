using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2
{
    public class Goods
    {
        public long Id { get; set; }

        public decimal GoodBuyPrice { get; set; }

        public string GoodName { get; set; }

        public long UserId { get; set; }

        public int GoodsState { get; set; }

        public bool IsSale { get; set; }
    }
}
