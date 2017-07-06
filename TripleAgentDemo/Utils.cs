using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripleAgent
{
    public static class Utils
    {
        public static int SpriteSheetPointToFrameIndex(Image spriteSheet, Size spriteSize, Point point)
        {
            int frameNum = 1;

            int numFramesX = (spriteSheet.Width / spriteSize.Width);
            int numFramesY = (spriteSheet.Height / spriteSize.Height);

            if (point.X > spriteSheet.Width - spriteSize.Width || point.X < 0 || point.Y > spriteSheet.Height - spriteSize.Height || point.Y < 0)
            {
                throw new Exception("Animation frame index out of range.");
            }

            int loopX = 0;
            int loopY = 0;
            for (int y = 1; y < numFramesY; y++)
            {
                for (int x = 0; x < numFramesX; x++)
                {
                    loopX = (spriteSize.Width * x);

                    if (point.X == loopX && point.Y == loopY)
                        return frameNum;

                    frameNum++;
                }

                loopY = (spriteSize.Height * y);
            }

            return frameNum;
        }

        public static Point SpriteSheetFrameIndexToPoint(Image spriteSheet, Size spriteSize, int index)
        {
            Point frameLoc = new Point(0, 0);

            int numFramesX = (spriteSheet.Width / spriteSize.Width);
            int numFramesY = (spriteSheet.Height / spriteSize.Height);

            if (index < 1)
                index = 1;

            if (index > (numFramesX * numFramesY))
            {
                throw new Exception("Animation frame index out of range.");
            }

            int frameNum = 1;
            for (int y = 0; y < numFramesY; y++)
            {
                for (int x = 0; x < numFramesX; x++)
                {
                    if (frameNum == index)
                    {
                        int locX = (int)(x * spriteSize.Width);
                        int locY = (int)(y * spriteSize.Height);
                        return new Point(locX, locY);
                    }
                    frameNum++;
                }
            }

            return frameLoc;
        }

    }
}
