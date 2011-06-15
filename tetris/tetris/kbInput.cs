//kbInput.cs
//Written by Scott Porcaro, 2011

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace tetris
{
    class KBInput
    {    
        //Keyboard Input handler.  Nothing terribly fancy here.
        private KeyboardState prevState, currState;
        private long currentTime;
        private Dictionary<Keys, long> KeyDownTime = new Dictionary<Keys, long>();
        private double repeatTime = 200;

        public void UpdateWithNewState(KeyboardState newState, GameTime gameTime)
        {
            prevState = currState;
            currState = newState;
            currentTime = (long)gameTime.TotalGameTime.TotalMilliseconds;
        }

        public bool WasKeyPressed(Keys key, bool repeat)
        {
            if (currState.IsKeyDown(key))
            {
                if (!prevState.IsKeyDown(key) || ((repeat == true) && 
                    (currentTime - KeyDownTime[key] > repeatTime)))
                {
                    KeyDownTime[key] = currentTime;
                    return true;
                }
            }
            return false;
        }

    }
}
