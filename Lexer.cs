using System;
using System.Collections.Generic;

namespace AnalizadorLexicoProlog
{
    /// <summary>
    /// Clase principal encargada de realizar el análisis léxico del lenguaje Prolog.
    /// Recorre el código fuente carácter por carácter, reconociendo los tokens definidos en los requisitos del proyecto.
    /// </summary>
    public class Lexer
    {
        // Lista donde se almacenan los tokens reconocidos
        public List<Token> Tokens { get; private set; } = new List<Token>();

        // Lista de errores encontrados durante el análisis
        public List<string> Errores { get; private set; } = new List<string>();

        /// <summary>
        /// Método principal del analizador léxico.
        /// Recibe el código fuente en formato string, lo analiza y genera las listas de Tokens y Errores.
        /// </summary>
        /// <param name="codigo">Texto fuente a analizar (contenido del archivo .pl)</param>
        public void Analizar(string codigo)
        {
            Tokens.Clear();
            Errores.Clear();

            //  Palabras reservadas básicas de Prolog
            var palabrasReservadas = new HashSet<string>
            {
                "consult", "listing", "fail", "true", "false", "not", "is", "repeat", "assert", "retract"
            };

            //  Operadores aritméticos admitidos (incluyendo mod y div)
            var operadoresAritmeticos = new HashSet<string>
            {
                "+", "-", "*", "/", "//", "mod", "div", "^", "**"
            };

            //  Operadores relacionales con y sin evaluación
            var operadoresRelacionales = new HashSet<string>
            {
                "is", "=:=", "=\\=", ">", "<", ">=", "=<", "==", "\\==", "@>", "@<", "@>=", "@=<"
            };

            //  Variables de control para el recorrido del código
            int linea = 1, columna = 1;
            int i = 0;
            bool dentroDeRegla = false; // Indica si el analizador está dentro del cuerpo de una regla (para distinguir las comas)

            //  Recorre todo el texto carácter a carácter
            while (i < codigo.Length)
            {
                char c = codigo[i];

                //  Ignorar espacios y saltos de línea
                if (char.IsWhiteSpace(c))
                {
                    if (c == '\n') { linea++; columna = 1; }
                    else columna++;
                    i++;
                    continue;
                }

                //  Comentario de línea (%)
                if (c == '%')
                {
                    while (i < codigo.Length && codigo[i] != '\n') i++;
                    continue;
                }

                //  Comentario de bloque /* ... */
                if (c == '/' && i + 1 < codigo.Length && codigo[i + 1] == '*')
                {
                    i += 2;
                    while (i + 1 < codigo.Length && !(codigo[i] == '*' && codigo[i + 1] == '/'))
                    {
                        if (codigo[i] == '\n') { linea++; columna = 1; }
                        i++;
                    }

                    // Si no se cerró el comentario de bloque correctamente
                    if (i + 1 >= codigo.Length)
                    {
                        Errores.Add($"Error: comentario de bloque sin cerrar en línea {linea}");
                        break;
                    }

                    i += 2;
                    continue;
                }

                //  Cadenas de texto ("texto")
                if (c == '"')
                {
                    int start = i;
                    i++;

                    // Se recorren los caracteres internos hasta cerrar la comilla
                    while (i < codigo.Length && codigo[i] != '"')
                    {
                        if (codigo[i] == '\\' && i + 1 < codigo.Length) i++; // Maneja escapes como \" o \n
                        i++;
                    }

                    // Si se encuentra la comilla de cierre
                    if (i < codigo.Length && codigo[i] == '"')
                    {
                        i++;
                        Tokens.Add(new Token
                        {
                            Lexema = codigo.Substring(start, i - start),
                            Tipo = "CADENA",
                            Linea = linea,
                            Columna = columna
                        });
                    }
                    else
                    {
                        Errores.Add($"Línea {linea}: cadena sin cerrar.");
                    }

                    columna = i + 1;
                    continue;
                }

                //  Identificadores, variables, operadores con nombre o palabras reservadas
                if (char.IsLetter(c) || c == '_')
                {
                    int start = i;
                    while (i < codigo.Length && (char.IsLetterOrDigit(codigo[i]) || codigo[i] == '_')) i++;

                    string lexema = codigo.Substring(start, i - start);
                    string tipo;

                    // Clasificación del token según su valor
                    if (operadoresAritmeticos.Contains(lexema))
                        tipo = "OPERADOR_ARITMETICO";
                    else if (operadoresRelacionales.Contains(lexema))
                        tipo = "OPERADOR_COMPARACION";
                    else if (palabrasReservadas.Contains(lexema))
                        tipo = "PALABRA_RESERVADA";
                    else if (char.IsUpper(lexema[0]) || lexema[0] == '_')
                        tipo = "VARIABLE";
                    else
                        tipo = "ATOMO"; // Por defecto, si empieza en minúscula es un átomo

                    Tokens.Add(new Token
                    {
                        Lexema = lexema,
                        Tipo = tipo,
                        Linea = linea,
                        Columna = columna
                    });

                    columna += lexema.Length;
                    continue;
                }

                //  Números enteros o decimales
                if (char.IsDigit(c))
                {
                    int start = i;
                    bool esDecimal = false;

                    // Parte entera
                    while (i < codigo.Length && char.IsDigit(codigo[i])) i++;

                    // Parte decimal (si existe)
                    if (i < codigo.Length - 1 && codigo[i] == '.' && char.IsDigit(codigo[i + 1]))
                    {
                        esDecimal = true;
                        i++;
                        while (i < codigo.Length && char.IsDigit(codigo[i])) i++;
                    }

                    string numero = codigo.Substring(start, i - start);
                    Tokens.Add(new Token
                    {
                        Lexema = numero,
                        Tipo = esDecimal ? "NUM_DECIMAL" : "NUM_ENTERO",
                        Linea = linea,
                        Columna = columna
                    });

                    columna += numero.Length;
                    continue;
                }

                //  Operadores compuestos de más de un carácter (=:==, >=, =<, @=<, etc.)
                string[] operadoresCompuestos = { "=:=", "=\\=", ">=", "=<", "==", "\\==", "@>", "@<", "@>=", "@=<" };
                bool encontrado = false;

                foreach (var op in operadoresCompuestos)
                {
                    if (codigo.Substring(i).StartsWith(op))
                    {
                        Tokens.Add(new Token
                        {
                            Lexema = op,
                            Tipo = "OPERADOR_COMPARACION",
                            Linea = linea,
                            Columna = columna
                        });
                        i += op.Length;
                        columna += op.Length;
                        encontrado = true;
                        break;
                    }
                }
                if (encontrado) continue;

                //  Operador de regla (:-)
                if (c == ':' && i + 1 < codigo.Length && codigo[i + 1] == '-')
                {
                    dentroDeRegla = true; // A partir de aquí, las comas son conjunciones
                    Tokens.Add(new Token
                    {
                        Lexema = ":-",
                        Tipo = "OPERADOR_REGLA",
                        Linea = linea,
                        Columna = columna
                    });
                    i += 2;
                    columna += 2;
                    continue;
                }

                //  Consulta (?-)
                if (c == '?' && i + 1 < codigo.Length && codigo[i + 1] == '-')
                {
                    Tokens.Add(new Token
                    {
                        Lexema = "?-",
                        Tipo = "CONSULTA",
                        Linea = linea,
                        Columna = columna
                    });
                    i += 2;
                    columna += 2;
                    continue;
                }

                //  Símbolos y operadores unitarios
                switch (c)
                {
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
                        dentroDeRegla = false; // Fin de regla
                        break;

                    // Detección contextual de comas (como separador o conjunción lógica)
                    case ',':
                        Tokens.Add(new Token
                        {
                            Lexema = ",",
                            Tipo = dentroDeRegla ? "OPERADOR_LOGICO_CONJUNCION" : "SEPARADOR",
                            Linea = linea,
                            Columna = columna
                        });
                        break;

                    // Disyunción lógica (;)
                    case ';':
                        Tokens.Add(new Token
                        {
                            Lexema = ";",
                            Tipo = "OPERADOR_LOGICO_DISYUNCION",
                            Linea = linea,
                            Columna = columna
                        });
                        break;

                    // Operadores aritméticos simples
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                        Tokens.Add(new Token { Lexema = c.ToString(), Tipo = "OPERADOR_ARITMETICO", Linea = linea, Columna = columna });
                        break;

                    // Operadores relacionales simples (>, <, =)
                    case '>':
                    case '<':
                    case '=':
                        Tokens.Add(new Token { Lexema = c.ToString(), Tipo = "OPERADOR_COMPARACION", Linea = linea, Columna = columna });
                        break;

                    // Símbolos no reconocidos
                    default:
                        Errores.Add($"Línea {linea}, Columna {columna}: símbolo no reconocido '{c}'");
                        break;
                }

                i++;
                columna++;
            }
        }
    }
}