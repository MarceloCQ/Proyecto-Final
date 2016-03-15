using PLearning_Backend.Enumerations;
using PLearning_Backend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend
{
    class Program
    {
        static void Main(string[] args)
        {
            

            int[, ,] cuboSemantico = new int[6, 6, 10];

           


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
