using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ratchet.Drawing.OpenGL
{
    /// <summary>
    /// This class contains functions to interact with the Microsoft Windows OpenGL
    /// functions.
    /// </summary>
    public unsafe static partial class WGL
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LoadLibrary(string name);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr module, string name);

        [System.Runtime.InteropServices.DllImport("opengl32.dll", SetLastError = true)]
        static extern IntPtr wglCreateContext(IntPtr Hdc);

        [System.Runtime.InteropServices.DllImport("gdi32.dll", SetLastError=true)]
        static extern int GetPixelFormat(IntPtr Hdc);

        [System.Runtime.InteropServices.DllImport("gdi32.dll", SetLastError = true)]
        static extern int ChoosePixelFormat(IntPtr Hdc, PIXELFORMATDESCRIPTOR* Descriptor);

        [System.Runtime.InteropServices.DllImport("gdi32.dll", SetLastError = true)]
        static extern int SetPixelFormat(IntPtr Hdc, int Format, PIXELFORMATDESCRIPTOR* Descriptor);

        [System.Runtime.InteropServices.DllImport("gdi32.dll", SetLastError = true)]
        static extern int DescribePixelFormat(IntPtr Hdc, int Format, uint Size, PIXELFORMATDESCRIPTOR* Descriptor);

        [System.Runtime.InteropServices.DllImport("opengl32.dll", SetLastError = true)]
        public static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);

        [System.Runtime.InteropServices.DllImport("opengl32.dll")]
        public static extern IntPtr wglGetProcAddress(string Name);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        static extern IntPtr GetDC(IntPtr Hwnd);

        static IntPtr _OGLHandle;

        static WGL()
        {
            // Force a module load at init so we hold a reference on it.
            _OGLHandle = LoadLibrary("opengl32.dll");
        }


        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct PIXELFORMATDESCRIPTOR
        {
            ushort nSize;
            ushort nVersion;
            public uint dwFlags;
            public byte iPixelType;
            public byte cColorBits;
            byte cRedBits;
            byte cRedShift;
            byte cGreenBits;
            byte cGreenShift;
            byte cBlueBits;
            byte cBlueShift;
            public byte cAlphaBits;
            byte cAlphaShift;
            public byte cAccumBits;
            byte cAccumRedBits;
            byte cAccumGreenBits;
            byte cAccumBlueBits;
            byte cAccumAlphaBits;
            public byte cDepthBits;
            public byte cStencilBits;
            public byte cAuxBuffers;
            public byte iLayerType;
            byte bReserved;
            uint dwLayerMask;
            uint dwVisibleMask;
            uint dwDamageMask;

            public const byte PFD_MAIN_PLANE = 0;
            public const byte PFD_TYPE_RGBA = 0;
            public const byte PFD_TYPE_COLORINDEX = 1;
            public const uint PFD_DOUBLEBUFFER = 1;
            public const uint PFD_STEREO = 2;
            public const uint PFD_DRAW_TO_WINDOW = 4;
            public const uint PFD_DRAW_TO_BITMAP = 8;
            internal const uint PFD_SUPPORT_OPENGL = 32;
            internal const uint PFD_GENERIC_ACCELERATED = 4096;

            public PIXELFORMATDESCRIPTOR(uint dwFlags, byte iPixelType, byte cColorBits, byte cDepthBits)
            {
                nSize = (ushort)sizeof(PIXELFORMATDESCRIPTOR);
                nVersion = 1;
                this.dwFlags = dwFlags | PFD_SUPPORT_OPENGL;
                this.iPixelType = iPixelType;
                this.cColorBits = cColorBits;
                this.cDepthBits = cDepthBits;

                iLayerType = PFD_MAIN_PLANE;

                cRedBits = 0;
                cRedShift = 0;
                cGreenBits = 0;
                cGreenShift = 0;
                cBlueBits = 0;
                cBlueShift = 0;
                cAuxBuffers = 0;
                cAlphaBits = 0;
                cAlphaShift = 0;
                cAccumBits = 0;
                cAccumAlphaBits = 0;
                cAccumRedBits = 0;
                cAccumGreenBits = 0;
                cAccumBlueBits = 0;
                cStencilBits = 0;
                dwLayerMask = 0;
                dwVisibleMask = 0;
                dwDamageMask = 0;
                bReserved = 0;

            }
        }

        public enum WindowClassStyle : int
        {
            VREDRAW = 0x01,
            HREDRAW = 0x02,
            OWNDC = 0x20,
            CLASSDC = 0x40,
            DBLCLKS = 0x08,
            PARENTDC = 0x80,
            SAVEBITS = 0x800,
            DROPSHADOW = 0x20000,
        }

        internal static void SetPixelFormat(IntPtr HDC, PIXELFORMATDESCRIPTOR PixelFormatDescriptor)
        {
            PixelFormatDescriptor.dwFlags |= PIXELFORMATDESCRIPTOR.PFD_SUPPORT_OPENGL;
            int format = ChoosePixelFormat(HDC, &PixelFormatDescriptor);
            int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
            if (error != 0) { throw new Exception("Invalid pixel format"); }
            SetPixelFormat(HDC, format, &PixelFormatDescriptor);
            error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
            if (error != 0) { throw new Exception("Invalid pixel format"); }
        }

        /// <summary>
        /// Create an OpenGL context from the procided Windows Drawing Context
        /// </summary>
        /// <param name="HDC">Handle to the windows Drawing Context</param>
        static public Context CreateContext(IntPtr HDC, PIXELFORMATDESCRIPTOR PixelFormatDescriptor)
        {
            PIXELFORMATDESCRIPTOR selectedPixelFormatDescriptor;
            try
            {
                // Always pick the format with HW acceleration first
                PixelFormatDescriptor.dwFlags |= PIXELFORMATDESCRIPTOR.PFD_GENERIC_ACCELERATED;
                SetPixelFormat(HDC, PixelFormatDescriptor);
                int format = GetPixelFormat(HDC);
                if (format == 0) { throw new Exception("Invalid pixel format"); }
                if (0 == DescribePixelFormat(HDC, format, (uint)sizeof(PIXELFORMATDESCRIPTOR), &selectedPixelFormatDescriptor)) { throw new Exception("Invalid pixel format"); }
                if ((selectedPixelFormatDescriptor.dwFlags & PIXELFORMATDESCRIPTOR.PFD_SUPPORT_OPENGL) == 0) { throw new Exception("The pixel format specified has no OpenGL support"); }
            }
            catch
            {
                PixelFormatDescriptor.dwFlags &= (~PIXELFORMATDESCRIPTOR.PFD_GENERIC_ACCELERATED);
                SetPixelFormat(HDC, PixelFormatDescriptor);
                int format = GetPixelFormat(HDC);
                if (format == 0) { throw new Exception("Invalid pixel format"); }
                if (0 == DescribePixelFormat(HDC, format, (uint)sizeof(PIXELFORMATDESCRIPTOR), &selectedPixelFormatDescriptor)) { throw new Exception("Invalid pixel format"); }
                if ((selectedPixelFormatDescriptor.dwFlags & PIXELFORMATDESCRIPTOR.PFD_SUPPORT_OPENGL) == 0) { throw new Exception("The pixel format specified has no OpenGL support"); }
            }


            IntPtr HWGLC = wglCreateContext(HDC);
            if (HWGLC.ToInt64() == 0 || HWGLC.ToInt64() < 0)
            {
                int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                switch (error)
                {
                    case 0x7D0: throw new Exception("Invalid pixel format");
                }
                throw new Exception("Context creation failed with error 0x" + error.ToString("X"));
            }
            return new Context(HDC, HWGLC);
        }

