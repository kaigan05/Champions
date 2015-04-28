using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace ImmortalSerials.Model
{
    static class ObjAiData
    {
        public static IEnumerable<Obj_AI_Base> GetMinionsInRangeObj(Obj_AI_Base obj,int range)
        {
            var result = ObjectManager.Get<Obj_AI_Base>().Where(
                minion => minion.IsVisible &&
                          !minion.IsDead &&
                          !minion.IsInvulnerable &&
                          !minion.IsZombie &&
                          !minion.IsMe &&
                          !minion.IsValid<Obj_AI_Turret>() &&
                          minion.InRange(obj,range));
            return result;
        }

        /// <summary>
        /// Kiểm tra obj1 có nằm trong obj2 với range chỉ định?
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool InRange(this Obj_AI_Base obj1,Obj_AI_Base obj2, int range)
        {
            return obj1.Distance(obj2) <= range;
        }
    }
}
