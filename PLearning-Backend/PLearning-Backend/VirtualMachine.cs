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
        private Memory ActiveMemory { get; set; }
        private Memory GlobalMemory { get; set; }
        private Memory NewMemory { get; set; }

        public VirtualMachine(Programa prog)
        {
            Program = prog;
            MemoryStack = new Stack<Memory>();
            ProgramCounter = 0;
        }


        private dynamic readValue(int vType, int dType, int virtualDir)
        {
            switch (vType)
            {
                case VirtualStructure.VariableType.Global:
                    return GlobalMemory.ReadValue(vType, dType, virtualDir);
                case VirtualStructure.VariableType.Local:                  
                case VirtualStructure.VariableType.Temporal:
                    return ActiveMemory.ReadValue(vType, dType, virtualDir);
                case VirtualStructure.VariableType.Constant:
                    int index = Program.ConstantTable.FindIndex(x => x.VirtualDir == virtualDir);
                    string constant = Program.ConstantTable[index].Name;
                    

                    switch (dType)
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

        private void writeValue(int vType, int dType, int virtualDir, dynamic value)
        {
            switch (vType)
            {
                case VirtualStructure.VariableType.Global:
                    GlobalMemory.WriteValue(vType, dType, virtualDir, value);
                    break;
                case VirtualStructure.VariableType.Local:
                case VirtualStructure.VariableType.Temporal:
                    ActiveMemory.WriteValue(vType, dType, virtualDir, value);
                    break;
            }      
        }

        private void makeOperation(Quadruple quadruple)
        {
            Tuple<int, int> VTypeAndDTypeOperand1 = VirtualStructure.getVTypeAndDType(quadruple.Operand1);
            Tuple<int, int> VTypeAndDTypeOperand2 = VirtualStructure.getVTypeAndDType(quadruple.Operand2);
            Tuple<int, int> VTypeAndDTypeTemporal = VirtualStructure.getVTypeAndDType(quadruple.TemporalRegorJump);

            int vTypeOperand1 = VTypeAndDTypeOperand1.Item1;
            int dTypeOperand1 = VTypeAndDTypeOperand1.Item2;

            int vTypeOperand2 = VTypeAndDTypeOperand2.Item1;
            int dTypeOperand2 = VTypeAndDTypeOperand2.Item2;

            int vTypeOperandTemporal = VTypeAndDTypeTemporal.Item1;
            int dTypeOperandTemporal = VTypeAndDTypeTemporal.Item2;

            dynamic value1 = readValue(vTypeOperand1, dTypeOperand1, quadruple.Operand1);
            dynamic value2 = readValue(vTypeOperand2, dTypeOperand2, quadruple.Operand2);

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

            }

            if (res != null)
            {
                writeValue(vTypeOperandTemporal, dTypeOperandTemporal, quadruple.TemporalRegorJump, res);
            }
            else
            {
                throw new Exception("El codigo de operación no es una operación o no existe.");
            }

        }

        public void Run()
        {
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

                        makeOperation(actQuadruple);
                        ProgramCounter++;
                        break;

                    case OperationCode.Assignment:

                        Tuple<int, int> VTypeAndDTypeRight = VirtualStructure.getVTypeAndDType(actQuadruple.Operand1);
                        Tuple<int, int> VTypeAndDTypeLeft = VirtualStructure.getVTypeAndDType(actQuadruple.TemporalRegorJump);

                        int vTypeRight = VTypeAndDTypeRight.Item1;
                        int dTypeRight = VTypeAndDTypeRight.Item2;

                        int vTypeLeft = VTypeAndDTypeLeft.Item1;
                        int dTypeLeft = VTypeAndDTypeLeft.Item2;

                        dynamic res = readValue(vTypeRight, dTypeRight, actQuadruple.Operand1);

                        writeValue(vTypeLeft, dTypeLeft, actQuadruple.TemporalRegorJump, res);
                        ProgramCounter++;

                        break;

                    case OperationCode.Goto:

                        ProgramCounter = actQuadruple.TemporalRegorJump;
                        break;

                    case OperationCode.GotoF:

                        Tuple<int, int> VTypeAndDTypeExp = VirtualStructure.getVTypeAndDType(actQuadruple.Operand1);

                        int vTypeExp = VTypeAndDTypeExp.Item1;
                        int dTypeExp = VTypeAndDTypeExp.Item2;

                        dynamic exp = readValue(vTypeExp, dTypeExp, actQuadruple.Operand1);

                        if (exp)
                        {
                            ProgramCounter = actQuadruple.TemporalRegorJump;
                        }
                        else
                        {
                            ProgramCounter++;
                        }

                        break;


                    case OperationCode.Print:
                    case OperationCode.ReadLine:
                    case OperationCode.Era:
                        Procedure proc = Program.ProcedureTable[Program.ProcedureList[actQuadruple.Operand1]];
                        NewMemory = new Memory(proc.Size);
                        break;
                    case OperationCode.Param:
                    case OperationCode.Return:
                    case OperationCode.Ret:
                    case OperationCode.GoSub:
                        MemoryStack.Push(ActiveMemory);
                        NewMemory.ReturnDir = ProgramCounter + 1;
                        ActiveMemory = NewMemory;
                        ProgramCounter = actQuadruple.TemporalRegorJump;

                        break;

                }

                actQuadruple = quadruples[ProgramCounter];
            }

        }


    }
}
