using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ImmortalSerials.Model;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ImmortalSerials
{
    public static class Helper
    {
        private const string Path = @"C:\ImmortalSerialLog.txt";
        public static void WriterLine(string s)
        {
            if (!File.Exists(Path))
            {
                File.CreateText(Path);
            }
            string[] content = File.ReadAllLines(Path);
            if (content.All(s1 => s1 != s))
            {
                using (StreamWriter streamWriter = File.AppendText(Path))
                {
                    streamWriter.WriteLine(s);
                }
            }
        }
        public static void WriterFile(string s)
        {
            if (!File.Exists(Path))
            {
                File.CreateText(Path);
            }
            using (var streamWriter = new StreamWriter(Path))
            {
                streamWriter.WriteLine(s);
            }
        }
        /// <summary>
        /// Lấy 360 điểm xung quanh center với khoảng cách range.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static List<Vector3> GetPosCircle(Vector3 center,int range)
        {
            var result=new List<Vector3>();
            for (int i = 0; i < 360; i += 1)
            {
                result.Add(new Vector3((float)(center.X + range * Math.Cos(i * Math.PI / 180)), (float)(center.Y + range * Math.Sin(i * Math.PI / 180)), ObjectManager.Player.Position.Z));
            }
            return result;
        }
        /// <summary>
        /// Lấy 360 điểm xung quanh center với khoảng cách range với điều kiện không phải dưới trụ địch hoặc tường.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static List<Vector3> GetSafePosCircle(this Vector3 center, int range)
        {
            var result = new List<Vector3>();
            for (int i = 0; i < 360; i += 1)
            {
                var v3 = new Vector3((float) (center.X + range * Math.Cos(i * Math.PI / 180)),(float) (center.Y + range * Math.Sin(i * Math.PI / 180)), ObjectManager.Player.Position.Z);
                if (!v3.IsWall() && v3.IsInTurret())
                {
                    result.Add(v3);
                }
            }
            return result;
        }
        /// <summary>
        /// Lấy 360 điểm xung quanh center với khoảng cách range với điều kiện không phải dưới trụ địch hoặc tường.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static List<Vector3> GetSafePosCircle(this Obj_AI_Base center, int range)
        {
            var result = new List<Vector3>();
            for (int i = 0; i < 360; i += 1)
            {
                var v3 = new Vector3((float)(center.Position.X + range * Math.Cos(i * Math.PI / 180)), (float)(center.Position.Y + range * Math.Sin(i * Math.PI / 180)), ObjectManager.Player.Position.Z);
                if (!v3.IsWall() && v3.IsInTurret())
                {
                    result.Add(v3);
                }
            }
            return result;
        }
        public static Destination ShortestPath(this Obj_AI_Base obj, List<Vector3> vList)
        {
            Vector3 result = obj.ServerPosition;
            bool first = true;
            foreach (Vector3 vEnd in vList)
            {
                if (first)
                {
                    first = false;
                    result = vEnd;
                    continue;
                }
                result = obj.Distance(vEnd) < obj.Distance(result) ? vEnd : result;
            }
            return new Destination(result, ChampionData.Player.Distance(result));
        }
        public static Destination LongestPath(this Obj_AI_Base obj, List<Vector3> vList)
        {
            Vector3 result = obj.ServerPosition;
            bool first = true;
            foreach (Vector3 vEnd in vList)
            {
                if (first)
                {
                    first = false;
                    result = vEnd;
                    continue;
                }
                result = obj.Distance(vEnd) > obj.Distance(result) ? vEnd : result;
            }
            return new Destination(result, ChampionData.Player.Distance(result));
        }
        //public void GetMinion()
        //{
        //    var minions=ObjectManager.Get<Obj_AI_Base>()
        //        .Where(
        //            minion =>
        //                minion.IsVisible && !minion.IsDead && !minion.IsMe &&
        //                minion.Distance(tur) < (TurretData.CastRange + 350) && !minion.IsValid<Obj_AI_Turret>());
        //}
    }
}
