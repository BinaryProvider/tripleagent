using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TripleAgent
{
    [Serializable]
    public class SpriteAnimation
    {
        private int _index;
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        public string Name { get; set; }

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

        private int _loopCount = 0;
        public int LoopCount
        {
            get { return _loopCount; }
            set { _loopCount = value; }
        }

        private int _loopDelay = 0;
        public int LoopDelay
        {
            get { return _loopDelay; }
            set { _loopDelay = value; }
        }

        public override string ToString()
        {
            return String.Format("{0}", Name);
        }

        public static List<SpriteAnimation> ParseAnimationData(XmlDocument doc, Image spriteSheet, Size spriteSize)
        {
            List<SpriteAnimation> animations = new List<SpriteAnimation>();

            XmlNodeList animationsData = doc.SelectNodes("//animation");
            foreach (XmlNode animationData in animationsData)
            {
                if (animationData.Attributes["name"] == null)
                    throw new Exception("Invalid animation data. No animation name.");

                string name = animationData.Attributes["name"].Value;
                int frameduration = 100;
                int subsequentanimationindex = -1;
                int loopcount = 0;
                int loopdelay = 0;

                if (animationData.Attributes["loopcount"] != null)
                {
                    if (!int.TryParse(animationData.Attributes["loopcount"].Value, out loopcount))
                        loopcount = 0;
                }

                if (animationData.Attributes["loopdelay"] != null)
                {
                    if (!int.TryParse(animationData.Attributes["loopdelay"].Value, out loopdelay))
                        loopdelay = 0;
                }

                XmlNode startFrame = animationData.SelectSingleNode("startframe");
                XmlNode endFrame = animationData.SelectSingleNode("endframe");

                if (animationData.Attributes["frameduration"] != null)
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

                    startNum = Utils.SpriteSheetPointToFrameIndex(spriteSheet, spriteSize, new Point(startX, startY));
                }
                else
                {
                    if (!int.TryParse(startFrame.Attributes["num"].Value, out startNum))
                        throw new Exception("Invalid animation data. Frames could not be parsed.");
                }

                if (endFrame.Attributes["num"] == null)
                {
                    if (!int.TryParse(endFrame.Attributes["x"].Value, out endX) || !int.TryParse(endFrame.Attributes["y"].Value, out endY))
                        throw new Exception("Invalid animation data. Frames could not be parsed.");

                    endNum = Utils.SpriteSheetPointToFrameIndex(spriteSheet, spriteSize, new Point(endX, endY));
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
                animation.LoopCount = loopcount;
                animation.LoopDelay = loopdelay;

                if (subsequentanimationindex > -1)
                    animation.SubsequentAnimationIndex = subsequentanimationindex;

                animations.Add(animation);
            }

            return animations;
        }

    }
}
