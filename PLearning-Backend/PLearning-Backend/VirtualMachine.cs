using PLearning_Backend.Enumerations;
using PLearning_Backend.Model;
using PLearning_Backend.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend
{
    class VirtualMachine
    {
        public Programa Program {get; set;}
        private int ProgramCounter { get; set; }
        private Stack<Memory> MemoryStack { get; set; }
        private Stack<Memory> NewMemoryStack { get; set; }
        private Memory ActiveMemory { get; set; }
        private Memory GlobalMemory { get; set; }
        private Memory NewMemory { get; set; }

        public VirtualMachine(Programa prog)
        {
            Program = prog;
            MemoryStack = new Stack<Memory>();
            NewMemoryStack = new Stack<Memory>();
            ProgramCounter = 0;
        }


        private dynamic readValue(int virtualDir)
        {

            if (virtualDir < 0)
            {
                virtualDir = readValue(virtualDir * -1);
            }

            MemoryDir memDir = VirtualStructure.getRealIndex(virtualDir);

            switch (memDir.VariableType)
            {
                case VirtualStructure.VariableType.Global:
                    return GlobalMemory.ReadValue(memDir);
                case VirtualStructure.VariableType.Local:                  
                case VirtualStructure.VariableType.Temporal:
                    return ActiveMemory.ReadValue(memDir);
                case VirtualStructure.VariableType.Constant:
                    int index = Program.ConstantTable.FindIndex(x => x.VirtualDir == virtualDir);
                    string constant = Program.ConstantTable[index].Name;
                    

                    switch (memDir.DataType)
                    {
                        case DataType.Int:
                            return int.Parse(constant);
                        case DataType.Float:
                            return float.Parse(constant);
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

        private void writeValue(int virtualDir, dynamic value)
        {
            if (virtualDir < 0)
            {
                virtualDir = readValue(virtualDir * -1);
            }

            MemoryDir memDir = VirtualStructure.getRealIndex(virtualDir);

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

        private void makeOperation(Quadruple quadruple)
        {
            dynamic value1 = readValue(quadruple.Operand1);
            dynamic value2 = readValue(quadruple.Operand2);

            dynamic res = null;
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

            if (res != null)
            {
                writeValue(quadruple.TemporalRegorJump, res);
            }
            else
            {
                throw new Exception("El codigo de operación no es una operación o no existe.");
            }

        }

        public void Run()
        {

            GlobalMemory = new Memory(Program.ProcedureTable[Program.Name].Size);

            List<Quadruple> quadruples = Program.Quadruples;

            Quadruple actQuadruple = quadruples[ProgramCounter];

            while (actQuadruple.OperationCode != OperationCode.EndProg)
            {
                switch (actQuadruple.OperationCode)
                {
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

                        makeOperation(actQuadruple);
                        ProgramCounter++;
                        break;

                    case OperationCode.Assignment:


                        dynamic res = readValue(actQuadruple.Operand1);

                        writeValue(actQuadruple.TemporalRegorJump, res);
                        ProgramCounter++;

                        break;

                    case OperationCode.Goto:

                        ProgramCounter = actQuadruple.TemporalRegorJump;
                        break;

                    case OperationCode.GotoF:

                        Tuple<int, int> VTypeAndDTypeExp = VirtualStructure.getVTypeAndDType(actQuadruple.Operand1);

                        int vTypeExp = VTypeAndDTypeExp.Item1;
                        int dTypeExp = VTypeAndDTypeExp.Item2;

                        dynamic exp = readValue( actQuadruple.Operand1);

                        if (!exp)
                        {
                            ProgramCounter = actQuadruple.TemporalRegorJump;
                        }
                        else
                        {
                            ProgramCounter++;
                        }

                        break;


                    case OperationCode.Print:

                        dynamic toPrint = readValue(actQuadruple.TemporalRegorJump);
                        Console.Write(toPrint);
                        ProgramCounter++;
                        break;
                    case OperationCode.ReadLine:
                    case OperationCode.Era:
                        Procedure proc = Program.ProcedureTable[Program.ProcedureList[actQuadruple.Operand1]];
                        if (actQuadruple.Operand2 == 1)
                        {
                            ActiveMemory = new Memory(proc.Size); 
                        }
                        else
                        {
                            if (NewMemory != null)
                            {
                                NewMemoryStack.Push(NewMemory);
                            }

                            NewMemory = new Memory(proc.Size);
                        }
                        
                        ProgramCounter++;
                        break;
                    case OperationCode.Param:

                        if (actQuadruple.Operand2 == -1)
                        {
                            MemoryDir memDir = VirtualStructure.getRealIndex(actQuadruple.TemporalRegorJump);

                            dynamic resValue = readValue(actQuadruple.Operand1);

                            NewMemory.WriteValue(memDir, resValue);
                        }
                        else
                        {
                            for (int i = 0; i < actQuadruple.Operand2; i++)
                            {
                                MemoryDir memDir = VirtualStructure.getRealIndex(actQuadruple.TemporalRegorJump + i);

                                dynamic resValue = readValue(actQuadruple.Operand1 + i);

                                NewMemory.WriteValue(memDir, resValue);
                            }
                        }
                        

                        ProgramCounter++;

                        break;
                    case OperationCode.Return:
                        break;
                    case OperationCode.Ret:
                        ActiveMemory = MemoryStack.Pop();
                        ProgramCounter = ActiveMemory.ReturnDir;

                        if (NewMemoryStack.Count > 0)
                        {
                            NewMemory = NewMemoryStack.Pop();
                        }

                        break;
                    case OperationCode.GoSub:
                        ActiveMemory.ReturnDir = ProgramCounter + 1;
                        MemoryStack.Push(ActiveMemory);
                        ActiveMemory = NewMemory;
                        NewMemory = null;
                        ProgramCounter = actQuadruple.TemporalRegorJump;

                        break;

                    case OperationCode.Verify:

                        dynamic rValue = readValue(actQuadruple.Operand1);


                        if (rValue < 0 || rValue >= actQuadruple.TemporalRegorJump)
                        {
                            Console.WriteLine("Error - El indice de la variable está fuera del rango.");
                            Console.ReadLine();
                            Environment.Exit(0);

                        }

                        ProgramCounter++;
                        
                        break;

                }

                actQuadruple = quadruples[ProgramCounter];
            }

        }


    }
}
