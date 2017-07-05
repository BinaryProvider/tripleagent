using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace SpriteLibrary
{
    /// <summary>
    /// A single frame of an animation
    /// </summary>
    internal class AnimationSingleFrame
    {
        public Image Frame;
        public Image ResizedFrame;
        public int ID;
        public AnimationSingleFrame(Image SpriteImage, int id)
        {
            Frame = SpriteImage;
            ID = id;
            ResizedFrame = null;
        }
    }
    internal class AnimationFrame
    {
        public int SingleFrameID;       //The ID number of the image we want to view
        public TimeSpan Duration; //How long does this image stay there.
        public AnimationFrame(int ID, TimeSpan HowLong)
        {
            SingleFrameID = ID;
            Duration = HowLong;
        }
    }

    /// <summary>
    /// One animation.  A series of images.
    /// </summary>
    internal class Animation
    {
        public int AnimationID;
        public List<AnimationFrame> Frames = new List<AnimationFrame>();

        //The simplest case.  It is just one image.  No animation.
        public Animation(SmartImage Smart_Image, Image SpriteImage)
        {
            //We create a new frame for it and make an animation of just one frame
            AnimationID = Smart_Image.AnimationCount;
            AnimationSingleFrame newSingle = new AnimationSingleFrame(SpriteImage, Smart_Image.FrameCount);
            AnimationFrame newFrame = new AnimationFrame(Smart_Image.FrameCount, TimeSpan.FromMilliseconds(500));
            Frames.Add(newFrame);
            Smart_Image.AddFrame(newSingle);
        }

        /// <summary>
        /// Create an image from an image that has a bunch of frames in the one image.
        /// Start at the specified position (Start), and grab Count items (if we can find them)
        /// </summary>
        /// <param name="Count">The number of frames to grab</param>
        /// <param name="Start">A point in the image where we start capturing frames</param>
        /// <param name="Smart_Image">The smart image this is part of</param>
        /// <param name="SpriteImage">the image we use for the sprite.  Should have lots of images as a part of it.</param>
        /// <param name="width">the width of each frame</param>
        /// <param name="height">the height of each frame</param>
        /// <param name="duration">The duration in miliseconds for this frame</param>
        internal Animation(Point Start, SmartImage Smart_Image, Image SpriteImage, int width, int height, int duration, int Count)
        {
            //We create a new animation number for this new animation
            AnimationID = Smart_Image.AnimationCount;

            //Now, we make new frames for each image we can find.
            int x = Start.X;
            int y = Start.Y;
            int counter = 0;
            Rectangle where;
            Image newImage;
            while (y + height <= SpriteImage.Height)
            {
                while (x + width <= SpriteImage.Width)
                {
                    where = new Rectangle(x, y, width, height);
                    Bitmap tImage = (Bitmap)SpriteImage;
                    newImage = (Bitmap)tImage.Clone(where, tImage.PixelFormat);
                    AnimationSingleFrame newSingle = new AnimationSingleFrame(newImage, Smart_Image.FrameCount);
                    AnimationFrame newFrame = new AnimationFrame(Smart_Image.FrameCount, TimeSpan.FromMilliseconds(duration));
                    Frames.Add(newFrame);
                    Smart_Image.AddFrame(newSingle);
                    x += width;
                    counter++;
                    if (counter >= Count)
                        return; //Stop when we have reached Count of them
                }
                y += height;
                x = 0;
            }
        }

        /// <summary>
        /// Create an image from an image that has a bunch of frames in the one image.
        /// Start at (0,0) with the specified height and width.  Pull out as many images as we can
        /// </summary>
        /// <param name="Smart_Image">The smart image this is part of</param>
        /// <param name="SpriteImage">the image we use for the sprite.  Should have lots of images as a part of it.</param>
        /// <param name="width">the width of each frame</param>
        /// <param name="height">the height of each frame</param>
        /// <param name="duration">The duration in miliseconds for this frame</param>
        internal Animation(SmartImage Smart_Image, Image SpriteImage, int width, int height, int duration)
        {
            //We create a new animation number for this new animation
            AnimationID = Smart_Image.AnimationCount;

            //Now, we make new frames for each image we can find.
            int x = 0;
            int y = 0;
            Rectangle where;
            Image newImage;
            while (y + height <= SpriteImage.Height)
            {
                while (x + width <= SpriteImage.Width)
                {
                    where = new Rectangle(x, y, width, height);
                    Bitmap tImage = (Bitmap)SpriteImage;
                    newImage = (Bitmap)tImage.Clone(where, tImage.PixelFormat);
                    AnimationSingleFrame newSingle = new AnimationSingleFrame(newImage, Smart_Image.FrameCount);
                    AnimationFrame newFrame = new AnimationFrame(Smart_Image.FrameCount, TimeSpan.FromMilliseconds(duration));
                    Frames.Add(newFrame);
                    Smart_Image.AddFrame(newSingle);
                    x += width;
                }
                y += height;
                x = 0;
            }
        }

        internal Animation(SmartImage Smart_Image, int AnimationToCopy, bool MirrorHorizontally, bool MirrorVertically)
        {
            //We create a new animation number for this new animation
            AnimationID = Smart_Image.AnimationCount;

            Animation One = Smart_Image.getAnimation(AnimationToCopy);
            Image MeImage;

            for (int i = 0; i < One.Frames.Count; i++)
            {
                AnimationFrame AF = Smart_Image.GetAnimationFrame(AnimationToCopy, i);
                AnimationSingleFrame ASF = Smart_Image.GetSingleFrame(AnimationToCopy, i);
                MeImage = ASF.Frame;
                TimeSpan Duration = AF.Duration;
                Image MirrorImage = new Bitmap(MeImage);
                if (MirrorHorizontally) MirrorImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                if (MirrorVertically) MirrorImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
                //GC.Collect();
                MeImage = MirrorImage;

                AnimationSingleFrame newSingle = new AnimationSingleFrame(MeImage, Smart_Image.FrameCount);
                AnimationFrame newFrame = new AnimationFrame(Smart_Image.FrameCount, Duration);
                Frames.Add(newFrame);
                Smart_Image.AddFrame(newSingle);
            }
        }

        internal Animation(SmartImage Smart_Image, int AnimationToCopy, int DegreesOfRotation)
        {
            //We create a new animation number for this new animation
            AnimationID = Smart_Image.AnimationCount;

            GraphicsUnit tUnit = GraphicsUnit.Pixel;
            Animation One = Smart_Image.getAnimation(AnimationToCopy);
            Image MeImage;

            for (int i = 0; i < One.Frames.Count; i++)
            {
                AnimationFrame AF = Smart_Image.GetAnimationFrame(AnimationToCopy, i);
                AnimationSingleFrame ASF = Smart_Image.GetSingleFrame(AnimationToCopy, i);
                MeImage = ASF.Frame;
                TimeSpan Duration = AF.Duration;
                Image rotatedImage = new Bitmap(MeImage.Width, MeImage.Height);
                using (Graphics g = Graphics.FromImage(rotatedImage))
                {
                    g.TranslateTransform(MeImage.Width / 2, MeImage.Height / 2); //set the rotation point as the center into the matrix
                    g.RotateTransform(DegreesOfRotation * -1); //rotate.  The rotation direction we use is opposite of what they use
                    g.TranslateTransform(-MeImage.Width / 2, -MeImage.Height / 2); //restore rotation point into the matrix
                    g.DrawImage(MeImage, MeImage.GetBounds(ref tUnit), rotatedImage.GetBounds(ref tUnit), tUnit); //draw the image on the new bitmap
                }
                //GC.Collect();
                MeImage = rotatedImage;

                AnimationSingleFrame newSingle = new AnimationSingleFrame(MeImage, Smart_Image.FrameCount);
                AnimationFrame newFrame = new AnimationFrame(Smart_Image.FrameCount, Duration);
                Frames.Add(newFrame);
                Smart_Image.AddFrame(newSingle);
            }
        }
    }
    /// <summary>
    /// This is the holder and parser for images within the AnimatedSprite world
    /// It allows you to store and access animations.  A smart image might be a "troll"
    /// that has a series of animations for up, down, left, right, and die.
    /// </summary>
    internal class SmartImage
    {
        SpriteController MyController;
        List<Animation> Animations = new List<Animation>();
        List<AnimationSingleFrame> Frames = new List<AnimationSingleFrame>();
        public int FrameCount { get { return Frames.Count; } }
        public int AnimationCount { get { return Animations.Count; } }

        public SmartImage(SpriteController Controller, Image SpriteImage)
        {
            MyController = Controller;
            AddAnimation(SpriteImage);
        }

        /// <summary>
        /// Make an animated image from an image that contains multiple frames
        /// </summary>
        /// <param name="Controller">The sprite controller this is attached to</param>
        /// <param name="SpriteImage">The image we use to draw the animation from</param>
        /// <param name="width">The width of the image to cut out of the main image</param>
        /// <param name="height">The height of the image to cut out of the main image</param>
        /// <param name="duration">The duration in miliseconds</param>
        public SmartImage(SpriteController Controller, Image SpriteImage, int width, int height, int duration)
        {
            MyController = Controller;
            AddAnimation(SpriteImage, width, height, duration);
        }
        public SmartImage(Point Start, SpriteController Controller, Image SpriteImage, int width, int height, int duration, int Count)
        {
            MyController = Controller;
            AddAnimation(Start, SpriteImage, width, height, duration, Count);
        }


        public Animation getAnimation(int index)
        {
            if (index < 0 || index > AnimationCount) return null;
            return Animations[index];
        }

        public void AddAnimation(Image SpriteImage)
        {
            Animation tAnimation = new Animation(this, SpriteImage);
            Animations.Add(tAnimation);
        }
        public void AddAnimation(Image SpriteImage, int width, int height, int duration)
        {
            Animation tAnimation = new Animation(this, SpriteImage, width, height, duration);
            Animations.Add(tAnimation);
        }
        public void AddAnimation(Point Start, Image SpriteImage, int width, int height, int duration, int Count)
        {
            Animation tAnimation = new Animation(Start, this, SpriteImage, width, height, duration, Count);
            Animations.Add(tAnimation);
        }

        public void AddAnimation(int AnimationToCopy, bool MirrorHorizontal, bool MirrorVertical)
        {
            if (AnimationToCopy < 0) return;
            if (AnimationToCopy >= AnimationCount) return;

            Animation tAnimation = new Animation(this, AnimationToCopy, MirrorHorizontal, MirrorVertical);
            Animations.Add(tAnimation);
        }
        public void AddAnimation(int AnimationToCopy, int RotationDegrees)
        {
            if (AnimationToCopy < 0) return;
            if (AnimationToCopy >= AnimationCount) return;
            Animation tAnimation = new Animation(this, AnimationToCopy, RotationDegrees);
            Animations.Add(tAnimation);
        }

        public void ReplaceImage(Image newimage, int animation, int frame)
        {
            Image SpriteImage;
            if (animation >= 0 && animation < Animations.Count)
            {
                if (frame >= 0 && frame < Animations[animation].Frames.Count)
                {
                    int wFrame = Animations[animation].Frames[frame].SingleFrameID;
                    for (int looper = 0; looper < Frames.Count; looper++)
                    {
                        if (Frames[looper].ID == wFrame)
                        {
                            SpriteImage = Frames[looper].Frame;
                            if (SpriteImage == null) return;
                            Graphics.FromImage(SpriteImage).Clear(Color.Transparent); //Erase the old image
                            Graphics.FromImage(SpriteImage).DrawImage(newimage, 0, 0, SpriteImage.Width, SpriteImage.Height);
                            Frames[looper].ResizedFrame = null; //make sure we redraw the resized frame if we need to do that.
                            return; //Now that we have found it, return from the loop
                        }
                    }
                }
            }
        }

        public AnimationFrame GetAnimationFrame(int animation, int frame)
        {
            if (animation >= 0 && animation < Animations.Count)
            {
                if (frame >= 0 && frame < Animations[animation].Frames.Count)
                {
                    return Animations[animation].Frames[frame];
                }
            }
            return null;
        }

        public AnimationSingleFrame GetSingleFrame(int animation, int frame)
        {
            AnimationFrame AF = GetAnimationFrame(animation, frame);
            int wFrame = AF.SingleFrameID;
            for (int looper = 0; looper < Frames.Count; looper++)
            {
                if (Frames[looper].ID == wFrame)
                    return Frames[looper];
            }
            return null;
        }

        public Image GetImage(int animation, int frame, Size HowBig)
        {
            Image tImage;
            bool NeedsResize = false;
            //This will change.
            if (animation >= 0 && animation < Animations.Count)
            {
                if (frame >= 0 && frame < Animations[animation].Frames.Count)
                {
                    int wFrame = Animations[animation].Frames[frame].SingleFrameID;
                    for (int looper = 0; looper < Frames.Count; looper++)
                    {
                        if (Frames[looper].ID == wFrame)
                        {
                            tImage = Frames[looper].Frame;
                            if (!MyController.OptimizeForLargeSpriteImages) return tImage; //If we are not set to optimize, do not try to do so.
                            if (HowBig.Width == 0) return tImage;
                            if (Math.Abs(((double)tImage.Size.Width - (double)HowBig.Width) / HowBig.Width) - 1 > .3)
                                NeedsResize = true;
                            if (Math.Abs(((double)tImage.Size.Height - (double)HowBig.Height) / HowBig.Height) - 1 > .3)
                                NeedsResize = true;
                            if(NeedsResize)
                            {
                                Size newsize = new Size(HowBig.Width + (int)(HowBig.Width * .2), HowBig.Height + (int)(HowBig.Height * .2));
                                if(Frames[looper].ResizedFrame == null || Frames[looper].ResizedFrame.Size != newsize)
                                {
                                    Frames[looper].ResizedFrame = new Bitmap(newsize.Width, newsize.Height);
                                    Rectangle newrec = new Rectangle(0, 0, newsize.Width, newsize.Height);
                                    Graphics.FromImage(Frames[looper].ResizedFrame).DrawImage(tImage, newrec);
                                }
                                return Frames[looper].ResizedFrame;
                            }
                            return tImage;
                        }
                    }
                }
            }
            return null;
        }

        public TimeSpan GetCurrentDuration(int animation,  int frame)
        {
            if (animation < 0 || animation >= Animations.Count) return new TimeSpan();
            if (frame < 0 || frame >= Animations[animation].Frames.Count) return new TimeSpan();
            return Animations[animation].Frames[frame].Duration;
        }

        public void AddFrame(AnimationSingleFrame ToAdd)
        {
            Frames.Add(ToAdd);
        }

        /// <summary>
        /// Return true if the specified animation and frame for that animation needs
        /// to be changed due to the time passing.
        /// </summary>
        /// <param name="animation">The animation index</param>
        /// <param name="frame">the frame index</param>
        /// <param name="duration">The time that has passed since the last frame was displayed.</param>
        /// <returns></returns>
        public bool NeedsNewImage(int animation, int frame, TimeSpan duration)
        {
            //If we do not have a valid index, return true
            if (frame < 0) return true;
            if (animation < 0) return true;
            if (animation >= Animations.Count) return true; //Hopefully we never get here
            if (frame >= Animations[animation].Frames.Count) return true;

            //If no duration is set, we never have to change it.
            if (Animations[animation].Frames[frame].Duration.TotalMilliseconds == 0)
                return true;

            //If we get here, we the current index is a valid one.  Now, see if the timeframe needs to be changed
            if (duration > Animations[animation].Frames[frame].Duration)
                return true;

            //If we get here, it does not need to be changed.
            return false;
        }

        /// <summary>
        /// Check to see if the animation is in the last frame.  Only works if animateonce is set to true
        /// </summary>
        /// <param name="AnimateOnce">The animateOnce value of the sprite</param>
        /// <param name="animation">The animation we think we are on</param>
        /// <param name="frame">The frame we think we are on</param>
        /// <returns></returns>
        public bool AnimationDone(int animation, int frame, bool AnimateOnce)
        {
            if (!AnimateOnce) return false;
            if (frame == Animations[animation].Frames.Count - 1)
                return true;
            return false;
        }

        /// <summary>
        /// Return the number of frames that the specified animation has.
        /// </summary>
        /// <param name="Animation">What animation to check</param>
        /// <returns>The number of animation frames found in that animation</returns>
        public int AnimationFrameCount(int Animation)
        {
            if (Animation < 0 || Animation >= Animations.Count) return 0;
            return Animations[Animation].Frames.Count;
        }

        public TimeSpan ChangeIndex(int animation, ref int frame, TimeSpan duration, bool AnimateOnce = false)
        {
            TimeSpan remainder = duration;
            if (frame < 0) frame = 0;
            if (animation < 0) return remainder;
            if (animation >= Animations.Count) return remainder; //Hopefully we never get here
            if (frame >= Animations[animation].Frames.Count) return remainder;

            //we have a valid index
            while (remainder.TotalMilliseconds >= Animations[animation].Frames[frame].Duration.TotalMilliseconds &&
                Animations[animation].Frames[frame].Duration.TotalMilliseconds != 0)
            {
                remainder = remainder - Animations[animation].Frames[frame].Duration;
                frame++;
                if (AnimateOnce && frame >= Animations[animation].Frames.Count)
                {
                    //we stay on the last frame.  We also stop decrementing the timeamount.  That lets us know when the last frame terminates.
                    frame = Animations[animation].Frames.Count - 1;
                    remainder = remainder + Animations[animation].Frames[frame].Duration;
                    return remainder;
                }
                else if (frame >= Animations[animation].Frames.Count)
                {
                    frame = 0;
                }
            }
            return remainder;
        }
    }
}
