using Divine.BeAware.MenuManager.ShowMeMore.Range;

namespace Divine.BeAware.MenuManager.ShowMeMore
{
    internal sealed class RangeMenu
    {
        public RangeMenu(Menu.Items.Menu showMeMoreMenu)
        {
            var rangeMenu = showMeMoreMenu.CreateMenu("Range");
            CustomRangeMenu = new CustomRangeMenu(rangeMenu);
            CustomRange2Menu = new CustomRange2Menu(rangeMenu);
            CustomRange3Menu = new CustomRange3Menu(rangeMenu);
        }

        public CustomRangeMenu CustomRangeMenu { get; }

        public CustomRange2Menu CustomRange2Menu { get; }

        public CustomRange3Menu CustomRange3Menu { get; }
    }
}