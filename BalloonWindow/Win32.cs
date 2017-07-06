// *****************************************************************************
//	File: ShadowedWindow.cs
//
//	Description:
//
//	History:
//
//  Copyright (c) 2002-2003 by Peter Rilling 
//	Portions Copyright � 2002 Rui Godinho Lopes
//	http://www.rilling.net/
// *****************************************************************************

using System;
using System.Runtime.InteropServices;

namespace Rilling.Common.Interop
{
	public class Win32 
	{
		
		public const int GWL_EXSTYLE = -20;

		public enum ClassStyle
		{
			DropShadow = 0x00020000
		}

		public enum Bool 
		{
			False = 0,
			True
		};

		[StructLayout(LayoutKind.Sequential)]
			public struct Point 
		{
			public Int32 x;
			public Int32 y;

			public Point(Int32 x, Int32 y) { this.x= x; this.y= y; }
		}

		[StructLayout(LayoutKind.Sequential)]
			public struct Size 
		{
			public Int32 cx;
			public Int32 cy;

			public Size(Int32 cx, Int32 cy) { this.cx= cx; this.cy= cy; }
		}

		[StructLayout(LayoutKind.Sequential, Pack=1)]
			struct ARGB 
		{
			public byte Blue;
			public byte Green;
			public byte Red;
			public byte Alpha;
		}

		[StructLayout(LayoutKind.Sequential, Pack=1)]
			public struct BLENDFUNCTION 
		{
			public byte BlendOp;
			public byte BlendFlags;
			public byte SourceConstantAlpha;
			public byte AlphaFormat;
		}


		public const Int32 ULW_COLORKEY = 0x00000001;
		public const Int32 ULW_ALPHA    = 0x00000002;
		public const Int32 ULW_OPAQUE   = 0x00000004;

		public const byte AC_SRC_OVER  = 0x00;
		public const byte AC_SRC_ALPHA = 0x01;


		[DllImport("user32.dll", ExactSpelling=true, SetLastError=true)]
		public static extern Bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc, Int32 crKey, ref BLENDFUNCTION pblend, Int32 dwFlags);

		[DllImport("user32.dll", ExactSpelling=true, SetLastError=true)]
		public static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("user32.dll", ExactSpelling=true)]
		public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

		[DllImport("gdi32.dll", ExactSpelling=true, SetLastError=true)]
		public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

		[DllImport("gdi32.dll", ExactSpelling=true, SetLastError=true)]
		public static extern Bool DeleteDC(IntPtr hdc);

		[DllImport("gdi32.dll", ExactSpelling=true)]
		public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

		[DllImport("gdi32.dll", ExactSpelling=true, SetLastError=true)]
		public static extern Bool DeleteObject(IntPtr hObject);

		[DllImport("user32.dll", ExactSpelling=true, SetLastError=true)]
		public static extern long SetWindowLong(IntPtr hWnd,
												int nIndex, 
												long dwNewLong);
	}
}