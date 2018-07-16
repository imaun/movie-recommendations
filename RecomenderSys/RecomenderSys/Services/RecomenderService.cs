using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using RecomenderSys.Models;

namespace RecomenderSys.Services
{
    public class RecomenderService
    {
        #region Properties
        public string DataSetFilePath { get; set; }

        public List<Entity> InputFiles { get; set; } 


        #endregion
        public RecomenderService() { }
  
        public RecomenderService(string sourcePath)
        {
            DataSetFilePath = sourcePath;
            InputFiles = FindTags(DataSetFilePath);
        }

        #region Map Parts of Speechs
        private string[] tag_list = new string[]
            {

            };

        private const float tag_weight = 1;

        private string[] noun_pharse_list = new string[]
        {

            "NP",
            "WHNP",


        };

        private const float noun_pharse_weight = (float)0.8;

        private string[] proper_noun_list = new string[]
        {
    "NNP",
    "NNPS"

        };

        private const float proper_noun_weight = (float)0.8;

        private string[] phrasal_verb_list = new string[]
        {

        };

        private const float phrasal_verb_weight = (float)0.8;

        private string[] noun_list = new string[]
        {
            "NN",
            "NNS"

        };

        private const float noun_weight = (float)1;

        private string[] verb_list = new string[]
        {

            "VB",
            "VBD",
            "VBG",
            "VBN",
            "VBP",
            "VBZ"
        };

        private const float verb_weight = (float)0.7;

        private string[] adjective_list = new string[]
        {
            "ADJP",
            "WHADJP",
            "JJ",
            "JJR",
            "JJS"
        };

        private const float adjective_weight = (float)0.5;

        private string[] adverb_list = new string[]
        {
            "RB",
            "RBR",
            "RBS",
            "WRB"

        };

        private const float adverb_weight = (float) 0.2;


        #endregion

        public List<Models.Entity> Process(Entity entity)
        {
            entity = InputFiles.FirstOrDefault(x => x.Title.ToLower() == entity.Title.ToLower());
            //var files = FindTags(DataSetFilePath);
            var result = new List<Models.Entity>();

            foreach (var file in InputFiles)
            {
                result.Add(WeightWords(file));
            }
            
            //IDF Calculation
            var files_count = result.Count;
            foreach (var file in result)
            {
                foreach (var word in file.Words)
                {
                    word.CountInFiles = CountWordInFiles(result, word);
                    word.InversDocumentFrequency = Math.Log10(files_count/(double) word.CountInFiles);
                }
                
            }

            entity = result.FirstOrDefault(x => x.RowNumber == entity.RowNumber);

            foreach (var file in result)
            {
                if(entity.Filename.ToLower() == file.Filename.ToLower())
                    continue;
                file.Similarity = Similarity(entity, file);
            }

            return result.OrderByDescending(x=> x.Similarity).ToList();
        }
        private double Similarity(Entity file1, Entity file2)
        {
            double s1 = 0d;

            if (file1.Words.Count > file2.Words.Count)
            {
                int i = 0;
                foreach (var word in file1.Words)
                {
                    if(i < file2.Words.Count)
                        s1 += (word.FeatureScore*file2.Words[i].FeatureScore);
                    else
                        break;
                    i++;
                }
            }
            else
            {
                int i = 0;
                foreach (var word in file2.Words)
                {
                    if (i < file1.Words.Count)
                        s1 += (word.FeatureScore * file1.Words[i].FeatureScore);
                    else
                        break;
                    i++;
                }
            }

            double file1_s2 = 0d;
            foreach (var word in file1.Words)
            {
                file1_s2 += Math.Pow(word.FeatureScore, 2);
            }
            file1_s2 = Math.Sqrt(file1_s2);

            double file2_s2 = 0d;
            foreach (var word in file2.Words)
            {
                file2_s2 += Math.Pow(word.FeatureScore, 2);
            }
            file2_s2 = Math.Sqrt(file2_s2);

            var result = s1/(file1_s2*file2_s2);

            return result;
        }
        private int CountWordInFiles(List<Entity> files , Word word)
        {
            int counter = 0;
            foreach (var file in files)
            {
                if (file.Words.Any(x => x.Content.ToLower() == word.Content.ToLower()))
                {
                    counter++;
                    continue;
                }
            }
            return counter;
        }
        public List<Entity> FindTags(string sourcePath)
        {
            List<Entity> result = new List<Entity>();
            if (string.IsNullOrEmpty(sourcePath))
            {
                sourcePath = DataSetFilePath;
            }
            int i = 1;
            foreach (var file in Directory.GetFiles(sourcePath, "*.pos"))
            {
                using (var reader = new StreamReader(file))
                {
                    var entity = new Entity
                    {
                        Filename = file,
                        Words = SplitWords(reader.ReadToEnd())
                    };
                    try
                    {
                        var str_id = file.Split('-')[0];
                        var id = int.Parse(str_id);
                        entity.RowNumber = id;
                    }
                    catch
                    {
                        entity.RowNumber = i;
                    }
                    result.Add(entity);
                    i++;
                }
            }

            return result;
        }

