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
    class RoadSegment
    {
        //NOTE: p1 and p2 are defined as the middle points of the road, therefore width is half the actual width of the road
        Position3D p1; //lower half of the road segment
        Position3D p2; //upper half of the road segment
        VertexPositionColor[] rumbleLeft, rumbleRight, road;
        List<VertexPositionColor[]> RoadLanes;
        float drawCurve;
        public float Curve { get { return drawCurve; } }
        public Position3D P1 { get { return p1; } }
        public Position3D P2 { get { return p2; } }

        //the more you ask, the less it makes sense
        //grass color is used by the fillRect method (XNA), while the road and rumble colors are used by the System.Drawing methods
        Color RoadColor, GrassColor, RumbleColor;

        bool repeating;
        bool drawLanes;
        public bool Repeating { get { return repeating; } set { repeating = value; } }
        public RoadSegment(Position3D bottom, Position3D top, Color providedRoadColor, Color providedGrassColor, Color providedRumbleColor, bool lanes, float curve)
        {
            p1 = bottom;
            p2 = top;
            RoadColor = providedRoadColor;
            GrassColor = providedGrassColor;
            RumbleColor = providedRumbleColor;
            repeating = false;
            drawLanes = lanes;
            drawCurve = curve;

            rumbleLeft = new VertexPositionColor[4];
            rumbleRight = new VertexPositionColor[4];
            road = new VertexPositionColor[4];
            RoadLanes = new List<VertexPositionColor[]>();
        }

        public void draw(GraphicsDevice Graphics, SpriteBatch spriteBatch, int lanes, int screenWidth, BasicEffect basicEffect, float fogDist)
        {
            //rumble strips
            float r1 = rumbleWidth(p1.width, lanes);
            float r2 = rumbleWidth(p2.width, lanes);

            Rectangle grassRectangle = new Rectangle(0, (int)p2.screen.Y, screenWidth, (int)(p1.screen.Y - p2.screen.Y));
            rumbleLeft[0].Position = new Vector3(p1.screen.X - p1.width, p1.screen.Y, 0);
            rumbleLeft[1].Position = new Vector3(p1.screen.X - p1.width - r1, p1.screen.Y, 0);
            rumbleLeft[2].Position = new Vector3(p2.screen.X - p2.width - r2, p2.screen.Y, 0);
            rumbleLeft[3].Position = new Vector3(p2.screen.X - p2.width, p2.screen.Y, 0);

            for(int i = 0; i < rumbleLeft.Length; i++)
            {
                rumbleLeft[i].Color = RumbleColor;
            }

            rumbleRight[0].Position = new Vector3(p1.screen.X + p1.width + r1, p1.screen.Y, 0);
            rumbleRight[1].Position = new Vector3(p1.screen.X + p1.width, p1.screen.Y, 0);
            rumbleRight[2].Position = new Vector3(p2.screen.X + p2.width, p2.screen.Y, 0);
            rumbleRight[3].Position = new Vector3(p2.screen.X + p2.width + r2, p2.screen.Y, 0);

            for (int i = 0; i < rumbleRight.Length; i++)
            {
                rumbleRight[i].Color = RumbleColor;
            }

            road[0].Position = new Vector3(p1.screen.X + p1.width, p1.screen.Y, 0);
            road[1].Position = new Vector3(p1.screen.X - p1.width, p1.screen.Y, 0);
            road[2].Position = new Vector3(p2.screen.X - p2.width, p2.screen.Y, 0);
            road[3].Position = new Vector3(p2.screen.X + p2.width, p2.screen.Y, 0);

            for (int i = 0; i < road.Length; i++)
            {
                road[i].Color = RoadColor;
            }

            if (drawLanes)
            {
                float lanew1 = (p1.width*2)/lanes;
                float lanew2 = (p2.width*2)/lanes;
                float lanex1 = p1.screen.X - p1.width + lanew1;
                float lanex2 = p2.screen.X - p2.width + lanew2;
                //lane markers
                float l1 = laneMarkerWidth(p1.width, lanes);
                float l2 = laneMarkerWidth(p2.width, lanes);
                for(int numLanesDrawn = 1; numLanesDrawn < lanes; lanex1 += lanew1, lanex2 += lanew2, numLanesDrawn++)
                {
                    VertexPositionColor[] temp = new VertexPositionColor[4];
                    temp[1].Position = new Vector3(lanex1 - l1 / 2, p1.screen.Y, 0);
                    temp[0].Position = new Vector3(lanex1 + l1 / 2, p1.screen.Y, 0);
                    temp[2].Position = new Vector3(lanex2 - l2 / 2, p2.screen.Y, 0);
                    temp[3].Position = new Vector3(lanex2 + l2 / 2, p2.screen.Y, 0);
                    for(int i = 0; i < temp.Length; i++)
                    {
                        temp[i].Color = Color.WhiteSmoke;
                    }
                    RoadLanes.Add(temp);
                }
            }

            short[] ind = new short[6];
            ind[0] = 0;
            ind[1] = 1;
            ind[2] = 2;
            ind[3] = 0;
            ind[4] = 2;
            ind[5] = 3;
            
            spriteBatch.Begin();
            spriteBatch.FillRectangle(grassRectangle, GrassColor);
            spriteBatch.End();
            foreach (EffectPass effectPass in basicEffect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                int temp = ind.Length/3;
                //Graphics.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, rumbleLeft, 0, 1);
                Graphics.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, rumbleLeft, 0, rumbleLeft.Length, ind, 0, ind.Length / 3, VertexPositionColor.VertexDeclaration);
                Graphics.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, rumbleRight, 0, rumbleRight.Length, ind, 0, ind.Length / 3);
                Graphics.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, road, 0, road.Length, ind, 0, ind.Length / 3);
                if (drawLanes)
                {
                    foreach(VertexPositionColor[] lane in RoadLanes)
                    {
                        Graphics.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, lane, 0, lane.Length, ind, 0, ind.Length/3);
                    }
                    RoadLanes.Clear();
                }
            }
            float fog = exponentialFog(fogDist, 5);
            if (fog < 1)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
                spriteBatch.FillRectangle(grassRectangle, new Color(Color.CornflowerBlue, (int)(255 - fog * 255)));
                spriteBatch.End();
            }

        }

        private float exponentialFog(float distance, float density)
        {
            return (float)(1/(Math.Pow(Math.E, distance * distance * density)));
        }

        private float rumbleWidth(float projRoadWidth, int lanes)
        {
            return projRoadWidth / Math.Max(6, 2 * lanes);
        }

        private float laneMarkerWidth(float projRoadWidth, int lanes)
        {
            return projRoadWidth / Math.Max(20, 5 * lanes);
        }

    }
}
