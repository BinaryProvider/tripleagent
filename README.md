# TripleAgent

TripleAgent is an open source help agent control, with the purpose to display and animate a sprite with an associated label on a Windows Form. The control can be used to present important information in a fun way. Inspiration has been taken from the old Microsoft Agents (Clippy, Merlin, Rover, etc.).

![TripleAgent Demo](https://i.snag.gy/8sBePN.jpg)

## Getting Started

Any spritesheet can be used, but ensure that each sprite in the sheet is the same size.

You can choose to defined each animation separately and add to the control, or load a XML definition of all available animations when you intialize the control.

### Example of animation data
```xml  
<animations>
    <animation name="Walk" loopcount="3">
        <startframe num="131" />
        <endframe num="139" />
    </animation>
    <animation name="Dance" loopcount="-1" loopdelay="0">
        <startframe num="183" />
        <endframe num="188" />
    </animation>
    <animation name="Shoot">
        <startframe x="320" y="64" />
        <endframe x="320" y="192" />
    </animation>
</animations>
```
Each animation is defined with a name as identifier and childnodes that define the start- and endframe of the animation. Note that you can choose to specify the exact frame number calculated from left to right, top to down, or by specifying the start coordinates of each frame. There are also options to loop the animation a specified number of times with the attribute loopcount (-1 is infinetely). Loopdelay specifies if the animation should pause for a moment on the last frame until the loop continues. 

### Example of control declaration
```C#
Size spriteSize = new Size(64, 64);
Point spriteLocation = new Point(0, 0);
int spriteStartFrame = 131;

agent = new TripleAgentControl(spriteSheet, spriteSize, spriteStartFrame, spriteLocation, animationFrameData);
Controls.Add(agent);
```

### Example of how to show an animated tooltip
```C#
agent.ShowTip(agent.SpriteAnimations[0], "Sample text");
```

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* SpriteLibrary ->
* SpriteSheet graphics ->
