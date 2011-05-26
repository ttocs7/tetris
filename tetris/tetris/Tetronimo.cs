using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace tetris
{
    class Tetronimo
    {
        //tetronimo class:  contains information regarding
        //the tetronimo currently in play:  shape, location, etc.
        //Also referenced when drawing the next/held pieces
        public Globals.coords location;

        private int rotation;

        private int[,] _shape;
        public int[,] shape
        {
            get { return _shape; }
        }

        private int _width;
        public int width
        {
            get { return _width; }
        }

        private int _height;
        public int height
        {
            get { return _height; }
        }

        private int _yoffset;
        public int yoffset
        {
            get { return _yoffset; }
        }

        private int[][,] shapeArray = new int[4][,];
        private static int[][][,] allshapes = new int[7][][,];

        private int _type;
        public int type
        {
            get { return _type; }
        }

        static Tetronimo()
        {
            allshapes[0] = new int[][,]
            {
                new int[,] { {0,0,0,0}, {1,1,1,1}, {0,0,0,0}, {0,0,0,0} },
                new int[,] { {0,0,1,0}, {0,0,1,0}, {0,0,1,0}, {0,0,1,0} },
                new int[,] { {0,0,0,0}, {1,1,1,1}, {0,0,0,0}, {0,0,0,0} },
                new int[,] { {0,0,1,0}, {0,0,1,0}, {0,0,1,0}, {0,0,1,0} }
            };
            allshapes[1] = new int[][,]
            {
                new int[,] { {0,0,2,0}, {0,0,2,0}, {0,2,2,0}, {0,0,0,0} },
                new int[,] { {0,2,0,0}, {0,2,2,2}, {0,0,0,0}, {0,0,0,0} },
                new int[,] { {0,0,2,2}, {0,0,2,0}, {0,0,2,0}, {0,0,0,0} },
                new int[,] { {0,0,0,0}, {0,2,2,2}, {0,0,0,2}, {0,0,0,0} }
            };
            allshapes[2] = new int[][,]
            {
                new int[,] { {0,3,0,0}, {0,3,0,0}, {0,3,3,0}, {0,0,0,0} },
                new int[,] { {0,0,0,0}, {3,3,3,0}, {3,0,0,0}, {0,0,0,0} },
                new int[,] { {3,3,0,0}, {0,3,0,0}, {0,3,0,0}, {0,0,0,0} },
                new int[,] { {0,0,3,0}, {3,3,3,0}, {0,0,0,0}, {0,0,0,0} }
            };
            allshapes[3] = new int[][,]
            {
                new int[,]{ {0,0,0,0}, {0,4,4,0}, {0,4,4,0}, {0,0,0,0} },
                new int[,]{ {0,0,0,0}, {0,4,4,0}, {0,4,4,0}, {0,0,0,0} },
                new int[,]{ {0,0,0,0}, {0,4,4,0}, {0,4,4,0}, {0,0,0,0} },
                new int[,]{ {0,0,0,0}, {0,4,4,0}, {0,4,4,0}, {0,0,0,0} }
            };

            allshapes[4] = new int[][,]
            {
                new int[,] { {0,0,0,0}, {0,0,5,5}, {0,5,5,0}, {0,0,0,0} },
                new int[,] { {0,5,0,0}, {0,5,5,0}, {0,0,5,0}, {0,0,0,0} },
                new int[,] { {0,0,0,0}, {0,0,5,5}, {0,5,5,0}, {0,0,0,0} },
                new int[,] { {0,5,0,0}, {0,5,5,0}, {0,0,5,0}, {0,0,0,0} }
            };
            allshapes[5] = new int[][,]
            {
                new int[,] { {0,0,0,0}, {0,0,6,0}, {0,6,6,6}, {0,0,0,0} },
                new int[,] { {0,0,0,0}, {0,0,6,0}, {0,0,6,6}, {0,0,6,0} },
                new int[,] { {0,0,0,0}, {0,0,0,0}, {0,6,6,6}, {0,0,6,0} },
                new int[,] { {0,0,0,0}, {0,0,6,0}, {0,6,6,0}, {0,0,6,0} }
            };
            allshapes[6] = new int[][,]
            {
                new int[,] { {0,0,0,0}, {0,7,7,0}, {0,0,7,7}, {0,0,0,0} },
                new int[,] { {0,0,7,0}, {0,7,7,0}, {0,7,0,0}, {0,0,0,0} },
                new int[,] { {0,0,0,0}, {0,7,7,0}, {0,0,7,7}, {0,0,0,0} },
                new int[,] { {0,0,7,0}, {0,7,7,0}, {0,7,0,0}, {0,0,0,0} }
            };
        }
        public Tetronimo(int type)
        {
           this._type = type;
           shapeArray = allshapes[type];
           _shape = shapeArray[0];
           location.x = Globals.FIELD_WIDTH / 2;
           location.y = 0;
           rotation = 0;
           _width = 0;
           _height = 0;
           GetDimensions();
        }

        public Tetronimo(Tetronimo oldPiece)
        {
            _shape = new int[4, 4];
            shapeArray = allshapes[oldPiece._type];
            rotation = oldPiece.rotation;
            this._shape = shapeArray[rotation];
            location = oldPiece.location;
            _width = oldPiece.width;
            _height = oldPiece.height;
            GetDimensions();
        }

        public static int[,] getShape(int type)
        {
            return allshapes[type][0];
        }

        public void GetDimensions()
        {
            int minX = Globals.PIECE_HEIGHT, minY = Globals.PIECE_WIDTH; 
            int maxX = 0, maxY = 0;

            for (int j = 0; j < 4; ++j)
            {
                for (int i = 0; i < 4; ++i)
                {
                    if (shape[i, j] > 0)
                    {
                        minX = Math.Min(j, minX);
                        minY = Math.Min(i, minY);

                        maxX = Math.Max(j, maxX);
                        maxY = Math.Max(i, maxY);
                    }
                }
            }

            _width = maxX - minX + 1;
            _height = maxY - minY + 1;
            _yoffset = minX;             
        }

        public int[,] getRotation(rotType dir)
        {
            int target = rotation;
            if (dir == rotType.CW)
                target++;
            if (dir == rotType.CCW)
                target--;

            return shapeArray[(target + 4) % 4];
        }

        public int[,] setRotation(rotType dir)
        {
            int target = rotation;
            if (dir == rotType.CW)
                target++;
            if (dir == rotType.CCW)
                target--;

            rotation = (target + 4) % 4;
            _shape = shapeArray[rotation];
            GetDimensions();
            return _shape;
        }

    }
}
