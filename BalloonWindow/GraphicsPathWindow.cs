// *****************************************************************************
//	File: GraphicsPathWindow.cs
//
//  Contains:
//		GraphicsPathWindow class
//
//	Description:
//		This class allows the client application to define a custom shape for
//		any inherited forms. 
//
//  Copyright (c) 2002-2003 by Peter Rilling 
//	http://www.rilling.net/
// *****************************************************************************
 
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Windows.Forms;

namespace Rilling.Common.UI.Forms
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class GraphicsPathWindow : Form
	{
		#region Creation and Destruction
		/// <summary>
		///		Initialize a new, default, instance.
		/// </summary>
		public GraphicsPathWindow()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint |
				ControlStyles.DoubleBuffer |
				ControlStyles.UserPaint, true);

			__shape				= null;
			__borderColor		= SystemColors.WindowFrame;
			__borderStyle		= null;
			__backStyle			= null;
			BackColor			= SystemColors.Window;
			FormBorderStyle		= FormBorderStyle.None;
		}
		#endregion

		#region Color and Style Management
		/// <summary>
		///		Sets the style for the form.  This property should not
		///		be set when using this, or any inherited, classes.
		/// </summary>
		[
		Browsable(false)
		]
		public new FormBorderStyle FormBorderStyle
		{
			get{return base.FormBorderStyle;}
			set{base.FormBorderStyle = value;}
		}

		/// <summary>
		///		Occurs when the value of the <see cref="BorderColor"/> property
		///		changes.
		/// </summary>
		[
		Description("Occurs when the BorderColor property has changed."),
		Category("Property Changed"),
		]
		public event EventHandler BorderColorChanged;

		/// <summary>
		///		Occurs when the value of the <see cref="BorderStyle"/> property
		///		changes.
		/// </summary>
		[
		Description("Occurs when the BorderStyle property has changed."),
		Category("Property Changed"),
		]
		public event EventHandler BorderStyleChanged;

		/// <summary>
		///		Occurs when the value of the <see cref="BackStyle"/> property
		///		changes.
		/// </summary>
		[
		Description("Occurs when the BackStyle property has changed."),
		Category("Property Changed"),
		]
		public event EventHandler BackStyleChanged;

		/// <summary>
		///		Sets or retreives the border color.
		/// </summary>
		[
		Description("Specifies which System.Drawing.Color will be used to render the border."),
		Category("Appearance"),
		]
		public virtual Color BorderColor
		{
			get{return __borderColor;}
			set
			{
				if(__borderColor != value)
				{
					__borderColor = value;
					OnBorderColorChanged(new EventArgs());
				}
			}
		}

		/// <summary>
		///		Sets or retreives the border style.  Takes precedence over
		///		<see cref="BorderColor"/> when set.
		/// </summary>
		[
		Browsable(false)
		]
		public virtual Pen BorderStyle
		{
			get{return __borderStyle;}
			set
			{
				if(__borderStyle != value)
				{
					__borderStyle		= value;
					__borderStyleIntern = 
						(value == null ? null : (Pen)__borderStyle.Clone());

					// Currently the .NET frameword has a bug with Inset such
					// that the border did not render correctly.  To compensate,
					// I fack the system with the _borderStyleIntern, where
					// I adjust the properties so the display is correct.
					//
					// This also override the user defined settings but the 
					// original object is never altered, in case the user
					// requests it.
					if(__borderStyleIntern != null)
					{
						__borderStyleIntern.Alignment = PenAlignment.Center;
						__borderStyleIntern.Width = 
							__borderStyleIntern.Width+(__borderStyleIntern.Width-1);
					}

					OnBorderStyleChanged(new EventArgs());
				}
			}
		}

		/// <summary>
		///		Sets or retrieves the background style.  Takes presedence over
		///		<see cref="BackColor"/> when set.
		/// </summary>
		[
		Browsable(false)
		]
		public virtual Brush BackStyle
		{
			get{return __backStyle;}
			set
			{
				if(__backStyle != value)
				{
					__backStyle = value;
					OnBackStyleChanged(new EventArgs());
				}
			}
		}

		/// <summary>
		///		Raises the <see cref="BorderColorChanged"/> event.
		/// </summary>
		/// <param name="e">
		///		Arguments passed to the event handler.
		/// </param>
		protected virtual void OnBorderColorChanged(EventArgs e)
		{
			if(e == null) throw(new ArgumentNullException("e"));

			Invalidate();
			if(BorderColorChanged != null)
				BorderColorChanged(this, e);
		}

		/// <summary>
		///		Raises the <see cref="BorderStyleChanged"/> event.
		/// </summary>
		///		Arguments passed to the event handler.
		/// <param name="e"></param>
		protected virtual void OnBorderStyleChanged(EventArgs e)
		{
			if(e == null) throw(new ArgumentNullException("e"));

			Invalidate();
			if(BorderStyleChanged != null) 
				BorderStyleChanged(this, e);
		}

		/// <summary>
		///		Raises the <see cref="BackStyleChanged"/> event.
		/// </summary>
		/// <param name="e">
		///		Arguments passed to the event handler.
		/// </param>
		protected virtual void OnBackStyleChanged(EventArgs e)
		{
			if(e == null) throw(new ArgumentNullException("e"));

			Invalidate();
			if(BackStyleChanged != null)
				BackStyleChanged(this, e);
		}
		#endregion

		#region Shape Management
		/// <summary>
		///		Occurs when the the window shape has changes.
		/// </summary>
		[
		Description("Occurs when the border shape has changed or is recalculated."),
		Category("Layout"),
		]
		public event EventHandler PathLayoutChanged;

		/// <summary>
		///		Raises the <see cref="PathLayoutChanged"/> event.
		/// </summary>
		/// <param name="e">
		///		Arguments passed to the event handler.
		/// </param>
		protected virtual void OnPathLayoutChanged(EventArgs e)
		{
			if(e == null) throw(new ArgumentNullException("e"));

			Invalidate();
			if(PathLayoutChanged != null)
				PathLayoutChanged(this, e);
		}

		/// <summary>
		///		Calculates the shape of the window.  Each derived class should
		///		override this method and provide their own calculated shape.
		/// </summary>
		/// <returns>
		///		A <see cref="GraphicsPath"/> defining the window shape.
		/// </returns>
		protected abstract GraphicsPath PreparePath();

		/// <summary>
		///		Sets the path shape.
		/// </summary>
		/// <param name="path">
		///		A <see cref="GraphicsPath"/> defining the window shape.
		/// </param>
		/// <remarks>
		///		The <see cref="PathLayoutChanged"/> event will not fire if
		///		a reference to the set path is changed outside of this object.
		/// </remarks>
		public void SetPath(GraphicsPath path)
		{
			if(path == null) throw(new ArgumentNullException("path"));

			if(__shape != path)
			{
				__shape = path;

				// Raise the event.
				OnPathLayoutChanged(new EventArgs());
			}
		}

		/// <summary>
		///		Gets the path shape.  If no path exists, then 
		///		<see cref="PreparePath"/> is called.
		/// </summary>
		/// <returns>
		///		A <see cref="GraphicsPath"/> defining the window shape.
		/// </returns>
		public GraphicsPath GetPath()
		{
			// Only calculate of not already available.
			return GetPath(false);
		}

		/// <summary>
		///		Gets the path shape.  If no path exists, then 
		///		<see cref="PreparePath"/> is called.
		/// </summary>
		/// <param name="forceCalc">
		///		Pass true if the path shape should always be recalculated, even
		///		if cached, otherwise pass false.
		/// </param>
		/// <returns>
		///		A <see cref="GraphicsPath"/> defining the window shape.
		/// </returns>
		/// <remarks>
		///		If the shape is being forced to recalculate then the
		///		<see cref="PathLayoutChanged"/> event will fire even if no 
		///		actual changes to the path occured.
		/// </remarks>
		public GraphicsPath GetPath(bool forceCalc)
		{
			if(forceCalc) ResetPath();
			GraphicsPath gp = __shape;

			if(gp == null)
			{
				gp = PreparePath();
				if(gp == null)
					throw(new InvalidOperationException(ErrorManager.GetErrorString("ERR_BW_PARAM_NULSTATE")));

				SetPath(gp);
			}

			return gp;
		}

		/// <summary>
		///		Clears any existing path.  Will force the derived class to 
		///		provide the shape again.
		/// </summary>
		/// <remarks>
		///		The <see cref="PathLayoutChanged"/> event will always fire the next
		///		time the path is aquired even if no actual changes where made.
		/// </remarks>
		public void ResetPath()
		{
			if(__shape != null)
			{
				__shape = null;
				Invalidate();
			}
		}
		#endregion

		#region Display Management
		/// <summary>
		///		Renders renders the balloon design.
		/// </summary>
		/// <param name="e"> 
		///		Arguments passed to the event handler.
		/// </param>
		protected override void OnPaint(PaintEventArgs e)
		{
			if( e == null) throw(new ArgumentException("e"));
			Brush backStyle; Pen borderStyle; Image backImage;

			GraphicsPath gp = GetPath();
			Graphics grx = e.Graphics;

			Region = RegionFromPath(gp); 

			// Determine how the border should be styled.
			borderStyle = 
				(BorderStyle == null ? new Pen(BorderColor) : __borderStyleIntern);

			// Draw the background color or patterns.
			backStyle = 
				(BackStyle == null ? new SolidBrush(BackColor) : BackStyle);
			grx.FillPath(backStyle, gp);

			// Draw background image if available.
			if((backImage = BackgroundImage) != null)
			{
				Rectangle destRect = new Rectangle(0, 0, Width-1, Height-1);
				Rectangle srcRect = 
					new Rectangle(0, 0, backImage.Width-1, backImage.Height-1);

				grx.DrawImage(backImage, destRect, srcRect, GraphicsUnit.Pixel);
			}

			grx.DrawPath(borderStyle, gp);

			base.OnPaint(e);
		}

		/// <summary>
		///		Translates a <see cref="GraphicsPath"/> object into a 
		///		<see cref="Region"/> object.
		/// </summary>
		/// <param name="gp">
		///		The <see cref="GraphicsPath"/> that will become the windows shape.
		/// </param>
		/// <returns>
		///		A <see cref="Region"/> defining the shape of the
		///		<see cref="GraphicsPath"/>.
		/// </returns>
		private Region RegionFromPath(GraphicsPath gp)
		{
			GraphicsPath tmpPath = (GraphicsPath)gp.Clone();
			tmpPath.Widen(Pens.Black);
			Region region = new Region(tmpPath);
			region.Union(gp);

			return region;
		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnResize(EventArgs e)
		{
			ResetPath();
			base.OnResize(e);
		}

		private GraphicsPath	__shape;
		private Color			__borderColor;
		private Pen				__borderStyle;
		private Pen				__borderStyleIntern;
		private Brush			__backStyle;
	}
}