using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ArchiveTool
{
    public delegate void UnArchPers(int persantage);
    public partial class Model
    {
        System.Windows.Forms.Timer timerForPers;
        FileStream streamUnCounts;
        public int countOfZeroes;
        public event UnArchPers unArchPers;
        long endOfFile;
        long bytesCount;
        int k = -1;
        public StringBuilder tempStr;
        int tempSizeTree = -1;
        public int sizeOftree;


        public string Unarchive(string path, out int sizeOfTree)
        {
            FileStream fileStream = File.OpenRead(path);        //открытие файла для разархивации
            StringBuilder codedFile = new StringBuilder();      //строка закодированного файла


            byte[] fileData = new byte[fileStream.Length - 2];    //массив для хранение файла
            int countOfZeroes = fileStream.ReadByte();          //количество нулей
            sizeOfTree = fileStream.ReadByte();                 //количество эллементов в дереве

            fileStream.Read(fileData, 0, (int)fileStream.Length - 2); //считывание из файла
            fileStream.Close();

            foreach (byte item in fileData)         //создание строки из закодированного файла
            {
                string temp = Convert.ToString(item, 2);    //конвертация из числа в строку из 0, 1

                for (int i = temp.Length; i < 8; i++)             //запись нулей в старшие биты
                    temp = temp.Insert(0, "0");
                codedFile.Append(temp);                      //расширение строки
            }
            codedFile.Remove(codedFile.Length - countOfZeroes, countOfZeroes);   //удаление нулей вконце


            return codedFile.ToString();
        }


        internal void CreateUnCounts(string path, BackgroundWorker sender)
        {
            if (streamUnCounts != null)
                streamUnCounts.Close();
            streamUnCounts = File.OpenRead(path);


            countOfZeroes = streamUnCounts.ReadByte();
            sizeOftree = streamUnCounts.ReadByte();

            tempStr = new StringBuilder();
            byte tempByte = 0;

            while (true)
            {
                tempSizeTree = -1;

                tempByte = (byte)streamUnCounts.ReadByte();
                string temp = Convert.ToString(tempByte, 2);
                for (int i = temp.Length; i < 8; i++)
                    temp = temp.Insert(0, "0");
                tempStr.Append(temp);

                for (int i = 0; i < tempStr.Length; i++)
                    if (tempStr[i] == '0')
                    {
                        tempSizeTree++;
                        i += 8;
                    }
                if (tempSizeTree == sizeOftree)
                    break;
            }
            for (int j = 0; j < 2; j++)
            {
                tempByte = (byte)streamUnCounts.ReadByte();
                string temp2 = Convert.ToString(tempByte, 2);
                for (int i = temp2.Length; i < 8; i++)
                    temp2 = temp2.Insert(0, "0");
                tempStr.Append(temp2);
            }
            tempSizeTree = -1;

            streamUnCounts.Close();
            streamUnCounts = null;


        }
        internal void UseTree(Node rootUn, string pathUnFrom, string pathUnTo, int k, BackgroundWorker worker)
        {
            
            worker.WorkerReportsProgress = true;            
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.ReportProgress(0);

            timerForPers = new System.Windows.Forms.Timer();
            timerForPers.Interval = 300;
            timerForPers.Tick += TimerForPers_Tick;

            worker.ReportProgress(0);
            int numberOfByte = 2 + (k + 1) / 8;
            int numberOfBits = (k + 1) % 8;
            Node temp = rootUn;
            
            FileStream input = File.OpenRead(pathUnFrom);
            FileStream output = File.OpenWrite(pathUnTo);
            endOfFile = input.Seek(0, SeekOrigin.End);
            input.Seek(0, SeekOrigin.Begin);
            input.Seek(numberOfByte, SeekOrigin.Begin);
            bytesCount = numberOfByte+1;

            byte tempByte = (byte)input.ReadByte();
            if (numberOfBits > 0)
            {
                string tempStr = Convert.ToString(tempByte, 2);
                for (int i = tempStr.Length; i < 8; i++)
                    tempStr = tempStr.Insert(0, "0");
                tempStr = tempStr.Remove(0, numberOfBits);
                for (int i = 8 - numberOfBits; i < 8; i++)
                    tempStr = tempStr.Insert(tempStr.Length, "0");
                tempByte = Convert.ToByte(tempStr, 2);
            }
            bool first = true;
            while (true)
            {
                try
                {
                    for (int i = 7; i > -1; i--)
                    {
                        if (Convert.ToBoolean(tempByte & (1 << i)))
                            temp = temp.Right;
                        else
                            temp = temp.Left;


                        if (temp.Data != -1)
                        {
                            
                            output.WriteByte((byte)temp.Data);
                            temp = rootUn;
                        }
                        if (first)
                            if (i == numberOfBits)
                            {
                                first = false;
                                break;
                            }
                    }

                    if ((endOfFile - (long)bytesCount) == 2)
                    {
                        string tempS = Convert.ToString(Convert.ToByte(input.ReadByte()), 1);
                        for (int i = tempS.Length; i < 8; i++)
                            tempS = tempS.Insert(0, "0");
                        if (countOfZeroes > 0)
                            tempS = tempS.Remove(tempS.Length - countOfZeroes);
                        for (int i = 0; i < tempS.Length; i++)
                        {
                            if (tempS[i] == '1')//идем вправо
                                temp = temp.Right;
                            else                        //идем влево
                                temp = temp.Left;

                            if (temp.Data != -1)        //если найдена буква
                            {
                                output.WriteByte((byte)temp.Data);
                                temp = rootUn;                   //возвращение в корень
                            }
                        }
                        break;
                    }
                    bytesCount++;
                    tempByte = (byte)input.ReadByte();
                }
                catch
                {
                    break;
                }
            }


            worker.ReportProgress(1);

            input.Close();
            output.Close();
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
                timerForPers.Start();
            else
                timerForPers.Stop();
        }

        private void TimerForPers_Tick(object sender, EventArgs e)
        {
            unArchPers((int)(((double)this.bytesCount / ((double)endOfFile)) * 100));
        }
    }
}