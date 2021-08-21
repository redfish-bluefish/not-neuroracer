using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Shapes;
using MonoGame.Extended;

namespace NotNeuroRacer
{
    class Player
    {
        Dictionary<string, Rectangle> spriteRectangles = new Dictionary<string, Rectangle>();
        Rectangle drawRect;
        float position;
        float grassDecel, naturalDecel, playerAccel, playerDecel;
        float maxSpeed, maxGrassSpeed;
        float speed;
        int steer;

        float turningDebt;

        //measuring time on and off road
        float roadTimeMs, totalTimeMs;
        float sinceLastAdjustmentMs;
        float timeBetweenAdjustmentsMs = 20000; //20 seconds between adjustments 

        float centrifugal = 0.27f; //centrifugal force constant
        float[] speedDifficulties = new float[]
        { 1/3.0f, 1/2.75f, 1/2.5f, 1/2.25f, 1/2.0f, 1/1.75f, 1/1.5f, 1/1.25f, 1.0f, 1.25f, 1.5f, 1.75f, 2.0f, 2.25f, 2.5f, 2.75f, 3.0f};
        int currentLevel;

        public int CurrentLevel { get { return currentLevel; } set { currentLevel = value; } }
        public int LevelLength { get { return speedDifficulties.Length; } }

        float dx;
        float playerX; //normalized x position
        //ranges from -2 to 2
        //between -1 and 1 is on the road, while all other values are off the road (on the grass)
        float playerY;

        public int Position
        {
            get { return (int)position; }
        }
        
        public float PlayerX
        {
            get { return playerX; }
        }

        public float PlayerY
        {
            get { return playerY; }
        }

        public bool ON_ROAD
        {
            get { return playerX < 1.0f && playerX > -1.0f; }
        }
        
        //pls change this instead of everything else
        public static Vector2 SPRITE_SIZE
        {
            get { return new Vector2(80, 41); }
        }

        public Player(int x, int y, int w, int h, int segmentLength)
        {
            //Yes these values are arbitrary, but that's just how the spritesheet turned out, okay?
            spriteRectangles.Add("player_flat_straight", new Rectangle(1085, 480, (int)SPRITE_SIZE.X, (int)SPRITE_SIZE.Y));
            spriteRectangles.Add("player_flat_left", new Rectangle(995, 480, (int)SPRITE_SIZE.X, (int)SPRITE_SIZE.Y));
            spriteRectangles.Add("player_flat_right", new Rectangle(995, 531, (int)SPRITE_SIZE.X, (int)SPRITE_SIZE.Y));
            drawRect = new Rectangle(x, y, w, h);
            speed = 0;
            turningDebt = 0;

            maxSpeed = segmentLength/speedDifficulties[currentLevel]; //assuming 60 frames a second. I don't really know
            maxGrassSpeed = maxSpeed / 3;

            playerAccel = maxSpeed / 60f;
            playerDecel = maxSpeed / 60f;
            grassDecel = maxSpeed / 20f;
            naturalDecel = maxSpeed / 120f;
        }

