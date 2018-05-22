using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmptyWindow
{
    public partial class EmptyWindow : Form
    {
        delegate void glClearType(uint Target);
        glClearType glClear;

        delegate void glClearColorType(float R, float G, float B, float A);
        glClearColorType glClearColor;

        public void Clear(float R, float G, float B, float A)
        {
            glClearColor(R, G, B, A);
            glClear(0x4000 /* GL_COLOR_BUFFER_BIT */);
        }

        Ratchet.Drawing.OpenGL.WGL.Context _Context;

        public EmptyWindow()
        {
            InitializeComponent();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                parameters.ClassStyle |= (int)(Ratchet.Drawing.OpenGL.WGL.WindowClassStyle.OWNDC | Ratchet.Drawing.OpenGL.WGL.WindowClassStyle.VREDRAW | Ratchet.Drawing.OpenGL.WGL.WindowClassStyle.HREDRAW);
                return parameters;
            }
        }

        private void EmptyWindow_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
            _Context = Ratchet.Drawing.OpenGL.WGL.CreateContext(this);
            _Context.MakeCurrent();
            glClear = Ratchet.Drawing.OpenGL.WGL.GetProcAddress<glClearType>("glClear");
            glClearColor = Ratchet.Drawing.OpenGL.WGL.GetProcAddress<glClearColorType>("glClearColor");
        }

        private void EmptyWindow_Paint(object sender, PaintEventArgs e)
        {
            _Context.MakeCurrent();
            Clear(0.1f, 0.3f, 1.0f, 1.0f);
            _Context.SwapBuffers();
        }
    }
}
