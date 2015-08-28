using System;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Heimerwalker
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
            if (Player.ChampionName != "Heimerdinger")
                return;

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1100);
            E = new Spell(SpellSlot.E, 925);
            R = new Spell(SpellSlot.R);

            Menu = new Menu(Player.ChampionName, Player.ChampionName, true);

            Menu orbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));

            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            Menu ts = Menu.AddSubMenu(new Menu("Target Selector", "Target Selector"));

            TargetSelector.AddToMenu(ts);

            Menu drawMenu = Menu.AddSubMenu(new Menu("Drawings", "Drawings"));

            drawMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawW", "Draw W").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));

            Menu.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;

            Notifications.AddNotification("Heimerwalker Loaded!", 10000);

        }

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
        }
    }
}
