using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Model
{
    /// <summary>
    /// Clase cuádruplo que inlute el codigo de operación, los dos operandos y el temporal o salto, dependiendo del código de operación
    /// </summary>
    class Quadruple
    {
        public int OperationCode { get; set; }
        public int Operand1 { get; set; }
        public int Operand2 { get; set; }
        public int TemporalRegorJump { get; set; }

        /// <summary>
        /// Constructor del cuádruplo el cual recibe todas los atributos
        /// </summary>
        /// <param name="operationCode">Código de operación correspondiente</param>
        /// <param name="operand1">Primer operando</param>
        /// <param name="operand2">Segundo operando</param>
        /// <param name="temporalRegorJump">Temporal o salto, depende de el código de operación</param>
        public Quadruple(int operationCode, int operand1, int operand2, int temporalRegorJump)
        {
            OperationCode = operationCode;
            Operand1 = operand1;
            Operand2 = operand2;
            TemporalRegorJump = temporalRegorJump;
        }

        
    }
}
