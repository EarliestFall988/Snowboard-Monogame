using BEPUphysics.Entities;

using Core.Utils;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utils
{
    public class GameObject : DrawableGameComponent
    {

        Entity entity;

        Model model;

        public BEPUutilities.Matrix Transform { get; set; } //think like transform.position in unity... but it's a matrix
        Matrix[] boneTransforms;

        public GameObject(Entity entity, Model model, BEPUutilities.Matrix transform, Game game) : base(game)
        {
            this.entity = entity;
            this.model = model;

            boneTransforms = new Matrix[model.Bones.Count];

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();

                }
            }
        }

        public override void Draw(GameTime gameTime)
        {

            Matrix worldMatrix = MathConverter.Convert(Transform * entity.WorldTransform);

            base.Draw(gameTime);
        }
    }
}
