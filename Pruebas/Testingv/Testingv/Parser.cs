
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
	public const int _PRINT = 20;
	public const int _READLINE = 21;
	public const int _FUNCTION = 22;
	public const int _TRUE = 23;
	public const int _FALSE = 24;
	public const int _REF = 25;
	public const int _PYC = 26;
	public const int _COMA = 27;
	public const int _IGUAL = 28;
	public const int _PARAB = 29;
	public const int _PARCI = 30;
	public const int _LLAVEAB = 31;
	public const int _LLAVECI = 32;
	public const int _CORCHAB = 33;
	public const int _CORCHCI = 34;
	public const int _MAS = 35;
	public const int _MENOS = 36;
	public const int _MULT = 37;
	public const int _DIV = 38;
	public const int _MAQUE = 39;
	public const int _MEQUE = 40;
	public const int _DIFERENTE = 41;
	public const int _COMPARACION = 42;
	public const int maxT = 43;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;



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
		Expect(1);
		Expect(26);
		while (StartOf(1)) {
			vars();
		}
		while (la.kind == 22) {
			funcion();
		}
		main();
	}

	void vars() {
		tipo();
		if (la.kind == 1) {
			Get();
			while (la.kind == 27) {
				Get();
				Expect(1);
			}
			if (la.kind == 28) {
				Get();
				if (la.kind == 4 || la.kind == 5) {
					ctelet();
				} else if (la.kind == 2 || la.kind == 3) {
					ctenum();
				} else if (la.kind == 23 || la.kind == 24) {
					ctebool();
				} else SynErr(44);
			}
			Expect(26);
		} else if (la.kind == 33) {
			Get();
			Expect(2);
			if (la.kind == 27) {
				Get();
				Expect(2);
			}
			Expect(34);
			Expect(1);
			while (la.kind == 27) {
				Get();
				Expect(1);
			}
			Expect(26);
		} else SynErr(45);
	}

	void funcion() {
		Expect(22);
		regresa();
		Expect(1);
		Expect(29);
		if (StartOf(2)) {
			parametro();
			while (la.kind == 27) {
				Get();
				parametro();
			}
		}
		Expect(30);
		bloque();
	}

	void main() {
		Expect(8);
		Expect(7);
		Expect(29);
		Expect(30);
		bloque();
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
		} else SynErr(46);
	}

	void ctelet() {
		if (la.kind == 4) {
			Get();
		} else if (la.kind == 5) {
			Get();
		} else SynErr(47);
	}

	void ctenum() {
		if (la.kind == 2) {
			Get();
		} else if (la.kind == 3) {
			Get();
		} else SynErr(48);
	}

	void ctebool() {
		if (la.kind == 23) {
			Get();
		} else if (la.kind == 24) {
			Get();
		} else SynErr(49);
	}

	void regresa() {
		if (StartOf(1)) {
			tipo();
		} else if (la.kind == 8) {
			Get();
		} else SynErr(50);
	}

	void parametro() {
		if (la.kind == 25) {
			Get();
		}
		tipo();
		Expect(1);
		if (la.kind == 33) {
			Get();
			Expect(34);
		}
	}

	void bloque() {
		Expect(31);
		while (StartOf(3)) {
			estatuto();
		}
		Expect(32);
	}

	void estatuto() {
		if (la.kind == 14) {
			condicion();
		} else if (la.kind == 20) {
			escritura();
		} else if (la.kind == 21) {
			lectura();
		} else if (la.kind == 18 || la.kind == 19) {
			ciclos();
		} else if (la.kind == 1) {
			asignacionollamada();
		} else SynErr(51);
	}

	void while() {
		Expect(18);
		Expect(29);
		expresion();
		Expect(30);
		bloque();
	}

	void expresion() {
		comparacion();
		while (la.kind == 16 || la.kind == 17) {
			if (la.kind == 16) {
				Get();
			} else {
				Get();
			}
			comparacion();
		}
	}

	void for() {
		Expect(19);
		Expect(29);
		Expect(1);
		Expect(28);
		expresion();
		Expect(26);
		expresion();
		Expect(26);
		expresion();
		Expect(30);
		bloque();
	}

	void ciclos() {
		if (la.kind == 18) {
			while();
		} else if (la.kind == 19) {
			for();
		} else SynErr(52);
	}

	void condicion() {
		Expect(14);
		Expect(29);
		expresion();
		Expect(30);
		bloque();
		if (la.kind == 15) {
			Get();
			bloque();
		}
	}

	void escritura() {
		Expect(20);
		Expect(29);
		expresion();
		while (la.kind == 27) {
			Get();
			expresion();
		}
		Expect(30);
		Expect(26);
	}

	void lectura() {
		Expect(21);
		Expect(29);
		Expect(30);
		Expect(26);
	}

	void asignacionollamada() {
		Expect(1);
		if (la.kind == 28 || la.kind == 33) {
			if (la.kind == 33) {
				cuantificador();
			}
			Expect(28);
			if (StartOf(4)) {
				expresion();
			} else if (la.kind == 21) {
				lectura();
			} else SynErr(53);
		} else if (la.kind == 29) {
			Get();
			if (StartOf(4)) {
				expresion();
				while (la.kind == 27) {
					Get();
					expresion();
				}
			}
			Expect(30);
		} else SynErr(54);
	}

	void cuantificador() {
		Expect(33);
		expresion();
		if (la.kind == 27) {
			Get();
			expresion();
		}
		Expect(34);
	}

	void comparacion() {
		exp();
		if (StartOf(5)) {
			if (la.kind == 39) {
				Get();
			} else if (la.kind == 40) {
				Get();
			} else if (la.kind == 41) {
				Get();
			} else {
				Get();
			}
			exp();
		}
	}

	void exp() {
		term();
		while (la.kind == 35 || la.kind == 36) {
			if (la.kind == 35) {
				Get();
			} else {
				Get();
			}
			term();
		}
	}

	void term() {
		factor();
		while (la.kind == 37 || la.kind == 38) {
			if (la.kind == 37) {
				Get();
			} else {
				Get();
			}
			factor();
		}
	}

	void factor() {
		if (la.kind == 29) {
			Get();
			expresion();
			Expect(30);
		} else if (StartOf(6)) {
			if (la.kind == 35 || la.kind == 36) {
				if (la.kind == 35) {
					Get();
				} else {
					Get();
				}
			}
			ctevar();
		} else SynErr(55);
	}

	void ctevar() {
		if (la.kind == 4 || la.kind == 5) {
			ctelet();
		} else if (la.kind == 23 || la.kind == 24) {
			ctebool();
		} else if (la.kind == 2 || la.kind == 3) {
			ctenum();
		} else if (la.kind == 1) {
			Get();
			if (la.kind == 29 || la.kind == 33) {
				if (la.kind == 29) {
					Get();
					if (StartOf(4)) {
						expresion();
						while (la.kind == 27) {
							Get();
							expresion();
						}
					}
					Expect(30);
				} else {
					cuantificador();
				}
			}
		} else SynErr(56);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		PLearning();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_x, _x},
		{_x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x}

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
			case 20: s = "PRINT expected"; break;
			case 21: s = "READLINE expected"; break;
			case 22: s = "FUNCTION expected"; break;
			case 23: s = "TRUE expected"; break;
			case 24: s = "FALSE expected"; break;
			case 25: s = "REF expected"; break;
			case 26: s = "PYC expected"; break;
			case 27: s = "COMA expected"; break;
			case 28: s = "IGUAL expected"; break;
			case 29: s = "PARAB expected"; break;
			case 30: s = "PARCI expected"; break;
			case 31: s = "LLAVEAB expected"; break;
			case 32: s = "LLAVECI expected"; break;
			case 33: s = "CORCHAB expected"; break;
			case 34: s = "CORCHCI expected"; break;
			case 35: s = "MAS expected"; break;
			case 36: s = "MENOS expected"; break;
			case 37: s = "MULT expected"; break;
			case 38: s = "DIV expected"; break;
			case 39: s = "MAQUE expected"; break;
			case 40: s = "MEQUE expected"; break;
			case 41: s = "DIFERENTE expected"; break;
			case 42: s = "COMPARACION expected"; break;
			case 43: s = "??? expected"; break;
			case 44: s = "invalid vars"; break;
			case 45: s = "invalid vars"; break;
			case 46: s = "invalid tipo"; break;
			case 47: s = "invalid ctelet"; break;
			case 48: s = "invalid ctenum"; break;
			case 49: s = "invalid ctebool"; break;
			case 50: s = "invalid regresa"; break;
			case 51: s = "invalid estatuto"; break;
			case 52: s = "invalid ciclos"; break;
			case 53: s = "invalid asignacionollamada"; break;
			case 54: s = "invalid asignacionollamada"; break;
			case 55: s = "invalid factor"; break;
			case 56: s = "invalid ctevar"; break;

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
