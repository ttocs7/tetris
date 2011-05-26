using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

namespace tetris
{
    public enum rotType { CW, CCW, NoRotation };

    public enum direction { NEW_PIECE, LEFT, RIGHT, DOWN, RotateCW, 
        HARD_DROP, RotateCCW, TICK_DOWN };

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        //counter var for timer
        double moveTime = 0;
        double moveRate = 1.5;
        int level = 1;

        //init field object
        Field field = new Field();

        //check to see if I need to send a new piece
        bool newPiece = true;

        //texture for sprite
        Texture2D blocksprite;
        Texture2D shadowsprite;

        //input module (kbInput.cs)
        KBInput kbInput = new KBInput();

        //pieces/states
        Tetronimo currPiece = null;
        int nextPiece;
        int heldPiece = -1;

        //flag to prevent spamming the held key
        bool justHeld = false;

        int score;
        int totalLines;

        int startTime = -1, countdown;
        enum gameState { NEW_GAME, IN_PROGRESS, GAME_OVER, PAUSED, COUNTDOWN };
        gameState state = gameState.NEW_GAME;
        
        Color[] colors = new Color[]{ Color.White, Color.Cyan, Color.Blue, Color.Orange, 
            Color.Yellow, Color.Green, Color.Purple, Color.Red };

        Globals.coords hardDropLocation;
        Globals.coords nextLocation;
        Globals.coords heldLocation;

        Menu menu;

        List<string> pausedItems;
        List<string> mainMenuItems;
        List<string> gameOverItems;
        List<string> instructionItems;

        //flag for DrawInstruction
        int instructionCount;

        //random number generator!
        Random random = new Random();

        

        public Game1()
        {
            Globals.graphics = new GraphicsDeviceManager(this);
            Globals.content = new ContentManager(Services);
            Globals.content.RootDirectory = "Content";
            Globals.graphics.PreferredBackBufferWidth = 1024;
            Globals.graphics.PreferredBackBufferHeight = 768;
        }

        protected override void Initialize()
        {
            //this is where I'd open any localization files and input data for the menus
            pausedItems = new List<string>();
            pausedItems.Add("PAUSED");
            pausedItems.Add("Continue Game");
            pausedItems.Add("Exit Game");

            mainMenuItems = new List<string>();
            mainMenuItems.Add("A Game of Blocks and Lines");
            mainMenuItems.Add("New Game");
            mainMenuItems.Add("Exit Game");

            gameOverItems = new List<string>();
            gameOverItems.Add("Game Over!");
            gameOverItems.Add("New Game");
            gameOverItems.Add("Exit Game");

            instructionItems = new List<string>();
            instructionItems.Add("Up - Rotate Clockwise");
            instructionItems.Add("Left/Right - Move Piece");
            instructionItems.Add("Down - Move Down (Soft Drop)");
            instructionItems.Add("Space - Hard Drop");
            instructionItems.Add("X - Rotate Counter-Clockwise");
            instructionItems.Add("C - Hold Piece");
            instructionItems.Add("Esc - Pause");

            //so I don't calculate this every frame, dtermining the upper
            //bound counter for DrawInstruction()
            if(instructionItems.Count % 2 == 1)
            {
                instructionCount = instructionItems.Count - 1;
            }
            else
            {
                instructionCount = instructionItems.Count;
            }

            menu = new Menu(mainMenuItems);

            nextLocation.x = 350;
            nextLocation.y = 55;

            heldLocation.x = 350;
            heldLocation.y = 225;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Globals.spriteBatch = new SpriteBatch(GraphicsDevice);

            //load sprites, font
            blocksprite = Globals.content.Load<Texture2D>("blocksprite");
            Globals.blanksprite = Globals.content.Load<Texture2D>("blanksprite");
            shadowsprite = Globals.content.Load<Texture2D>("shadowsprite");
            Globals.menusprite = Globals.content.Load<Texture2D>("menusprite");
            Globals.font = Globals.content.Load<SpriteFont>("font");

        }

