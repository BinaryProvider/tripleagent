using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using SpriteLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing.Drawing2D;

namespace TripleAgent
{
    public class TripleAgentControl : UserControl
    {
        private Image _spriteSheet;

        [Browsable(true)]
        public Image SpriteSheet
        {
            get { return _spriteSheet; }
            set { _spriteSheet = value; }
        }

        private Size _spriteSize;

        [Browsable(true)]
        public Size SpriteSize
        {
            get { return _spriteSize; }
            set {
                Size s = new Size(value.Width, value.Height);
                if (s.Width == 0) s.Width = 1;
                if (s.Height == 0) s.Height = 1;
                _spriteSize = s;
            }
        }

        private int _spriteStartFrame = 1;

        [Browsable(true)]
        public int SpriteStartFrame
        {
            get { return _spriteStartFrame; }
            set {
                if (value > 0) _spriteStartFrame = value; else _spriteStartFrame = 1;
            }
        }

        private Point _spriteStartLocation = new Point(0, 0);

        [Browsable(true)]
        public Point SpriteStartLocation
        {
            get { return _spriteStartLocation; }
            set { _spriteStartLocation = value; }
        }

        private bool _spriteHiddenFromStart = false;

        [Browsable(true)]
        public bool SpriteHiddenFromStart
        {
            get { return _spriteHiddenFromStart; }
            set { _spriteHiddenFromStart = value; }
        }

        private List<SpriteAnimation> _spriteAnimations;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<SpriteAnimation> SpriteAnimations
        {
            get { return _spriteAnimations; }
            set {
                _spriteAnimations = value;
                for (int i = 0; i < _spriteAnimations.Count; i++)
                {
                    _spriteAnimations[i].Index = i;
                }
            }
        }

        private PictureBox _canvas = new PictureBox();
        private SpriteController _spriteController;
        private Sprite _sprite;
        private Queue<int> _spriteAnimationQueue = new Queue<int>();

        public TripleAgentControl(Image spriteSheet = null, Size? spriteSize = null, int? spriteStartFrame = null, Point? spriteStartLocation = null, XmlDocument animationData = null)
        {
            if (spriteSheet != null)
                SpriteSheet = spriteSheet;

            if (spriteSize != null)
                SpriteSize = (Size)spriteSize;

            if (spriteStartFrame != null)
                SpriteStartFrame = (int)spriteStartFrame;

            if (spriteStartLocation != null)
                SpriteStartLocation = (Point)spriteStartLocation;

            if (animationData != null)
                LoadAnimationData(animationData);

            if (SpriteAnimations == null)
                SpriteAnimations = new List<SpriteAnimation>();

            this.BackColor = Color.Transparent;
            this.Load += TripleAgentControl_Load;
        }

        private void TripleAgentControl_Load(object sender, System.EventArgs e)
        {
            if (!this.DesignMode)
            {
                if (_spriteSheet != null)
                {
                    CreateCanvas();

                    _spriteController = new SpriteController(_canvas);
                    _sprite = new Sprite(Utils.SpriteSheetFrameIndexToPoint(_spriteSheet, _spriteSize, _spriteStartFrame), _spriteController, _spriteSheet, _spriteSize.Width, _spriteSize.Height, 100, 1);
                    _sprite.SpriteAnimationComplete += SpriteAnimationComplete;
                    _sprite.SpriteChangesAnimationFrames += SpriteChangesAnimationFrames;

                    AddAnimations();

                    if (!_spriteHiddenFromStart)
                    {
                        _sprite.PutBaseImageLocation(_spriteStartLocation);
                    }
                }
            }
        }

        public void HideTip()
        {
            _canvas.Controls.RemoveByKey("TooltipLabel");
            _canvas.Controls.RemoveByKey("TooltipArrow");
        }

        public void ToggleSpritevisibility(bool show)
        {
            if (!show)
            {
                _sprite.HideSprite();
            } else
            {
                _sprite.UnhideSprite();
            }
        }

