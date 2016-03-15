using PLearning_Backend.Model;
using System.Collections.Generic;
using PLearning_Backend.Enumerations;
using PLearning_Backend.Structures;



using System;



public class Parser {
	public const int _EOF = 0;
	public const int _ID = 1;
	public const int _CTEENTERA = 2;
	public const int _CTEFLOAT = 3;
	public const int _CTESTRING = 4;
	public const int _CTECHAR = 5;
	public const int _PROGRAM = 6;
	public const int _MAIN = 7;
	public const int _VOID = 8;
	public const int _INT = 9;
	public const int _FLOAT = 10;
	public const int _STRING = 11;
	public const int _BOOL = 12;
	public const int _CHAR = 13;
	public const int _IFF = 14;
	public const int _ELSE = 15;
	public const int _AND = 16;
	public const int _OR = 17;
	public const int _WHILE = 18;
	public const int _FOR = 19;
	public const int _RETURN = 20;
	public const int _PRINT = 21;
	public const int _READLINE = 22;
	public const int _FUNCTION = 23;
	public const int _TRUE = 24;
	public const int _FALSE = 25;
	public const int _REF = 26;
	public const int _PYC = 27;
	public const int _COMA = 28;
	public const int _IGUAL = 29;
	public const int _PARAB = 30;
	public const int _PARCI = 31;
	public const int _LLAVEAB = 32;
	public const int _LLAVECI = 33;
	public const int _CORCHAB = 34;
	public const int _CORCHCI = 35;
	public const int _MAS = 36;
	public const int _MENOS = 37;
	public const int _MULT = 38;
	public const int _DIV = 39;
	public const int _MAQUE = 40;
	public const int _MEQUE = 41;
	public const int _DIFERENTE = 42;
	public const int _COMPARACION = 43;
	public const int maxT = 44;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

Dictionary<string, Procedure> procedureTable;
	Procedure actualProcedure;
	int tipoActual;
	int scopeActual = VirtualStructure.VariableType.Global;
	string programID;

	Stack<int> POper = new Stack<int>();
	Stack<int> PilaOperandos = new Stack<int>();
	Stack<int> PTipos = new Stack<int>();

	List<Quadruple> quadruples = new List<Quadruple>();

	private void tryToGenerateQuadruple()
	{
		int operat = POper.Pop();
		int operand2 = PilaOperandos.Pop();
		int tipo2 = PTipos.Pop();
		int operand1 = PilaOperandos.Pop();
		int tipo1 = PTipos.Pop();

		int newType = SemanticCube.getCombiningType(tipo1, tipo2, operat);

		if (newType != 0)
		{
			int temp = VirtualStructure.getNext(VirtualStructure.VariableType.Temporal, newType);				 
			Quadruple qAux = new Quadruple(operat, operand1, operand2, temp);
			quadruples.Add(qAux);
			PilaOperandos.Push(temp);
			PTipos.Push(newType);
		}
		else
		{
			SemErr("Error - Tipos incompatibles");
		}
	}

	private void tryToInsertVariable()
	{
		if (!actualProcedure.VariableTable.ContainsKey(t.val))
		{
			if (!procedureTable[programID].VariableTable.ContainsKey(t.val))
			{
				SemErr("Variable no declarada");
			}
			else
			{
				Variable v = procedureTable[programID].VariableTable[t.val];
				PilaOperandos.Push(v.VirtualDir);
				PTipos.Push(v.Type);
			}
			
		}
		else
		{
			Variable v = actualProcedure.VariableTable[t.val];
			PilaOperandos.Push(v.VirtualDir);
			PTipos.Push(v.Type);
		}
	}


/*--------------------------------------------------------------------------*/


	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void PLearning() {
		Expect(6);
		procedureTable = new Dictionary<string, Procedure>(); 
		
		Expect(1);
		actualProcedure = new Procedure(t.val, ReturnType.Program);
		procedureTable.Add(t.val, actualProcedure);   
		programID = t.val;
		
		Expect(27);
		while (StartOf(1)) {
			vars();
		}
		scopeActual = VirtualStructure.VariableType.Local; 
		
		while (la.kind == 23) {
			funcion();
		}
		main();
	}

