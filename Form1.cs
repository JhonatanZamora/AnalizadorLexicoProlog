using System;
using System.Windows.Forms;

namespace AnalizadorLexicoProlog
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnAnalizar_Click(object sender, EventArgs e)
        {
            // Limpiar la tabla y el área de errores
            dgvTokens.Rows.Clear();
            lblErrores.Text = "";

            string codigo = txtCodigo.Text;

            // Crear el analizador
            Lexer lexer = new Lexer();

            // Analizar el texto (el método NO devuelve nada)
            lexer.Analizar(codigo);

            // Mostrar tokens encontrados
            foreach (var token in lexer.Tokens)
            {
                dgvTokens.Rows.Add(token.Lexema, token.Tipo, $"{token.Linea}:{token.Columna}");
            }

            // Mostrar errores, si existen
            if (lexer.Errores.Count > 0)
            {
                lblErrores.Text = " Errores léxicos detectados:\n" + string.Join("\n", lexer.Errores);
            }
            else
            {
                lblErrores.Text = $" Análisis completado correctamente. Tokens encontrados: {lexer.Tokens.Count}";
            }
        }
    }
}
