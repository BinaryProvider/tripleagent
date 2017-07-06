// *****************************************************************************
//	File: BalloonWindow.cs
//
//	Description:
//
//	History:
//		12.28.2002 - Created.
// 
//  Copyright (c) 2002-2003 by Peter Rilling 
//	http://www.rilling.net/
// *****************************************************************************

using System;
using System.Windows.Forms;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using Rilling.Common.UI.Controls;

// Map namespace to an alias.
using Threading = System.Threading;

namespace Rilling.Common.UI.Forms
{
	public class BalloonWindow : ShadowedWindow
	{
		/// <summary>
		/// 
		/// </summary>
		private int CLOSEBOX_DISTANCE_FROM_EDGE = 4;

		#region Movement and Position Management
		/// <summary>
		///		Shows the balloon and anchors it to the given point.
		/// </summary>
		/// <param name="pt">
		///		The point the anchor will occupy, in screen coordinates.
		/// </param>
		public void Show(Point pt)
		{
			// Move the balloon to anchor position.
			MoveAnchorTo(pt);

			// Show the balloon.
			if(!Visible) Show();
		}

		/// <summary>
		///		Shows the balloon and anchors it to the given rectangle.
		/// </summary>
		/// <param name="rect">
		///		The rectangle the anchor will occupy, in screen coordinates.
		/// </param>
		/// <remarks>
		///		If <see cref="AllowObscure"/> is true, the anchor will be 
		///		centered in the rectangle, otherwise it will be anchored to a point
		///		outside the rectangle.
		/// </remarks>
		public void Show(Rectangle rect)
		{
			if(rect.IsEmpty) throw(new ArgumentException());

			// Move the balloon to anchor position.
			MoveAnchorTo(rect);

			// Show the balloon.
			if(!Visible) Show();
		}

		/// <summary>
		///		Shows the balloon and anchors it to the given control.
		/// </summary>
		/// <param name="control">
		///		The control the anchor will occupy.
		/// </param>
		/// <remarks>
		///		If <see cref="AllowObscure"/> is true, the anchor will be 
		///		centered in the control, otherwise it will be anchored to a point
		///		outside the control bounds.
		/// </remarks>
		public void Show(Control control)
		{
			if(control == null) throw(new ArgumentNullException("control"));

			// Move the balloon to anchor position.
			MoveAnchorTo(control);

			// Show the balloon.
			if(!Visible) Show();
		}

		/// <summary>
		///		Moves the anchor to the specified rectangle.
		/// </summary>
		/// <param name="rect">
		///		The rectangle the anchor will occupy, in screen coordinates.
		/// </param>
		/// <remarks>
		///		If <see cref="AllowObscure"/> is true, the anchor will be 
		///		centered in the rectangle, otherwise it will be anchored to a point
		///		outside the rectangle.
		/// </remarks>
		public void MoveAnchorTo(Rectangle rect)
		{
			if(rect.IsEmpty) throw(new ArgumentException());
			bool allowObscure = AllowObscure;
			Rectangle screenBounds = Screen.GetBounds(rect);

			// Calculate the anchor point.
			rect.Intersect(screenBounds);	// restrict to visible screen.
			int centerX = ((rect.Width-1)/2)+rect.Left;
			int centerY = ((rect.Height-1)/2)+rect.Top;
			Point anchorPoint = new Point(centerX, centerY);
			Point pivotPoint = anchorPoint;
			AnchorQuadrant anchorQuadrant = __layout.AnchorQuadrant;

			if(!allowObscure)
			{
				// Move the anchor point out of the rectangle region.
				Size offset =
					new Size(Math.Abs(centerX-rect.Left+4), 
					Math.Abs(centerY-rect.Top+4));

				switch(anchorQuadrant)
				{
					case AnchorQuadrant.Top:
						anchorPoint.Offset(0, offset.Height+1);
						break;
					case AnchorQuadrant.Bottom:
						anchorPoint.Offset(0, -offset.Height);
						break;
					case AnchorQuadrant.Left:
						anchorPoint.Offset(offset.Width+1, 0);
						break;
					case AnchorQuadrant.Right:
						anchorPoint.Offset(-offset.Width, 0);
						break;
				}
			}

			MoveAnchorTo(anchorPoint, pivotPoint);
			__targetRect = rect;
		}

		/// <summary>
		///		Moves the anchor to the specified control.
		/// </summary>
		/// <param name="control">
		///		The control the anchor will occupy.
		/// </param>
		/// <remarks>
		///		If <see cref="AllowObscure"/> is true, the anchor will be 
		///		centered in the control, otherwise it will be anchored to a point
		///		outside the control bounds.
		/// </remarks>
		public void MoveAnchorTo(Control control)
		{
			if(control == null) throw(new ArgumentNullException("control"));
			Control containerControl = control.Parent;
			Rectangle containerBounds;
			Rectangle visibleBounds = 
				control.RectangleToScreen(control.ClientRectangle);
			Rectangle screenBounds = Screen.GetBounds(control);

			if(!InSafeContext)
			{
				if(!control.Visible)
				{
					// The control must be visible.
					throw(new ArgumentOutOfRangeException("control", control, 
						ErrorManager.GetErrorString("ERR_BW_ANCHOR_CONTROLINVISIBLE", 
						control)));
				}

				if(!screenBounds.IntersectsWith(visibleBounds))
				{
					// The control must be located on a visible part of the screen.
					throw(new ArgumentOutOfRangeException("control", control,
						ErrorManager.GetErrorString("ERR_BW_ANCHOR_VISIBLECONTROL",
						control)));
				}
			}

			while(containerControl != null && !visibleBounds.IsEmpty)
			{
				// Make sure that the control is visible in each parent
				// container.
				containerBounds = 
					containerControl.RectangleToScreen(containerControl.ClientRectangle);

				visibleBounds =
					Rectangle.Intersect(visibleBounds, containerBounds);

				containerControl = containerControl.Parent;
			}

			visibleBounds =
				Rectangle.Intersect(screenBounds, visibleBounds);

			if(!InSafeContext)
			{
				if(visibleBounds == Rectangle.Empty)
					// The control must be visible.
					throw(new ArgumentOutOfRangeException("control", control, 
						ErrorManager.GetErrorString("ERR_BW_ANCHOR_CONTROLBOUND", 
						control)));
			}

			if(TargetControl != null && MoveWithTarget)
			{
				if(visibleBounds == Rectangle.Empty && Visible)
					Hide();
				else if(visibleBounds != Rectangle.Empty && !Visible)
					Show();
			}

			if(visibleBounds != Rectangle.Empty) MoveAnchorTo(visibleBounds);

			Owner = control.FindForm();
			TargetControl = control;
		}

