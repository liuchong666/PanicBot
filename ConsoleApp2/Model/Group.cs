using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2
{
    public class Group
    {
        public int GroupId { get; set; }

        public decimal DlowP { get; set; }

        public decimal DupP { get; set; }

        public List<User> Users { get; set; }

        public ConcurrentQueue<Goods> Queue { get; set; } = new ConcurrentQueue<Goods>();
    }

    public class GroupsUser
    {
        public List<Group> Groups { get; set; }
    }
}
