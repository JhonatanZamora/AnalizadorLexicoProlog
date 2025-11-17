using System;
using System.Collections.Generic;
using System.Text;

namespace AnalizadorLexicoProlog
{
    public class Lexer
    {
        public List<Token> Tokens { get; private set; } = new();
        public List<string> Errores { get; private set; } = new();

        private readonly HashSet<string> palabrasReservadas = new(StringComparer.Ordinal)
        {
            "consult", "listing", "fail", "true", "false", "not", "is", "repeat", "assert", "retract"
        };

        public void Analizar(string codigo)
        {
            Tokens.Clear();
            Errores.Clear();

            if (codigo == null)
            {
                return;
            }

            int linea = 1;
            int columna = 1;
            int i = 0;

            while (i < codigo.Length)
            {
                char c = codigo[i];

                if (char.IsWhiteSpace(c))
                {
                    Avanzar(codigo, ref i, ref linea, ref columna);
                    continue;
                }

                if (c == '%')
                {
                   Avanzar(codigo, ref i, ref linea, ref columna);
                    while (i < codigo.Length && codigo[i] != '\n')
                    {
                        Avanzar(codigo, ref i, ref linea, ref columna);
                    }
                    continue;
                }

                if (c == '/' && i + 1 < codigo.Length && codigo[i + 1] == '*')
                {
                    int inicioLinea = linea;
                    int inicioColumna = columna;
                    Avanzar(codigo, ref i, ref linea, ref columna);
                    Avanzar(codigo, ref i, ref linea, ref columna);
                    bool cerrado = false;

                    while (i < codigo.Length)
                    {
                        if (codigo[i] == '*' && i + 1 < codigo.Length && codigo[i + 1] == '/')
                        {
                            Avanzar(codigo, ref i, ref linea, ref columna);
                            Avanzar(codigo, ref i, ref linea, ref columna);
                            cerrado = true;
                            break;
                        }

                        Avanzar(codigo, ref i, ref linea, ref columna);
                    }

                    if (!cerrado)
                    {
                        Errores.Add($"Linea {inicioLinea}, columna {inicioColumna}: comentario de bloque sin cerrar.");
                    }

                    continue;
                }

                if (c == '"')
                {
                    int inicioLinea = linea;
                    int inicioColumna = columna;
                    var sb = new StringBuilder();
                    Avanzar(codigo, ref i, ref linea, ref columna);
                    bool cerrada = false;

                    while (i < codigo.Length)
                    {
                        char actual = codigo[i];

                        if (actual == '\\')
                        {
                            if (i + 1 >= codigo.Length)
                            {
                                Errores.Add($"Linea {inicioLinea}, columna {inicioColumna}: secuencia de escape incompleta en cadena.");
                                Avanzar(codigo, ref i, ref linea, ref columna);
                                break;
                            }

                            sb.Append(actual);
                            Avanzar(codigo, ref i, ref linea, ref columna);
                            char escapeChar = codigo[i];
                            sb.Append(escapeChar);
                            Avanzar(codigo, ref i, ref linea, ref columna);
                            continue;
                        }

                        if (actual == '"')
                        {
                            Avanzar(codigo, ref i, ref linea, ref columna);
                            cerrada = true;
                            break;
                        }

                        sb.Append(actual);
                        Avanzar(codigo, ref i, ref linea, ref columna);
                    }

                    if (!cerrada)
                    {
                        Errores.Add($"Linea {inicioLinea}, columna {inicioColumna}: cadena sin cerrar.");
                    }
                    else
                    {
                        Tokens.Add(new Token
                        {
                            Lexema = "\"" + sb.ToString() + "\"",
                            Tipo = "CADENA",
                            Linea = inicioLinea,
                            Columna = inicioColumna
                        });
                    }

                    continue;
                }

                if (char.IsDigit(c))
                {
                    int inicioLinea = linea;
                    int inicioColumna = columna;
                    var sb = new StringBuilder();
                    bool esDecimal = false;

                    while (i < codigo.Length)
                    {
                        char actual = codigo[i];

                        if (actual == '.')
                        {
                            if (esDecimal || i + 1 >= codigo.Length || !char.IsDigit(codigo[i + 1]))
                            {
                                break;
                            }

                            esDecimal = true;
                            sb.Append(actual);
                            Avanzar(codigo, ref i, ref linea, ref columna);
                            continue;
                        }

                        if (!char.IsDigit(actual))
                        {
                            break;
                        }

                        sb.Append(actual);
                        Avanzar(codigo, ref i, ref linea, ref columna);
                    }

                    string numero = sb.ToString();
                    string tipoNumero = esDecimal ? "NUM_DECIMAL" : "NUM_ENTERO";

                    Tokens.Add(new Token
                    {
                        Lexema = numero,
                        Tipo = tipoNumero,
                        Linea = inicioLinea,
                        Columna = inicioColumna
                    });

                    continue;
                }

                if (char.IsLetter(c) || c == '_')
                {
                    int inicioLinea = linea;
                    int inicioColumna = columna;
                    var sb = new StringBuilder();

                    while (i < codigo.Length)
                    {
                        char actual = codigo[i];
                        if (!char.IsLetterOrDigit(actual) && actual != '_')
                        {
                            break;
                        }

                        sb.Append(actual);
                        Avanzar(codigo, ref i, ref linea, ref columna);
                    }

                    string identificador = sb.ToString();
                    if (identificador.Length > 15)
                    {
                        Errores.Add($"Linea {inicioLinea}, columna {inicioColumna}: identificador '{identificador}' excede los 15 caracteres.");
                    }

                    string tipoIdentificador = palabrasReservadas.Contains(identificador)
                        ? "PALABRA_RESERVADA"
                        : "IDENTIFICADOR";

                    Tokens.Add(new Token
                    {
                        Lexema = identificador,
                        Tipo = tipoIdentificador,
                        Linea = inicioLinea,
                        Columna = inicioColumna
                    });

                    continue;
                }

                if (i + 1 < codigo.Length)
                {
                    string par = string.Concat(c, codigo[i + 1]);
                    int inicioLinea = linea;
                    int inicioColumna = columna;

                    if (par is "==" or "!=" or ">=" or "<=")
                    {
                        Tokens.Add(new Token
                        {
                            Lexema = par,
                            Tipo = "OP_COMPARACION",
                            Linea = inicioLinea,
                            Columna = inicioColumna
                        });
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        continue;
                    }

                    if (par is "&&" or "||")
                    {
                        Tokens.Add(new Token
                        {
                            Lexema = par,
                            Tipo = "OP_LOGICO",
                            Linea = inicioLinea,
                            Columna = inicioColumna
                        });
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        continue;
                    }

                    if (par is "++" or "--")
                    {
                        Tokens.Add(new Token
                        {
                            Lexema = par,
                            Tipo = "OP_INCREMENTO",
                            Linea = inicioLinea,
                            Columna = inicioColumna
                        });
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        continue;
                    }

                    if (par == ":-")
                    {
                        Tokens.Add(new Token
                        {
                            Lexema = par,
                            Tipo = "OP_ASIGNACION",
                            Linea = inicioLinea,
                            Columna = inicioColumna
                        });
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        continue;
                    }

                    if (par == "?-")
                    {
                        Tokens.Add(new Token
                        {
                            Lexema = par,
                            Tipo = "OP_CONSULTA",
                            Linea = inicioLinea,
                            Columna = inicioColumna
                        });
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        continue;
                    }
                }

                switch (c)
                {
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '%':
                        Tokens.Add(new Token { Lexema = c.ToString(), Tipo = "OP_ARITMETICO", Linea = linea, Columna = columna });
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        continue;
                    case '=':
                        Tokens.Add(new Token { Lexema = "=", Tipo = "OP_ASIGNACION", Linea = linea, Columna = columna });
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        continue;
                    case '>':
                    case '<':
                        Tokens.Add(new Token { Lexema = c.ToString(), Tipo = "OP_COMPARACION", Linea = linea, Columna = columna });
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        continue;
                    case '!':
                        Tokens.Add(new Token { Lexema = "!", Tipo = "OP_LOGICO", Linea = linea, Columna = columna });
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        continue;
                    case '(':
                    case ')':
                        Tokens.Add(new Token { Lexema = c.ToString(), Tipo = "PARENTESIS", Linea = linea, Columna = columna });
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        continue;
                    case '{':
                    case '}':
                        Tokens.Add(new Token { Lexema = c.ToString(), Tipo = "LLAVE", Linea = linea, Columna = columna });
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        continue;
                    case '.':
                        Tokens.Add(new Token { Lexema = ".", Tipo = "TERMINAL", Linea = linea, Columna = columna });
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        continue;
                    case ',':
                        Tokens.Add(new Token { Lexema = ",", Tipo = "SEPARADOR", Linea = linea, Columna = columna });
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        continue;
                    case '?':
                        Tokens.Add(new Token { Lexema = "?", Tipo = "OP_CONSULTA", Linea = linea, Columna = columna });
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        continue;
                    default:
                        Errores.Add($"Linea {linea}, columna {columna}: simbolo no reconocido '{c}'.");
                        Avanzar(codigo, ref i, ref linea, ref columna);
                        continue;
                }
            }
        }

        private static void Avanzar(string codigo, ref int indice, ref int linea, ref int columna)
        {
            if (indice >= codigo.Length)
            {
                return;
            }

            char actual = codigo[indice];
            indice++;

            if (actual == '\n')
            {
                linea++;
                columna = 1;
            }
            else
            {
                columna++;
            }
        }
    }
}
