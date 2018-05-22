using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ratchet.Drawing.OpenGL
{
    public unsafe static partial class WGL
    {
        unsafe public class Context
        {
            IntPtr _HWGLC;
            IntPtr _HDC;
            internal Context(IntPtr HDC, IntPtr HWGLC)
            {
                _HWGLC = HWGLC;
                _HDC = HDC;
            }

            [System.Runtime.InteropServices.DllImport("opengl32.dll")]
            public static extern IntPtr wglGetProcAddress(string Name);

            [System.Runtime.InteropServices.DllImport("opengl32.dll")]
            public static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);
            public void MakeCurrent()
            {
                if (!wglMakeCurrent(_HDC, _HWGLC))
                {
                    throw new Exception("Impossible to set the current context");
                }
            }

            [System.Runtime.InteropServices.DllImport("opengl32.dll")]
            static extern bool wglSwapBuffers(IntPtr hdc);
            public void SwapBuffers()
            {
                if (!wglSwapBuffers(_HDC))
                {
                    throw new Exception("Impossible to perform the wap buffer operations");
                }
            }
        }
    }
}
