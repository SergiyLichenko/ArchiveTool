using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveTool
{
    public partial class Model
    {        
        public int sizeOfArhivingFile;
        public string pathToArchivingFile;
        public byte sizeOfTree;
        public double entropy;
        public bool creatingCounts;
        
        private StringBuilder statistics = new StringBuilder();
        private FileStream fileStreamForCounts;
        private int[] result;
        
        internal void CreateCounts(string path, BackgroundWorker worker)
        {
            pathToArchivingFile = path;
            creatingCounts = true;
            if (fileStreamForCounts != null)
                fileStreamForCounts.Close();
            fileStreamForCounts = File.OpenRead(path);

            sizeOfArhivingFile = (int)fileStreamForCounts.Length;
            result = new int[256];

            while (true)
            {
                try
                {                 
                    result[fileStreamForCounts.ReadByte()]++;
                }
                catch (Exception)
                {
                    break;
                }
            }
            fileStreamForCounts.Close();
            fileStreamForCounts = null;
            creatingCounts = false;
        }
        internal List<Node> CreateLeaves() 
        {
            List<Node> fileData = new List<Node>();   
            for (int i = 0; i < result.Length; i++)            
                if (result[i] != 0)
                    fileData.Add(new Node((byte)i, result[i]));

            fileData.Sort((a, b) => a.Weight.CompareTo(b.Weight));
            return fileData;      
        }

        public void CreateStatistics(List<Node> fileData)
        {
            sizeOfTree = (byte)(fileData.Count - 1);
            statistics.Append(Environment.NewLine);
            entropy = CountEntropy(fileData);

            foreach (Node item in fileData)
            {
                statistics.Append(String.Format("{0} - {1}", item.Data, item.Weight + Environment.NewLine));
            }

        }


        private double CountEntropy(List<Node> fileData)
        {
            double result = 0;
            foreach (Node item in fileData)
            {
                double chance = (double)item.Weight / (double)sizeOfArhivingFile;
                result += -chance * Math.Log(chance, 2);
            }
            return result;
        }

        internal void WriteStatistics(Dictionary<int, List<int>> codesTable)
        {
            string path = Environment.CurrentDirectory + "Statistics.txt";
            if (File.Exists(path))
                File.Delete(path);
            Stream stream = File.OpenWrite(path);                 
            StreamWriter writer = new StreamWriter(stream, Encoding.Default);   
            writer.Write("Энтропия = " + entropy);
            writer.Write(statistics.ToString());

            StringBuilder temp = new StringBuilder();
            temp.Append("Коды дерева:");
            foreach (KeyValuePair<int, List<int>> item in codesTable)
            {
                string tempStr = null;
                foreach (var item2 in item.Value)
                {
                    bool tempBool = Convert.ToBoolean(item2);
                    if (tempBool)
                        tempStr += "1";
                    else
                        tempStr += "0";
                }
                temp.Append(String.Format("{0} - {1}", item.Key, tempStr));
                temp.Append(Environment.NewLine);
            }
            writer.Write(temp);                                           
            writer.Close();
        }

        internal double CountEntropy2(string fileNameTo)
        {
            FileStream stream = File.OpenRead(fileNameTo);
            int size = (int)stream.Length;
            int[] resu = new int[256];

            while (true)
            {
                try
                {
                    resu[stream.ReadByte()]++;
                }
                catch
                {
                    break;
                }
            }
            List<Node> fileData = new List<Node>();  
            for (int i = 0; i < resu.Length; i++)           
                if (resu[i] != 0)
                    fileData.Add(new Node((byte)i, resu[i]));

            fileData.Sort((a, b) => a.Weight.CompareTo(b.Weight));
            double entropy2 = CountEntropy(fileData);
            return entropy2;
        }
    }
}