#if !NET_STANDARD
        /// <summary>
        /// Create a WGL Context for the specified Windows Form. It is up to the caller to
        /// have configured the window correctly for this (see: Windows.Form.CreateParams)
        /// </summary>
        /// <param name="Form">The form that will be used to create the context</param>
        /// <returns></returns>
        static public Context CreateContext(System.Windows.Forms.Form Form)
        {
            PIXELFORMATDESCRIPTOR desc = new PIXELFORMATDESCRIPTOR(PIXELFORMATDESCRIPTOR.PFD_DOUBLEBUFFER, PIXELFORMATDESCRIPTOR.PFD_TYPE_RGBA, 32, 32);
            return CreateContext(Form, desc);
        }

        /// <summary>
        /// Create a WGL Context for the specified Windows Form. It is up to the caller to
        /// have configured the window correctly for this (see: Windows.Form.CreateParams)
        /// </summary>
        /// <param name="Form">The form that will be used to create the context</param>
        /// <param name="PixelFormatDescriptor"></param>
        /// <returns></returns>
        static public Context CreateContext(System.Windows.Forms.Form Form, PIXELFORMATDESCRIPTOR PixelFormatDescriptor)
        {
            PixelFormatDescriptor.dwFlags |= PIXELFORMATDESCRIPTOR.PFD_DRAW_TO_WINDOW;
            IntPtr HWND = Form.Handle;
            if (HWND.ToInt64() == 0) { throw new Exception("Invalid from handle"); }
            IntPtr HDC = GetDC(HWND);
            return CreateContext(HDC, PixelFormatDescriptor);
        }

        static public Context CreateContext(System.Windows.Forms.UserControl UserControl, PIXELFORMATDESCRIPTOR PixelFormatDescriptor)
        {
            IntPtr HWND = UserControl.Handle;
            if (HWND.ToInt64() == 0) { throw new Exception("Invalid user control handle"); }
            IntPtr HDC = GetDC(HWND);
            return CreateContext(HDC, PixelFormatDescriptor);
        }
