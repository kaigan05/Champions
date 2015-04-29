using System;
using System.Collections.Generic;
using System.Linq;
using ImmortalSerials.Objects;
using LeagueSharp;
using LeagueSharp.Common;

namespace ImmortalSerials.Model
{
    public static class SpellDb
    {
        public static MySpell Q, W, E, R;
        public static MySpell Flash, Ignite;
        public static List<MySpell> PlayerSpells = new List<MySpell>();
        public static readonly List<MySpell> SpellList = new List<MySpell>();

        static SpellDb()
        {
            //Katarina
            SpellList.AddRange(
                new List<MySpell>
                {
                    new MySpell(SpellSlot.Q, 675) { ChampionName = "Katarina" },
                    new MySpell(SpellSlot.W, 375) { ChampionName = "Katarina",CastType = CastType.Self},
                    new MySpell(SpellSlot.E, 700)
                    {
                        ChampionName = "Katarina",
                        TargetTypes = new[] { TargetType.Ally, TargetType.Enemy }
                    },
                    new MySpell(SpellSlot.R, 550) { ChampionName = "Katarina",CastType = CastType.Self }
                });
            //LeeSin
            SpellList.AddRange(
                 new List<MySpell>
                {
                    new MySpell(SpellSlot.Q, 1100)
                    {
                        ChampionName = "LeeSin",
                        CastType = CastType.Skillshot,
                        Delay = 250,
                        Width = 65,
                        Speed = 1800,
                        Collision = true,
                        TargetTypes = new[]{TargetType.Enemy},
                        Type = SkillshotType.SkillshotLine,
                    },
                    new MySpell(SpellSlot.W, 700)
                    {
                        ChampionName = "LeeSin",CastType = CastType.Targeted,TargetTypes = new[] { TargetType.Ally}
                    },
                     new MySpell(SpellSlot.E, 350)
                     {
                         ChampionName = "LeeSin",CastType = CastType.Self,
                     },
                    new MySpell(SpellSlot.R, 375)
                    {
                        ChampionName = "LeeSin",CastType = CastType.Targeted,
                    }
                });
            foreach (var spell in SpellList.Where(spell => spell.ChampionName == ChampionData.Player.ChampionName))
            {
                switch (spell.Slot)
                {
                    case SpellSlot.Q:
                        PlayerSpells.Add(Q = spell);
                        break;
                    case SpellSlot.W:
                        PlayerSpells.Add(W = spell);
                        break;
                    case SpellSlot.E:
                        PlayerSpells.Add(E = spell);
                        break;
                    case SpellSlot.R:
                        PlayerSpells.Add(R = spell);
                        break;
                }
            }

            var flash = ObjectManager.Player.GetSpellSlot("summonerflash");
            if (flash != SpellSlot.Unknown)
            {
                Flash = new MySpell(flash, 400);
            }
            var ignite = ObjectManager.Player.GetSpellSlot("summonerdot");
            if (ignite != SpellSlot.Unknown)
            {
                Ignite = new MySpell(ignite, 600);
            }
        }
        //public static Dictionary<SpellSlot, MySpell> GetSpell()
        //{
        //    return SpellList.Where(spell => spell.ChampionName == ObjectManager.Player.ChampionName).ToDictionary(spell => spell.Slot);
        //}
    }
}
