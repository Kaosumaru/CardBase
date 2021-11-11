using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UI.Game.Common
{
    public class MovementUtils
    {
        public static float GoToTarget(float current, float target, float delta, out bool done)
        {
            done = false;
            if (current < target)
            {
                current += delta;
                if (current >= target)
                {
                    done = true;
                    current = target;
                }
                return current;
            }
            else if (current > target)
            {
                current -= delta;
                if (current <= target)
                {
                    done = true;
                    current = target;
                }
                return current;
            }
            else
            {
                done = true;
            }
            return current;
        }


       static public Vector3 ChangeVectorToTarget(Vector3 initial, Vector3 target, float speed, out bool done)
        {
            var delta = speed * Time.deltaTime;

            var dir = target - initial;
            var t = delta / dir.magnitude;
            var v = Vector3.Lerp(initial, target, t);
            done = v == target;

            return v;
        }

        static public Vector3 ChangeVectorToTargetSlerp(Vector3 initial, Vector3 target, float speed, out bool done)
        {
            var delta = speed * Time.deltaTime;

            var dir = target - initial;
            var t = delta / dir.magnitude;
            var v = Vector3.Slerp(initial, target, t);
            done = v == target;

            return v;
        }

    }
}
