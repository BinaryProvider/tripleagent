using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.IO.Packaging;

namespace SpriteLibrary
{
    /// <summary>
    /// SpriteLibrary is a .net graphical library for creating and controlling sprites on a PictureBox.
    /// 
    /// A sprite is an animated image that can be moved around on a
    /// picturebox.  You can give the sprite an initial location, and either move it around manually or give it
    /// specific movement controls.
    /// 
    /// To use this library, you will need to add a reference to it in your project.  You will also need a reference to
    /// "Windows Base."
    /// In the solution explorer, if you right-click your project and go to "add", and then "reference" and click 
    /// "WindowsBase" towards the bottom.
    /// On that same window, on the left, click "browse." Then, click the "Browse..." button and find the sprite-library dll.
    /// The main places to find the SpriteLibrary and sample programs using this SpriteLibrary are here: 
    /// <see href="http://www.codeproject.com/Articles/1085446/Using-Sprites-Inside-Windows-Forms"/>
    /// and
    /// <see href="http://tyounglightsys.ddns.info/SpriteLibrary"/>
    /// </summary>
    internal class NamespaceDoc
    {

    }

    /// <summary>
    /// The various types of collisions a sprite can have.  Currently only rectangle works.  The other types were added when I
    /// thought the different types of collision types were needed.  Someday we may add these if we find they are useful, or if
    /// someone else decides they want to help program the SpriteLibrary.  These values are primarily used in Sprite Events
    /// </summary>
    public enum SpriteCollisionMethod {
        /// <summary>
        /// Checks if the two rectangles that contain the sprites overlap.  Each rectangle is the starting location of the sprite
        /// (top left) with the sprite width, and height marking the other sides of the rectangle.
        /// </summary>
        rectangle,
        /// <summary>
        /// Draws a circle (ellipse) inside the sprite rectangles and see if those ellipses overlap
        /// </summary>
        circle,
        /// <summary>
        /// Check to see if nontransparent portions of a sprite collide.  Not working.
        /// </summary>
        transparency }

    /// <summary>
    /// A structure that contains the width and height adjustment ratio.  Use this if you need to manually calculate positions
    /// between the PictureBox that the sprite is in, and the Background Image itself.
    /// </summary>
    public struct SpriteAdjustmentRatio {
        /// <summary>
        /// Divide a picturebox ratio by this to get the image location.  Multiply an image location by this to get the picturebox location.
        /// </summary>
        public double width_ratio;
        /// <summary>
        /// Divide a picturebox ratio by this to get the image location.  Multiply an image location by this to get the picturebox location.
        /// </summary>
        public double height_ratio; }
    /// <summary>
    /// The type of pause signals you can give a sprite or the sprite controller
    /// </summary>
    public enum SpritePauseType {
        /// <summary>
        /// Pause the animating.  Animation resumes from the current frame when we unpause.  A paused animation will continue
        /// to display the same image frame until it is unpaused.
        /// </summary>
        PauseAnimation,
        /// <summary>
        /// Pause any automatic movement.  Movement resumes where it was left off if you unpause.  The sprite will 
        /// just sit there until unpaused. 
        /// </summary>
        PauseMovement,
        /// <summary>
        /// Pause events. Sprite collisions, movement checks, etc are stopped until the unpause.
        /// </summary>
        PauseEvents,
        /// <summary>
        /// All pausable things are paused.  PauseAnimation, PauseMovement, and PauseEvents.
        /// </summary>
        PauseAll }

    /// <summary>
    /// A sprite controller is the main heart of the sprite class.  Each SpriteController manages one picturebox.
    /// If at all possible, try to keep each game in one picturebox, and try to avoid making and destroying
    /// new forms with SpriteController/pictureboxes in them.  It is hard to destroy them completely.
    /// </summary>
    /// <example>
    /// A sprite controller controls animations and
    /// can help you check for <see cref="SpriteController.IsKeyPressed(Keys)">key-presses.</see> To make a sprite controller,
    /// you need to have one defined for your main form:
    /// <code language="C#">
    /// SpriteController MySpriteController;
    /// </code>
    /// And then, when the form is created, after the InitializeComponents() function, you
    /// need to configure the drawing area and create the sprite controller:
    /// <code language="C#">
    /// MainDrawingArea.BackgroundImage = Properties.Resources.Background;
    /// MainDrawingArea.BackgroundImageLayout = ImageLayout.Stretch;
    /// MySpriteController = new SpriteController(MainDrawingArea);
    /// </code>
    /// In this case, MainDrawingArea is the picturebox where all the sprites will be displayed.
    /// </example>
    public class SpriteController
    {
        Image MyOriginalImage;  //The untainted background
        PictureBox DrawingArea;   //The PictureBox we draw ourselves on
        List<Sprite> Sprites = new List<Sprite>();

        /// <summary>
        /// Since everything needs a random number generator, we make one that should be accessible throughout your program.
        /// </summary>
        public Random RandomNumberGenerator = new Random();

        /// <summary>
        /// This is only used by the SpriteController.  It allows us to queue up invalidation requests.
        /// </summary>
        private List<Rectangle> InvalidateList = new List<Rectangle>();

        private KeyMessageFilter MessageFilter = new KeyMessageFilter();
        /// <summary>
        /// The count of all the sprites the controller knows about.  This includes named 
        /// sprites, which may not be visible.
        /// </summary>
        public int SpriteCount { get { return Sprites.Count; } }
        System.Windows.Forms.Timer MyTimer = new System.Windows.Forms.Timer();
        //private bool lockObject=false;
        //System.Threading.Timer ThreadTimer; 


        //These two are used for tracking mouse-leave, and mouse enter functions.
        private Point MousePoint = Point.Empty;
        private List<Sprite> SpritesUnderMouse = new List<Sprite>();

        /// <summary>
        /// If your sprite images need substantial growing or shrinking when displayed, you can try setting this to "true"
        /// to see if it makes it run any faster.  What it does is to resize the image once, and keep a cached copy of that
        /// image at that size.  If you use the same sprite, but with different sizes, setting this to "True" may actually slow
        /// down the game instead of speeding it up.
        /// </summary>
        public bool OptimizeForLargeSpriteImages = false;

