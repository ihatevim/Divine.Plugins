﻿namespace Debugger.Tools.Information
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Text;

    using Debugger.Menus;

    using Divine;
    using Divine.Menu.EventArgs;
    using Divine.Menu.Items;
    using Divine.SDK.Localization;
    using Divine.SDK.Managers.Update;

    using Logger;

    using SharpDX;

    internal class Items : IDebuggerTool
    {
        private readonly Player player;

        private MenuSwitcher autoUpdate;

        private MenuSwitcher information;

        private uint lastUnitInfo;

        private readonly ILog log;

        private IMainMenu mainMenu;

        private Menu menu;

        private MenuSwitcher showBackpack;

        private MenuSwitcher showBehavior;

        private MenuSwitcher showCastRange;

        private MenuSwitcher showInventory;

        private MenuSwitcher showManaCost;

        private MenuSwitcher showSpecialData;

        private MenuSwitcher showStash;

        private MenuSwitcher showTargetType;


        public Items(IMainMenu mainMenu, ILog log)
        {
            this.mainMenu = mainMenu;
            this.log = log;
            this.player = EntityManager.LocalPlayer;
        }

        public int LoadPriority { get; } = 80;

        public void Activate()
        {
            this.menu = this.mainMenu.InformationMenu.CreateMenu("Items");

            this.information = this.menu.CreateSwitcher("Get", false);
            this.information.ValueChanged += this.InformationOnPropertyChanged;

            this.autoUpdate = this.menu.CreateSwitcher("Auto update", false);
            this.autoUpdate.ValueChanged += this.AutoUpdateOnPropertyChanged;

            this.showInventory = this.menu.CreateSwitcher("Show inventory items", true);
            this.showBackpack = this.menu.CreateSwitcher("Show backpack items", false);
            this.showStash = this.menu.CreateSwitcher("Show stash items", false);
            this.showManaCost = this.menu.CreateSwitcher("Show mana cost", false);
            this.showCastRange = this.menu.CreateSwitcher("Show cast range", false);
            this.showBehavior = this.menu.CreateSwitcher("Show behavior", false);
            this.showTargetType = this.menu.CreateSwitcher("Show target type", false);
            this.showSpecialData = this.menu.CreateSwitcher("Show all special data", false);

            this.AutoUpdateOnPropertyChanged(null, null);
        }

        public void Dispose()
        {
            this.information.ValueChanged -= this.InformationOnPropertyChanged;
            this.autoUpdate.ValueChanged -= this.AutoUpdateOnPropertyChanged;
            GameManager.FireEvent -= this.GameOnFireEvent;
        }

        private void AutoUpdateOnPropertyChanged(MenuSwitcher switcher, SwitcherEventArgs e)
        {
            UpdateManager.BeginInvoke(() =>
            {
                if (this.autoUpdate)
                {
                    this.menu.AddAsterisk();
                    GameManager.FireEvent += this.GameOnFireEvent;
                    this.InformationOnPropertyChanged(null, null);
                }
                else
                {
                    this.menu.RemoveAsterisk();
                    GameManager.FireEvent -= this.GameOnFireEvent;
                }
            });
        }

        private void GameOnFireEvent(FireEventEventArgs e)
        {
            if (e.Name != "dota_player_update_selected_unit" && e.Name != "dota_player_update_query_unit")
            {
                return;
            }

            var unit = (this.player.QueryUnit ?? this.player.SelectedUnits.FirstOrDefault()) as Unit;
            if (unit?.IsValid != true)
            {
                return;
            }

            if (unit.Handle == this.lastUnitInfo)
            {
                return;
            }

            this.InformationOnPropertyChanged(null, null);
        }

        private void InformationOnPropertyChanged(MenuSwitcher switcher, SwitcherEventArgs e)
        {
            UpdateManager.BeginInvoke(() =>
            {
                var unit = (this.player.QueryUnit ?? this.player.SelectedUnits.FirstOrDefault()) as Unit;
                if (unit?.IsValid != true || !unit.HasInventory)
                {
                    return;
                }

                this.lastUnitInfo = unit.Handle;

                var item = new LogItem(LogType.Item, Color.PaleGreen, "Items information");

                item.AddLine("Unit name: " + unit.Name, unit.Name);
                item.AddLine("Unit network name: " + unit.NetworkName, unit.NetworkName);
                item.AddLine("Unit classID: " + unit.ClassId, unit.ClassId);

                var localizeName = LocalizationHelper.LocalizeName(unit);
                item.AddLine("Unit display name: " + localizeName, localizeName);

                var items = new List<Item>();

                if (this.showInventory)
                {
                    items.AddRange(unit.Inventory.Items);
                }

                if (this.showBackpack)
                {
                    items.AddRange(unit.Inventory.BackpackItems);
                }

                if (this.showStash)
                {
                    items.AddRange(unit.Inventory.StashItems);
                }

                foreach (var ability in items.Reverse<Item>())
                {
                    var abilityItem = new LogItem(LogType.Spell, Color.PaleGreen);

                    abilityItem.AddLine("Name: " + ability.Name, ability.Name);
                    abilityItem.AddLine("Network name: " + ability.NetworkName, ability.NetworkName);
                    abilityItem.AddLine("ClassID: " + ability.ClassId, ability.ClassId);

                    var localizeAbilityName = LocalizationHelper.LocalizeAbilityName(ability.Name);
                    abilityItem.AddLine("Display name: " + localizeAbilityName, localizeAbilityName);

                    if (this.showManaCost)
                    {
                        abilityItem.AddLine("Mana cost: " + ability.ManaCost, ability.ManaCost);
                    }

                    if (this.showCastRange)
                    {
                        abilityItem.AddLine("Cast range: " + ability.CastRange, ability.CastRange);
                    }

                    if (this.showBehavior)
                    {
                        abilityItem.AddLine("Behavior: " + ability.AbilityBehavior, ability.AbilityBehavior);
                    }

                    if (this.showTargetType)
                    {
                        abilityItem.AddLine("Target type: " + ability.TargetType, ability.TargetType);
                        abilityItem.AddLine("Target team type: " + ability.TargetTeamType, ability.TargetTeamType);
                    }

                    if (this.showSpecialData)
                    {
                        abilityItem.AddLine("Special data =>");
                        foreach (var abilitySpecialData in ability.AbilitySpecialData.Where(x => !x.Name.StartsWith("#")))
                        {
                            var values = new StringBuilder();
                            var count = abilitySpecialData.Count;

                            for (uint i = 0; i < count; i++)
                            {
                                values.Append(abilitySpecialData.GetValue(i));
                                if (i < count - 1)
                                {
                                    values.Append(", ");
                                }
                            }

                            abilityItem.AddLine("  " + abilitySpecialData.Name + ": " + values, abilitySpecialData.Name);
                        }
                    }

                    this.log.Display(abilityItem);
                }

                this.log.Display(item);
            });
        }
    }
}