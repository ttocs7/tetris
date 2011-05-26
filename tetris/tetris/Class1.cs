using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace tetris
{
    class Menu
    {
        double transitionlevel = 0.0f;
        double boxtransitionlevel = 0.0f;
        TimeSpan fadetime, growtime;
        SpriteBatch spriteBatch;
        Game1 game;
        Texture2D blanksprite, shadowsprite;
        KeyboardState oldstate;
        bool exit = false;


        public Menu(Game1 game)
        {
            fadetime = TimeSpan.FromSeconds(0.2);
            growtime = TimeSpan.FromSeconds(0.5);
            this.game = game;
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            blanksprite = game.Content.Load<Texture2D>("blanksprite");
            shadowsprite = game.Content.Load<Texture2D>("shadowsprite");
        }

        public bool Update(GameTime gameTime)
        {
            if(transitionlevel <= 1)
                transitionlevel += gameTime.ElapsedGameTime.TotalMilliseconds / fadetime.TotalMilliseconds;

            if (exit)
                return false;

            return true;
        }

        public void HandleInput()
        {
            KeyboardState newState = Keyboard.GetState();
            if (newState != oldstate)
            {
                if (newState.IsKeyDown(Keys.F1))
                {
                    exit = true;
                }
            }
            oldstate = newState;
        }
        

        public void Draw(GameTime gameTime)
        {
            float currentWidth = 600 * (float)transitionlevel;
            float currentHeight = 400 * (float)transitionlevel;
            spriteBatch.Begin();
            spriteBatch.Draw(blanksprite, new Rectangle(0, 0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height), Color.Black * (float)transitionlevel);
            spriteBatch.Draw(shadowsprite, new Rectangle(200, 200, (int)currentWidth, (int)currentHeight), Color.White);
            spriteBatch.End();
        }
    }
}
