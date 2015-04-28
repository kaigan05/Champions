using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using SharpDX;

namespace ImmortalSerials
{
    public class Destination
    {
        public Vector3 Position;
        public float Timer;
        public float Distance;
        public Obj_AI_Base ObjAiBase;

        public Destination(Vector3 pos,float distance,float time=0)
        {
            Position = pos;
            Distance = distance;
            Timer = time;
        }
        public Destination(Obj_AI_Base objAiBase, float distance, float time = 0)
        {
            ObjAiBase = objAiBase;
            Position = objAiBase.ServerPosition;
            Distance = distance;
            Timer = time;
        }
    }
}
