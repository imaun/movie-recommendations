using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RecomenderSys.Models;

namespace RecomenderSys.Services
{
    public class MapReduceService
    {
        #region Properties
        public string DataSetFilePath { get; set; }

        public string UserProfilesPath { get; set; }

        public List<Entity> Films { get; set; }

        public List<UserProfile> UserProfiles { get; set; } 



        #endregion

        public MapReduceService(string datasetPath, string userProfilePath)
        {
            DataSetFilePath = datasetPath;
            UserProfilesPath = userProfilePath;
            Films = LoadFilms(datasetPath);
            UserProfiles = LoadUserProfiles(UserProfilesPath);
        }

        public List<Entity> MapReduce(Entity entity, int threshold)
        {
            var result = new List<Entity>();
            //Dictionary<Dictionary<int, int>, int> Map2 = new Dictionary<Dictionary<int, int>, int>();
            var map2 = new List<Map2Reduce>();

            foreach (var user in UserProfiles)
            {
                for (int i = 0; i < user.History.Count; i++)
                {
                    for (int j = 0; j < user.History.Count; j++)
                    {
                        if (j != i)
                            if(!user.PairVideos.Exists(x=> x.Key == user.History[i] 
                                && x.Value == user.History[j])
                                &&
                                !user.PairVideos.Exists(x=> x.Key == user.History[j]
                                && x.Value == user.History[i])
                                )
                                user.PairVideos.Add(new PairValue
                                {
                                    Key = user.History[i],
                                    Value = user.History[j]
                                });
                    }
                }

                //Map2.Add(user.PairVideos, 1);
                map2.Add(new Map2Reduce
                {
                    PairVideos = user.PairVideos,
                    //Score = 0
                });
                
            }

            var all_pairs = new List<PairValue>();
            foreach (var map in map2)
            {
                all_pairs.AddRange(map.PairVideos);
            }


            var distinct_pairs = new List<PairValue>();
            foreach (var pair in all_pairs)
            {
                if (!distinct_pairs.Exists(x => x.Key == pair.Key && x.Value == pair.Value)
                    &&
                    !distinct_pairs.Exists(x => x.Value == pair.Key && x.Key == pair.Value))
                {
                    pair.Score = all_pairs.Count(x => x.Key == pair.Key && x.Value == pair.Value
                                                      || x.Key == pair.Value && x.Value == pair.Key);
                    distinct_pairs.Add(pair);
                }
            }

            var pair_result = distinct_pairs.Where(x => x.Score >= threshold);


            pair_result = pair_result.Where(x => x.Key == entity.RowNumber ||
                                                 x.Value == entity.RowNumber
                ).ToList();

            foreach (var item in pair_result)
            {
                if (item.Key != entity.RowNumber)
                {
                    result.Add(Films.FirstOrDefault(x=> x.RowNumber == item.Key));
                }
                if (item.Value != entity.RowNumber)
                {
                    result.Add(Films.FirstOrDefault(x=> x.RowNumber == item.Value));
                }
            }


            return result;

        }

        public List<Entity> LoadFilms(string sourcePath)
        {
            List<Entity> result = new List<Entity>();
            if (string.IsNullOrEmpty(sourcePath))
            {
                sourcePath = DataSetFilePath;
            }
            int i = 1;
            foreach (var file in Directory.GetFiles(sourcePath, "*.txt"))
            {
                using (var reader = new StreamReader(file))
                {
                    var entity = new Entity
                    {
                        Filename = file,
                        RowNumber = i
                    };
                    result.Add(entity);
                }
                i++;
            }

            return result;
        }

        public List<UserProfile> LoadUserProfiles(string sourcePath)
        {
            List<UserProfile> result = new List<UserProfile>();
            if (string.IsNullOrEmpty(sourcePath))
            {
                sourcePath = DataSetFilePath;
            }
            int i = 1;
            foreach (var file in Directory.GetFiles(sourcePath, "*.txt"))
            {
                using (var reader = new StreamReader(file))
                {
                    var profile = new UserProfile
                    {
                        Name = new FileInfo(file).Name
                    };
                    while (!reader.EndOfStream)
                    {
                        try
                        {
                            var line = reader.ReadLine();
                            if(line != null)
                                profile.History.Add(int.Parse(line));
                        }
                        catch { }
                    }
                    result.Add(profile);
                }
                i++;
            }

            return result;
        } 
    }
}
