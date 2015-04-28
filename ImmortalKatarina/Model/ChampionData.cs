using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace ImmortalSerials.Model
{
    public enum TargetTeam
    {
        All,
        Ally,
        Enemy,
        Neutral
    }
    public static class ChampionData
    {
        public static Obj_AI_Hero Player = ObjectManager.Player;
        private static readonly List<Obj_AI_Hero> Champions = new List<Obj_AI_Hero>();
        static ChampionData()
        {
            foreach (var champion in ObjectManager.Get<Obj_AI_Hero>().Where(hero => !hero.IsMe))
            {
                Champions.Add(champion);
            }
        }

        public static Obj_AI_Hero GetByChampionName(string championName)
        {
            return Champions.FirstOrDefault(hero => hero.IsEnemy && hero.ChampionName == championName);
        }
        public static IEnumerable<Obj_AI_Hero> CanTarget(float range,TargetTeam team)
        {
            switch (team)
            {
                case TargetTeam.Enemy:
                    return Champions.Where(hero =>hero.IsEnemy && !hero.IsDead && !hero.IsZombie && hero.IsVisible && !hero.IsInvulnerable && Player.Distance(hero)<=range);
                case TargetTeam.Ally:
                    return Champions.Where(hero => hero.IsAlly && !hero.IsDead && !hero.IsZombie && hero.IsVisible && !hero.IsInvulnerable && Player.Distance(hero) <= range);
                case TargetTeam.Neutral:
                    return Champions.Where(hero => !hero.IsAlly && !hero.IsEnemy && !hero.IsZombie && !hero.IsDead && hero.IsVisible && !hero.IsInvulnerable && Player.Distance(hero) <= range);
                default:
                    return Champions.Where(hero => !hero.IsDead && hero.IsVisible && !hero.IsZombie && !hero.IsInvulnerable && Player.Distance(hero) <= range);
            } 
        }
    }
}
