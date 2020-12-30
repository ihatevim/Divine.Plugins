﻿using System;
using System.Linq;

using Divine.BeAware.Helpers;
using Divine.BeAware.MenuManager.ShowMeMore;
using Divine.Helpers;
using Divine.SDK.Helpers;
using Divine.SDK.Managers.Update;

using SharpDX;

namespace Divine.BeAware.ShowMeMore
{
    internal class Additional
    {
        private CheckRuneMenu CheckRuneMenu { get; }

        private CheckHandOfMidasMenu CheckHandOfMidasMenu { get; }

        private RoshanMenu RoshanMenu { get; }

        private MessageCreator MessageCreator { get; }

        private SoundHelper SoundHelper { get; }

        public Vector2 RoshanPanelPosition { get; }

        private bool RoshanDead { get; set; }

        private int RoshanTick { get; set; }

        private string RoshanTextTimer { get; set; } = string.Empty;

        private float AegisTime { get; set; }

        private bool AegisEvent { get; set; }

        private bool AegisWasFound { get; set; }

        private Item Aegis { get; set; }

        private string AegisTextTimer { get; set; } = string.Empty;

        public Additional(Common common)
        {
            CheckRuneMenu = common.MenuConfig.ShowMeMoreMenu.CheckRuneMenu;
            CheckHandOfMidasMenu = common.MenuConfig.ShowMeMoreMenu.CheckHandOfMidasMenu;
            RoshanMenu = common.MenuConfig.ShowMeMoreMenu.RoshanMenu;

            MessageCreator = common.MessageCreator;
            SoundHelper = common.SoundHelper;

            RendererManager.LoadTextureFromResource("Divine.BeAware.Resources.Textures.roshan_alive.png");
            RendererManager.LoadTextureFromResource("Divine.BeAware.Resources.Textures.roshan_dead.png");

            RoshanPanelPosition = HUDInfo.GetCustomTopPanelPosition(1, Team.Radiant) - (new Vector2(480, -4) * RendererManager.Scaling);

            GameManager.FireEvent += OnFireEvent;
            UpdateManager.Subscribe(1000, OnTimeEvent);
            RendererManager.Draw += OnDraw;
        }

        public void Dispose()
        {
            RendererManager.Draw -= OnDraw;
            UpdateManager.Unsubscribe(OnTimeEvent);
            GameManager.FireEvent += OnFireEvent;
        }

        private void OnDraw()
        {
            if (!RoshanMenu.PanelItem)
            {
                return;
            }

            var textureSize = new Vector2(320, 72) * RendererManager.Scaling;

            if (RoshanDead)
            {
                RendererManager.DrawTexture("Divine.BeAware.Resources.Textures.roshan_dead.png", new RectangleF(RoshanPanelPosition.X, RoshanPanelPosition.Y, textureSize.X, textureSize.Y));
                RendererManager.DrawText(RoshanTextTimer, RoshanPanelPosition + (new Vector2(86, 12) * RendererManager.Scaling), Color.Red, 40 * RendererManager.Scaling);

                if (AegisWasFound && RoshanMenu.AegisItem)
                {
                    RendererManager.DrawText(AegisTextTimer, RoshanPanelPosition + (new Vector2(148, 44) * RendererManager.Scaling), Color.Aqua, 28 * RendererManager.Scaling);
                }
            }
            else
            {
                RendererManager.DrawTexture("Divine.BeAware.Resources.Textures.roshan_alive.png", new RectangleF(RoshanPanelPosition.X, RoshanPanelPosition.Y, textureSize.X, textureSize.Y));
                RendererManager.DrawText("Roshan Alive", RoshanPanelPosition + (new Vector2(86, 12) * RendererManager.Scaling), Color.Aqua, 40 * RendererManager.Scaling);
            }
        }

        private void OnFireEvent(FireEventEventArgs e)
        {
            var name = e.Name;
            if (name == "dota_roshan_kill")
            {
                RoshanDead = true;
                return;
            }

            if (RoshanMenu.PanelItem && RoshanMenu.AegisItem && name == "aegis_event")
            {
                AegisTime = GameManager.GameTime;
                AegisEvent = true;
                return;
            }
        }