        /// <summary>
        /// Create a sprite controller, specifying the picturebox on which the sprites
        /// will be displayed.  You want to have the PictureBox already defined, and a background image
        /// already set for the PictureBox.
        /// </summary>
        /// <example>
        /// This is an example of a Form class that defines a SpriteController.  The MainDrawingArea is a 
        /// <see cref="System.Windows.Forms.PictureBox">PictureBox.</see>
        /// <code lang="C#">
        /// public partial class ShootingFieldForm : Form
        /// {
        ///     public ShootingFieldForm()
        ///     {
        ///         InitializeComponent();
        ///         MainDrawingArea.BackgroundImage = Properties.Resources.Background;
        ///         MainDrawingArea.BackgroundImageLayout = ImageLayout.Stretch;
        ///         MySpriteController = new SpriteController(MainDrawingArea);
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <param name="Area">The <see cref="System.Windows.Forms.PictureBox">PictureBox.</see> 
        /// that the sprites will be drawn in</param>
        public SpriteController(PictureBox Area)
        {
            DrawingArea = Area;
            Local_Setup();
        }

        /// <summary>
        /// Create a sprite controller, specifying the picturebox on which the sprites
        /// will be displayed.
        /// </summary>
        /// <example>
        /// This is an example of a Form class that defines a SpriteController.  The MainDrawingArea is a 
        /// <see cref="System.Windows.Forms.PictureBox">PictureBox.</see>  While defining the SpriteController, we
        /// are also setting a function used for the <see cref="SpriteLibrary.SpriteController.DoTick">DoTick.</see> event.
        /// <code lang="C#">
        /// public partial class ShootingFieldForm : Form
        /// {
        ///     public ShootingFieldForm()
        ///     {
        ///         InitializeComponent();
        ///         MainDrawingArea.BackgroundImage = Properties.Resources.Background;
        ///         MainDrawingArea.BackgroundImageLayout = ImageLayout.Stretch;
        ///         MySpriteController = new SpriteController(MainDrawingArea, CheckForKeyPress);
        ///     }
        ///     
        ///     private void CheckForKeyPress(object sender, EventArgs e)
        ///     {
        ///         //Do stuff here
        ///     }
        /// }
        /// 
        /// </code>
        /// </example>
        /// <param name="Area">The picturebox that the sprites will be drawn in</param>
        /// <param name="TimerTickMethod">A function on the form that you want to have run every tick</param>
        public SpriteController(PictureBox Area, System.EventHandler TimerTickMethod)
        {
            DrawingArea = Area;
            Local_Setup();
            DoTick += new System.EventHandler(TimerTickMethod);
        }


        /// <summary>
        /// Define some things and set up some things that need defining at instantiation
        /// </summary>
        private void Local_Setup()
        {
            //Make a clone of the background image.  We take images from the "original image"
            //when we want to erase a sprite and re-draw it elsewhere.
            if (DrawingArea.BackgroundImage == null)
            {
                DrawingArea.BackgroundImage = new Bitmap(DrawingArea.Width, DrawingArea.Height);
                Graphics.FromImage(DrawingArea.BackgroundImage).FillRectangle(new SolidBrush(Form.DefaultBackColor), 
                    new Rectangle(0,0, DrawingArea.Width, DrawingArea.Height)); //Fill it with the default background color.
            }
            MyOriginalImage = (Image)DrawingArea.BackgroundImage.Clone(); //Duplicate it and store it
                                                                          //The messagefilter allows us to check for keypresses.
            Application.AddMessageFilter(MessageFilter);

            //Set up the timer.
            MyTimer.Interval = 10;
            MyTimer.Tick += TimerTick;
            MyTimer.Start();
            //ThreadTimer = new System.Threading.Timer(TryTimer, null, 0, 10);

            //Add a mouseclick event
            DrawingArea.MouseClick += MouseClickOnBox;
            DrawingArea.MouseHover += MouseHover;
            DrawingArea.MouseMove += MouseMove;

            //Add a function to be called when the parent form is resized.  This keeps things from
            //Misbehaving immediately after a resize.
            Form tParent = (Form)DrawingArea.FindForm();
            //tParent.ResizeEnd += ProcessImageResize;
            tParent.SizeChanged += ProcessImageResize;
        }

        /// <summary>
        /// Change the Tick Interval.  By default, the spritecontroller does a tick every 10ms, which
        /// is very fast.  Some people may prefer it to happen less regularly. Must be > 5, and less than 1001
        /// </summary>
        /// <param name="newTickMilliseconds">The new tick interval</param>
        public void ChangeTickInterval(int newTickMilliseconds)
        {
            if (newTickMilliseconds < 5) return;
            if (newTickMilliseconds > 1000) return;
            MyTimer.Interval = newTickMilliseconds;
        }

        //private void TryTimer(object state)
        //{
        //    if (System.Threading.Monitor.TryEnter(lockObject,10))
        //    {
        //        try
        //        {
        //            // Work here
        //            DoTick(null,null);
        //        }
        //        finally
        //        {
        //            System.Threading.Monitor.Exit(lockObject);
        //        }
        //    }
        //}

        /// <summary>
        /// Allow the sprite sort-method to be overridden.  
        /// </summary>
        /// <example>
        /// The default sprite sort method is: 
        /// <code lang="C#">
        /// SpriteComparisonDelegate = delegate (Sprite first, Sprite second) { return first.Zvalue.CompareTo(second.Zvalue); };
        /// </code>
        /// Which compares just the Zvalues of the two sprites.  Often you will want to have a more refined sort.  The sort
        /// order determines which sprites appear on top of other sprites.  In the default state, if two sprites have the
        /// same Zvalue, it is very uncleaer which one will draw on top of the other one.  By overridding this sort function,
        /// you can specify a very precise order of which sprite is on top and which is behind.
        /// </example>
        public Comparison<Sprite> SpriteComparisonDelegate = null;

        /// <summary>
        /// This is what happens when someone clicks on the PictureBox.  We want to pass any Click events to the Sprite
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseClickOnBox(object sender, MouseEventArgs e)
        {
            List<Sprite> SpritesHere = SpritesAtPoint(e.Location);
            foreach(Sprite one in SpritesHere.ToList())
            {
                one.ClickedOn(SpriteCollisionMethod.rectangle);
                if (one.SpriteAtPictureBoxPoint(e.Location, SpriteCollisionMethod.transparency))
                    one.ClickedOn(SpriteCollisionMethod.transparency);
            }
        }

