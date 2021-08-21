using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotNeuroRacer
{
    class Camera
    {
        float x, y, z;
        public float X { get { return x; } set { x = value; } }
        public float Y { get { return y; } set { y = value; } }
        public float Z { get { return z; } set { z = value; } }

        float dCamToScreen; //distance from the camera to the screen
        public float DCamToScreen { get { return dCamToScreen; } }

        public Camera(int x1, int y1, int z1, int fov)
        {
            x = x1;
            y = y1;
            z = z1;
            float fovRad = fov * (float)Math.PI / 180;
            dCamToScreen = (float)((float)1 / (Math.Tan(fovRad / 2)));
        }
    }
}
