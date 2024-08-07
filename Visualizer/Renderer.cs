/*
 * Copyright (C) 2024 Trent University. All Rights Reserved.
 *
 * Author(s):
 *  - Matthew Brown <matthewbrown@trentu.ca>
 */

namespace LabViz.Rendering;

// using System.Diagnostics; // For `Debug.WriteLine`
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


public class Renderer : Game
{
    // Fields for rendering
    protected readonly GraphicsDeviceManager graphics;
    protected SpriteBatch spriteBatch;
    public string WindowTitle { set => Window.Title = value; }

    protected readonly int WindowWidth;
    protected readonly int WindowHeight;

    // Playback state
    private bool playing;
    public bool IsPlaying
    {
        get => playing;
        set
        {
            playing = value;
            ResetButtonStates();
        }
    }

    public bool IsPaused { get => !IsPlaying; set => IsPlaying = !value; }

    protected TimeSpan LastStepTime;

    // Animation state
    protected readonly int[] ItemValues;
    protected readonly int MinItemValue;
    protected readonly int MaxItemValue;

    protected readonly List<Frame> Frames;
    public int LastFrameNum { get => Frames.Count - 1; }

    private int frame;
    protected int FrameNumber
    {
        get => frame;
        set
        {
            bool validRange = 0 <= value && value < Frames.Count;
            if (!validRange)
                return;

            // Starting at the current frame, loop forward/backward to the destination frame. If we encounter any swap
            // frames, apply them to the items.
            if (frame <= value)
                for (int i = frame + 1; i <= value; i++) // i.e. 5->8; apply frames 6, 7, and 8
                    Frames[i].ApplyMutation(ItemValues);
            else
                for (int i = frame; i > value; i--) // i.e. 8->5; undo frames 8, 7, and 6
                    Frames[i].UndoMutation(ItemValues);

            frame = value;

            if (IsPlaying && frame >= LastFrameNum)
            {
                IsPaused = true; // pause if we hit the end
            }

            // --- Replacement for printing the text at the top while font is disabled ---
            // ---------------------------------------------------------------------------

            var desc = CurrentFrame.Describe(ItemValues) ?? "Initial state.";
            var text = $"Step {frame}: {desc}";
            Console.WriteLine(text);

            ResetButtonStates();
        }
    }

    protected Frame CurrentFrame { get => Frames[FrameNumber]; }

    // Animation configuration
    public TimeSpan FrameDelay = new(0);

    // A 1x1 texture of solid white, tinted different colours to draw filled rectangles.
    protected Texture2D boxTexture;

    // --- Font disabled to work without FreeType dependency ---
    // ---------------------------------------------------------

    // protected SpriteFont mainFont;

    protected static readonly Color BoxColorNormal = Color.White;
    protected static readonly Color BoxColorCompare = Color.Red;
    protected static readonly Color BoxColorSwapped = Color.LimeGreen;
    protected static readonly Color BoxColorPivot = Color.Blue;
    protected static readonly Color BoxColorHighlight = new(255, 255, 100);

    // Buttons
    private protected Button buttonPause;
    private protected Button buttonPlay;
    private protected Button buttonNext;
    private protected Button buttonPrev;

    private void ResetButtonStates()
    {
        buttonPrev.Enabled = !playing && FrameNumber > 0;
        buttonNext.Enabled = !playing && FrameNumber < LastFrameNum;
        buttonPlay.Enabled = !playing;
        buttonPause.Enabled = playing;
    }

    protected Rectangle mainRegion;             // Area where the main game is drawn; gives space for the buttons.
    protected const int buttonSize = 36;        // How large the buttons should be.
    protected const int buttonSpacing = 4;      // The space between each button.
    protected const int regionMargin = 4;       // How much space between the edges of any regions/the window edges.
    protected const int textRegionSize = 48;    // How tall the area for the text is.


    // --- Textures disabled to work without FreeImage dependency ---
    // --------------------------------------------------------------

