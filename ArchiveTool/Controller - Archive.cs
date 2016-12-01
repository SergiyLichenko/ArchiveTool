using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveTool
{
    delegate void ArhiveEnd(long inputSize, long outpuSize, double entropy, double entropy2, double bitPerSymbol);
    partial class Controller
    {
        Model model;    //объект класса Model
        public Node root;      //ссылка на корень дерева

        Dictionary<int, List<int>> codesTable;    //таблица укороченных кодов

        BackgroundWorker workerForArchive;      //поток для архивации
        BackgroundWorker workerForUnArchive;    //поток для разархивации
        BackgroundWorker workerForStatistics;
        System.Windows.Forms.Timer timerForPers;
        List<byte> coded;       //список закодированных байтов


        StringBuilder treeCode;        //закодированное дерево
        List<int> treePath;        //путь обхода по дереву

        public event ArhiveEnd arhiveEnd;
        string fileNameFrom;
        string fileNameTo;
        public Controller()         // конструктор
        {
            model = new Model();
            codesTable = new Dictionary<int, List<int>>();

            coded = new List<byte>();

            result = new StringBuilder();
            timerForPers = new System.Windows.Forms.Timer();
            timerForPers.Interval = 100;
            timerForPers.Tick += TimerForPers_Tick; ;

            workerForArchive = new BackgroundWorker();
            workerForArchive.DoWork += WorkerForArchive_DoWork;
            workerForArchive.WorkerReportsProgress = true;
            workerForArchive.ProgressChanged += WorkerForArchive_ProgressChanged;
            workerForArchive.RunWorkerCompleted += WorkerForArchive_RunWorkerCompleted;

            workerForUnArchive = new BackgroundWorker();
            workerForUnArchive.DoWork += WorkerForUnArchive_DoWork;
            workerForUnArchive.RunWorkerCompleted += WorkerForUnArchive_RunWorkerCompleted;

            workerForStatistics = new BackgroundWorker();
            workerForStatistics.DoWork += WorkerForStatistics_DoWork;
            workerForStatistics.WorkerSupportsCancellation = true;

            workerUnStatistics = new BackgroundWorker();
            workerUnStatistics.DoWork += WorkerUnStatistics_DoWork; ;
            workerUnStatistics.WorkerSupportsCancellation = true;

            treePath = new List<int>();
            treeCode = new StringBuilder();
        }

        private void WorkerForArchive_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            timerForPers.Start();
        }

        public int archPersentage;
        private void TimerForPers_Tick(object sender, EventArgs e)
        {
            archPersentage = (int)((this.forPers / this.sizeFile) * 100);
        }

        public void Archive(string fileNameFrom, string fileNameTo)
        {
            this.fileNameFrom = fileNameFrom;
            this.fileNameTo = fileNameTo;
            workerForArchive.RunWorkerAsync();      //запуск архивации в фоновом потоке
        }
        private void WorkerForArchive_DoWork(object sender, DoWorkEventArgs e)  //архивация
        {
            while (model.creatingCounts)
                ;
            codesTable.Clear();
            root = null;
            treeCode.Clear();

            List<Node> fileData = model.CreateLeaves();   //считывания из файла
            if (fileData.Count == 0)
                return;

            model.CreateStatistics(fileData);

            if (fileData.Count > 1)
                root = CreateBinaryTree(fileData);
            else
                root = fileData[0];

            MakeCodes(root);
            MakeTreePath(root);
            model.WriteStatistics(codesTable);

            workerForArchive.ReportProgress(0);
            MakeArchivedCode();

        }

        internal void CreateStatistics(string fileName)
        {
            if (this.workerForStatistics.IsBusy)
            {
                workerForStatistics.CancelAsync();
                workerForStatistics = new BackgroundWorker();
                workerForStatistics.DoWork += WorkerForStatistics_DoWork;
                workerForStatistics.WorkerSupportsCancellation = true;
            }

            workerForStatistics.RunWorkerAsync(fileName);
        }
        private void WorkerForStatistics_DoWork(object sender, DoWorkEventArgs e)
        {
            model.CreateCounts((string)e.Argument, (BackgroundWorker)sender);
        }
        private void WorkerForArchive_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)  //завершение потока архивации
        {
            FileStream inPut =  File.OpenRead(fileNameFrom);
            FileStream outPu =  File.OpenRead(fileNameTo);
            try
            {
                timerForPers.Stop();
                double lenthFrom = inPut.Length;
                double lenthTo = outPu.Length;
                if(arhiveEnd!=null)
                arhiveEnd((long)lenthFrom, (long)lenthTo, model.entropy, model.CountEntropy2(fileNameTo), lenthTo / lenthFrom * 8);
            }
            catch
            {
                if (arhiveEnd != null)
                    arhiveEnd(new FileInfo(fileNameFrom).Length, 0, model.entropy, 0, 0);
                
            }
        }
        private Node CreateBinaryTree(List<Node> fileData)      //создание дерева
        {
            while (true)
            {
                Node result = new Node();
                result.Add(fileData[0], fileData[1]);           //объединение узлов дерева
                result.Data = -1;

                fileData.RemoveAt(0);                           //
                fileData.RemoveAt(0);                           //добавление и удаление из списка узлов дерева
                fileData.Add(result);                           //

                fileData.Sort((a, b) => a.Weight.CompareTo(b.Weight));

                if (fileData.Count == 1)
                    break;
            }
            return fileData[0];                                 //возвращение корня дерева
        }



        private void MakeCodes(Node root)                               //создание вспомагательной таблицы
        {
            if (root == null)
                return;
            if (root.Left != null)                                      //идем влево
            {
                treePath.Add(0);
                MakeCodes(root.Left);
            }
            if (root.Right != null)                                     //идем вправо
            {
                treePath.Add(1);
                MakeCodes(root.Right);
            }

            if (root.Data != -1)                                        //если это лист дерева
                codesTable.Add((byte)root.Data, new List<int>(treePath));              //добавление в таблицу
            if (treePath != null)
                if (treePath.Count != 0)
                    treePath.RemoveAt(treePath.Count - 1);
        }



        private void MakeTreePath(Node root)             //создание обхода дерева для архивации
        {
            if (root == null)
                return;
            if (root.Left != null)                                  //пока можна идти по дереву
            {
                treeCode.Append("1");                                    //пишем 1
                MakeTreePath(root.Left);
            }
            if (root.Right != null)
            {
                treeCode.Append("1");
                MakeTreePath(root.Right);
            }
            if (root.Data != -1)                                    //если встерилась буква
            {
                string code = Convert.ToString(root.Data, 2);       //конвертация из числа в строку из 0,1

                if (code.Length != 8)                               //
                    for (int i = code.Length; i < 8; i++)           //добавление в старшие разряды 0
                        code = code.Insert(0, "0");                 //

                treeCode.Append("0" + code);                             //добавление в путь обхода дерева
            }
        }
        double sizeFile;
        double forPers;
        private void MakeArchivedCode()       //создание списка байтов для записи в файл
        {
            if (treeCode == null)
                return;
            byte countOfZeroes = 0;
            StringBuilder temp = new StringBuilder();           //временная строковая переменная
            FileStream input = File.OpenRead(model.pathToArchivingFile);
            FileStream output = File.OpenWrite(fileNameTo);
            output.WriteByte(countOfZeroes);
            output.WriteByte(model.sizeOfTree);

            sizeFile = input.Seek(0, SeekOrigin.End);
            input.Seek(0, SeekOrigin.Begin);

            int counter = 0;
            while (true)
            {
                try
                {
                    temp.Append(treeCode.ToString().Substring(8 * counter, 8));


                    output.WriteByte(Convert.ToByte(temp.ToString(), 2));
                    temp.Remove(0, 8);
                    counter++;
                }
                catch (Exception)
                {
                    temp.Append(treeCode.ToString().Substring(8 * counter));
                    break;
                }

            }
            int count2 = temp.ToString().Length;
            for (int m = temp.Length; m < 8; m++)
                temp.Insert(temp.Length, "0");




            int letter = Convert.ToByte(temp.ToString(), 2);

            while (true)
            {
                try
                {
                    foreach (int item in codesTable[input.ReadByte()])
                    {
                        letter = letter | item << (7 - count2);

                        count2++;
                        if (count2 == 8)
                        {
                            count2 = 0;
                            forPers++;
                            output.WriteByte(Convert.ToByte(letter));
                            letter = 0;
                        }
                    }
                }
                catch (Exception)
                {
                    string tempStr = Convert.ToString(letter, 2);

                    for (int m = tempStr.Length; m < 8; m++)
                    {
                        tempStr.Insert(tempStr.Length, "0");
                        countOfZeroes++;
                    }
                    for (int m = tempStr.Length - 1; m > -1; m--)
                    {
                        if (tempStr[m] == '1')
                            break;
                        countOfZeroes++;
                    }
                    countOfZeroes %= countOfZeroes;
                    output.WriteByte(Convert.ToByte(tempStr, 2));
                    break;
                }
            }
            output.Seek(0, SeekOrigin.Begin);
            output.WriteByte(countOfZeroes);

            input.Close();
            output.Close();

        }


    }
}
