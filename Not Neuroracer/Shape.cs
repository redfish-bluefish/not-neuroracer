using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

//whether or not a shape is correct or not
public enum Correct
{
    RIGHT, LEFT
}

namespace NotNeuroRacer
{
    class Shape : _2DObject
    {
        Correct correctnessValue;
        public Correct CorrectnessValue {
            get { return correctnessValue; }
            set { correctnessValue = value; }
        }
        public Shape(Vector2 pos, int w, int h, Correct correct)
        {
            position = new Vector2(pos.X, pos.Y);
            sizeRect = new Rectangle((int)pos.X, (int)pos.Y, w, h);
            correctnessValue = correct;
        }

        public override void loadContent() { }
        public void loadContent(ContentManager Content, String name)
        {
            texture = Content.Load<Texture2D>(name);
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(texture, sizeRect, Color.White);
            spriteBatch.End();
        }
    }
}