#endif
        /// <summary>
        /// Create a WGL Context for the specified Window. It is up to the caller to
        /// have configured the window correctly for this (see: Windows.Form.CreateParams)
        /// and to pass a valid handle to it (see: Form.Handle)
        /// </summary>
        /// <param name="HWND">The Window handle that will be used to create the context</param>
        /// <returns></returns>
        static public Context CreateContextFromWindowHandle(IntPtr HWND)
        {
            PIXELFORMATDESCRIPTOR desc = new PIXELFORMATDESCRIPTOR(PIXELFORMATDESCRIPTOR.PFD_DOUBLEBUFFER, PIXELFORMATDESCRIPTOR.PFD_TYPE_RGBA, 32, 32);
            return CreateContextFromWindowHandle(HWND, desc);
        }

        /// <summary>
        /// Create a WGL Context for the specified Window. It is up to the caller to
        /// have configured the window correctly for this (see: Windows.Form.CreateParams)
        /// and to pass a valid handle to it (see: Form.Handle)
        /// </summary>
        /// <param name="HWND">The Window handle that will be used to create the context</param>
        /// <param name="PixelFormatDescriptor"></param>
        /// <returns></returns>
        static public Context CreateContextFromWindowHandle(IntPtr HWND, PIXELFORMATDESCRIPTOR PixelFormatDescriptor)
        {
            PixelFormatDescriptor.dwFlags |= PIXELFORMATDESCRIPTOR.PFD_DRAW_TO_WINDOW;
            if (HWND.ToInt64() == 0) { throw new Exception("Invalid from handle"); }
            IntPtr HDC = GetDC(HWND);
            return CreateContext(HDC, PixelFormatDescriptor);
        }

        /// <summary>
        /// Get the specified OpenGL function. An exception will be raised if he function does not exist
        /// or is not supported
        /// </summary>
        /// <typeparam name="T">Delegate type for the returned function</typeparam>
        /// <param name="Name">Name of the function</param>
        /// <returns>A delegate of type T referencing the specified OpenGL function</returns>
        static public T GetProcAddress<T>(string Name)
        {
            IntPtr ptr = wglGetProcAddress(Name);
            if (ptr.ToInt64() == 0)
            {
                ptr = GetProcAddress(_OGLHandle, Name);
                if (ptr.ToInt64() == 0)
                {
                    throw new Exception("Function " + Name + " not supported");
                }
            }
            Delegate del = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer(ptr, typeof(T));
            return (T)((object)del);
        }

    }
}
