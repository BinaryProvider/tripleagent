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

namespace TripleAgent
{
    public class TripleAgent : UserControl
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

        public TripleAgent()
        {
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
            this.Controls.RemoveByKey("TooltipLabel");
            this.Controls.RemoveByKey("TooltipArrow");
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

        public void ShowTip(SpriteAnimation animation, Point spriteLocation, Point labelLocation, Size labelSize, string labelText, int spriteDelay = 0, int labelDelay = 0)
        {
            Point arrowOffset = new Point(1, 0);

            Label label = new Label();
            label.Name = "TooltipLabel";
            this.Controls.Add(label);
            label.Text = labelText;
            label.BorderStyle = BorderStyle.FixedSingle;
            label.BackColor = Color.FromArgb(255, 255, 203);

            Font f = new Font(this.Font.FontFamily, 12);
            label.Font = f;

            label.TextAlign = ContentAlignment.MiddleLeft;

            label.AutoSize = true;
            label.Padding = new Padding(10);
            label.MaximumSize = labelSize;

            label.Location = labelLocation;
            label.BringToFront();

            PictureBox arrow = new PictureBox();
            arrow.Name = "TooltipArrow";
            arrow.BackgroundImage = Properties.Resources.left_arrow;
            arrow.BackgroundImageLayout = ImageLayout.Center;
            arrow.Size = arrow.BackgroundImage.Size;

            Point arrowLocation = labelLocation;

            if (labelLocation.X > spriteLocation.X)
            {
                arrowLocation = labelLocation;
                arrowLocation.Offset(-arrow.Width + arrowOffset.X, (label.Height / 2) - (arrow.Height / 2) + arrowOffset.Y);
            }
            else if(labelLocation.X < spriteLocation.X)
            {
                arrow.BackgroundImage = Properties.Resources.right_arrow;
                arrowLocation.Offset(label.Width - arrowOffset.X, (label.Height / 2) - (arrow.Height / 2) + arrowOffset.Y);
            }

            arrow.Location = arrowLocation;

            this.Controls.Add(arrow);
            arrow.BringToFront();

            label.Hide();
            arrow.Hide();

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(spriteDelay);
                this.Invoke(new Action(() => {
                    _sprite.PutBaseImageLocation(spriteLocation);
                    PlayAnimation(animation);
                }));
            });

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(labelDelay);
                this.Invoke(new Action(() => {
                    label.Show();
                    arrow.Show();
                    label.BringToFront();
                    arrow.BringToFront();
                }));
            });
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
