using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecomenderSys.Models
{
    public class Word
    {
        public string Content { get; set; }

        public string Part { get; set; }

        public float Weight { get; set; }

        public int TermFrequency { get; set; }

        public double InversDocumentFrequency { get; set; }

        public int CountInFiles { get; set; }

        public double TermScore
        {
            get { return TermFrequency*InversDocumentFrequency; }
        }

        public double FeatureScore
        {
            get { return TermScore*Weight; }
        }
    }
}
