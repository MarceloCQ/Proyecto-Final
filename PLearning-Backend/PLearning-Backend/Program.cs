using PLearning_Backend.Enumerations;
using PLearning_Backend.Model;
using PLearning_Backend.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PLearning_Backend
{
    class Program
    {
        static void printQuadruples(List<Quadruple> quadruples)
        {
            StreamWriter escribe = new StreamWriter(@"C:\Users\Marcelo\OneDrive\Documentos\TEC\8. Octavo Semestre\Diseño de compiladores\Proyecto Final\Cuadruplos.txt");
            int i = 0;
            foreach (Quadruple q in quadruples)
            {
                escribe.WriteLine(i + "." + "\t\t" + OperationCode.toString(q.OperationCode) + "\t\t" + q.Operand1 + "\t\t" + q.Operand2 + "\t\t" + q.TemporalRegorJump);
                i++;
            }

            escribe.Close();

        }

        static void Main(string[] args)
        {
            
            Console.Write("Nombre de archivo: ");
            string nombre = Console.ReadLine();

            Scanner scanner = new Scanner(nombre);
            Parser parser = new Parser(scanner);
            Programa p = parser.Parse();

            VirtualMachine vm = new VirtualMachine(p);
            vm.Run();

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