		/// <summary>
		///		Moves the anchor to specified point.
		/// </summary>
		/// <param name="point">
		///		The point the anchor will occupy, in screen coordinates.
		/// </param>
		public void MoveAnchorTo(Point point)
		{
			MoveAnchorTo(point, point);
			__targetRect = Rectangle.Empty;
		}

		/// <summary>
		///		Moves the anchor to specified point.
		/// </summary>
		/// <param name="point">
		///		The point the anchor will occupy, in screen coordinates.
		/// </param>
		/// <param name="pivot">
		///		A focal (pivot) point is used when the balloon cannot fit on 
		///		the screen.  When the balloon is moved, the distance
		///		between the anchor and the pivot is maintained.  Useful when
		///		auto-orientation is set.
		/// </param>
		private void MoveAnchorTo(Point point, Point pivot)
		{
			Rectangle screenBounds = Screen.GetBounds(point);
			Rectangle balloonBounds = Bounds;
			Point pivotPoint = pivot;

			if(!InSafeContext)
			{
				if(__targetControl != null && MoveWithTarget)
					// Make sure the anchor is not already bound to a control.
					throw(new InvalidOperationException(
						ErrorManager.GetErrorString("ERR_BW_ANCHOR_ANCHORCONTROL",
						point, __targetControl)));

				if(!screenBounds.Contains(point))
					// Make sure the point is contained within the visible screen.
					throw(new ArgumentOutOfRangeException("point", point, 
						ErrorManager.GetErrorString("ERR_BW_ANCHOR_VISIBLESCREEN", 
						point)));
			}

			// Make sure relative distance is maintained between edges 
			// and anchor.
			Point anchorPoint = AnchorPoint;
			balloonBounds.Offset(point.X-anchorPoint.X, point.Y-anchorPoint.Y);
			Location = balloonBounds.Location;

			// Make sure the balloon remains on the visible screen.
			if(!screenBounds.Contains(balloonBounds))
			{
				Point layoutAnchorPoint = __layout.AnchorPoint;
				RecalcLayout();
				if(layoutAnchorPoint != __layout.AnchorPoint)
				{
					ResetPath();

					// Make sure relative distance is maintained between edges 
					// and anchor.
					anchorPoint = AnchorPoint;
					Size offsetBy = new Size(point.X-anchorPoint.X, point.Y-anchorPoint.Y);
					if(point.X != anchorPoint.X) offsetBy.Width += (pivot.X-point.X)*2;
					if(point.Y != anchorPoint.Y) offsetBy.Height += (pivot.Y-point.Y)*2;
						
						
					balloonBounds.Offset(offsetBy.Width, offsetBy.Height);
				}
			}

			Location = balloonBounds.Location;
		}

		/// <summary>
		///		Moves the anchor to the specified X and Y coordinates.
		/// </summary>
		/// <param name="x">
		///		The horizontal coordinate.
		/// </param>
		/// <param name="y">
		///		The vertical coordinate.
		/// </param>
		public void MoveAnchorTo(int x, int y)
		{
			MoveAnchorTo(new Point(x, y));
		}

		/// <summary>
		///		Shifts the anchor by  <paramref name="dx"/> and 
		///		<paramref name="dy"/>.
		/// </summary>
		/// <param name="dx">
		///		The horizontal delta.
		/// </param>
		/// <param name="dy">
		///		The vertical delta.
		/// </param>
		public void OffsetAnchorBy(int dx, int dy)
		{
			if(dx == 0 && dy == 0) return;
			Point anchorPoint = AnchorPoint;

			MoveAnchorTo(new Point(anchorPoint.X+dx, anchorPoint.Y+dy));
		}

		/// <summary>
		///		Shifts the anchor by the dimensions specified in 
		///		<paramref name="size"/>.
		/// </summary>
		/// <param name="size">
		///		Specifies the horizontal and vertical offsets.
		/// </param>
		public void OffsetAnchorBy(Size size)
		{
			OffsetAnchorBy(size.Width, size.Height);
		}
		#endregion

		#region Layout Management
		/// <summary>
		///		Sets or retrieves the quadrant the anchor will be displayed in
		///		as defined by the <see cref="AnchorQuadrant"/> enumeration.
		/// </summary>
		/// <remarks>
		///		When <see cref="AnchorQuadrent.Auto"/> is set, 
		///		<see cref="BalloonWindow"/> will calculate the quadrant to 
		///		ensure the best placement for the anchor based on its position on
		///		the screen to ensure the balloon remains entirly visible on the 
		///		screen, when possible.
		/// </remarks>
		[
		Description("Determines which side the anchor will be displayed on.  Auto " +
					"will allow the best placement to be calculated."),
		Category("Layout")
		]
		public AnchorQuadrant AnchorQuadrant
		{
			get{return __anchorQuadrant;}
			set
			{
				if(__anchorQuadrant != value)
				{
					__anchorQuadrant = value;

					Point origAnchorPoint = AnchorPoint;
					ResetPath();
					if(Visible)
					{
						if(__targetRect != Rectangle.Empty)
						{
							EnterSafeContext();
							MoveAnchorTo(__targetRect);
							ExitSafeContext();
						}
						else
							MoveAnchorTo(origAnchorPoint);
					}
				}
			}
		}

		/// <summary>
		///		The base anchor quadrant for the balloon.
		/// </summary>
		/// <remarks>
		///		<p>
		///		The base anchor quadrant allows the client to indicate the starting
		///		quadrant when <see cref="AnchorQuadrant"/> is set to 
		///		<see cref="AnchorQuadrant.Auto"/>.  This must be set prior to the 
		///		the first display of the balloon.  This property is ignored after
		///		the balloon is first displayed.
		///		</p>
		///		<p>
		///		The value cannot be set to <see cref="AnchorQuadrant.Auto"/>.
		///		</p>
		/// </remarks>
		[
		Description("Determines which side the anchor will be initially displayed " +
					"on when AnchorQuadrant is set to Auto"),
		Category("Layout")
		]
		public AnchorQuadrant AnchorQuadrantBase
		{
			get{return __baseAnchorQuadrant;}
			set
			{
				// Only valid before the balloon is first displayed.
				if((!IsLoaded && __baseAnchorQuadrant != value) || DesignMode) 
				{
					if(value == AnchorQuadrant.Auto) 
						throw(new ArgumentException());
				
					__layout.AnchorQuadrant = __baseAnchorQuadrant = value;
					ResetPath();
				}
			}
		}

