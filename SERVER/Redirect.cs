
namespace ServerPing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.IO;

    namespace Server_Frm.CSS
    {
        class TextBoxWriter : TextWriter
        {
            TextBox textBox;
            delegate void WriteFunc(string value);
            WriteFunc write;
            WriteFunc writeLine;

            public TextBoxWriter(TextBox textBox)
            {
                this.textBox = textBox;
                write = Write;
                writeLine = WriteLine;
            }

            /// <summary>
            /// Encoding conversion-UTF8
            /// </summary>
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
                //get { return Encoding.Unicode; }
            }

            /// <summary>
            /// At least the methods that need to be rewritten
            /// </summary>
            public override void Write(string value)
            {
                if (textBox.InvokeRequired)
                {
                    textBox.BeginInvoke(write, value);
                }
                else
                {
                    textBox.AppendText(value);
                }
            }

            /// <summary>
            /// Directly process one line of output to improve efficiency
            /// </summary>
            public override void WriteLine(string value)
            {
                if (textBox.InvokeRequired)
                {
                    textBox.BeginInvoke(writeLine, value);
                }
                else
                {
                    textBox.AppendText(value);
                    textBox.AppendText(this.NewLine);
                }

            }
        }
    }
}
