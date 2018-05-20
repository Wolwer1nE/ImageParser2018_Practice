using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2
{
    class main
    {
        static void Main(string[] args)
        {
            var shapeD = new shape();
            var output = shapeD.Shape("001.gif");
            Console.WriteLine(output);
            Console.ReadKey();
        }
    }
}
