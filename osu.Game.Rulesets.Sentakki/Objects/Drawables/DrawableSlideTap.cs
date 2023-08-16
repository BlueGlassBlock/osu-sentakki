﻿using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableSlideTap : DrawableTap
    {
        protected override Drawable CreateTapRepresentation() => new SlideTapPiece();

        public DrawableSlideTap()
            : this(null)
        {
        }

        public DrawableSlideTap(SlideTap? hitObject)
            : base(hitObject)
        {
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            var note = (SlideTapPiece)TapVisual;

            double spinDuration = 0;

            if (ParentHitObject is DrawableSlide slide)
            {
                spinDuration = slide.HitObject.SlideInfoList.FirstOrDefault()?.Duration + 250 ?? 1000;
                note.SecondStar.Alpha = slide.SlideBodies.Count > 1 ? 1 : 0;
            }

            if (spinDuration != 0)
                note.Stars.Spin(spinDuration, RotationDirection.Counterclockwise).Loop();
        }
    }
}