        protected override void UnloadContent()
        {
            //blank, for now.
        }

        //Maybe separate out Holding stuff to another function?
        public void DispatchPiece(bool holding = false)
        {
            if (state == gameState.COUNTDOWN)
                nextPiece = random.Next(0, 7);

            if (holding)
            {
                if (heldPiece > -1)
                {
                    int temp = currPiece.type;
                    currPiece = new Tetronimo(heldPiece);
                    heldPiece = temp;
                }
                else
                {
                    heldPiece = currPiece.type;
                    currPiece = new Tetronimo(nextPiece);
                    nextPiece = random.Next(0, 7);
                }
                //don't want them to spam the hold key, let another piece go first.
                justHeld = true;
            }
            else
            {
                currPiece = new Tetronimo(nextPiece);
                nextPiece = random.Next(0, 7);
                //reset justHeld, we got a new piece
                justHeld = false;
            }
            //if the piece can't be drawn, it's blocked, game ends.
            if (!ValidMove(direction.NEW_PIECE))
            {
                menu = new Menu(gameOverItems);
                state = gameState.GAME_OVER;
            }
            else
            {
                hardDropLocation = field.getHardDrop(currPiece.shape, currPiece.location);
            }
        }


        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
#if XBOX
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
#endif
            switch (state)
            {
                case gameState.IN_PROGRESS:
                    InProgressUpdate(gameTime);
                    break;
                case gameState.PAUSED:
                    PausedUpdate(gameTime);
                    break;
                case gameState.GAME_OVER:
                    GameOverUpdate(gameTime);
                    break;
                case gameState.NEW_GAME:
                    MainMenuUpdate(gameTime);
                    break;
                case gameState.COUNTDOWN:
                    if (startTime == -1)
                    {
                        startTime = (int)gameTime.TotalGameTime.TotalSeconds + 3;
                    }
                    countdown = startTime - (int)gameTime.TotalGameTime.TotalSeconds;
                    if (countdown <= 0)
                    {
                        startTime = -1;
                        state = gameState.IN_PROGRESS;
                    }
                    break;
            }

