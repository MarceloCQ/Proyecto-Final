using PLearning_Backend.Enumerations;
using PLearning_Backend.Model;
using PLearning_Backend.Structures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;

namespace PLearning_Backend
{
    /// <summary>
    /// Clase Maquina virtual que representa la máquina virtual que se ejecutará
    /// </summary>
    class VirtualMachine
    {
        public Programa Program {get; set;}                     //Programa a ejecutar, contiene todo lo necesario
        private int ProgramCounter { get; set; }                //Apuntador a la siguiente instrucción a ejecutar
        private Stack<Memory> MemoryStack { get; set; }         //Stack de memorias dormidas
        private Stack<Memory> NewMemoryStack { get; set; }      //Stack de nuevas memorias por si hay llamadas dentro de las llamadas
        private Memory ActiveMemory { get; set; }               //Memoria activa en cierto momento
        private Memory GlobalMemory { get; set; }               //Memoria global, siempre activa
        private Memory NewMemory { get; set; }                  //Nueva memoria a ocupar el lugar de la activa
        private TextBlock Output { get; set; }                  //Output del IDE, para escribir
        private List<string> Input { get; set; }                //Input del programa

        /// <summary>
        /// Método constructor de la máquina virutal
        /// </summary>
        /// <param name="prog">Programa a ejecutar</param>
        /// <param name="tb">Output del IDE</param>
        /// <param name="input">Input del programa</param>
        public VirtualMachine(Programa prog, TextBlock tb, List<string> input)
        {
            Program = prog;
            Output = tb;
            MemoryStack = new Stack<Memory>();
            NewMemoryStack = new Stack<Memory>();
            ProgramCounter = 0;
            Input = input;
        }

        /// <summary>
        /// Obtiene el tipo de dato de una constante, para el input
        /// </summary>
        /// <param name="s">La constante a analizar</param>
        /// <param name="value">El valor ya convertido a el tipo de dato correcto</param>
        /// <returns>El tipo de dato de la constante</returns>
        public static int getType(string s, out dynamic value)
        {
            int aux;
            value = null;
            float aux2;
             
            //Si empieza y termina con comillas, entonces es un string
            if (s.StartsWith("\"") && s.EndsWith("\""))
            {
                value = s.Replace("\"", "").Replace(@"\n", "\n");
                return DataType.String;
            }
            //Si es true o false, entonces es un booleano
            else if (s == "true" || s == "false")
            {
                value = bool.Parse(s);
                return DataType.Bool;
            }
            //Si tiene un solo caracter y empieza y termina con comilla simple es un char
            else if (s.StartsWith("'") && s.EndsWith("'") && s.Length == 1)
            {
                value = char.Parse(s);
                return DataType.Char;
            }
            //Si se puede hacer el parsing de entero, es un entero
            else if (int.TryParse(s, out aux))
            {
                value = int.Parse(s);
                return DataType.Int;
            }
            //Si se puede hacer el parsing de float es un flotante
            else if (float.TryParse(s, out aux2))
            {
                value = float.Parse(s);
                return DataType.Float;
            }

            //Si no coincidió con nada, no se reconoce
            return -1;
        }

        /// <summary>
        /// Método que sirve para leer un valor de memoria
        /// </summary>
        /// <param name="virtualDir">La dirección virtual que se quiere leer</param>
        /// <returns>El valor leído</returns>
        private dynamic readValue(int virtualDir)
        {

            //Si es direccionamiento indirecto, se obtiene la dirección virtual que se encuentra en la casilla
            if (virtualDir < 0)
            {
                virtualDir = readValue(virtualDir * -1);
            }

            //Se obtiene la dirección real, el tipo de dato y el tipo de variable
            MemoryDir memDir = VirtualStructure.getRealIndex(virtualDir);

            switch (memDir.VariableType)
            {
                //Si la variable es global, entonces se lee de la memoria global
                case VirtualStructure.VariableType.Global:
                    return GlobalMemory.ReadValue(memDir);

                //Si la variable es local o temporal, se lee de la memoria activa
                case VirtualStructure.VariableType.Local:                  
                case VirtualStructure.VariableType.Temporal:
                    return ActiveMemory.ReadValue(memDir);

                //Si la memoria es una constante, se lee de la tabla de constantes
                case VirtualStructure.VariableType.Constant:
                    //Se saca el índice y luego la constante
                    int index = Program.ConstantTable.FindIndex(x => x.VirtualDir == virtualDir);
                    string constant = Program.ConstantTable[index].Name;
                    
                    //Dependiendo del tipo, se convierte al tipo de dato real
                    switch (memDir.DataType)
                    {
                        case DataType.Int:
                            return int.Parse(constant);
                        case DataType.Float:
                            return float.Parse(constant, CultureInfo.InvariantCulture);
                        case DataType.String:
                            return constant;
                        case DataType.Char:
                            return char.Parse(constant);
                        case DataType.Bool:
                            return bool.Parse(constant);
                    }

                    break;

            }

            return null;
        }

