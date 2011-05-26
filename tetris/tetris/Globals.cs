using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace tetris
{
    class Globals
    {

        public struct coords
        {
            private int xval;
            public int x
            {
                get
                {
                    return xval;
                }
                set
                {
                    xval = value;
                }
            }

            private int yval;
            public int y
            {
                get
                {
                    return yval;
                }
                set
                {
                    yval = value;
                }
            }
        }

        public const int FIELD_WIDTH = 10;
        public const int FIELD_HEIGHT = 20;

        public const int PIECE_WIDTH = 4;
        public const int PIECE_HEIGHT = 4;

        public const int PADDING = 3;

        public static GraphicsDeviceManager graphics;
        public static SpriteBatch spriteBatch;
        public static ContentManager content;

        public static SpriteFont font;

        public static Texture2D blanksprite;
        public static Texture2D menusprite;

        public static Vector2 fieldoffset = new Vector2(15f, 15f);
    }
}
