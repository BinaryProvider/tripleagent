using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace SpriteLibrary
{
    /// <summary>
    /// An EventArgs that contains information about Sprites.  Most of the Sprite events use
    /// this SpriteEventArgs.
    /// </summary>
    public class SpriteEventArgs : EventArgs
    {
        /// <summary>
        /// If another Sprite is involved in the event (Collision), than that Sprite is included here.
        /// It will be null if no other Sprite is involved.
        /// </summary>
        public Sprite TargetSprite=null;
        /// <summary>
        /// The CollisionMethod used in the event.  Currently, only rectangle collisions are used
        /// </summary>
        public SpriteCollisionMethod CollisionMethod = SpriteCollisionMethod.rectangle;
        /// <summary>
        /// For the CheckBeforeMove event, newlocation will be the location the sprite is trying
        /// to move to. You can adjust the point (move it left, right, up, down) and it will affect
        /// the placement of the sprite.
        /// </summary>
        public Point NewLocation = new Point(-1,-1);
        /// <summary>
        /// Used primarily in the CheckBeforeMove event.  If you set cancel to true, then the move fails.
        /// You can use this to keep a Sprite from going places where it ought not to go.
        /// </summary>
        public bool Cancel = false;
    }
    /// <summary>
    /// A Sprite is an animated image that has a size, position, rotation, and possible vector
    /// It tracks where in the animation sequence it is, can report colisions, etc.  This SpriteController
    /// draws, moves, and deals with most graphical aspects of the sprites for you.
    /// </summary>
    public class Sprite
    {
        int _ID = -1;
        /// <summary>
        /// The Sprite ID as specified by the sprite controller.
        /// </summary>
        public int ID { get { return _ID; } private set { _ID = value; } }
        private bool SpriteHasInitialized = false;
        /// <summary>
        /// The name of the sprite.  Use SetSpriteName(Name) to change this name.  Most Named sprites
        /// are used to define what a sprite is.  Once you have created a named sprite, you usually use
        /// <see cref="SpriteController.DuplicateSprite(String)"/> to clone the sprite for use.  The basic rule of thumb is
        /// to load your sprites from images once, and name the initial sprites.  Then, when you go to use
        /// those sprites, get duplicates of them.  The reason for this is because it takes more processing time to initially
        /// create the sprites than it takes to duplicate them.
        /// </summary>
        public string SpriteName { get; private set; }
        /// <summary>
        /// Set the opacity of the sprite.  The value should be between 0 and 1.  1 is solid, 0 is transparent.
        /// Sometimes you want to drag a sprite around the map, or show a sprite that "could be there."  Setting
        /// the sprite opacity is usually how you do that.  One warning, however.  The opacity value takes effect the
        /// next time it is drawn.  If the sprite is animating rapidly, it will take effect nearly emmediately.  If
        /// it is not animating, not moving, or just sitting there, then it may not take effect for quite some time.
        /// </summary>
        public float Opacity { get { return _opacity; } set { if (value <= 1 && value >= 0) _opacity = value; } }
        private float _opacity = 1;

        /// <summary>
        /// Return the name of the sprite that this was duplicated from.  A duplicated sprite will have
        /// no name, but will have a SpriteOriginName.
        /// </summary>
        public string SpriteOriginName { get; private set; }
        SmartImage MyImage;
        private double SpeedAdjust = 1;
        DateTime LastResetImage = DateTime.UtcNow;
        int _FrameIndex = -1;
        /// <summary>
        /// This is the frame of the current animation sequence.  You can use this if you need to figure out what frame index
        /// to resume something at, or something like that.
        /// </summary>
        public int FrameIndex
        {
            get { return _FrameIndex; }
            private set { _FrameIndex = value; }
        } //We start out with -1 so we know we need to draw from the begining.
        int _AnimationIndex = 0;
        int AnimationNumberCount = 0;
        private bool SetToAnimateOnce = false;
        private bool _AnimationDone = false;
        /// <summary>
        /// Report whether or not the animation has been completed.  When you tell a Sprite to AnimateOnce,
        /// this will report "false" until the animation sequence has been finished.  At that time, the value
        /// will be "True."  The tricky bit is that this is a boolean.  If you have not told a sprite to
        /// animate once, it will always return "false."  If a sprite is paused, this returns "false."  The only
        /// time this returns "true" is when you tell a sprite to animate once, or animate a few times, and those
        /// times have completed.  At that time, this will report "True".  If you have a sprite with only one frame,
        /// it may not look like it is "animating", but it is.  It is simply animating that one frame over and over.
        /// So, AnimationDone reports false, unless you have told it to animate_once.
        /// </summary>
        public bool AnimationDone { get { return _AnimationDone; } private set { _AnimationDone = value; } }
        private bool ForceRedraw = false;
        private bool NeedsDrawingAtEndOfTick = false; //We use this to say that we need to draw at end
        private System.Windows.Vector SpriteVector;
        /// <summary>
        /// The movement speed of the sprite.  To make a Sprite move, you need to set the MovementSpeed,
        /// the direction (using 
        /// <see cref="SpriteLibrary.Sprite.SetSpriteDirection(System.Windows.Vector)"/>, 
        /// <see cref="SpriteLibrary.Sprite.SetSpriteDirectionToPoint(Point)"/>, 
        /// <see cref="SpriteLibrary.Sprite.SetSpriteDirectionRadians(double)"/>,
        /// or <see cref="SpriteLibrary.Sprite.SetSpriteDirectionDegrees(double)"/>), and the 
        /// <see cref="SpriteLibrary.Sprite.AutomaticallyMoves"/> property.
        /// The speed is calculated in pixels per amount of time.  A higher number is faster than a lower number.
        /// </summary>
        public int MovementSpeed = 0; //Used in calculating when to move next.
        private DateTime LastMovement = DateTime.UtcNow;

        //For tracking if we are moving along a set of points
        private List<Point> MovementDestinations = new List<Point>();
        /// <summary>
        /// Tells us if we are in the process of doing a MoveTo operation.  This boolean should be the 
        /// opposite of SpriteReachedEndpoint, but that boolean is poorly named.  This is usually the easier
        /// one to use.
        /// </summary>
        public bool MovingToPoint { get { return _MovingToPoint; } private set { _MovingToPoint = value; } }
        private bool _MovingToPoint = false;

        /// <summary>
        /// If we are trying to collide with a sprite, we store that sprite here.
        /// </summary>
        private Sprite MovingToSprite = null;

        private bool _AutomaticallyMoves = false; //Does the sprite move in the direction it has been set?
        /// <summary>
        /// Determine if the sprite automatically moves (you need to give it a direction [using one of the
        /// SetSpriteDirection functions] and speed [MovementSpeed = X] also)
        /// </summary>     
        public bool AutomaticallyMoves
        {
            get { return _AutomaticallyMoves; }
            set
            {
                _AutomaticallyMoves = value;
                LastMovement = DateTime.UtcNow;
            }
        }
        private int _Zvalue = 50;
        /// <summary>
        /// A number from 0 to 100.  Default = 50. Higher numbers print on top of lower numbers.  If you want a sprite to 
        /// always be drawn on top of other sprites, give it a number higher than 50.  If you want a sprite to go under 
        /// other sprites, make its number lower than 50.
        /// </summary>
        public int Zvalue
        {
            get { return _Zvalue; }
            set { _Zvalue = value; if (_Zvalue < 0) _Zvalue = 0; if (_Zvalue > 100) _Zvalue = 100; MySpriteController.SortSprites(); }
        }
        private bool PausedAnimation = false;
        private bool PausedMovement = false;
        private bool PausedEvents = false;
        private DateTime PausedAnimationTime = DateTime.UtcNow;
        private DateTime PausedMovementTime = DateTime.UtcNow;
        /// <summary>
        /// Determine if the sprite will automatically move outside the box.  If not, it will hit the side of the box and stick
        /// </summary>
        public bool CannotMoveOutsideBox = false; //If set to true, it will not automatically move outside the picture box.

        /// <summary>
        /// Get or set the animation nimber.  It is best to change the animation using ChangeAnimation.
        /// It is safer.
        /// </summary>
        public int AnimationIndex
        {
            get { return _AnimationIndex; }
            set { if (value > -1 && value < MyImage.AnimationCount) _AnimationIndex = value; }
        }

        /// <summary>
        /// The number of animations this sprite has
        /// </summary>
        public int AnimationCount
        {
            get { return MyImage.AnimationCount; }
        }

        //The location and size of the sprite
        int xPositionOnImage = -1;
        int yPositionOnImage = -1;
        int xPositionOnPictureBox = -1;
        int yPositionOnPictureBox = -1;
        double xOnVector = -1;
        double yOnVector = -1;
        int Width = -1;
        int Height = -1;
        Rectangle MyRectangle { get { return new Rectangle(xPositionOnImage, yPositionOnImage, Width, Height); } }
        bool _HasBeenDrawn = false;
        /// <summary>
        /// Report whether or not this Sprite has been drawn.  If it has, then it needs to be erased at
        /// some point in time.
        /// </summary>
        public bool HasBeenDrawn { get { return _HasBeenDrawn; } private set { _HasBeenDrawn = value; } }
        /// <summary>
        /// The sprite location as found on the base image.  This is usually the easiest location to use.
        /// </summary>
        public Point BaseImageLocation { get { return new Point(xPositionOnImage, yPositionOnImage); } }
        /// <summary>
        /// The sprite location as found on the picture-box that this sprite is associated with.  Used when dealing with mouse-clicks
        /// </summary>
        public Point PictureBoxLocation { get { return new Point(xPositionOnPictureBox, yPositionOnPictureBox); } }
        /// <summary>
        /// Return the size of the sprite in reference to the image on which it is drawn.  To get the
        /// size of the Sprite in relation to the PictureBox, use GetVisibleSize
        /// </summary>
        public Size GetSize { get { return new Size(Width, Height); } }
        /// <summary>
        /// Return the relative size of the Sprite in relation to the PictureBox.  If the box has been 
        /// stretched or shrunk, that affects the visible size of the sprite.
        /// </summary>
        public Size GetVisibleSize { get { return new Size(VisibleWidth, VisibleHeight); } }

        //This is the rotation of the item.  If we change this, we will draw the sprite rotated.
        int _Rotation = 0;

        /// <summary>
        /// Change the rotation of the sprite, using degrees.  0 degrees is to the right.  90 is up.  
        /// 180 left, 270 down.  But, if your sprite was drawn facing up, then rotating it 90 degrees
        /// will have it pointing left.  The angle goes counter-clockwise.  The image will be scaled
        /// such that it continues to fit within the rectangle that it was originally in.  This results
        /// in a little bit of shrinking at times, but you should rarely notice that.
        /// </summary>
        public int Rotation { set { if (value >= 0 && value <= 360) _Rotation = value; } get { return _Rotation; } }

        /// <summary>
        /// Flip the image when it gets printed.  If your sprite is walking left, flipping it will
        /// make it look like it is going right.
        /// </summary>
        public bool MirrorHorizontally = false;

        /// <summary>
        /// Flip the image when it gets printed.  If your sprite looks like it is facing up, doing 
        /// this will make it look like it faces down.
        /// </summary>
        public bool MirrorVertically = false;

        SpriteController MySpriteController;
        private bool _Destroying = false;
        /// <summary>
        /// If the Sprite is in the middle of being Destroyed, this is set to true.  When a Sprite is
        /// Destroyed, it needs to erase itself and do some house-cleaning before it actually vanishes.
        /// During this time, you may not want to use it.  It is always a good thing to verify a Sprite
        /// is not in the middle of being destroyed before you do something important with it.  To Destroy
        /// a Sprite, use the Sprite.Destroy() function.
        /// </summary>
        public bool Destroying { get { return _Destroying; } }

        /// <summary>
        /// This is true unless we are using MoveTo(point) or MoveTo(list of points) to tell the sprite to move
        /// from one place to the next.  This boolean tells us if it has finished or not.
        /// </summary>
        public bool SpriteReachedEndPoint { get { return _SpriteReachedEndPoint; } internal set { _SpriteReachedEndPoint = value; } }
        bool _SpriteReachedEndPoint = true;

        /// <summary>
        /// The visible Height as seen in the PictureBox.  It may be stretched, or shrunk from the actual
        /// image size.
        /// </summary>
        public int VisibleHeight { get { return MySpriteController.ReturnPictureBoxAdjustedHeight(Height); } }
        /// <summary>
        /// The visible width as seen in the PictureBox.  The Sprite may be stretched or shrunk from the
        /// actual image size.
        /// </summary>
        public int VisibleWidth { get { return MySpriteController.ReturnPictureBoxAdjustedWidth(Width); } }

        /// <summary>
        /// A Sprite can hold a payload.  Use this to store extra information about the various Sprites.  Health, Armor,
        /// Shoot time, etc.  But, to store information in the payload, you need to make a new class of SpritePayload.  The syntax
        /// for doing so is: 
        /// <code Lang="C#">
        /// public class TankPayload : SpritePayload {  public int Armor; public int Speed; }
        /// </code>
        /// You can access the payload and retrieve the various values.  
        /// </summary>
        public SpritePayload payload = null; //The user can put anything they want here.
        //We changed the payload from being an object.  The Object was too vague.  By making it a defined type,
        //It helps with some level of type protection.
        //public object payload = null; //The user can put anything they want here.

        private List<Sprite> CollisionSprites = new List<Sprite>(); //The list of sprites that are colliding with this one

        //*********************************
        //****   Add event stubs **********
        /// <summary>
        /// A delegate that has a SpriteEventArgs instead of EventArgs.  Used for most
        /// of the Sprite events.  This allows us to pass more information from sprite events than
        /// a basic EventArgs allows for
        /// </summary>
        /// <param name="sender">The Sprite that triggers the event</param>
        /// <param name="e">A SpriteEventArgs class which contains Sprite Event values</param>
        public delegate void SpriteEventHandler(object sender, SpriteEventArgs e);
        /// <summary>
        /// This event happens right after the sprite is created.  Use this to immediately set a 
        /// sprite to animate once or something like that.
        /// </summary>
        public event SpriteEventHandler SpriteInitializes = delegate { };
        /// <summary>
        /// This happens when the sprite hits the border of the picture-box.  
        /// Useful for when you want to have shots explode when they hit the side.
        /// </summary>
        public event SpriteEventHandler SpriteHitsPictureBox = delegate { };
        /// <summary>
        /// This happens when the sprite has exited the picture box.  Useful when you want to 
        /// keep sprites from traveling on forever after exiting.
        /// </summary>
        public event SpriteEventHandler SpriteExitsPictureBox = delegate { };
        /// <summary>
        /// Only used when you tell an animation to animate once.  At the end of the animation, 
        /// this function fires off.
        /// </summary>
        public event SpriteEventHandler SpriteAnimationComplete = delegate { };
        /// <summary>
        /// This happens when two sprites hit each-other.  The SpriteEventArgs that is returned 
        /// contains the sprite that this sprite hits.
        /// </summary>
        public event SpriteEventHandler SpriteHitsSprite = delegate { };
        /// <summary>
        /// This event fires off before a sprite is drawn. Use it if you have constraints.  You 
        /// can change the location or cancel the move entirely.
        /// </summary>
        public event SpriteEventHandler CheckBeforeMove = delegate { };
        /// <summary>
        /// This event happens when someone clicks on the sprite (on the rectangle in which the sprite is).
        /// If you want the event to fire off only when someone clicks on the visible part of the sprite,
        /// use ClickTransparent instead.
        /// </summary>
        public event SpriteEventHandler Click = delegate { };
        /// <summary>
        /// This event happens when someone clicks on the sprite (on the sprite image itself).
        /// If the sprite is sometimes hidden, but you want the click to work even if it is not
        /// visible at that instant, use Click instead.
        /// </summary>
        public event SpriteEventHandler ClickTransparent = delegate { };
        /// <summary>
        /// This event happens when the mouse moves over the sprite, and then pauses.  We use the hover timing from the
        /// parent form.
        /// </summary>
        public event SpriteEventHandler MouseHover = delegate { };
        /// <summary>
        /// When the mouse moves over the sprite.  Use this for a menu, when you want the menu item to glow when the
        /// mouse is over the menu item sprite.
        /// </summary>
        public event SpriteEventHandler MouseEnter = delegate { };
        /// <summary>
        /// When the mouse moves off the sprite.  Use this for a menu, when you want the menu item to stop glowing when
        /// the mouse moves away from the menu item sprite.
        /// </summary>
        public event SpriteEventHandler MouseLeave = delegate { };
        /// <summary>
        /// When the frame of an animation changes.  If you want to have something happen every time
        /// the foot of your monster comes down, when the swing of your sword is at certain points, etc.
        /// Check to see that the Animaton and FrameIndex are what you expect them to be.
        /// </summary>
        public event SpriteEventHandler SpriteChangesAnimationFrames = delegate { };
        /// <summary>
        /// An event for when you tell a Sprite to MoveTo(Point) a specific point, or, when you 
        /// tell the Sprite to MoveTo(list of points).  When the Sprite has reached the final destination,
        /// the Sprite fires off this event.
        /// </summary>
        public event SpriteEventHandler SpriteArrivedAtEndPoint = delegate { };
        /// <summary>
        /// When you tell a sprite to MoveTo(list of points), this fires off every time it gets to
        /// one of the points.  When it gets to the final point, only the SpriteAtEndPoint event fires off.
        /// </summary>
        public event SpriteEventHandler SpriteArrivedAtWaypoint = delegate { };

        /// <summary>
        /// The Sprite has just been told to be destroyed.  You might want to do some cleanup.
        /// If you need to destroy some payload data, or tell something to cleanup after the sprite
        /// this is where to do that.
        /// </summary>
        public event SpriteEventHandler SpriteBeingDestroyed = delegate { };


        // **********************************************************
        //     *************  Start of Sprite Code  **********
        //          *************************************


        /// <summary>
        /// Generate a new sprite.  It takes the image and the width and height.  If there are multiple images of that width 
        /// and height in the image, an animation is created.
        /// </summary>
        /// <param name="Controller">The sprite controller that manages this sprite</param>
        /// <param name="SpriteImage">The image we pull the animation from</param>
        /// <param name="width">The width of one animation frame</param>
        /// <param name="height">The height of one animation frame</param>
        public Sprite(SpriteController Controller, Image SpriteImage, int width, int height)
        {
            MySpriteController = Controller;
            ID = MySpriteController.SpriteCount;
            Width = width;
            Height = height;
            MyImage = new SmartImage(MySpriteController, SpriteImage);
            MySpriteController.AddSprite(this);
        }
        /// <summary>
        /// Generate a new sprite.  It takes a width, height, and the duration in Milliseconds for each frame
        /// </summary>
        /// <param name="Controller">The sprite controller</param>
        /// <param name="SpriteImage">The image we pull the animations from</param>
        /// <param name="width">The width of one animation frame</param>
        /// <param name="height">the height of one animation frame</param>
        /// <param name="durationInMilliseconds">The number of milliseconds each frame is shown for as it animates.</param>
        public Sprite(SpriteController Controller, Image SpriteImage, int width, int height, int durationInMilliseconds)
        {
            MySpriteController = Controller;
            ID = MySpriteController.SpriteCount;
            Width = width;
            Height = height;
            MyImage = new SmartImage(MySpriteController, SpriteImage, width, height, durationInMilliseconds);
            MySpriteController.AddSprite(this);
        }
        /// <summary>
        /// Create a Sprite from an animation image, specifying the number of consecutive 
        /// frames to grab.
        /// </summary>
        /// <param name="Start">A point on the specified image where we begin grabbing frames</param>
        /// <param name="Controller">The Sprite controller we are associating the sprite with</param>
        /// <param name="SpriteImage">An image that we grab the frames from</param>
        /// <param name="width">The width of one frame</param>
        /// <param name="height">The height of one frame</param>
        /// <param name="duration">The number of milliseconds each frame is displayed for</param>
        /// <param name="Count">The number of frames to grab as a part of this animation</param>
        public Sprite(Point Start, SpriteController Controller, Image SpriteImage, int width, int height, int duration, int Count)
        {
            MySpriteController = Controller;
            ID = MySpriteController.SpriteCount;
            Width = width;
            Height = height;
            MyImage = new SmartImage(Start, MySpriteController, SpriteImage, width, height, duration, Count);
            MySpriteController.AddSprite(this);
        }

        /// <summary>
        /// Create a Sprite that is based off of the specified sprite.  Clone the Sprite except that
        /// we set SpriteName = "" and OrigSpriteName = the OldSprite.SpriteName.  That way we know that
        /// the sprite was duplicated from the original, and we can still distinguish the original from
        /// the duplicate.
        /// </summary>
        /// <param name="OldSprite">The Sprite to make a copy of</param>
        /// <param name="RetainName">If we want to set this sprite name to be that of the original.  This is a terrible idea.  Never do it.</param>
        public Sprite(Sprite OldSprite, bool RetainName = false)
        {
            MySpriteController = OldSprite.MySpriteController;
            ID = MySpriteController.SpriteCount;
            Width = OldSprite.Width;
            Height = OldSprite.Height;
            MyImage = OldSprite.MyImage;
            MovementSpeed = OldSprite.MovementSpeed;
            SpriteOriginName = OldSprite.SpriteName;

            //duplicate any eventhandlers we may have added to the one being cloned.
            SpriteHitsPictureBox += OldSprite.SpriteHitsPictureBox;
            SpriteExitsPictureBox += OldSprite.SpriteExitsPictureBox;
            SpriteHitsSprite += OldSprite.SpriteHitsSprite;
            SpriteAnimationComplete += OldSprite.SpriteAnimationComplete;
            SpriteInitializes += OldSprite.SpriteInitializes;
            CheckBeforeMove += OldSprite.CheckBeforeMove;
            Click += OldSprite.Click;
            ClickTransparent += OldSprite.ClickTransparent;
            SpriteChangesAnimationFrames += OldSprite.SpriteChangesAnimationFrames;
            SpriteArrivedAtEndPoint += OldSprite.SpriteArrivedAtEndPoint;
            SpriteArrivedAtWaypoint += OldSprite.SpriteArrivedAtWaypoint;
            SpriteBeingDestroyed += OldSprite.SpriteBeingDestroyed;

            if (RetainName)
                SpriteName = OldSprite.SpriteName;
            MySpriteController.AddSprite(this);
        }


        // *******************

        /// <summary>
        /// Give this sprite a name.  This way we can make a duplicate of it by specifying the name
        /// </summary>
        /// <param name="Name">A string that represents the new name of the sprite</param>
        public void SetName(string Name)
        {
            SpriteName = Name;
        }

        /// <summary>
        /// Add another animation to an existing Sprite.  After you add animations, you can use
        /// ChangeAnimation to select which animation you want the specified sprite to show.
        /// For example, you may want to have Animation 0 be a guy walking left, and animation 1 is
        /// that same guy walking right.  Because we do not specify the number of frames, it starts
        /// at the top-left corner and grabs as many frames as it can from the image.
        /// </summary>
        /// <param name="SpriteImage">The animation image to grab the frames from</param>
        /// <param name="width">The width of each frame</param>
        /// <param name="height">The height of each frame</param>
        public void AddAnimation(Image SpriteImage, int width, int height)
        {
            MyImage.AddAnimation(SpriteImage);
        }

        /// <summary>
        /// Add another animation to an existing Sprite.  After you add animations, you can use
        /// ChangeAnimation to select which animation you want the specified sprite to show.
        /// For example, you may want to have Animation 0 be a guy walking left, and animation 1 is
        /// that same guy walking right. Because we do not specify the number of frames, it starts
        /// at the top-left corner and grabs as many frames as it can from the image.
        /// </summary>
        /// <param name="SpriteImage">The animation image to grab the frames from</param>
        /// <param name="width">The width of each frame</param>
        /// <param name="height">The height of each frame</param>
        /// <param name="duration">The time in milliseconds we use for each frame</param>
        public void AddAnimation(Image SpriteImage, int width, int height, int duration)
        {
            MyImage.AddAnimation(SpriteImage, width, height, duration);
        }

        /// <summary>
        /// Add another animation to an existing Sprite.  After you add animations, you can use
        /// ChangeAnimation to select which animation you want the specified sprite to show.
        /// For example, you may want to have Animation 0 be a guy walking left, and animation 1 is
        /// that same guy walking right. Because we do not specify the number of frames, it starts
        /// at the top-left corner and grabs as many frames as it can from the image.
        /// </summary>
        /// <param name="SpriteImage">The animation image to grab the frames from</param>
        /// <param name="width">The width of each frame</param>
        /// <param name="height">The height of each frame</param>
        /// <param name="duration">The time in milliseconds we use for each frame</param>
        /// <param name="Count">The number of frames we grab from the image</param>
        /// <param name="Start">The starting position on the Image where we grab the first frame</param>
        public void AddAnimation(Point Start, Image SpriteImage, int width, int height, int duration, int Count)
        {
            MyImage.AddAnimation(Start, SpriteImage, width, height, duration, Count);
        }

        /// <summary>
        /// Duplicate an animation, except rotated by the specified number of degrees.  For example, if you have
        /// a single animation (0), and you want to rotate it by 90 degrees, it will create animation 1 with that
        /// rotation to it.  In the long haul, generating a few rotated animations is less memory intensive than
        /// rotating it on demand.
        /// </summary>
        /// <param name="AnimationToCopy">An integer value specifying the animation to duplicate</param>
        /// <param name="RotationDegrees">The amount of counter-clockwise rotation to add</param>
        public void AddAnimation(int AnimationToCopy, int RotationDegrees)
        {
            if (AnimationToCopy < 0) return;
            if (AnimationToCopy > MyImage.AnimationCount) return;
            MyImage.AddAnimation(AnimationToCopy, RotationDegrees);
        }
        /// <summary>
        /// Duplicate an animation, except rotated by the specified number of degrees.  For example, if you have
        /// a single animation (0), and you want to rotate it by 90 degrees, it will create animation 1 with that
        /// rotation to it.  In the long haul, generating a few rotated animations is less memory intensive than
        /// rotating it on demand.
        /// </summary>
        /// <param name="AnimationToCopy">An integer value specifying the animation to duplicate</param>
        /// <param name="MirrorHorizontal">A boolean, stating if we should mirror horizontally</param>
        /// <param name="MirrorVertical">A boolean, stating if we should mirror vertically</param>
        public void AddAnimation(int AnimationToCopy, bool MirrorHorizontal, bool MirrorVertical)
        {
            if (AnimationToCopy < 0) return;
            if (AnimationToCopy > MyImage.AnimationCount) return;
            MyImage.AddAnimation(AnimationToCopy, MirrorHorizontal, MirrorVertical);
        }

        /// <summary>
        /// Start a new animation, but do it just once.  You can use AnimateJustAFewTimes(1) to the same effect.
        /// Or, you can use AnimateJustAFewTimes with a different number.  The SpriteAnimationComplete event will
        /// fire off when the animation completes.  The variable, Sprite.AnimationDone will be true once the 
        /// animation finishes animating.
        /// </summary>
        /// <param name="WhichAnimation">The animation index you want to use</param>
        public void AnimateOnce(int WhichAnimation)
        {
            AnimateJustAFewTimes(WhichAnimation, 1);
        }

        /// <summary>
        /// Start a new animation.  It will complete the animation the number of times you specify.
        /// For example, if your sprite is walking, and one animation is one step, specifying 4 here
        /// will result in your sprite taking 4 steps and then the animation stops.  You will want
        /// to make sure you are checking for when the animation stops, using the SpriteAnimationComplete event,
        /// checking the Sprite.AnimationDone flag.
        /// </summary>
        /// <param name="WhichAnimation">The animation index you want to use</param>
        /// <param name="HowManyAnimations">The number of animations to do before it stops</param>
        public void AnimateJustAFewTimes(int WhichAnimation, int HowManyAnimations)
        {
            if (WhichAnimation > -1 && WhichAnimation < MyImage.AnimationCount)
            { //We have a valid animation.  Do it once
                SetToAnimateOnce = true;
                AnimationNumberCount = HowManyAnimations;
                //Console.WriteLine("Setting to animate: " + HowManyAnimations);
                AnimationDone = false;
                AnimationIndex = WhichAnimation;
                FrameIndex = 0;
                ForceRedraw = true;
                LastResetImage = DateTime.UtcNow;
            }

        }
        /// <summary>
        /// Start a new animation index from scratch
        /// </summary>
        /// <param name="WhichAnimation">The animation index you want to use</param>
        /// <param name="StartFrame">The first frame you want to start the animation at.</param>
        public void ChangeAnimation(int WhichAnimation, int StartFrame = 0)
        {
            if (WhichAnimation > -1 && WhichAnimation < MyImage.AnimationCount)
            { //We have a valid animation.  Do it once
                SetToAnimateOnce = false;
                AnimationDone = false;
                AnimationIndex = WhichAnimation;
                FrameIndex = 0;
                int NumFrames = MyImage.AnimationFrameCount(WhichAnimation);
                if (StartFrame >= 0 && StartFrame <= NumFrames)
                    FrameIndex = StartFrame;
                ForceRedraw = true;
                LastResetImage = DateTime.UtcNow; //start from this second.
            }
        }
        /// <summary>
        /// Change the animation speed of a particular animation.  This looks at the first frame
        /// and compares that frame to the speed specified.  It adjusts all the animations by the
        /// same percentage.
        /// </summary>
        /// <param name="WhichAnimation">The integer representing the animation to change</param>
        /// <param name="newSpeed">The speed in milliseconds for the new animation</param>
        public void ChangeAnimationSpeed(int WhichAnimation, int newSpeed)
        {
            if (WhichAnimation > -1 && WhichAnimation < MyImage.AnimationCount)
            { //We have a valid animation
                TimeSpan CurrentTS = MyImage.GetCurrentDuration(WhichAnimation, 0);
                double Current = CurrentTS.TotalMilliseconds;
                double New = 1; //This is 1/2 times slower.  1 is the actual speed...
                if ((int)Current != newSpeed)
                {
                    //We have a different speed
                    if (Current == 0) Current = 10000; //A duration of zero should not rotate
                    if (newSpeed == 0)
                        New = 1;
                    else
                    {
                        New = Current / newSpeed; //We have a ratio.
                    }
                }
                //Console.WriteLine("New SpeedAdjust: " + New.ToString());
                SpeedAdjust = New;
            }
        }

        /// <summary>
        /// Change the animation speed of a specific frame.  Beware.  This affects every sprite using this frame
        /// </summary>
        /// <param name="WhichAnimation">The index of the animation</param>
        /// <param name="WhichFrame">The index of the frame within the animation</param>
        /// <param name="newSpeed">The new frame duration in milliseconds</param>
        public void ChangeFrameAnimationSpeed(int WhichAnimation, int WhichFrame, int newSpeed)
        {
            if (WhichAnimation > -1 && WhichAnimation < MyImage.AnimationCount)
            {
                Animation tAnimation = MyImage.getAnimation(WhichAnimation);
                if (WhichFrame < 0 || WhichFrame >= tAnimation.Frames.Count) return;
                tAnimation.Frames[WhichFrame].Duration = TimeSpan.FromMilliseconds(newSpeed);
            }
        }

        /// <summary>
        /// Get the animation speed of a single frame.
        /// </summary>
        /// <param name="WhichAnimation">The animation we are looking at</param>
        /// <param name="WhichFrame">The index of the frame we wish to get the speed of</param>
        /// <returns>-1 if either index is out of range.  Otherwise, return the total milliseconds of the specified frame.</returns>
        public int GetFrameAnimationSpeed(int WhichAnimation, int WhichFrame)
        {
            if (WhichAnimation > -1 && WhichAnimation < MyImage.AnimationCount)
            {
                Animation tAnimation = MyImage.getAnimation(WhichAnimation);
                if (WhichFrame < 0 || WhichFrame >= tAnimation.Frames.Count) return -1;
                return (int)tAnimation.Frames[WhichFrame].Duration.TotalMilliseconds;
            }
            return -1;
        }

        /// <summary>
        /// Return the animation speed of this particualar animation of the sprite.
        /// </summary>
        /// <param name="WhichAnimation">The animation we are looking at</param>
        /// <returns>The speed which was set.  The speed is calculated in pixels per amount of time.  A higher number is faster than a lower number</returns>
        public int GetAnimationSpeed(int WhichAnimation)
        {
            if (WhichAnimation > -1 && WhichAnimation < MyImage.AnimationCount)
            {
                TimeSpan CurrentTS = MyImage.GetCurrentDuration(WhichAnimation, 0);
                return (int)(CurrentTS.TotalMilliseconds / SpeedAdjust);
            }
            return -1;
        }
        private void EraseMe(bool SkipInvalidate = false)
        {
            EraseMe(MyRectangle.Location, SkipInvalidate);
        }
        private void EraseMe(Point tLocation, bool SkipInvalidate = false)
        {
            Image ChangedImage = MySpriteController.BackgroundImage; //This is the image itself.  Changes to this affect what is displayed
            Image OriginalImage = MySpriteController.OriginalImage;
            System.Drawing.Rectangle oldPlace = new System.Drawing.Rectangle(tLocation.X, tLocation.Y, Width, Height);
            
            using(Graphics gx = Graphics.FromImage(ChangedImage))
            {
                gx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                gx.DrawImage(OriginalImage, oldPlace, oldPlace, GraphicsUnit.Pixel);
            }
            if (!SkipInvalidate)
                MySpriteController.Invalidate(oldPlace);
            //Tell any sprite we overlap with that it needs to redraw
            foreach (Sprite CollidesWith in CollisionSprites)
            {
                CollidesWith.RedrawMeAndAllICollideWith();
            }
        }

        private void RedrawMeAndAllICollideWith()
        {
            if (NeedsDrawingAtEndOfTick) return; //we are already doing this
            NeedsDrawingAtEndOfTick = true;
            //if (Opacity != 1)
                EraseMe(true);
            //foreach (Sprite CollidesWith in CollisionSprites)
            //{
            //    CollidesWith.RedrawMeAndAllICollideWith();
            //}
        }

        private void DrawMe(bool SkipInvalidate = false, bool ActuallyDraw = false)
        {
            DrawMe(MyRectangle.Location, SkipInvalidate, ActuallyDraw);
        }
        private void DrawMe(Point tLocation, bool SkipInvalidate = false, bool ActuallyDraw = false)
        {
            if (ActuallyDraw)
            {
                Image ChangedImage = MySpriteController.BackgroundImage; //This is the image itself.  Changes to this affect what is displayed
                System.Drawing.Rectangle ThePlace = new System.Drawing.Rectangle(tLocation.X, tLocation.Y, MyRectangle.Width, MyRectangle.Height);

                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = Opacity;
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                Image MeImage = MyImage.GetImage(AnimationIndex, FrameIndex, GetVisibleSize);
                if (MirrorHorizontally || MirrorVertically)
                {
                    MeImage = (Bitmap)MeImage.Clone();
                    if (MirrorHorizontally) MeImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    if (MirrorVertically) MeImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
                }
                GraphicsUnit tUnit = GraphicsUnit.Pixel;
                if (Rotation != 0 && MeImage != null)
                {
                    Image rotatedImage = new Bitmap(MeImage.Width, MeImage.Height);
                    using (Graphics g = Graphics.FromImage(rotatedImage))
                    {
                        g.TranslateTransform(MeImage.Width / 2, MeImage.Height / 2); //set the rotation point as the center into the matrix
                        g.RotateTransform(Rotation); //rotate
                        g.TranslateTransform(-MeImage.Width / 2, -MeImage.Height / 2); //restore rotation point into the matrix
                        g.DrawImage(MeImage, MeImage.GetBounds(ref tUnit), rotatedImage.GetBounds(ref tUnit), tUnit); //draw the image on the new bitmap
                        
                    }
                    //GC.Collect();
                    MeImage = rotatedImage;
                }
                if (MeImage != null)
                {
                    //g.DrawImage(MeImage, new Rectangle(0, 0, MeImage.Width, MeImage.Height), 0, 0, rotatedImage.Width, rotatedImage.Height, tUnit, attributes); //draw the image on the new bitmap
                    //Graphics.FromImage(ChangedImage).DrawImage(MeImage, ThePlace, MeImage.GetBounds(ref tUnit), tUnit);
                    Graphics.FromImage(ChangedImage).DrawImage(MeImage, ThePlace, 0, 0, MeImage.Width, MeImage.Height, tUnit, attributes); //draw the image on the new bitmap
                    //Pen tmpPen = new Pen(Color.Black);
                    //Graphics.FromImage(ChangedImage).DrawRectangle(tmpPen, MyRectangle);
                    MySpriteController.Invalidate(MyRectangle);
                }
                NeedsDrawingAtEndOfTick = false;
            }
            else
            {
                NeedsDrawingAtEndOfTick = true;
            }
        }

        /// <summary>
        /// Actually draw the Sprite.  Never use this.  It is used by the SpriteController
        /// </summary>
        internal void ActuallyDraw()
        {
            if (NeedsDrawingAtEndOfTick)
                DrawMe(false, true);
        }

        /// <summary>
        /// Put the Sprite at a specified location, using the dimentions of the BackgroundImage.
        /// Unless you are using coordinates you have gotten from a mouse-click, this is how you want
        /// to place a Sprite somewhere.  It is the easiest way to track things.  But, if you are
        /// doing something using mouse-click coordinates, you want to use PutPictureBoxLocation
        /// </summary>
        /// <param name="NewLocationOnImage">The new point on the Image</param>
        public void PutBaseImageLocation(Point NewLocationOnImage)
        {
            Point PointOnPictureBox = MySpriteController.ReturnPictureBoxAdjustedPoint(NewLocationOnImage);
            if (MyRectangle.Location != NewLocationOnImage || !HasBeenDrawn)
            {
                if (HasBeenDrawn)
                    EraseMe();
                xPositionOnImage = NewLocationOnImage.X;
                yPositionOnImage = NewLocationOnImage.Y;
                xPositionOnPictureBox = PointOnPictureBox.X;
                yPositionOnPictureBox = PointOnPictureBox.Y;
                xOnVector = NewLocationOnImage.X;
                yOnVector = NewLocationOnImage.Y;
                DrawMe(NewLocationOnImage);
                HasBeenDrawn = true;
            }
        }
        /// <summary>
        /// Put the Sprite at a specified location, using the dimentions of the BackgroundImage.
        /// Unless you are using coordinates you have gotten from a mouse-click, this is how you want
        /// to place a Sprite somewhere.  It is the easiest way to track things.  But, if you are
        /// doing something using mouse-click coordinates, you want to use PutPictureBoxLocation
        /// </summary>
        /// <param name="X">The X location on the background image</param>
        /// <param name="Y">the Y location on the background image</param>
        public void PutBaseImageLocation(double X, double Y)
        {
            Point NewLocation = new Point((int)X, (int)Y);
            Point actualPoint = MySpriteController.ReturnPictureBoxAdjustedPoint(NewLocation);
            if (MyRectangle.Location != NewLocation || !HasBeenDrawn)
            {
                if (HasBeenDrawn)
                    EraseMe();
                xPositionOnImage = NewLocation.X;
                yPositionOnImage = NewLocation.Y;
                xPositionOnPictureBox = actualPoint.X;
                yPositionOnPictureBox = actualPoint.Y;
                xOnVector = X;
                yOnVector = Y;
                DrawMe();
                HasBeenDrawn = true;
            }
        }

        /// <summary>
        /// Put the Sprite at a specified location, using the dimentions of the PictureBox.
        /// You want to use this if you got your X/Y position from a mouse-click.  Otherwise,
        /// this is the harder way to track things, particularly if your window can resize.  Use
        /// PutBaseImageLocation instead.
        /// </summary>
        /// <param name="NewLocationOnPictureBox">A point on the PictureBox</param>
        public void PutPictureBoxLocation(Point NewLocationOnPictureBox)
        {
            //We adjust to the location
            Point actualPoint = MySpriteController.ReturnPointAdjustedForImage(NewLocationOnPictureBox);
            PutBaseImageLocation(actualPoint);
            xPositionOnPictureBox = NewLocationOnPictureBox.X;
            yPositionOnPictureBox = NewLocationOnPictureBox.Y;
        }

        /// <summary>
        /// Done when the box resizes.  We need to recompute the picturebox location.  The resize function
        /// automatically calls this.  You should never need to do so.
        /// </summary>
        public void RecalcPictureBoxLocation()
        {
            Point nPoint = MySpriteController.ReturnPictureBoxAdjustedPoint(new Point(xPositionOnImage, yPositionOnImage));
            xPositionOnPictureBox = nPoint.X;
            yPositionOnPictureBox = nPoint.Y;
        }

        private void SetupForDrawing()
        {
            //We make sure we have the correct index for the image
            TimeSpan duration = DateTime.UtcNow - LastResetImage;
            if (PausedAnimation)
            {
                TimeSpan newduration = DateTime.UtcNow - PausedAnimationTime;
                LastResetImage = LastResetImage + newduration;
                PausedAnimationTime = DateTime.UtcNow;
                duration = DateTime.UtcNow - LastResetImage;
            }
            double orig = duration.TotalMilliseconds;
            if (SpeedAdjust != 1)
            {
                double Adjust = duration.TotalMilliseconds * SpeedAdjust;
                //Console.WriteLine(SpriteName + " " + SpriteOriginName + " " + orig.ToString() + " " + Adjust.ToString());
                duration = TimeSpan.FromMilliseconds(Adjust);
            }
            if (MyImage.NeedsNewImage(AnimationIndex, FrameIndex, duration) || ForceRedraw)
            {
                EraseMe();
                bool AtEnd = false;
                int oldindex = FrameIndex;
                if (SetToAnimateOnce && AnimationNumberCount <= 1) AtEnd = true;
                //Console.Write("Changing Animation Frame: " + FrameIndex);
                TimeSpan difference = MyImage.ChangeIndex(AnimationIndex, ref _FrameIndex, duration, AtEnd);
                //Console.WriteLine("  To: " + FrameIndex);
                SpriteChangesAnimationFrames(this, new SpriteEventArgs());
                LastResetImage = DateTime.UtcNow - difference;
                if (SetToAnimateOnce)
                {
                    if ((FrameIndex == 0 && oldindex != FrameIndex) || MyImage.AnimationDone(AnimationIndex, FrameIndex, SetToAnimateOnce))
                    {
                        //Console.WriteLine("We are at the end frame:");
                        TimeSpan HowLong = MyImage.GetCurrentDuration(AnimationIndex, FrameIndex);
                        if (difference > HowLong || FrameIndex == 0)
                        {
                            AnimationNumberCount--;
                            //Console.WriteLine("One animation done.  Now we have: " + AnimationNumberCount);
                            if (AnimationNumberCount < 1)
                            {
                                AnimationDone = true;
                                if (!PausedEvents)
                                    SpriteAnimationComplete(this, new SpriteEventArgs());
                            }
                        }
                    }
                }

                DrawMe();
                ForceRedraw = false; //We have done a redraw.  No need to do it every time
            }
        }

        /// <summary>
        /// This is run from the sprite controller every 10 miliseconds.  You should never
        /// need to call this yourself.
        /// </summary>
        internal void Tick()
        {
            if (!SpriteHasInitialized)
            {
                SpriteHasInitialized = true;
                SpriteInitializes(this, new SpriteEventArgs());
            }
            if (HasBeenDrawn)
            {
                SetupForDrawing();
                TryToMove();
            }
        }

        /// <summary>
        /// Return the point that this sprite needs to be shooting for, for the center of this sprite to 
        /// hit the center of the destination sprite.
        /// </summary>
        /// <param name="destination">The sprite we are shooting for trying to hit</param>
        /// <returns>A point which allows the moving sprite to collide with the destination sprite.</returns>
        private Point GetMoveToSpritePoint(Sprite destination)
        {
            if (destination.Destroying) return new Point(-1, -1);
            Point DestCenter = destination.GetSpriteBaseImageCenter();
            Point targetPoint = new Point(DestCenter.X - Width / 2, DestCenter.Y - Height / 2);
            return targetPoint;
        }

        private void TryToMove()
        {
            if (AutomaticallyMoves && MovementSpeed > 1)
            {
                //Now, we move it.
                //Take 1 sec and divide it by the speed.  That is how many ms we need before we move

                double MS = 50 / MovementSpeed;
                if (MS < 2) MS = 2;
                TimeSpan Elapsed = DateTime.UtcNow - LastMovement;
                if (PausedMovement)
                {
                    TimeSpan newduration = DateTime.UtcNow - PausedMovementTime;
                    LastMovement = LastMovement + newduration;
                    PausedMovementTime = DateTime.UtcNow;
                    Elapsed = DateTime.UtcNow - LastMovement;
                }
                if (Elapsed.TotalMilliseconds > MS)
                {
                    if(MovingToSprite != null && !MovingToSprite.Destroying)
                    {
                        MovementDestinations.Clear();
                        Point TargetPoint = GetMoveToSpritePoint(MovingToSprite);
                        MovementDestinations.Add(TargetPoint);
                    }
                    else if(MovingToSprite != null && MovingToSprite.Destroying)
                    {
                        MovingToSprite = null; //We stop shooting for the location of the sprite.
                        //We are still shooting for where the sprite used to be.
                    }
                    if (MovingToPoint && MovementDestinations.Count > 0)
                        SetSpriteDirectionToPoint(MovementDestinations[0]); //recalculate to decrease error
                    double MovementDelta = Elapsed.TotalMilliseconds / MS;
                    double newX = xOnVector + (MovementDelta * SpriteVector.X);
                    double newY = yOnVector + (MovementDelta * SpriteVector.Y);
                    //if(double.IsNaN(newX) || newX > 1000 || newX < -1000)
                    //{
                    //    Console.WriteLine("Major issue. NAN:   xOnVecor:" + xOnVector.ToString() + " MovementDelta:" + MovementDelta.ToString() + " SpriteVec:" + SpriteVector.X.ToString());
                    //}
                    //if (double.IsNaN(newY) || newY > 1000 || newY < -1000)
                    //{
                    //    Console.WriteLine("Major issue. NAN:   yOnVecor:" + yOnVector.ToString() + " MovementDelta:" + MovementDelta.ToString() + " SpriteVec:" + SpriteVector.Y.ToString());
                    //}
                    if (CannotMoveOutsideBox)
                    {
                        double tNewx = newX;
                        double tNewy = newY;
                        Image tImage = MySpriteController.BackgroundImage;
                        if (newX < -1) newX = -1;
                        if (newY < -1) newY = -1;
                        if (newX > tImage.Width - Width) newX = (tImage.Width - Width) + 1;
                        if (newY > tImage.Height - Height) newY = (tImage.Height - Height) + 1;
                        if (tNewx != newX || tNewy != newY && MovingToPoint)
                        {
                            //We are not allowed to go outside the box, but our point is outside the box.  Cancel
                            CancelMoveTo();
                        }
                    }
                    SpriteEventArgs e = new SpriteEventArgs();
                    e.NewLocation = new Point((int)Math.Round(newX), (int)Math.Round(newY));
                    if (MovingToPoint && MovementDestinations.Count > 0)
                    {
                        //do not go past the destination point
                        if (SpriteVector.X < 0 && newX < MovementDestinations[0].X) newX = MovementDestinations[0].X;
                        if (SpriteVector.X > 0 && newX > MovementDestinations[0].X) newX = MovementDestinations[0].X;
                        if (SpriteVector.X == 0) newX = MovementDestinations[0].X;
                        if (SpriteVector.Y < 0 && newY < MovementDestinations[0].Y) newY = MovementDestinations[0].Y;
                        if (SpriteVector.Y > 0 && newY > MovementDestinations[0].Y) newY = MovementDestinations[0].Y;
                        if (SpriteVector.Y == 0) newY = MovementDestinations[0].Y;
                        //Check to see if we have hit the waypoint
                        //Console.WriteLine("Heading to: " + MovementDestinations[0].ToString() + " At: " + newX.ToString() + " " + newY.ToString());
                        if ((int)newX == MovementDestinations[0].X && (int)newY == MovementDestinations[0].Y)
                        {
                            //check to see if we have hit the endpoint
                            MovementDestinations.RemoveAt(0); //Yank the destination
                            if (MovementDestinations.Count == 0)
                            {
                                CancelMoveTo();
                                SpriteArrivedAtEndPoint(this, e);
                            }
                            else
                            {
                                SpriteArrivedAtWaypoint(this, e);
                                SetSpriteDirectionToPoint(MovementDestinations[0]);
                            }
                        }
                    }
                    else if (MovingToPoint) {
                        CancelMoveTo();
                    }
                    if (!PausedEvents)
                        CheckBeforeMove(this, e); //See if there is any code to let us go or not go
                    //if(e.Cancel)
                    //{
                    //    Console.WriteLine("canceled. " + newX.ToString() + "  " + newY.ToString() + " " + MovementDelta.ToString());
                    //}
                    if (!e.Cancel)
                    {
                        //This allows our 'check before move'function to adjust the destination.
                        PutBaseImageLocation(e.NewLocation);
                    }
                    LastMovement = DateTime.UtcNow;
                    CheckForEvents();
                }
            }
        }

        /// <summary>
        /// Resize the sprite using the base image coordinates.  The width and height specified
        /// are relative to the size of the background image, not the picturebox.
        /// </summary>
        /// <param name="NewSize">The size (width, height) to make the sprite</param>
        public void SetSize(Size NewSize)
        {
            if (NewSize.Width == MyRectangle.Width && NewSize.Height == MyRectangle.Height)
                return;  //No need to do anything if we are not making changes
            if (HasBeenDrawn)
            {
                EraseMe(); //Erase ourselves
            }
            Width = NewSize.Width;
            Height = NewSize.Height;
            if (HasBeenDrawn)
            {
                DrawMe();
            }
        }

        /// <summary>
        /// Tell the sprite to kill itself.  It will erase itself and then
        /// be removed from the spritelist.  Then it will be gone forever.
        /// </summary>
        public void Destroy()
        {
            //If we are not already destroying ourselves
            if (!_Destroying)
            {
                //Mark ourselves as being destroyed
                _Destroying = true;
                //Erase ourselves
                EraseMe();
                //Remove ourselves from the controller (and the tick process)
                SpriteBeingDestroyed(this, new SpriteEventArgs()); 
                MySpriteController.DestroySprite(this);
            }
        }

        /// <summary>
        /// Remove the sprite from the field.  This does not destroy the sprite.  It simply removes it from action.
        /// Use UnhideSprite to show it again.
        /// </summary>
        public void HideSprite()
        {
            EraseMe();
            HasBeenDrawn = false;
        }


        /// <summary>
        /// Make the sprite reappear.  If you have not positioned it yet, it will show up at the top corner.  It is best to only
        /// use this when you have hidden it using HideSprite
        /// </summary>
        public void UnhideSprite()
        {
            DrawMe();
            HasBeenDrawn = true;
        }
        /// <summary>
        /// Return true or false, asking if the specifiec sprite is at the point on the picturebox.
        /// You can use this with a mouse-click to see if you are clicking on a sprite.  Use the 
        /// SpriteCollisionMethod "transparent" to see if you have clicked on an actual pixel of the 
        /// sprite instead of just within the sprite rectangle.
        /// </summary>
        /// <param name="location">The x and y location in ImageBox coordinates.</param>
        /// <param name="method">The method of determining if the sprite is at that position</param>
        /// <returns>True if the sprite is at the specified location, false if it is not</returns>
        public bool SpriteAtPictureBoxPoint(Point location, SpriteCollisionMethod method = SpriteCollisionMethod.rectangle)
        {
            //Translate the position to a position on the drawing pane
            SpriteAdjustmentRatio PictureBoxRatio = MySpriteController.ReturnAdjustmentRatio();
            int x = (int)(location.X / PictureBoxRatio.width_ratio);
            int y = (int)(location.Y / PictureBoxRatio.height_ratio);

            //The x,y is now the pixel in the sprite.
            return SpriteAtImagePoint(new Point(x, y), method);
        }

        /// <summary>
        /// Because sprites are scaled (shrunk or stretched), this function finds the point
        /// within the sprite that is specified by the location.  this function is used by
        /// a number of internal processes, but may be useful to you.  But probably not.
        /// </summary>
        /// <param name="location">A point given in Image coordinates</param>
        /// <returns>A point within the pixel that can be used to find a particular pixel in a sprite.</returns>
        public Point SpriteAdjustedPoint(Point location)
        {
            Point internalPoint = new Point(location.X - MyRectangle.Location.X, location.Y - MyRectangle.Location.Y);
            SpriteAdjustmentRatio Ratio = ReturnAdjustmentRatio();
            Point AdjustedPoint = new Point((int)(internalPoint.X / Ratio.width_ratio), (int)(internalPoint.Y / Ratio.height_ratio));
            return AdjustedPoint;
        }

        /// <summary>
        /// Check to see if the sprite exists at the point specified.  The point given is
        /// in coordinates used by the image (not the PictureBox, use SpriteAtPictureBox for that)
        /// </summary>
        /// <param name="location">An imagebox location</param>
        /// <param name="method">the method to use to determine if the image is there</param>
        /// <returns>true if the sprite is at that position, false if it is not</returns>
        public bool SpriteAtImagePoint(Point location, SpriteCollisionMethod method = SpriteCollisionMethod.rectangle)
        {
            if (location.X < MyRectangle.Location.X) return false;
            if (location.X > MyRectangle.Location.X + MyRectangle.Width) return false;
            if (location.Y < MyRectangle.Location.Y) return false;
            if (location.Y > MyRectangle.Location.Y + MyRectangle.Height) return false;

            Point internalPoint = new Point(location.X - MyRectangle.Location.X, location.Y - MyRectangle.Location.Y);
            SpriteAdjustmentRatio Ratio = ReturnAdjustmentRatio();
            Point AdjustedPoint = SpriteAdjustedPoint(location);

            if (method == SpriteCollisionMethod.transparency)
            {
                Bitmap tImage = (Bitmap)GetImage();
                //Check the point within the sprite
                if (AdjustedPoint.X < 0 || AdjustedPoint.X >= tImage.Width) return false;
                if (AdjustedPoint.Y < 0 || AdjustedPoint.Y >= tImage.Height) return false;
                Color OneSpace = tImage.GetPixel(AdjustedPoint.X, AdjustedPoint.Y);
                if (OneSpace.A == 0)
                    return false; //It is transparent.  No collision at this point.  255 is solid
                return true;
            }
            if (method == SpriteCollisionMethod.circle)
            {

                Point center = new Point(
                  MyRectangle.Width / 2,
                  MyRectangle.Height / 2);

                double _xRadius = MyRectangle.Width / 2;
                double _yRadius = MyRectangle.Height / 2;


                if (_xRadius <= 0.0 || _yRadius <= 0.0)
                    return false;
                /* This is a more general form of the circle equation
                 *
                 * X^2/a^2 + Y^2/b^2 <= 1
                 */

                Point normalized = new Point(location.X - center.X,
                                             location.Y - center.Y);

                return ((double)(normalized.X * normalized.X)
                         / (_xRadius * _xRadius)) + ((double)(normalized.Y * normalized.Y) / (_yRadius * _yRadius))
                    <= 1.0;
            }

            return true;
        }


        /// <summary>
        /// return the current image frame.  Warning:  If you write to this image, it will
        /// affect all sprites using this frame.
        /// </summary>
        /// <returns>An image that is the current sprite frame for the current animation</returns>
        public Image GetImage()
        {
            if (AnimationIndex < 0) AnimationIndex = 0;
            if (FrameIndex < 0) FrameIndex = 0;
            return MyImage.GetImage(AnimationIndex, FrameIndex, GetVisibleSize);
        }

        /// <summary>
        /// return the frame for the given index.  Warning:  If you write to this image, it will
        /// affect all sprites using this frame.
        /// </summary>
        /// <param name="Animation_Index">The Animation index we are trying to find</param>
        /// <param name="Frame_Index">The Frame index we are trying to find</param>
        /// <returns>An image that is the current sprite frame for the current animation</returns>
        public Image GetImage(int Animation_Index, int Frame_Index)
        {
            if (Animation_Index < 0) Animation_Index = 0;
            if (Animation_Index > MyImage.AnimationCount) return null;
            if (Frame_Index < 0) Frame_Index = 0;
            if (Frame_Index > MyImage.AnimationFrameCount(Animation_Index)) return null;
            return MyImage.GetImage(Animation_Index, Frame_Index, GetVisibleSize);
        }


        /// <summary>
        /// Replace a sprite image.  It will replace the current frame unless you specify both an animation
        /// and the frame within the animation you wish to replace.  Warning:  This replaces the image_frame 
        /// for every sprite that uses that is based off the same image.
        /// </summary>
        /// <param name="newimage">The new image to use</param>
        /// <param name="animation">The animation you want to change</param>
        /// <param name="frame">The frame within the animation you want to change</param>
        public void ReplaceImage(Image newimage, int animation = -1, int frame = -1)
        {
            if (newimage == null) return; //do not replace it with nothing
            if (AnimationIndex < 0) AnimationIndex = 0;
            if (FrameIndex < 0) FrameIndex = 0;
            if (animation == -1) animation = AnimationIndex;
            if (frame == -1) frame = FrameIndex;
            MyImage.ReplaceImage(newimage, animation, frame);
        }

        /// <summary>
        /// Taking into consideration how the sprite is stretched or shrunk, it
        /// returns a SpriteAdjustmentRatio that can be used to work with the sprite
        /// itself.
        /// </summary>
        /// <returns>The current SpriteAdjustmentRatio used to display this sprite</returns>
        public SpriteAdjustmentRatio ReturnAdjustmentRatio()
        {
            SpriteAdjustmentRatio Ratio = new SpriteAdjustmentRatio();

            Image tImage = GetImage();
            //if(tImage == null) Console.WriteLine(this.ID.ToString());
            Ratio.width_ratio = (double)MyRectangle.Width / (double)tImage.Width;
            Ratio.height_ratio = (double)MyRectangle.Height / (double)tImage.Height;
            return Ratio;
        }

        /// <summary>
        /// Return true if the sprite can go to this point and still be on the drawing-board.
        /// </summary>
        /// <param name="newpoint">The point, given in pixels and corresponding to pixels on the picturebox</param>
        /// <returns>true or false</returns>
        public bool SpriteCanMoveOnPictureBox(Point newpoint)
        {
            return SpriteCanMoveOnImage(MySpriteController.ReturnPointAdjustedForImage(newpoint));
        }

        /// <summary>
        /// Return true if the sprite can go to this point and still be on the drawing-board.
        /// </summary>
        /// <param name="newpoint">The point, given in pixels and corresponding to pixels on the background image</param>
        /// <returns>true or false</returns>
        public bool SpriteCanMoveOnImage(Point newpoint)
        {
            Image tImage = MySpriteController.BackgroundImage;
            if (newpoint.X < 0) return false;
            if (newpoint.X + Width > tImage.Width) return false;
            if (newpoint.Y < 0) return false;
            if (newpoint.Y + Height > tImage.Height) return false;
            return true;
        }

        /// <summary>
        /// Move to where the destination sprite currently is at.  This is a dumb move.  It does not take into
        /// consideration the movement direction of the destination sprite.  So the moving sprite does need to be
        /// moving a bit faster than the sprite you are trying to hit for it to do so.
        /// </summary>
        /// <param name="Destination">The sprite we are trying to hit</param>
        public void MoveTo(Sprite Destination)
        {
            MovingToSprite = Destination;
            LastMovement = DateTime.UtcNow;
            MovementDestinations.Clear();
            Point TargetPoint = GetMoveToSpritePoint(Destination);
            MovementDestinations.Add(TargetPoint);
            MovingToPoint = true;
            SpriteReachedEndPoint = false;
            SetSpriteDirectionToPoint(TargetPoint);
        }

        /// <summary>
        /// Tell the Sprite to move towards a destination.  You need to give the sprite a MovementSpeed
        /// and tell the sprite that it can automatically move.  But the sprite will begin a journey towards
        /// that point at the MovementSpeed you have set.  When it gets to the point, the SpriteArrivedAtEndPoint event
        /// will fire off.  Also, the SpriteReachedEnd bool will be true.
        /// </summary>
        /// <param name="Destination">An image-point that the sprite will move to.</param>
        public void MoveTo(Point Destination)
        {
            List<Point> tList = new List<Point>();
            tList.Add(Destination);
            MoveTo(tList); //This way we only have one function to make.
        }

        /// <summary>
        /// Tell the sprite to move towards each point in turn.  The sprite will move in a straight line until the first point.
        /// From there it moves to the next point, until it has reached the last point.  Every time it reaches a point, the
        /// SpriteArrivedAtWaypoint event is triggered.  When it reaches the final point in the list, the SpriteArrivedAtEndPoint
        /// event is triggered.  While the sprite is moving, the SpriteReachedEndPoint attribute is set to false.  When it has
        /// arrived, it is set to true.
        /// </summary>
        /// <param name="DestinationList">A list of Image-Points that the sprite will follow, one after the other</param>
        public void MoveTo(List<Point> DestinationList)
        {
            MovingToSprite = null; //If we were trying to hit something, we are no longer trying to hit it.
            LastMovement = DateTime.UtcNow;
            if (DestinationList.Count == 0) CancelMoveTo(); //If we tell it to move nowhere, cancel it
            MovementDestinations.Clear();
            foreach(Point one in DestinationList)
                MovementDestinations.Add(one);
            MovingToPoint = true;
            SpriteReachedEndPoint = false;
            SetSpriteDirectionToPoint(DestinationList[0]);
        }

        /// <summary>
        /// Sets the Sprite Moving towards a given point.  You are responsible to do something with it once it gets there.
        /// If you want it to automatically stop upon reaching it, use MoveTo instead.  Actually, the MoveTo function works
        /// a lot better than this one.  Because of integer rounding and a few other things, this function is a little
        /// bit imprecise.  If you send it towards a point, it will go in that general direction.  The MoveTo function
        /// will perpetually recalculate its way to the destination point and actually reach that point.  SetSpriteDirectionToPoint
        /// will sort-of head in the direction of the point.  But MoveTo will go to that point.
        /// </summary>
        /// <param name="ImagePointDestination">The destination, based off a point on the background image, that we send the sprite towards.</param>
        public void SetSpriteDirectionToPoint(Point ImagePointDestination)
        {
            double x = ImagePointDestination.X - xPositionOnImage;
            double y = ImagePointDestination.Y - yPositionOnImage;
            double distance = Math.Sqrt(x *x + y * y);
            if (distance == 0) return; //No need to go anywhere.
            System.Windows.Vector newVec = new System.Windows.Vector(x / distance, y / distance);
            //if(double.IsNaN(newVec.X) || double.IsNaN(newVec.Y))
            //{
            //    Console.WriteLine("SetSpriteDirectionToPoint: Creating invalid vector " + distance);
            //}
            SetSpriteDirection(newVec);
            //Console.WriteLine("SetDirectionToPoint " + ImagePointDestination.ToString());
        }

        /// <summary>
        /// Cancel a MoveTo command.  The sprite will stop moving, and all the waypoints will be removed.
        /// </summary>
        public void CancelMoveTo()
        {
            SpriteReachedEndPoint = true;
            MovementDestinations.Clear();
            MovingToSprite = null; //If we were heading towards a sprite.  Stop doing so.
            if (!MovingToPoint) return; //We do not do anything if we are not moving.
            MovementSpeed = 0;
            //AutomaticallyMoves = false;
            SetSpriteDirectionDegrees(0);//Basically reset it to nothing
            MovingToPoint = false;
        }

        /// <summary>
        /// Given a "degree" (from 0 to 360, set the direction
        /// that the sprite moves automatically.  0 is right, 90 is up, 180 is left
        /// and 270 is down.
        /// </summary>
        /// <param name="AngleInDegrees">the degrees to use</param>
        public void SetSpriteDirectionDegrees(double AngleInDegrees)
        {
            //convert to Radians and set it with that
            double Radians = ConvertDegreesToRadians(AngleInDegrees);
            SetSpriteDirectionRadians(Radians);
        }

        /// <summary>
        /// Set the sprite direction using Radians.  Most people do not want to use this.
        /// Use SetSpriteDirectionDegrees instead unless you like math and know what you
        /// are doing with Radians.
        /// </summary>
        /// <param name="AngleInRadians">The angle in radians</param>
        public void SetSpriteDirectionRadians(double AngleInRadians)
        {
            //Turn it into a vector
            System.Windows.Vector newVector = new System.Windows.Vector((float)Math.Cos(AngleInRadians),
                                                                        -(float)Math.Sin(AngleInRadians));
            SetSpriteDirection(newVector);
        }

        /// <summary>
        /// Set the sprite direction using a vector.  The vector may contain
        /// a speed as well as the movement delta (amount of x shift, and amount
        /// of y shift.)  If so, this function may also affect the movement speed
        /// Most people prefer to use SetSpriteDirectionDegrees instead of using
        /// vectors.
        /// </summary>
        /// <param name="newVector">A vector</param>
        public void SetSpriteDirection(System.Windows.Vector newVector)
        {
            System.Windows.Vector oldVector = newVector;
            //use the specified vector
            if (Math.Round(newVector.Length) != 1)
            {
                double NewSpeed = Math.Round(newVector.Length);
                MovementSpeed = (int)NewSpeed;
            }
            newVector.Normalize();
            if (double.IsNaN(newVector.X) || double.IsNaN(newVector.Y))
            {
                //Console.WriteLine("SetSpriteDirection: Error setting direction.  " + oldVector.ToString());
                newVector = oldVector;
            }
            SpriteVector = newVector;
        }

        /// <summary>
        /// Convert a number from degrees to radians.
        /// </summary>
        /// <param name="Degrees">The number from 0 to 360 in degrees</param>
        /// <returns>The corresponding number converted to radians</returns>
        public double ConvertDegreesToRadians(double Degrees)
        {
            return (Math.PI / 180.0) * Degrees;
        }

        /// <summary>
        /// Convert a number from radians to degrees.
        /// </summary>
        /// <param name="Radians">The number of radians</param>
        /// <returns>The corresponding number in degrees</returns>
        public double ConvertRadiansToDegrees(double Radians)
        {
            double degrees = (180.0 / Math.PI) * Radians;
            if (Radians < 0) degrees += 360;
            return degrees;
        }

        /// <summary>
        /// Return the current vector that the sprite is moving along
        /// </summary>
        /// <returns>The current sprite vector</returns>
        public System.Windows.Vector GetSpriteVector()
        {
            return SpriteVector;
        }

        /// <summary>
        /// Returns the direction the sprite is currently traveling, using Radians.
        /// </summary>
        /// <returns>The direction in radians that the sprite is traveling in</returns>
        public double GetSpriteRadans()
        {
            //double radians = Math.Atan2((double)SpriteVector.Y, (double)SpriteVector.X);
            double radians;
            if(SpriteVector.Y > 0 && SpriteVector.X > 0)
            {
                //calculate it in the other quadrant and then adjust.
                radians = Math.Atan2((double)SpriteVector.Y, (double)SpriteVector.X);
                radians = (2 * Math.PI) - radians; 
            }
            else
                radians = Math.Atan2(-(double)SpriteVector.Y, (double)SpriteVector.X);
            return radians;
        }

        /// <summary>
        /// Get the direction that the sprite is traveling in in degrees.  You may want to
        /// use Math.Round on the results.  The value returned is usually just a tiny bit off
        /// from what you set it with.  For example, if you set the sprite movement direction
        /// to be 270 degrees (down), this function may return it as 269.999992.  Rounding the
        /// number will give it back to you at probably the same direction you set it as.
        /// </summary>
        /// <returns>A double (it has a decimal place) that represents the direction in degrees</returns>
        public double GetSpriteDegrees()
        {
            double radians = GetSpriteRadans();
            double degrees = ConvertRadiansToDegrees(radians);
            if (degrees < 0) degrees = 360 + degrees;
            return degrees;
        }

        /// <summary>
        /// Return the centerpoint of the sprite, as found on the background image
        /// </summary>
        /// <returns>a point with the x and y based off the background image location</returns>
        public Point GetSpriteBaseImageCenter()
        {
            Point corner = BaseImageLocation;
            return new Point(corner.X + Width / 2, corner.Y + Height / 2);
        }

        /// <summary>
        /// Return the centerpoint of the sprite, as found on the picturebox
        /// </summary>
        /// <returns>A point with the x and y found on the picturebox</returns>
        public Point GetSpritePictureboxCenter()
        {
            Point corner = PictureBoxLocation;
            return new Point(corner.X + VisibleWidth / 2, corner.Y + VisibleHeight / 2);
        }

        // ***************** Events ***********
        private bool CheckForHittingEdgeOfImage()
        {
            Image tImage = MySpriteController.BackgroundImage;
            bool outOfBounds = false;
            if (xPositionOnImage < 0) outOfBounds = true;
            if (xPositionOnImage + Width > tImage.Width) outOfBounds = true;
            if (yPositionOnImage < 0) outOfBounds = true;
            if (yPositionOnImage + Height > tImage.Height) outOfBounds = true;
            if (outOfBounds)
            {
                if (!PausedEvents)
                    SpriteHitsPictureBox(this, new SpriteEventArgs());
            }
            return outOfBounds;
        }
        private bool CheckForExitingImage()
        {
            Image tImage = MySpriteController.BackgroundImage;
            bool outOfBounds = false;
            if (xPositionOnImage + Width < 0) outOfBounds = true;
            if (xPositionOnImage > tImage.Width) outOfBounds = true;
            if (yPositionOnImage + Width < 0) outOfBounds = true;
            if (yPositionOnImage > tImage.Height) outOfBounds = true;
            if (outOfBounds)
            {
                if (!PausedEvents)
                    SpriteExitsPictureBox(this, new SpriteEventArgs());
            }
            return outOfBounds;
        }

        private bool CheckToSeeIfSpriteHitsSprite(Sprite target, SpriteCollisionMethod how)
        {
            if (target == this) return false; //We do not collide with ourselves.
            if (target.xPositionOnImage + target.Width < xPositionOnImage) return false;
            if (target.xPositionOnImage > xPositionOnImage + Width) return false;
            if (target.yPositionOnImage + target.Height < yPositionOnImage) return false;
            if (target.yPositionOnImage > yPositionOnImage + Height) return false;
            //If we get here, we have two rectangles ovelapping.
            if (how == SpriteCollisionMethod.circle)
            {

            }
            if (how == SpriteCollisionMethod.transparency)
            {

            }
            return true;
        }

        /// <summary>
        /// Check to see if the specified rectangle overlaps with the sprite.
        /// </summary>
        /// <param name="target">The rectangle we are looking to see if we hit</param>
        /// <returns>True if the rectangle overlaps the sprite rectabgle</returns>
        public bool SpriteIntersectsRectangle(Rectangle target)
        {
            if (target.X + target.Width < xPositionOnImage) return false;
            if (target.X > xPositionOnImage + Width) return false;
            if (target.Y + target.Height < yPositionOnImage) return false;
            if (target.Y > yPositionOnImage + Height) return false;
            //If we get here, we have two rectangles ovelapping.
            return true;
        }

        /// <summary>
        /// Check to see if two sprites hit each-other.  The sprite collision methods are
        /// not all programmed in.
        /// </summary>
        /// <param name="target">The Sprite we are checking to see if we hit</param>
        /// <param name="how">The method we use to determine if they hit</param>
        public void CheckSpriteHitsSprite(Sprite target, SpriteCollisionMethod how)
        {
            if (target == this) return;
            if (CheckToSeeIfSpriteHitsSprite(target, how))
            {
                target.NoteSpriteHitsSprite(this, how);
                NoteSpriteHitsSprite(target, how);
                CollisionSprites.Add(target);
            }
        }

        /// <summary>
        /// This is used when two sprites hit each-other. 
        /// </summary>
        /// <param name="target">The sprite it hits</param>
        /// <param name="how">the method for checking</param>
        internal void NoteSpriteHitsSprite(Sprite target, SpriteCollisionMethod how)
        {
            if (target == this) return;
            SpriteEventArgs newArgs = new SpriteEventArgs();
            newArgs.TargetSprite = target;
            newArgs.CollisionMethod = how;
            if (SpriteHitsSprite != null)
            {
                if (!PausedEvents)
                    SpriteHitsSprite(this, newArgs);
            }
        }

        internal void CheckForEvents()
        {
            if (!CheckForExitingImage())
            {
                CheckForHittingEdgeOfImage();
            }
        }

        internal void ClearCollisionList()
        {
            CollisionSprites.Clear();
        }

        /// <summary>
        /// Make the sprite show up in front of all other sprites.
        /// </summary>
        public void SendToFront()
        {
            MySpriteController.SpriteToFront(this);
        }

        /// <summary>
        /// Make the sprite go behind all other sprites
        /// </summary>
        public void SendToBack()
        {
            MySpriteController.SpriteToBack(this);
        }

        /// <summary>
        /// Pause the sprite.  We can pause just the animation (and still let it move), pause movement (and let it animate), or pause everything.
        /// </summary>
        /// <param name="What">Which aspects of the sprite you want to pause.</param>
        public void Pause(SpritePauseType What = SpritePauseType.PauseAll)
        {
            if(!PausedAnimation  && (What == SpritePauseType.PauseAnimation || What == SpritePauseType.PauseAll))
            {
                PausedAnimationTime = DateTime.UtcNow;
                PausedAnimation = true;
            }
            if (!PausedMovement && (  What == SpritePauseType.PauseMovement || What == SpritePauseType.PauseAll))
            {
                PausedMovementTime = DateTime.UtcNow;
                PausedMovement = true;
            }
            if (What == SpritePauseType.PauseEvents || What == SpritePauseType.PauseAll)
            {
                PausedEvents = true;
            }
        }
        /// <summary>
        /// unpause the sprite.
        /// </summary>
        /// <param name="What">Which aspects of the sprite you want to unpause.</param>
        public void UnPause(SpritePauseType What = SpritePauseType.PauseAll)
        {
            TimeSpan duration;
            if (PausedAnimation && What == SpritePauseType.PauseAnimation || What == SpritePauseType.PauseAll)
            {
                duration = DateTime.UtcNow - PausedAnimationTime;
                LastResetImage = LastResetImage + duration;
                PausedAnimation = false;
            }
            if (PausedMovement && What == SpritePauseType.PauseMovement || What == SpritePauseType.PauseAll)
            {
                duration = DateTime.UtcNow - PausedMovementTime;
                LastMovement = LastResetImage + duration;
                PausedMovement = false;
            }
            if (PausedEvents && What == SpritePauseType.PauseEvents || What == SpritePauseType.PauseAll)
            {
                PausedEvents = false;
            }
        }
        /// <summary>
        /// Ask if the sprite is paused using the specified sprite type (default is PauseAll)
        /// </summary>
        /// <param name="What">The spritePauseType to see if the sprite is paused with</param>
        /// <returns>True if the sprite is set to pause the specified item, false if not</returns>
        public bool IsPaused(SpritePauseType What = SpritePauseType.PauseAll)
        {
            if (What == SpritePauseType.PauseAnimation && PausedAnimation) return true;
            if (What == SpritePauseType.PauseMovement && PausedMovement) return true;
            if (What == SpritePauseType.PauseEvents && PausedMovement) return true;
            if (What == SpritePauseType.PauseAll && PausedAnimation && PausedMovement && PausedEvents) return true;
            return false;
        }

        internal void ClickedOn(SpriteCollisionMethod how)
        {
            if (how == SpriteCollisionMethod.rectangle)
                Click(this, new SpriteEventArgs());
            if (how == SpriteCollisionMethod.transparency)
                ClickTransparent(this, new SpriteEventArgs());
        }

        internal void HoverOver()
        {
            MouseHover(this, new SpriteEventArgs());
        }
        internal void Enter()
        {
            MouseEnter(this, new SpriteEventArgs());
        }
        internal void Leave()
        {
            MouseLeave(this, new SpriteEventArgs());
        }
    }
}
