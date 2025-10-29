namespace AnalizadorLexicoProlog
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Button btnAnalizar;
        private System.Windows.Forms.DataGridView dgvTokens;
        private System.Windows.Forms.Label lblErrores;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtCodigo = new System.Windows.Forms.TextBox();
            this.btnAnalizar = new System.Windows.Forms.Button();
            this.dgvTokens = new System.Windows.Forms.DataGridView();
            this.lblErrores = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTokens)).BeginInit();
            this.SuspendLayout();
            // 
            // txtCodigo
            // 
            this.txtCodigo.Location = new System.Drawing.Point(12, 12);
            this.txtCodigo.Multiline = true;
            this.txtCodigo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCodigo.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(450, 400);
            // 
            // btnAnalizar
            // 
            this.btnAnalizar.Location = new System.Drawing.Point(480, 20);
            this.btnAnalizar.Name = "btnAnalizar";
            this.btnAnalizar.Size = new System.Drawing.Size(150, 40);
            this.btnAnalizar.Text = "Analizar Código";
            this.btnAnalizar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnAnalizar.Click += new System.EventHandler(this.btnAnalizar_Click);
            // 
            // dgvTokens
            // 
            this.dgvTokens.Location = new System.Drawing.Point(480, 80);
            this.dgvTokens.Name = "dgvTokens";
            this.dgvTokens.Size = new System.Drawing.Size(500, 300);
            this.dgvTokens.ColumnCount = 3;
            this.dgvTokens.Columns[0].Name = "Lexema";
            this.dgvTokens.Columns[1].Name = "Token";
            this.dgvTokens.Columns[2].Name = "Posición";
            // 
            // lblErrores
            // 
            this.lblErrores.Location = new System.Drawing.Point(12, 420);
            this.lblErrores.Name = "lblErrores";
            this.lblErrores.Size = new System.Drawing.Size(960, 40);
            this.lblErrores.ForeColor = System.Drawing.Color.Red;
            this.lblErrores.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Italic);
            this.lblErrores.Text = "Errores léxicos: ninguno.";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 480);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Controls.Add(this.txtCodigo);
            this.Controls.Add(this.btnAnalizar);
            this.Controls.Add(this.dgvTokens);
            this.Controls.Add(this.lblErrores);
            this.Name = "Form1";
            this.Text = "Analizador Léxico - SWI Prolog";
            ((System.ComponentModel.ISupportInitialize)(this.dgvTokens)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