		/// <summary>
		///		Retrieves the anchor quadrant location.
		/// </summary>
		/// <remarks>
		///		This is necessary because the client can set the value of 
		///		<see cref="AnchorQuadrant"/> to <see cref="AnchorQuadrant.Auto"/>
		///		which makes no sense when calculating the layout.  The actual 
		///		quadrant, as used by the layout, is returned.
		/// </remarks>
		private AnchorQuadrant AnchorQuadrantEx
		{
			get
			{
				Rectangle screenBounds			= Screen.GetBounds(this);
				Rectangle balloonBounds			= Bounds;
				AnchorQuadrant anchorQuadrant	= AnchorQuadrant;
				int anchorOffset				= AnchorOffset;

				// Need to have the quadrant calculated based on anchor and balloon
				// position.
				if(anchorQuadrant == AnchorQuadrant.Auto)
					GetAnchorLayout(ref anchorQuadrant, ref anchorOffset, 
						__pivotPoint);

				return anchorQuadrant;
			}
		}
		
		/// <summary>
		///		Sets or retrieves the increasing distance along the anchor quadrant
		///		edge where the anchor tip will be positioned.
		/// </summary>
		/// <remarks>
		///		<p>
		///		When the value is -1, <see cref="BalloonWindow"/> will calculate the
		///		offset based on the balloon's screen position.  When the value is -2,
		///		the anchor will maintain a center position.
		///		</p>
		/// </remarks>
		[
		Description("Determines the distance along the anchor quadrant edge where " +
					"the anchor tip will be positioned."),
		Category("Layout"),
		]
		public int AnchorOffset
		{
			get{return __anchorOffset;}
			set
			{
				if(value < -2) throw(new ArgumentOutOfRangeException("value"));

				if(__anchorOffset != value)
				{
					int oldAnchorOffset = AnchorOffset;
					Size adjust = Size.Empty;

					__anchorOffset = value;

					if(value == -1) __layout.AnchorOffset = 10;

					ResetPath();
					if(Visible)
					{
						Point origAnchorPoint = AnchorPoint;

						if(__targetRect != Rectangle.Empty)
						{
							EnterSafeContext();
							MoveAnchorTo(__targetRect);
							ExitSafeContext();
						}
						else
							MoveAnchorTo(origAnchorPoint);
					}
				}
			}
		}

		/// <summary>
		///		The base anchor offset for the balloon.
		/// </summary>
		/// <remarks>
		///		<p>
		///		The base anchor offset allows the client to indicate the starting
		///		offset when <see cref="AnchorOffset"/> is set to -1.
		///		This must be set prior to the 
		///		the first display of the balloon.  This property is ignored after
		///		the balloon is first displayed.
		///		</p>
		///		<p>
		///		The value cannot be set to -1.
		///		</p>
		/// </remarks>
		[
		Description("Determines the distance along the anchor quadrant edge where " +
					"the anchor tip will be initially positioned when " +
					"AnchorOffset is set to -1."),
		Category("Layout"),
		]
		public int AnchorOffsetBase
		{
			get{return __baseAnchorOffset;}
			set
			{
				if(value < -2) throw(new ArgumentOutOfRangeException("value"));
				if(!IsLoaded || DesignMode)
				{
					if(value == -1) throw(new ArgumentException());

					__layout.AnchorOffset = __baseAnchorOffset = value;
					ResetPath();
				}
			}
		}

		/// <summary>
		///		Retrieves the anchor offset location.
		/// </summary>
		/// <remarks>
		///		This is necessary because the client can set the value of 
		///		<see cref="AnchorQuadrant"/> to <see cref="AnchorQuadrant.Auto"/>
		///		which makes no sense when calculating the layout.  The actual 
		///		quadrant, as used by the layout, is returned.
		/// </remarks>
		private int AnchorOffsetEx
		{
			get
			{
				Rectangle screenBounds			= Screen.GetBounds(this);
				Rectangle balloonBounds			= Bounds;
				int anchorOffset				= AnchorOffset;
				AnchorQuadrant anchorQuadrant	= AnchorQuadrant;

				if(anchorOffset == -1)
					GetAnchorLayout(ref anchorQuadrant, ref anchorOffset, 
						__pivotPoint);
				else if(anchorOffset == -2)
				{
					switch(__layout.AnchorQuadrant)
					{
						case AnchorQuadrant.Left:
						case AnchorQuadrant.Right:
							anchorOffset = (int)(Height-CornerRadius*2)/2;
							break;
						case AnchorQuadrant.Top:
						case AnchorQuadrant.Bottom:
							anchorOffset = (int)(Width-CornerRadius*2)/2;
							break;
					}
				}

				return anchorOffset;
			}
		}

		/// <summary>
		///		Sets or retrieves the distance the anchor tip extends from the 
		///		balloon border.
		/// </summary>
		[
		Description("Determines the distance the anchor tip extends from the " +
					"balloon border."),
		Category("Layout"),
		]
		public int AnchorMargin
		{
			get{return __anchorMargin;}
			set
			{
				if(value < 0) throw(new ArgumentOutOfRangeException("value"));
				if(__anchorMargin != value)
				{
					__anchorMargin = value;
					ResetPath();
				}
			}
		}

		/// <summary>
		///		Used internally to detemine the actual margin the layout will use.
		/// </summary>
		/// <remarks>
		///		<see cref="AnchorMargin"/> is a constrained property.  This means
		///		the client can set the property to any value, but then the layout
		///		is calculated, the actual value used will be dependent on other
		///		factors such as the size of the balloon.
		/// </remarks>
		private int AnchorMarginEx
		{
			get
			{
				// TODO: implement smarter logic.
				return AnchorMargin;
			}
		}

		/// <summary>
		///		Sets or retrieves the radius, in degrees, for the balloon
		///		corners.
		/// </summary>
		[
		Description("Determines the curvature radius for the corners."),
		Category("Layout"),
		]
		public int CornerRadius
		{
			get{return __cornerRadius;}
			set
			{
				if(value == 0) throw(new ArgumentOutOfRangeException("value"));
				if(__cornerRadius != value)
				{
					__cornerRadius = value;

					Point origAnchorPoint = AnchorPoint;
					ResetPath();
					if(Visible)
					{
						if(__targetRect != Rectangle.Empty)
						{
							EnterSafeContext();
							MoveAnchorTo(__targetRect);
							ExitSafeContext();
						}
						else
							MoveAnchorTo(origAnchorPoint);
					}
				}
			}
		}

		/// <summary>
		///		Used internally to detemine the radius the layout will use.
		/// </summary>
		/// <remarks>
		///		<see cref="CornerRadius"/> is a constrained property.  This means
		///		the client can set the property to any value, but then the layout
		///		is calculated, the actual value used will be dependent on other
		///		factors such as the size of the balloon.
		/// </remarks>
		private int CornerRadiusEx
		{
			get
			{
				// TODO: implement smarter logic.
				return CornerRadius;
			}
		}

