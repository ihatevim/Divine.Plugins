using System.Linq;

using Divine.BeAware.MenuManager.ShowMeMore;
using Divine.Menu.EventArgs;
using Divine.Menu.Items;
using Divine.SDK.Extensions;

namespace Divine.BeAware.ShowMeMore
{
    internal sealed class TrueSightVision
    {
        private readonly Hero LocalHero = EntityManager.LocalHero;

        private readonly TrueSightVisionMenu TrueSightVisionMenu;

        public TrueSightVision(Common common)
        {
            TrueSightVisionMenu = common.MenuConfig.ShowMeMoreMenu.TrueSightVisionMenu;

            TrueSightVisionMenu.EnableItem.ValueChanged += OnEnableValueChanged;
        }

        public void Dispose()
        {
            TrueSightVisionMenu.EnableItem.ValueChanged -= OnEnableValueChanged;

            if (TrueSightVisionMenu.EnableItem)
            {
                foreach (var unit in EntityManager.GetEntities<Unit>())
                {
                    if (!unit.IsAlly(LocalHero))
                    {
                        continue;
                    }

                    if (!unit.ModifierStatus.Debuffs.Any(x => x.Name == "modifier_truesight"))
                    {
                        continue;
                    }

                    ParticleRemove(unit.Handle);
                }

                ModifierManager.ModifierAdded -= OnModifierManagerModifierAdded;
                ModifierManager.ModifierRemoved -= OnModifierManagerModifierRemoved;
            }
        }

        private void OnEnableValueChanged(MenuSwitcher switcher, SwitcherEventArgs e)
        {
            if (e.Value)
            {
                foreach (var unit in EntityManager.GetEntities<Unit>())
                {
                    if (!unit.IsAlly(LocalHero))
                    {
                        continue;
                    }

                    if (!unit.ModifierStatus.Debuffs.Any(x => x.Name == "modifier_truesight"))
                    {
                        continue;
                    }

                    ParticleAdd(unit);
                }

                ModifierManager.ModifierAdded += OnModifierManagerModifierAdded;
                ModifierManager.ModifierRemoved += OnModifierManagerModifierRemoved;
            }
            else
            {
                foreach (var unit in EntityManager.GetEntities<Unit>())
                {
                    if (!unit.IsAlly(LocalHero))
                    {
                        continue;
                    }

                    if (!unit.ModifierStatus.Debuffs.Any(x => x.Name == "modifier_truesight"))
                    {
                        continue;
                    }

                    ParticleRemove(unit.Handle);
                }

                ModifierManager.ModifierAdded -= OnModifierManagerModifierAdded;
                ModifierManager.ModifierRemoved -= OnModifierManagerModifierRemoved;
            }
        }

        private void OnModifierManagerModifierAdded(ModifierAddedEventArgs e)
        {
            var owner = e.Modifier.Owner;
            if (owner is not Unit unit || unit.IsEnemy(LocalHero) || e.Modifier.Name != "modifier_truesight")
            {
                return;
            }

            ParticleAdd(EntityManager.GetEntityByHandle(unit.Handle));
        }

        private void OnModifierManagerModifierRemoved(ModifierRemovedEventArgs e)
        {
            var owner = e.Modifier.Owner;
            if (owner is not Unit unit || unit.IsEnemy(LocalHero) || e.Modifier.Name != "modifier_truesight")
            {
                return;
            }

            ParticleRemove(unit.Handle);
        }

        private void ParticleAdd(Entity entity)
        {
            if (entity == null)
            {
                return;
            }

            ParticleManager.CreateOrUpdateParticle(
                    $"TrueSightVision_{entity.Handle}",
                    "particles/items2_fx/ward_true_sight.vpcf",
                    entity,
                    ParticleAttachment.AbsOriginFollow);
        }

        private void ParticleRemove(uint handle)
        {
            ParticleManager.RemoveParticle($"TrueSightVision_{handle}");
        }
    }
}