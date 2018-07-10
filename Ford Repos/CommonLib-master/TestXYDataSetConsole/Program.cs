using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib;

namespace TestXYDataSetConsole
{

    class Program
    {
        static void Main(string[] args)
        {
            double[] X = { 75.0, 83, 85, 85, 92, 97, 99 };
            double[] Y = { 16.0, 20, 25, 27, 32, 48, 48 };
            var ds = new CommonLib.Numerical.XYDataSet(X, Y);

            Console.WriteLine(Math.Round(ds.Slope, 2)); //1.45
            Console.WriteLine(Math.Round(ds.YIntercept, 2)); //-96.85
            Console.WriteLine(Math.Round(ds.ComputeRSquared(), 3)); //0.927
        }
    }
}
