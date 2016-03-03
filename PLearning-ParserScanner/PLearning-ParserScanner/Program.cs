using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_ParserScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Nombre de archivo: ");
            string nombre = Console.ReadLine();

            Scanner scanner = new Scanner(nombre);
            Parser parser = new Parser(scanner);
            parser.Parse();

            if (parser.errors.count == 0)
            {
                Console.WriteLine("No se encontraron errores");
            }
            else
            {
                Console.WriteLine("Se encontraron " + parser.errors.count + " errores.");
            }

            
            Console.ReadLine();
        }
    }
}
