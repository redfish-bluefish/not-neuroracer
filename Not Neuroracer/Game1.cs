using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

public struct Results
{
    public int shapesLevel;
    public int drivingLevel;

    public Results(int l, int d)
    {
        shapesLevel = l;
        drivingLevel = d;
    }
    
}

enum GameState
{
    MENU, INSTRUCTION, SHAPES, TEST
}

namespace NotNeuroRacer
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        ShapeGameController shapeGame;
        DrivingGameController drivingGame;
        BasicEffect basicEffect;
        Vector2 screenSize;
        bool wroteResults;

        GameState gameState;
        Texture2D instructionBackground, leftShape1, leftShape2, rightShape1, rightShape2;
        SpriteFont titleFont, descriptionFont;
        bool currSpace, prevSpace;

        string startTime, endTime;

        //path to the results file (in this case 
        string resultsFile = "results.csv";
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 768;
            graphics.PreferredBackBufferWidth = 1024;
            screenSize = new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);


            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related con                      tent.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            
            Results prevResults = readPreviousResults();
            //no dynamic changing of levels, but the ability to read from previous implemented
            shapeGame = new ShapeGameController();
            drivingGame = new DrivingGameController(GraphicsDevice);
            shapeGame.DifficultyLevel = prevResults.shapesLevel;
            drivingGame.DifficultyLevel = prevResults.drivingLevel;

            startTime = DateTime.Now.ToString("MM-dd/HH:mm");

            Vector2 center;
            center.X = GraphicsDevice.Viewport.Width * 0.5f;
            center.Y = GraphicsDevice.Viewport.Height * 0.5f;

            Matrix View = Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0));
            Matrix Projection = Matrix.CreateOrthographic(center.X * 2, center.Y * 2, -0.5f, 1);

            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.VertexColorEnabled = true;
            basicEffect.View = View;
            basicEffect.Projection = Projection;
            //basicEffect.Projection = Matrix.CreateOrthographic(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height, 0, 1);  // this says we are using an orthographic projection - looking at 0,0 in the center of the screen

            gameState = GameState.MENU;
            wroteResults = false;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            drivingGame.loadContent(Content);
            shapeGame.loadContent(Content);
            instructionBackground = Content.Load<Texture2D>("instruction_screen");
            titleFont = Content.Load<SpriteFont>("titleFont");
            descriptionFont = Content.Load<SpriteFont>("descriptionFont");
            rightShape1 = Content.Load<Texture2D>("greensquare");
            rightShape2 = Content.Load<Texture2D>("purplepentagon");
            leftShape1 = Content.Load<Texture2D>("greenpentagon");
            leftShape2 = Content.Load<Texture2D>("purplesquare");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            currSpace = keyboardState.IsKeyDown(Keys.Space);
            switch (gameState)
            {
                case GameState.MENU:
                    if(currSpace && !prevSpace)
                    {
                        gameState = GameState.INSTRUCTION;
                    }
                    break;
                case GameState.INSTRUCTION:
                    if (currSpace && !prevSpace)
                    {
                        gameState = GameState.SHAPES;
                    }
                    break;
                case GameState.SHAPES:
                    if (currSpace && !prevSpace)
                    {
                        gameState = GameState.TEST;
                    }
                    break;
                case GameState.TEST:
                    drivingGame.update(gameTime, screenSize, keyboardState);
                    shapeGame.update(gameTime, Content, keyboardState);
                    if(Math.Abs(drivingGame.DifficultyLevel - shapeGame.DifficultyLevel) >= 10)
                    {
                        //Cannot have too great of a disparity between driving and shape game proficiencies
                        if(drivingGame.DifficultyLevel > shapeGame.DifficultyLevel && shapeGame.DifficultyLevel < shapeGame.LevelLength - 1)
                        {
                            shapeGame.DifficultyLevel++;
                        }
                        else if(shapeGame.DifficultyLevel > drivingGame.DifficultyLevel && drivingGame.DifficultyLevel < drivingGame.LevelLength - 1)
                        {
                            drivingGame.DifficultyLevel++;
                        }
                    }
                    break;
                default:
                    break;
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            prevSpace = currSpace;
            base.Update(gameTime);
        }
        
        private void writeResults()
        {
            //open StreamWriter in append mode

            endTime = DateTime.Now.ToString("MM-dd/HH:mm");
            StreamWriter sw = new StreamWriter(resultsFile, true);
            //sw.Write(shapeGame.CorrectTests+","+shapeGame.WrongTests+","+shapeGame.DifficultyLevel+Environment.NewLine);
            sw.Write(shapeGame.DifficultyLevel + "," + drivingGame.DifficultyLevel + "," + startTime + "," + endTime + Environment.NewLine);

            sw.Close();
            wroteResults = true;
        }

        private Results readPreviousResults()
        {
            if (File.Exists(resultsFile))
            {
                string currLine = "";
                string prevLine = "";
                StreamReader sr = new StreamReader(resultsFile);
                currLine = sr.ReadLine();
                if (currLine == null)
                {
                    sr.Close();
                    return new Results(15, 9);
                    //empty file don't do anything pls
                }
                else
                {
                    //know end of file is reached when final line is null
                    while (currLine != null)
                    {
                        prevLine = currLine;
                        currLine = sr.ReadLine();
                    }
                    sr.Close();
                    string[] temp = prevLine.Split(',');
                    Results results = new Results(Int32.Parse(temp[0]), Int32.Parse(temp[1]));
                    return results;
                }
            }
            else
            {
                File.Create(resultsFile);
                return new Results(15, 9);
            }
        }

        protected override void OnExiting(Object sender, EventArgs args)
        {
            writeResults();
            base.OnExiting(sender, args);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);
            string titleText;
            string description;
            Vector2 titleWidth, descriptionWidth;
            switch (gameState)
            {
                case GameState.MENU:
                    titleText = "Not NeuroRacer";
                    description = "Press Space to begin!";
                    titleWidth = titleFont.MeasureString(titleText);
                    descriptionWidth = descriptionFont.MeasureString(description);
                    spriteBatch.Begin();
                    spriteBatch.DrawString(titleFont, titleText,
                        new Vector2((graphics.PreferredBackBufferWidth - titleWidth.X)/2, (graphics.PreferredBackBufferHeight - titleWidth.Y)/2 - 200)
                        , Color.Black);
                    spriteBatch.DrawString(descriptionFont, description,
                        new Vector2((graphics.PreferredBackBufferWidth - descriptionWidth.X)/2, (graphics.PreferredBackBufferHeight - descriptionWidth.Y)/2)
                        , Color.Black);
                    spriteBatch.End();
                    break;
                case GameState.INSTRUCTION:
                    int scale = 1;
                    Vector2 textPos;
                    spriteBatch.Begin();
                    spriteBatch.Draw(instructionBackground, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);
                    description = "Shapes will pop up between the arrows above";
                    descriptionWidth = descriptionFont.MeasureString(description);
                    textPos = new Vector2((graphics.PreferredBackBufferWidth - descriptionWidth.X) / 2, (graphics.PreferredBackBufferHeight - descriptionWidth.Y) / 2);
                    spriteBatch.DrawString(descriptionFont, description, textPos + new Vector2(1 * scale, 1 * scale), Color.Black);
                    spriteBatch.DrawString(descriptionFont, description, textPos + new Vector2(-1 * scale, -1 * scale), Color.Black);
                    spriteBatch.DrawString(descriptionFont, description, textPos + new Vector2(-1 * scale, 1 * scale), Color.Black);
                    spriteBatch.DrawString(descriptionFont, description, textPos + new Vector2(1 * scale, -1 * scale), Color.Black);
                    spriteBatch.DrawString(descriptionFont, description, textPos, Color.White);
                    description = "Focus your eyes on this piece of text while driving, not directly on the car";
                    descriptionWidth = descriptionFont.MeasureString(description);
                    textPos = new Vector2((graphics.PreferredBackBufferWidth - descriptionWidth.X) / 2, (graphics.PreferredBackBufferHeight + 4 * descriptionWidth.Y) / 2);
                    spriteBatch.DrawString(descriptionFont, description, textPos + new Vector2(1 * scale, 1 * scale), Color.Black);
                    spriteBatch.DrawString(descriptionFont, description, textPos + new Vector2(-1 * scale, -1 * scale), Color.Black);
                    spriteBatch.DrawString(descriptionFont, description, textPos + new Vector2(-1 * scale, 1 * scale), Color.Black);
                    spriteBatch.DrawString(descriptionFont, description, textPos + new Vector2(1 * scale, -1 * scale), Color.Black);
                    spriteBatch.DrawString(descriptionFont, description, textPos, Color.White);
                    description = "Use the left and right arrow keys to control your car, and stay off the grass!";
                    descriptionWidth = descriptionFont.MeasureString(description);
                    textPos = new Vector2((graphics.PreferredBackBufferWidth - descriptionWidth.X) / 2, (graphics.PreferredBackBufferHeight + 10 * descriptionWidth.Y) / 2);
                    spriteBatch.DrawString(descriptionFont, description, textPos + new Vector2(1 * scale, 1 * scale), Color.Black);
                    spriteBatch.DrawString(descriptionFont, description, textPos + new Vector2(-1 * scale, -1 * scale), Color.Black);
                    spriteBatch.DrawString(descriptionFont, description, textPos + new Vector2(-1 * scale, 1 * scale), Color.Black);
                    spriteBatch.DrawString(descriptionFont, description, textPos + new Vector2(1 * scale, -1 * scale), Color.Black);
                    spriteBatch.DrawString(descriptionFont, description, textPos, Color.White);
                    description = "Press space to continue";
                    descriptionWidth = descriptionFont.MeasureString(description);
                    textPos = new Vector2((graphics.PreferredBackBufferWidth - descriptionWidth.X) / 2, (graphics.PreferredBackBufferHeight + 15 * descriptionWidth.Y) / 2);
                    spriteBatch.DrawString(descriptionFont, description, textPos + new Vector2(1 * scale, 1 * scale), Color.Black);
                    spriteBatch.DrawString(descriptionFont, description, textPos + new Vector2(-1 * scale, -1 * scale), Color.Black);
                    spriteBatch.DrawString(descriptionFont, description, textPos + new Vector2(-1 * scale, 1 * scale), Color.Black);
                    spriteBatch.DrawString(descriptionFont, description, textPos + new Vector2(1 * scale, -1 * scale), Color.Black);
                    spriteBatch.DrawString(descriptionFont, description, textPos, Color.White);
                    spriteBatch.End();
                    break;
                case GameState.SHAPES:
                    titleText = "Shapes to react to";
                    description = "When this purple pentagon or green square pops up, press the D Key";
                    titleWidth = titleFont.MeasureString(titleText);
                    descriptionWidth = descriptionFont.MeasureString(description);
                    spriteBatch.Begin();
                    spriteBatch.DrawString(titleFont, titleText,
                        new Vector2((graphics.PreferredBackBufferWidth - titleWidth.X) / 2, 50)
                        , Color.Black);
                    spriteBatch.DrawString(descriptionFont, description,
                        new Vector2((graphics.PreferredBackBufferWidth - descriptionWidth.X) / 2, (graphics.PreferredBackBufferHeight - descriptionWidth.Y) / 2)
                        , Color.Black);

                    spriteBatch.Draw(rightShape1, new Rectangle(graphics.PreferredBackBufferWidth / 2 - 150, graphics.PreferredBackBufferHeight / 2 - 200, 200, 200), Color.White);
                    spriteBatch.Draw(rightShape2, new Rectangle(graphics.PreferredBackBufferWidth / 2 - 50, graphics.PreferredBackBufferHeight / 2 - 200, 200, 200), Color.White);

                    description = "When this purple square or green pentagon pops up, press the A Key";
                    descriptionWidth = descriptionFont.MeasureString(description);
                    spriteBatch.DrawString(descriptionFont, description,
                        new Vector2((graphics.PreferredBackBufferWidth - descriptionWidth.X) / 2, (graphics.PreferredBackBufferHeight + descriptionWidth.Y) / 2 + 250)
                        , Color.Black);
                    
                    spriteBatch.Draw(leftShape1, new Rectangle(graphics.PreferredBackBufferWidth / 2 - 150, graphics.PreferredBackBufferHeight / 2 + 50, 200, 200), Color.White);
                    spriteBatch.Draw(leftShape2, new Rectangle(graphics.PreferredBackBufferWidth / 2 - 50, graphics.PreferredBackBufferHeight / 2 + 50, 200, 200), Color.White);

                    description = "Press Space to begin!";
                    descriptionWidth = descriptionFont.MeasureString(description);
                    spriteBatch.DrawString(descriptionFont, description,
                        new Vector2((graphics.PreferredBackBufferWidth - descriptionWidth.X) / 2, (graphics.PreferredBackBufferHeight + descriptionWidth.Y) / 2 + 300)
                        , Color.Black);
                    spriteBatch.End();
                    break;
                case GameState.TEST:
                    drivingGame.draw(GraphicsDevice, spriteBatch, new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), basicEffect);
                    shapeGame.draw(spriteBatch);

                    spriteBatch.Begin();
                    scale = 1;
                    description = "A Key";
                    descriptionWidth = titleFont.MeasureString(description);
                    textPos = new Vector2(50, (graphics.PreferredBackBufferHeight - descriptionWidth.Y)/2 - 200);
                    spriteBatch.DrawString(titleFont, description, textPos + new Vector2(1 * scale, 1 * scale), Color.Black);
                    spriteBatch.DrawString(titleFont, description, textPos + new Vector2(-1 * scale, -1 * scale), Color.Black);
                    spriteBatch.DrawString(titleFont, description, textPos + new Vector2(-1 * scale, 1 * scale), Color.Black);
                    spriteBatch.DrawString(titleFont, description, textPos + new Vector2(1 * scale, -1 * scale), Color.Black);
                    spriteBatch.DrawString(titleFont, description, textPos, Color.White);

                    spriteBatch.Draw(leftShape1, new Rectangle((int)textPos.X - 100, (int)textPos.Y, 200, 200), Color.White);
                    spriteBatch.Draw(leftShape2, new Rectangle((int)textPos.X, (int)textPos.Y, 200, 200), Color.White);

                    description = "D Key";
                    descriptionWidth = titleFont.MeasureString(description);
                    textPos = new Vector2(graphics.PreferredBackBufferWidth - descriptionWidth.X - 50, (graphics.PreferredBackBufferHeight - descriptionWidth.Y) / 2 - 200);
                    spriteBatch.DrawString(titleFont, description, textPos + new Vector2(1 * scale, 1 * scale), Color.Black);
                    spriteBatch.DrawString(titleFont, description, textPos + new Vector2(-1 * scale, -1 * scale), Color.Black);
                    spriteBatch.DrawString(titleFont, description, textPos + new Vector2(-1 * scale, 1 * scale), Color.Black);
                    spriteBatch.DrawString(titleFont, description, textPos + new Vector2(1 * scale, -1 * scale), Color.Black);
                    spriteBatch.DrawString(titleFont, description, textPos, Color.White);

                    spriteBatch.Draw(rightShape1, new Rectangle((int)textPos.X - 100, (int)textPos.Y, 200, 200), Color.White);
                    spriteBatch.Draw(rightShape2, new Rectangle((int)textPos.X, (int)textPos.Y - 10, 200, 200), Color.White);

                    spriteBatch.End();
                    break;
                default:
                    break;
            }

            base.Draw(gameTime);
        }
    }
}
