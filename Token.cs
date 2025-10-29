namespace AnalizadorLexicoProlog
{
    public class Token
    {
        public string Lexema { get; set; }
        public string Tipo { get; set; }
        public int Linea { get; set; }
        public int Columna { get; set; }

        public Token() { }  // constructor vacío por compatibilidad

        public Token(string lexema, string tipo, int linea, int columna)
        {
            Lexema = lexema;
            Tipo = tipo;
            Linea = linea;
            Columna = columna;
        }
    }
}
