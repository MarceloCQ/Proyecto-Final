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
	public const int _MAQUEEQ = 42;
	public const int _MEQUEEQ = 43;
	public const int _DIFERENTE = 44;
	public const int _COMPARACION = 45;
	public const int maxT = 46;

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

	Procedure actualProcedure;								//Apunta al procedimiento actual en el que se está
	int tipoActual;
	int scopeActual = VirtualStructure.VariableType.Global;	//Apunta al scope actual, global o local
	string programID;										//Nombre del programa
	string valorConst;

	Stack<int> POper = new Stack<int>();					//Pila de operadores para la generación de cuadruplos
	Stack<int> PilaOperandos = new Stack<int>();			//Pila de operandos para la generación de cuádruplos
	Stack<int> PTipos = new Stack<int>();					//Pila de tipos para la validación de semántica
	Stack<int> PSaltos = new Stack<int>();					//Pila de saltos para guardar los Jumps o la referencia a un Goto/GotoF
	Stack<string> PilaVarDim = new Stack<string>();			//Pila que guarda el nombre de las variables dimensionadas que se agregaron en la pila de operandos


	List<Quadruple> quadruples = new List<Quadruple>();		//Lista de los cuádruplos del programa


	/// <summary>
	/// Método que inserta un nuevo cuadruplo y lo mete a la lista de cuadruplos
	/// </summary>
	private void insertQuadruple(int opCode, int oper1, int oper2, int TemporalRegorJump)
	{
		Quadruple qAux = new Quadruple(opCode, oper1, oper2, TemporalRegorJump);
		quadruples.Add(qAux);
	}


	/// <summary>
    /// Método que sirve para intentar generar un cuádruplo de las expresiones, revisa que se
    /// pueda realizar la operación y si no, manda un error de semántica. 
    /// </summary>
	private void tryToGenerateQuadruple()
	{
		int operat = POper.Pop();				//Se saca el operador de la pila de operadores
		int operand2 = PilaOperandos.Pop();		//Se saca el operando del lado derecho de la pila de operandos
		int tipo2 = PTipos.Pop();				//Se saca el tipo del operando del lado derecho de la pila de tipos
		int operand1 = PilaOperandos.Pop();		//Se saca el operando del lado izquierdo de la pila de operandos
		int tipo1 = PTipos.Pop();				//Se saca el tipo del operando del lado izquierdo de la pila de tipos


		//Si en cualquier lado de la pila de tipos hay un -1 significa que se uso la variable dimensionada sin indexar, se mearca error
		if (operand1 == -1 || operand2 == -1)
		{
			SemErr("Error - No se indexo la variable dimensionada.");
			finishExecution();

		}

		//Se obtiene el tipo resultante del cubo semántico al combinar ambos tipos con el operador
		int newType = SemanticCube.getCombiningType(tipo1, tipo2, operat);  

		//Se verifica que el tipo resultante no sea cero, es decir, que si se puedan combinar dichos tipos
		if (newType != 0)
		{
			int temp = VirtualStructure.getNext(VirtualStructure.VariableType.Temporal, newType); 	//Se obtiene la siguiente dirección temporal
			actualProcedure.increaseCounter(VirtualStructure.VariableType.Temporal, newType);	  	//Se incrementa el contador de tamaño de temporales					 	
			insertQuadruple(operat, operand1, operand2, temp); 										//Se genera un nuevo cuadruplo con los datos obtenidos		
			PilaOperandos.Push(temp);																//Se mete el temporal resultante a la pila de operandos
			PTipos.Push(newType);																	//Se mete el tipo resultante a a la pila de tipos
		}
		//Si no se pueden combinar
		else
		{
			//Se genera un error de semántica y se termina la ejecución
			SemErr("Error - Tipos incompatibles");
			finishExecution();
		}
	}

	/// <summary>
    /// Método que sirve para intentar insertar una variable en la pila de operandos 
    /// </summary>
	private Variable tryToInsertVariable()
	{
		//Si la variable no está en la tabla de variables del procedimiento correspondiente
		if (!actualProcedure.VariableTable.ContainsKey(t.val))
		{
			//Si la varaible no está en la tabla de variables del procedimiento global
			if (!procedureTable[programID].VariableTable.ContainsKey(t.val))
			{
				//Se genera un error de semántica
				SemErr("Variable no declarada");
				finishExecution();
			}
			//Si la variable se encuentra en la tabla de variables del procedimiento global
			else
			{
				//Se mete la dirección en la pila de operandos y el tipo en la pila de tipos.
				Variable v = procedureTable[programID].VariableTable[t.val];

				//Si la variable es dimensionada
				if (v.Dimensions.Count > 0)
				{
					//Se mete un -1 a la pila de operandos para saber que es dimensionada
					PilaOperandos.Push(-1);
					//Se mete el nombre a la pila de variables dimensionadas para poder accesarla
					PilaVarDim.Push(v.Name);

				}
				else
				{
					//Si es normal, se mete solamente a la pila de operandos
					PilaOperandos.Push(v.VirtualDir);
				}
				
				//Se mete el tipo de variable a pila de tipos
				PTipos.Push(v.Type);

				//Se regresa la variable
				return v;
			}
			
		}
		//Si la variable si se encuentra en la tabla de variables del procedimiento actual, se hace lo mismo
		else
		{
			//Se mete la dirección en la pila de operandos y el tipo en la pila de tipos.
			Variable v = actualProcedure.VariableTable[t.val];
			if (v.Dimensions.Count > 0)
			{
				PilaOperandos.Push(-1);
				PilaVarDim.Push(v.Name);

			}
			else
			{
				PilaOperandos.Push(v.VirtualDir);
			}
			PTipos.Push(v.Type);
			return v;
		}

		return null;
	}

	/// <summary>
    /// Método que sirve para intentar insertar una constante en la pila de operandos 
    /// </summary>
	private void tryToInsertConstant(int dataType, string val)
	{
		int virtualDir;							//Direccion virtual de la constante

		int index = constantTable.FindIndex(x => x.Name == val);

		//Si la constante ya se encuentra la lista
		if (index >= 0)
		{	
			//Se obtiene la dirección virtual
			virtualDir = constantTable[index].VirtualDir;
		}
		//Si no se encuentra en el diccionario
		else
		{
			//Se obtiene la dirección virtual de la estructura virtual
			virtualDir = VirtualStructure.getNext(VirtualStructure.VariableType.Constant, dataType);

			//Se crea la nueva constante
			Constant c = new Constant(val, virtualDir);

			//Se inserta en la tabla de constantes
			constantTable.Add(c);
		}
		
		//Se inserta la constante en la pila de operandos y en la pila de tipos
		PilaOperandos.Push(virtualDir);
		PTipos.Push(dataType);
	}

	private int tryToInsertArgument(int k, string id, bool refer)
	{
		//Se saca el argumento junto con su tipo
		int argument = PilaOperandos.Pop();
		int argumentType = PTipos.Pop();

		//Si el numero de procedimiento es menor a la cantidad de procedimientos
		if (k < procedureTable[id].Parameters.Count)
		{
			//Se revisa que el tipo coincida
			if (argumentType != procedureTable[id].Parameters[k].Type)
			{
				SemErr("Error - Los tipos de parámetros no coinciden con la función");
			}

			//Si el parametro usado en la llamada es por referencia y en la declaración no lo es, o viceversa, se marca error
			if (refer != procedureTable[id].Parameters[k].Reference)
			{
				if (refer)
				{
					SemErr("Error - El parámetro que se declaró en la función no es por referencia.");
				}
				else
				{
					SemErr("Error - El parámetro que se declaró en la función es por referencia.");
				}

			}


			Variable v = null;

			//Si el argumento es igual a -1, quiere decir que es una variable dimensionada
			if (argument == -1)
			{
				//Se obtiene la variable con la pila de variables dimensionadas
				v = actualProcedure.VariableTable[PilaVarDim.Pop()];

				//Se revisa que el numero de dimensiones del parametro coincida con lo declarado
				if (procedureTable[id].Parameters[k].Dimensions.Count != v.Dimensions.Count)
				{
					SemErr("Error - El numero de dimensiones en el parametro no coincide con lo declarado");
				}


				int i = 0;
				int tam = 1;

				//Se recorren todas las dimensiones de la declaración junto con las dimensionadas de la variable que se usó en la llamada
				foreach (Dimension d in procedureTable[id].Parameters[k].Dimensions)
				{
					//Si no coincide la dimension con lo declarado, se marca error
					if (d.Dim != v.Dimensions[i].Dim)
					{
						SemErr("Error - El tamaño de la matriz dimensionada tiene que ser igual al declarado.");
					}

					//Se obtiene el tamaño al multiplicar las dimensiones
					tam *= d.Dim;
					i++;
				}

				//Se añade un nuevo cuadruplo "Param" con el numero de procedimiento
				insertQuadruple(OperationCode.Param, v.VirtualDir, tam, procedureTable[id].Parameters[k].VirtualDir);					



			}
			else
			{
				//Se añade un nuevo cuadruplo "Param" con el numero de procedimiento
				insertQuadruple(OperationCode.Param, argument, -1, procedureTable[id].Parameters[k].VirtualDir);
			}
			
		}
		else
		{
			SemErr("El número de parámetros no coindice con la declaración de la función.");
		}

		//Se regresa el argumento
		return argument;

	}

	///<summary>
	/// Método que sirve para encontrar el numero de procedimiento
	///</summary>
	private int findProcedure(string name)
	{
		//Se recorren todos los procedimientos de la lista de procedimientos, y si el nombre coincide, se regresa el Ã­ndice
		for (int i = 0; i < procedureList.Count; i++)
		{
			if (procedureList[i] == name)
			{
				return i;
			}			
		}

		//Si no coincide se regresa -1
		return -1;
	}

	///<summary>
	/// Método que sirve para procesar cada dimension, es decir cuadruplificar la fórmula de indexamiento
	///</summary>

	private void processDimension(int dim, Dimension d, Variable vDim)
	{

		//Si en el tope de la pila de tipos no hay un entero se marca error porque el indice solo puede ser de tipo entero.
		if (PTipos.Peek() != DataType.Int)
		{
			SemErr("Error - El indexamiento para variables dimensioandas necesita ser de tipo entero.");
		}

		//Se inserta el cuádruplo para verificar que el indice caiga dentro de los rangos de la dimension correspondiente
		insertQuadruple(OperationCode.Verify, PilaOperandos.Peek(), -1, d.Dim);

		//Si aún hay otra dimension
		if (dim + 1 < vDim.Dimensions.Count)
		{
			//Se saca de la pila de operandos la s
			int s = PilaOperandos.Pop();
			//Se saca de la pila de tipos el tipo de la s
			int sTipo = PTipos.Pop();

			//Si en la pila de operandos hay un -1, quiere decir que la variable dimensionada no se indexo y se marca error
			if (s == -1)
			{
				SemErr("Error - No se indexo la variable dimensionada.");
			}

			//Se obtiene un temporal de la estructura y se incrementa el contador
			int vTemp = VirtualStructure.getNext(VirtualStructure.VariableType.Temporal, DataType.Int);
			actualProcedure.increaseCounter(VirtualStructure.VariableType.Temporal, DataType.Int);

			//Inserta la constante M a la lista de constantes para poder usar su dirección
			tryToInsertConstant(DataType.Int, d.M.ToString());

			//Se obtiene esa dirección
			int virtualDir = PilaOperandos.Pop();
			PTipos.Pop();

			//Se multiplica sn * mn para la fórmula de indexamiento
			insertQuadruple(OperationCode.Multiplication, s, virtualDir, vTemp);
			PilaOperandos.Push(vTemp);
			PTipos.Push(DataType.Int);

			
		}

		//Si no es la primera dimensión
		if (dim > 0)
		{

			//Se sacan ambos lados de la suma y sus tipos
			int aux2 = PilaOperandos.Pop();
			int aux1 = PilaOperandos.Pop();

			int tAux2 = PTipos.Pop();
			int tAux1 = PTipos.Pop();

			//Se obtiene un temporal
			int vTemp2 = VirtualStructure.getNext(VirtualStructure.VariableType.Temporal, DataType.Int);
			actualProcedure.increaseCounter(VirtualStructure.VariableType.Temporal, DataType.Int);

			//Se realiza la suma de mn-1 * sn-1 + mn * sn
			insertQuadruple(OperationCode.Sum, aux1, aux2, vTemp2);

			//Se mete el resultado a la pila de operandos
			PilaOperandos.Push(vTemp2);
			PTipos.Push(DataType.Int);

		}
	}

	///<summary>
	/// Método que sirve para finalizar la ejecución del programa. Usado en testing solamente
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
        throw new FatalError(t.line + "*" + msg);
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
		actualProcedure = new Procedure(t.val, ReturnType.Program);		//Se asigna como procedimiento actual al procedimiento global
		procedureTable.Add(t.val, actualProcedure);   					//Se añade este procedimiento a la tabla de procedimientos
		procedureList.Add(t.val);										//Se añade también a la lista de procedimientos
		programID = t.val;												//Se asigna el id del programa para uso posterior
		
		Expect(27);
		while (StartOf(1)) {
			vars();
		}
		scopeActual = VirtualStructure.VariableType.Local;				//Se asigna el scope como local cuando se acaba lo primero
		PSaltos.Push(quadruples.Count);									//Añade en la pila de saltos la posición en donde estará el goto al main
		insertQuadruple(OperationCode.Goto, -1, -1, -1);
		
		while (la.kind == 23) {
			funcion();
		}
		main();
		insertQuadruple(OperationCode.EndProg, -1, -1, -1);				//Se añade el ENDPROG al final del programa
		
	}

	void vars() {
		tipo();
		List<int> registros = new List<int>();		//Lista de registros que sirve para asignar las variables que tienen asignación inicial
		tipoActual = DataType.toDataType(t.val); 	//Se guarda el tipo en una variable
		
		if (la.kind == 1) {
			Get();
			if (actualProcedure.VariableTable.ContainsKey(t.val))
			{
			SemErr("Error - Variable " + t.val + " previamente declarada.");
			}
			//Si no estaba declarada
			else
			{
			//Se asigna una nueva dirección virtual
			int virtualDir = VirtualStructure.getNext(scopeActual, tipoActual);
			//Se añade a la lista de registros
			registros.Add(virtualDir);
			//Se añade la nueva variable a la tabla de variables del procedimiento en cuestión
			actualProcedure.VariableTable.Add(t.val, new Variable(t.val, tipoActual, virtualDir));
			//Se incrementa el contador de tamaño correspondiente
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
				} else if (StartOf(2)) {
					ctenum();
				} else if (la.kind == 24 || la.kind == 25) {
					ctebool();
				} else SynErr(47);
				foreach (int r in registros)
				{
				
				int index = constantTable.FindIndex(x => x.Name == valorConst);
				insertQuadruple(OperationCode.Assignment, constantTable[index].VirtualDir, -1, r);
				}
				
				
			}
		} else if (la.kind == 34) {
			Get();
			Expect(2);
			List<int> dimensions = new List<int>();
			int dim = int.Parse(t.val);
			dimensions.Add(dim);
			int r = dim;
			
			if (la.kind == 28) {
				Get();
				Expect(2);
				dim = int.Parse(t.val);
				dimensions.Add(dim);
				r *= dim;
				
				if (la.kind == 28) {
					Get();
					Expect(2);
					dim = int.Parse(t.val);
					dimensions.Add(dim);
					r *= dim;
					
				}
			}
			Expect(35);
			Expect(1);
			int tam = r;
			//Si la variable dimensionada se encuentra en la tabla de variables se marca error
			if (actualProcedure.VariableTable.ContainsKey(t.val))
			{
			SemErr("Error - Variable '" + t.val + "' previamente declarada");
			}
			
			//Si no, entonces se añade a la tabla de variables
			else
			{
			Variable vDim = new Variable(t.val, tipoActual, VirtualStructure.getNext(scopeActual, tipoActual));
			VirtualStructure.reserveSpaces(scopeActual, tipoActual, r - 1);
			actualProcedure.increaseCounterByX(VirtualStructure.VariableType.Local, tipoActual, r);
			
			foreach(int dimension in dimensions)
			{
			r = r / dimension;
			vDim.Dimensions.Add(new Dimension(dimension, r));
			}
			
			actualProcedure.VariableTable.Add(t.val, vDim);
			
			}
			
			while (la.kind == 28) {
				Get();
				Expect(1);
				r = tam;
				Variable vDim = new Variable(t.val, tipoActual, VirtualStructure.getNext(scopeActual, tipoActual));
				VirtualStructure.reserveSpaces(scopeActual, tipoActual, r - 1);
				actualProcedure.increaseCounterByX(VirtualStructure.VariableType.Local, tipoActual, r);
				
				foreach(int dimension in dimensions)
				{
				r = r / dimension;
				vDim.Dimensions.Add(new Dimension(dimension, r));
				}
				
				actualProcedure.VariableTable.Add(t.val, vDim);
				
			}
		} else SynErr(48);
		Expect(27);
	}

	void funcion() {
		Expect(23);
		regresa();
		int retType = ReturnType.toReturnType(t.val);
		
		
		
		bool hasReturn = false;
		
		
		
		Expect(1);
		string functionName = t.val;
		int virtualDir = -1;
		//Se revise que la función no haya sido previamente declarada en el diccionario de procedimientos
		if (procedureTable.ContainsKey(t.val))
		{
		SemErr("Error - Funcion '" + t.val + "' previamente declarada.");
		}
		else
		{
		//Si no, se añade a la tabla de procedimientos
		actualProcedure = new Procedure(t.val, retType);
		procedureTable.Add(t.val, actualProcedure);
		procedureList.Add(t.val);
		
		if (retType != ReturnType.Void)
		{
			//Se genera un dirección virtual para la función
			virtualDir = VirtualStructure.getNext(VirtualStructure.VariableType.Global, retType);
		
			//Se añade la función a la tabla de variables global
			procedureTable[programID].VariableTable.Add(functionName, new Variable(functionName, retType, virtualDir));
		
			//Se añade al tamaño de lo global
			procedureTable[programID].increaseCounter(VirtualStructure.VariableType.Local, retType);
		
		}
		
		
		
		}
		
		
		Expect(30);
		if (StartOf(3)) {
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
		while (StartOf(4)) {
			estatuto();
		}
		if (la.kind == 20) {
			Get();
			expresion();
			int returnType = PTipos.Pop();
			int ret = PilaOperandos.Pop();
			
			//Si lo que se sacó es un -1, significa que la variable dimensionada no se indexó
			if (ret == -1)
			{
			SemErr("Error - No se indexó la variable dimensionada.");
			}
			
			//Si el tipo de la expresión no coincide con el tipo de retorno se marca error
			if (returnType != retType)
			{
			SemErr("Error - Tipos incompatibles en return.");
			}
			else
			{
			//Se prende la bandera de retorno para al final revisar si coincide con lo que se declaró
			hasReturn = true;
			
			
			//Si si coincide se genera un nuevo cuádruplo
			insertQuadruple(OperationCode.Assignment, ret, -1, virtualDir);
			}
			
			Expect(27);
		}
		Expect(33);
		insertQuadruple(OperationCode.Ret, -1, -1, -1);
		
		
		//Si la función no es void y tiene return entonces hay error.
		if (retType != ReturnType.Void && !hasReturn)
		{
		SemErr("Error - Una función que no es void tiene que tener return.");
		}
		
		//Se resetean los contadores para que sean locales a cada función
		VirtualStructure.resetCounters();
		
		
		
	}

	void main() {
		Expect(8);
		Expect(7);
		actualProcedure = new Procedure("main", ReturnType.Main);
		procedureTable.Add("main", actualProcedure);
		procedureList.Add("main");
		
		//Se añade un nuevo cuadruplo "Era" con el numero de procedimiento
		insertQuadruple(OperationCode.Era, findProcedure("main"), 1, -1);
		
		
		
		Expect(30);
		Expect(31);
		Expect(32);
		actualProcedure.InitialDir = quadruples.Count;
		//Se saca la posición del goto y se rellena con la dirección inicial
		int got = PSaltos.Pop();
		quadruples[got].TemporalRegorJump = quadruples.Count - 1;
		
		
		while (StartOf(1)) {
			vars();
		}
		while (StartOf(4)) {
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
		} else SynErr(49);
	}

	void ctelet() {
		if (la.kind == 4) {
			Get();
			tryToInsertConstant(DataType.String, t.val.Replace("\"", "").Replace(@"\n", "\n"));
			valorConst = t.val;
			
		} else if (la.kind == 5) {
			Get();
			tryToInsertConstant(DataType.Char, t.val);
			valorConst = t.val;
			
		} else SynErr(50);
	}

	void ctenum() {
		bool negativo = false;
		
		if (la.kind == 36 || la.kind == 37) {
			if (la.kind == 36) {
				Get();
			} else {
				Get();
				negativo = true;
				
			}
		}
		if (la.kind == 2) {
			Get();
			tryToInsertConstant(DataType.Int, (negativo? "-" : "") + t.val);
			valorConst = (negativo? "-" : "") + t.val;
			
		} else if (la.kind == 3) {
			Get();
			tryToInsertConstant(DataType.Float, (negativo? "-" : "") + t.val);
			valorConst = (negativo? "-" : "") + t.val;
			
		} else SynErr(51);
	}

	void ctebool() {
		if (la.kind == 24) {
			Get();
		} else if (la.kind == 25) {
			Get();
		} else SynErr(52);
		tryToInsertConstant(DataType.Bool, t.val);
		valorConst = t.val;
		
	}

	void regresa() {
		if (StartOf(1)) {
			tipo();
		} else if (la.kind == 8) {
			Get();
		} else SynErr(53);
	}

	void parametro() {
		bool refer = false;
		
		if (la.kind == 26) {
			Get();
			refer = true;
			
		}
		tipo();
		int type = DataType.toDataType(t.val);
		
		
		if (la.kind == 1) {
			Get();
			if (actualProcedure.VariableTable.ContainsKey(t.val))
			{
			SemErr("Variable previamente declarada.");
			}
			else
			{
			//Si no, entonces se añade se la tabla de variables y se incrementa el contador de tamaño
			int virtDir = VirtualStructure.getNext(scopeActual, type);
			actualProcedure.VariableTable.Add(t.val, new Variable(t.val, type, virtDir));
			actualProcedure.Parameters.Add(new Parameter(type, virtDir, refer));
			actualProcedure.increaseCounter(VirtualStructure.VariableType.Local, type);
			}
			
		} else if (la.kind == 34) {
			Get();
			Expect(2);
			if (refer)
			{
			SemErr("No se pueden mandar variables dimensionadas como parámetros por referencia.");
			}
			
			//Se crea una nueva lista de dimensiones y se añade la primera dimensión que se encontró
			List<int> dimensions = new List<int>();
			int dim = int.Parse(t.val);
			dimensions.Add(dim);
			int r = dim;
			
			if (la.kind == 28) {
				Get();
				Expect(2);
				dim = int.Parse(t.val);
				dimensions.Add(dim);
				r *= dim;
				
				if (la.kind == 28) {
					Get();
					Expect(2);
					dim = int.Parse(t.val);
					dimensions.Add(dim);
					r *= dim;
					
				}
			}
			Expect(35);
			Expect(1);
			int tam = r;
				//Si la variable dimensionada se encuentra en la tabla de variables se marca error
			if (actualProcedure.VariableTable.ContainsKey(t.val))
			{
			SemErr("Error - Variable '" + t.val + "' previamente declarada");
			}
			
			//Si no, entonces se añade a la tabla de variables
			else
			{
			Variable vDim = new Variable(t.val, type, VirtualStructure.getNext(scopeActual, type));
			VirtualStructure.reserveSpaces(scopeActual, type, r - 1);
			actualProcedure.increaseCounterByX(VirtualStructure.VariableType.Local, type, r);
			
			//También se añade el parametro a la tabla de parametros 
			Parameter p = new Parameter(type, vDim.VirtualDir, false);
			p.Dimensions = vDim.Dimensions;
			actualProcedure.Parameters.Add(p);
			
			//Se recorren todas las dimensiones para sacar las Ms y guardarlas
			foreach(int dimension in dimensions)
			{
				r = r / dimension;
				vDim.Dimensions.Add(new Dimension(dimension, r));
			}
			
			//Se añade la variable a la tabla de variables del procedimiento
			actualProcedure.VariableTable.Add(t.val, vDim);
			
			}
			
		} else SynErr(54);
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
		} else SynErr(55);
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
		while (StartOf(4)) {
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
		SemErr("Error - Se esperaba un booleano en el while.");
		finishExecution();
		}
		else
		{
		//Si si es booleano entonces se saca el resultado de la expresión
		int res = PilaOperandos.Pop();
		
		if (res == -1)
		{
			SemErr("Error - No se indexo la variable dimensionada.");
			finishExecution();
		}
		//Se mete la posición del GotoF en la pila de Saltos
		PSaltos.Push(quadruples.Count);
		//Se genera el cuádruplo del GotoF con el resultado de la expresión
		insertQuadruple(OperationCode.GotoF, res, -1, -1);
		
		
		}
		
		Expect(31);
		bloque();
		int falso = PSaltos.Pop();
		int retorno = PSaltos.Pop();
		
		//Se genera un nuevo cuádruplo Goto para regresar antes de la expresión y se añade a la lista de cuádruplos
		insertQuadruple(OperationCode.Goto, -1, -1, retorno);
		
		//Se rellena el cuadruplo del GotoF con la posición del siguiente cuádruplo
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
		
		
		if (ladoIzq == -1 || ladoDer == -1)
		{
			SemErr("Error - No se indexo la variable dimensionada.");
		finishExecution();
		}
		
		//Se obtiene el tipo resultante del lado izquierdo y del lado derecho, asÃ­ como el operador
		int tipoDer = PTipos.Pop();
		int tipoIzq = PTipos.Pop();
		int asigna = POper.Pop();
		
		//Si si se puede asignar
		if (tipoDer == tipoIzq)
		{		
			//Se genera un nuevo cuádruplo para realizar la asignación y se mete a la lista	
			insertQuadruple(asigna, ladoDer, -1, ladoIzq);
		
		}
		else
		{
			//Si no, entonces hay un error de semántica
			SemErr("Error - Tipos no compatibles en asignación.");
		}
		
		PSaltos.Push(quadruples.Count);		//Pointer a antes de condición
		
		
		Expect(27);
		expresion();
		int res = PilaOperandos.Pop();										//Resultado de evaluar la expresión del for
		int tipo = PTipos.Pop();											//Se obtiene el tipo resultante de la expresión
		
		if (res == -1)
		{
		SemErr("Error - No se indexo la variable dimensionada.");
		finishExecution();
		}
		
		//Se revisa que el tipo resultante sea booleano
		if (tipo != DataType.Bool)
		{
		//Si no lo es se marca error de semántica
		SemErr("Error - Se esperaba un booleano en el for.");
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
		
		if (res2 == -1)
		{
		SemErr("Error - No se indexo la variable dimensionada.");
		finishExecution();
		}
		
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
		
		//Se rellena el GotoF hacia la siguiente instrucción a ejecutar
		gotoF = PSaltos.Pop();
		quadruples[gotoF].TemporalRegorJump = quadruples.Count;
		
		
	}

	void ciclos() {
		if (la.kind == 18) {
			ciclowhile();
		} else if (la.kind == 19) {
			ciclofor();
		} else SynErr(56);
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
		//Si si es, entonces se saca el resultado de la expresión 
		int res = PilaOperandos.Pop();
		
		if (res == -1)
		{
			SemErr("Error - No se indexo la variable dimensionada.");
			finishExecution();
		}
		
		//Se añade a la pila de saltos la posición del GotoF
		PSaltos.Push(quadruples.Count);
		//Se genera y añade a la lista un nuevo cuádruplo GotoF con el resultado
		insertQuadruple(OperationCode.GotoF, res, -1, -1);
		
		
		
		}
		
		Expect(31);
		bloque();
		if (la.kind == 15) {
			Get();
			insertQuadruple(OperationCode.Goto, -1, -1, -1);
			
			//Se obtiene la posición del GotoF y se rellena con la siguiente posición de la lista
			int falso = PSaltos.Pop();
			quadruples[falso].TemporalRegorJump = quadruples.Count;
			
			//Se mete a la pila de saltos la posición del Goto 
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
		
		//Si el tope de la pila es un -1 quiere decir que era una variable dimensionada sin indexar
		if (resExp == -1)
		{
		SemErr("Error - No se indexo la variable dimensionada.");
		finishExecution();
		}
		
		//Se genera el cuádruplo del print
		insertQuadruple(OperationCode.Print, -1, -1, resExp);
		
		while (la.kind == 28) {
			Get();
			expresion();
			resExp = PilaOperandos.Pop();
			
			if (resExp == -1)
			{
			SemErr("Error - No se indexo la variable dimensionada.");
			finishExecution();
			}
			
			insertQuadruple(OperationCode.Print, -1, -1, resExp);
			
		}
		Expect(31);
		Expect(27);
	}

	void asignacionollamada() {
		Expect(1);
		string id = t.val; //Se guarda el id por si es una función
		
		if (la.kind == 29 || la.kind == 34) {
			tryToInsertVariable();
			
			if (la.kind == 34) {
				PilaOperandos.Pop();
				PTipos.Pop();
				
				cuantificador();
			}
			Expect(29);
			POper.Push(OperationCode.Assignment);
			
			if (StartOf(5)) {
				expresion();
				int ladoDer = PilaOperandos.Pop();
				int ladoIzq = PilaOperandos.Pop();
				
				if (ladoDer == -1 || ladoIzq == -1)
				{
				SemErr("Error - No se indexo la variable dimensionada.");
				}
				
				//Se saca el tipo del lado derecho y el tipo de lado izquierdo de la asignación
				int tipoDer = PTipos.Pop();
				int tipoIzq = PTipos.Pop();
				int asigna = POper.Pop();
				
				//Si lo tipos son iguales o acepta cualquier tipo
				if (tipoDer == tipoIzq || tipoDer == 0)
				{	
				//Se genera un nuevo cuádruplo y se mete a la lista		
				insertQuadruple(asigna, ladoDer, -1, ladoIzq);
				}
				else
				{
				//Si no, se genera un error de semántica
				SemErr("Error - Tipos no compatibles en asignación.");
				finishExecution();
				}	
				
			} else if (la.kind == 22) {
				lectura();
				int ladoIzq = PilaOperandos.Pop();
				int tipoIzq = PTipos.Pop();
				POper.Pop();
				
				//Si es lectura se genera un cuadruplo para guardar en la variable lo que se lea
				insertQuadruple(OperationCode.Read, tipoIzq, -1, ladoIzq);
				
				
				
			} else SynErr(57);
		} else if (la.kind == 30) {
			Get();
			List<int> refParams = new List<int>();
			//Si el procedimiento no se encuentra en el diccionario de procedimientos
			if (!procedureTable.ContainsKey(id))
			{
			//Se marca error de semántica
			SemErr("Error - La función que se intenta llamar no existe.");
			finishExecution();
			}
			else
			{
			//Si la función que se intentó llamar se declaró como algo diferente a void entonces hay error
			if (procedureTable[id].Type != 0)
			{
				SemErr("Error - La llamada a la función tiene que ser a una función void.");
			}
			else
			{
				//Se añade un nuevo cuadruplo "Era" con el numero de procedimiento
				insertQuadruple(OperationCode.Era, findProcedure(id), -1, -1);
			}
			
			}
			
			
			int k = 0; 					//Se inicializa el apuntador a parametros
			List<int> referenceList = new List<int>();
			
			
			
			if (StartOf(6)) {
				if (la.kind == 26) {
					Get();
					Expect(1);
					Variable v = tryToInsertVariable();
					//Se mete también a los argumentos
					tryToInsertArgument(k, id, true);
					//Se mete a la lista de parametros por referencia
					referenceList.Add(v.VirtualDir);
					
				} else {
					expresion();
					tryToInsertArgument(k, id, false);
					
				}
				while (la.kind == 28) {
					Get();
					k++;
					
					if (la.kind == 26) {
						Get();
						Expect(1);
						Variable v = tryToInsertVariable();
						tryToInsertArgument(k, id, true);
						referenceList.Add(v.VirtualDir);
						
					} else if (StartOf(5)) {
						expresion();
						tryToInsertArgument(k, id, false);
						
					} else SynErr(58);
				}
			}
			Expect(31);
			if (k + 1 < procedureTable[id].Parameters.Count)
			{
			SemErr("Error - El número de parámetros no coindice con la declaración de la función.");
			}
			
			//Se inserta el cuádruplo de ir a la función
			insertQuadruple(OperationCode.GoSub, findProcedure(id), -1, procedureTable[id].InitialDir);
			
			//Se recorren todos los parametros por referencia para agregar los cuadruplos ref correspondientes correspondientes
			int i = 0;
			foreach (Parameter p in procedureTable[id].Parameters)
			{
			if (p.Reference)
			{
				insertQuadruple(OperationCode.Ref, p.VirtualDir, referenceList[i], -1);
				i++;
			}
			}
			
			//Se genera un nuevo cuadruplo que indica que se terminó la función
			insertQuadruple(OperationCode.EndFunc, -1, -1, -1);
			
		} else SynErr(59);
		Expect(27);
	}

	void lectura() {
		Expect(22);
		Expect(30);
		Expect(31);
	}

	void cuantificador() {
		string id = t.val;
		int dim = 0;
		Dimension d = null;
		
		//Se mete la variable a la pila de operadores para revisar que exista
		Variable vDim = tryToInsertVariable();
		
		//Se saca la direccion base y el tipo de las pilas
		int dirBase = PilaOperandos.Pop();
		int tipo = PTipos.Pop();
		
		//Si la variable declarada no tiene dimensiones, se marca error.
		if (vDim.Dimensions.Count == 0)
		{
		SemErr("Error - La variable que se intenta accesar no es dimensionada.");
		}
		else
		{
		//Si si tiene dimensiones entonces se inicializa el apuntador de dimension en 0 y se mete a la pila de operadores un fondo falso
		dim = 0;
		d = vDim.Dimensions[dim];
		POper.Push(-1);
		}
		
		
		Expect(34);
		expresion();
		processDimension(dim, d, vDim);
		
		if (la.kind == 28) {
			Get();
			dim++;
			
			//Si no coincide el número de dimensiones con lo declarado, entonces se marca error.
			if (dim + 1 > vDim.Dimensions.Count)
			{
			SemErr("Error - El número de dimensiones no coincide con lo declarado.");
			}
			
			//Se obtiene la siguiente dimensión
			d = vDim.Dimensions[dim];
			
			expresion();
			processDimension(dim, d, vDim);
			
			if (la.kind == 28) {
				Get();
				dim++;
				
				if (dim + 1 > vDim.Dimensions.Count)
				{
				SemErr("Error - El número de dimensiones no coincide con lo declarado.");
				}
				
				d = vDim.Dimensions[dim];
				
				expresion();
				processDimension(dim, d, vDim);
				
			}
		}
		Expect(35);
		if (dim + 1 != vDim.Dimensions.Count)
		{
		SemErr("Error - El número de dimensiones no coincide con lo declarado.");
		}
		
		//Se saca de la pila de operandos lo utlimo que se calculo para ya solo sumarle la direccion base
		int aux1 = PilaOperandos.Pop();
		int tAux1 = PTipos.Pop();
		
		//Se obtiene un nuevo temporal
		int vTemp3 = VirtualStructure.getNext(VirtualStructure.VariableType.Temporal, DataType.Int);
		actualProcedure.increaseCounter(VirtualStructure.VariableType.Temporal, DataType.Int);
		
		//Se mete la dirección base a la tabla de constantes
		tryToInsertConstant(DataType.Int, vDim.VirtualDir.ToString());
		
		int vDir = PilaOperandos.Pop();
		PTipos.Pop();
		
		//Se hace la suma de lo calculado con la dirección base
		insertQuadruple(OperationCode.Sum, aux1, vDir, vTemp3);
		PTipos.Push(vDim.Type);
		
		//Se mete a la pila de operandos la dirección donde se encuentra la dirección (por eso se ingresa negativo) de lo que se quiere accesar 
		PilaOperandos.Push(-vTemp3);
		
		POper.Pop();
		
	}

	void comparacion() {
		exp();
		if (StartOf(7)) {
			switch (la.kind) {
			case 40: {
				Get();
				break;
			}
			case 41: {
				Get();
				break;
			}
			case 42: {
				Get();
				break;
			}
			case 43: {
				Get();
				break;
			}
			case 44: {
				Get();
				break;
			}
			case 45: {
				Get();
				break;
			}
			}
			POper.Push(OperationCode.toOperationCode(t.val));
			
			exp();
			if (POper.Count > 0 && (POper.Peek() == OperationCode.MoreThan || POper.Peek() == OperationCode.LessThan || POper.Peek() == OperationCode.MoreThanEq || POper.Peek() == OperationCode.LessThanEq || POper.Peek() == OperationCode.Different || POper.Peek() == OperationCode.EqualComparison))
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
			
		} else if (StartOf(8)) {
			ctevar();
		} else SynErr(60);
	}

	void ctevar() {
		if (la.kind == 4 || la.kind == 5) {
			ctelet();
		} else if (la.kind == 24 || la.kind == 25) {
			ctebool();
		} else if (StartOf(2)) {
			ctenum();
		} else if (la.kind == 1) {
			Get();
			string id = t.val;
			
			if (la.kind == 30) {
				Get();
				POper.Push(-1);
				
				//Si el procedimiento no se encuentra en el diccionario de procedimientos
				if (!procedureTable.ContainsKey(id))
				{
				//Se marca error de semántica
				SemErr("Error - La función que se intenta llamar no existe.");
				finishExecution();
				}
				else
				{
				if (procedureTable[id].Type == ReturnType.Void || procedureTable[id].Type == ReturnType.Program || procedureTable[id].Type == ReturnType.Main)
				{
					SemErr("Error - La función tiene que regresar algo para usarse como expresión.");
				}
				else
				{
					//Se añade un nuevo cuadruplo "Era" con el numero de procedimiento
					insertQuadruple(OperationCode.Era, findProcedure(id), -1, -1);
				}
				
				}
				
				int k = 0; 					//Se inicializa el apuntador a parametros
				List<int> referenceList = new List<int>();
				
				
				if (StartOf(6)) {
					if (la.kind == 26) {
						Get();
						Expect(1);
						Variable v = tryToInsertVariable();
						tryToInsertArgument(k, id, true);
						referenceList.Add(v.VirtualDir);
						
					} else {
						expresion();
						tryToInsertArgument(k, id, false);
						
					}
					while (la.kind == 28) {
						Get();
						k++;
						
						
						if (la.kind == 26) {
							Get();
							Expect(1);
							Variable v = tryToInsertVariable();
							tryToInsertArgument(k, id, true);
							referenceList.Add(v.VirtualDir);
							
						} else if (StartOf(5)) {
							expresion();
							tryToInsertArgument(k, id, false);
							
						} else SynErr(61);
					}
				}
				Expect(31);
				if (k + 1 < procedureTable[id].Parameters.Count)
				{
				SemErr("Error - El número de parámetros no coindice con la declaración de la función");
				}
				
				//Se inserta el cuádruplo de GoSub para regresar en donde se habia quedado
				insertQuadruple(OperationCode.GoSub, findProcedure(id), -1, procedureTable[id].InitialDir);
				
				//Se recorren todos los parametros por referencia para insertar el cuadruplo ref
				int i = 0;
				foreach (Parameter p in procedureTable[id].Parameters)
				{
				if (p.Reference)
				{
				insertQuadruple(OperationCode.Ref, p.VirtualDir, referenceList[i], -1);
				i++;
				}
				}
				
				//Se genera el cuadruplo para indicar que se acabo la función y no hay mas parametros por referencia
				insertQuadruple(OperationCode.EndFunc, -1, -1, -1);
				
				//Se saca la variable global que corresponde a la función
				Variable funcVariable = procedureTable[programID].VariableTable[id];
				
				//Se genera un nuevo temporal
				int temporal = VirtualStructure.getNext(VirtualStructure.VariableType.Temporal, funcVariable.Type);
				actualProcedure.increaseCounter(VirtualStructure.VariableType.Temporal, funcVariable.Type);
				
				//Se asigna el valor de la variable de la función al nuevo temporal y se mete a la pila de operandos.
				PilaOperandos.Push(temporal);
				PTipos.Push(funcVariable.Type);
				insertQuadruple(OperationCode.Assignment, funcVariable.VirtualDir, -1, temporal);
				
				POper.Pop();
				
				
				
			} else if (la.kind == 34) {
				cuantificador();
			} else if (StartOf(9)) {
				tryToInsertVariable();
				
			} else SynErr(62);
		} else SynErr(63);
	}



	public Programa Parse() {
		la = new Token();
		la.val = "";		
		Get();
		PLearning();
		Expect(0);
        return new Programa(programID, procedureTable, procedureList, constantTable, quadruples);

    }
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_T,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_x, _x,_x,_T,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_x,_x},
		{_x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_x,_T, _x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_x}

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
			case 42: s = "MAQUEEQ expected"; break;
			case 43: s = "MEQUEEQ expected"; break;
			case 44: s = "DIFERENTE expected"; break;
			case 45: s = "COMPARACION expected"; break;
			case 46: s = "??? expected"; break;
			case 47: s = "invalid vars"; break;
			case 48: s = "invalid vars"; break;
			case 49: s = "invalid tipo"; break;
			case 50: s = "invalid ctelet"; break;
			case 51: s = "invalid ctenum"; break;
			case 52: s = "invalid ctebool"; break;
			case 53: s = "invalid regresa"; break;
			case 54: s = "invalid parametro"; break;
			case 55: s = "invalid estatuto"; break;
			case 56: s = "invalid ciclos"; break;
			case 57: s = "invalid asignacionollamada"; break;
			case 58: s = "invalid asignacionollamada"; break;
			case 59: s = "invalid asignacionollamada"; break;
			case 60: s = "invalid factor"; break;
			case 61: s = "invalid ctevar"; break;
			case 62: s = "invalid ctevar"; break;
			case 63: s = "invalid ctevar"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;

        throw new FatalError(line + "*" + s);
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
