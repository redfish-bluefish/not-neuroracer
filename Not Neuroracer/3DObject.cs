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
    abstract class _3DObject
    {
        protected Texture2D texture;

        protected float xWorld, yWorld, zWorld; //position in the psuedo 3d world
        protected float xCam, yCam, zCam; //translating actual position to one relative to camera
        protected float xProj, yProj; //normalizing relative coordinates on to a projection plane (this is the hard part)
        protected float xScreen, yScreen; //scaling to our screen size
        protected float dCamToScreen; //distance from camera to screen

        public _3DObject(int x, int y, int z)
        {
            xWorld = x;
            yWorld = y;
            zWorld = z;
        }

        private void translate(Camera camera)
        {
            xCam = xWorld - camera.X;
            yCam = yWorld - camera.Y;
            zCam = zWorld - camera.Z;
        }

        private void normalize(Camera camera)
        {
            //why the fuck does math.round return a decimal instead of an int ???
            xProj = xCam * (camera.DCamToScreen)/zCam;
            yProj = yCam * (camera.DCamToScreen)/zCam;
        }

        private void scale(Vector2 screenSize)
        {
            xScreen = (screenSize.X / 2) + (screenSize.X / 2) * xProj;
            yScreen = (screenSize.Y / 2) - (screenSize.Y / 2) * yProj;
        }

        //run every update cycle to ensure all the x,y,z coords are consistent with one another
        protected void updatePositions(Camera camera, Vector2 screenSize)
        {
            translate(camera);
            normalize(camera);
            scale(screenSize);
        }

        public abstract void loadContent(ContentManager Content);
    }
}
