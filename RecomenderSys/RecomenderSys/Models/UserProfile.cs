using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace RecomenderSys.Models
{
    public class UserProfile
    {
        public string Name { get; set; }

        public List<int> History { get; set; }

        public List<PairValue> PairVideos { get; set; } 

        public UserProfile ()
        {
            History = new List<int>();
            PairVideos = new List<PairValue>();
        }
    }

    public class PairValue
    {
        public int Key { get; set; }

        public int Value { get; set; }

        public int Score { get; set; }
    }

    public class MapUserVideo
    {
        public UserProfile User { get; set; }

        public Entity Video { get; set; }
    }

    public class Map2Reduce
    {
        public List<PairValue> PairVideos { get; set; }
        
        //public int Score { get; set; }

        public Map2Reduce()
        {
            PairVideos = new List<PairValue>();
        }
    }
}
