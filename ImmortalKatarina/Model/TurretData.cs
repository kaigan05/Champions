using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ImmortalSerials.Model
{
    public enum TurretTeam
    {
        Enemy,Ally,All
    }
    public static class TurretData
    {
        /// <summary>
        /// Tầm đánh của trụ
        /// </summary>
        public const int CastRange = 904;
        /// <summary>
        /// Tầm an toàn ngoài trụ
        /// </summary>
        public const int SafeDist = CastRange+2;
        /// <summary>
        /// Danh sách các trụ
        /// </summary>
        private static readonly List<Obj_AI_Turret> Turrets = new List<Obj_AI_Turret>();
        
        static TurretData()
        {
            foreach (var turret in ObjectManager.Get<Obj_AI_Turret>())
            {
                Turrets.Add(turret);
            }
            GameObject.OnDelete += GameObject_OnDelete;
        }
        ///// <summary>
        ///// Lấy danh sách trụ thuộc đội của param team trong phạm vi range đối với người chơi
        ///// </summary>
        ///// <param name="team"></param>
        ///// <param name="range"></param>
        ///// <returns></returns>
        //public static IEnumerable<Obj_AI_Turret> GetTurrets(float range,TurretTeam team)
        //{
        //    switch (team)
        //    {
        //        case TurretTeam.Enemy:
        //            return Turrets.Where(turret => turret.IsEnemy && ChampionData.Player.Distance(turret) <= range);
        //        case TurretTeam.Ally:
        //            return Turrets.Where(turret => turret.IsAlly && ChampionData.Player.Distance(turret) <= range);
        //        default:
        //            return Turrets.Where(turret => ChampionData.Player.Distance(turret) <= range);
        //    }
        //}

        /// <summary>
        /// Lấy obj trụ mà obj nằm trong.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="team"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static Obj_AI_Turret GetTurret(this Obj_AI_Base obj, int range = CastRange, TurretTeam team = TurretTeam.Enemy)
        {
            return GetTurret(obj.ServerPosition, team);
        }

        /// <summary>
        /// Lấy obj trụ mà pos nằm trong.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="team"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static Obj_AI_Turret GetTurret(this Vector3 pos, TurretTeam team = TurretTeam.Enemy)
        {
            switch (team)
            {
                case TurretTeam.Enemy:
                    return Turrets.FirstOrDefault(turret => turret.IsEnemy && pos.Distance(turret.Position) <= CastRange);
                case TurretTeam.Ally:
                    return Turrets.FirstOrDefault(turret => turret.IsAlly && pos.Distance(turret.Position) <= CastRange);
                default:
                    return Turrets.FirstOrDefault(turret => pos.Distance(turret.Position) <= CastRange);
            }
        }

        /// <summary>
        /// Kiểm tra pos có nằm trong trụ nào không?
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="team"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool IsInTurret(this Vector3 pos, TurretTeam team = TurretTeam.Enemy)
        {
            if (GetTurret(pos, team)==null)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Kiểm tra obj có nằm trong trụ nào không?
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="team"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool IsInTurret(this Obj_AI_Base obj, TurretTeam team = TurretTeam.Enemy)
        {
            return IsInTurret(obj.ServerPosition, team);
        }
        /// <summary>
        /// Kiểm tra pos có nằm trong range trụ chỉ định không?
        /// </summary>
        /// <param name="v"></param>
        /// <param name="turret"></param>
        /// <returns></returns>
        public static bool InTurretRange(this Vector3 v, Obj_AI_Turret turret)
        {
            return v.Distance(turret.Position) <= CastRange;
        }

        /// <summary>
        /// Kiểm tra Obj_AI_Base có nằm trong range trụ chỉ định không?
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="turret"></param>
        /// <returns></returns>
        public static bool InTurretRange(this Obj_AI_Base obj, Obj_AI_Turret turret)
        {
            return InTurretRange(obj.ServerPosition,turret);
        }
        static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            var turret = sender as Obj_AI_Turret;
            if (turret != null)
            {
                foreach (var turretRemove in Turrets.Where(turretRemove => turretRemove.Name == turret.Name))
                {
                    Turrets.Remove(turretRemove);
                    break;
                }
            }
        }
    }
}
