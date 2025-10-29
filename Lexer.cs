using System;
using System.Collections.Generic;

namespace AnalizadorLexicoProlog
{
    public class Lexer
    {
        public List<Token> Tokens { get; private set; } = new();
        public List<string> Errores { get; private set; } = new();

        private readonly HashSet<string> palabrasReservadas = new()
        {
            "if", "else", "while", "for", "true", "false", "write", "read", "is", "not"
        };

        public void Analizar(string codigo)
        {
            Tokens.Clear();
            Errores.Clear();

            int linea = 1;
            int columna = 1;
            int i = 0;

            while (i < codigo.Length)
            {
                char c = codigo[i];

                // Ignorar espacios y saltos de línea
                if (char.IsWhiteSpace(c))
                {
                    if (c == '\n') { linea++; columna = 1; }
                    else columna++;
                    i++;
                    continue;
                }

                // 🔹 Comentario de línea //
                if (c == '/' && i + 1 < codigo.Length && codigo[i + 1] == '/')
                {
                    while (i < codigo.Length && codigo[i] != '\n') i++;
                    continue;
                }

                // 🔹 Comentario de bloque /* ... */
                if (c == '/' && i + 1 < codigo.Length && codigo[i + 1] == '*')
                {
                    int inicioLinea = linea;
                    i += 2;
                    bool cerrado = false;
                    while (i < codigo.Length)
                    {
                        if (codigo[i] == '\n') { linea++; columna = 1; }
                        if (codigo[i] == '*' && i + 1 < codigo.Length && codigo[i + 1] == '/')
                        {
                            i += 2;
                            cerrado = true;
                            break;
                        }
                        i++;
                    }
                    if (!cerrado)
                        Errores.Add($"Línea {inicioLinea}: comentario de bloque sin cerrar.");
                    continue;
                }

                // 🔹 Cadenas de caracteres
                if (c == '"')
                {
                    int inicioColumna = columna;
                    string cadena = "";
                    i++; columna++;
                    bool cerrada = false;

                    while (i < codigo.Length)
                    {
                        char actual = codigo[i];
                        if (actual == '\\' && i + 1 < codigo.Length)
                        {
                            cadena += actual;
                            cadena += codigo[i + 1];
                            i += 2; columna += 2;
                            continue;
                        }
                        if (actual == '"')
                        {
                            cerrada = true;
                            i++; columna++;
                            break;
                        }
                        if (actual == '\n')
                        {
                            linea++;
                            columna = 1;
                        }
                        cadena += actual;
                        i++; columna++;
                    }

                    if (!cerrada)
                        Errores.Add($"Línea {linea}: cadena sin cerrar.");
                    else
                        Tokens.Add(new Token { Lexema = $"\"{cadena}\"", Tipo = "CADENA", Linea = linea, Columna = inicioColumna });
                    continue;
                }

                // 🔹 Números (enteros o decimales)
                if (char.IsDigit(c))
                {
                    string numero = "";
                    int inicioColumna = columna;
                    bool esDecimal = false;

                    while (i < codigo.Length && (char.IsDigit(codigo[i]) || codigo[i] == '.'))
                    {
                        if (codigo[i] == '.')
                        {
                            if (esDecimal)
                                break; // dos puntos → termina
                            esDecimal = true;
                        }
                        numero += codigo[i];
                        i++; columna++;
                    }

                    string tipo = esDecimal ? "NUM_DECIMAL" : "NUM_ENTERO";
                    Tokens.Add(new Token { Lexema = numero, Tipo = tipo, Linea = linea, Columna = inicioColumna });
                    continue;
                }

                // 🔹 Identificadores y palabras reservadas
                if (char.IsLetter(c) || c == '_')
                {
                    string id = "";
                    int inicioColumna = columna;
                    while (i < codigo.Length && (char.IsLetterOrDigit(codigo[i]) || codigo[i] == '_'))
                    {
                        id += codigo[i];
                        i++; columna++;
                    }

                    if (id.Length > 15)
                        Errores.Add($"Línea {linea}: identificador '{id}' excede los 15 caracteres.");

                    string tipo = palabrasReservadas.Contains(id) ? "PALABRA_RESERVADA" : "IDENTIFICADOR";
                    Tokens.Add(new Token { Lexema = id, Tipo = tipo, Linea = linea, Columna = inicioColumna });
                    continue;
                }

                // 🔹 Operadores compuestos
                if (i + 1 < codigo.Length)
                {
                    string dos = $"{codigo[i]}{codigo[i + 1]}";

                    if (dos is "==" or "!=" or ">=" or "<=")
                    {
                        Tokens.Add(new Token { Lexema = dos, Tipo = "OP_COMPARACION", Linea = linea, Columna = columna });
                        i += 2; columna += 2;
                        continue;
                    }

                    if (dos is "&&" or "||")
                    {
                        Tokens.Add(new Token { Lexema = dos, Tipo = "OP_LOGICO", Linea = linea, Columna = columna });
                        i += 2; columna += 2;
                        continue;
                    }

                    if (dos is "++" or "--")
                    {
                        Tokens.Add(new Token { Lexema = dos, Tipo = "OP_INCREMENTO", Linea = linea, Columna = columna });
                        i += 2; columna += 2;
                        continue;
                    }
                }

                // 🔹 Operadores simples
                switch (c)
                {
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '%':
                        Tokens.Add(new Token { Lexema = c.ToString(), Tipo = "OP_ARITMETICO", Linea = linea, Columna = columna });
                        break;
                    case '=':
                        Tokens.Add(new Token { Lexema = "=", Tipo = "OP_ASIGNACION", Linea = linea, Columna = columna });
                        break;
                    case '>':
                    case '<':
                        Tokens.Add(new Token { Lexema = c.ToString(), Tipo = "OP_COMPARACION", Linea = linea, Columna = columna });
                        break;
                    case '!':
                        Tokens.Add(new Token { Lexema = "!", Tipo = "OP_LOGICO", Linea = linea, Columna = columna });
                        break;
                    case '(':
                    case ')':
                        Tokens.Add(new Token { Lexema = c.ToString(), Tipo = "PARENTESIS", Linea = linea, Columna = columna });
                        break;
                    case '{':
                    case '}':
                        Tokens.Add(new Token { Lexema = c.ToString(), Tipo = "LLAVE", Linea = linea, Columna = columna });
                        break;
                    case '.':
                        Tokens.Add(new Token { Lexema = ".", Tipo = "TERMINAL", Linea = linea, Columna = columna });
                        break;
                    case ',':
                        Tokens.Add(new Token { Lexema = ",", Tipo = "SEPARADOR", Linea = linea, Columna = columna });
                        break;
                    default:
                        Errores.Add($"Línea {linea}, Columna {columna}: símbolo no reconocido '{c}'");
                        break;
                }

                i++; columna++;
            }
        }
    }
}
