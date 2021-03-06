using System.Text;

using Divine.BeAware.Helpers;
using Divine.BeAware.MenuManager;
using Divine.BeAware.MenuManager.ShowMeMore;
using Divine.Helpers;

using SharpDX;

namespace Divine.BeAware.ShowMeMore.MoreInformation
{
    internal abstract class Base
    {
        protected readonly MenuConfig MenuConfig;

        protected readonly MoreInformationMenu MoreInformationMenu;

        protected readonly Verification Verification;

        protected readonly Hero LocalHero = EntityManager.LocalHero;

        public Base(Common common)
        {
            MenuConfig = common.MenuConfig;
            MoreInformationMenu = MenuConfig.ShowMeMoreMenu.MoreInformationMenu;

            Verification = common.Verification;
        }

        public virtual bool Modifier(Unit unit, Modifier modifier)
        {
            return false;
        }

        public virtual bool Modifier(Unit unit, Modifier modifier, bool isHero)
        {
            return false;
        }

        public virtual bool Entity(Unit unit)
        {
            return false;
        }

        public virtual bool Entity(Unit unit, Hero hero)
        {
            return false;
        }

        public virtual bool Particle(Particle particle, string name)
        {
            return false;
        }

        protected Vector3 Pos(Vector3 position, bool menu)
        {
            return menu ? position : Vector3.Zero;
        }

        protected Vector2 MinimapPos(Vector3 position, bool menu)
        {
            return menu ? position.WorldToMinimap() : Vector2.Zero;
        }

        protected void DrawRange(string id, Vector3 position, float radius, Color color, int alpha)
        {
            ParticleManager.CreateOrUpdateParticle(
                $"DrawRange_{id}",
                @"materials\ensage_ui\particles\alert_range.vpcf",
                ParticleAttachment.AbsOrigin,
                new ControlPoint(0, position),
                new ControlPoint(1, color),
                new ControlPoint(2, radius, 255, alpha));
        }

        protected void DrawRangeRemove(string id)
        {
            ParticleManager.RemoveParticle($"DrawRange_{id}");
        }

        protected void DrawLine(string id, Vector3 startPosition, Vector3 endPosition, int size, int alpha, Color color)
        {
            ParticleManager.CreateOrUpdateParticle(
                $"DrawLine_{id}",
                @"materials\ensage_ui\particles\rectangle.vpcf",
                ParticleAttachment.AbsOrigin,
                new ControlPoint(1, startPosition),
                new ControlPoint(2, endPosition),
                new ControlPoint(3, size, alpha, 0),
                new ControlPoint(4, color));
        }

        protected void DrawLineRemove(string id)
        {
            ParticleManager.RemoveParticle($"DrawLine_{id}");
        }

        protected void DisplayMessage(string message, bool encodingDefault = false)
        {
            if (encodingDefault)
            {
                GameManager.ExecuteCommand(Encoding.Default.GetString(Encoding.UTF8.GetBytes(message)));
                return;
            }

            GameManager.ExecuteCommand(message);
        }
    }
}