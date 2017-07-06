using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Rilling.Common.UI.Forms
{
	[Flags()]
	public enum MessageBalloonOptions
	{
		/// <summary>Expands horizontal dimension allowing message to fit.</summary>
		HorizontalAutoScale	= 1,

		/// <summary>Expands vertical dimension allowing message to fit.</summary>
		VerticalAutoScale	= 2,

		/// <summary>Displays the shadow.</summary>
		ShowShadow			= 4,

		/// <summary>Anchor is centered on target control.</summary>
		AllowObscure		= 8,

		/// <summary>Dismisses the balloon after 10 seconds.</summary>
		Timeout				= 16,

		/// <summary></summary>
		All					= HorizontalAutoScale | VerticalAutoScale | ShowShadow |
							  Timeout | AllowObscure,

		/// <summary></summary>
		None				= 0,

		/// <summary></summary>
		Default				= All - Timeout
	}

	public sealed class MessageBalloon : BalloonWindow
	{
		/// <summary>
		///		Displays a message balloon.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="control"></param>
		public static void Show(string message, Control control)
		{
			Show(message, String.Empty, null, MessageBalloonOptions.Default, control);
		}

		/// <summary>
		///		Displays a message balloon.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="point"></param>
		public static void Show(string message, Point point)
		{
			Show(message, String.Empty, null, MessageBalloonOptions.Default, point);
		}

		/// <summary>
		///		Displays a message balloon.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="title"></param>
		/// <param name="control"></param>
		public static void Show(string message, string title, Control control)
		{
			Show(message, title, null, MessageBalloonOptions.Default, control);
		}

		/// <summary>
		///		Displays a message balloon.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="title"></param>
		/// <param name="point"></param>
		public static void Show(string message, string title, Point point)
		{
			Show(message, title, null, MessageBalloonOptions.Default, point);
		}

		/// <summary>
		///		Displays a message balloon.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="title"></param>
		/// <param name="icon"></param>
		/// <param name="control"></param>
		public static void Show(string message, string title, Icon icon, Control control)
		{
			Show(message, title, icon, MessageBalloonOptions.Default, control);
		}

		/// <summary>
		///		Displays a message balloon.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="title"></param>
		/// <param name="icon"></param>
		/// <param name="point"></param>
		public static void Show(string message, string title, Icon icon, Point point)
		{
			Show(message, title, icon, MessageBalloonOptions.Default, point);
		}

		/// <summary>
		///		Displays a message balloon.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="title"></param>
		/// <param name="icon"></param>
		/// <param name="options"></param>
		/// <param name="control"></param>
		public static void Show(string message, string title, Icon icon, MessageBalloonOptions options, Control control)
		{
			MessageBalloon bw = CreateBalloonContext(message, title, icon, options);
			bw.Show(control);
		}

		/// <summary>
		///		Displays a message balloon.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="title"></param>
		/// <param name="icon"></param>
		/// <param name="options"></param>
		/// <param name="point"></param>
		public static void Show(string message, string title, Icon icon, MessageBalloonOptions options, Point point)
		{
			MessageBalloon bw = CreateBalloonContext(message, title, icon, options);
			bw.Show(point);
		}

		private static MessageBalloon CreateBalloonContext(string message, string title, Icon icon, MessageBalloonOptions options)
		{
			MessageBalloon bw = new MessageBalloon();

			bw.Message		= message;
			bw.Title		= title;
			bw.Icon			= icon;

			bw.HorizontalScale =
				(options & MessageBalloonOptions.HorizontalAutoScale) == 
				MessageBalloonOptions.HorizontalAutoScale;
			bw.VerticalScale = 
				(options & MessageBalloonOptions.VerticalAutoScale) ==
				MessageBalloonOptions.VerticalAutoScale;
			bw.Shadow = 
				(options & MessageBalloonOptions.ShowShadow) == 
				MessageBalloonOptions.ShowShadow;
			bw.AllowObscure = 
				(options & MessageBalloonOptions.AllowObscure) ==
				MessageBalloonOptions.AllowObscure;
			bw.Timeout = ((options & MessageBalloonOptions.Timeout) ==
				MessageBalloonOptions.Timeout ? 10000 : 0);

			return bw;
		}













		protected override void OnLoad(EventArgs e)
		{
			// Auto-expand the balloon size, when required.
			Size size = GetScaleFactor(HorizontalScale, VerticalScale);
				 size.Width += (int)CornerRadius*2+25;
				 size.Height += (int)AnchorMargin*2+20;

			Size = size;

			base.OnLoad(e);
		}

		private Size GetScaleFactor(bool hScale, bool vScale)
		{
			Rectangle screenBounds = Screen.GetBounds(this);
			Size maxSize = new Size((int)(0.33333*screenBounds.Width), 
									(int)(0.33333*screenBounds.Height));
			Size minSize = new Size(100, 100);
			Size size = Size.Empty;
			System.Drawing.Font font = Font;
			Graphics grx = CreateGraphics();

			if(HorizontalScale)
				size.Width = grx.MeasureString(Message, font).ToSize().Width;

			if(size.Width > maxSize.Width)
				size.Width = maxSize.Width;
			else if(size.Width < minSize.Width)
				size.Width = minSize.Width;

			SizeF t = new SizeF(size.Width, 1000);
//			StringFormat sf = new StringFormat();
//			CharacterRange[] crs = {new CharacterRange(0, Message.Length)};
//			RectangleF rect = new RectangleF(0,0,size.Width, 1000);
//			Region[] regions = new Region[1];
//			sf.SetMeasurableCharacterRanges(crs);
//			regions = grx.MeasureCharacterRanges(Message, font, rect, sf);
//			rect = regions[0].GetBounds(grx);
//
//			size.Height = (int)rect.Height;


			if(VerticalScale)
				size.Height =
					grx.MeasureString(Message, font,t).ToSize().Height;

			if(size.Height > maxSize.Height)
				size.Height = maxSize.Height;
			else if(size.Height < minSize.Height)
				size.Height = minSize.Height;

			//size = new Size(200, 195);

			return size;
		}

//		System.Drawing.StringFormat     format  = new System.Drawing.StringFormat ();
//		System.Drawing.RectangleF       rect    = new System.Drawing.RectangleF(0, 0,
//			1000, 1000);
//		System.Drawing.CharacterRange[] ranges  = { new System.Drawing.CharacterRange(0, 
//													  text.Length) };
//		System.Drawing.Region[]         regions = new System.Drawing.Region[1];
//
//		format.SetMeasurableCharacterRanges (ranges);
//
//		regions = graphics.MeasureCharacterRanges (text, font, rect, format);
//		rect    = regions[0].GetBounds (graphics);
//
//		return (int)(rect.Right + 1.0f);


		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Graphics grx = e.Graphics;
			Rectangle balloonBounds = GetBalloonBounds();
			Icon icon = Icon;
			string title = Title;
			string message = Message;

			// Define the rectangle for each of the message balloon elements.
			Point loc = balloonBounds.Location;
			Rectangle iconBounds = Rectangle.Empty;
			Rectangle titleBounds = Rectangle.Empty;
			Rectangle msgBounds = Rectangle.Empty;

			loc.X += 12; loc.Y += 7;
			if(icon != null) iconBounds = new Rectangle(loc, new Size(16, 16));
			loc.X += 22; loc.Y += 0;
			if(title != String.Empty) titleBounds = new Rectangle(new Point(loc.X, loc.Y+2), new Size(balloonBounds.Width-60, 15));
			loc.X -=22; loc.Y +=22;
			if(message != String.Empty) msgBounds = new Rectangle(loc, new Size(balloonBounds.Width-(loc.X*3)-2, balloonBounds.Height-loc.Y-6));

#if DEBUG
			// Show bounds for debugging.
			grx.DrawRectangle(Pens.Red, iconBounds);
			grx.DrawRectangle(Pens.Red, titleBounds);
			grx.DrawRectangle(Pens.Red, msgBounds);
#endif

			// Draw the message balloon elements.
			grx.SmoothingMode = SmoothingMode.AntiAlias;
			StringFormat sf = new StringFormat();
						// sf.FormatFlags = StringFormatFlags.NoWrap;
						 sf.Trimming = StringTrimming.EllipsisCharacter;
			if(icon != null) 
				grx.DrawIcon(icon, iconBounds);
			if(title != null) 
				grx.DrawString(title, new Font(Font, FontStyle.Bold), Brushes.Black, titleBounds, sf);
			if(message != null) 
				grx.DrawString(message, Font, Brushes.Black, msgBounds, sf);
		}























		private void InitializeComponent()
		{
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(112, 120);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			// 
			// MessageBalloon
			// 
			this.AnchorPoint = new System.Drawing.Point(30, 13);
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(300, 300);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.pictureBox1});
			this.Name = "MessageBalloon";
			this.ResumeLayout(false);

		}

		private MessageBalloon()
		{
			__message	= String.Empty;
			__title		= String.Empty;
			__icon		= null;
			__hScale	= false;
			__vScale	= false;

			InitializeComponent();
		}

		public string Message
		{
			get{return __message;}
			set{__message = value;}
		}

		// TODO (suggestion):  add support for a click callback.


		public string Title
		{
			get{return __title;}
			set{__title = value;}
		}

		public bool HorizontalScale
		{
			get{return __hScale;}
			set{__hScale = value;}
		}

		public bool VerticalScale
		{
			get{return __vScale;}
			set{__vScale = value;}
		}

		private string __message;
		private string __title;
		private Icon __icon;
		private bool __hScale;
		private System.Windows.Forms.PictureBox pictureBox1;
		private bool __vScale;
	




//		public static void Show(string message, NotifyIcon notifyIcon)
//		{
//			Show(message, String.Empty, null, MessageBalloonOptions.Default, notifyIcon);
//		}
//

//		public static void Show(string message, string title, NotifyIcon notifyIcon)
//		{
//			Show(message, title, null, MessageBalloonOptions.Default, notifyIcon);
//		}

//		public static void Show(string message, string title, Icon icon, NotifyIcon notifyIcon)
//		{
//			Show(message, title, icon, MessageBalloonOptions.Default, notifyIcon);
//		}

//		public static void Show(string message, string title, Icon icon, MessageBalloonOptions options, NotifyIcon notifyIcon)
//		{
////			BalloonWindow bw = CreateBalloonContext(message, title, icon, options);
//			bw.Show(notifyIcon);
//		}







		//		private static Size GetConstrainedBounds(bool horizontal, bool vertical)
		//		{
		//			Rectangle bounds = Rectangle.Empty;
		//
		//			if(horizontal)
		//			{
		//			}
		//
		//			if(vertical)
		//			{
		//			}
		//
		//			return bounds;
		//		}

	}
}