    // // Onscreen control textures come from kenney.nl and are CC0 licensed (public domain)
    // // https://kenney.nl/assets/onscreen-controls
    // private const string texPathPause = "kenney-pause";
    // private const string texPathSkip = "kenney-skip";
    // private const string texPathPlay = "kenney-play";
    // private const string fontPath = "MainFont";


    #region Constructor

    // Suppressed because they are initialized in `Initialize()` instead:
#pragma warning disable CS8618 // Non-nullable fields must have value when exiting constructor.

    public Renderer(int[] sortedItems, List<Frame> frames, int windowWidth, int windowHeight)
    {
        // Animation and playback state
        // ----------------------------

        ItemValues = sortedItems;
        Frames = frames;

        MinItemValue = 0;
        MaxItemValue = 0;
        for (int i = 0; i < ItemValues.Length; i++)
        {
            MinItemValue = Math.Min(MinItemValue, ItemValues[i]);
            MaxItemValue = Math.Max(MaxItemValue, ItemValues[i]);
        }

        frame = frames.Count - 1; // We start on the last frame since we are given the already-sorted items
        playing = false;
        LastStepTime = TimeSpan.Zero;

        // Window and graphics setup
        // -------------------------

        WindowWidth = windowWidth;
        WindowHeight = windowHeight;

        graphics = new GraphicsDeviceManager(this)
        {
            IsFullScreen = false,
            PreferredBackBufferWidth = WindowWidth,
            PreferredBackBufferHeight = WindowHeight,
            // GraphicsProfile = GraphicsProfile.HiDef,
            // PreferMultiSampling = true,
            // -- Can't get multi-sampling (AA) to work without it messing up the sprite colours, sadly.
        };

        graphics.ApplyChanges();

        IsMouseVisible = true;
        Content.RootDirectory = "Content";
        Window.AllowUserResizing = false;
    }

#pragma warning restore CS8618 // Non-nullable fields must have value when exiting constructor.

    #endregion


    #region Initialize

    protected override void Initialize()
    {
        // Window and graphics setup
        // -------------------------

        spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create textures
        // ---------------

        boxTexture = new Texture2D(GraphicsDevice, 1, 1);
        boxTexture.SetData(new Color[] { Color.White });

        // Texture2D playTexture = Content.Load<Texture2D>(texPathPlay);
        // Texture2D skipTexture = Content.Load<Texture2D>(texPathSkip);
        // Texture2D pauseTexture = Content.Load<Texture2D>(texPathPause);
        // mainFont = Content.Load<SpriteFont>(fontPath);

        Texture2D playTexture = RawTextureData.BytesToTexture(GraphicsDevice, RawTextureData.playButton);
        Texture2D skipTexture = RawTextureData.BytesToTexture(GraphicsDevice, RawTextureData.skipButton);
        Texture2D pauseTexture = RawTextureData.BytesToTexture(GraphicsDevice, RawTextureData.pauseButton);

        // Configure layout
        // ----------------

        mainRegion = new Rectangle(
            x: regionMargin,                        // inset from left
            y: textRegionSize + regionMargin * 2,   // inset from top, including text region
            width: WindowWidth - regionMargin * 2,  // subtract window edges
            height: WindowHeight - buttonSize - textRegionSize - regionMargin * 4  // subtract other regions + edges
        );

        static int buttonX(int i) => regionMargin + (buttonSize + buttonSpacing) * i;
        int buttonY = WindowHeight - regionMargin - buttonSize; // at bottom

        buttonPause = new Button(pauseTexture, buttonX(0), buttonY, buttonSize) { Enabled = false, };
        buttonPrev = new Button(skipTexture, buttonX(1), buttonY, buttonSize) { Enabled = true, SpriteEffects = SpriteEffects.FlipHorizontally };
        buttonNext = new Button(skipTexture, buttonX(2), buttonY, buttonSize) { Enabled = true, };
        buttonPlay = new Button(playTexture, buttonX(3), buttonY, buttonSize) { Enabled = true, };

        // Configure button actions
        // ------------------------

        // Relies on setter properties to perform more complex logic
        buttonNext.Action = () => FrameNumber += 1;
        buttonPrev.Action = () => FrameNumber -= 1;
        buttonPause.Action = () => IsPaused = true;
        buttonPlay.Action = () =>
        {
            // If the play button is pressed while paused at the end, reset playback (but stay paused).
            if (IsPaused && FrameNumber == LastFrameNum)
                FrameNumber = 0;
            else
                IsPaused = false;
        };

        // Reset animation
        // ---------------

        // Now that all other setup is done, run the setter for FrameNumber to reset back to initial state
        FrameNumber = 0;

        base.Initialize();
    }