        /// <summary>
        /// Check to see if we are hovering over anything
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseHover(object sender, EventArgs e)
        {
            if (MousePoint == Point.Empty) return;
            List<Sprite> SpritesHere = SpritesAtPoint(MousePoint);
            foreach (Sprite one in SpritesHere.ToList())
            {
                one.HoverOver();
            }
        }

        //Track when we move over a new sprite, and when we leave a sprite
        private void MouseMove(object sender, MouseEventArgs e)
        {
            MousePoint = e.Location;
            List<Sprite> SpritesHere = SpritesAtPoint(e.Location);
            List<Sprite> OldSprites = new List<Sprite>();
            OldSprites.AddRange(SpritesUnderMouse);
            SpritesUnderMouse.Clear();
            foreach (Sprite one in SpritesHere.ToList())
            {
                if(!OldSprites.Contains(one))
                {
                    //This is the first time we have run into it
                    one.Enter();
                }
                OldSprites.Remove(one);
            }
            //Now, anything we have not "removed" is a sprite we are no longer over.
            foreach(Sprite donewith in OldSprites.ToList())
            {
                donewith.Leave();
            }
            SpritesUnderMouse.AddRange(SpritesHere);
        }

        /// <summary>
        /// Replace the image on which the sprites are drawn.  Use this when you move to a new playing field, 
        /// or want to have a different background
        /// <example>
        /// Replacing the background image is actually a lot more complex than you might imagine.  Once you use the 
        /// below code, it can be done without any problem.  But you need to do it this way, or it just goofs up in 
        /// a number of small ways.
        /// You need to tell the sprite controller that you are replacing the background image, 
        /// and you need to change the image to that image as well.Because the Images are actually 
        /// pointers to memory where the image sets, changes to one image will affect the other image.This goofs 
        /// things up, so what we do is duplicate the image twice, and tell the sprite controller to use one of the 
        /// copies and then set the background to be the other one of the two copies.Finally, we tell the picturebox 
        /// to invalidate itself.That does everything that is needed.
        /// <code lang="C#">
        /// void ReplaceBackground(Image NewBackground)
        ///{
        ///    if (MyController == null) return;
        ///    if (NewBackground == null) return;
        ///
        ///    Image OneImage = new Bitmap(NewBackground);
        ///    MyController.ReplaceOriginalImage(OneImage);
        ///
        ///    Image TwoImage = new Bitmap(NewBackground);
        ///    pb_map.BackgroundImage = TwoImage;
        ///    pb_map.Invalidate();
        ///}
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="tImage">The new image that all sprites will be drawn on</param>
        public void ReplaceOriginalImage(Image tImage)
        {
            if(MyOriginalImage == null)
            {
                MyOriginalImage = (Image)DrawingArea.BackgroundImage.Clone();
            }
            else
            {
                Graphics.FromImage(MyOriginalImage).Clear(Color.Transparent); //erase the old image
                Graphics.FromImage(MyOriginalImage).DrawImage(tImage, new Rectangle(0, 0, MyOriginalImage.Width, MyOriginalImage.Height));
                Graphics.FromImage(DrawingArea.BackgroundImage).Clear(Color.Transparent); //erase the old image
                Graphics.FromImage(DrawingArea.BackgroundImage).DrawImage(tImage, new Rectangle(0, 0, MyOriginalImage.Width, MyOriginalImage.Height));
            }
            DrawingArea.Invalidate();
        }

        /// <summary>
        /// Notify the sprite controller that you have changed the background image on the
        /// PictureBox.  Whatever background is on the picturebox is now used to draw all the sprites on.
        /// </summary>
        public void ReplaceOriginalImage()
        {
            if (MyOriginalImage == null)
            {
                MyOriginalImage = (Image)DrawingArea.BackgroundImage.Clone();
            }
            else
            {
                Graphics.FromImage(MyOriginalImage).DrawImage(DrawingArea.BackgroundImage, new Rectangle(0, 0, MyOriginalImage.Width, MyOriginalImage.Height));
            }
        }

        /// <summary>
        /// The function called by the timer every 10 millisecods  We also call do_tick, which
        /// is the function defined by the user.  This is usually where they will do the majority of the work.
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerTick(object sender, EventArgs e)
        {
            //If we have added a function to call on the timer, do it.
            DoTick(sender, e);
            Tick();
        }

        /// <summary>
        /// The function called by the timer every 10 millisecods  This is usually where you will do the majority of the work.
        /// You can define this manually, or when you <see cref="SpriteLibrary.SpriteController.SpriteController(PictureBox, EventHandler)">instantiate the SpriteController</see>
        /// </summary>
        /// <example>
        /// The Sprite controller uses a <see cref="System.Windows.Forms.Timer">System.Windows.Forms.Timer.</see>  This timer is notoriously un-precise, but it is very 
        /// easy to set up initially.  It tries to fire off every 10 milliseconds, but it can fire off incredibly 
        /// slowly if you have long pieces of code; the DoTick function needs to finish before it can start again.  You want all your 
        /// functions to run as quickly as possible to avoid things looking jerky.
        /// Most programs you will make using the sprite library will begin by tapping into the DoTick Event. 
        /// Every time the sprite controller is ready to pass control back to your program, it will call 
        /// the DoTick event.  You want to see if you should be doing anything, and then exiting the do-tick function.
        /// <code lang = "C#">
        /// public partial class ShootingFieldForm : Form
        /// {
        ///     public ShootingFieldForm()
        ///     {
        ///         InitializeComponent();
        ///         MainDrawingArea.BackgroundImage = Properties.Resources.Background;
        ///         MainDrawingArea.BackgroundImageLayout = ImageLayout.Stretch;
        ///         MySpriteController = new SpriteController(MainDrawingArea, CheckForKeyPress);
        ///     }
        ///     
        ///     private void CheckForKeyPress(object sender, EventArgs e)
        ///     {
        ///        bool left = false;
        ///        bool right = false;
        ///        bool space = false;
        ///        bool didsomething = false;
        ///        TimeSpan duration = DateTime.Now - LastMovement;
        ///        if (duration.TotalMilliseconds &lt; 100)
        ///            return;
        ///        LastMovement = DateTime.Now;
        ///        if (MySpriteController.IsKeyPressed(Keys.A) || MySpriteController.IsKeyPressed(Keys.Left))
        ///        {
        ///            left = true;
        ///        }
        ///        if (MySpriteController.IsKeyPressed(Keys.D)||MySpriteController.IsKeyPressed(Keys.Right))
        ///        {
        ///            right = true;
        ///        }
        ///        if (left &amp;&amp; right) return; //do nothing if we conflict
        ///        if (left)
        ///        {               
        ///            if (LastDirection != MyDir.left)
        ///            {
        ///                Spaceship.SetSpriteDirectionDegrees(180);
        ///                //We want to only change animation once.  Every time we change
        ///                //the animation, it starts at the first frame again.
        ///                Spaceship.ChangeAnimation(0);
        ///                LastDirection = MyDir.left;
        ///            }
        ///            didsomething = true;
        ///            Spaceship.MovementSpeed = 15;
        ///            Spaceship.AutomaticallyMoves = true;
        ///        }
        ///        if (right)
        ///        {                
        ///            if (LastDirection != MyDir.right)
        ///            {
        ///                Spaceship.SetSpriteDirectionDegrees(0);
        ///                Spaceship.ChangeAnimation(0);
        ///                LastDirection = MyDir.right;
        ///            }
        ///            didsomething = true;
        ///            Spaceship.AutomaticallyMoves = true;
        ///            Spaceship.MovementSpeed = 15;
        ///        }
        ///        if(!didsomething)
        ///        {
        ///            LastDirection = MyDir.stopped;
        ///            //No keys pressed.  Stop moving
        ///            Spaceship.MovementSpeed = 0;
        ///        }
        ///    }
        /// </code>
        /// </example>
        public event EventHandler DoTick = delegate { };

