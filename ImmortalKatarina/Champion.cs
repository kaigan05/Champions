using System;
using System.Collections.Generic;
using System.Linq;
using ImmortalSerials.Model;
using ImmortalSerials.Objects;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ImmortalSerials
{
    public class Champion
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static bool LastHiting = false;
        public Menu MainMenu;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static int LastMoveT;
        public Champion()
        {
            LoadMenu();
        }
        public void LoadMenu()
        {
            MainMenu = new Menu("Immortal " + Player.ChampionName, "ImmortalChampions", true);
            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            MainMenu.AddSubMenu(targetSelector);
            Orbwalker = new Orbwalking.Orbwalker(MainMenu.AddSubMenu(new Menu("Orbwalking", "Orbwalking")));
        }

        public void CastIgnite(Obj_AI_Hero target)
        {
            if (SpellDb.Ignite.IsReady() && Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health)
            {
                SpellDb.Ignite.CastOnUnit(target);
            }
        }
        public static void MoveTo(Vector3 pos)
        {
            if (Environment.TickCount - LastMoveT < 100) return;
            LastMoveT = Environment.TickCount;
            Player.IssueOrder(GameObjectOrder.MoveTo, pos);
        }
    }
}
