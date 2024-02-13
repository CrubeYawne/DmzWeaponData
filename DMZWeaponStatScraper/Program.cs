using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using Newtonsoft.Json;

namespace DMZWeaponStatScraper
{
    class Program
    {
        public class StatSection
        {
            public StatName name;
            public int y;
            public StatSection(StatName name, int y)
            {
                this.name = name;
                this.y = y;
            }
        }

        [System.Serializable]
        private class SavedStat
        {
            public string filename;
            public Dictionary<StatName, float> stats;

            public SavedStat()
            {
                stats = new Dictionary<StatName, float>();
            }
        }

        public enum StatName {  Damage, FireRate, Range, Accuracy, RecoilControl, Mobility, Handling };

        const int STAT_START_X = 209;
        const int STAT_END_X = 447;

        const float minimum_valid_brightness = 0.5f;
        const float minimum_valid_green = 131.0f;
        const float minimum_invalid_red = 200;

        static StatSection[] sections = new StatSection[] { 
            new StatSection( StatName.Damage, 491) ,
            new StatSection( StatName.FireRate, 515) ,
            new StatSection( StatName.Range, 537) ,
            new StatSection( StatName.Accuracy, 561) ,
            new StatSection( StatName.RecoilControl, 583) ,
            new StatSection( StatName.Mobility, 606) ,
            new StatSection( StatName.Handling, 629) ,
        };

        static void Main(string[] args)
        {
            string filepath = args[0];

            if (string.IsNullOrEmpty(filepath))
            {
                Console.WriteLine("No file path provided");
                return;
            }

            if(!System.IO.Directory.Exists(filepath))
            {
                Console.WriteLine("invalid path " + filepath);
                return;
            }

            string[] fileList = System.IO.Directory.GetFiles(filepath, "*.png");

            foreach (string file in fileList)
            {
                Console.WriteLine(string.Format("Checking: {0}", file));
                using (Bitmap img = new Bitmap(file))
                {
                    string shortName = System.IO.Path.GetFileName(file);

                    SavedStat newStat = new SavedStat();
                    newStat.filename = shortName;                    

                    foreach (StatSection ss in sections)
                    {
                        float stat_value = 0;

                        List<Color> rowPixels = new List<Color>();

                        for (int x = STAT_START_X; x <= STAT_END_X; ++x)
                        {
                            rowPixels.Add(img.GetPixel(x, ss.y));
                        }

                        for(int i=0; i != rowPixels.Count; ++i)
                        {

                            if (rowPixels[i].R > minimum_invalid_red && rowPixels[i].G < 100 && rowPixels[i].B < 100)
                            {
                                Console.WriteLine("Break on red " + ss.name);
                                break;
                            }

                            bool isValidPixelValue = false;

                            if (rowPixels[i].G > minimum_valid_green)
                                isValidPixelValue = true;
                            else if (rowPixels[i].GetBrightness() >= minimum_valid_brightness)                            
                                isValidPixelValue = true;
                            

                            if (isValidPixelValue)
                            {
                                stat_value = (float)i / (float)rowPixels.Count();
                            }

                        }

                        newStat.stats.Add(ss.name, stat_value );
                    }

                    string path = System.IO.Path.GetDirectoryName(file);
                    

                    string newFileName = string.Format("{0}.{1}.json", shortName, DateTime.Now.ToString("yyyyMMddHHmmss"));

                    System.IO.File.WriteAllText(System.IO.Path.Combine(path, newFileName), JsonConvert.SerializeObject(newStat, Formatting.Indented));
                }
            }

            //Console.ReadLine();
            
        }
    }
}
