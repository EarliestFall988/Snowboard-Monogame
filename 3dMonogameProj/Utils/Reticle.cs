using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utils
{
    public class Reticle : DrawableGameComponent
    {
        Texture2D _texture;
        Vector2 size = new Vector2(0.0125f, 0.0125f);

        public Reticle(Game game, Texture2D texture) : base(game)
        {
            _texture = texture;
        }

        public override void Draw(GameTime gameTime)
        {

            Vector2 centerOfScreen = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

            if (_texture != null)
            {
                SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
                spriteBatch.Begin(SpriteSortMode.FrontToBack);
                spriteBatch.Draw(_texture, centerOfScreen, new Rectangle(0, 0, _texture.Width, _texture.Height), Color.White, 0f, new Vector2(_texture.Width / 2, _texture.Height / 2), size, SpriteEffects.None, 0f);
                spriteBatch.End();

                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }

            base.Draw(gameTime);
        }
    }
}
