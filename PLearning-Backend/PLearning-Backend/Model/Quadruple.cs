using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Model
{
    class Quadruple
    {
        public int OperationCode { get; set; }
        public int Operand1 { get; set; }
        public int Operand2 { get; set; }
        public int TemporalReg { get; set; }

        public Quadruple(int operationCode, int operand1, int operand2, int temporalReg)
        {
            OperationCode = operationCode;
            Operand1 = operand1;
            Operand2 = operand2;
            TemporalReg = temporalReg;
        }

        
    }
}
