using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveTool
{
    delegate void UnArchived(long inputSize, long outpuSize);         //делегат для пользовательского события
    partial class Controller
    {
          
        StringBuilder result;                           //строка результата
        Node rootUn;
        int tempSizeTree = -1;
        public int unArchPersentage;

        string pathUnFrom;
        string pathUnTo;
        public event UnArchived unArchived;             //пользовательское событие завершения разархивации
        BackgroundWorker workerUnStatistics;

        internal void CreateUnStatistics(string fileName)
        {
            if (this.workerUnStatistics.IsBusy)
            {
                workerUnStatistics.CancelAsync();
                workerUnStatistics = new BackgroundWorker();
                workerUnStatistics.DoWork += WorkerUnStatistics_DoWork;
                workerUnStatistics.WorkerSupportsCancellation = true;
            }

            workerUnStatistics.RunWorkerAsync(fileName);
        }
        private void WorkerUnStatistics_DoWork(object sender, DoWorkEventArgs e)
        {
            model.CreateUnCounts((string)e.Argument, (BackgroundWorker)sender);
        }

        internal void UnArchive(string fileNameFrom, string fileNameTo)        //разархивация
        {
            pathUnFrom = fileNameFrom;
            pathUnTo = fileNameTo;
            workerForUnArchive.RunWorkerAsync(fileNameFrom);        //запуск разархивации в фоновом потоке
        }


        

        private void WorkerForUnArchive_DoWork(object sender, DoWorkEventArgs e)
        {
            rootUn = new Node();
            rootUn.Data = -1;
            CreateTree(rootUn);
            model.unArchPers += Model_unArchPers;
            model.UseTree(rootUn, pathUnFrom,pathUnTo,k,(BackgroundWorker) sender);                  //использование дерева
            model.unArchPers -= Model_unArchPers;
        }

        private void Model_unArchPers(int persantage)
        {
            if (persantage < 100)
                this.unArchPersentage = persantage;
            else
                this.archPersentage = 100;
        }

        private void WorkerForUnArchive_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {            
            unArchived(new FileInfo(pathUnFrom).Length, new FileInfo(pathUnTo).Length);                          //вызов события завершения разархивации
        }
        
        int k=-1;
        private void CreateTree(Node rootUnarchive)         //разархивирование дерева
        {
            k++;
            if (tempSizeTree == model.sizeOftree)
                return;

            if (model.tempStr[k] == '1')                 //пока можна идти по дереву
            {
                if (rootUnarchive.Left == null)         //идем влево
                {
                    rootUnarchive.Left = new Node();
                    rootUnarchive.Left.Data = -1;
                    CreateTree(rootUnarchive.Left);     //рекурсивно вызываем
                }
                else                                    //идем вправо
                {
                    rootUnarchive.Right = new Node();
                    rootUnarchive.Right.Data = -1;
                    CreateTree(rootUnarchive.Right);
                }
                if (rootUnarchive.Left == null || rootUnarchive.Right == null)
                    CreateTree(rootUnarchive);              //возвращение на один уровень на верх 

            }
            else        //создание узла дерева
            {
                tempSizeTree++;
                k++;                    //
                string byteCode = model.tempStr.ToString().Substring(k, 8);     //строка кода символа дерева, состоящая из 0, 1
                rootUnarchive.Data = Convert.ToByte(byteCode, 2);   //запись символа в лист дерева

                k += 7;
            }
        }
        

    }
}
