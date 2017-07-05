using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteLibrary
{
    /// <summary>
    /// The SpritePayload is a stub of a class, for storing user-defined data and functions along with a sprite.
    /// </summary>
    /// <example>
    /// Basically, you want to "override" this class.  You do this by making your
    /// own class that looks something like:  
    /// <code lang="C#">
    /// public class TankPayload : SpritePayload 
    /// { 
    ///     public int Armor = 20; 
    ///     public int FireTime = 100; 
    /// } 
    /// </code>
    /// And then you add that to your sprite:  
    /// <code lang="C#">TankSprite.Payload = new TankPayload(); </code>
    /// If there is no payload, then the payload 
    /// property is null.  If you have multiple types of SpritePayloads, you may need to do something like: 
    /// <code lang="C#">
    /// if(TankSprite.payload != null and TankSprite.payload is TankPayload) 
    /// { 
    ///     TankPayload tPayload = (TankPayload)TankSprite.payload; tPayload.Armor--; 
    /// }
    /// </code>
    /// </example>
    public class SpritePayload
    {
    }
}
