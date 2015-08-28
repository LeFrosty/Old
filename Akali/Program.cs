using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;


namespace Akali
{
    class Program
    {

        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private static Orbwalking.Orbwalker Orbwalker;

        private static Spell Q, W, E, R;

        private static Menu Menu;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Akali")
                return;

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R, 700);

            Menu = new Menu(Player.ChampionName, Player.ChampionName, true);

            Menu orbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));

            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            Menu ts = Menu.AddSubMenu(new Menu("Target Selector", "Target Selector"));

            TargetSelector.AddToMenu(ts);

            Menu comboMenu = Menu.AddSubMenu(new Menu("Combo", "Combo"));

            comboMenu.AddItem(new MenuItem("comboQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboR", "Use R").SetValue(true));

            comboMenu.AddItem(new MenuItem("Combo", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));

            Menu harassMenu = Menu.AddSubMenu(new Menu("Harass", "Harass"));

            harassMenu.AddItem(new MenuItem("harassQ", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("harassW", "Use W").SetValue(false));
            harassMenu.AddItem(new MenuItem("harassE", "Use E").SetValue(true));
            harassMenu.AddItem(new MenuItem("harassR", "Use R").SetValue(false));

            harassMenu.AddItem(
                new MenuItem("Harass", "Harass").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Menu clearMenu = Menu.AddSubMenu(new Menu("Lane Clear", "Lane Clear"));

            clearMenu.AddItem(new MenuItem("clearQ", "Use Q").SetValue(true));
            clearMenu.AddItem(new MenuItem("clearE", "Use E").SetValue(true));
            clearMenu.AddItem(new MenuItem("clearR", "Use R").SetValue(false));

            clearMenu.AddItem(
                new MenuItem("Lane Clear", "Lane Clear").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Menu drawMenu = Menu.AddSubMenu(new Menu("Drawings", "Drawings"));

            drawMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawW", "Draw W").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawR", "Draw R").SetValue(true));

            Menu.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;

            Game.OnUpdate += Game_OnUpdate;

            Notifications.AddNotification("Akali Loaded!", 10000);
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                comboQ();
                comboR();
                comboE();
                comboR2();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                harassQ();
                harassR();
                harassE();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                clearQ();
                clearE();
                clearR();
            }
        }

        //Combo

        private static void comboQ()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (Q.IsReady() && target.IsValidTarget(Q.Range) && Menu.Item("comboQ").GetValue<bool>())
                Q.Cast(target);
        }

        private static void comboR()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if (target.HasBuff("AkaliMota") && Menu.Item("comboR").GetValue<bool>())
            {
                R.Cast(target);
            }
        }

        private static void comboE()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (E.IsReady() && target.IsValidTarget(E.Range) && Menu.Item("comboE").GetValue<bool>())
                E.Cast();
        }

        private static void comboR2()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if (R.IsReady() && target.IsValidTarget(R.Range) && Menu.Item("comboR").GetValue<bool>())
                R.Cast(target);
        }

        //Harass

        private static void harassQ()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (Q.IsReady() && target.IsValidTarget(Q.Range) && Menu.Item("harassQ").GetValue<bool>())
                Q.Cast(target);
        }

        private static void harassR()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if (target.HasBuff("AkaliMota") && Menu.Item("harassR").GetValue<bool>())
            {
                R.Cast(target);
            }
        }

        private static void harassE()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (E.IsReady() && target.IsValidTarget(E.Range) && Menu.Item("harassE").GetValue<bool>())
                E.Cast();
        }

        //Lane Clear

        private static void clearQ()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, Q.Range).FirstOrDefault();
            if (minion == null || minion.Name.ToLower().Contains("ward"))
            {
                return;
            }

            var farmLocation =
                MinionManager.ReferenceEquals(
                    MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy)
                        .Select(m => m.ServerPosition.To2D())
                        .ToList(),
                    Q.Range);

            if (Menu.Item("clearQ").GetValue<bool>() && minion.IsValidTarget() && Q.IsReady())
            {
                Q.Cast(minion);
            }
        }

        private static void clearE()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, E.Range).FirstOrDefault();
            if (minion == null || minion.Name.ToLower().Contains("ward"))
            {
                return;
            }

            var farmLocation =
                MinionManager.ReferenceEquals(
                    MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy)
                        .Select(m => m.ServerPosition.To2D())
                        .ToList(),
                    E.Range);

            if (Menu.Item("clearE").GetValue<bool>() && minion.IsValidTarget() && E.IsReady())
            {
                E.Cast();
            }
        }

        private static void clearR()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, R.Range).FirstOrDefault();
            if (minion == null || minion.Name.ToLower().Contains("ward"))
            {
                return;
            }

            var farmLocation =
                MinionManager.ReferenceEquals(
                    MinionManager.GetMinions(R.Range, MinionTypes.All, MinionTeam.Enemy)
                        .Select(m => m.ServerPosition.To2D())
                        .ToList(),
                    R.Range);

            if (Menu.Item("clearR").GetValue<bool>() && minion.IsValidTarget() && R.IsReady())
            {
                R.Cast(minion);
            }
        }

        //Drawings

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Menu.Item("drawQ").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.Aqua);
            }

            if (Menu.Item("drawW").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.Aqua);
            }

            if (Menu.Item("drawE").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.Aqua);
            }

            if (Menu.Item("drawR").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Aqua);
            }
        }

    }
}
