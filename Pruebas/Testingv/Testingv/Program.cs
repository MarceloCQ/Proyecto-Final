using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testingv
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Nombre de archivo: ");
            string nombre = Console.ReadLine();

            Scanner scanner = new Scanner(nombre);
            Parser parser = new Parser(scanner);
            parser.Parse();
            Console.WriteLine(parser.errors.count + " errors detected");
            Console.ReadLine();
        }
    }
}
