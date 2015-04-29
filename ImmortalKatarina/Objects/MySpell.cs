using System;
using System.Collections.Generic;
using System.Linq;
using ImmortalSerials.Model;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ImmortalSerials.Objects
{
    public enum CastType
    {
        Skillshot,
        Targeted,
        Self,
    }

    public enum TargetType
    {
        //None,
        Ally,
        Enemy,
        //AllyWard,
        //EnemyWard,
        //All,
    }
    public enum EvadeType
    {
        None,
        Shield,
        MoveBuff,
        Dash,
        Blink,
        Invulnerability
    }
    public class MySpell : LeagueSharp.Common.Spell
    {
        public string Name { get; set; }
        public MySpell(SpellSlot slot, float range): base(slot, range)
        {
            if (slot == SpellSlot.Summoner1 || slot == SpellSlot.Summoner2)
            {
                Name = Champion.Player.GetSpell(slot).Name;
            }
        }
        public bool SmartCast(Obj_AI_Base unit, bool packetCast = false)
        {
            switch (CastType)
            {
                case CastType.Self:
                    return Cast(packetCast);
                case CastType.Targeted:
                    return CastOnUnit(unit, packetCast);
                default:
                    var pred = GetPrediction(unit);
                    if (IsInRange(pred.UnitPosition))
                    {
                        if (Collision && (pred.CollisionObjects.Where(a => a.IsValidTarget(Range) && a.IsMinion).ToList().Count) == 0)
                        {
                            return Cast(pred.CastPosition, packetCast);
                        }
                        if (!Collision)
                        {
                            return Cast(pred.CastPosition, packetCast);
                        }
                    }
                    return false;
            }
        }

        public new float GetDamage(Obj_AI_Base target, int state = 0)
        {
            if (Name == "summonerdot")
            {
                return 50 + 20 * ObjectManager.Player.Level - (target.HPRegenRate / 5 * 3);
            }
            return base.GetDamage(target, state);
        }

        public bool CanTarget(TargetType target)
        {
            return TargetTypes.Any(targetType => target == targetType);
        }

        public IList<Obj_AI_Base> CanCast(IList<Obj_AI_Base> minions, bool checkRange = false)
        {
            var result = new List<Obj_AI_Base>();
            foreach (var targetType in TargetTypes)
            {
                switch (targetType)
                {
                    case TargetType.Enemy:
                        result.AddRange(minions.Where(m => m.IsEnemy && (!checkRange || IsInRange(m))));
                        break;
                    case TargetType.Ally:
                        result.AddRange(minions.Where(m => m.IsAlly && (!checkRange || IsInRange(m))));
                        break;
                }

            }
            return result;
        }
        public bool CanCast(Obj_AI_Base minions, bool checkRange = false)
        {
            if (checkRange && !IsInRange(minions))
            {
                return false;
            }
            foreach (var targetType in TargetTypes)
            {
                if (targetType == TargetType.Enemy)
                {
                    return minions.IsEnemy;
                }
                if (targetType == (TargetType.Ally))
                {
                    return minions.IsAlly;
                }
            }
            return false;
        }
        //public delegate float MoveBuffSpeed();
        //public MoveBuffSpeed MoveSpeed;
        public CastType CastType = CastType.Targeted;
        public TargetType[] TargetTypes = { TargetType.Enemy };
        private bool _isReadyKs;
        private bool _isReadyHarass;
        private bool _isReadyClear;
        private bool _isReadyFarm;
        public bool IsReadyHarass
        {
            get { return _isReadyHarass && this.IsReady(); }
            set { _isReadyHarass = value; }
        }
        public bool IsReadyKs
        {
            get { return _isReadyKs && this.IsReady(); }
            set { _isReadyKs = value; }
        }
        public bool IsReadyClear
        {
            get { return _isReadyClear && this.IsReady(); }
            set { _isReadyClear = value; }
        }
        public bool IsReadyFarm
        {
            get { return _isReadyFarm && this.IsReady(); }
            set { _isReadyFarm = value; }
        }

        public string ChampionName { get; set; }
    }
}