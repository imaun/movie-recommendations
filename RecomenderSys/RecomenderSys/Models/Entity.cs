using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RecomenderSys.Models
{
    public class Entity
    {
        public string Filename { get; set; }

        public string Title
        {
            get
            {
                try
                {
                    return Path.GetFileNameWithoutExtension(Filename);
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public List<Word> Words { get; set; } 

        public double Similarity { get; set; }

        public int RowNumber { get; set; }

    }
}