        /// <summary>
        /// Process a form resize by recalculating all the picturebox locations for all sprites.
        /// </summary>
        /// <param name="sender">The form</param>
        /// <param name="e">Form event args</param>
        internal void ProcessImageResize(object sender, EventArgs e)
        {
            //Go through all sprites and recalculate the Ratio.
            foreach (Sprite oneSprite in Sprites)
            {
                oneSprite.RecalcPictureBoxLocation();
            }
        }

        /// <summary>
        /// Count the number of sprites that were duplicated from the sprite with the specified name.  When you use a 
        /// <see cref="SpriteLibrary.SpriteController.DuplicateSprite(string)">SpriteController.DuplicateSprite(string)</see>
        /// command, it creates a new sprite that is based off the named sprite.  This function will count those duplicated sprites.
        /// </summary>
        /// <param name="Name">The name to look for</param>
        /// <returns>The count of sprites that are duplicates of the specified name</returns>
        public int CountSpritesBasedOff(string Name)
        {
            int count = 0;
            foreach (Sprite OneSprite in Sprites)
            {
                if (OneSprite.SpriteOriginName == Name)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Return a list of all sprites
        /// </summary>
        /// <returns>A list of all sprites</returns>
        public List<Sprite> AllSprites()
        {
            return Sprites;
        }

        /// <summary>
        /// Return all sprites that were based off a particular sprite name.
        ///  When you use a 
        /// <see cref="SpriteLibrary.SpriteController.DuplicateSprite(string)">SpriteController.DuplicateSprite(string)</see>
        /// command, it creates a new sprite that is based off the named sprite.  This function returns a list of those
        /// duplicated sprites.
        /// </summary>
        /// <param name="SpriteName">The sprite name to find</param>
        /// <returns>A list of sprites that were based off the named sprite</returns>
        public List<Sprite> SpritesBasedOff(string SpriteName)
        {
            List<Sprite> newList = new List<Sprite>();
            foreach(Sprite one in Sprites)
            {
                if (one.SpriteOriginName == SpriteName)
                    newList.Add(one);
            }
            return newList;
        }

        /// <summary>
        /// Return a list of all sprites which have been drawn on the image
        /// </summary>
        /// <returns>A list of sprites that have been drawn</returns>
        public List<Sprite> SpritesThatHaveBeenDrawn()
        {
            List<Sprite> newList = new List<Sprite>();
            foreach (Sprite one in Sprites)
            {
                if (one.HasBeenDrawn)
                    newList.Add(one);
            }
            return newList;
        }

        /// <summary>
        /// Return a list of all sprites which are not master sprites (which are duplicates of something)
        /// </summary>
        /// <returns>A list of sprites</returns>
        public List<Sprite> SpritesBasedOffAnything()
        {
            List<Sprite> newList = new List<Sprite>();
            foreach (Sprite one in Sprites)
            {
                if (one.SpriteOriginName != "" && one.SpriteOriginName != null)
                    newList.Add(one);
            }
            return newList;
        }

        /// <summary>
        /// Return an adjustment ratio.  This is the image-size to picture-box ratio.
        /// It is used for calculating precise pixels or picture-box locations.
        /// </summary>
        /// <returns>A SpriteAdjustmentRatio containing the current ratio of picture-box pixels to image-box pixels</returns>
        public SpriteAdjustmentRatio ReturnAdjustmentRatio()
        {
            SpriteAdjustmentRatio Ratio = new SpriteAdjustmentRatio();

            //default to stretch
            switch (DrawingArea.BackgroundImageLayout)
            {
                case ImageLayout.Center:
                case ImageLayout.None:
                case ImageLayout.Tile:
                case ImageLayout.Zoom:
                case ImageLayout.Stretch:
                default:
                    //This is the code for Stretch.
                    Ratio.width_ratio = DrawingArea.ClientRectangle.Width / (double)MyOriginalImage.Width;
                    Ratio.height_ratio = DrawingArea.ClientRectangle.Height / (double)MyOriginalImage.Height;
                    break;
            }
            return Ratio;
        }

        /// <summary>
        /// This takes a point, the location on a picturebox, and returns the corresponding point on the BackgroundImage.
        /// Picturebox locations are "sloppy"; the background image locations are very precise.  Since this takes a "sloppy"
        /// number and returns a precise number, it does some rounding to figure out where the specified location is.  
        /// </summary>
        /// <param name="LocationOnPicturebox">A point on the picturebox that you want the corresponding image pixel location for.</param>
        /// <returns>A point (x,y) on the background image which corresponds to the picture-box coordinates you sent into the function.</returns>
        public Point ReturnPointAdjustedForImage(Point LocationOnPicturebox)
        {
            SpriteAdjustmentRatio Ratio = ReturnAdjustmentRatio();
            Point returnedPoint = new Point((int)(LocationOnPicturebox.X / Ratio.width_ratio), (int)(LocationOnPicturebox.Y / Ratio.height_ratio));
            return returnedPoint;
        }

        /// <summary>
        /// Return the height of an object in picture-box terms.  It is basically the virtual height
        /// of the sprite or other item.
        /// </summary>
        /// <param name="Height">The image-box heigh (or sprite height)</param>
        /// <returns>An integer that corresponds to the hight as displayed in the picturebox</returns>
        public int ReturnPictureBoxAdjustedHeight(int Height)
        {
            SpriteAdjustmentRatio Ratio = ReturnAdjustmentRatio();
            int returnedAmount = (int)(Height * Ratio.height_ratio);
            return returnedAmount;
        }

        /// <summary>
        /// Return the width of an object in picture-box terms.  It takes the width of a sprite or other
        /// item that is being displayed on the screen, and calculates the width as displayed in the
        /// picture-box (taking into consideration stretching or shrinking)
        /// </summary>
        /// <param name="Width">An integer width of the drawn item</param>
        /// <returns>An integer that contains the number of pixels wide it is on the picturebox</returns>
        public int ReturnPictureBoxAdjustedWidth(int Width)
        {
            SpriteAdjustmentRatio Ratio = ReturnAdjustmentRatio();
            int returnedAmount = (int)(Width * Ratio.width_ratio);
            return returnedAmount;
        }

        /// <summary>
        /// This does the reverse of an adjusted point.  It takes a point on the image and 
        /// transforms it to one on the PictureBox
        /// </summary>
        /// <param name="LocationOnImage">A point on the image, using the x and y pixels on the image</param>
        /// <returns>A location that can be used on the picture-box, taking into consideration the image being stretched.</returns>
        public Point ReturnPictureBoxAdjustedPoint(Point LocationOnImage)
        {
            SpriteAdjustmentRatio Ratio = ReturnAdjustmentRatio();
            Point returnedPoint = new Point((int)(LocationOnImage.X * Ratio.width_ratio), (int)(LocationOnImage.Y * Ratio.height_ratio));
            return returnedPoint;
        }

        /// <summary>
        /// Adjust a rectangle that is based on the image, according to the stretch of the picturebox
        /// </summary>
        /// <param name="ImageRectangle">A rectangle using coordinates from the image</param>
        /// <returns>a rectangle that is adjusted for the PictureBox</returns>
        public Rectangle AdjustRectangle(Rectangle ImageRectangle)
        {
            if (DrawingArea.BackgroundImageLayout == ImageLayout.Stretch)
            {
                SpriteAdjustmentRatio Ratio = ReturnAdjustmentRatio();
                double width_ratio = Ratio.width_ratio;
                double height_ratio = Ratio.height_ratio;
                int x, y, width, height;
                x = (int)(ImageRectangle.X * width_ratio);
                y = (int)(ImageRectangle.Y * height_ratio);
                width = (int)(ImageRectangle.Width * width_ratio);
                height = (int)(ImageRectangle.Height * height_ratio);

                Rectangle newRec = new Rectangle(x, y, width, height);
                return newRec;
            }

            return ImageRectangle; //If we do not know what it is, return the curent
        }

        /// <summary>
        /// Adjust an image point so that it conforms to the picturebox.
        /// </summary>
        /// <param name="LocationOnImage">The image location</param>
        /// <returns>the corresponding point on the PictuerBox</returns>
        public Point AdjustPoint(Point LocationOnImage)
        {
            SpriteAdjustmentRatio Ratio = ReturnAdjustmentRatio();
            double width_ratio = Ratio.width_ratio;
            double height_ratio = Ratio.height_ratio;
            int x, y;
            x = (int)(LocationOnImage.X * width_ratio);
            y = (int)(LocationOnImage.Y * height_ratio);
            return new Point(x, y);
        }

        /// <summary>
        /// Invalidate a rectangle that is specified in image coordinates
        /// </summary>
        /// <param name="ImageRectangle">A rectangle based on the image coordinates</param>
        /// <param name="QueueUpInvalidation">Whether to do it now, or to queue it up for another time.</param>
        public void Invalidate(Rectangle ImageRectangle, bool QueueUpInvalidation = true)
        {
            if (QueueUpInvalidation)
            {
                InvalidateList.Add(ImageRectangle);
            }
            else
            {
                //Figure out the area we are looking at
                if (DrawingArea.BackgroundImageLayout == ImageLayout.Stretch)
                {
                    Rectangle newRec = AdjustRectangle(ImageRectangle);
                    newRec = new Rectangle(newRec.Location.X, newRec.Location.Y, newRec.Width + 2, newRec.Height + 2);
                    //Now we invalidate the adjusted rectangle
                    DrawingArea.Invalidate(newRec);
                }
            }
        }

        /// <summary>
        /// Invalidate the entire image on which the sprites are drawn
        /// </summary>
        /// <param name="QueueUpInvalidation">Whether to do it now, or to queue it up for another time.</param>
        public void Invalidate(bool QueueUpInvalidation = true)
        {
            Invalidate(DrawingArea.ClientRectangle);
        }

        /// <summary>
        /// The Background Image on which the sprites are drawn.  This image ends up having
        /// sprite parts on it. The OriginalImage is the version that is clean.  Use
        /// ReplaceOriginalImage to replace the background Image.
        /// </summary>
        public Image BackgroundImage { get { return DrawingArea.BackgroundImage; } }
        /// <summary>
        /// The Image from which the background is taken when we erase sprites.  The BackgroundImage
        /// is the image that contains images of the sprites as well as the background image.  Use
        /// ReplaceOriginalImage to replace this and the BackgroundImage.
        /// </summary>
        public Image OriginalImage { get { return MyOriginalImage; } }


        void Tick()
        {
            //We check for collisions.
            for (int looper = 0; looper < Sprites.Count; looper++)
            {
                if (Sprites[looper] != null && !Sprites[looper].Destroying && Sprites[looper].HasBeenDrawn)
                {
                    for (int checkloop = 0; checkloop < Sprites.Count; checkloop++)
                    {
                        if (Sprites[checkloop] != null && !Sprites[checkloop].Destroying && Sprites[checkloop].HasBeenDrawn)
                        {
                            //Check to see if they have hit
                            Sprites[looper].CheckSpriteHitsSprite(Sprites[checkloop], SpriteCollisionMethod.rectangle);
                        }
                    }
                }
            }

            //We do a tick for each sprite
            foreach (Sprite tSprite in Sprites.ToList())
            {
                if (!tSprite.Destroying)
                {
                    tSprite.Tick();
                }
            }
            foreach (Sprite tSprite in Sprites.ToList())
            {
                if (!tSprite.Destroying)
                {
                    tSprite.ActuallyDraw();
                }
            }
            foreach(Rectangle rec in InvalidateList)
            {
                Invalidate(rec, false);
            }
            InvalidateList.Clear();
        }

        /// <summary>
        /// Make a duplicate of the specified sprite.  The duplicate does not yet have a location.
        /// </summary>
        /// <param name="What">The sprite to duplicate</param>
        /// <returns>A new sprite.  If What is null, returns null</returns>
        public Sprite DuplicateSprite(Sprite What)
        {
            //Make a new sprite that is based off of the old one
            if (What == null) return null;
            return new Sprite(What);
        }

        /// <summary>
        /// Find a sprite that has been named with the specified name.  Then duplicate that sprite
        /// </summary>
        /// <example>
        /// Below is a function that creates a sprite based off a name, and puts it at the designated coordinates.
        /// <code lang="C#">
        /// public void AddSprite(string name, int startx, int starty)
        /// {
        ///      Sprite NewSprite = MySpriteController.DuplicateSprite(What.ToString());
        ///      if(NewSprite != null)
        ///      {
        ///          NewSprite.AutomaticallyMoves = true;
        ///          NewSprite.CannotMoveOutsideBox = true;
        ///          NewSprite.SetSpriteDirectionDegrees(180); //left
        ///          NewSprite.PutBaseImageLocation(new Point(startx, starty));
        ///          NewSprite.MovementSpeed = 5;
        ///      }
        /// }
        /// </code>
        /// </example>
        /// <param name="Name">The name of a sprite</param>
        /// <returns>A duplicate of the specified sprite.  It has no location, and does not retain the sprite name.</returns>
        public Sprite DuplicateSprite(string Name)
        {
            Sprite tSprite = SpriteFromName(Name);
            if (tSprite == null) return null;
            return new Sprite(tSprite); //Make a new sprite that is based off the original
        }

        /// <summary>
        /// Find a sprite that has a specified name.  This returns the actual sprite with that name.
        /// You usually want to use DuplicateSprite(Name) to clone the sprite and get one you can
        /// destroy.  If you destroy a named sprite without duplicating it, you may end up losing
        /// it for the remainder of the program.
        /// </summary>
        /// <param name="Name">A string that matches something added to a sprite with Sprite.SetName</param>
        /// <returns>A sprite that has the specified name, or null if no such sprite exists.</returns>
        public Sprite SpriteFromName(string Name)
        {
            foreach (Sprite OneSprite in Sprites)
            {
                if (OneSprite.SpriteName == Name)
                { return OneSprite; }
            }
            return null;
        }

        /// <summary>
        /// Add the specified sprite to the list of sprites we know about.  You usually do not need to do this.
        /// Sprites add themselves to the controller when you create a new sprite.
        /// </summary>
        /// <param name="SpriteToAdd">The sprite to add to the sprite-controller</param>
        public void AddSprite(Sprite SpriteToAdd)
        {
            Sprites.Add(SpriteToAdd);
            SortSprites();
        }

        /// <summary>
        /// Tell a sprite to destroy itself.  The sprite will have Destroying property set to true from
        /// the time you destroy it until it vanishes.  Whe you destroy a sprite, it will erase itself 
        /// and remove itself from the controller.  After it is destroyed, it is completely gone.
        /// </summary>
        /// <param name="what">The Sprite to destroy</param>
        public void DestroySprite(Sprite what)
        {
            if (what == null) return;
            Sprites.Remove(what);
            if (!what.Destroying)
            {
                what.Destroy();
            }
        }

        /// <summary>
        /// Remove all sprites (even named sprites that have not yet been displayed)
        /// </summary>
        public void DestroyAllSprites()
        {
            for(int i= Sprites.Count -1; i>=0; i--)
            {
                Sprites[i].Destroy();
            }
        }

        /// <summary>
        /// Find the specified Sprite in the controller and change its name to the specified string.
        /// You can do the same thing with <see cref="SpriteLibrary.Sprite.SetName(string)">Sprite.SetName(Name)</see>
        /// </summary>
        /// <param name="What">The Sprite to find</param>
        /// <param name="Name">The string to change the name to</param>
        public void NameSprite(Sprite What, string Name)
        {
            What.SetName(Name);
        }

        /// <summary>
        /// This takes a point, as given by the mouse-click args, and returns the sprites at that point. Different
        /// functions use different coordinates, whether based off the background image, or based off the picturebox.
        /// This one uses the picturebox coordinates.  So you can use this directly from a MouseDown or MouseUp function.
        /// </summary>
        /// <param name="Location">The picture-box point being clicked on</param>
        /// <returns>A list of sprites that are all at the specified point.</returns>
        public List<Sprite> SpritesAtPoint(Point Location)
        {
            List<Sprite> tList = new List<Sprite>();
            foreach (Sprite OneSprite in Sprites)
            {
                if (OneSprite.HasBeenDrawn && OneSprite.SpriteAtPictureBoxPoint(Location))
                {
                    tList.Add(OneSprite);
                }
            }
            return tList;
        }
        /// <summary>
        /// This takes a point, as as specified on the image, and returns the sprites at that point. Different
        /// functions use different coordinates, whether based off the background image, or based off the picturebox.
        /// This one uses the background image coordinates.  Use SpritesAdPoint() if you are doing something based off
        /// a MouseUp or MouseDown function.  This is used for functions based on sprite location or based off the absoloute
        /// location (using the background image location is much more precise than the visible location in the picturebox)
        /// </summary>
        /// <param name="Location">The point being looked at</param>
        /// <returns>A list of sprites that are all at the specified image point</returns>
        public List<Sprite> SpritesAtImagePoint(Point Location)
        {
            List<Sprite> tList = new List<Sprite>();
            foreach (Sprite OneSprite in Sprites)
            {
                if (OneSprite.HasBeenDrawn && OneSprite.SpriteAtImagePoint(Location))
                {
                    tList.Add(OneSprite);
                }
            }
            return tList;
        }

        /// <summary>
        /// Return a list of all the sprites that intersect with the given background-image-based rectangle
        /// </summary>
        /// <param name="Location">The rectangle on the image we are trying to find</param>
        /// <returns>A list of the sprites that have any portion of it inside the rectangle</returns>
        public List<Sprite> SpritesInImageRectangle(Rectangle Location)
        {
            List<Sprite> tList = new List<Sprite>();
            foreach (Sprite OneSprite in Sprites)
            {
                if (OneSprite.HasBeenDrawn && OneSprite.SpriteIntersectsRectangle(Location))
                {
                    tList.Add(OneSprite);
                }
            }
            return tList;
        }
        /// <summary>
        /// Check to see if any keys are pressed. There is a small glitch with the
        /// key-pressed system.  If the form loses focus, and someone releases a key, the key-up is never
        /// triggered.  It is a good thing to ResetKeypressState() occasionally if you think your form may have
        /// lost focus.
        /// </summary>
        /// <returns>True if a key is pressed, false if no keys are pressed.</returns>
        public bool IsKeyPressed()
        {
            return MessageFilter.IsKeyPressed();
        }

        /// <summary>
        /// Return a list of all the keys that are currently pressed.  There is a small glitch with the
        /// key-pressed system.  If the form loses focus, and someone releases a key, the key-up is never
        /// triggered.  It is a good thing to ResetKeypressState() occasionally if you think your form may have
        /// lost focus.
        /// </summary>
        /// <returns>A List of Keys which are currently considered to be pressed.</returns>
        public List<Keys> KeysPressed()
        {
            return MessageFilter.KeysPressed();
        }

        /// <summary>
        /// Check to see if the given key is pressed. There is a small glitch with the
        /// key-pressed system.  If the form loses focus, and someone releases a key, the key-up is never
        /// triggered.  It is a good thing to ResetKeypressState() occasionally if you think your form may have
        /// lost focus.
        /// </summary>
        /// <param name="k">The key to check to see if it is pressed</param>
        /// <returns>True if the key is pressed, false if that key is not pressed</returns>
        public bool IsKeyPressed(Keys k)
        {
            return MessageFilter.IsKeyPressed(k);
        }

        /// <summary>
        /// If you want to have a KeyDown function that is triggered by a keypress function, add the event here.
        /// The event should have the parameters (object sender, KeyEventArgs e)
        /// </summary>
        /// <param name="Func">The function to set</param>
        public void RegisterKeyDownFunction(SpriteKeyEventHandler Func)
        {
            MessageFilter.KeyDown += Func;
        }

        /// <summary>
        /// If you want to have a KeyUp function that is triggered by a keypress function, add the event here.
        /// The event should have the parameters (object sender, KeyEventArgs e)
        /// </summary>
        /// <param name="Func">The function to set</param>
        public void RegisterKeyUpFunction(SpriteKeyEventHandler Func)
        {
            MessageFilter.KeyUp += Func;
        }

        /// <summary>
        /// Reset the keypress status.  Sometimes the sprite controller misses a key being released (usually
        /// because a window has taken priority, or something has changed).  Calling this function will reset
        /// the stored memory of whether a key has been pressed.
        /// </summary>
        public void ResetKeypressState()
        {
            MessageFilter.ResetState();
        }

        /// <summary>
        /// Change the display order of the specified sprite so it goes in front of all other sprites.
        /// </summary>
        /// <param name="What">The sprite we want to show up in front</param>
        public void SpriteToFront(Sprite What)
        {
            What.Zvalue = 100;
        }

        /// <summary>
        /// Change the display order of the specified sprite so it goes behind all other sprites.
        /// </summary>
        /// <param name="What">The sprite to send behind all other sprites</param>
        public void SpriteToBack(Sprite What)
        {
            What.Zvalue = 0;
        }
        /// <summary>
        /// Change the display order of the specified sprite so it is more likely to go behind all other sprites.
        /// </summary>
        /// <param name="What">The sprite to send behind all other sprites</param>
        public void SpriteBackwards(Sprite What)
        {
            What.Zvalue--;
        }
        /// <summary>
        /// Change the display order of the specified sprite so it is more likely to go in front of other sprites
        /// </summary>
        /// <param name="What">The sprite to send behind all other sprites</param>
        public void SpriteForwards(Sprite What)
        {
            What.Zvalue++;
        }
        /// <summary>
        /// Change the display order of the sprites such that the specified sprite appears behind the other sprite.
        /// </summary>
        /// <param name="WhatToSend">The sprite we are changing the display order of</param>
        /// <param name="ToGoBehind">The sprite we want to go behind</param>
        public void PlaceSpriteBehind(Sprite WhatToSend, Sprite ToGoBehind)
        {
            if (WhatToSend == ToGoBehind) return;
            if (WhatToSend == null) return;
            if (ToGoBehind == null) return;
            WhatToSend.Zvalue = ToGoBehind.Zvalue - 1;
        }

        /// <summary>
        /// Make the sprite go in front of the specified sprite.
        /// </summary>
        /// <param name="WhatToSend">The sprite to change the display order of</param>
        /// <param name="ToGoInFrontOf">The sprite we want to make sure we display in front of</param>
        public void PlaceSpriteInFrontOf(Sprite WhatToSend, Sprite ToGoInFrontOf)
        {
            if (WhatToSend == ToGoInFrontOf) return;
            if (WhatToSend == null) return;
            if (ToGoInFrontOf == null) return;
            WhatToSend.Zvalue = ToGoInFrontOf.Zvalue + 1;
        }

        //****************************//
        //*******  SOUND Stuff *******//
        private struct SoundEntry
        {
            public string SoundName;
            public bool HasBeenPlayed;
        }

        List<SoundEntry> MySounds = new List<SoundEntry>();
        /// <summary>
        /// Play a sound that we can check to see if it has completed.
        /// </summary>
        /// <param name="ToPlay">The sound to play</param>
        /// <param name="Name">The name, which we can use to determine if it has finished.</param>
        public void SoundPlay(System.IO.Stream ToPlay, string Name)
        {
            if (SoundIsFinished(Name))
            {
                PlayAsync(ToPlay, Name, SoundIsDone);
                RegisterSound(Name);
            }
        }

        /// <summary>
        /// Play a sound bit in a separate thread.  When the thread is done, set a bool saying that
        /// </summary>
        /// <param name="ToPlay">The sound to play</param>
        /// <param name="RegisterName">The string that we can use to track the status of the sound</param>
        /// <param name="WhenDone">A function that gets called when the sound is complete</param>
        private void PlayAsync(System.IO.Stream ToPlay, string RegisterName, EventHandler WhenDone)
        {
            ToPlay.Position = 0;
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                using (System.Media.SoundPlayer player = new System.Media.SoundPlayer(ToPlay))
                {
                    player.PlaySync();
                }

                if (WhenDone != null) WhenDone(RegisterName, EventArgs.Empty);
            });
            //new System.Threading.Thread(() => {
            //    var c = new System.Windows.Media.MediaPlayer();
            //    byte[] buffer = new byte[ToPlay.Length];
            //    int totalBytesCopied;
            //    for (totalBytesCopied = 0; totalBytesCopied < ToPlay.Length;)
            //        totalBytesCopied += ToPlay.Read(buffer, totalBytesCopied, Convert.ToInt32(ToPlay.Length) - totalBytesCopied);
            //    Uri tURI = Create_Memory_Resource_Uri(buffer, totalBytesCopied);
            //    c.Open(tURI);
            //    c.Play();
            //    if (WhenDone != null) WhenDone(RegisterName, EventArgs.Empty);
            //}).Start();
        }

