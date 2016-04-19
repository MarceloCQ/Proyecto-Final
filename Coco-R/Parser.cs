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

Dictionary<string, Procedure> procedureTable;			//Diccionario de procedimientos, la llave es el nombre del procedimiento
	List<string> procedureList;								//Lista que sirve para mapear los procedimientos con un numero
	List<Constant> constantTable;							//Lista de constantes

	Procedure actualProcedure;								//Apunta al procedimiento actual en el que se estÃ¡
	int tipoActual;
	int scopeActual = VirtualStructure.VariableType.Global;	//Apunta al scope actual, global o local
	string programID;										//Nombre del programa

	Stack<int> POper = new Stack<int>();					//Pila de operadores para la generaciÃ³n de cuadruplos
	Stack<int> PilaOperandos = new Stack<int>();			//Pila de operandos para la generaciÃ³n de cuÃ¡druplos
	Stack<int> PTipos = new Stack<int>();					//Pila de tipos para la validaciÃ³n de semÃ¡ntica
	Stack<int> PSaltos = new Stack<int>();					//Pila de saltos para guardar los Jumps o la referencia a un Goto/GotoF

	List<Quadruple> quadruples = new List<Quadruple>();		//Lista de los cuÃ¡druplos del programa


	/// <summary>
	/// MÃ©todo que inserta un nuevo cuadruplo y lo mete a la lista de cuadruplos
	/// </summary>
	private void insertQuadruple(int opCode, int oper1, int oper2, int TemporalRegorJump)
	{
		Quadruple qAux = new Quadruple(opCode, oper1, oper2, TemporalRegorJump);
		quadruples.Add(qAux);
	}


	/// <summary>
    /// MÃ©todo que sirve para intentar generar un cuÃ¡druplo de las expresiones, revisa que se
    /// pueda realizar la operaciÃ³n y si no, manda un error de semÃ¡ntica. 
    /// </summary>
	private void tryToGenerateQuadruple()
	{
		int operat = POper.Pop();				//Se saca el operador de la pila de operadores
		int operand2 = PilaOperandos.Pop();		//Se saca el operando del lado derecho de la pila de operandos
		int tipo2 = PTipos.Pop();				//Se saca el tipo del operando del lado derecho de la pila de tipos
		int operand1 = PilaOperandos.Pop();		//Se saca el operando del lado izquierdo de la pila de operandos
		int tipo1 = PTipos.Pop();				//Se saca el tipo del operando del lado izquierdo de la pila de tipos

		//Se obtiene el tipo resultante del cubo semÃ¡ntico al combinar ambos tipos con el operador
		int newType = SemanticCube.getCombiningType(tipo1, tipo2, operat);  

		//Se verifica que el tipo resultante no sea cero, es decir, que si se puedan combinar dichos tipos
		if (newType != 0)
		{
			int temp = VirtualStructure.getNext(VirtualStructure.VariableType.Temporal, newType); 	//Se obtiene la siguiente direcciÃ³n temporal
			actualProcedure.increaseCounter(VirtualStructure.VariableType.Temporal, newType);	  	//Se incrementa el contador de tamaÃ±o de temporales					 	
			insertQuadruple(operat, operand1, operand2, temp); 										//Se genera un nuevo cuadruplo con los datos obtenidos		
			PilaOperandos.Push(temp);																//Se mete el temporal resultante a la pila de operandos
			PTipos.Push(newType);																	//Se mete el tipo resultante a a la pila de tipos
		}
		//Si no se pueden combinar
		else
		{
			//Se genera un error de semÃ¡ntica y se termina la ejecuciÃ³n
			SemErr("Error - Tipos incompatibles");
			finishExecution();
		}
	}

	/// <summary>
    /// MÃ©todo que sirve para intentar insertar una variable en la pila de operandos 
    /// </summary>
	private void tryToInsertVariable()
	{
		//Si la variable no estÃ¡ en la tabla de variables del procedimiento correspondiente
		if (!actualProcedure.VariableTable.ContainsKey(t.val))
		{
			//Si la varaible no estÃ¡ en la tabla de variables del procedimiento global
			if (!procedureTable[programID].VariableTable.ContainsKey(t.val))
			{
				//Se genera un error de semÃ¡ntica
				SemErr("Variable no declarada");
				finishExecution();
			}
			//Si la variable se encuentra en la tabla de variables del procedimiento global
			else
			{
				//Se mete la direcciÃ³n en la pila de operandos y el tipo en la pila de tipos.
				Variable v = procedureTable[programID].VariableTable[t.val];
				PilaOperandos.Push(v.VirtualDir);
				PTipos.Push(v.Type);
			}
			
		}
		//Si la variable si se encuentra en la tabla de variables del procedimiento actual
		else
		{
			//Se mete la direcciÃ³n en la pila de operandos y el tipo en la pila de tipos.
			Variable v = actualProcedure.VariableTable[t.val];
			PilaOperandos.Push(v.VirtualDir);
			PTipos.Push(v.Type);
		}
	}

	/// <summary>
    /// MÃ©todo que sirve para intentar insertar una constante en la pila de operandos 
    /// </summary>
	private void tryToInsertConstant(int dataType)
	{
		int virtualDir;							//Direccion virtual de la constante

		int index = constantTable.FindIndex(x => x.Name == t.val);

		//Si la constante ya se encuentra la lista
		if (index >= 0)
		{	
			//Se obtiene la direcciÃ³n virtual
			virtualDir = constantTable[index].VirtualDir;
		}
		//Si no se encuentra en el diccionario
		else
		{
			//Se obtiene la direcciÃ³n virtual de la estructura virtual
			virtualDir = VirtualStructure.getNext(VirtualStructure.VariableType.Constant, dataType);

			//Se crea la nueva constante
			Constant c = new Constant(t.val, virtualDir);

			//Se inserta en la tabla de constantes
			constantTable.Add(c);
		}
		
		//Se inserta la constante en la pila de operandos y en la pila de tipos
		PilaOperandos.Push(virtualDir);
		PTipos.Push(dataType);
	}

	private void tryToInsertArgument(int k, string id)
	{
		//Se saca el argumento junto con su tipo
		int argument = PilaOperandos.Pop();
		int argumentType = PTipos.Pop();

		//Si el tipo de argumento no el mismo con el definido, se marca error.
		if (k < procedureTable[id].Parameters.Count)
		{
			if (argumentType != procedureTable[id].Parameters[k])
			{
				SemErr("Error - Los tipos no coinciden con la funciÃ³n");
				finishExecution();
			}
			else
			{

				//Se aÃ±ade un nuevo cuadruplo "Era" con el numero de procedimiento
				insertQuadruple(OperationCode.Param, argument, -1, k);
			}
		}
		else
		{
			SemErr("El numero de parametros no coindice con la declaraciÃ³n de la funciÃ³n");
		}
	}

	///<summary>
	/// MÃ©todo que sirve para encontrar el numero de procedimiento
	///</summary>
	private int findProcedure(string name)
	{
		for (int i = 0; i < procedureList.Count; i++)
		{
			if (procedureList[i] == name)
			{
				return i;
			}			
		}

		return -1;
	}

	///<summary>
	/// MÃ©todo que sirve para finalizar la ejecuciÃ³n del programa.
	///</summary>
	private void finishExecution()
	{
		Console.ReadLine();
		Environment.Exit(1);
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
		procedureTable = new Dictionary<string, Procedure>();			//Se inicializa la tabla de procedimientos
		procedureList = new List<string>();								//Se inicializa la lista de procedimientos
		constantTable = new List<Constant>(); 							//Se inicializa la lista de constantes
		
		Expect(1);
		actualProcedure = new Procedure(t.val, ReturnType.Program);		//Se asgian como procedimiento actual al procedimiento global
		procedureTable.Add(t.val, actualProcedure);   					//Se aÃ±ade este procedimiento a la tabla de procedimientos
		procedureList.Add(t.val);										//Se aÃ±ade tambiÃ©n a la lista de procedimientos
		programID = t.val;												//Se asigna el id del programa para uso posterior
		
		Expect(27);
		while (StartOf(1)) {
			vars();
		}
		scopeActual = VirtualStructure.VariableType.Local;				//Se asigna el scope como local cuando se acaba lo primero
		PSaltos.Push(quadruples.Count);									//AÃ±ade en la pila de saltos la posiciÃ³n en donde estarÃ¡ el goto al main
		insertQuadruple(OperationCode.Goto, -1, -1, -1);
		
		while (la.kind == 23) {
			funcion();
		}
		main();
		insertQuadruple(OperationCode.EndProg, -1, -1, -1);
		
	}

	void vars() {
		tipo();
		List<int> registros = new List<int>();		//Lista de registros que sirve para asignar las variables que tienen asignaciÃ³n inicial
		tipoActual = DataType.toDataType(t.val); 	//Se guarda el tipo en una variable
		
		if (la.kind == 1) {
			Get();
			if (actualProcedure.VariableTable.ContainsKey(t.val))
			{
			SemErr("Error - Variable " + t.val + " previamente declarada");
			}
			//Si no estaba declarada
			else
			{
			//Se asigna una nueva direcciÃ³n virtual
			int virtualDir = VirtualStructure.getNext(scopeActual, tipoActual);
			//Se aÃ±ade a la lista de registros
			registros.Add(virtualDir);
			//Se aÃ±ade la nueva variable a la tabla de variables del procedimiento en cuestiÃ³n
			actualProcedure.VariableTable.Add(t.val, new Variable(t.val, tipoActual, virtualDir));
			//Se incrementa el contador de tamaÃ±o correspondiente
			actualProcedure.increaseCounter(VirtualStructure.VariableType.Local, tipoActual);
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
				int virtualDir = VirtualStructure.getNext(scopeActual, tipoActual);
				registros.Add(virtualDir);
				actualProcedure.VariableTable.Add(t.val, new Variable(t.val, tipoActual, virtualDir));
				actualProcedure.increaseCounter(VirtualStructure.VariableType.Local, tipoActual);
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
				foreach (int r in registros)
				{
				
				int index = constantTable.FindIndex(x => x.Name == t.val);
				insertQuadruple(OperationCode.Assignment, constantTable[index].VirtualDir, -1, r);
				}
				
				
			}
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
			//Si no, entonces se aÃ±ade a la tabla de variables
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
		} else SynErr(46);
		Expect(27);
	}

	void funcion() {
		Expect(23);
		regresa();
		int retType = ReturnType.toReturnType(t.val);
		
		bool hasReturn = false;
		
		
		
		Expect(1);
		string functionName = t.val;
		//Se revise que la funciÃ³n no haya sido previamente declarada en el diccionario de procedimientos
		if (procedureTable.ContainsKey(t.val))
		{
		SemErr("Error - Funcion '" + t.val + "' previamente declarada");
		}
		else
		{
		//Si no, se aÃ±ade a la tabla de procedimientos
		actualProcedure = new Procedure(t.val, retType);
		procedureTable.Add(t.val, actualProcedure);
		procedureList.Add(t.val);
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
		actualProcedure.InitialDir = quadruples.Count;
		
		while (StartOf(1)) {
			vars();
		}
		while (StartOf(3)) {
			estatuto();
		}
		if (la.kind == 20) {
			Get();
			expresion();
			int returnType = PTipos.Pop();
			int ret = PilaOperandos.Pop();
			
			//Si el tipo de la expresiÃ³n no coincide con el tipo de retorno se marca error
			if (returnType != retType)
			{
			SemErr("Error - Tipos incompatibles en return");
			finishExecution();
			}
			else
			{
			//Si si coincide se genera un nuevo cuÃ¡druplo
			insertQuadruple(OperationCode.Return, ret, -1, -1);
			}
			
			hasReturn = true;
			
			//Se genera un direcciÃ³n virtual para la funciÃ³n
			int virtualDir = VirtualStructure.getNext(VirtualStructure.VariableType.Global, retType);
			
			//Se aÃ±ade la funciÃ³n a la tabla de variables global
			procedureTable[programID].VariableTable.Add(functionName, new Variable(functionName, retType, virtualDir));
			
			
			
			
			Expect(27);
		}
		Expect(33);
		insertQuadruple(OperationCode.Ret, -1, -1, -1);
		
		
		//Si la funciÃ³n no es void y tiene return entonces hay error.
		if (retType != ReturnType.Void && !hasReturn)
		{
		SemErr("Error - Una funciÃ³n que no es void tiene que tener return.");
		finishExecution();
		}
		
		//Se resetean los contadores para que sean locales a cada funciÃ³n
		VirtualStructure.resetCounters();
		
		
		
	}

	void main() {
		Expect(8);
		Expect(7);
		actualProcedure = new Procedure("main", ReturnType.Main);
		procedureTable.Add("main", actualProcedure);
		procedureList.Add("main");
		
		//Se aÃ±ade un nuevo cuadruplo "Era" con el numero de procedimiento
		insertQuadruple(OperationCode.Era, findProcedure("main"), -1, -1);
		
		
		
		Expect(30);
		Expect(31);
		Expect(32);
		actualProcedure.InitialDir = quadruples.Count;
		//Se saca la posiciÃ³n del goto y se rellena con la direcciÃ³n inicial
		int got = PSaltos.Pop();
		quadruples[got].TemporalRegorJump = quadruples.Count;
		
		
		while (StartOf(1)) {
			vars();
		}
		while (StartOf(3)) {
			estatuto();
		}
		Expect(33);
		VirtualStructure.resetCounters();
		
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
			tryToInsertConstant(DataType.String);
			
		} else if (la.kind == 5) {
			Get();
			tryToInsertConstant(DataType.Char);
			
		} else SynErr(48);
	}

	void ctenum() {
		if (la.kind == 2) {
			Get();
			tryToInsertConstant(DataType.Int);
			
		} else if (la.kind == 3) {
			Get();
			tryToInsertConstant(DataType.Int);
			
		} else SynErr(49);
	}

	void ctebool() {
		if (la.kind == 24) {
			Get();
		} else if (la.kind == 25) {
			Get();
		} else SynErr(50);
		tryToInsertConstant(DataType.Bool);
		
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
		actualProcedure.Parameters.Add(type);
		
		Expect(1);
		if (actualProcedure.VariableTable.ContainsKey(t.val))
		{
		SemErr("Variable previamente declarada");
		}
		else
		{
		//Si no, entonces se aÃ±ade se la tabla de variables y se incrementa el contador de tamaÃ±o
		actualProcedure.VariableTable.Add(t.val, new Variable(t.val, type, VirtualStructure.getNext(scopeActual, type)));
		actualProcedure.increaseCounter(VirtualStructure.VariableType.Local, type);
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
		//Se resuelve generando un cuadruplo
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
			//Se resuelve generando un cuadruplo
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
		PSaltos.Push(quadruples.Count);
		
		Expect(30);
		expresion();
		int aux = PTipos.Pop();
		
		//Si este no es booleano, entonces se marca error
		if (aux != DataType.Bool)
		{
		SemErr("Error - Se esperaba un booleano en el while");
		finishExecution();
		}
		else
		{
		//Si si es booleano entonces se saca el resultado de la expresiÃ³n
		int res = PilaOperandos.Pop();
		//Se mete la posiciÃ³n del GotoF en la pila de Saltos
		PSaltos.Push(quadruples.Count);
		//Se genera el cuÃ¡druplo del GotoF con el resultado de la expresiÃ³n
		insertQuadruple(OperationCode.GotoF, res, -1, -1);
		
		
		}
		
		Expect(31);
		bloque();
		int falso = PSaltos.Pop();
		int retorno = PSaltos.Pop();
		
		//Se genera un nuevo cuÃ¡druplo Goto para regresar antes de la expresiÃ³n y se aÃ±ade a la lista de cuÃ¡druplos
		insertQuadruple(OperationCode.Goto, -1, -1, retorno);
		
		//Se rellena el cuadruplo del GotoF con la posiciÃ³n del siguiente cuÃ¡druplo
		quadruples[falso].TemporalRegorJump = quadruples.Count;
		
		
	}

	void ciclofor() {
		Expect(19);
		Expect(30);
		Expect(1);
		tryToInsertVariable();
		
		Expect(29);
		POper.Push(OperationCode.Assignment);
		
		expresion();
		int ladoDer = PilaOperandos.Pop();
		int ladoIzq = PilaOperandos.Pop();
		
		//Se obtiene el tipo resultante del lado izquierdo y del lado derecho, asÃ­ como el operador
		int tipoDer = PTipos.Pop();
		int tipoIzq = PTipos.Pop();
		int asigna = POper.Pop();
		
		//Si si se puede asignar
		if (tipoDer == tipoIzq)
		{		
			//Se genera un nuevo cuÃ¡druplo para realizar la asignaciÃ³n y se mete a la lista	
			insertQuadruple(asigna, ladoDer, -1, ladoIzq);
		
		}
		else
		{
			//Si no, entonces hay un error de semÃ¡ntica
			SemErr("Error - Tipos no compatibles en asignaciÃ³n.");
		}
		
		PSaltos.Push(quadruples.Count);		//Pointer a antes de condiciÃ³n
		
		
		Expect(27);
		expresion();
		int res = PilaOperandos.Pop();										//Resultado de evaluar la expresiÃ³n del for
		int tipo = PTipos.Pop();											//Se obtiene el tipo resultante de la expresiÃ³n
		
		
		//Se revisa que el tipo resultante sea booleano
		if (tipo != DataType.Bool)
		{
		//Si no lo es se marca error de semÃ¡ntica
		SemErr("Error - Se esperaba un booleano en el for");
		finishExecution();
		}
		else
		{
		PSaltos.Push(quadruples.Count);										//Pointer para regresar al GotoF
		insertQuadruple(OperationCode.GotoF, res, -1, -1);					//Genera cuadrupulo del GotoF
		PSaltos.Push(quadruples.Count);										//Pointer para regresar al Goto siguiente
		insertQuadruple(OperationCode.Goto, -1, -1, -1);					//Goto que sirve para saltar el incremento
		PSaltos.Push(quadruples.Count);										//Pointer a antes del incremento
		}
		
		
		
		Expect(27);
		expresion();
		int res2 = PilaOperandos.Pop();													//Resultado de lo que cambia la variable
		insertQuadruple(OperationCode.Assignment, res2, -1, ladoIzq);					//Se asigna el resultado de la expresion a la variable de control
		
		//Se sacan todos los datos que habiamos metido de la pila de saltos para crear el goto a antes de la condicion y rellenar el Goto que se saltaba la expresion de
		//la variable de control
		int antIncremento = PSaltos.Pop();										
		int saltoAEj = PSaltos.Pop();
		int gotoF = PSaltos.Pop();
		int antCondicion = PSaltos.Pop();
		
		insertQuadruple(OperationCode.Goto, -1, -1, antCondicion);
		
		
		quadruples[saltoAEj].TemporalRegorJump = quadruples.Count;
		
		//Se vuelven a meter los datos no usados al stack
		PSaltos.Push(gotoF);
		PSaltos.Push(antIncremento);
		
		
		Expect(31);
		bloque();
		antIncremento = PSaltos.Pop();
		insertQuadruple(OperationCode.Goto, -1, -1, antIncremento);
		
		//Se rellena el GotoF hacia la siguiente instrucciÃ³n a ejecutar
		gotoF = PSaltos.Pop();
		quadruples[gotoF].TemporalRegorJump = quadruples.Count;
		
		
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
		int aux = PTipos.Pop();
		
		//Se revisa que este tipo sea booleano
		if (aux != DataType.Bool)
		{
		//Si no lo es, se genera error
		SemErr("Error - Se esperaba un booleano en el if");
		finishExecution();
		}
		else
		{
		//Si si es, entonces se saca el resultado de la expresiÃ³n 
		int res = PilaOperandos.Pop();
		//Se aÃ±ade a la pila de saltos la posiciÃ³n del GotoF
		PSaltos.Push(quadruples.Count);
		//Se genera y aÃ±ade a la lista un nuevo cuÃ¡druplo GotoF con el resultado
		insertQuadruple(OperationCode.GotoF, res, -1, -1);
		
		
		
		}
		
		Expect(31);
		bloque();
		if (la.kind == 15) {
			Get();
			insertQuadruple(OperationCode.Goto, -1, -1, -1);
			
			//Se obtiene la posiciÃ³n del GotoF y se rellena con la siguiente posiciÃ³n de la lista
			int falso = PSaltos.Pop();
			quadruples[falso].TemporalRegorJump = quadruples.Count;
			
			//Se mete a la pila de saltos la posiciÃ³n del Goto 
			PSaltos.Push(quadruples.Count - 1);
			
			
			bloque();
		}
		int fin = PSaltos.Pop();
		quadruples[fin].TemporalRegorJump = quadruples.Count;
		
	}

	void escritura() {
		Expect(21);
		Expect(30);
		expresion();
		int resExp = PilaOperandos.Pop();
		insertQuadruple(OperationCode.Print, -1, -1, resExp);
		
		while (la.kind == 28) {
			Get();
			expresion();
			resExp = PilaOperandos.Pop();
			insertQuadruple(OperationCode.Print, -1, -1, resExp);
			
		}
		Expect(31);
		Expect(27);
	}

	void asignacionollamada() {
		Expect(1);
		string id = t.val; //Se guarda el id por si es una funciÃ³n
		
		if (la.kind == 29 || la.kind == 34) {
			tryToInsertVariable();
			
			if (la.kind == 34) {
				cuantificador();
			}
			Expect(29);
			POper.Push(OperationCode.Assignment);
			
			if (StartOf(4)) {
				expresion();
			} else if (la.kind == 22) {
				lectura();
			} else SynErr(54);
			int ladoDer = PilaOperandos.Pop();
			int ladoIzq = PilaOperandos.Pop();
			
			//Se saca el tipo del lado derecho y el tipo de lado izquierdo de la asignaciÃ³n
			int tipoDer = PTipos.Pop();
			int tipoIzq = PTipos.Pop();
			int asigna = POper.Pop();
			
			//Si lo tipos son iguales o acepta cualquier tipo
			if (tipoDer == tipoIzq || tipoDer == 0)
			{	
			//Se genera un nuevo cuÃ¡druplo y se mete a la lista		
			insertQuadruple(asigna, ladoDer, -1, ladoIzq);
			}
			else
			{
			//Si no, se genera un error de semÃ¡ntica
			SemErr("Error - Tipos no compatibles en asignaciÃ³n.");
			finishExecution();
			}	
			
		} else if (la.kind == 30) {
			Get();
			if (!procedureTable.ContainsKey(id))
			{
			//Se marca error de semÃ¡ntica
			SemErr("Error - La funciÃ³n que se intenta llamar no existe.");
			finishExecution();
			}
			else
			{
			if (procedureTable[id].Type != 0)
			{
				SemErr("Error - La llamada a la funciÃ³n tiene que ser void");
			}
			else
			{
				//Se aÃ±ade un nuevo cuadruplo "Era" con el numero de procedimiento
				insertQuadruple(OperationCode.Era, findProcedure(id), -1, -1);
			}
			
			}
			
			int k = 0; 					//Se inicializa el apuntador a parametros
			
			
			
			if (StartOf(4)) {
				expresion();
				tryToInsertArgument(k, id);
				
				while (la.kind == 28) {
					Get();
					k++;
					
					expresion();
					tryToInsertArgument(k, id);
					
				}
			}
			Expect(31);
			if (k + 1 < procedureTable[id].Parameters.Count)
			{
			SemErr("Error - El numero de parametros no coindice con la declaraciÃ³n de la funciÃ³n");
			}
			
			insertQuadruple(OperationCode.GoSub, findProcedure(id), procedureTable[id].InitialDir, -1);
			
		} else SynErr(55);
		Expect(27);
	}

	void lectura() {
		Expect(22);
		int tempString = VirtualStructure.getNext(VirtualStructure.VariableType.Temporal, DataType.String);
		PilaOperandos.Push(tempString);
		//Se mete un cero la pila de tipos ya que la lectura puede ser de cualquier tipo
		PTipos.Push(0);
		
		//Se genera un nuevo cuÃ¡druplo con el Readline y el tempString
		Quadruple qAux = new Quadruple(OperationCode.ReadLine, -1, -1, tempString);
		
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
			//Se resuelve generando un cuadruplo
			tryToGenerateQuadruple();
			}
			
		}
	}

	void exp() {
		term();
		if (POper.Count > 0 && (POper.Peek() == OperationCode.Sum || POper.Peek() == OperationCode.Substraction))
		{
		//Se resuelve generando un cuadruplo
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
			//Se resuelve generando un cuadruplo
			tryToGenerateQuadruple();
			}
			
		}
	}

	void term() {
		factor();
		if (POper.Count > 0 && (POper.Peek() == OperationCode.Multiplication || POper.Peek() == OperationCode.Division))
		{
		//Se resuelve generando un cuadruplo
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
			//Se resuelve generando un cuadruplo
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
				if (!procedureTable.ContainsKey(id))
				{
				//Se marca error de semÃ¡ntica
				SemErr("Error - La funciÃ³n que se intenta llamar no existe.");
				finishExecution();
				}
				else
				{
				if (procedureTable[id].Type == ReturnType.Void || procedureTable[id].Type == ReturnType.Program || procedureTable[id].Type == ReturnType.Main)
				{
					SemErr("Error - La funciÃ³n tiene que regresar algo para usarse como expresiÃ³n.");
				}
				else
				{
					//Se aÃ±ade un nuevo cuadruplo "Era" con el numero de procedimiento
					insertQuadruple(OperationCode.Era, findProcedure(id), -1, -1);
				}
				
				}
				
				int k = 0; 					//Se inicializa el apuntador a parametros
				
				if (StartOf(4)) {
					expresion();
					tryToInsertArgument(k, id);
					
					while (la.kind == 28) {
						Get();
						k++;
						
						expresion();
						tryToInsertArgument(k, id);
						
					}
				}
				Expect(31);
				if (k + 1 < procedureTable[id].Parameters.Count)
				{
				SemErr("Error - El numero de parametros no coindice con la declaraciÃ³n de la funciÃ³n");
				}
				insertQuadruple(OperationCode.GoSub, findProcedure(id), procedureTable[id].InitialDir, -1);
				
				Variable funcVariable = procedureTable[programID].VariableTable[id];
				
				int temporal = VirtualStructure.getNext(VirtualStructure.VariableType.Temporal, funcVariable.Type);
				PilaOperandos.Push(temporal);
				PTipos.Push(funcVariable.Type);
				insertQuadruple(OperationCode.Assignment, funcVariable.VirtualDir, -1, temporal);
				
				
				
				
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
