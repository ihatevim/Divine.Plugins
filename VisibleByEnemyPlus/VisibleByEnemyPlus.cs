using Divine;

using SharpDX;

namespace VisibleByEnemyPlus
{
    public class VisibleByEnemyPlus : Bootstrapper
    {
        private Config Config { get; set; }

        private bool AddEffectType { get; set; }

        private int Red => Config.RedItem;

        private int Green => Config.GreenItem;

        private int Blue => Config.BlueItem;

        private int Alpha => Config.AlphaItem;

        protected override void OnActivate()
        {
            Config = new Config();

            Config.EffectTypeItem.ValueChanged += (selector, e) => { UpdateMenu(e.NewValue, Red, Green, Blue, Alpha); };
            Config.RedItem.ValueChanged += (slider, e) => { UpdateMenu(Config.EffectTypeItem, e.NewValue, Green, Blue, Alpha); };
            Config.GreenItem.ValueChanged += (slider, e) => { UpdateMenu(Config.EffectTypeItem, Red, e.NewValue, Blue, Alpha); };
            Config.BlueItem.ValueChanged += (slider, e) => { UpdateMenu(Config.EffectTypeItem, Red, Green, e.NewValue, Alpha); };
            Config.AlphaItem.ValueChanged += (slider, e) => { UpdateMenu(Config.EffectTypeItem, Red, Green, Blue, e.NewValue); };

            UpdateManager.IngameUpdate += OnIngameUpdate;
        }

        /*protected override void OnDeactivate()
        {
            UpdateManager.Unsubscribe(LoopEntities);

            Config.EffectTypeItem.PropertyChanged -= ItemChanged;

            Config.RedItem.PropertyChanged -= ItemChanged;
            Config.GreenItem.PropertyChanged -= ItemChanged;
            Config.BlueItem.PropertyChanged -= ItemChanged;
            Config.AlphaItem.PropertyChanged -= ItemChanged;

            Config?.Dispose();
            ParticleManager.Dispose();
        }*/

        private void UpdateMenu(string selector, int red, int green, int blue, int alpha)
        {
            if (selector == "Default")
            {
                Config.RedItem.SetFontColor(Color.Black);
                Config.GreenItem.SetFontColor(Color.Black);
                Config.BlueItem.SetFontColor(Color.Black);
                Config.AlphaItem.SetFontColor(Color.Black);
            }
            else
            {
                Config.RedItem.SetFontColor(new Color(red, 0, 0, 255));
                Config.GreenItem.SetFontColor(new Color(0, green, 0, 255));
                Config.BlueItem.SetFontColor(new Color(0, 0, blue, 255));
                Config.AlphaItem.SetFontColor(new Color(185, 176, 163, alpha));
            }

            var localHero = EntityManager.LocalHero;
            if (localHero != null && localHero.IsValid)
            {
                HandleEffect(localHero, true);
                AddEffectType = false;
            }
        }

        private bool IsMine(Entity sender)
        {
            return sender.ClassId == ClassId.CDOTA_NPC_TechiesMines;
        }

        private bool IsUnit(Unit sender)
        {
            return (sender.ClassId != ClassId.CDOTA_BaseNPC_Creep_Lane
                   && sender.ClassId != ClassId.CDOTA_BaseNPC_Creep_Siege
                   || sender.IsControllable)
                   && sender.ClassId != ClassId.CDOTA_NPC_TechiesMines
                   && sender.ClassId != ClassId.CDOTA_NPC_Observer_Ward
                   && sender.ClassId != ClassId.CDOTA_NPC_Observer_Ward_TrueSight
                   && sender.ClassId != ClassId.CDOTA_BaseNPC_Healer;
        }

        private float LastTime;

        private void OnIngameUpdate()
        {
            if (GameManager.RawGameTime - LastTime < 0.25f)
            {
                return;
            }

            LastTime = GameManager.RawGameTime;

            var localHero = EntityManager.LocalHero;
            if (localHero == null || !localHero.IsValid)
            {
                return;
            }

            foreach (var unit in EntityManager.GetEntities<Unit>())
            {
                if (unit.Team == localHero.Team)
                {
                    if (Config.AlliedHeroesItem && unit is Hero)
                    {
                        HandleEffect(unit, unit.IsVisibleToEnemies);
                    }
                    else if (Config.BuildingsItem && unit is Building)
                    {
                        HandleEffect(unit, unit.IsVisibleToEnemies);
                    }
                    else if (Config.WardsItem && unit is WardObserver)
                    {
                        HandleEffect(unit, unit.IsVisibleToEnemies);
                    }
                    else if (Config.MinesItem && IsMine(unit))
                    {
                        HandleEffect(unit, unit.IsVisibleToEnemies);
                    }
                    else if (Config.OutpostsItem && unit is Outpost)
                    {
                        HandleEffect(unit, unit.IsVisibleToEnemies);
                    }
                    else if (Config.UnitsItem && IsUnit(unit))
                    {
                        HandleEffect(unit, unit.IsVisibleToEnemies);
                    }
                }
                else if (Config.NeutralsItem && unit is Neutral)
                {
                    HandleEffect(unit, unit.IsVisibleToEnemies);
                }
            }
        }

        private void HandleEffect(Unit unit, bool visible)
        {
            if (!AddEffectType /*&& Owner.Animation.Name != "idle"*/)
            {
                AddEffectType = true;
            }

            if (visible && unit.IsAlive /*&& unit.Position.IsOnScreen()*/)
            {
                ParticleManager.CreateOrUpdateParticle(
                    $"VisibleByEnemyPlus.{unit.Handle}",
                    Config.Effects[Config.EffectTypeItem],
                     unit,
                    ParticleAttachment.AbsOriginFollow,
                    new ControlPoint(1, Red, Green, Blue),
                    new ControlPoint(2, Alpha));
            }
            else if (AddEffectType)
            {
                ParticleManager.RemoveParticle($"VisibleByEnemyPlus.{unit.Handle}");
            }
        }
    }
}