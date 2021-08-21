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
    public enum CURVE
    {
        NONE = 0,
        EASY = 2,
        MEDIUM = 4,
        HARD = 5
    }

    public enum LENGTH
    {
        NONE = 0,
        SHORT = 25,
        MEDIUM = 50,
        LONG = 100
    }

    public enum HILL
    {
        NONE = 0,
        LOW = 20,
        MEDIUM = 40,
        HIGH = 60
    }

    class DrivingGameController
    {
        List<RoadSegment> segments;
        RoadSegment baseSegment;
        RoadSegment playerSegment;
        Camera camera;
        int roadWidth = 1500;
        int segmentLength = 200;
        int rumbleLength = 3; //length of rumble strip in segments
        int lanes = 3;
        int drawDistance = 300; //in segments
        int fogDensity = 5; //for the exponential fog
        float spriteScale;
        float trackLength;
        Rectangle fogRect;
        VertexPositionColor[] fog;
        Player player;
        Texture2D spriteSheet;
        Random rng;

        public int DifficultyLevel { get { return player.CurrentLevel; } set { player.CurrentLevel = value; } }
        public int LevelLength { get { return player.LevelLength; } }

        public DrivingGameController(GraphicsDevice Graphics)
        {
            segments = new List<RoadSegment>();
            camera = new Camera(0, 1250, 0, 100);
            fogRect = new Rectangle(0, 0, Graphics.Viewport.Width, 0);
            fog = new VertexPositionColor[4];
            rng = new Random();

            spriteScale = 1 / (Player.SPRITE_SIZE.X * 3);
            player = new Player(0,0,0,0, segmentLength);
            resetRoad();
        }
        public void update(GameTime gameTime, Vector2 screenSize, KeyboardState keyState)
        {
            float pRemaining = percentRemaining((int)(player.Position + camera.Y * camera.DCamToScreen), segmentLength);
            playerSegment = findSegment(player.Position + camera.Y * camera.DCamToScreen);
            player.update(spriteScale, (1f/camera.Y), roadWidth, screenSize, keyState, playerSegment, camera, pRemaining, gameTime, segmentLength);
            camera.X = (player.PlayerX * roadWidth);
            camera.Z = (player.Position % trackLength);
        }

        public void loadContent(ContentManager content)
        {
            spriteSheet = content.Load<Texture2D>("sprites");
        }

        public void draw(GraphicsDevice Graphics, SpriteBatch spriteBatch, Vector2 screenSize, BasicEffect basicEffect)
        {
            int baseSegPos, segPos;
            float maxY = screenSize.Y; //used to check if the segment we want to render has already been rendered in a Y position
            baseSegment = findSegment(player.Position); //temporarily at 0, but really should be at the player's position
            baseSegPos = segments.IndexOf(baseSegment);
            //necessary, because the curve is of the second order (smoothness)
            float dx = -(baseSegment.Curve * percentRemaining(player.Position, segmentLength));
            float x = 0;
            for(int i = 0; i < drawDistance; i++)
            {
                //allows repetition
                segPos = (baseSegPos + i) % segments.Count;
                RoadSegment segment = segments[segPos];
                //a segment repeats itself (i.e. is displayed as if it is one tracklength ahead) if it is behind the current segment in the segments list
                segment.Repeating = segPos < baseSegPos;
                segment.P1.projectSegment(player, camera, screenSize, segment.Repeating, roadWidth, trackLength, x);
                segment.P2.projectSegment(player, camera, screenSize, segment.Repeating, roadWidth, trackLength, x + dx);
                
                x += dx;
                dx += segment.Curve;

                if (segment.P1.cam.Z <= camera.DCamToScreen || //if segment is behind us
                    segment.P2.screen.Y >= maxY) //or if it clips into another, already rendered segment
                {
                    continue;
                }
                else
                {
                    segment.draw(Graphics, spriteBatch, lanes, (int)screenSize.X, basicEffect, (float)i/drawDistance);
                    maxY = segment.P2.screen.Y;
                }
            }
            fogRect.Y = (int)maxY;
            fogRect.Height = Graphics.Viewport.Height - fogRect.Y;
            player.draw(spriteSheet, spriteBatch);
            
        }

        //find the segment at any absolute position (able to loop around at z>segmentLength)
        private RoadSegment findSegment(float z)
        {
            return segments[(int)Math.Floor(z / segmentLength) % segments.Count];
        }

        private void resetRoad()
        {
            /*
            addCurve(LENGTH.SHORT, CURVE.HARD, true);
            addHill(LENGTH.MEDIUM, CURVE.NONE, HILL.MEDIUM, false, false);
            addHill(LENGTH.MEDIUM, CURVE.EASY, HILL.MEDIUM, true, true);
            addHill(LENGTH.LONG, CURVE.MEDIUM, HILL.LOW, false, true);
            addCurve(LENGTH.LONG, CURVE.MEDIUM, true);
            addCurve(LENGTH.LONG, CURVE.EASY, true);
            addCurve(LENGTH.LONG, CURVE.MEDIUM, false);
            addCurve(LENGTH.LONG, CURVE.EASY, true);*/
            for(int i = 0; i < 200; i++)
            {
                LENGTH tempLength;
                HILL tempHill;
                CURVE tempCurve;
                bool tempNegCurve, tempNegHill;
                int t;

                t = rng.Next(0, 10);
                if(t < 6)
                {
                    tempLength = LENGTH.SHORT;
                } else if(t < 8)
                {
                    tempLength = LENGTH.MEDIUM;
                }
                else
                {
                    tempLength = LENGTH.LONG;
                }

                t = rng.Next(0, 10);
                if(t < 2)
                {
                    tempCurve = CURVE.EASY;
                } else if(t < 8)
                {
                    tempCurve = CURVE.MEDIUM;
                }
                else
                {
                    tempCurve = CURVE.HARD;
                }

                t = rng.Next(0, 10);
                if (t < 3)
                {
                    tempHill = HILL.NONE;
                }
                else if (t < 7)
                {
                    tempHill = HILL.LOW;
                }
                else if (t < 9)
                {
                    tempHill = HILL.MEDIUM;
                }
                else
                {
                    tempHill = HILL.HIGH;
                }

                t = rng.Next(0, 10);
                if(t < 5)
                {
                    tempNegCurve = false;
                }
                else
                {
                    tempNegCurve = true;
                }

                t = rng.Next(0,10);
                if(t < 5)
                {
                    tempNegHill = false;
                }
                else
                {
                    tempNegHill = true;
                }

                addHill(tempLength, tempCurve, tempHill, tempNegCurve, tempNegHill);
            }
            addDownhillToEnd();
            trackLength = segments.Count * segmentLength;
        }

        private void addSegment(float curve, float y)
        {
            int i = segments.Count;
            segments.Add(new RoadSegment(
                    new Position3D(0,lastY(),i * segmentLength), new Position3D(0,y,(i + 1) * segmentLength),
                    (i / rumbleLength) % 2 == 0 ? new Color(70,70,70) : new Color(75,75,75),
                    (i / rumbleLength) % 2 == 0 ? Color.DarkGreen : Color.Green,
                    (i / rumbleLength) % 2 == 0 ? Color.White : Color.Red,
                    ((i / rumbleLength) % 2 == 0), 
                    curve));
        }

        private float lastY()
        {
            if(segments.Count == 0)
            {
                return 0;
            }
            else
            {
                return segments[segments.Count - 1].P2.world.Y;
            }
        }

        //general formula for adding a segment of road
        //if the curve is 0, this ends up generating a straighaway with the length of enter + hold + exit
        //as easeIn and easeInOut degenerate to 0 if a=b
        private void addRoad(int enter, int hold, int exit, int curve, float y)
        {
            float startY = lastY();
            float endY = startY + y * segmentLength;
            int total = enter + hold + exit;
            for(int i = 0; i < enter; i++)
            {
                addSegment(easeIn(0, curve, i / (float)enter), easeInOut(startY, endY, i/ (float)total));
            }
            for(int i = 0; i < hold; i++)
            {
                addSegment(curve, easeInOut(startY, endY, (i+enter)/(float)total));
            }
            for(int i = 0; i < exit; i++)
            {
                addSegment(easeInOut(curve, 0, i / (float)exit), easeInOut(startY, endY, (i+enter+hold)/(float)total));
            }
        }

        private void addStraight(LENGTH length)
        {
            addRoad((int)length, (int)length, (int)length, (int)CURVE.NONE, (int)HILL.NONE);
        }

        private void addCurve(LENGTH length, CURVE curve, bool negative)
        {
            addRoad((int)length, (int)length, (int)length, (int)curve * (negative ? -1 : 1), (int)HILL.NONE);
        }

        private void addHill(LENGTH length, CURVE curve, HILL hill, bool negCurve, bool negHill)
        {
            addRoad((int)length, (int)length, (int)length, (int)curve * (negCurve ? -1 : 1), (int)hill * (negHill ? -1 : 1));
        }

        private void addSCurves()
        {
            addRoad((int)LENGTH.MEDIUM, (int)LENGTH.MEDIUM, (int)LENGTH.MEDIUM, -(int)CURVE.EASY, (int)HILL.NONE);
            addRoad((int)LENGTH.MEDIUM, (int)LENGTH.MEDIUM, (int)LENGTH.MEDIUM, (int)CURVE.MEDIUM, (int)HILL.NONE);
            addRoad((int)LENGTH.MEDIUM, (int)LENGTH.MEDIUM, (int)LENGTH.MEDIUM, (int)CURVE.EASY, (int)HILL.NONE);
            addRoad((int)LENGTH.MEDIUM, (int)LENGTH.MEDIUM, (int)LENGTH.MEDIUM, -(int)CURVE.EASY, (int)HILL.NONE);
            addRoad((int)LENGTH.MEDIUM, (int)LENGTH.MEDIUM, (int)LENGTH.MEDIUM, -(int)CURVE.MEDIUM, (int)HILL.NONE);
        }

        private void addLowRollingHills()
        {
            addRoad((int)LENGTH.SHORT, (int)LENGTH.SHORT, (int)LENGTH.SHORT, 0, (int)HILL.LOW / 2);
            addRoad((int)LENGTH.SHORT, (int)LENGTH.SHORT, (int)LENGTH.SHORT, 0, -(int)HILL.LOW);
            addRoad((int)LENGTH.SHORT, (int)LENGTH.SHORT, (int)LENGTH.SHORT, 0, 0);
            addRoad((int)LENGTH.SHORT, (int)LENGTH.SHORT, (int)LENGTH.SHORT, 0, (int)HILL.LOW / 2);
            addRoad((int)LENGTH.SHORT, (int)LENGTH.SHORT, (int)LENGTH.SHORT, 0, (int)HILL.LOW / 2);
            addRoad((int)LENGTH.SHORT, (int)LENGTH.SHORT, (int)LENGTH.SHORT, 0, 0);
        }

        private void addDownhillToEnd()
        {
            addRoad(200, 200, 200, (int)CURVE.EASY, -lastY()/segmentLength);
        }

        //curve functions
        //ease in+out are quadratic curves
        //easeInOut is a sinusoidal curve
        private float easeIn(float a, float b, float percent)
        {
            return (float)(a + (b - a) * Math.Pow(percent, 2));
        }

        private float easeOut(float a, float b, float percent)
        {
            return (float)(a + (b - a) * (1 - Math.Pow(percent, 2)));
        }

        private float easeInOut(float a, float b, float percent)
        {
            return (float)(a + (b-a)*(-Math.Cos((Math.PI*percent))/2 + 0.5));
        }
        
        public static float interpolate(float a, float b, float percent)
        {
            return a + (b - a) * percent;
        }

        public static float percentRemaining(int n, int total)
        {
            return (n % total) / (float)total;
        }
    }
}
