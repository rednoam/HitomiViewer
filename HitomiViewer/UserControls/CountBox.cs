using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HitomiViewer.Scripts
{
    class CountBox
    {
        public string text = "";
        public string caption = "";
        public decimal defval = 0;
        public CountBox(string text, string caption, decimal defval = 0)
        {
            this.text = text;
            this.caption = caption;
            this.defval = defval;
        }

        public decimal ShowDialog()
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            NumericUpDown numericBox = new NumericUpDown() { Left = 50, Top = 50, Width = 400, Value = defval };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(numericBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? numericBox.Value : defval;
        }
    }
}
