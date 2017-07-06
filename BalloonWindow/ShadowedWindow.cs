// *****************************************************************************
//	File: ShadowedWindow.cs
//
//	Description:
// 
//	History:
//		12/16/02 - Created
//
//  Copyright (c) 2002-2003 by Peter Rilling 
//	Portions Copyright © 2002 Rui Godinho Lopes
//	http://www.rilling.net/
// *****************************************************************************

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing.Imaging;
using Rilling.Common.Interop;

namespace Rilling.Common.UI.Forms
{
	public abstract class ShadowedWindow : GraphicsPathWindow
	{
		#region Initialization Constructors
		/// <summary>
		///		Initialize a new, default, instance.
		/// </summary>
		public ShadowedWindow()
		{
			InitializeComponent();
			InitializeComponentEx();
		}

		private void InitializeComponentEx()
		{
			__shadowColor		= Color.Black;
			__shadowMargin		= 5;
			__lightSource		= new Point(0, 0);
			__showingShadow		= true;
			__shadow			= null;
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}
		#endregion
		#endregion

		#region Exposed Properties
		/// <summary>
		///		Sets or retrieves the color of the shadow.
		/// </summary>
		[
		Description("Determines which System.Drawing.Color will be used to render the shadow."),
		Category("Appearance"),
		]
		public Color ShadowColor
		{
			get{return __shadowColor;}
			set
			{
				if(__shadowColor != value)
				{
					__shadowColor = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		///		Sets or retrieves the distance the shadow extends.
		/// </summary>
		[
		Description("Determines the distance from the shadow will extend from the parent window."),
		Category("Appearance"),
		]
		public int ShadowMargin
		{
			get{return __shadowMargin;}
			set
			{
				if(__shadowMargin != value)
				{
					__shadowMargin = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		///		Sets or retrieves whether the shadow should be displayed.
		/// </summary>
		[
		Description("Determines whether the shadow should be displayed."),
		Category("Appearance"),
		]
		public bool Shadow
		{
			get{return __showingShadow;}
			set
			{
				if(__showingShadow != value)
				{
					if(value)
						ShowShadow();
					else
						HideShadow();

					__showingShadow = value;
				}
			}
		}
		#endregion

		#region Public methods
		/// <summary>
		///		Displays a shadow is one is not already displayed.
		/// </summary>
		public void ShowShadow()
		{
			if(!DesignMode)
			{
				Projection shadow = __shadow;

				if(shadow == null) shadow = __shadow = CreateShadowProjection();

				// Offset the shadow from its owning window.
				int shadowMargin = ShadowMargin;
				Point shadowLocation =
					new Point(Location.X+shadowMargin, Location.Y+shadowMargin);
				Size shadowSize = Size;

				shadow.Location = shadowLocation;
				shadow.Size = shadowSize;

				shadow.Show();
			}
		}

		/// <summary>
		///		Removes the shadow if one is currently displayed.
		/// </summary>
		public void HideShadow()
		{
			if(__shadow != null)
			{
				GraphicsPathWindow shadow = __shadow;
				shadow.Hide();
			}
		}
		#endregion

		#region Protected methods
		/// <summary>
		///		Synchs the state of the shadow with the window's visible state.
		/// </summary>
		/// <param name="e">
		///		Parameter containing event information.
		/// </param>
		protected override void OnVisibleChanged(EventArgs e)
		{
			if(e == null) throw(new ArgumentNullException("e"));

			bool visibleState = Visible;

			if(__shadow != null && !visibleState)
				HideShadow();

			if(Shadow && visibleState)
				ShowShadow();

			base.OnVisibleChanged(e);
		}

		/// <summary>
		///		The shadow should be initialized if one is required.
		/// </summary>
		/// <param name="e">
		///		Parameter containing event information.
		///</param>
		protected override void OnLoad(EventArgs e)
		{
			if(e == null) throw(new ArgumentNullException("e"));

			if(Shadow)
			{
				if(__shadow == null)
				{
					Projection shadow = __shadow = CreateShadowProjection();
					ShowShadow();
				}
			}

			base.OnLoad(e);
		}

		protected override void OnPathLayoutChanged(EventArgs e)
		{
			if(__shadow != null)
			{
				GraphicsPathWindow shadow = __shadow;
				shadow.SetPath(GetPath());
			}

			base.OnPathLayoutChanged(e);
		}

		/// <summary>
		///		Ensures the shadow conforms to the parameters of its owner.
		/// </summary>
		/// <param name="e">
		///		Parameter containing event information.
		/// </param>
		//		protected override void OnPaint(PaintEventArgs e)
		//		{
		//			if(e == null) throw(new ArgumentNullException("e"));
		//
		//			if(__shadow != null)
		//			{
		//				GraphicsPathWindow shadow = __shadow;
		//
		//				ResetPath();
		//
		//				shadow.SetPath(GetPath());
		//				shadow.Invalidate();
		//			}
		//
		//			base.OnPaint(e);
		//		}

		/// <summary>
		///		Ensures that the shadow follows its parent.
		/// </summary>
		/// <param name="e">
		///		Parameter containing event information.
		/// </param>
		protected override void OnMove(EventArgs e)
		{
			if(e == null) throw(new ArgumentNullException("e"));

			if(__shadow != null)
			{
				Point shadowLocation = 
					new Point(Location.X+ShadowMargin, Location.Y+ShadowMargin);
				__shadow.Location = shadowLocation;
			}

			base.OnMove(e);
		}

		/// <summary>
		///		Ensures that the shadow follows its parent.
		/// </summary>
		/// <param name="e">
		///		Parameter containing event information.
		/// </param>
		protected override void OnResize(EventArgs e)
		{
			if(e == null) throw(new ArgumentNullException("e"));
			
			if(__shadow != null) __shadow.Size = Size;

			base.OnResize(e);
		}

		/// <summary>
		///		Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		#endregion

		#region Helper methods
		/// <summary>
		///		Connect the window and its shadow.
		/// </summary>
		/// <param name="shadowWindow">
		///		The window's shadow.
		/// </param>
		/// <param name="owningWindow">
		///		The window that owns the shadow.
		/// </param>
		private void BindShadowToOwner(Projection shadowWindow,
			GraphicsPathWindow owningWindow)
		{
			//if(!DesignMode)
			//	owningWindow.Owner = shadowWindow;
		}

		/// <summary>
		///		Initializes a new shadow projects for this window.
		/// </summary>
		/// <returns>
		///		The shadow projection for the window.
		/// </returns>
		private Projection CreateShadowProjection()
		{
			Projection shadow = new Projection(this);
			shadow.BackColor = Color.White;
			//			shadow.ShowInTaskbar = false;
			shadow.StartPosition = FormStartPosition.Manual;

			BindShadowToOwner(shadow, this);

			return shadow;
		}
		#endregion

		private Color				__shadowColor;
		private int					__shadowMargin;
		private Point				__lightSource;
		private bool				__showingShadow;
		private Projection			__shadow;
		private IContainer			components = null;

		private class Projection : GraphicsPathWindow
		{
			public Projection(ShadowedWindow owner)
			{
				__owner = owner;
				StartPosition = FormStartPosition.Manual;
				//				TopLevel			= true;
				//				TopMost				= true;
				OnPaint(null);
			}

			private ShadowedWindow __owner;

			protected override void OnPaint(PaintEventArgs e)
			{
				//if(e == null) throw(new ArgumentNullException("e"));
				Bitmap img = new Bitmap(Width, Height);
				GraphicsPath path = GetPath();
				Graphics grx = Graphics.FromImage(img);
				float scaleFactor = 1F-((float)__owner.ShadowMargin*2/(float)Width);
				PathGradientBrush backStyle = new PathGradientBrush(path);
				backStyle.CenterPoint = new Point(0, 0);
				backStyle.CenterColor = __owner.ShadowColor;
				backStyle.FocusScales = 
					new PointF(scaleFactor, scaleFactor);
				backStyle.SurroundColors = 
					new Color[]{Color.Transparent};

				Region region = new Region(path);
				region.Translate(-__owner.ShadowMargin, -__owner.ShadowMargin);
				
				grx.SetClip(region, CombineMode.Xor);

				grx.FillPath(backStyle, path);

				SetBitmap(img);

				//base.OnPaint(e);
			}

			/// <summary>
			///		Changes the current bitmap.
			/// </summary>
			public void SetBitmap(Bitmap bitmap) 
			{
				SetBitmap(bitmap, 255);
			}

			protected override GraphicsPath PreparePath()
			{
				GraphicsPath path = __owner.GetPath();
				return path;
			}

			/// <para>Changes the current bitmap with a custom opacity level.  Here is where all happens!</para>
			public void SetBitmap(Bitmap bitmap, byte opacity) 
			{
				if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
					throw new ApplicationException("The bitmap must be 32ppp with alpha-channel.");

				// The ideia of this is very simple,
				// 1. Create a compatible DC with screen;
				// 2. Select the bitmap with 32bpp with alpha-channel in the compatible DC;
				// 3. Call the UpdateLayeredWindow.

				IntPtr screenDc = Win32.GetDC(IntPtr.Zero);
				IntPtr memDc = Win32.CreateCompatibleDC(screenDc);
				IntPtr hBitmap = IntPtr.Zero;
				IntPtr oldBitmap = IntPtr.Zero;

				try 
				{
					hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));  // grab a GDI handle from this GDI+ bitmap
					oldBitmap = Win32.SelectObject(memDc, hBitmap);

					Win32.Size size = new Win32.Size(bitmap.Width, bitmap.Height);
					Win32.Point pointSource = new Win32.Point(0, 0);
					Win32.Point topPos = new Win32.Point(Left, Top);
					Win32.BLENDFUNCTION blend = new Win32.BLENDFUNCTION();
					blend.BlendOp             = Win32.AC_SRC_OVER;
					blend.BlendFlags          = 0;
					blend.SourceConstantAlpha = opacity;
					blend.AlphaFormat         = Win32.AC_SRC_ALPHA;

					Win32.UpdateLayeredWindow(Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, Win32.ULW_ALPHA);
				}
				finally
				{
					Win32.ReleaseDC(IntPtr.Zero, screenDc);
					if (hBitmap != IntPtr.Zero) 
					{
						Win32.SelectObject(memDc, oldBitmap);
						//Windows.DeleteObject(hBitmap); // The documentation says that we have to use the Windows.DeleteObject... but since there is no such method I use the normal DeleteObject from Win32 GDI and it's working fine without any resource leak.
						Win32.DeleteObject(hBitmap);
					}
					Win32.DeleteDC(memDc);
				}
			}

			protected override CreateParams CreateParams	
			{
				get 
				{
					CreateParams cp = base.CreateParams;
					cp.ExStyle |= 0x00080000; // This form has to have the WS_EX_LAYERED extended style
					return cp;
				}
			}
		}
	}
}