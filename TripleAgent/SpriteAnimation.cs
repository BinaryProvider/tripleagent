using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripleAgent
{
    [Serializable]
    public class SpriteAnimation
    {
        public string Description { get; set; }

        private int _frameStart = 1;
        public int FrameStart
        {
            get { return _frameStart; }
            set { _frameStart = value; }
        }

        private int _frameEnd = 1;
        public int FrameEnd
        {
            get { return _frameEnd; }
            set { _frameEnd = value; }
        }

        private int _frameDuration = 100;
        public int FrameDuration
        {
            get { return _frameDuration; }
            set { _frameDuration = value; }
        }

        private int? _subsequentAnimationIndex;
        public int? SubsequentAnimationIndex
        {
            get { return _subsequentAnimationIndex; }
            set { _subsequentAnimationIndex = value; }
        }

        public override string ToString()
        {
            return String.Format("{0}", Description);
        }
    }
}
