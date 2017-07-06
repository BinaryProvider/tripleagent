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

        private ContentAlignment _spriteStartLocation = ContentAlignment.TopLeft;

        [Browsable(true)]
        public ContentAlignment SpriteStartLocation
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
            set { _spriteAnimations = value; }
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
                    _sprite = new Sprite(FrameLoc(_spriteStartFrame), _spriteController, _spriteSheet, _spriteSize.Width, _spriteSize.Height, 100, 1);
                    _sprite.SpriteAnimationComplete += SpriteAnimationComplete;
                    _sprite.SpriteChangesAnimationFrames += SpriteChangesAnimationFrames;

                    AddAnimations();

                    if (!_spriteHiddenFromStart)
                    {
                        _sprite.PutBaseImageLocation(AlignmentToPoint(_spriteStartLocation));
                    }

                }
            }
        }

        private void CreateCanvas()
        {
            _canvas.Location = new Point(0, 0);
            _canvas.Size = this.Size;
            _canvas.BackgroundImageLayout = ImageLayout.Stretch;
            this.Controls.Add(_canvas);
        }

        public void AddAnimationData(XmlDocument doc)
        {
            SpriteAnimations.Clear();

            XmlNodeList animationsData = doc.SelectNodes("//animation");
            foreach(XmlNode animationData in animationsData)
            {
                if (animationData.Attributes["name"] == null)
                    throw new Exception("Invalid animation data. No animation name.");

                string name = animationData.Attributes["name"].Value;
                int frameduration = 100;
                int subsequentanimationindex = -1;

                XmlNode startFrame = animationData.SelectSingleNode("startframe");
                XmlNode endFrame = animationData.SelectSingleNode("endframe");

                if(animationData.Attributes["frameduration"] != null)
                {
                    if (!int.TryParse(animationData.Attributes["frameduration"].Value, out frameduration))
                        frameduration = 100;
                }

                if (animationData.Attributes["subsequentanimationindex"] != null)
                {
                    if (!int.TryParse(animationData.Attributes["subsequentanimationindex"].Value, out subsequentanimationindex))
                        subsequentanimationindex = -1;
                }

                if (startFrame == null || endFrame == null)
                    throw new Exception("Invalid animation data. Frames could not be parsed.");

                if ((startFrame.Attributes["x"] == null || startFrame.Attributes["y"] == null) && startFrame.Attributes["num"] == null)
                    throw new Exception("Invalid animation data. Frames could not be parsed.");

                if ((endFrame.Attributes["x"] == null || endFrame.Attributes["y"] == null) && endFrame.Attributes["num"] == null)
                    throw new Exception("Invalid animation data. Frames could not be parsed.");

                int startX, startY, endX, endY, startNum, endNum;

                if (startFrame.Attributes["num"] == null)
                {
                    if (!int.TryParse(startFrame.Attributes["x"].Value, out startX) || !int.TryParse(startFrame.Attributes["y"].Value, out startY))
                        throw new Exception("Invalid animation data. Frames could not be parsed.");

                    startNum = LocFrame(new Point(startX, startY));
                }
                else
                {
                    if (!int.TryParse(startFrame.Attributes["num"].Value, out startNum))
                        throw new Exception("Invalid animation data. Frames could not be parsed.");
                }

                if(endFrame.Attributes["num"] == null)
                {
                    if (!int.TryParse(endFrame.Attributes["x"].Value, out endX) || !int.TryParse(endFrame.Attributes["y"].Value, out endY))
                        throw new Exception("Invalid animation data. Frames could not be parsed.");

                    endNum = LocFrame(new Point(endX, endY));
                }
                else
                {
                    if (!int.TryParse(endFrame.Attributes["num"].Value, out endNum))
                        throw new Exception("Invalid animation data. Frames could not be parsed.");
                }

                SpriteAnimation animation = new SpriteAnimation();
                animation.Name = name;
                animation.FrameStart = startNum;
                animation.FrameEnd = endNum;
                animation.FrameDuration = frameduration;

                if (subsequentanimationindex > -1)
                    animation.SubsequentAnimationIndex = subsequentanimationindex;

                SpriteAnimations.Add(animation);
            }
        }

        private void AddAnimations()
        {
            foreach (SpriteAnimation animation in _spriteAnimations)
            {
                int frameStart = (animation.FrameStart > 0) ? animation.FrameStart : 1;
                int frameEnd = (animation.FrameEnd > 0) ? animation.FrameEnd : 1;
                int frameTotal = (frameEnd - frameStart);
                int frameDuration = animation.FrameDuration;
                Point startLoc = FrameLoc(frameStart);
                _sprite.AddAnimation(startLoc, _spriteSheet, _spriteSize.Width, _spriteSize.Height, frameDuration, frameTotal);
            }
        }

        public void PlayAnimation(int animationIndex, int loopTimes, bool loopForever = false)
        {
            if (_sprite != null)
            {
                if (loopForever)
                    loopTimes = Int32.MaxValue;

                if (loopTimes <= 1)
                {
                    _sprite.AnimateOnce(animationIndex + 1);
                }
                else
                {
                    _sprite.AnimateJustAFewTimes(animationIndex + 1, loopTimes);
                }

                if (animationIndex >= 0)
                {
                    if (_spriteAnimations[animationIndex].SubsequentAnimationIndex != null)
                    {
                        _spriteAnimationQueue.Enqueue((int)_spriteAnimations[animationIndex].SubsequentAnimationIndex);
                    }
                }

                _sprite.UnPause();
            }
        }

        private void SpriteAnimationComplete(object sender, SpriteEventArgs e)
        {
            _sprite.Pause();

            if (_spriteAnimationQueue.Count != 0)
            {
                PlayAnimation(_spriteAnimationQueue.Dequeue(), 1);
            }
            else
            {
                PlayAnimation(-1, 1);
            }
        }

        private void SpriteChangesAnimationFrames(object sender, SpriteEventArgs e)
        {
        }

        private int LocFrame(Point point)
        {
            int frameNum = 1;

            int numFramesX = (_spriteSheet.Width / _spriteSize.Width);
            int numFramesY = (_spriteSheet.Height / _spriteSize.Height);

            if (point.X > _spriteSheet.Width - _spriteSize.Width || point.X < 0 || point.Y > _spriteSheet.Height - _spriteSize.Height || point.Y < 0)
                throw new Exception("Animation frame index out of range.");

            int loopX = 0;
            int loopY = 0;
            for (int y = 1; y < numFramesY; y++)
            {
                for (int x = 0; x < numFramesX; x++)
                {
                    loopX = (_spriteSize.Width * x);

                    if (point.X == loopX && point.Y == loopY)
                        return frameNum;

                    frameNum++;
                }

                

                loopY = (_spriteSize.Height * y);
            }

            return frameNum;
        }

        private Point FrameLoc(int index)
        {
            Point frameLoc = new Point(0, 0);

            int numFramesX = (_spriteSheet.Width / _spriteSize.Width);
            int numFramesY = (_spriteSheet.Height / _spriteSize.Height);

            if (index < 1)
                index = 1;

            if(index > (numFramesX * numFramesY))
                throw new Exception("Animation frame index out of range.");

            int frameNum = 1;
            for (int y = 0; y < numFramesY; y++)
            {
                for (int x = 0; x < numFramesX; x++)
                {
                    if(frameNum == index)
                    {
                        int locX = (int)(x * _spriteSize.Width);
                        int locY = (int)(y * _spriteSize.Height);
                        return new Point(locX, locY);
                    }
                    frameNum++;
                }
            }

            return frameLoc;
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
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Name = "TripleAgent";
            this.Size = new System.Drawing.Size(52, 60);
            this.ResumeLayout(false);

        }
    }
}