        public List<Word> SplitWords(string contents)
        {
            List<Word> result = new List<Word>();
            var space_splited = contents.Split(' ');

            foreach (var s in space_splited)
            {
                if (s == Environment.NewLine)
                    continue;
                try
                {
                    var w = s.Split('/');
                    var word = new Word
                    {
                        Part = w[1],
                        Content = w[0]
                    };
                    result.Add(word);
                }
                catch (Exception)
                {
                }
                
            }

            return result;
        }
        public Entity WeightWords(Entity file)
        {
            var words = file.Words;
            var result = new Entity
            {
                Filename = file.Filename,
                RowNumber = file.RowNumber
            };
            var tagWords = new List<Word>();
            var naghes = new List<string>();


            var stopWords = LoadStopWords();
            result.Words = new List<Word>();
            foreach (var word in words)
            {
                var _word = new Word
                {
                    Part = word.Part,
                    Content = word.Content,
                    Weight = word.Weight
                };
                if(stopWords.Exists(x=> x.ToLower() == _word.Content.ToLower()))
                    continue;

                _word.Weight = GetWordWeight(_word);
                _word.TermFrequency = words.Count(x => x.Content.ToLower() == _word.Content.ToLower());
                result.Words.Add(_word);
            }

            return result;
        }
        private float GetWordWeight(Word word)
        {
            float result = 0;

            if (!string.IsNullOrEmpty(
                    tag_list.FirstOrDefault(x => x.ToLower() ==
                    word.Part.ToLower())))
            {
                result = tag_weight;
            }

            else if (!string.IsNullOrEmpty(
                proper_noun_list.FirstOrDefault(x => x.ToLower() ==
                word.Part.ToLower())))
            {
                result = proper_noun_weight;
            }

            else if (!string.IsNullOrEmpty(
                noun_pharse_list.FirstOrDefault(x => x.ToLower() ==
                word.Part.ToLower())))
            {
                result= noun_pharse_weight;
            }

            else if (!string.IsNullOrEmpty(
                phrasal_verb_list.FirstOrDefault(x => x.ToLower() ==
                word.Part.ToLower())))
            {
                result = phrasal_verb_weight;
            }

            else if (!string.IsNullOrEmpty(
                noun_list.FirstOrDefault(x => x.ToLower() ==
                word.Part.ToLower())))
            {
                result  = noun_weight;
            }

            else if (!string.IsNullOrEmpty(
                verb_list.FirstOrDefault(x => x.ToLower() ==
                word.Part.ToLower())))
            {
                result = verb_weight;
            }

            if (!string.IsNullOrEmpty(
                verb_list.FirstOrDefault(x => x.ToLower() ==
                word.Part.ToLower())))
            {
                result = verb_weight;
            }

            if (!string.IsNullOrEmpty(
                adverb_list.FirstOrDefault(x => x.ToLower() ==
                word.Part.ToLower())))
            {
                result = adverb_weight;
            }

            if (!string.IsNullOrEmpty(
               adjective_list.FirstOrDefault(x => x.ToLower() ==
               word.Part.ToLower())))
            {
                result = adjective_weight;
            }

            return result;
        }
        private List<string> LoadStopWords()
        {
            var result = new List<string>();
            var stopWordsPath = Application.StartupPath + "\\stopwords.txt";

            using (var reader = new StreamReader(stopWordsPath))
            {
                while (!reader.EndOfStream)
                {
                    result.Add(reader.ReadLine());
                }
            }


            return result;
        }
    }
}