	void vars() {
		tipo();
		tipoActual = DataType.toDataType(t.val); 
		
		if (la.kind == 1) {
			Get();
			if (actualProcedure.VariableTable.ContainsKey(t.val))
			{
			SemErr("Error - Variable " + t.val + " previamente declarada");
			}
			else
			{
			actualProcedure.VariableTable.Add(t.val, new Variable(t.val, tipoActual, VirtualStructure.getNext(scopeActual, tipoActual)));
			}
			
			while (la.kind == 28) {
				Get();
				Expect(1);
				if (actualProcedure.VariableTable.ContainsKey(t.val))
				{
				SemErr("Error - Variable '" + t.val + "' previamente declarada");
				}
				else
				{
				actualProcedure.VariableTable.Add(t.val, new Variable(t.val, tipoActual, VirtualStructure.getNext(scopeActual, tipoActual)));
				} 
				
			}
			if (la.kind == 29) {
				Get();
				if (la.kind == 4 || la.kind == 5) {
					ctelet();
				} else if (la.kind == 2 || la.kind == 3) {
					ctenum();
				} else if (la.kind == 24 || la.kind == 25) {
					ctebool();
				} else SynErr(45);
			}
			Expect(27);
		} else if (la.kind == 34) {
			Get();
			Expect(2);
			if (la.kind == 28) {
				Get();
				Expect(2);
			}
			Expect(35);
			Expect(1);
			if (actualProcedure.VariableTable.ContainsKey(t.val))
			{
			SemErr("Error - Variable '" + t.val + "' previamente declarada");
			}
			else
			{
			actualProcedure.VariableTable.Add(t.val, new Variable(t.val, tipoActual, VirtualStructure.getNext(scopeActual, tipoActual)));
			}
			
			while (la.kind == 28) {
				Get();
				Expect(1);
				if (actualProcedure.VariableTable.ContainsKey(t.val))
				{
				SemErr("Error - Variable '" + t.val + "' previamente declarada");
				}
				else
				{
				actualProcedure.VariableTable.Add(t.val, new Variable(t.val, tipoActual, VirtualStructure.getNext(scopeActual, tipoActual)));
				}
				
			}
			Expect(27);
		} else SynErr(46);
	}

	void funcion() {
		Expect(23);
		regresa();
		int retType = ReturnType.toReturnType(t.val);
		
		Expect(1);
		if (procedureTable.ContainsKey(t.val))
		{
		SemErr("Error - Funcion '" + t.val + "' previamente declarada");
		}
		else
		{
		actualProcedure = new Procedure(t.val, retType);
		procedureTable.Add(t.val, actualProcedure);
		}
		
		
		Expect(30);
		if (StartOf(2)) {
			parametro();
			while (la.kind == 28) {
				Get();
				parametro();
			}
		}
		Expect(31);
		Expect(32);
		while (StartOf(1)) {
			vars();
		}
		while (StartOf(3)) {
			estatuto();
		}
		if (la.kind == 20) {
			Get();
			expresion();
			Expect(27);
		}
		Expect(33);
	}

	void main() {
		Expect(8);
		Expect(7);
		actualProcedure = new Procedure("main", ReturnType.Main);
		procedureTable.Add("main", actualProcedure);
		
		Expect(30);
		Expect(31);
		Expect(32);
		while (StartOf(1)) {
			vars();
		}
		while (StartOf(3)) {
			estatuto();
		}
		Expect(33);
	}

	void tipo() {
		if (la.kind == 9) {
			Get();
		} else if (la.kind == 10) {
			Get();
		} else if (la.kind == 11) {
			Get();
		} else if (la.kind == 12) {
			Get();
		} else if (la.kind == 13) {
			Get();
		} else SynErr(47);
	}

