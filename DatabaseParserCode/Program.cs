﻿using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ParseFileNames
{
    internal class DatabaseEntry
    {
        public DatabaseEntry(string subj, string image_number, string gender, string glasses, string eye_state,
            string reflections, string lighting_conditions_or_image_quality, string SensorType,FileInfo origFile)
        {
            this.subject_ID = int.Parse(subj.TrimStart(new char[] { 's', 'S' }));
            this.image_number = int.Parse(image_number);
            this.gender = int.Parse(gender);
            this.glasses = int.Parse(glasses);
            this.eye_state = int.Parse(eye_state);
            this.reflections = int.Parse(reflections);
            this.lighting_conditions_or_image_quality = int.Parse(lighting_conditions_or_image_quality);
            this.SensorTypeKey = int.Parse(SensorType);
            this.origFile = origFile;
        }

        internal FileInfo origFile;
        internal int subject_ID;

        internal int image_number;

        internal int gender;

        internal enum enmGender : int
        {
            Male = 0,
            Female = 1
        };

        internal int glasses;

        internal enum enmGlasses : int
        {
            No = 0,
            Yes = 1
        };

        internal int eye_state;

        internal enum enmEye_state : int
        {
            Close = 0,
            Open = 1
        };

        internal int reflections;

        internal enum enmReflections : int
        {
            Non = 0,
            Low = 1,
            High = 2
        };

        internal int lighting_conditions_or_image_quality;

        internal enum enmLightQuality : int
        {
            Bad = 0,
            Good = 1
        };

        internal Dictionary<int, KeyValuePair<string, double[]>> SensorType
            = new Dictionary<int, KeyValuePair<string, double[]>>()
            {
                {1, new KeyValuePair<string, double[]>("RealSense_SR300", new double[] {640, 480})},
                {2, new KeyValuePair<string, double[]>("IDS_Imaging", new double[] {1280, 1024})},
                {3, new KeyValuePair<string, double[]>("Aptina Imagin", new double[] {752, 480})}
            };

        internal int SensorTypeKey;

        public override string ToString()
        {
            string result = String.Empty;

            result = String.Format("{0},{1},{2},{3:},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",
                subject_ID, image_number,
                gender, ((enmGender)gender).ToString(),
                glasses, ((enmGlasses)glasses).ToString(), eye_state, ((enmEye_state)eye_state).ToString(),
                reflections, ((enmReflections)reflections).ToString(),
                lighting_conditions_or_image_quality,
                ((enmLightQuality)lighting_conditions_or_image_quality).ToString(),
                SensorTypeKey, SensorType[SensorTypeKey].Key,
                SensorType[SensorTypeKey].Value[0].ToString() + "x" + SensorType[SensorTypeKey].Value[1].ToString()
            );
            return result;
        }

        public static string GetHeaderString()
        {
            return "Subject ID, Image number," +
                   "Gender-Discrete,Gender-Nominal," +
                   "Glasses-Discrete,Glasses-Nominal,Eye state-Discrete,Eye state-Nominal," +
                   "Reflections-Ordinal,Reflections-Nominal," +
                   "Lightning conditions-Ordinal,Lightning conditions-Nominal," +
                   "Sensor Type-Ordinal,Sensor type-nominal,Resolution-Categorical";


            //string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
        }
    }

    //            example:
    //        s001_00123_0_0_0_0_0_01.png


    internal class Program
    {
        public static void Main(string[] args)
        {
            string sOriginalPath = @"E:\Download\AI-ML-Course\mrlEyes_2018_01";
            DirectoryInfo di = new DirectoryInfo(sOriginalPath);
            if (!Directory.Exists(di.Parent.FullName + "\\TrainDataBase"))
            {
                Directory.CreateDirectory(di.Parent.FullName + "\\TrainDataBase");
            }
            if(!Directory.Exists(di.Parent.FullName + "\\TrainDataBase\\Open"))
            {
                Directory.CreateDirectory(di.Parent.FullName + "\\TrainDataBase\\Open");
            }
            if (!Directory.Exists(di.Parent.FullName + "\\TrainDataBase\\Close"))
            {
                Directory.CreateDirectory(di.Parent.FullName + "\\TrainDataBase\\Close");
            }

            var files = di.GetFiles("*.png", SearchOption.AllDirectories).OrderBy(f => f.Name).ToArray();
            List<DatabaseEntry> lstData = new List<DatabaseEntry>();
            for (int ind = 0; ind < files.Length; ind++)
            {
                string[] s1parts = files[ind].Name.Replace(".png", "").Split(new char[] { '_' });
                lstData.Add(new DatabaseEntry(
                    s1parts[0], s1parts[1], s1parts[2], s1parts[3], s1parts[4], s1parts[5], s1parts[6], s1parts[7], files[ind]));
            }

            string[] s1FileContent = new string[lstData.Count + 1];
            s1FileContent[0] = DatabaseEntry.GetHeaderString();
            using (FileStream fs = new FileStream(sOriginalPath + "\\DatabaseParsed.csv", FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(s1FileContent[0]);
                for (int ind = 1; ind < lstData.Count; ind++)
                {
                    s1FileContent[ind] = lstData[ind].ToString();
                    sw.WriteLine(s1FileContent[ind]);
                    if (lstData[ind].eye_state == 0)
                    {
                        File.Copy(lstData[ind].origFile.FullName, di.Parent.FullName + 
                            "\\TrainDataBase\\Close\\"+ lstData[ind].origFile.Name,true);
                    }
                    else
                    {
                        File.Copy(lstData[ind].origFile.FullName, di.Parent.FullName +
                                                                  "\\TrainDataBase\\Open\\" + lstData[ind].origFile.Name,true);

                    }
                    


                }
            }
        }
    }
}