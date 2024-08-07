/*
 * Copyright (C) 2024 Trent University. All Rights Reserved.
 *
 * Author(s):
 *  - Matthew Brown <matthewbrown@trentu.ca>
 */

namespace LabViz.Rendering;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


class Button
{
    public Rectangle Bounds { get; set; }
    public Texture2D Texture { get; private set; }
    public Action? Action { get; set; }

    // Used for flipping sprites around
    public SpriteEffects SpriteEffects { get; set; } = SpriteEffects.None;

    private bool _enabled = true;
    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            if (!_enabled) CurrentState = State.Normal;
        }
    }

    // ------------------------------------------------

    public enum State
    {
        Normal,
        Hovered,
        Pressed,
    }

    public State CurrentState { get; protected set; }

    // ------------------------------------------------

    public Button(Texture2D texture)
    {
        Texture = texture;
        Bounds = new(0, 0, texture.Width, texture.Height);
    }

    public Button(Texture2D texture, int x, int y, int size)
    {
        Texture = texture;
        Bounds = new Rectangle(x, y, size, size);
    }

    // ------------------------------------------------

    public void Draw(SpriteBatch spriteBatch)
    {
        Color tint = Color.White;
        int yOffset = CurrentState switch
        {
            State.Normal => 0,
            State.Hovered => -1,
            State.Pressed => +2,
            _ => 0,
        };

        if (!Enabled)
        {
            yOffset = 0;
            tint = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        }

        Rectangle drawPos = new(Bounds.X, Bounds.Y + yOffset, Bounds.Width, Bounds.Height);
        spriteBatch.Draw(Texture, drawPos, null, tint, 0.0f, Vector2.Zero, SpriteEffects, 0.0f);
        // spriteBatch.Draw(Texture, drawPos, tint);
    }

    // ------------------------------------------------

    public void Update(MouseState mouse)
    {
        if (!Enabled)
            return;

        (int l, int r) = (Bounds.X, Bounds.X + Bounds.Width);
        (int t, int b) = (Bounds.Y, Bounds.Y + Bounds.Height);
        bool inBoundsH = l <= mouse.X && mouse.X <= r;
        bool inBoundsV = t <= mouse.Y && mouse.Y <= b;

        bool mouseOver = inBoundsH && inBoundsV;
        bool mouseDown = mouse.LeftButton == ButtonState.Pressed;

        switch (CurrentState)
        {
            // button not touched, mouse is now over it
            case State.Normal when mouseOver:
                CurrentState = State.Hovered;
                break;

            // button hovered (not pressed), but mouse has left
            case State.Hovered when !mouseOver:
                CurrentState = State.Normal;
                break;

            // button wasn't pressed, but mouse is now pressed & over button
            case not State.Pressed when mouseOver && mouseDown:
                CurrentState = State.Pressed;
                break;

            // button pressed, mouse has been released while still over button
            case State.Pressed when !mouseDown && mouseOver:
                CurrentState = State.Hovered;
                Action?.Invoke();
                break;

            // button pressed and mouse has been released, but was not released not over the button
            case State.Pressed when !mouseDown:
                CurrentState = State.Normal;
                break;

            // All other fallbacks
            case State.Normal when !mouseOver: // button not touched, mouse not even over it
            case State.Pressed when mouseDown: // button pressed, but mouse is still down
            case State.Hovered when mouseOver: // button hovered, mouse still over it
                // Do nothing.
                break;

            default:
                throw new NotImplementedException("Uncaught button state");
        }
    }
}
