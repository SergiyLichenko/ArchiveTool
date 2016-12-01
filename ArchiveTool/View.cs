using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArchiveTool
{
    public partial class View : Form
    {
        Controller controller;//объект класса Controller

        System.Windows.Forms.Timer timer2;
        DateTime now2;
        Timer timerForBar2;

        System.Windows.Forms.Timer timer1;
        DateTime now;
        Timer timerForBar;

        public View()//конструктор
        {
            InitializeComponent();
            controller = new Controller();
            this.button1.Enabled = false;
            this.button2.Enabled = false;
        }





        private void Controller_arhiveEnd(long inputSize, long outpuSize, double entropy,double entropy2, double bitPerSymbol)
        {
            TimeSpan temp = (DateTime.Now - now);
            richTextBox1.Text = temp.ToString();
            timer1.Stop();
            timerForBar.Stop();
            this.toolStripProgressBar1.Value = 100;
            this.button1.Enabled = true;
            controller.arhiveEnd -= Controller_arhiveEnd;
            MessageBox.Show(String.Format("Finished\nTime: {0}min {1}s {2}ms\nSize:\n\tInput File: {3} bytes \n\tOutput File: {4} bytes\n\nEntropy1: {5}\nEntropy2: {6}\nBit per Symbol: {7}",
                temp.Minutes.ToString(), temp.Seconds.ToString(), temp.Milliseconds, inputSize.ToString(), outpuSize.ToString(), entropy,entropy2,bitPerSymbol));

            StreamReader reader = new StreamReader(Environment.CurrentDirectory + "Statistics.txt");
            string forInfo = reader.ReadToEnd();
            reader.Close();
            Info info = new Info(forInfo);
            PrintTree(controller.root, 0);

            info.ShowDialog();
            info = new Info(tree);
            info.ShowDialog();
            
        }
        string tree;
        void PrintTree(Node root, int k)
        {
            if (root != null)
            {
                PrintTree(root.Left, k + 3);
                for (int i = 0; i < k; i++)
                    tree = tree + ("       ");
            
           
                if (root.Data != -1)
                    tree = tree + root.Weight + " (" + root.Data + ")\n";
                else
                    tree = tree + root.Weight + "\n";
                PrintTree(root.Right, k + 3);
            }
        }

        private void Controller_unArchived(long inputSize, long outpuSize)
        {
            TimeSpan temp = (DateTime.Now - now2);
            richTextBox2.Text = temp.ToString();
            timer2.Stop();
            timerForBar2.Stop();
            this.toolStripProgressBar2.Value = 100;
            this.button2.Enabled = true;
            controller.unArchived -= Controller_unArchived;
            MessageBox.Show(String.Format("Finished\nTime: {0}min {1}s {2}ms\nSize:\n\tInput File: {3} bytes \n\tOutput File: {4} bytes\n",
              temp.Minutes.ToString(), temp.Seconds.ToString(), temp.Milliseconds, inputSize.ToString(), outpuSize.ToString()));
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            SaveFileDialog fileDialogSave = new SaveFileDialog();
            fileDialogSave.Filter = "Binary File|*.bin";
            if (fileDialogSave.ShowDialog() == DialogResult.OK)
            {
                this.textBox_Arhive_To.Text = fileDialogSave.FileName;
            }
            if (!String.IsNullOrEmpty(this.textBox_Arhive_From.Text.Trim()) && !String.IsNullOrEmpty(this.textBox_Arhive_To.Text.Trim()))
                this.button1.Enabled = true;
            else
                this.button1.Enabled = false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialogOpen = new OpenFileDialog();
            if (fileDialogOpen.ShowDialog() == DialogResult.OK)
            {
                this.textBox_Arhive_From.Text = fileDialogOpen.FileName;
                this.controller.CreateStatistics(fileDialogOpen.FileName);
            }
            if (!String.IsNullOrEmpty(this.textBox_Arhive_From.Text.Trim()) && !String.IsNullOrEmpty(this.textBox_Arhive_To.Text.Trim()))
                this.button1.Enabled = true;
            else
                this.button1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.button1.Enabled = false;
            if (timer1 == null)
                timer1 = new System.Windows.Forms.Timer();

            timer1.Tick += Timer1_Tick;
            timer1.Interval = 5;
            this.richTextBox1.Text = String.Empty;

            timerForBar = new System.Windows.Forms.Timer();
            timerForBar.Tick += TimerForBar_Tick;
            timerForBar.Interval = 300;
            timerForBar.Start();

            controller.arhiveEnd += Controller_arhiveEnd;

            now = DateTime.Now;
            timer1.Start();
            controller.Archive(textBox_Arhive_From.Text.Trim(), textBox_Arhive_To.Text.Trim());//архивация
        }

        private void TimerForBar_Tick(object sender, EventArgs e)
        {
            if (controller.archPersentage > 100 || controller.archPersentage<0)
                return;
            string s = "";
            lock (s)
            {
                this.toolStripProgressBar1.Value = controller.archPersentage;
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            richTextBox1.Text = (DateTime.Now - now).ToString();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialogOpen = new OpenFileDialog();
            fileDialogOpen.Filter = "Binary File|*.bin";
            if (fileDialogOpen.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = fileDialogOpen.FileName;
                this.controller.CreateUnStatistics(fileDialogOpen.FileName);
            }
            if (!String.IsNullOrEmpty(this.textBox1.Text.Trim()) && !String.IsNullOrEmpty(this.textBox2.Text.Trim()))
                this.button2.Enabled = true;
            else
                this.button2.Enabled = false;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            SaveFileDialog fileDialogSave = new SaveFileDialog();

            if (fileDialogSave.ShowDialog() == DialogResult.OK)
            {
                this.textBox2.Text = fileDialogSave.FileName;
            }
            if (!String.IsNullOrEmpty(this.textBox1.Text.Trim()) && !String.IsNullOrEmpty(this.textBox2.Text.Trim()))
                this.button2.Enabled = true;
            else
                this.button2.Enabled = false;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            this.button2.Enabled = false;
            if (timer2 == null)
                timer2 = new Timer();
            timer2.Tick += Timer2_Tick; ;
            timer2.Interval = 5;
            this.richTextBox2.Text = String.Empty;

            timerForBar2 = new System.Windows.Forms.Timer();
            timerForBar2.Interval = 300;
            timerForBar2.Tick += TimerForBar2_Tick;

            timerForBar2.Start();
            controller.unArchived += Controller_unArchived;//подписка на пользовательское событие окончания архивации
            now2 = DateTime.Now;
            timer2.Start();
            controller.UnArchive(this.textBox1.Text.Trim(), this.textBox2.Text.Trim());//разархивация
        }

        private void TimerForBar2_Tick(object sender, EventArgs e)
        {
            if (controller.unArchPersentage < 0 || controller.unArchPersentage > 100)
               return;
            string s = "";
            lock (s)
            {
                this.toolStripProgressBar2.Value = controller.unArchPersentage;
            }
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            richTextBox2.Text = (DateTime.Now - now2).ToString();
        }
    }
}
