using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveTool
{
    delegate void UnArchived(long inputSize, long outpuSize);         
    partial class Controller
    {          
        public event UnArchived unArchived;   
        public int unArchPersentage;
        
        private StringBuilder result;                          
        private Node rootUn;
        private int tempSizeTree = -1;
        private string pathUnFrom;
        private string pathUnTo;
        private BackgroundWorker workerUnStatistics;

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

        internal void UnArchive(string fileNameFrom, string fileNameTo)     
        {
            pathUnFrom = fileNameFrom;
            pathUnTo = fileNameTo;
            workerForUnArchive.RunWorkerAsync(fileNameFrom);       
        }
        private void WorkerForUnArchive_DoWork(object sender, DoWorkEventArgs e)
        {
            rootUn = new Node();
            rootUn.Data = -1;
            CreateTree(rootUn);
            model.unArchPers += Model_unArchPers;
            model.UseTree(rootUn, pathUnFrom,pathUnTo,k,(BackgroundWorker) sender);                 
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
            unArchived(new FileInfo(pathUnFrom).Length, new FileInfo(pathUnTo).Length);                        
        }
        
        int k=-1;
        private void CreateTree(Node rootUnarchive)       
        {
            k++;
            if (tempSizeTree == model.sizeOftree)
                return;

            if (model.tempStr[k] == '1')                
            {
                if (rootUnarchive.Left == null)       
                {
                    rootUnarchive.Left = new Node();
                    rootUnarchive.Left.Data = -1;
                    CreateTree(rootUnarchive.Left);    
                }
                else                                   
                {
                    rootUnarchive.Right = new Node();
                    rootUnarchive.Right.Data = -1;
                    CreateTree(rootUnarchive.Right);
                }
                if (rootUnarchive.Left == null || rootUnarchive.Right == null)
                    CreateTree(rootUnarchive);            

            }
            else      
            {
                tempSizeTree++;
                k++;                  
                string byteCode = model.tempStr.ToString().Substring(k, 8);    
                rootUnarchive.Data = Convert.ToByte(byteCode, 2);  

                k += 7;
            }
        }
    }
}