		/// <summary>
		///		Recalculates the balloon layout based on the balloon's current 
		///		state.
		/// </summary>
		private void RecalcLayout()
		{			
			BalloonLayout layout		= __layout;
			AnchorQuadrant quadrant		= AnchorQuadrantEx;
			int	offset					= AnchorOffsetEx;
			int radius					= (int)CornerRadiusEx;
			int margin					= (int)AnchorMarginEx;

			LayoutInfo info =
				new LayoutInfo(quadrant, offset, radius, margin);

			switch(__layout.AnchorQuadrant)
			{
				case AnchorQuadrant.Top:
				case AnchorQuadrant.Bottom:
					Size = new Size(Size.Width, Size.Height-__layout.AnchorMargin);
					break;
				case AnchorQuadrant.Left:
				case AnchorQuadrant.Right:
					Size = new Size(Size.Width-__layout.AnchorMargin, Size.Height);
					break;
			}

			switch(quadrant)
			{
				case AnchorQuadrant.Top:
				case AnchorQuadrant.Bottom:
					Size = new Size(Size.Width, Size.Height+margin);
					break;
				case AnchorQuadrant.Left:
				case AnchorQuadrant.Right:
					Size = new Size(Size.Width+margin, Size.Height);
					break;
			}


			// Force the layout to be recalculated.
			Rectangle oldBounds = GetBalloonBounds();
			RecalcLayout(layout, info);
			Rectangle newBounds = GetBalloonBounds();

			// Adjust position of all contained controls.  This is because the 
			// when the anchor changes quadrant, the controls should be moved.
			Size offsetBy = new Size(newBounds.X-oldBounds.X, 
				newBounds.Y-oldBounds.Y);
			foreach(Control ctl in Controls)
			{
				Point loc = ctl.Location;
				loc.Offset(offsetBy.Width, offsetBy.Height);
				ctl.Location = loc;
			}
		}

		/// <summary>
		///		Recalculates the balloon layout. 
		/// </summary>
		/// <param name="layout">
		///		The <see cref="BalloonLayout"/> object used to calculate the layout.
		/// </param>
		/// <param name="layoutInfo">
		///		The state that the layout will be set to.
		/// </param>
		private void RecalcLayout(BalloonLayout layout, LayoutInfo layoutInfo)
		{
			layout.AnchorOffset		= layoutInfo.AnchorOffset;
			layout.AnchorQuadrant	= layoutInfo.AnchorQuadrant;
			layout.CornerRadius		= layoutInfo.CornerRadius;
			layout.AnchorMargin		= layoutInfo.AnchorMargin;

			layout.RecalcLayout(new Rectangle(Point.Empty, Size));
		}

		/// <summary>
		///		<see cref="AnchorQuadrant"/> and <see cref="AnchorOffset"/> are
		///		dependent properties when auto-orientation is enabled.  This method
		///		makes sure to take both into account when determining property
		///		layout state.
		/// </summary>
		/// <param name="quadrant">
		///		The <see cref="AnchorQuadrant"/> state for the balloon.
		/// </param>
		/// <param name="offset">
		///		The <see cref="AnchorOffset"/> state for the balloon.
		/// </param>
		/// <param name="pivot">
		///		The pivot by which the balloon will be flipped, if necessary.
		/// </param>
		private void GetAnchorLayout(ref AnchorQuadrant quadrant, 
			ref int offset, Point pivot)
		{
			Point point = pivot;	// TODO: not exactly true; need to fix later.
			Rectangle balloonBounds = Bounds;
			Rectangle screenBounds = Screen.GetBounds(this);
			AnchorQuadrant anchorQuadrant = quadrant;

			if(quadrant == AnchorQuadrant.Auto)
			{
				quadrant = __layout.AnchorQuadrant;

				if(quadrant == AnchorQuadrant.Left &&
					balloonBounds.Right >= screenBounds.Right)
				{quadrant = AnchorQuadrant.Right;}
			
				if(quadrant == AnchorQuadrant.Right && 
					balloonBounds.Left <= screenBounds.Left)
				{quadrant = AnchorQuadrant.Left;}
			
				if(quadrant == AnchorQuadrant.Top &&
					balloonBounds.Bottom >= screenBounds.Bottom)
				{quadrant = AnchorQuadrant.Bottom;}
			
				if(quadrant == AnchorQuadrant.Bottom &&
					balloonBounds.Top <= screenBounds.Top)
				{quadrant = AnchorQuadrant.Top;}
			}

			if(offset == -1)
			{
				offset = __layout.AnchorOffset;

				if((__layout.AnchorQuadrant == AnchorQuadrant.Top ||
					__layout.AnchorQuadrant == AnchorQuadrant.Bottom) &&
					balloonBounds.Right >= screenBounds.Right)
					offset = (int)(Width-(__layout.CornerRadius*2)-10);

				if((__layout.AnchorQuadrant == AnchorQuadrant.Top || 
					__layout.AnchorQuadrant == AnchorQuadrant.Bottom) &&
					balloonBounds.Left <= screenBounds.Left)
					offset = 10;

				if((__layout.AnchorQuadrant == AnchorQuadrant.Left || 
					__layout.AnchorQuadrant == AnchorQuadrant.Right) &&
					balloonBounds.Top <= screenBounds.Top)
					offset = 10;

				if((__layout.AnchorQuadrant == AnchorQuadrant.Left ||
					__layout.AnchorQuadrant == AnchorQuadrant.Right) &&
					balloonBounds.Bottom >= screenBounds.Bottom)
					offset = (int)(Height-(__layout.CornerRadius*2)-10);
			}
		}
		#endregion

		#region Special Effects Management
		/// <summary>
		///		Sets or retrieves the number of milliseconds that the balloon
		///		is visible.  After the time has elapsed, the balloon will hide.
		/// </summary>
		/// <remarks>
		///		To prevent the balloon from automatically closing, specify
		///		the value 0.
		/// </remarks>
		/// <remarks>
		///		When the time elaspes, the balloon is only hidden.  That means, 
		///		any resources the balloon was using are still in memory.  The 
		///		client must close and release the balloon.
		/// </remarks>
		[
		Description("Determines the number of milliseconds the balloon is " +
					"visible; 0 to specify the balloon will remain visible."),
		Category("Behavior"),
		]
		public int Timeout
		{
			get{return __timeout;}
			set{__timeout = value;}
		}

		/// <summary>
		///		Callback for when the timeout countdown as elapsed.
		/// </summary>
		/// <param name="state">
		///		Will always be 'null'.
		/// </param>
		private void TimeoutCallback(object state)
		{
			Hide();
		}
		#endregion