        //Uri Create_Memory_Resource_Uri(byte[] in_memory_resource, int Size)
        //{
        //    MemoryStream packStream = new MemoryStream();
        //    using (
        //    Package pack =
        //        Package.Open(packStream, FileMode.Create, FileAccess.ReadWrite))
        //    {
        //        Uri packUri = new Uri("AnyBeforeColon:");
        //        PackageStore.AddPackage(packUri, pack);
        //        Uri packPartUri = new Uri("/AnyAfterSlash", UriKind.Relative);
        //        //PackagePart packPart = 
        //        //   pack.CreatePart(packPartUri, "application/vnd.ms-opentype");
        //        PackagePart packPart =
        //          pack.CreatePart(packPartUri, "AnyBeforeSlash/AnyAfterSlash");
        //        //MemoryStream resourceStream = new MemoryStream(in_memory_resource);
        //        //CopyStream(resourceStream, packPart.GetStream());
        //        packPart.GetStream().Write(in_memory_resource, 0, Size);
        //        Uri memory_resource_uri =
        //            PackUriHelper.Create(packUri, packPart.Uri);
        //        return memory_resource_uri;
        //    }
        //}

        private void SoundIsDone(object sender, EventArgs e)
        {
            string Name = (string)sender;
            RemoveEntry(Name);
            SoundEntry newsound = new SoundEntry();
            newsound.SoundName = Name;
            newsound.HasBeenPlayed = true; //Mark it as done
            MySounds.Add(newsound);
        }