	void ctelet() {
		if (la.kind == 4) {
			Get();
			int virtualDir = VirtualStructure.getNext(VirtualStructure.VariableType.Constant, DataType.String);
			PilaOperandos.Push(virtualDir);
			PTipos.Push(DataType.String);
			
		} else if (la.kind == 5) {
			Get();
			int virtualDir = VirtualStructure.getNext(VirtualStructure.VariableType.Constant, DataType.Char);
			PilaOperandos.Push(virtualDir);
			PTipos.Push(DataType.Char);
			
		} else SynErr(48);
	}

	void ctenum() {
		if (la.kind == 2) {
			Get();
			int virtualDir = VirtualStructure.getNext(VirtualStructure.VariableType.Constant, DataType.Int);
			PilaOperandos.Push(virtualDir);
			PTipos.Push(DataType.Int);
			
		} else if (la.kind == 3) {
			Get();
			int virtualDir = VirtualStructure.getNext(VirtualStructure.VariableType.Constant, DataType.Float);
			PilaOperandos.Push(virtualDir);
			PTipos.Push(DataType.Float);
			
		} else SynErr(49);
	}

	void ctebool() {
		if (la.kind == 24) {
			Get();
		} else if (la.kind == 25) {
			Get();
		} else SynErr(50);
		int virtualDir = VirtualStructure.getNext(VirtualStructure.VariableType.Constant, DataType.Bool);
		PilaOperandos.Push(virtualDir);
		PTipos.Push(DataType.Bool);
		
	}

	void regresa() {
		if (StartOf(1)) {
			tipo();
		} else if (la.kind == 8) {
			Get();
		} else SynErr(51);
	}

	void parametro() {
		if (la.kind == 26) {
			Get();
		}
		tipo();
		int type = DataType.toDataType(t.val);
		
		Expect(1);
		if (actualProcedure.VariableTable.ContainsKey(t.val))
		{
		SemErr("Variable previamente declarada");
		}
		else
		{
		actualProcedure.VariableTable.Add(t.val, new Variable(t.val, type, VirtualStructure.getNext(scopeActual, type)));
		}
		
		if (la.kind == 34) {
			Get();
			Expect(35);
		}
	}

	void estatuto() {
		if (la.kind == 14) {
			condicion();
		} else if (la.kind == 21) {
			escritura();
		} else if (la.kind == 18 || la.kind == 19) {
			ciclos();
		} else if (la.kind == 1) {
			asignacionollamada();
		} else SynErr(52);
	}

	void expresion() {
		comparacion();
		if (POper.Count > 0 && (POper.Peek() == OperationCode.And || POper.Peek() == OperationCode.Or))
		{
		tryToGenerateQuadruple();
		}
		
		while (la.kind == 16 || la.kind == 17) {
			if (la.kind == 16) {
				Get();
			} else {
				Get();
			}
			POper.Push(OperationCode.toOperationCode(t.val));
			
			comparacion();
			if (POper.Count > 0 && (POper.Peek() == OperationCode.And || POper.Peek() == OperationCode.Or))
			{
			tryToGenerateQuadruple();
			}
			
		}
	}

	void bloque() {
		Expect(32);
		while (StartOf(3)) {
			estatuto();
		}
		Expect(33);
	}

	void ciclowhile() {
		Expect(18);
		Expect(30);
		expresion();
		Expect(31);
		bloque();
	}

	void ciclofor() {
		Expect(19);
		Expect(30);
		Expect(1);
		Expect(29);
		expresion();
		Expect(27);
		expresion();
		Expect(27);
		expresion();
		Expect(31);
		bloque();
	}

	void ciclos() {
		if (la.kind == 18) {
			ciclowhile();
		} else if (la.kind == 19) {
			ciclofor();
		} else SynErr(53);
	}