		#region Supporting Control Management
		/// <summary>
		///		
		/// </summary>
		private void ShowCloseBox()
		{
			Rectangle balloonBounds = Bounds;
			CloseButton cb;

			if(__cb == null)
				cb = new CloseButton();
			else
				cb = __cb;

			Size boxSize = cb.Size;
			Point boxLoc = Point.Empty;
			AnchorQuadrant anchorQuadrant = __layout.AnchorQuadrant;
			int anchorMargin = (int)AnchorMargin;

			// Determine correct placement based on anchor location.
			switch(anchorQuadrant)
			{
				case AnchorQuadrant.Top:
					boxLoc =
						new Point(balloonBounds.Width-cb.Width-
						CLOSEBOX_DISTANCE_FROM_EDGE, 
						anchorMargin+CLOSEBOX_DISTANCE_FROM_EDGE);
					break;
				case AnchorQuadrant.Bottom:
					boxLoc =
						new Point(balloonBounds.Width-cb.Width-
						CLOSEBOX_DISTANCE_FROM_EDGE, CLOSEBOX_DISTANCE_FROM_EDGE);
					break;
				case AnchorQuadrant.Left:
					boxLoc = 
						new Point(balloonBounds.Width-cb.Width-
						CLOSEBOX_DISTANCE_FROM_EDGE, CLOSEBOX_DISTANCE_FROM_EDGE);
					break;
				case AnchorQuadrant.Right:
					boxLoc = 
						new Point(balloonBounds.Width-cb.Width-
						CLOSEBOX_DISTANCE_FROM_EDGE-anchorMargin, 
						CLOSEBOX_DISTANCE_FROM_EDGE);
					break;
			}

			cb.Location = boxLoc;
			__cb = cb;

			cb.Click += new EventHandler(CloseBoxHandler);

			if(!Controls.Contains(cb)) Controls.Add(cb);

			cb.BringToFront();
		}

		/// <summary>
		/// 
		/// </summary>
		private void HideCloseBox()
		{
			CloseButton cb = __cb;

			if(__cb == null) return;

			Controls.Remove(cb);
		}

		/// <summary>
		///		Sets or retrieves whether the close box should be displayed.
		/// </summary>
		[
		Description("Determines whether a close box should be displayed."),
		Category("Window Style"),
		]
		public bool CloseBox
		{
			get{return __showCloseBox;}
			set
			{
				if(__showCloseBox != value)
				{
					if(value)
						ShowCloseBox();
					else
						HideCloseBox();

					__showCloseBox = value;
				}
			}
		}

		/// <summary>
		///		Handler for when the user clicks the close box.
		/// </summary>
		/// <param name="sender">
		///		A reference to the close box.
		/// </param>
		/// <param name="e">
		///		The arguments passed to the event handler.
		/// </param>
		private void CloseBoxHandler(object sender, EventArgs e)
		{
			Hide();
		}
		#endregion

		/// <summary>
		///		Sets or retrieves whether the the balloon should be repositioned
		///		when the target area is moved.
		/// </summary>
		/// <remarks>
		///		Only applicable when a control is specified as the target.
		/// </remarks>
		[
		Description("Determines if the balloon follows the movement of the target " +
					"control."),
		Category("Behavior"),
		]
		public bool MoveWithTarget
		{
			get{return __lockAnchor;}
			set
			{
				if(__lockAnchor != value)
				{
					Control targetControl = __targetControl;
					if(targetControl != null)
					{
						if(value)
						{
							RegisterLayoutTracking(targetControl);
							MoveAnchorTo(targetControl);
						}
						else
							UnregisterLayoutTracking(targetControl);
					}

					__lockAnchor = value;
				}
			}
		}

		/// <summary>
		///		Sets or retrieves the control that the balloon will be anchored
		///		to.
		/// </summary>
		[
		Description("Determines the control the balloon will be locked onto."),
		Category("Behavior"),
		]
		public Control TargetControl
		{
			get{return __targetControl;}
			set
			{
				if(__targetControl != value)
				{
					if(MoveWithTarget)
					{
						if(value == null) 
							UnregisterLayoutTracking(__targetControl);
						else
							RegisterLayoutTracking(value);
					}

					__targetControl = value;
				}
			}
		}

		/// <summary>
		///		Sets or retrieves whether the balloon is allowed to 
		///		overlap any part of the target region.
		/// </summary>
		/// <remarks>
		///		<p>
		///		When a balloon is allowed to overlap target areas, the anchor tip
		///		is centered in the visible region of that control or rectangle.  
		///		If overlaping is not allowed, the anchor tip is snapped to the edge
		///		of the control, preserving the anchor quadrant.
		///		</p>
		/// </remarks>
		[
		Description("Determines whether the balloon can overlap the anchored control " +
					"or rectangle, when visible."),
		Category("Behavior"),
		]
		public bool AllowObscure
		{
			get{return __obscure;}
			set
			{
				if(__obscure != value)
				{
					__obscure = value;

					if(Visible)
					{
						// Can only snap when following a target control.
						if(__targetRect != Rectangle.Empty)
						{
							EnterSafeContext();
							MoveAnchorTo(__targetRect);
							ExitSafeContext();
						}
					}
				}
			}
		}

		/// <summary>
		///		Sets or retrieves the <see cref="Point"/> the anchor tip is 
		///		located at.
		/// </summary>
		[
		Description("Determines the position for the anchor tip."),
		Category("Layout"),
		]
		public Point AnchorPoint
		{
			get
			{
				if(DesignMode)
					return __designAnchorPoint;
				else
				{
					Point balloonLoc = Location;
					Point clientAnchorPt = __layout.AnchorPoint;
					return (new Point(balloonLoc.X+clientAnchorPt.X, 
							balloonLoc.Y+clientAnchorPt.Y));
				}
			}
			set
			{
				if(DesignMode)
					__designAnchorPoint = value;
				else
					if(Visible)
						if(AnchorPoint != value) MoveAnchorTo(value);
			}
		}

		#region Internal Storage Fields
		/// <summary>
		///		Stores the quadrant used initially when 
		///		<see cref="AnchorQuadrant"/> is set to
		///		<see cref="AnchorQuadrant.Auto"/>.
		/// </summary>
		private AnchorQuadrant	__baseAnchorQuadrant;

		/// <summary>
		///		Stores a reference to the close box on the balloon.
		/// </summary>
		private CloseButton		__cb;

		/// <summary>
		///		Stores the offset used initially when
		///		<see cref="AnchorOffset"/> is -1.
		/// </summary>
		private int				__baseAnchorOffset;

		/// <summary>
		///		Stores the quadrant the anchor will display in.  Specify
		///		<see cref="AnchorQuadrent.Auto"/> to allow the system to 
		///		determine the appropriate position based on anchor position and
		///		balloon size.
		/// </summary>
		private AnchorQuadrant	__anchorQuadrant;