        private void RemoveEntry(string Name)
        {
            if (Name == null || Name == "") return;
            for (int count = MySounds.Count - 1; count >= 0; count--)
            {
                if (MySounds[count].SoundName == Name)
                {
                    MySounds.RemoveAt(count);
                }
            }
        }
        private void RegisterSound(string Name)
        {
            if (Name == null || Name == "") return;
            RemoveEntry(Name);
            SoundEntry newsound = new SoundEntry();
            newsound.SoundName = Name;
            newsound.HasBeenPlayed = false;
            MySounds.Add(newsound);
        }

        private void RegisterSoundDone(string Name)
        {
            if (Name == null || Name == "") return;
            RemoveEntry(Name);
            SoundEntry newsound = new SoundEntry();
            newsound.SoundName = Name;
            newsound.HasBeenPlayed = true;
            MySounds.Add(newsound);
        }

        /// <summary>
        /// Check to see if the specified sound has finished playing
        /// </summary>
        /// <param name="Name">The name of the sound</param>
        /// <returns>True if the sound is not currently playing.  False if it is currently playing.</returns>
        public bool SoundIsFinished(string Name)
        {
            foreach(SoundEntry one in MySounds.ToList())
            {
                if (one.SoundName == Name)
                    return one.HasBeenPlayed;
            }
            return true; //It does not exist, therefore it is not playing
        }

