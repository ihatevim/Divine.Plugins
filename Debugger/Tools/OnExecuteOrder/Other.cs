//namespace Debugger.Tools.OnExecuteOrder
//{
//    using System.Collections.Generic;
//    using System.ComponentModel;
//    using System.ComponentModel.Composition;

//    using Logger;

//    using SharpDX;

//    internal class Other : IDebuggerTool
//    {
//        private readonly HashSet<OrderId> ignoreOrders = new HashSet<OrderId>
//        {
//            OrderId.Ability,
//            OrderId.AbilityLocation,
//            OrderId.AbilityTarget,
//            OrderId.AbilityTargetRune,
//            OrderId.AbilityTargetTree,
//            OrderId.ToggleAbility,
//            OrderId.AttackLocation,
//            OrderId.AttackTarget,
//            OrderId.MoveLocation,
//            OrderId.MoveTarget,
//            OrderId.Stop,
//            OrderId.Hold,
//            OrderId.Continue,
//            OrderId.Patrol
//        };

//        private MenuItem<bool> enabled;

//        [Import]
//        private readonly ILog log;

//        [Import]
//        private IMenu mainMenu;

//        private MenuFactory menu;

//        public int LoadPriority { get; } = 86;

//        public void Activate()
//        {
//            this.menu = this.mainMenu.OnExecuteOrderMenu;

//            this.enabled = this.menu.Item("Other", false);
//            this.enabled.Item.SetTooltip("Player.OnExecuteOrder");
//            this.enabled.PropertyChanged += this.EnabledOnPropertyChanged;

//            if (this.enabled)
//            {
//                this.menu.AddAsterisk();
//            }
//        }

//        public void Dispose()
//        {
//            this.enabled.PropertyChanged -= this.EnabledOnPropertyChanged;
//            Player.OnExecuteOrder -= this.PlayerOnExecuteOrder;
//        }

//        private void EnabledOnPropertyChanged(object sender, PropertyChangedEventArgs e)
//        {
//            if (this.enabled)
//            {
//                this.menu.AddAsterisk();
//                Player.OnExecuteOrder += this.PlayerOnExecuteOrder;
//            }
//            else
//            {
//                this.menu.RemoveAsterisk();
//                Player.OnExecuteOrder -= this.PlayerOnExecuteOrder;
//            }
//        }

//        private bool IsValid(ExecuteOrderEventArgs args)
//        {
//            if (this.ignoreOrders.Contains(args.OrderId))
//            {
//                return false;
//            }

//            return true;
//        }

//        private void PlayerOnExecuteOrder(Player sender, ExecuteOrderEventArgs args)
//        {
//            if (!this.IsValid(args))
//            {
//                return;
//            }

//            var item = new LogItem(LogType.ExecuteOrder, Color.Magenta, "Execute order");

//            item.AddLine("Order: " + args.OrderId, args.OrderId);
//            if (args.Ability != null)
//            {
//                item.AddLine("Ability name: " + args.Ability.Name, args.Ability.Name);
//                item.AddLine("Ability network name: " + args.Ability.NetworkName, args.Ability.NetworkName);
//                item.AddLine("Ability classID: " + args.Ability.ClassId, args.Ability.ClassId);
//            }

//            if (args.Target != null)
//            {
//                item.AddLine("Target name: " + args.Target.Name, args.Target.Name);
//                item.AddLine("Target network name: " + args.Target.NetworkName, args.Target.NetworkName);
//                item.AddLine("Target classID: " + args.Target.ClassId, args.Target.ClassId);
//            }

//            if (!args.TargetPosition.IsZero)
//            {
//                item.AddLine("Target position: " + args.TargetPosition, args.TargetPosition);
//            }

//            this.log.Display(item);
//        }
//    }
//}