        public void update(float spriteScale, float scale, int roadWidth, Vector2 screenSize, KeyboardState keyState, RoadSegment playerSegment, Camera camera, float pPercent, GameTime gameTime, float segmentLength)
        {
            playerY = DrivingGameController.interpolate(playerSegment.P1.world.Y, playerSegment.P2.world.Y, pPercent);

            //projections relative to sprite widths and roadwidths
            drawRect.Width = (int)(SPRITE_SIZE.X * scale * (screenSize.X/2) * spriteScale * roadWidth);
            drawRect.Height = (int)(SPRITE_SIZE.Y * scale * (screenSize.X / 2) * spriteScale * roadWidth);
            drawRect.X = (int)(screenSize.X / 2 - drawRect.Width / 2);
            drawRect.Y = (int)(screenSize.Y - drawRect.Height);

            speed += playerAccel;
            if (!ON_ROAD && speed > maxGrassSpeed)
            {
                speed -= grassDecel;
            }

            limitSpeed();
            dx = (speed/maxSpeed)/30.0f;
            //float playerDelta = 0;
            //playerDelta += dx * playerSegment.Curve * centrifugal;

            //maintaining left-right position
            
            if (keyState.IsKeyDown(Keys.Left))
            {
                /*
                if(Math.Abs(dx * playerSegment.Curve * centrifugal) > Math.Abs(dx))
                {
                    playerX += 0;
                }
                else
                {
                    playerX += dx * playerSegment.Curve * centrifugal - (dx);
                }*/
                playerX += dx * playerSegment.Curve * centrifugal - (dx);
                steer = -1;
            }
            else if (keyState.IsKeyDown(Keys.Right))
            {
                /*
                if(Math.Abs(dx * playerSegment.Curve * centrifugal) > Math.Abs(dx))
                {
                    playerX += 0;
                }
                else
                {
                    playerX += dx * playerSegment.Curve * centrifugal + (dx);
                }*/
                playerX += dx * playerSegment.Curve * centrifugal + (dx);
                steer = 1;
            }
            else
            {
                playerX += dx * playerSegment.Curve * centrifugal;
                steer = 0;
            }
            //from the curve the player is on
            //playerX += playerDelta;
            /*
            if(playerSegment.Curve > 0 && !keyState.IsKeyDown(Keys.Left) && speed > 0)
            {
                turningDebt += playerSegment.Curve * centrifugal * dx;
            } else if(playerSegment.Curve < 0 && !keyState.IsKeyDown(Keys.Right) && speed > 0)
            {
                turningDebt += playerSegment.Curve * centrifugal * dx;
            }


            if(Math.Abs(turningDebt) < dx * centrifugal)
            {
                turningDebt = 0;
            } else if(turningDebt > 0)
            {
                playerX += (dx * centrifugal);
                turningDebt -= (dx*centrifugal);
            } else if(turningDebt < 0)
            {
                playerX -= (dx * centrifugal);
                turningDebt += (dx*centrifugal);
            }*/

            limitPosition();
            position += speed;

            adjustiveStepAlgoritm(gameTime, segmentLength);
        }

        public void draw(Texture2D spriteSheet, SpriteBatch spriteBatch)
        {
            Rectangle tempRect;
            if (steer > 0)
            {
                //steering right
                spriteRectangles.TryGetValue("player_flat_right", out tempRect);
            }
            else if (steer < 0)
            {
                //steering left
                spriteRectangles.TryGetValue("player_flat_left", out tempRect);
            }
            else
            {
                //not steering (i.e. straight)
                spriteRectangles.TryGetValue("player_flat_straight", out tempRect);
            }

            spriteBatch.Begin();
            spriteBatch.Draw(spriteSheet, drawRect, tempRect, Color.White); //draw the player
            spriteBatch.End();
        }

        private void limitSpeed()
        {
            speed = Math.Max(Math.Min(maxSpeed, speed), 0);
        }

        private void limitPosition()
        {
            if (playerX > 0)
            {
                playerX = Math.Min(2, playerX);
            }
            else
            {
                playerX = Math.Max(-2, playerX);
            }
        }

        //adjust player's speed based on their performance on the track
        private void adjustiveStepAlgoritm(GameTime gameTime, float segmentLength)
        {
            totalTimeMs += gameTime.ElapsedGameTime.Milliseconds;
            sinceLastAdjustmentMs += gameTime.ElapsedGameTime.Milliseconds;
            //on the track
            if(playerX < 1 && playerX > -1)
            {
                roadTimeMs += gameTime.ElapsedGameTime.Milliseconds;
            }
            //adjust speed down only if spending between  of time on the road, and time since the last adjustment is valid
            if(roadTimeMs/totalTimeMs < 0.8f && sinceLastAdjustmentMs >= timeBetweenAdjustmentsMs)
            {
                if(currentLevel < speedDifficulties.Length - 1)
                {
                    currentLevel++;
                }
                sinceLastAdjustmentMs = 0;
            } else if(roadTimeMs/totalTimeMs > 0.9f && sinceLastAdjustmentMs >= timeBetweenAdjustmentsMs)
            {
                if(currentLevel > 0)
                {
                    currentLevel--;
                }
                sinceLastAdjustmentMs = 0;
            }
            if(sinceLastAdjustmentMs == 0)
            {
                roadTimeMs = 0;
                totalTimeMs = 0;
            }
            maxSpeed = segmentLength / speedDifficulties[currentLevel];
        }
    }
}