        /// <summary>
        /// Método que sirve para escribir un valor en memoria
        /// </summary>
        /// <param name="virtualDir">Dirección virtual en donde se va a escribir</param>
        /// <param name="value">Valor que se va a escribir</param>
        private void writeValue(int virtualDir, dynamic value)
        {
            //Si es direccionamiento indirecto, se obtiene la dirección virtual que se encuentra en la casilla
            if (virtualDir < 0)
            {
                virtualDir = readValue(virtualDir * -1);
            }

            //Se obtiene la dirección real, el tipo de dato y el tipo de variable
            MemoryDir memDir = VirtualStructure.getRealIndex(virtualDir);

            //Dependiendo del tipo de variable se escribe en la memoria global o de la memoria activa
            switch (memDir.VariableType)
            {
                case VirtualStructure.VariableType.Global:
                    GlobalMemory.WriteValue(memDir, value);
                    break;
                case VirtualStructure.VariableType.Local:
                case VirtualStructure.VariableType.Temporal:
                    ActiveMemory.WriteValue(memDir, value);
                    break;
            }      
        }

        /// <summary>
        /// Método que sirve para realizar una operación dada un cuádruplo de operación
        /// </summary>
        /// <param name="quadruple">Cuádruplo de operación a realizar</param>
        private void makeOperation(Quadruple quadruple)
        {

            //Se leen los dos operandos
            dynamic value1 = readValue(quadruple.Operand1);
            dynamic value2 = readValue(quadruple.Operand2);

            dynamic res = null;

            //Dependiendo del tipo de operación, se hace la operación
            switch (quadruple.OperationCode)
            {
                case OperationCode.Sum:
                    res = value1 + value2;
                    break;
                case OperationCode.Substraction:
                    res = value1 - value2;
                    break;
                case OperationCode.Multiplication:
                    res = value1 * value2;
                    break;
                case OperationCode.Division:
                    res = value1 / value2;
                    break;
                case OperationCode.And:
                    res = value1 && value2;
                    break;
                case OperationCode.Or:
                    res = value1 || value2;
                    break;
                case OperationCode.EqualComparison:
                    res = value1 == value2;
                    break;
                case OperationCode.Different:
                    res = value1 != value2;
                    break;
                case OperationCode.MoreThan:
                    res = value1 > value2;
                    break;
                case OperationCode.LessThan:
                    res = value1 < value2;
                    break;
                case OperationCode.MoreThanEq:
                    res = value1 >= value2;
                    break;
                case OperationCode.LessThanEq:
                    res = value1 <= value2;
                    break;

            }

            //Si el resultado no es nulo, entonces se escribe en el temporal asignado
            if (res != null)
            {
                writeValue(quadruple.TemporalRegorJump, res);
            }
            else
            {
                //Si no, se tira una excepción interna porque se mando un codigo de operación que no es una operación aritmética
                throw new Exception("El codigo de operación no es una operación o no existe.");
            }

        }

