using BEPUphysics.Entities;
using Core.Utils;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.FPS;

namespace Core
{

    /*
     *  Apart of the bepuphysics library
     */

    /// <summary>
    /// Component that draws a model following the position and orientation of a BEPUphysics entity.
    /// </summary>
    public class EntityModel : DrawableGameComponent
    {

        Entity entity;
        Model model;

        public BEPUutilities.Matrix Transform;
        Matrix[] boneTransforms;

        ICamera camera;


        public EntityModel(Entity entity, Model model, ICamera camera, BEPUutilities.Matrix transform, Game game)
            : base(game)
        {
            this.entity = entity;
            this.model = model;
            this.Transform = transform;
            this.camera = camera;


            boneTransforms = new Matrix[model.Bones.Count];
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.LightingEnabled = true;
                    effect.DirectionalLight0.DiffuseColor = new Vector3(0.3f, 0.37f, 0.33f);
                    effect.DirectionalLight0.Direction = new Vector3(1, -2, 0);
                    effect.PreferPerPixelLighting = true;
                    effect.AmbientLightColor = new Vector3(0.3f, 0.37f, 0.33f);
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {

            Matrix worldMatrix = MathConverter.Convert(Transform * entity.WorldTransform);


            model.CopyAbsoluteBoneTransformsTo(boneTransforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                }
                mesh.Draw();
            }
            base.Draw(gameTime);
        }
    }
}
