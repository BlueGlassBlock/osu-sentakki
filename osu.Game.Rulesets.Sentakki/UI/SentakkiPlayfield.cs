using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI
{
    [Cached]
    public class SentakkiPlayfield : Playfield
    {
        private readonly Container<DrawableSentakkiJudgement> judgementLayer;

        private readonly Container<HitExplosion> explosionLayer;

        private readonly DrawablePool<DrawableSentakkiJudgement> judgementPool;
        private readonly DrawablePool<HitExplosion> explosionPool;

        private readonly SentakkiRing ring;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public const float RINGSIZE = 600;
        public const float DOTSIZE = 20f;
        public const float INTERSECTDISTANCE = 296.5f;
        public const float NOTESTARTDISTANCE = 66f;

        public readonly LanedPlayfield LanedPlayfield;
        private readonly TouchPlayfield touchPlayfield;

        internal readonly Container AccentContainer;

        private readonly KiaiColorBlast colorBlast;

        public static readonly float[] LANEANGLES =
        {
            22.5f,
            67.5f,
            112.5f,
            157.5f,
            202.5f,
            247.5f,
            292.5f,
            337.5f
        };

        public SentakkiPlayfield()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.None;
            Rotation = 0;
            Size = new Vector2(RINGSIZE);
            AddRangeInternal(new Drawable[]
            {
                colorBlast = new KiaiColorBlast(),
                explosionPool = new DrawablePool<HitExplosion>(8),
                judgementPool = new DrawablePool<DrawableSentakkiJudgement>(8),
                AccentContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new PlayfieldVisualisation(),
                        ring = new SentakkiRing(),
                    }
                },
                LanedPlayfield = new LanedPlayfield(),
                HitObjectContainer, // This only contains TouchHolds, which needs to be above others types
                touchPlayfield = new TouchPlayfield(), // This only contains Touch, which needs a custom playfield to handle their input
                explosionLayer = new Container<HitExplosion>() { RelativeSizeAxes = Axes.Both },
                judgementLayer = new Container<DrawableSentakkiJudgement>
                {
                    RelativeSizeAxes = Axes.Both,
                }
            });
            AddNested(LanedPlayfield);
            AddNested(touchPlayfield);
            NewResult += onNewResult;
        }

        [Resolved]
        private DrawableSentakkiRuleset drawableSentakkiRuleset { get; set; }

        [Resolved(canBeNull: true)]
        private SentakkiRulesetConfigManager sentakkiRulesetConfig { get; set; }

        private IBindable<Skin> skin;
        private Bindable<ColorOption> ringColor = new Bindable<ColorOption>();

        private IBindable<StarDifficulty?> beatmapDifficulty;

        [BackgroundDependencyLoader(true)]
        private void load(SkinManager skinManager, IBeatmap beatmap, BeatmapDifficultyCache difficultyCache)
        {
            RegisterPool<TouchHold, DrawableTouchHold>(2);

            // handle colouring of playfield elements
            beatmapDifficulty = difficultyCache.GetBindableDifficulty(beatmap.BeatmapInfo);

            skin = skinManager.CurrentSkin.GetBoundCopy();
            sentakkiRulesetConfig?.BindWith(SentakkiRulesetSettings.RingColor, ringColor);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            skin.BindValueChanged(_ => changePlayfieldAccent(), true);
            ringColor.BindValueChanged(_ => changePlayfieldAccent(), true);
        }

        protected override HitObjectLifetimeEntry CreateLifetimeEntry(HitObject hitObject) => new SentakkiHitObjectLifetimeEntry(hitObject, sentakkiRulesetConfig, drawableSentakkiRuleset);

        protected override GameplayCursorContainer CreateCursor() => new SentakkiCursorContainer();

        public override void Add(HitObject h)
        {
            switch (h)
            {
                case SentakkiLanedHitObject _:
                    LanedPlayfield.Add(h);
                    break;
                case Objects.Touch _:
                    touchPlayfield.Add(h);
                    break;
                default:
                    base.Add(h);
                    break;
            }
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!judgedObject.DisplayResult || !DisplayJudgements.Value || !(judgedObject is DrawableSentakkiHitObject sentakkiHitObject))
                return;

            judgementLayer.Add(judgementPool.Get(j => j.Apply(result, judgedObject)));

            if (!result.IsHit) return;

            if (judgedObject is DrawableSlideBody) return;

            if (judgedObject.HitObject.Kiai)
                ring.KiaiBeat();

            if (judgedObject is DrawableTouchHold && (judgedObject.HitObject.Kiai || judgedObject.HitObject.Samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH)))
                colorBlast.PerformColorBlast();

            var explosion = explosionPool.Get(e => e.Apply(sentakkiHitObject));
            explosionLayer.Add(explosion);
        }

        [Resolved]
        private OsuColour colours { get; set; }

        private void changePlayfieldAccent()
        {
            switch (ringColor.Value)
            {
                case ColorOption.Difficulty:
                    AccentContainer.FadeColour(colours.ForDifficultyRating(beatmapDifficulty?.Value.Value.DifficultyRating ?? DifficultyRating.Normal, true), 200);
                    break;
                case ColorOption.Skin:
                    AccentContainer.FadeColour(skin.Value.GetConfig<GlobalSkinColours, Color4>(GlobalSkinColours.MenuGlow)?.Value ?? Color4.White, 200);
                    break;
                default:
                    AccentContainer.FadeColour(Color4.White, 200);
                    break;
            }
        }
    }
}
