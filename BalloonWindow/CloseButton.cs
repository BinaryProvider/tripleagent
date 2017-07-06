// *****************************************************************************
//	File: CloseButton.cs
//
//	Description:
//
//	History:
//
//  Copyright (c) 2002-2003 by Peter Rilling 
//	http://www.rilling.net/
// *****************************************************************************

using System;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Data;
using System.IO;
using System.Resources;
using System.Windows.Forms;

namespace Rilling.Common.UI.Controls
{
	/// <summary>
	/// Summary description for CloseButton.
	/// </summary>
	public class CloseButton : UserControl
	{
		public CloseButton()
		{
			this.Size = new Size(18, 18);
		}

		private void DrawButtonUp(Graphics grx)
		{
			Image buttonImg = GetButtonUpImage();
			grx.DrawImageUnscaled(buttonImg, 0, 0);
		}

		private void DrawButtonHover(Graphics grx)
		{
			Image buttonImg = GetButtonHoverImage();
			grx.DrawImageUnscaled(buttonImg, 0, 0);
		}

		private void DrawButtonDown(Graphics grx)
		{
			Image buttonImg = GetButtonDownImage();
			grx.DrawImageUnscaled(buttonImg, 0, 0);
		}

		private Image GetButtonUpImage()
		{
			Assembly assembly = Assembly.GetAssembly(typeof(CloseButton));
			Stream stream =
				assembly.GetManifestResourceStream("Rilling.UI.BalloonWindow.CloseUp.bmp");
			Image img = Image.FromStream(stream);

			return img;




//			Bitmap img = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
//			Graphics grx = Graphics.FromImage(img);
//			GraphicsPath path = new GraphicsPath();
//
//			path.AddArc(0, 0, 1, 1, 180, 90);
//			path.AddArc(Width-2, 0, 1, 1, -90, 90);
//			path.AddArc(Width-2, Height-2, 1, 1, 90, 90);
//			path.AddArc(0, Height-2, 1, 1, 0, 90);
//			path.CloseFigure();
//			grx.DrawPath(new Pen(Color.FromArgb(199, 190, 166), 1), path);
//
//			SolidBrush brush = new SolidBrush(Color.FromArgb(255, 255, 245));
//			grx.FillPath(brush, path);

		}

		private Image GetButtonDownImage()
		{
			Assembly assembly = Assembly.GetAssembly(typeof(CloseButton));
			Stream stream =
				assembly.GetManifestResourceStream("Rilling.UI.BalloonWindow.CloseDown.bmp");
			Image img = Image.FromStream(stream);

			return img;

//			Bitmap img = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
//			Graphics grx = Graphics.FromImage(img);
//
//			grx.DrawLine(Pens.Red, new Point(0,0), new Point(Width, Height));
//
//			return img;
		}

		private Image GetButtonHoverImage()
		{
			Assembly assembly = Assembly.GetAssembly(typeof(CloseButton));
			Stream stream =
				assembly.GetManifestResourceStream("Rilling.UI.BalloonWindow.CloseHover.bmp");
			Image img = Image.FromStream(stream);

			return img;

//			Bitmap img = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
//			Graphics grx = Graphics.FromImage(img);
//
//			GraphicsPath path = new GraphicsPath();
//
//			path.AddArc(0, 0, 1, 1, 180, 90);
//			path.AddArc(Width-2, 0, 1, 1, -90, 90);
//			path.AddArc(Width-2, Height-2, 1, 1, 90, 90);
//			path.AddArc(0, Height-2, 1, 1, 0, 90);
//			path.CloseFigure();
//			grx.DrawPath(Pens.White, path);
//
//			PathGradientBrush brush = new PathGradientBrush(path);
//							  brush.CenterColor = Color.FromArgb(255, 222, 18);
//							  brush.CenterPoint = new Point(10, 10);
//							  brush.SurroundColors = new Color[] {Color.Green};
//			grx.FillPath(brush, path);
//
//			return img;
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			BackgroundImage = GetButtonHoverImage();

			base.OnMouseEnter(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			Point mousePos = new Point(e.X, e.Y);
			Rectangle bounds = Bounds;

			if(this.Capture)
			{
				if(bounds.Contains(mousePos))
					BackgroundImage = GetButtonDownImage();
				else
					BackgroundImage = GetButtonHoverImage();
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			BackgroundImage = GetButtonUpImage();

			base.OnMouseLeave(e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			BackgroundImage = GetButtonDownImage();

			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			BackgroundImage = GetButtonHoverImage();

			base.OnMouseUp(e);
		}

		protected override void OnLoad(EventArgs e)
		{
			BackgroundImage = GetButtonUpImage();

			base.OnLoad(e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			// TODO: Add custom paint code here

			// Calling the base class OnPaint
			base.OnPaint(e);
		}
	}
}
