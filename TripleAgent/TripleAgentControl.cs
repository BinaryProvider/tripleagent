using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using SpriteLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

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

        public TripleAgentControl()
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

        public void PlayAnimation(int animationIndex, int loopTimes)
        {
            if (loopTimes <= 1)
            {
                _sprite.AnimateOnce(animationIndex + 1);
            }
            else
            {
                _sprite.AnimateJustAFewTimes(animationIndex + 1, loopTimes);
            }

            if (_spriteAnimations[animationIndex].SubsequentAnimationIndex != null)
            {
                _spriteAnimationQueue.Enqueue((int)_spriteAnimations[animationIndex].SubsequentAnimationIndex);
            }

            _sprite.UnPause();
        }

        private void SpriteAnimationComplete(object sender, SpriteEventArgs e)
        {
            _sprite.Pause();

            if (_spriteAnimationQueue.Count != 0)
                PlayAnimation(_spriteAnimationQueue.Dequeue(), 1);
        }

        private void SpriteChangesAnimationFrames(object sender, SpriteEventArgs e)
        {
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

    }
}