    #endregion


    #region Update

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyState = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();

        if (keyState.IsKeyDown(Keys.Escape))
            Exit();

        if (IsPlaying && gameTime.TotalGameTime - LastStepTime >= FrameDelay)
        {
            FrameNumber += 1; // will pause automatically when it hits the end
            LastStepTime = gameTime.TotalGameTime;
        }

        buttonPause.Update(mouseState);
        buttonNext.Update(mouseState);
        buttonPrev.Update(mouseState);
        buttonPlay.Update(mouseState);

        base.Update(gameTime);
    }

    #endregion


    #region Draw

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        DrawBoxes();
        DrawButtons();
        // DrawText();

        base.Draw(gameTime);
    }

    private void DrawBoxes()
    {
        const int boxGap = 1; // The spacing between each box

        int n = ItemValues.Length;

        // There are n-1 gaps plus the borders; need to account for that before dividing the width evenly.
        int workingWidth = mainRegion.Width - boxGap * (n - 1);
        int baseBoxWidth = workingWidth / n; // i.e. 800/64 = 12 pixels each;
        int numWithExtra = workingWidth % n; // plus an extra 1 pixel on the first 32 boxes.

        // For height, there are only border at the top and bottom.
        int workingHeight = mainRegion.Height;

        // Used for both drawing boxes and marker line
        int GetHeight(int value) => (int)(value / (double)MaxItemValue * workingHeight);

        // Main drawing loop for the boxes; instead of trying to calculate the X-position of each box, just count up
        // as we draw them.
        spriteBatch.Begin();

        for (int i = 0; i < n; i++)
        {
            int itemValue = ItemValues[i];

            // Positioning
            // -----------

            int width = baseBoxWidth + (i < numWithExtra ? 1 : 0);
            int height = GetHeight(itemValue);

            // There are `i` boxes that come before this one, so there are `i` gaps and `i` baseBoxWidths. Some of
            // the boxes have an extra pixel; if this is box 48, but only 32 boxes have extra, then there are 32
            // with that extra pixel. Finally, there's a border on the left.
            int x = mainRegion.X + (baseBoxWidth + boxGap) * i + Math.Min(numWithExtra, i);
            int y = mainRegion.Y + mainRegion.Height - height; // Pull up from the bottom of the area.

            Rectangle boxPos = new(x, y, width, height);
            Color boxColor = CurrentFrame.GetBoxColor(i, itemValue);
            spriteBatch.Draw(boxTexture, boxPos, boxColor);
        }

        // If there is a marked value, draw a line across
        if (CurrentFrame.MarkedValue is (int markedVal, _))
        {
            int h = GetHeight(markedVal);
            int y = mainRegion.Y + mainRegion.Height - h;
            Rectangle linePos = new(regionMargin, y, mainRegion.Width, 2);
            spriteBatch.Draw(boxTexture, linePos, Color.Blue);
        }

        spriteBatch.End();
    }

    private void DrawButtons()
    {
        spriteBatch.Begin();

        buttonPause.Draw(spriteBatch);
        buttonNext.Draw(spriteBatch);
        buttonPrev.Draw(spriteBatch);
        buttonPlay.Draw(spriteBatch);

        spriteBatch.End();
    }

    // private void DrawText()
    // {
    //     spriteBatch.Begin();

    //     var desc = CurrentFrame.Describe(ItemValues) ?? "Initial state.";
    //     var pos = new Vector2(regionMargin, regionMargin);
    //     spriteBatch.DrawString(mainFont, $"Step {FrameNumber}: {desc}", pos, Color.White);

    //     spriteBatch.End();
    // }

    #endregion
}
