using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NotNeuroRacer
{
    class Position3D
    {
        //this probably should be a class instead of just a struct
        //oh well
        //haha it's finally a class baybeeeeee
        //turns out that was the issue
        public Vector3 world;
        public Vector3 cam;
        public Vector2 project;
        public Vector2 screen; //only screen is really useful -- where the position is on a 2d screen
        public float width; //road width NOTE: is actually half the road width (from the center)

        public Position3D(Vector3 worldPos, Camera camera, Vector2 screenSize, float roadWidth)
        {
            world = new Vector3(worldPos.X, worldPos.Y, worldPos.Z);
            cam = new Vector3(
                world.X - camera.X,
                world.Y - camera.Y,
                world.Z - camera.Z
                );
            project = new Vector2(
                cam.X * (camera.DCamToScreen) / cam.Z,
                cam.Y * (camera.DCamToScreen) / cam.Z
                );
            screen = new Vector2(
                (screenSize.X / 2) + (screenSize.X / 2) * project.X,
                (screenSize.Y / 2) - (screenSize.Y / 2) * project.Y
                );
            width = (camera.DCamToScreen) / cam.Z * roadWidth * screenSize.X / 2;
        }

        public Position3D(float z)
        {
            world = new Vector3(0, 0, z);
            cam = new Vector3();
            project = new Vector2();
            screen = new Vector2();
            width = 0;
        }

        public Position3D(float x, float y, float z)
        {
            world = new Vector3(x, y, z);
            cam = new Vector3();
            project = new Vector2();
            screen = new Vector2();
            width = 0;
        }

        public void projectSegment(Player player, Camera camera, Vector2 screenSize, bool looped, float roadWidth, float trackLength, float curveOffset)
        {
            //the absolute position of a point in the world with regards to the camera (relative)
            cam.X = world.X - camera.X - curveOffset;
            cam.Y = world.Y - camera.Y - player.PlayerY;
            cam.Z = world.Z - camera.Z;

            //to enable smoother transitions when we should see the looped items in the background, but aren't in a "looped" position
            if (looped)
            {
                cam.Z += trackLength;
            }

            //voodoo psuedo 3d magics
            //projecting the 3d position on to a "screen" the camera sees
            project.X = (float)Math.Round(cam.X * (camera.DCamToScreen) / cam.Z);
            project.Y = (float)Math.Round(cam.Y * (camera.DCamToScreen) / cam.Z);

            float scale = camera.DCamToScreen / cam.Z;

            //adjust the normalized projection based on the size of the screen
            screen.X = (float)Math.Round((screenSize.X / 2) + (screenSize.X / 2) * scale * cam.X);
            screen.Y = (float)Math.Round((screenSize.Y / 2) - (screenSize.Y / 2) * scale * cam.Y);

            width = (float)Math.Round((camera.DCamToScreen) / cam.Z * roadWidth * screenSize.X / 2);
        }
    }
}