                base.Update(gameTime);
        }

        private void PausedUpdate(GameTime gameTime)
        {
            int returnVal = menu.Update(gameTime, true);
            switch (returnVal)
            {
                case 1:  //continue
                case -1:
                    moveTime = gameTime.TotalGameTime.TotalSeconds + moveRate;
                    state = gameState.COUNTDOWN;
                    menu = null;
                    break;
                case 2: //exit
                    this.Exit();
                    break;
            }

        }

        private void GameOverUpdate(GameTime gameTime)
        {
            int returnVal = menu.Update(gameTime, false);
            switch(returnVal)
            {
                case 1: //new game
                    field = new Field();
                    state = gameState.COUNTDOWN;
                    menu = null;
                    break;
                case 2:
                    this.Exit();
                    break;
            }
        }

        private void MainMenuUpdate(GameTime gameTime)
        {
            int returnVal = menu.Update(gameTime, false);
            switch (returnVal)
            {
                case 1: //new game
                    field = new Field();
                    state = gameState.COUNTDOWN;
                    menu = null;
                    break;
                case 2:
                    this.Exit();
                    break;
            }
        }

        private void InProgressUpdate(GameTime gameTime)
        {
            kbInput.UpdateWithNewState(Keyboard.GetState(), gameTime);
            if (!newPiece)
            {
                if (kbInput.WasKeyPressed(Keys.Left, true))
                    ValidMove(direction.LEFT);

                if (kbInput.WasKeyPressed(Keys.Right, true))
                    ValidMove(direction.RIGHT);

                if (kbInput.WasKeyPressed(Keys.Up, true))
                    ValidMove(direction.RotateCW);

                if (kbInput.WasKeyPressed(Keys.Down, true))
                    ValidMove(direction.DOWN);

                if (kbInput.WasKeyPressed(Keys.Space, false))
                    ValidMove(direction.HARD_DROP);

                if (kbInput.WasKeyPressed(Keys.X, true))
                    ValidMove(direction.RotateCCW);

                if (kbInput.WasKeyPressed(Keys.C, true))
                    if(!justHeld)
                    {
                        DispatchPiece(true);
                    }
                if (kbInput.WasKeyPressed(Keys.Escape, false))
                {
                    state = gameState.PAUSED;
                    menu = new Menu(pausedItems);
                }

                if (gameTime.TotalGameTime.TotalSeconds >= moveTime)
                {
                    ValidMove(direction.TICK_DOWN);
                    moveTime = gameTime.TotalGameTime.TotalSeconds + moveRate;
                }

            }
            else if (state != gameState.GAME_OVER)
            {
                if (currPiece != null)
                {
                    field.placePiece(currPiece.shape, currPiece.location);
                }
                int lines = field.checkLines();
                score += 50 * lines + (25 * lines);
                totalLines += lines;
                level = (totalLines / 10) + 1;
                moveRate = 1.50 - ((level - 1) * .1);
                DispatchPiece();
                newPiece = false;
                moveTime = gameTime.TotalGameTime.TotalSeconds + moveRate;
            }
        }

        private bool ValidMove(direction dir)
        {
            Globals.coords tempcoord = currPiece.location;
            int[,] shape = new int[4, 4];
            bool valid = false;

            //setup phase:  change coordinates or piece for checkPiece
            switch(dir)
            {
                //modify x/y values for pieces if necessary
                case direction.LEFT:
                    tempcoord.x = currPiece.location.x - 1;
                    shape = currPiece.shape;
                    break;
                case direction.RIGHT:
                    tempcoord.x = currPiece.location.x + 1;
                    shape = currPiece.shape;
                    break;
                case direction.TICK_DOWN:
                case direction.DOWN:
                    tempcoord.y = currPiece.location.y + 1;
                    shape = currPiece.shape;
                    break;
                case direction.RotateCW:
                    shape = currPiece.getRotation(rotType.CW);
                    break;
                case direction.HARD_DROP:
                    tempcoord = hardDropLocation;
                    shape = currPiece.shape;
                    newPiece = true;
                    break;
                case direction.NEW_PIECE:
                    tempcoord.x = 5;
                    tempcoord.y = 0;
                    shape = currPiece.shape;
                    break;
            }

            //hack:  if an I tetrionimo is positioned at X = -1, checking 
            //bounds results in an IndexOutofRangeException (since we're checking L to R).
            //should enable a temporary x-offset property that's generated 
            //when the temporary piece is rotated.
            int temp = shape[0,0] + shape[1,0] + shape[2,0] + shape[3,0];


            //action phase:  checkPiece
            if(tempcoord.x > -1 || temp == 0)
            {
                if (field.checkPiece(shape, tempcoord))
                {
                    currPiece.location = tempcoord;
                    valid = true;
                }
            }

            //finishing phase: move is valid, set the rotations.
            if (valid == true)
            {
                switch (dir)
                {
                    case direction.RotateCW:
                        currPiece.setRotation(rotType.CW);
                        break;
                    case direction.RotateCCW:
                        currPiece.setRotation(rotType.CCW);
                        break;
                }

                hardDropLocation = field.getHardDrop(currPiece.shape, currPiece.location);
            }
            else
            {
                if (dir == direction.TICK_DOWN)
                    newPiece = true;
            }

            return valid;

        }

        void DrawInstructions()
        {
            Globals.spriteBatch.Draw(Globals.menusprite, new Rectangle(150, 580, 700, 140), Color.White);
            int yPos = 560;
            int xPos;
            if (instructionItems.Count % 2 == 1)
            {
                instructionCount = instructionItems.Count - 1;
            }
            else
            {
                instructionCount = instructionItems.Count;
            }
            for (int i = 0; i < instructionCount; i += 2)
            {
                yPos += 30;
                xPos = 160;
                Globals.spriteBatch.DrawString(Globals.font, instructionItems[i], 
                    new Vector2(xPos, yPos), Color.White);

                xPos += 420;

                Globals.spriteBatch.DrawString(Globals.font, instructionItems[i + 1], 
                    new Vector2(xPos, yPos), Color.White);
            }
            if (instructionCount < instructionItems.Count) //odd number, draw the last one
            {
                Globals.spriteBatch.DrawString(Globals.font, instructionItems[instructionItems.Count-1], 
                    new Vector2(450, yPos + 30), Color.White);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Vector2 currPosition = Globals.fieldoffset;
            Vector2 shadowPosition = Vector2.Zero;
            Vector2 nextPosition;
            Vector2 heldPosition;

            Globals.spriteBatch.Begin();
            //backgrounds for the field borders
            Globals.spriteBatch.Draw(Globals.menusprite, new Rectangle(0, 0, 350, 780), Color.White);
            Globals.spriteBatch.Draw(Globals.menusprite, new Rectangle(350, 0, 143, 345), Color.White);
            //scoreboard
            Globals.spriteBatch.Draw(Globals.menusprite, new Rectangle(575, 175, 200, 135), Color.White);

            //black backgrounds for held/next
            Globals.spriteBatch.Draw(Globals.blanksprite, new Rectangle(350, 35, 128, 128), Color.White);
            Globals.spriteBatch.Draw(Globals.blanksprite, new Rectangle(350, 198, 128, 128), Color.White);
            //scoreboard
            Globals.spriteBatch.Draw(Globals.blanksprite, new Rectangle(590, 190, 170, 105), Color.White);

            //held/next text
            Globals.spriteBatch.DrawString(Globals.font, "Next Piece", new Vector2(364, 12), Color.White);
            Globals.spriteBatch.DrawString(Globals.font, "Held Piece", new Vector2(364, 173), Color.White);

            
            //rows loop (Y)
            for (int i = 0; i < field.fieldHeight-1; i++)
            {
                    //cols loop(X)
                    for (int j = 0; j <= field.fieldWidth-1; j++)
                    {
                        if (field.fieldArray[i, j] < 9)
                        {
                            if (field.fieldArray[i, j] > 0)
                            {
                                Globals.spriteBatch.Draw(blocksprite, currPosition, 
                                    colors[field.fieldArray[i, j]]);
                            }
                            else
                            {
                                Globals.spriteBatch.Draw(Globals.blanksprite, 
                                    currPosition, Color.White);
                            }
                            currPosition.X += 32.0f;
                        }
                        //sprites are 32x32
                    }
                    currPosition.Y += 32.0f;
                    currPosition.X = Globals.fieldoffset.X;
                }

                
                if(currPiece != null)
                {
                    for (int i = 0; i < Globals.PIECE_HEIGHT; i++)
                    {
                        for (int j = 0; j < Globals.PIECE_WIDTH; j++)
                        {
                            //current piece
                            currPosition.X = ((currPiece.location.x + j) - 1 ) 
                                * 32.0f + Globals.fieldoffset.X;

                            currPosition.Y = (currPiece.location.y + i) 
                                * 32.0f + Globals.fieldoffset.Y;

                            //hard drop shadow
                            shadowPosition.X = ((hardDropLocation.x + j) - 1)
                                * 32.0f + Globals.fieldoffset.X;

                            shadowPosition.Y = (hardDropLocation.y + i)
                                * 32.0f + Globals.fieldoffset.Y;

                            //next piece
                            nextPosition.X = nextLocation.x + (32f * j);
                            nextPosition.Y = nextLocation.y + (32f * i);

                            //held piece
                            heldPosition.X = heldLocation.x + (32f * j);
                            heldPosition.Y = heldLocation.y + (32f * i);

                            if (currPiece.shape[i, j] > 0)
                            {
                                Globals.spriteBatch.Draw(shadowsprite, shadowPosition,
                                    colors[currPiece.shape[i, j]]);
                                Globals.spriteBatch.Draw(blocksprite, currPosition,
                                    colors[currPiece.shape[i, j]]);
                            }
                            if (Tetronimo.getShape(nextPiece)[i, j] > 0)
                            {
                                Globals.spriteBatch.Draw(blocksprite, nextPosition,
                                    colors[Tetronimo.getShape(nextPiece)[i,j]]);
                            }
                            if (heldPiece > -1)
                            {
                                if (Tetronimo.getShape(heldPiece)[i, j] > 0)
                                {
                                    Globals.spriteBatch.Draw(blocksprite, heldPosition, 
                                        colors[Tetronimo.getShape(heldPiece)[i, j]]);
                                }
                            }
                        }
                    }
                }
                /*
                    Debug stuff.
                    Globals.spriteBatch.DrawString(Globals.font, "currPiece.location.x: " + 
                        currPiece.location.x.ToString(), new Vector2(600, 200), Color.White);
                  
                    Globals.spriteBatch.DrawString(Globals.font, "currPiece.location.y: " + 
                        currPiece.location.y.ToString(), new Vector2(600, 225), Color.White);
                  
                    Globals.spriteBatch.DrawString(Globals.font, "newPiece: " + 
                        newPiece.ToString(), new Vector2(600, 250), Color.White);
                  
                    Globals.spriteBatch.DrawString(Globals.font, "width: " + 
                        currPiece.width.ToString(), new Vector2(600, 300), Color.White);
                  
                    Globals.spriteBatch.DrawString(Globals.font, "yoffset: " + 
                        currPiece.yoffset.ToString(), new Vector2(600, 325), Color.White);
                  
                    Globals.spriteBatch.DrawString(Globals.font, "height: " + 
                        currPiece.height.ToString(), new Vector2(600, 350), Color.White);
                 
                    Globals.spriteBatch.DrawString(Globals.font, "moveTime: " + 
                        moveTime, new Vector2(500, 300), Color.White);
                
                    Globals.spriteBatch.DrawString(Globals.font, gameTime.TotalGameTime.TotalSeconds.ToString(), 
                        new Vector2(500, 325), Color.White);

                if (currPiece != null)
                {
                    Globals.spriteBatch.DrawString(Globals.font, "currPiece.location.x: " + 
                        currPiece.location.x.ToString(), new Vector2(600, 200), Color.White);
                    Globals.spriteBatch.DrawString(Globals.font, "currPiece.location.y: " + 
                        currPiece.location.y.ToString(), new Vector2(600, 225), Color.White);
                }
                 
                Globals.spriteBatch.DrawString(Globals.font, "fieldWidth: "
                        + field.fieldWidth.ToString(), new Vector2(600, 375), Color.White);
                Globals.spriteBatch.DrawString(Globals.font, "fieldHeight: "
                        + field.fieldHeight.ToString(), new Vector2(600, 400), Color.White);
            */

                Globals.spriteBatch.DrawString(Globals.font, "Score: " + score, new Vector2(600, 200), Color.White);
                Globals.spriteBatch.DrawString(Globals.font, "Lines: " + totalLines, new Vector2(600, 225), Color.White);
                Globals.spriteBatch.DrawString(Globals.font, "Level: " + level, new Vector2(600, 250), Color.White);

                
            //} 

            //menus!
            if(state == gameState.GAME_OVER || state == gameState.PAUSED
                || state == gameState.NEW_GAME)
            { 
                menu.Draw(gameTime);
                if (state == gameState.NEW_GAME)
                    DrawInstructions();
            }
            else if (state == gameState.COUNTDOWN)
            {
                Globals.spriteBatch.Draw(Globals.blanksprite, new Rectangle(0, 0, 
                    Globals.graphics.GraphicsDevice.Viewport.Width, Globals.graphics.GraphicsDevice.Viewport.Height), 
                    new Color(255, 255, 255, 192));

                Globals.spriteBatch.DrawString(Globals.font, countdown.ToString(), 
                    new Vector2(150, 200), Color.White, 0f, Vector2.Zero,1.5f, SpriteEffects.None, 0);
            }
            Globals.spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
