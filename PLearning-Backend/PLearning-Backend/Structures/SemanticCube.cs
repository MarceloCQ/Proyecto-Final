using PLearning_Backend.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Structures
{
    /// <summary>
    /// Clase estática que se encarga de manejar el cubo semántico
    /// </summary>
    static class SemanticCube
    {
        private static readonly int[, ,] semanticCube = new int[6, 6, 10];      //Cubo de 6 x 6 x 10 (5 tipos de datos mas el cero, 5 tipos de datos mas el cero y 10 códigos de operación correspondientes a operaciones

        /// <summary>
        /// Constructor del cubo semántico el cual solo se ejecuta la primera vez que el cubo es utilizado
        /// sirve para generar el tipo de dato resultante al hacer una operación con dos tipos. Todo lo que 
        /// no está asignado se considera como error.
        /// </summary>
        static SemanticCube()
        {
            //AND
            semanticCube[DataType.Bool, DataType.Bool, OperationCode.And] = DataType.Bool;

            //OR
            semanticCube[DataType.Bool, DataType.Bool, OperationCode.Or] = DataType.Bool;

            // >
            semanticCube[DataType.Int, DataType.Int, OperationCode.MoreThan] = DataType.Bool;
            semanticCube[DataType.Int, DataType.Float, OperationCode.MoreThan] = DataType.Bool;
            semanticCube[DataType.Float, DataType.Int, OperationCode.MoreThan] = DataType.Bool;
            semanticCube[DataType.Float, DataType.Float, OperationCode.MoreThan] = DataType.Bool;

            // <
            semanticCube[DataType.Int, DataType.Int, OperationCode.LessThan] = DataType.Bool;
            semanticCube[DataType.Int, DataType.Float, OperationCode.LessThan] = DataType.Bool;
            semanticCube[DataType.Float, DataType.Int, OperationCode.LessThan] = DataType.Bool;
            semanticCube[DataType.Float, DataType.Float, OperationCode.LessThan] = DataType.Bool;

            // !=
            semanticCube[DataType.Int, DataType.Int, OperationCode.Different] = DataType.Bool;
            semanticCube[DataType.Int, DataType.Float, OperationCode.Different] = DataType.Bool;
            semanticCube[DataType.Float, DataType.Int, OperationCode.Different] = DataType.Bool;
            semanticCube[DataType.Float, DataType.Float, OperationCode.Different] = DataType.Bool;
            semanticCube[DataType.Char, DataType.Char, OperationCode.Different] = DataType.Bool;
            semanticCube[DataType.String, DataType.String, OperationCode.Different] = DataType.Bool;
            semanticCube[DataType.Bool, DataType.Bool, OperationCode.Different] = DataType.Bool;

            // ==
            semanticCube[DataType.Int, DataType.Int, OperationCode.EqualComparison] = DataType.Bool;
            semanticCube[DataType.Int, DataType.Float, OperationCode.EqualComparison] = DataType.Bool;
            semanticCube[DataType.Float, DataType.Int, OperationCode.EqualComparison] = DataType.Bool;
            semanticCube[DataType.Float, DataType.Float, OperationCode.EqualComparison] = DataType.Bool;
            semanticCube[DataType.Char, DataType.Char, OperationCode.EqualComparison] = DataType.Bool;
            semanticCube[DataType.String, DataType.String, OperationCode.EqualComparison] = DataType.Bool;
            semanticCube[DataType.Bool, DataType.Bool, OperationCode.EqualComparison] = DataType.Bool;

            // +
            semanticCube[DataType.Int, DataType.Int, OperationCode.Sum] = DataType.Int;
            semanticCube[DataType.Int, DataType.Float, OperationCode.Sum] = DataType.Float;
            semanticCube[DataType.Float, DataType.Int, OperationCode.Sum] = DataType.Float;
            semanticCube[DataType.Float, DataType.Float, OperationCode.Sum] = DataType.Float;
            semanticCube[DataType.String, DataType.String, OperationCode.Sum] = DataType.String;

            // -
            semanticCube[DataType.Int, DataType.Int, OperationCode.Substraction] = DataType.Int;
            semanticCube[DataType.Int, DataType.Float, OperationCode.Substraction] = DataType.Float;
            semanticCube[DataType.Float, DataType.Int, OperationCode.Substraction] = DataType.Float;
            semanticCube[DataType.Float, DataType.Float, OperationCode.Substraction] = DataType.Float;

            // *
            semanticCube[DataType.Int, DataType.Int, OperationCode.Multiplication] = DataType.Int;
            semanticCube[DataType.Int, DataType.Float, OperationCode.Multiplication] = DataType.Float;
            semanticCube[DataType.Float, DataType.Int, OperationCode.Multiplication] = DataType.Float;
            semanticCube[DataType.Float, DataType.Float, OperationCode.Multiplication] = DataType.Float;

            // / 
            semanticCube[DataType.Int, DataType.Int, OperationCode.Division] = DataType.Float;
            semanticCube[DataType.Int, DataType.Float, OperationCode.Division] = DataType.Float;
            semanticCube[DataType.Float, DataType.Int, OperationCode.Division] = DataType.Float;
            semanticCube[DataType.Float, DataType.Float, OperationCode.Division] = DataType.Float;
        }

        /// <summary>
        /// Método que sirve para obtener el tipo de dato resultante a raiz de dos tipos y un código de operación
        /// </summary>
        /// <param name="operandType1">Tipo de dato 1</param>
        /// <param name="operandType2">Tipo de dato 2</param>
        /// <param name="operator_">Operador</param>
        /// <returns>El tipo de dato resultante</returns>
        public static int getCombiningType(int operandType1, int operandType2, int operator_)
        {
            return semanticCube[operandType1, operandType2, operator_];
        }


    }
}