		/// <summary>
		///		Stores the anchor tip offset from the left or top of the
		///		quadrent.
		/// </summary>
		private int				__anchorOffset;

		/// <summary>
		///		Stores the number of milliseconds before a balloon will be hidden.
		/// </summary>
		private int				__timeout;

		/// <summary>
		///		Stores the distance between the anchor tip and the balloon 
		///		border.
		/// </summary>
		private int				__anchorMargin;

		/// <summary>
		///		Stores the radius for the balloon corners.
		/// </summary>
		private int				__cornerRadius;

		/// <summary>
		///		Stores whether the close box should be displayed.
		/// </summary>
		private bool			__showCloseBox;

		/// <summary>
		/// 
		/// </summary>
		private Threading.Timer	__dismissTimer;

		/// <summary>
		///		
		/// </summary>
		private bool			__obscure;

		/// <summary>
		/// 
		/// </summary>
		private Control			__targetControl;

		/// <summary>
		/// 
		/// </summary>
		private bool			__lockAnchor;
		#endregion











































		private bool IsLoaded
		{
			get{return __isLoaded;}
			set{__isLoaded = value;}
		}

		private bool __isLoaded = false;





































		private Point __pivotPoint = Point.Empty;





































		#region public
		#region Constructors
		/// <summary>
		///		Initialize a new instance.
		/// </summary>
		public BalloonWindow()
		{
			// Layout must be created first.  Other properties depend on it.
			__layout			= new BalloonLayout();

			__anchorQuadrant		= AnchorQuadrant.Auto;
			__baseAnchorQuadrant	= __layout.AnchorQuadrant = AnchorQuadrant.Top;
			__anchorOffset			= -1;
			__baseAnchorOffset		= __layout.AnchorOffset = 10;
			__anchorMargin			= 20;
			__showCloseBox			= true;
			__cornerRadius			= 7;
			__timeout				= 0;
			__lockAnchor			= true;
			__obscure				= true;
			__layoutTrackHandler	= new EventHandler(TrackLayoutHandler);
			__visibleHandler		= new EventHandler(VisibleHandler);
			ShowInTaskbar			= false;


			RecalcLayout();
			//			BackgroundImage = Image.FromFile(@"C:\Documents and Settings\Peter\Desktop\53\images\aboutus.jpg");
			Visible				= false;
			//			TopLevel			= true;	
			//			TopMost				= true;
//			__imgPlacement			= ImagePlacement.ClientRegionOnly;

			Opacity				= 1;
			BackColor			= Color.FromArgb(255, 255, 225);
			BorderColor			= Color.Black;
			Shadow				= true;
			StartPosition = FormStartPosition.Manual;
		}
		#endregion

		#region Methods



		private void EnterSafeContext()
		{
			__safeContext.Push(true);
		}

		private void ExitSafeContext()
		{
			if(__safeContext.Count == 0) throw(new InvalidOperationException());

			__safeContext.Pop();
		}

		private bool InSafeContext
		{
			get{return (__safeContext.Count>0);}
		}

		private Stack __safeContext = new Stack();




		#endregion

		#region Properties




		private Point __anchorPoint = Point.Empty;


























		#endregion
		#endregion

		#region private
		#region Methods


		/// <summary>
		///		Hide the balloon but maintains maintains the event handlers.
		/// </summary>
		/// <remarks>
		///		Used when a balloon is already visible but the target control
		///		is removed from the screen.  For example, if the form is moved
		///		and the control is no longer within the display.  When the form
		///		is moved back, and the control is again visible, the balloon
		///		will appear.
		/// </remarks>
		private void HideAndTrack()
		{
			Visible = false;
		}

		/// <summary>
		///		Removes event handler registration for Move and Resize events.
		/// </summary>
		/// <param name="control">
		///		Target control to watch.
		/// </param>
		private void UnregisterLayoutTracking(Control control)
		{
			if(control == null) throw(new ArgumentNullException("control"));
			Control cur = control;

			// Unregister all necessary event handlers.
			do
			{
				cur.Move -= __layoutTrackHandler;
				cur.Resize -= __layoutTrackHandler;

				if(cur == control.Parent) break;
			} while((cur = control.Parent) != null);

			Form form = control.FindForm();
			form.Move -= __layoutTrackHandler;
			form.Resize -= __layoutTrackHandler;

			handlingLayoutEvents = false;
		}

		/// <summary>
		///		Adds event handler registration for Move and Resize events.
		/// </summary>
		/// <param name="control">
		///		Target control to watch.
		/// </param>
		/// <remarks>
		///		Must watch the events for all container controls as well.
		/// </remarks>
		private void RegisterLayoutTracking(Control control)
		{
			if(control == null) throw(new ArgumentNullException("control"));

			if(!handlingLayoutEvents)
			{
				Control cur = control;

				// Register all necessary event handlers.  If the control gets
				// moved at all, balloon should be notified.
				do
				{
					cur.Move			+= __layoutTrackHandler;
					cur.Resize			+= __layoutTrackHandler;
					cur.VisibleChanged	+= __visibleHandler;

					if(cur == control.Parent) break;
				} while((cur = control.Parent) != null);

				Form form = control.FindForm();
				form.Move += __layoutTrackHandler;
				form.Resize += __layoutTrackHandler;
				form.VisibleChanged += __visibleHandler;

				handlingLayoutEvents = true;
			}
		}

		private bool handlingLayoutEvents = false;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TrackLayoutHandler(object sender, EventArgs e)
		{
			if(sender == null) throw(new ArgumentNullException("sender"));
			if(e == null) throw(new ArgumentNullException("e"));
			Control targetControl = TargetControl;

			if(MoveWithTarget && targetControl != null)
			{
				Rectangle oldCtlRect = __targetControlRect;
				Rectangle newCtlRect = 
					targetControl.RectangleToScreen(targetControl.Bounds);
				Size offset = 
					new Size(newCtlRect.X-oldCtlRect.X, newCtlRect.Y-oldCtlRect.Y);
				Point newLoc = Location + offset;
				
				Location = newLoc;

				__targetControlRect = newCtlRect;
			}

			EnterSafeContext();
			MoveAnchorTo(__targetControl);
			ExitSafeContext();
		}




		private Rectangle __targetControlRect = Rectangle.Empty;


		private void VisibleHandler(object sender, EventArgs e)
		{
			if(sender == null) throw(new ArgumentNullException("sender"));
			if(e == null) throw(new ArgumentNullException("e"));

			bool state = ((Control)sender).Visible;
			if(__targetControl != null)
			{
				if(state)
				{
					Visible = true;
					RegisterLayoutTracking(__targetControl);
				}
				else
				{
					Visible = false;
					UnregisterLayoutTracking(__targetControl);
				}
			}
		}




