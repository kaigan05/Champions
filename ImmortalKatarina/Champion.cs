using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SpellData = ImmortalSerials.Model.SpellData;

namespace ImmortalSerials
{
    public class Champion
    {
        public Items.Item Zhy;
        public static Orbwalking.Orbwalker Orbwalker;
        public static bool LastHiting = false;
        public Menu MainMenu;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        
        public static int LastMoveT;
        public Champion()
        {
            LoadMenu();
            Zhy = new Items.Item((int)ItemId.Zhonyas_Hourglass);
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
            if (SpellData.Ignite.IsReady() && Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health)
            {
                SpellData.Ignite.SmartCast(target);
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
