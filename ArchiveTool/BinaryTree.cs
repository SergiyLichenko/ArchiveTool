using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveTool
{
    public class Node
    {
        public int Data { get;  set; }//код символа
        public int Weight { get;  set; }//количество повторений
        public Node Left { get; set; }//левая ветка дерева
        public Node Right { get; set; }//правая ветка дерева


        public Node() { }//конструктор по умолчанию
        public Node(byte data, int weight)//перегруженый конструктор
        {
            this.Data = data;       //установка полей
            this.Weight = weight;   //
        }
        
        public Node Add(Node left, Node right)//объдинение двух узлов
        {
            this.Weight = left.Weight + right.Weight;//
            this.Left = left;                        //установка полей
            this.Right = right;                      //

            return this;//возвращение текущего узла
        }
    }
}