        private readonly Sleeper checkRuneSleeper = new();

        private readonly Sleeper midasSleeper = new();

        private void OnTimeEvent()
        {
            if (GameManager.IsPaused)
            {
                return;
            }

            // Check Rune
            var gameTime = GameManager.GameTime;
            if (CheckRuneMenu.EnableItem && !checkRuneSleeper.Sleeping && ((Math.Round(gameTime + 10)) % 120 == 0 || (Math.Round(gameTime + 10)) % 300 == 0))
            {
                if (CheckRuneMenu.SideMessageItem)
                {
                    MessageCreator.MessageCheckRuneCreator();
                }

                if (CheckRuneMenu.PlaySoundItem)
                {
                    SoundHelper.Play("check_rune");
                }

                checkRuneSleeper.Sleep(2000);
            }

            // Hand Of Midas
            if (CheckHandOfMidasMenu.EnableItem)
            {
                var handOfMidas = EntityManager.LocalHero.Inventory.GetItemsById(AbilityId.item_hand_of_midas).FirstOrDefault();
                if (handOfMidas != null && Math.Round(handOfMidas.Cooldown) == 5 && !midasSleeper.Sleeping)
                {
                    if (CheckHandOfMidasMenu.SideMessageItem)
                    {
                        MessageCreator.MessageUseMidasCreator();
                    }

                    if (CheckHandOfMidasMenu.PlaySoundItem)
                    {
                        SoundHelper.Play("use_midas");
                    }

                    midasSleeper.Sleep(2000);
                }
            }

            if (RoshanDead)
            {
                RoshanTick += 1;

                if (RoshanMenu.PanelItem)
                {
                    var tickMin = TimeSpan.FromSeconds(480 - RoshanTick);
                    var tickMax = TimeSpan.FromSeconds(660 - RoshanTick);

                    var roshanMin = "0:00";
                    if (tickMin.TotalSeconds > 0)
                    {
                        roshanMin = string.Format("{0:0}:{1:00}", tickMin.Minutes, tickMin.Seconds);
                    }

                    var roshanMax = "0:00";
                    if (tickMax.TotalSeconds > 0)
                    {
                        roshanMax = string.Format("{0:0}:{1:00}", tickMax.Minutes, tickMax.Seconds);
                    }

                    RoshanTextTimer = $"{ roshanMin } - { roshanMax }";
                }

                if (RoshanTick == 480)
                {
                    if (RoshanMenu.SideMessageItem)
                    {
                        MessageCreator.MessageRoshanMBAliveCreator();
                    }

                    if (RoshanMenu.PlaySoundItem)
                    {
                        SoundHelper.Play("roshan_mb_alive");
                    }
                }

                if (RoshanTick > 480)
                {
                    if (EntityManager.GetEntities<Unit>().Any(x => x.IsAlive && x.ClassId == ClassId.CDOTA_Unit_Roshan) || RoshanTick == 660)
                    {
                        if (RoshanMenu.SideMessageItem)
                        {
                            MessageCreator.MessageRoshanAliveCreator();
                        }

                        if (RoshanMenu.PlaySoundItem)
                        {
                            SoundHelper.Play("roshan_alive");
                        }

                        RoshanTick = 0;
                        RoshanDead = false;
                    }
                }
            }

            if (AegisEvent)
            {
                if (!AegisWasFound)
                {
                    Aegis = EntityManager.GetEntities<Item>().FirstOrDefault(x => x.Id == AbilityId.item_aegis);
                }

                if (Aegis != null && !AegisWasFound)
                {
                    AegisWasFound = true;
                }

                var time = TimeSpan.FromSeconds(300 - (gameTime - AegisTime));
                if (time.Ticks > 0)
                {
                    AegisTextTimer = string.Format("{0:0}:{1:00}", time.Minutes, time.Seconds);
                }

                if (time.Ticks < 0 || (AegisWasFound && (Aegis == null || !Aegis.IsValid)))
                {
                    AegisEvent = false;
                    AegisWasFound = false;
                }
            }
        }
    }
}