using System;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
using System.Linq;

namespace Zilean
{
    class Program
    {

        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private static Orbwalking.Orbwalker Orbwalker;

        private static Spell Q, W, E, R;

        private static Menu Menu;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Zilean")
                return;

            //Spell Stuff

            Q = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 900);

            Q.SetSkillshot(0.30f, 210f, 2000f, false, SkillshotType.SkillshotCircle);

            //Menu Stuff

            Menu = new Menu(Player.ChampionName, Player.ChampionName, true);

            Menu orbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));

            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            Menu ts = Menu.AddSubMenu(new Menu("Target Selector", "Target Selector"));

            TargetSelector.AddToMenu(ts);

            Menu comboMenu = Menu.AddSubMenu(new Menu("Combo", "Combo"));

            comboMenu.AddItem(new MenuItem("comboQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboW", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboE", "Use E").SetValue(true));

            comboMenu.AddItem(new MenuItem("Combo", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));

            Menu harassMenu = Menu.AddSubMenu(new Menu("Harass", "Harass"));

            harassMenu.AddItem(new MenuItem("harassQ", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("harassW", "Use W").SetValue(true));
            harassMenu.AddItem(new MenuItem("harassE", "Use E").SetValue(false));

            harassMenu.AddItem(
                new MenuItem("Harass", "Harass").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Menu laneclearMenu = Menu.AddSubMenu(new Menu("Lane Clear", "Lane Clear"));

            laneclearMenu.AddItem(new MenuItem("laneclearQ", "Use Q").SetValue(true));
            laneclearMenu.AddItem(new MenuItem("laneclearW", "Use W").SetValue(true));

            laneclearMenu.AddItem(
                new MenuItem("Lane Clear", "Lane Clear").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Menu fleeMenu = Menu.AddSubMenu(new Menu("Flee", "Flee"));

            fleeMenu.AddItem(
                new MenuItem("Flee", "Flee").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            Menu miscMenu = Menu.AddSubMenu(new Menu("RSettings", "R Settings"));

            miscMenu.AddItem(new MenuItem("autoRme", "Auto R Yourself").SetValue(true));
            miscMenu.AddItem(new MenuItem("RmeHP", "Ult Self %")).SetValue(new Slider(20, 1, 100));
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && !hero.IsMe))
                miscMenu.AddItem(new MenuItem("autoRally" + hero.CharData, Player.ChampionName).SetValue(true));
            miscMenu.AddItem(new MenuItem("RallyHP", "Ult Ally %")).SetValue(new Slider(20, 1, 100));

            Menu drawMenu = Menu.AddSubMenu(new Menu("Drawings", "Drawings"));

            drawMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawR", "Draw R").SetValue(true));

            Menu.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;

            Game.OnUpdate += Game_OnUpdate;

            Notifications.AddNotification("Zilean Loaded!", 10000);

        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                comboE();
                comboQ();
                comboW();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                harassE();
                harassQ();
                harassW();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                laneclearQ();
                laneclearW();
            }

            if (Menu.Item("Flee").GetValue<KeyBind>().Active)
            {
                Flee();
            }

            if (Menu.Item("autoRme").GetValue<bool>())
            {
                autoRme();
            }

            if (Menu.Item("autoRally").GetValue<bool>())
            {
                autoRally();
            }
        }

        //Combo

        private static void comboE()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (E.IsReady() && target.IsValidTarget(E.Range) && Menu.Item("comboE").GetValue<bool>())
                E.Cast(target);
        }

        private static void comboQ()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (Q.IsReady() && target.IsValidTarget(Q.Range) && Menu.Item("comboQ").GetValue<bool>())
                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh);
        }

        private static void comboW()
        {

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target.HasBuff("ZileanQEnemyBomb") && Menu.Item("comboW").GetValue<bool>())
            {
                W.Cast();
            }
        }

        //Harass

        private static void harassE()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (E.IsReady() && target.IsValidTarget(E.Range) && Menu.Item("harassE").GetValue<bool>())
                E.Cast(target);
        }

        private static void harassQ()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (Q.IsReady() && target.IsValidTarget(Q.Range) && Menu.Item("harassQ").GetValue<bool>())
                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh);
        }

        private static void harassW()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target.HasBuff("ZileanQEnemyBomb") && Menu.Item("harassW").GetValue<bool>())
            {
                W.Cast();
            }
            else
            {
                return;
            }
        }

        //Lane Clear

        private static void laneclearQ()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, Q.Range).FirstOrDefault();
            if (minion == null || minion.Name.ToLower().Contains("ward"))
            {
                return;
            }

            var farmLocation =
                MinionManager.GetBestCircularFarmLocation(
                    MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy)
                        .Select(m => m.ServerPosition.To2D())
                        .ToList(),
                    Q.Width,
                    Q.Range);

            if (Menu.Item("laneclearQ").GetValue<bool>() && minion.IsValidTarget() && Q.IsReady())
            {
                Q.Cast(farmLocation.Position);
            }
        }

        private static void laneclearW()
        {

            var minion = MinionManager.GetMinions(Player.ServerPosition, Q.Range).FirstOrDefault();
            if (minion == null || minion.Name.ToLower().Contains("ward"))
            {
                return;
            }

            if (Menu.Item("laneclearW").GetValue<bool>() && minion.IsValidTarget() && W.IsReady())
            {
                W.Cast(Player);
            }
        }

        //Flee

        private static void Flee()
        {

            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (E.IsReady())
            {
                E.Cast(Player);
            }

            if (W.IsReady())
            {
                W.Cast();
            }
        }

        //Ultimate

        private static void autoRme()
        {

            if (Player.IsRecalling() || Player.InFountain())
                return;

            var autoRmeHP = Menu.Item("RmeHP").GetValue<Slider>().Value;
            var autoRme = Menu.Item("autoRme").GetValue<bool>();

            if (autoRme && (Player.Health / Player.MaxHealth) * 100 <= autoRmeHP && R.IsReady()
                && Player.CountEnemiesInRange(600) > 0)
            {
                R.Cast(Player);
            }
        }

        private static void autoRally()
        {
            var autoRally = Menu.Item("autoRally").GetValue<bool>();
            var RallyHP = Menu.Item("RallyHP").GetValue<Slider>().Value;

            foreach (var Ally in ObjectManager.Get<Obj_AI_Hero>().Where(Ally => Ally.IsAlly && !Ally.IsMe))
            {
                var allys = Menu.Item("autoRally" + Ally.CharData);

                if (Player.InFountain() || Player.IsRecalling())
                    return;

                if (autoRally && ((Ally.Health / Ally.MaxHealth) * 100 <= RallyHP) && R.IsReady() &&
                    Player.CountEnemiesInRange(900) > 0 && (Ally.Distance(Player.Position) <= R.Range))
                {
                    if (allys != null && allys.GetValue<bool>())
                    {
                        R.Cast(Ally);
                    }
                }
            }
        }

        //Drawing

        private static void Drawing_OnDraw(EventArgs args)
        {
            {
                if (Player.IsDead)
                    return;

                if (Menu.Item("drawQ").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Color.Aqua);
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
}
