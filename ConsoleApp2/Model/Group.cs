using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2
{
    public class Group
    {
        public decimal DlowP { get; set; }

        public decimal DupP { get; set; }

        public List<User> Users { get; set; }
    }

    public class GroupsUser
    {
        public List<Group> Groups { get; set; }
    }
}