	void condicion() {
		Expect(14);
		Expect(30);
		expresion();
		Expect(31);
		bloque();
		if (la.kind == 15) {
			Get();
			bloque();
		}
	}

	void escritura() {
		Expect(21);
		Expect(30);
		expresion();
		int resExp = PilaOperandos.Pop();
		Quadruple qAux = new Quadruple(OperationCode.Print, -1, -1, resExp);
		quadruples.Add(qAux);
		
		while (la.kind == 28) {
			Get();
			expresion();
			resExp = PilaOperandos.Pop();
			qAux = new Quadruple(OperationCode.Print, -1, -1, resExp);
			quadruples.Add(qAux);
			
		}
		Expect(31);
		Expect(27);
	}

	void asignacionollamada() {
		Expect(1);
		if (la.kind == 29 || la.kind == 34) {
			tryToInsertVariable();
			
			if (la.kind == 34) {
				cuantificador();
			}
			Expect(29);
			POper.Push(OperationCode.Assignment);
			
			if (StartOf(4)) {
				expresion();
				int ladoDer = PilaOperandos.Pop();
				int ladoIzq = PilaOperandos.Pop();
				int asigna = POper.Pop();
				Quadruple qAux = new Quadruple(asigna, ladoDer, -1, ladoIzq);
				quadruples.Add(qAux);
				
			} else if (la.kind == 22) {
				lectura();
			} else SynErr(54);
		} else if (la.kind == 30) {
			Get();
			if (StartOf(4)) {
				expresion();
				while (la.kind == 28) {
					Get();
					expresion();
				}
			}
			Expect(31);
		} else SynErr(55);
		Expect(27);
	}

	void lectura() {
		Expect(22);
		Expect(30);
		Expect(31);
	}

	void cuantificador() {
		Expect(34);
		expresion();
		if (la.kind == 28) {
			Get();
			expresion();
		}
		Expect(35);
	}

	void comparacion() {
		exp();
		if (StartOf(5)) {
			if (la.kind == 40) {
				Get();
			} else if (la.kind == 41) {
				Get();
			} else if (la.kind == 42) {
				Get();
			} else {
				Get();
			}
			POper.Push(OperationCode.toOperationCode(t.val));
			
			exp();
			if (POper.Count > 0 && (POper.Peek() == OperationCode.MoreThan || POper.Peek() == OperationCode.LessThan || POper.Peek() == OperationCode.Different || POper.Peek() == OperationCode.EqualComparison))
			{
			tryToGenerateQuadruple();
			}
			
		}
	}

	void exp() {
		term();
		if (POper.Count > 0 && (POper.Peek() == OperationCode.Sum || POper.Peek() == OperationCode.Substraction))
		{
		tryToGenerateQuadruple();
		}
		
		while (la.kind == 36 || la.kind == 37) {
			if (la.kind == 36) {
				Get();
			} else {
				Get();
			}
			POper.Push(OperationCode.toOperationCode(t.val));
			
			term();
			if (POper.Count > 0 && (POper.Peek() == OperationCode.Sum || POper.Peek() == OperationCode.Substraction))
			{
			tryToGenerateQuadruple();
			}
			
		}
	}

	void term() {
		factor();
		if (POper.Count > 0 && (POper.Peek() == OperationCode.Multiplication || POper.Peek() == OperationCode.Division))
		{
		tryToGenerateQuadruple();
		}
		
		while (la.kind == 38 || la.kind == 39) {
			if (la.kind == 38) {
				Get();
			} else {
				Get();
			}
			POper.Push(OperationCode.toOperationCode(t.val));
			
			factor();
			if (POper.Count > 0 && (POper.Peek() == OperationCode.Multiplication || POper.Peek() == OperationCode.Division))
			{
			tryToGenerateQuadruple();
			}
			
		}
	}

