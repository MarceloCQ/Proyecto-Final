﻿using PLearning_Backend.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Model
{
    class Procedure
    {
        public string Name { get; set; }
        public ReturnType Type { get; set; }
        public Dictionary<string, Variable> VariableTable { get; set; }

        public Procedure (string name, ReturnType type)
        {
            Name = name;
            Type = type;
            VariableTable = new Dictionary<string, Variable>();
        }

        public static ReturnType toReturnType(string returnType)
        {
            switch (returnType)
            {
                case "int":
                    return ReturnType.Int;
                case "float":
                    return ReturnType.Float;
                case "char":
                    return ReturnType.Char;
                case "bool":
                    return ReturnType.Bool;
                case "string":
                    return ReturnType.String;
                case "void":
                    return ReturnType.Void;
                default:
                    return ReturnType.String;
            }
        }

    }
}