        /// <summary>
        /// Pause everything.  It loops through all the sprites in the SpriteController and sends the specified
        /// SpritePauseType to each one.  Look at the documentation for SpritePauseType to determine which pause
        /// type to use.
        /// </summary>
        /// <param name="What">The SpritePauseType to send all sprites</param>
        public void Pause(SpritePauseType What = SpritePauseType.PauseAll)
        {
            for(int i=0; i< Sprites.Count; i++)
            {
                Sprites[i].Pause(What);
            }
        }
        /// <summary>
        /// un-Pause everything.  This will send the specified SpritePauseType unpause command
        /// to all sprites.
        /// </summary>
        /// <param name="What">The SpritePauseType to unpause for all sprites</param>
        public void UnPause(SpritePauseType What = SpritePauseType.PauseAll)
        {
            for (int i = 0; i < Sprites.Count; i++)
            {
                Sprites[i].UnPause(What);
            }
        }


        internal void SortSprites()
        {
            //Define the sort we use if we do not have another one specified
            Comparison<Sprite> G = null;
            //G = delegate (Sprite first, Sprite second) {
            //    if(first.Zvalue != second.Zvalue) return first.Zvalue.CompareTo(second.Zvalue);
            //    if (first.BaseImageLocation.Y != second.BaseImageLocation.Y) return first.BaseImageLocation.Y.CompareTo(second.BaseImageLocation.Y);
            //    return first.BaseImageLocation.X.CompareTo(second.BaseImageLocation.X);
            //};
            G = delegate (Sprite first, Sprite second) { return first.Zvalue.CompareTo(second.Zvalue); }; 
            if (SpriteComparisonDelegate != null)
                G = SpriteComparisonDelegate;            
            //Sprites.Sort((x, y) => x.Zvalue.CompareTo(y.Zvalue));
            Sprites.Sort(G);
        }
    }

}