        /// <summary>
        /// Método principal que sirve para ejecutar el programa
        /// </summary>
        public void Run()
        {
            //Se crea la memoria gloval con el tamaño asignado
            GlobalMemory = new Memory(Program.ProcedureTable[Program.Name].Size);

            //Se obtiene la lista de cuádruplos del programa
            List<Quadruple> quadruples = Program.Quadruples;

            //Se obtiene el primer cuadruplo
            Quadruple actQuadruple = quadruples[ProgramCounter];

            //Mientras el cuádruplo actual no sea el de finalizar programa
            while (actQuadruple.OperationCode != OperationCode.EndProg)
            {
                switch (actQuadruple.OperationCode)
                {
                    //Si es una operación
                    case OperationCode.Sum:
                    case OperationCode.Substraction:
                    case OperationCode.Multiplication:
                    case OperationCode.Division:
                    case OperationCode.And:
                    case OperationCode.Or:
                    case OperationCode.EqualComparison:
                    case OperationCode.Different:
                    case OperationCode.MoreThan:
                    case OperationCode.LessThan:
                    case OperationCode.MoreThanEq:
                    case OperationCode.LessThanEq:

                        //Se hace la operación y se incrementa el program counter
                        makeOperation(actQuadruple);
                        ProgramCounter++;
                        break;

                    //Si es una asignación
                    case OperationCode.Assignment:

                        //Se obtiene el valor a asignar
                        dynamic res = readValue(actQuadruple.Operand1);
                        //Se escribe en la dirección dada
                        writeValue(actQuadruple.TemporalRegorJump, res);

                        //Se incrementa el program counter
                        ProgramCounter++;

                        break;

                    //Si es un goto
                    case OperationCode.Goto:
                        //Se asigna el program counter al jump dado
                        ProgramCounter = actQuadruple.TemporalRegorJump;
                        break;

                    //Si es un gotoF
                    case OperationCode.GotoF:

                        //Se lee el valor booleano
                        dynamic exp = readValue(actQuadruple.Operand1);

                        //Si es falso
                        if (!exp)
                        {
                            //Se asigna el program counter con el jump dado
                            ProgramCounter = actQuadruple.TemporalRegorJump;
                        }
                        //Si no
                        else
                        {
                            //Se incrementa el contador
                            ProgramCounter++;
                        }

                        break;

                    //Si es un print
                    case OperationCode.Print:

                        //Se lee el valor a imprimir
                        dynamic toPrint = readValue(actQuadruple.TemporalRegorJump);
                        //Se despliega en el output
                        Output.Text += toPrint;
                        //Se incrementa el contado
                        ProgramCounter++;

                        break;

                    //Si es un read
                    case OperationCode.Read:
                        //Si hay algo en el input
                        if (Input.Count > 0)
                        {
                            //Se saca el primer valor y se elimina
                            string s = Input[0];
                            Input.RemoveAt(0);

                            dynamic resVal;

                            //Se obtiene el tipo de dato de la constante leida
                            int type = getType(s, out resVal);

                            //Si no se encuentra, entonces se marca error
                            if (type == -1)
                            {
                                Output.Text = "Error de ejecución - No se reconoce ningún tipo en el input.";
                                ProgramCounter = quadruples.Count - 1;
                            }
                            //Si no es igual al tipo de la variable a asignar, se marca error
                            else if (type != actQuadruple.Operand1)
                            {
                                Output.Text = "Error de ejecución - El tipo de variable y el input no coinciden.";
                                ProgramCounter = quadruples.Count - 1;
                            }
                            else
                            {
                                //Se escribe el valor del input en la dirección dada
                                writeValue(actQuadruple.TemporalRegorJump, resVal);
                                ProgramCounter++;
                            }
                        }
                        else
                        {
                            Output.Text = "Error de ejecución - No hay suficientes datos en el input.";
                            ProgramCounter = quadruples.Count - 1;
                        }

                        break;

                    //Si es un era
                    case OperationCode.Era:

                        //Se obtiene el nombre de procedimiento de la lista de procedimientos y luego se obtiene el procedimiento con el nombre
                        Procedure proc = Program.ProcedureTable[Program.ProcedureList[actQuadruple.Operand1]];

                        //Si es el main, indicado con un 1, se crea una nueva memoria y se activa
                        if (actQuadruple.Operand2 == 1)
                        {
                            ActiveMemory = new Memory(proc.Size);
                        }
                        //Si no es el main
                        else
                        {
                            //Si hay una nueva memoria esperando a ser activada
                            if (NewMemory != null)
                            {
                                //Se mete al stack de nuevas memorias
                                NewMemoryStack.Push(NewMemory);
                            }

                            //Se crea la nueva memoria
                            NewMemory = new Memory(proc.Size);
                        }

                        //Se incrementa el contador
                        ProgramCounter++;
                        break;

                    //Si es un param
                    case OperationCode.Param:

                        //Si el operando 2 es un -1, entonces no es dimensionado el parametro
                        if (actQuadruple.Operand2 == -1)
                        {
                            //Se obtiene la dirección real de la función
                            MemoryDir memDir = VirtualStructure.getRealIndex(actQuadruple.TemporalRegorJump);
                            //Se obtiene el valor a asignar
                            dynamic resValue = readValue(actQuadruple.Operand1);
                            //Se asigna el valor a la función, la cual se encuentra esperando en newMemory
                            NewMemory.WriteValue(memDir, resValue);
                        }
                        //Si no es -1, es dimensionada
                        else
                        {
                            //Se recorren todas las direcciones del parametro
                            for (int i = 0; i < actQuadruple.Operand2; i++)
                            {
                                //Se obtiene la dirección real de la función
                                MemoryDir memDir = VirtualStructure.getRealIndex(actQuadruple.TemporalRegorJump + i);
                                //Se obtiene el valor a asignar
                                dynamic resValue = readValue(actQuadruple.Operand1 + i);
                                //Se escribe en memoria
                                NewMemory.WriteValue(memDir, resValue);
                            }
                        }


                        ProgramCounter++;

                        break;
                
                    //Si es un ret
                    case OperationCode.Ret:

                        //Se asigna el program counter a lo ultimo que se estaba ejecutando antes de ir a al función
                        ProgramCounter = MemoryStack.Peek().ReturnDir;

                        break;
                    
                    //Si es un endFunc
                    case OperationCode.EndFunc:
                        //Se revive la memoria en el tope del stack
                        ActiveMemory = MemoryStack.Pop();

                        //Si hay nuevas memorias esperando
                        if (NewMemoryStack.Count > 0)
                        {
                            //Se asigna a nueva memoria el tope del stack de nuevas memorias
                            NewMemory = NewMemoryStack.Pop();
                        }

                        //Se incrementa el contador
                        ProgramCounter++;

                        break;
                    
                    //Si es un gosub
                    case OperationCode.GoSub:
                        //Se guarda la dirección de retorno
                        ActiveMemory.ReturnDir = ProgramCounter + 1;
                        //Se mete la memoria al stack de memorias dormidas
                        MemoryStack.Push(ActiveMemory);
                        //Se asigna la memoria activa a la nueva memoria
                        ActiveMemory = NewMemory;             
                        NewMemory = null;

                        //Se pone el contador en donde empieza la función
                        ProgramCounter = actQuadruple.TemporalRegorJump;

                        break;
                    
                    //Su es un verify
                    case OperationCode.Verify:

                        //Se lee el indice superior de la dimensión
                        dynamic rValue = readValue(actQuadruple.Operand1);

                        //Se revisa que no sea menor que cero o mayor o igual que el limite superior
                        if (rValue < 0 || rValue >= actQuadruple.TemporalRegorJump)
                        {
                            Output.Text = "Error - El índice de la variable está fuera del rango.";
                            ProgramCounter = quadruples.Count - 1;

                        }
                        else
                        {
                            ProgramCounter++;
                        }

                        
                        
                        break;
                    
                    //Si es un ref
                    case OperationCode.Ref:

                        //Se lee el valor del parametro por referencia de la memoria actual (es decir de la función donde se encuentra declarado el ref)
                        dynamic value = readValue(actQuadruple.Operand1);

                        //Se obitiene la dirección real de donde se va a poner ese valor (Es decir, la función que lo llamó)
                        MemoryDir memDir2 = VirtualStructure.getRealIndex(actQuadruple.Operand2);

                        //Se escribe en la memoria que esté en el tope de la pila de memorias dormidas
                        MemoryStack.Peek().WriteValue(memDir2, value);

                        //Se incrementa el program counter
                        ProgramCounter++;
                        break;

                }

                //Se asigna el cuadruplo actual al siguiente cuádruplo a ejecutar
                actQuadruple = quadruples[ProgramCounter];
            }

        }


    }
}