	void factor() {
		if (la.kind == 30) {
			Get();
			POper.Push(-1);
			
			expresion();
			Expect(31);
			POper.Pop();
			
		} else if (StartOf(6)) {
			if (la.kind == 36 || la.kind == 37) {
				if (la.kind == 36) {
					Get();
				} else {
					Get();
				}
			}
			ctevar();
		} else SynErr(56);
	}

	void ctevar() {
		if (la.kind == 4 || la.kind == 5) {
			ctelet();
		} else if (la.kind == 24 || la.kind == 25) {
			ctebool();
		} else if (la.kind == 2 || la.kind == 3) {
			ctenum();
		} else if (la.kind == 1) {
			Get();
			string id = t.val;
			
			if (la.kind == 30) {
				Get();
				if (StartOf(4)) {
					expresion();
					while (la.kind == 28) {
						Get();
						expresion();
					}
				}
				Expect(31);
			} else if (la.kind == 34) {
				cuantificador();
			} else if (StartOf(7)) {
				tryToInsertVariable();
				
			} else SynErr(57);
		} else SynErr(58);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		PLearning();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_T,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _x,_x},
		{_x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_x,_T, _x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "ID expected"; break;
			case 2: s = "CTEENTERA expected"; break;
			case 3: s = "CTEFLOAT expected"; break;
			case 4: s = "CTESTRING expected"; break;
			case 5: s = "CTECHAR expected"; break;
			case 6: s = "PROGRAM expected"; break;
			case 7: s = "MAIN expected"; break;
			case 8: s = "VOID expected"; break;
			case 9: s = "INT expected"; break;
			case 10: s = "FLOAT expected"; break;
			case 11: s = "STRING expected"; break;
			case 12: s = "BOOL expected"; break;
			case 13: s = "CHAR expected"; break;
			case 14: s = "IFF expected"; break;
			case 15: s = "ELSE expected"; break;
			case 16: s = "AND expected"; break;
			case 17: s = "OR expected"; break;
			case 18: s = "WHILE expected"; break;
			case 19: s = "FOR expected"; break;
			case 20: s = "RETURN expected"; break;
			case 21: s = "PRINT expected"; break;
			case 22: s = "READLINE expected"; break;
			case 23: s = "FUNCTION expected"; break;
			case 24: s = "TRUE expected"; break;
			case 25: s = "FALSE expected"; break;
			case 26: s = "REF expected"; break;
			case 27: s = "PYC expected"; break;
			case 28: s = "COMA expected"; break;
			case 29: s = "IGUAL expected"; break;
			case 30: s = "PARAB expected"; break;
			case 31: s = "PARCI expected"; break;
			case 32: s = "LLAVEAB expected"; break;
			case 33: s = "LLAVECI expected"; break;
			case 34: s = "CORCHAB expected"; break;
			case 35: s = "CORCHCI expected"; break;
			case 36: s = "MAS expected"; break;
			case 37: s = "MENOS expected"; break;
			case 38: s = "MULT expected"; break;
			case 39: s = "DIV expected"; break;
			case 40: s = "MAQUE expected"; break;
			case 41: s = "MEQUE expected"; break;
			case 42: s = "DIFERENTE expected"; break;
			case 43: s = "COMPARACION expected"; break;
			case 44: s = "??? expected"; break;
			case 45: s = "invalid vars"; break;
			case 46: s = "invalid vars"; break;
			case 47: s = "invalid tipo"; break;
			case 48: s = "invalid ctelet"; break;
			case 49: s = "invalid ctenum"; break;
			case 50: s = "invalid ctebool"; break;
			case 51: s = "invalid regresa"; break;
			case 52: s = "invalid estatuto"; break;
			case 53: s = "invalid ciclos"; break;
			case 54: s = "invalid asignacionollamada"; break;
			case 55: s = "invalid asignacionollamada"; break;
			case 56: s = "invalid factor"; break;
			case 57: s = "invalid ctevar"; break;
			case 58: s = "invalid ctevar"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