		#endregion

		#region Fields










		/// <summary>
		///		
		/// </summary>
		#endregion;
		#endregion






















































		private Point __targetPivot = Point.Empty;



		



		protected override void OnClosing(CancelEventArgs e)
		{
			Hide();
			e.Cancel = true;
		}


		Bitmap __bitmap;

		private BalloonLayout __layout;




		protected override void OnLoad(EventArgs e)
		{

			this.IsLoaded = true;
			if(CloseBox) ShowCloseBox();

			Bitmap bmp = __bitmap = new Bitmap(Width, this.Height);
			Graphics grx = Graphics.FromImage(bmp);

			grx.Clear(Color.White);

			//			DrawBalloon(grx);

			//			this.BackgroundImage = bmp;

			base.OnLoad(e);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{



			bool state = Visible;


			if(__targetControl != null && MoveWithTarget)
			{
				if(state)
					RegisterLayoutTracking(__targetControl);
				else
					UnregisterLayoutTracking(__targetControl);
			}

			if(state)
			{
				int timeout = (int)Timeout;

				if(timeout != 0)
				{
					__dismissTimer =
						new Threading.Timer(new TimerCallback(TimeoutCallback),
						null, timeout, 
						Threading.Timeout.Infinite);
				}
			} 
			else 
			{
				__dismissTimer = null;
			}

			base.OnVisibleChanged(e);
		}



		







		private class BalloonLayout
		{
			/// <summary>
			///		Calculates the border path for the balloon and anchor.
			/// </summary>
			/// <param name="rect">
			///		The <see cref="Rectangle"/> constraining the returned
			///		<see cref="GraphicsPath"/>.
			/// </param>
			/// <returns>
			///		A <see cref="GraphicsPath"/> object containing shapes that
			///		define the balloon and anchor.
			/// </returns>
			public GraphicsPath RecalcLayout(Rectangle rect)
			{
				if(rect == Rectangle.Empty) throw(new ArgumentNullException("rect"));

				GraphicsPath gp = new GraphicsPath();

				// Since the user can change the border's width, need to 
				// compensate for this and adjust the boundaries to ensure
				// the entire border gets drawn within the clip region.
				// TODO (implement): currently not being used.
//				Pen borderStyle =
//					(__owner.BorderStyle == null ? new Pen(__owner.BorderColor) : __owner.BorderStyle);
//				int borderAdjustment = (int)(borderStyle.Width/2);

				// Indicates the anchor is being drawn opposite of what the final
				// render will show.  Used to compensate for rotating down or left.
				bool anchorFlipped = false;

				Rectangle clipBounds	= rect;
				Rectangle balloonBounds = clipBounds;
				int cornerRadius = (int)CornerRadius;
				int cornerDiameter = cornerRadius*2;
				Matrix matrix = new Matrix();
				int anchorMargin = (int)AnchorMargin;
				Point[] anchorPoints =
					new Point[] {Point.Empty, Point.Empty, Point.Empty};

				balloonBounds.Size = 
					new Size(balloonBounds.Width-1, 
					balloonBounds.Height-1);

				AnchorQuadrant anchorQuadrant = AnchorQuadrant;

				// Determines which quadrent the anchor will be displayed on and
				// initializes necessary variables for later calculations.
				switch(anchorQuadrant)
				{
					case AnchorQuadrant.Top:	// Anchor appears on the top edge.
						break;
					case AnchorQuadrant.Bottom:	// Anchor appears on the bottom edge.
						// Rotate and position so anchor is pointing down.
						matrix.Translate(balloonBounds.Width, balloonBounds.Height);
						matrix.Rotate(180);

						anchorFlipped = true;
						break;
					case AnchorQuadrant.Left:	// Anchor appears on the left edge.
						balloonBounds.Size =
							new Size(balloonBounds.Height, balloonBounds.Width);

						// Rotate and position so anchor is pointing toward left.
						matrix.Translate(0, balloonBounds.Width);
						matrix.Rotate(-90);

						anchorFlipped = true;
						break;
					case AnchorQuadrant.Right:	// Anchor appears on the right edge.
						balloonBounds.Size =
							new Size(balloonBounds.Height, balloonBounds.Width);

						// Rotate and position so anchor is pointing toward right.
						matrix.Translate(balloonBounds.Height, 0);
						matrix.Rotate(90);
						break;
				}

				// Calculates the coordinates for each of the three points in the
				// anchor.  The shape of the anchor depends on the position along
				// the balloon edge.
				// TODO: calculate the points from angle.
				int anchorOffset = (int)AnchorOffset;
				int balloonEdgeCenter =
					((int)(balloonBounds.Width-(cornerRadius*2))/2)+1;
				int offsetFromEdge = cornerRadius;
				if(anchorFlipped)
					anchorOffset = 
						((int)balloonBounds.Width-(offsetFromEdge*2)-anchorOffset)+1;
				if(anchorOffset < balloonEdgeCenter)
				{
					// Defines the vertisies for an anchor to the left of center.
					anchorPoints[0] =
						new Point(anchorOffset+offsetFromEdge,
						(int)balloonBounds.Y+anchorMargin);
					anchorPoints[1] =
						new Point(anchorOffset+offsetFromEdge,
						(int)balloonBounds.Y);
					anchorPoints[2] =
						new Point(anchorOffset+anchorMargin+offsetFromEdge,
						(int)balloonBounds.Y+anchorMargin);
				} 
				else if(anchorOffset > balloonEdgeCenter)
				{
					// Defines the vertisies for an anchor to the right of center.
					anchorPoints[0] =
						new Point(anchorOffset-anchorMargin+offsetFromEdge,
						(int)balloonBounds.Y+anchorMargin);
					anchorPoints[1] =
						new Point(anchorOffset+offsetFromEdge,
						(int)balloonBounds.Y);
					anchorPoints[2] =
						new Point(anchorOffset+offsetFromEdge,
						(int)balloonBounds.Y+anchorMargin);
				} 
				else 
				{
					// Defines the vertisies for an anchor directly on center.
					anchorPoints[0] =
						new Point(anchorOffset-anchorMargin+offsetFromEdge,
						(int)balloonBounds.Y+anchorMargin);
					anchorPoints[1] =
						new Point(anchorOffset+offsetFromEdge,
						(int)balloonBounds.Y);
					anchorPoints[2] =
						new Point(anchorOffset+anchorMargin+offsetFromEdge,
						(int)balloonBounds.Y+anchorMargin);
				}

				// Build the balloon path.
				gp.AddArc(balloonBounds.Left, balloonBounds.Top+anchorMargin,
					cornerDiameter, cornerDiameter,180, 90);

				gp.AddLine(anchorPoints[0], anchorPoints[1]);
				gp.AddLine(anchorPoints[1], anchorPoints[2]);

				gp.AddArc(balloonBounds.Width-cornerDiameter,
					balloonBounds.Top+anchorMargin,
					cornerDiameter, cornerDiameter, -90, 90);
				gp.AddArc(balloonBounds.Width-cornerDiameter,
					balloonBounds.Bottom-cornerDiameter,
					cornerDiameter, cornerDiameter, 0, 90);
				gp.AddArc(balloonBounds.Left, balloonBounds.Bottom-cornerDiameter,
					cornerDiameter, cornerDiameter, 90, 90);

				gp.CloseFigure();
				
				// Adjust to final position.
				gp.Transform(matrix);

				// Calc and stores the anchor point in screen coordinates.
				//__anchorPoint = __owner.PointToScreen(Point.Round(gp.PathPoints[5]));
				//__anchorPonit = Point.Round(gp.PathPoints[5]);
				__anchorPoint = Point.Round(gp.PathPoints[5]);

				return __path = gp;
			}

			/// <summary>
			///		Retrieves the anchor point in screen coordinates.
			/// </summary>
			public Point AnchorPoint
			{
				get{
					return __anchorPoint;
				}
			}

			/// <summary>
			///		Sets or retreives the quadrent the anchor will be displayed in.
			///		The quadrents can be Top, Bottom, Left, Right, or Auto.
			/// </summary>
			/// <remarks>
			///		When <see cref="AnchorQuadrant.Auto"/> is set, 
			///		<see cref="BalloonWindow"/> will calculate the quadrent to 
			///		ensure the best placement for the anchor.
			/// </remarks>
			public AnchorQuadrant AnchorQuadrant
			{
				get{return __anchorQuadrant;}
				set{__anchorQuadrant = value;}
			}

			public int CornerRadius
			{
				set{__cornerRadius = value;}
				get{return __cornerRadius;}
			}

			public int AnchorMargin
			{
				set{__anchorMargin = value;}
				get{return __anchorMargin;}
			}

			private int __anchorMargin;
			private int __cornerRadius;

			/// <summary>
			///		Sets or retreives a value that indicates the number of pixels,
			///		from the top or left depending on the quadrent, that the anchor
			///		tip will be positioned at.
			/// </summary>
			/// <remarks>
			///		When <see cref="AnchorQuadrent"/> is either 
			///		<see cref="AnchorQuadrent.Top"/> or
			///		<see cref="AnchorQuadrent.Bottom"/>, the offset increases from
			///		left to right.  When <see cref="AnchorQuadrent"/> is either
			///		<see cref="AnchorQuadrent.Left"/> or
			///		<see cref="AnchorQuadrent.Right"/>, the offset increases from
			///		top to bottom.
			/// </remarks>
			public int AnchorOffset
			{
				get{return __anchorOffset;}
				set{__anchorOffset = value;}
			}

















			/// <summary>
			/// 
			/// </summary>
			public GraphicsPath Path
			{
				get{return __path;}
				set
				{
					__path = value;
				}
			}


			private GraphicsPath	__path;
			private Point			__anchorPoint;
			
			private AnchorQuadrant __anchorQuadrant;
			private int __anchorOffset;

			public BalloonLayout()
			{
				__anchorPoint = Point.Empty;
				this.__cornerRadius = 7;
				this.__anchorMargin = 20;
			}
		}



		private void SuspendAnchorTarget()
		{
			__oldAnchorTarget = __targetControl;
			Hide();
			RegisterLayoutTracking(__oldAnchorTarget);
			__trackingSuspended = true;
		}

		private struct LayoutInfo
		{
			public LayoutInfo(AnchorQuadrant quadrant, 
				int offset, int cornerRadius, int margin)
			{
				AnchorQuadrant = quadrant;
				AnchorOffset = offset;
				AnchorMargin = margin;
				CornerRadius = cornerRadius;
			}

			public int AnchorOffset;
			public AnchorQuadrant AnchorQuadrant;
			public int CornerRadius;
			public int AnchorMargin;
		}

		private Point __designAnchorPoint;

		private void ResumeAnchorTarget()
		{
			__oldAnchorTarget = null;
			Show(__oldAnchorTarget);
			__trackingSuspended = false;
		}

		private Control __oldAnchorTarget = null;
		private bool __trackingSuspended = false;

		private bool IsTrackingSuspended
		{
			get{return __trackingSuspended;}
		}

		private void InvalidateLayout()
		{
			Invalidate(true);
			__bitmap = null;
		}



		public new void Hide()
		{
			base.Hide();
			__targetControl = null;
			__targetRect = Rectangle.Empty;
		}





		private EventHandler __layoutTrackHandler = null;
		private EventHandler __visibleHandler = null;


		private Rectangle __targetRect;


		protected Rectangle GetBalloonBounds()
		{
			AnchorQuadrant anchorQuadrant = __layout.AnchorQuadrant;
			Rectangle bounds = Rectangle.Empty;
			int anchorMargin = (int)AnchorMargin;

			switch(anchorQuadrant)
			{
				case AnchorQuadrant.Top:
					bounds = new Rectangle(0, anchorMargin, Width, Height-anchorMargin);
					break;
				case AnchorQuadrant.Bottom:
					bounds = new Rectangle(0, 0, Width, Height-anchorMargin);
					break;
				case AnchorQuadrant.Left:
					bounds = new Rectangle(anchorMargin, 0, Width-anchorMargin, Height);
					break;
				case AnchorQuadrant.Right:
					bounds = new Rectangle(0, 0, Width-anchorMargin, Height);
					break;
			}

			return bounds;
		}

		protected override GraphicsPath PreparePath()
		{
			RecalcLayout();
			return __layout.Path;
		}

		private enum AnchorTargetType
		{
			Point, Rectangle, Control
		}
	}

	/// <summary>
	///		Enumeration the defines the possible location for the balloon's anchor.
	/// </summary>
	public enum AnchorQuadrant
	{
		/// <summary>
		///		Identifies the top-most edge of the balloon.
		/// </summary>
		Top,

		/// <summary>
		///		Identifies the bottom-most edge of the balloon.
		/// </summary>
		Bottom,

		/// <summary>
		///		Identifies the left-most edge of the balloon.
		/// </summary>
		Left,

		/// <summary>
		///		Identifies the right-most edge of the balloon.
		/// </summary>
		Right,

		/// <summary>
		///		Identifies <see cref="BalloonWindow"/> will calculate the "best"
		///		quadrant based on the balloons position to ensure the 
		///		largest visible area is displayed on the screen.
		/// </summary>
		Auto
	}
}