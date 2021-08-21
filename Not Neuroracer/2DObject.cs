using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NotNeuroRacer
{
    public abstract class _2DObject
    {
        protected Vector2 position;
        protected Rectangle sizeRect;
        protected Texture2D texture;

        public abstract void loadContent();
        public abstract void draw(SpriteBatch spriteBatch);
    }
}
