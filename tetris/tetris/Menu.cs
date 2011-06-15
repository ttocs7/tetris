//Menu.cs
//Written by Scott Porcaro, 2011

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

        //Menu class:  All menus.  Except the instruction screen
        //below the main menu (see Game1.DrawInstructions())
        double transitionLevel = 0.0f;
       
        float boxwidth = 400;
        float boxheight = 300;

        TimeSpan fadetime;

        KBInput input;

        //Texture2D menusprite;

        bool exit = false;
        int selected = 0;
        List<string> items;
        int listCount;

        int blue = 255;
        float selScale = 1.29f;
        int direction = 1;

        int returnVal;
        
        public Menu(ContentManager content)
        {
            fadetime = TimeSpan.FromSeconds(0.2);
            
            //menusprite = Globals.content.Load<Texture2D>("menusprite");

            input = new KBInput();

            items = new List<string>();
            this.items.Add("PAUSED");
            this.items.Add("Continue Game");
            this.items.Add("Exit Game");

            listCount = 2;

        }

        public Menu(List<string> items)
        {

            fadetime = TimeSpan.FromSeconds(0.2);

            //menusprite = Globals.content.Load<Texture2D>("menusprite");
            
            input = new KBInput();

            this.items = new List<string>();
            this.items.AddRange(items);
            listCount = items.Count() - 1;
        }

        private void ResetSelection()
        {
            //resetting the highlight values for instant feedback
            blue = 255;
            selScale = 1.29f;
        }

        private void transition(int dir, GameTime gameTime)
        {
            transitionLevel += (gameTime.ElapsedGameTime.TotalMilliseconds 
                / fadetime.TotalMilliseconds) * dir;
        }

        public int Update(GameTime gameTime, bool escape)
        {
            if (transitionLevel <= 1 && !exit)
                transition(1, gameTime);
            else
            {
                KeyboardState newState = Keyboard.GetState();
                input.UpdateWithNewState(newState, gameTime);
                if (input.WasKeyPressed(Keys.Down, true))
                {
                    selected++;
                    ResetSelection();
                }
                if (input.WasKeyPressed(Keys.Up, true))
                {
                    selected--;
                    ResetSelection();
                }

                if (input.WasKeyPressed(Keys.Enter, false))
                {
                    exit = true;
                    returnVal = selected + 1;
                }
                if (escape)
                {
                    if (input.WasKeyPressed(Keys.Escape, false))
                    {
                        exit = true;
                        returnVal = -1;
                    }
                }
                selected = ((selected + listCount) % listCount);
            }

            //transition out if we're exiting, then return
            if (transitionLevel > 0 && exit)
                transition(-1, gameTime);
            else if (transitionLevel < 0)
                return returnVal;

            //determining blue value and direction (for size) of highlight effect
            if (blue >= 255)
            {
                direction = -1;
            }
            if (blue <= 0)
            {
                direction = 1;
            }

            blue = blue + (9 * direction);
            selScale = selScale + (.01f * direction);
            return 0;
        }

        void DrawString(string str, bool selected, int yPos)
        {
            Vector2 size = Globals.font.MeasureString(str);
            Color color = Color.White;
            float scale = 1f;
            if (selected)
            {
                color = new Color(255, 255, blue);
                scale = selScale;
                size.X *= scale;
            }
            Globals.spriteBatch.DrawString(Globals.font, str, new Vector2((Globals.graphics.GraphicsDevice.Viewport.Width / 2) - 
                (size.X / 2), yPos), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        public void Draw(GameTime gameTime)
        {
            float currentWidth = boxwidth * (float)transitionLevel;
            float currentHeight = boxheight * (float)transitionLevel;
            float backgroundTransition = (float)transitionLevel * .66f;

            Globals.spriteBatch.Draw(Globals.blanksprite, new Rectangle(0, 0, Globals.graphics.GraphicsDevice.Viewport.Width, 
                Globals.graphics.GraphicsDevice.Viewport.Height), Color.Black * backgroundTransition);

            Globals.spriteBatch.Draw(Globals.menusprite, new Rectangle((int)(Globals.graphics.GraphicsDevice.Viewport.Width/2 - currentWidth/2), 
                (int)(Globals.graphics.GraphicsDevice.Viewport.Height/2 - currentHeight/2), (int)currentWidth, (int)currentHeight), Color.White);

            if (transitionLevel > 1)
            {
                DrawString(items[0], false, 250);
                int i = 1;
                //should probably start yPos based on the Menu's top
                int yPos = 300;
                while (i <= listCount)
                {
                    //increment selected by 1, to adjust for title being items[0]
                    DrawString(items[i], (selected + 1 == i), yPos);
                    yPos+= 30;
                    i++;
                }
            }
        }
    }

}
