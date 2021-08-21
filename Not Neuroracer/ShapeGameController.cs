using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NotNeuroRacer
{
    class ShapeGameController
    {
        List<Shape> shapes;
        Random generator;
        //where on the screen the shapes are displayed
        Vector2 shapeDisplay;
        Rectangle leftArrow, rightArrow;
        Texture2D leftArrowTexture, rightArrowTexture;

        bool testing;
        bool currKey; //whether or not the key for reaction is being pressed right now
        bool prevKey; //whether or not the key for reaction has been pressed before
        bool output; //have yet to output

        bool currKeyLeft, prevKeyLeft;

        int shapeSize;
        int timeSinceLastTest_ms = 0;
        int timeSinceTestBeginning_ms = 0;
        //corresponds to the index reactionTime chooses from
        //lower difficulty levels are harder
        int difficultyLevel;
        int numTests;

        //NOTE: correctTests and wrongTests do not necessarily sum to total tests
        //It is possible to "fail" a test outside of a testing state by pressing the reaction key outside of a testing state
        int totalTests = 0;
        int tempTotalTests = 0;
        public int CorrectTests
        {
            get { return correctTests; }
        }
        int correctTests = 0;
        int tempCorrectTests = 0;
        public int WrongTests
        {
            get { return wrongTests; }
        }
        int wrongTests = 0;
        int tempWrongTests = 0;
        public bool TestsComplete
        {
            get { return totalTests >= numTests; }
        }

        public int DifficultyLevel
        {
            get { return difficultyLevel; }
            set { difficultyLevel = value; }
        }

        public int LevelLength { get { return reactionTime_ms.Length; } }

        int minTimeBetweenTests_ms = 5000;
        //the amount of time a shape remains on screen in milliseconds, level corresponding with index (i.e. level 0 = number at index 0)
        int[] reactionTime_ms = new int[] { 330, 350, 370, 390, 410, 430, 450, 470, 490, 510, 530, 550, 570, 590, 610, 630, 650, 670, 690, 710, 730, 750, 770, 790};

        public ShapeGameController()
        {
            shapes = new List<Shape>();
            generator = new Random();
            shapeDisplay = new Vector2(1024/2 - 100, 786 / 2 - 250); //temp numbers
            shapeSize = 200;
            testing = false;

            leftArrow = new Rectangle((int)shapeDisplay.X + shapeSize, (int)shapeDisplay.Y, shapeSize, shapeSize);
            rightArrow = new Rectangle((int)shapeDisplay.X - shapeSize, (int)shapeDisplay.Y, shapeSize, shapeSize);

            prevKey = false;
            currKey = false;
            output = true;
        }

        public void update(GameTime gameTime, ContentManager Content, KeyboardState keyboardState)
        {
            currKey = keyboardState.IsKeyDown(Keys.D);
            currKeyLeft = keyboardState.IsKeyDown(Keys.A);
            if(timeSinceLastTest_ms > minTimeBetweenTests_ms && !testing)
            {
                //initialize a new test
                Shape newShape;
                int temp = generator.Next(0, 10);
                if (temp < 5)
                {
                    newShape = new Shape(shapeDisplay, shapeSize, shapeSize, Correct.LEFT);
                    if (temp <= 2)
                    {
                        newShape.loadContent(Content, "greenpentagon");
                    }
                    else
                    {
                        newShape.loadContent(Content, "purplesquare");
                    }
                    //add a wrong texture to the shape
                }
                else
                {
                    newShape = new Shape(shapeDisplay, shapeSize, shapeSize, Correct.RIGHT);
                    if(temp >= 8)
                    {
                        newShape.loadContent(Content, "greensquare");
                    }
                    else
                    {
                        newShape.loadContent(Content, "purplepentagon");
                    }
                    //add a correct texture to the shape
                }
                shapes.Add(newShape);
                testing = true;
            }
            else
            {

            }
            //insert tests and checking here
            if (testing)
            {
                if(timeSinceTestBeginning_ms < reactionTime_ms[difficultyLevel])
                {
                    //test is still valid for keypress
                    if (currKey && !prevKey)
                    {
                        if (shapes[0].CorrectnessValue.Equals(Correct.LEFT))
                        {
                            wrongTests++;
                            tempWrongTests++;
                            //shape is wrong
                        }
                        else
                        {
                            correctTests++;
                            tempCorrectTests++;
                        }
                        unloadTest();
                    }

                    if (currKeyLeft && !prevKeyLeft)
                    {
                        if (shapes[0].CorrectnessValue.Equals(Correct.RIGHT))
                        {
                            wrongTests++;
                            tempWrongTests++;
                        }
                        else
                        {
                            correctTests++;
                            tempWrongTests++;
                        }
                        unloadTest();
                    }
                    timeSinceTestBeginning_ms += gameTime.ElapsedGameTime.Milliseconds;
                }
                else
                {
                    //not reacting in time
                    wrongTests++;
                    tempWrongTests++;
                    unloadTest();
                }
            }
            else
            {
                //only count time since the last test if not testing
                timeSinceLastTest_ms += gameTime.ElapsedGameTime.Milliseconds;

                if (currKey && !prevKey || currKeyLeft && !prevKeyLeft)
                {
                    //pressing a key when a test is not currently running
                    wrongTests++;
                    tempWrongTests++;
                }
            }
            adaptiveStepAlgorithm();
            prevKey = currKey;
            prevKeyLeft = currKeyLeft;
        }

        public void loadContent(ContentManager Content)
        {
            leftArrowTexture = Content.Load<Texture2D>("left-arrow");
            rightArrowTexture = Content.Load<Texture2D>("right-arrow");
        }

        private void unloadTest()
        {
            testing = false;
            timeSinceLastTest_ms = 0;
            timeSinceTestBeginning_ms = 0;
            totalTests++;
            tempTotalTests++;
            shapes.Clear();
        }

        private void adaptiveStepAlgorithm()
        {
            //Check to see how a player is doing every 10 tests
            if(tempTotalTests >= 10)
            {
                if(difficultyLevel < reactionTime_ms.Length-1 && tempWrongTests/(float)tempTotalTests >= 0.2f)
                {
                    //step down in difficulty if player gets more than 20% wrong
                    difficultyLevel++;
                } else if (difficultyLevel > 0 && tempWrongTests/(float)tempTotalTests <= 0.1f)
                {
                    //step up in difficulty if player gets less than 10% wrong
                    difficultyLevel--;
                }
                //flush the temporary values
                tempTotalTests = 0;
                tempCorrectTests = 0;
                tempWrongTests = 0;
            }
        }

        public void draw(SpriteBatch spriteBatch)
        {
            foreach(Shape shape in shapes)
            {
                shape.draw(spriteBatch);
            }
            spriteBatch.Begin();
            spriteBatch.Draw(leftArrowTexture, leftArrow, Color.White);
            spriteBatch.Draw(rightArrowTexture, rightArrow, Color.White);
            spriteBatch.End();
        }
    }
}
