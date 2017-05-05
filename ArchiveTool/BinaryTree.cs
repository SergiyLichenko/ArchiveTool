using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveTool
{
    public class Node
    {
        public int Data { get;  set; }
        public int Weight { get;  set; }
        public Node Left { get; set; }
        public Node Right { get; set; }


        public Node() { }
        public Node(byte data, int weight)
        {
            this.Data = data;       
            this.Weight = weight;
        }
        
        public Node Add(Node left, Node right)
        {
            this.Weight = left.Weight + right.Weight;
            this.Left = left;                      
            this.Right = right;                    

            return this;
        }
    }
}
