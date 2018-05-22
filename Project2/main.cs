using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2
{
    class Main
    {
        static void Mainn(string[] args)
        {
            var shapeD = new Shape();
            var output = shapeD.ShapeD("001.gif");
            Console.WriteLine(output);
            Console.ReadKey();
        }
    }
}