        /// <summary>
        /// Start an animation and show a label in a tooltip balloon format.
        /// </summary>
        /// <param name="animation">The animation that should be played when the tip is shown.</param>
        /// <param name="labelText">The text to be shown.</param>
        /// <param name="labelAlignment">Where to place the tooltip label.</param>
        /// <param name="labelWidth">Desired width of the label.</param>
        /// <param name="labelMargin">Desired margin around the label.</param>
        /// <param name="labelPadding">Desired padding within the label.</param>
        /// <param name="labelBackgroundColor">Desired background color of the label.</param>
        /// <param name="labelTextColor">Desired text color of the label.</param>
        /// <param name="labelBorder">If the label should have a border or not.</param>
        /// <param name="labelFont">Desired font of the label.</param>
        /// <param name="arrowSize">Desired size of the tooltip arrow.</param>
        /// <param name="arrowCornerOffsetPercentage">Desired distance of the tooltip arrow from the top or bottom corner of the tooltip label. Specified in percentage of the entire tooltip label height.</param>
        /// <param name="spriteDelay">Desired delay until the sprite is shown (in milliseconds).</param>
        /// <param name="labelDelay">Desired delay until the tooltip label is shown (in milliseconds).</param>
        /// <param name="hideAfter">Desired time until the tooltip label is hidden (in milliseconds). Is persistant if undefined.</param>
        /// <param name="altSpriteLocation">Desired location of the sprite, if the original sprite location is not desired.</param>
        public void ShowTip(SpriteAnimation animation, string labelText, ContentAlignment? labelAlignment = null, int? labelWidth = null, Padding? labelMargin = null, int? labelPadding = null, Color? labelBackgroundColor = null, Color? labelTextColor = null, bool? labelBorder = null, Font labelFont = null, int? arrowSize = null, int? arrowCornerOffsetPercentage = null, int? spriteDelay = null, int? labelDelay = null, int? hideAfter = null, Point? altSpriteLocation = null)
        {
            HideTip();

            Point spriteLocation = SpriteStartLocation;

            if (altSpriteLocation != null)
                spriteLocation = (Point)altSpriteLocation;

            Color tooltipLabelTextColor = Color.FromArgb(0, 0, 0);
            Color tooltipLabelBackgroundColor = Color.FromArgb(255, 255, 203);
            Color tooltipBorderColor = Color.FromArgb(100, 100, 100);

            if (labelBackgroundColor != null)
                tooltipLabelBackgroundColor = (Color)labelBackgroundColor;

            if (labelTextColor != null)
                tooltipLabelTextColor = (Color)labelTextColor;

            int tooltipBorderWidth = 1;
            bool tooltipBorder = true;

            if (labelBorder != null)
                tooltipBorder = (bool)labelBorder;

            if (!tooltipBorder)
            {
                tooltipBorderWidth = 0;
                tooltipBorderColor = tooltipLabelBackgroundColor;
            }

            int tooltipArrowHeight = 20;

            if (arrowSize != null)
                tooltipArrowHeight = ((int)arrowSize > 1) ? (int)arrowSize : 2;

            int tooltipArrowWidth = tooltipArrowHeight / 2;

            Point arrowOffset = new Point(tooltipBorderWidth, 0);

            Label tooltipLabel = new Label();
            tooltipLabel.Name = "TooltipLabel";
            tooltipLabel.Text = labelText;

            Font tooltipFont = new Font(tooltipLabel.Font, tooltipLabel.Font.Style);

            if (labelFont != null)
                tooltipFont = labelFont;

            tooltipLabel.Font = tooltipFont;

            tooltipLabel.ForeColor = tooltipLabelTextColor;

            Padding tooltipLabelMargin = new Padding(0);

            if (labelMargin != null)
                tooltipLabelMargin = (Padding)labelMargin;

            int tooltipPadding = 10;
            if (labelPadding != null)
                tooltipPadding = (int)labelPadding;

            tooltipLabel.Padding = new Padding(tooltipPadding);
 
            if(tooltipBorder)
                tooltipLabel.BorderStyle = BorderStyle.FixedSingle;

            tooltipLabel.BackColor = tooltipLabelBackgroundColor;

            ContentAlignment tooltipLabelAlignment = ContentAlignment.MiddleRight;
            if (labelAlignment != null)
                tooltipLabelAlignment = (ContentAlignment)labelAlignment;

            tooltipLabel.Size = new Size(0, 0);
            tooltipLabel.MaximumSize = new Size(0, 0);
            tooltipLabel.AutoSize = true;

            if (labelWidth != null)
            {
                tooltipLabel.Size = new Size((int)labelWidth, 0);
                tooltipLabel.MaximumSize = new Size((int)labelWidth, 0);
            }
            else
            {
                int maxWidth = 0;
                int maxWidthMargin = 20;
                switch(tooltipLabelAlignment)
                {
                    case ContentAlignment.TopRight:
                    case ContentAlignment.MiddleRight:
                    case ContentAlignment.BottomRight:
                        maxWidth = ((this.Width - spriteLocation.X) - (SpriteSize.Width + tooltipLabelMargin.Left + tooltipLabelMargin.Right + Math.Max(tooltipArrowHeight, tooltipArrowWidth))) - maxWidthMargin;
                        break;

                    case ContentAlignment.TopLeft:
                    case ContentAlignment.MiddleLeft:
                    case ContentAlignment.BottomLeft:
                        maxWidth = spriteLocation.X - maxWidthMargin;
                        break;

                    default:
                        maxWidth = ((this.Width + SpriteSize.Width) - spriteLocation.X) - (tooltipLabelMargin.Left + tooltipLabelMargin.Right);
                        break;
                }
                tooltipLabel.Size = new Size(maxWidth, 0);
                tooltipLabel.MaximumSize = new Size(maxWidth, 0);
            }

            Point tooltipLabelLocation = spriteLocation;

            Size tooltipLabelSize = Size.Round(tooltipLabel.CreateGraphics().MeasureString(tooltipLabel.Text, tooltipLabel.Font, new SizeF(tooltipLabel.Size.Width - (tooltipPadding * 2), tooltipLabel.Size.Height - (tooltipPadding * 2))));

            if (tooltipLabelSize.Width > 0 && tooltipLabelSize.Width < tooltipLabel.MaximumSize.Width)
                tooltipLabelSize.Width = tooltipLabel.Size.Width;

            if (tooltipLabelSize.Height > 0 && tooltipLabelSize.Height < tooltipLabel.MaximumSize.Height)
                tooltipLabelSize.Height = tooltipLabel.Size.Height;

            switch (tooltipLabelAlignment)
            {
                case ContentAlignment.TopLeft:
                    tooltipLabelLocation.Offset(-tooltipLabelSize.Width, -(tooltipLabelSize.Height + (tooltipPadding * 2)));
                    break;

                case ContentAlignment.TopCenter:
                    tooltipLabelLocation.Offset(-((tooltipLabelSize.Width / 2) - (SpriteSize.Width / 2)), -(tooltipLabelSize.Height + (tooltipPadding * 2)));
                    break;

                case ContentAlignment.TopRight:
                    tooltipLabelLocation.Offset(SpriteSize.Width, -(tooltipLabelSize.Height + (tooltipPadding * 2)));
                    break;

                case ContentAlignment.MiddleLeft:
                    tooltipLabelLocation.Offset(-tooltipLabelSize.Width, (SpriteSize.Height / 2) - (tooltipLabelSize.Height / 2) - tooltipPadding);
                    break;

                case ContentAlignment.MiddleCenter:
                    tooltipLabelLocation.Offset(-((tooltipLabelSize.Width / 2) - (SpriteSize.Width / 2)), (SpriteSize.Height / 2) - (tooltipLabelSize.Height / 2) - tooltipPadding);
                    break;

                case ContentAlignment.MiddleRight:
                    tooltipLabelLocation.Offset(SpriteSize.Width, (SpriteSize.Height / 2) - (tooltipLabelSize.Height / 2) - tooltipPadding);
                    break;

                case ContentAlignment.BottomLeft:
                    tooltipLabelLocation.Offset(-tooltipLabelSize.Width, (SpriteSize.Height));
                    break;

                case ContentAlignment.BottomCenter:
                    tooltipLabelLocation.Offset(-((tooltipLabelSize.Width / 2) - (SpriteSize.Width / 2)), (SpriteSize.Height));
                    break;

                case ContentAlignment.BottomRight:
                    tooltipLabelLocation.Offset(SpriteSize.Width, (SpriteSize.Height));
                    break;

                default:
                    break;
            }

            int xMargin = (tooltipLabelMargin.Left == tooltipLabelMargin.Right) ? tooltipLabelMargin.Left : (tooltipLabelMargin.Left - tooltipLabelMargin.Right);

            int yMargin = (tooltipLabelMargin.Top == tooltipLabelMargin.Bottom) ? tooltipLabelMargin.Top : (tooltipLabelMargin.Top - tooltipLabelMargin.Bottom);

            tooltipLabelLocation.Offset(xMargin, yMargin);

            tooltipLabel.Location = tooltipLabelLocation;
            tooltipLabel.Hide();
            tooltipLabel.SendToBack();
            _canvas.Controls.Add(tooltipLabel);

            PictureBox tooltipArrow = new PictureBox();
            tooltipArrow.Name = "TooltipArrow";

            Size tooltipArrowSize = new Size(tooltipArrowHeight, tooltipArrowWidth);
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(new Point[] { new Point(0, tooltipArrowSize.Height), new Point(tooltipArrowSize.Width, tooltipArrowSize.Height), new Point(tooltipArrowSize.Width / 2, 0) });

            SolidBrush arrowColorBrush = new SolidBrush(tooltipLabelBackgroundColor);
            Pen arrowBorderPen = new Pen(new SolidBrush(tooltipBorderColor), tooltipBorderWidth);

            Bitmap arrowImage = new Bitmap(tooltipArrowSize.Width, tooltipArrowSize.Height);
            using (Graphics g = Graphics.FromImage(arrowImage))
            {
                g.FillPolygon(arrowColorBrush, path.PathPoints);
                g.DrawPolygon(arrowBorderPen, path.PathPoints);
            }

            Point tooltipArrowLocation = tooltipLabel.Location;

            int arrowCornerMarginPercentage = 20;

            if (arrowCornerOffsetPercentage != null)
                arrowCornerMarginPercentage = (int)arrowCornerOffsetPercentage;

            int arrowCornerMargin = (int)(tooltipLabel.Height * (arrowCornerMarginPercentage / 100.0));

            switch (tooltipLabelAlignment)
            {
                case ContentAlignment.TopLeft:
                    arrowImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    tooltipArrow.Size = arrowImage.Size;
                    tooltipLabel.Left -= tooltipArrow.Size.Width;
                    tooltipArrowLocation.Offset((tooltipLabel.Width - tooltipBorderWidth) - tooltipArrow.Size.Width, (tooltipLabel.Height - tooltipArrow.Size.Height) - arrowCornerMargin);
                    break;

                case ContentAlignment.MiddleLeft:
                    arrowImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    tooltipArrow.Size = arrowImage.Size;
                    tooltipLabel.Left -= tooltipArrow.Size.Width;
                    tooltipArrowLocation.Offset((tooltipLabel.Width - tooltipBorderWidth) - tooltipArrow.Size.Width, (tooltipLabel.Height / 2) - (tooltipArrow.Size.Height / 2));
                    break;

                case ContentAlignment.BottomLeft:
                    arrowImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    tooltipArrow.Size = arrowImage.Size;
                    tooltipLabel.Left -= tooltipArrow.Size.Width;
                    tooltipArrowLocation.Offset((tooltipLabel.Width - tooltipBorderWidth) - tooltipArrow.Size.Width, arrowCornerMargin);
                    break;

                case ContentAlignment.TopCenter:
                    arrowImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    tooltipArrow.Size = arrowImage.Size;
                    tooltipArrowLocation.Offset((tooltipLabel.Width / 2) - (tooltipArrow.Size.Width / 2), (tooltipLabel.Height - tooltipBorderWidth));
                    break;

                case ContentAlignment.MiddleCenter:
                    tooltipArrow.Size = arrowImage.Size;
                    tooltipArrowLocation.Offset((tooltipLabel.Width / 2) - (tooltipArrow.Size.Width / 2), -(tooltipArrow.Size.Height - tooltipBorderWidth));
                    break;

                case ContentAlignment.BottomCenter:
                    tooltipArrow.Size = arrowImage.Size;
                    tooltipArrowLocation.Offset((tooltipLabel.Width / 2) - (tooltipArrow.Size.Width / 2), -(tooltipArrow.Size.Height - tooltipBorderWidth));
                    break;

                case ContentAlignment.TopRight:
                    arrowImage.RotateFlip(RotateFlipType.Rotate90FlipX);
                    tooltipArrow.Size = arrowImage.Size;
                    tooltipLabel.Left += tooltipArrow.Size.Width;
                    tooltipArrowLocation.Offset(tooltipBorderWidth, (tooltipLabel.Height - tooltipArrow.Size.Height) - arrowCornerMargin);
                    break;

                case ContentAlignment.MiddleRight:
                    arrowImage.RotateFlip(RotateFlipType.Rotate90FlipX);
                    tooltipArrow.Size = arrowImage.Size;
                    tooltipLabel.Left += tooltipArrow.Size.Width;
                    tooltipArrowLocation.Offset(tooltipBorderWidth, (tooltipLabel.Height / 2) - (tooltipArrow.Size.Height / 2));
                    break;

                case ContentAlignment.BottomRight:
                    arrowImage.RotateFlip(RotateFlipType.Rotate90FlipX);
                    tooltipArrow.Size = arrowImage.Size;
                    tooltipLabel.Left += tooltipArrow.Size.Width;
                    tooltipArrowLocation.Offset(tooltipBorderWidth, arrowCornerMargin);
                    break;

                default:
                    break;
            }

            tooltipArrow.BackgroundImage = arrowImage;
            tooltipArrow.BackColor = Color.Transparent;
            tooltipArrow.Location = tooltipArrowLocation;
            tooltipArrow.Hide();

            _canvas.Controls.Add(tooltipArrow);

            int tooltipSpriteDelay = 0;
            if (spriteDelay != null)
                tooltipSpriteDelay = (int)spriteDelay;

            int tooltipLabelDelay = 0;
            if (labelDelay != null)
                tooltipLabelDelay = (int)labelDelay;

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(tooltipSpriteDelay);
                this.Invoke(new Action(() => {
                    _sprite.PutBaseImageLocation(spriteLocation);
                    PlayAnimation(animation);
                }));
            });

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(tooltipLabelDelay);
                this.Invoke(new Action(() => {
                    tooltipLabel.Show();
                    tooltipArrow.Show();
                    tooltipLabel.BringToFront();
                    tooltipArrow.BringToFront();
                }));
            });

            if(hideAfter != null)
            {
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep((int)hideAfter + Math.Max(tooltipSpriteDelay, tooltipLabelDelay));
                    this.Invoke(new Action(() => {
                        HideTip();
                    }));
                });
            }
        }

        private void CreateCanvas()
        {
            _canvas.BackColor = this.BackColor;
            _canvas.Location = new Point(0, 0);
            _canvas.Size = this.Size;
            _canvas.BackgroundImageLayout = ImageLayout.Stretch;
            this.Controls.Add(_canvas);
        }

        public void LoadAnimationData(XmlDocument doc)
        {
            SpriteAnimations = SpriteAnimation.ParseAnimationData(doc, _spriteSheet, _spriteSize);
        }

        private void AddAnimations()
        {
            foreach (SpriteAnimation animation in _spriteAnimations)
            {
                int frameStart = (animation.FrameStart > 0) ? animation.FrameStart : 1;
                int frameEnd = (animation.FrameEnd > 0) ? animation.FrameEnd : 1;
                int frameTotal = (frameEnd - frameStart);
                int frameDuration = animation.FrameDuration;
                Point startLoc = Utils.SpriteSheetFrameIndexToPoint(_spriteSheet, _spriteSize, frameStart);
                _sprite.AddAnimation(startLoc, _spriteSheet, _spriteSize.Width, _spriteSize.Height, frameDuration, frameTotal);
            }
        }

        public void PlayAnimation(SpriteAnimation animation, int? loopCount = null, int? loopDelay = null)
        {
            if (_sprite != null)
            {
                //_sprite.HideSprite();

                // Start animation
                if(animation == null)
                {
                    animation = new SpriteAnimation();
                    animation.Index = -1;
                    animation.FrameStart = _spriteStartFrame;
                    animation.FrameEnd = _spriteStartFrame;
                }

                if (loopCount != null)
                    animation.LoopCount = (int)loopCount;

                if (loopDelay != null)
                    animation.LoopDelay = (int)loopDelay;

                if (animation.LoopCount == -1)
                    animation.LoopCount = Int32.MaxValue;

                if (animation.LoopCount <= 1)
                {
                    _sprite.AnimateOnce(animation.Index + 1);
                }
                else
                {
                    if (animation.LoopDelay > 0)
                    {
                        _sprite.ChangeFrameAnimationSpeed(animation.Index + 1, (animation.FrameEnd - animation.FrameStart) - 1, animation.LoopDelay);
                    }
                    _sprite.AnimateJustAFewTimes(animation.Index + 1, animation.LoopCount);
                }

                if (animation.Index > -1)
                {
                    if (_spriteAnimations[animation.Index].SubsequentAnimationIndex != null)
                    {
                        _spriteAnimationQueue.Enqueue((int)_spriteAnimations[animation.Index].SubsequentAnimationIndex);
                    }
                }

                //_sprite.UnPause();
            }
        }

        private void SpriteAnimationComplete(object sender, SpriteEventArgs e)
        {
            //_sprite.Pause();

            if (_spriteAnimationQueue.Count != 0)
            {
                PlayAnimation(SpriteAnimations[_spriteAnimationQueue.Dequeue()]);
            }
            else
            {
                PlayAnimation(null);
            }
        }

        private void SpriteChangesAnimationFrames(object sender, SpriteEventArgs e)
        {
  
        }
  
        private Point AlignmentToPoint(ContentAlignment alignment)
        {
            Point p = new Point(0, 0);

            Size halfWinSize = new Size(this.Width / 2, this.Height / 2);
            Size halfSpriteSize = new Size(_spriteSize.Width / 2, _spriteSize.Height / 2);

            switch (alignment)
            {
                case ContentAlignment.TopLeft:
                    p = new Point(0, 0);
                    break;

                case ContentAlignment.TopCenter:
                    p = new Point(halfWinSize.Width - halfSpriteSize.Width, 0);
                    break;

                case ContentAlignment.TopRight:
                    p = new Point(this.Width - _spriteSize.Width, halfWinSize.Height - halfSpriteSize.Height);
                    break;

                case ContentAlignment.MiddleLeft:
                    p = new Point(0, halfWinSize.Height - halfSpriteSize.Height);
                    break;

                case ContentAlignment.MiddleCenter:
                    p = new Point(halfWinSize.Width - halfSpriteSize.Width, halfWinSize.Height - halfSpriteSize.Height);
                    break;

                case ContentAlignment.MiddleRight:
                    p = new Point(this.Width - _spriteSize.Width, halfWinSize.Height - halfSpriteSize.Height);
                    break;

                case ContentAlignment.BottomLeft:
                    p = new Point(0, this.Height - _spriteSize.Height);
                    break;

                case ContentAlignment.BottomCenter:
                    p = new Point(halfWinSize.Width - halfSpriteSize.Width, this.Height - _spriteSize.Height);
                    break;

                case ContentAlignment.BottomRight:
                    p = new Point(this.Width - _spriteSize.Width, this.Height - _spriteSize.Height);
                    break;

                default:
                    break;
            }

            return p;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // TripleAgent
            // 
            this.Name = "TripleAgent";
            this.Size = new System.Drawing.Size(267, 233);
            this.ResumeLayout(false);

        }
    }
}
