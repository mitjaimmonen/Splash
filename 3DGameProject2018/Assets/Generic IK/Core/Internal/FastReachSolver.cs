using UnityEngine;

namespace Generics.Dynamics
{
    /// <summary>
    /// Reachs a target as fast as possiable
    /// </summary>
    public static class FastReachSolver
    {

        /// <summary>
        /// Apply IK
        /// </summary>
        /// <param name="chain"></param>
        public static bool Process(Core.Chain chain)
        {
            if (chain.joints.Count <= 0) return false;

            if (!chain.initiated) chain.InitiateJoints();

            chain.MapVirtualJoints();

            for (int i = 0; i < chain.iterations; i++)
            {
                SolveInward(chain);
                SolveOutward(chain);
            }

            MapSolverOutput(chain);

            return true;
        }

        /// <summary>
        /// Find the virtual new solved position of joints in the chain inward
        /// </summary>
        /// <param name="chain"></param>
        public static void SolveInward(Core.Chain chain)
        {
            int c = chain.joints.Count;

            //Use Weight first
            chain.joints[c - 1].pos = Vector3.Lerp(chain.GetVirtualEE(), chain.GetIKtarget(), chain.weight);

            //find the joint on the chain's virtual line
            for (int i = c - 2; i >= 0; i--)
            {
                Vector3 _p = chain.joints[i + 1].pos;   //point 
                Vector3 _d = chain.joints[i].pos - _p;  //direction

                _d.Normalize();
                _d *= Vector3.Distance(chain.joints[i + 1].joint.position, chain.joints[i].joint.position);   //all points in a direction along a length

                chain.joints[i].pos = _p + _d;
            }
        }

        /// <summary>
        /// Find the virtual new solved position of joints in the chain outward
        /// </summary>
        /// <param name="chain"></param>
        public static void SolveOutward(Core.Chain chain)
        {
            chain.joints[0].pos = chain.joints[0].joint.position;

            for (int i = 1; i < chain.joints.Count; i++)
            {
                Vector3 _p = chain.joints[i - 1].pos;   //point
                Vector3 _d = chain.joints[i].pos - _p;  //direction

                _d.Normalize();
                _d *= Vector3.Distance(chain.joints[i - 1].joint.position, chain.joints[i].joint.position);

                chain.joints[i].pos = _p + _d;
            }
        }

        /// <summary>
        /// Map the vitual solver's joints onto the physical ones
        /// </summary>
        /// <param name="chain"></param>
        public static void MapSolverOutput(Core.Chain chain)
        {
            for (int i = 0; i < chain.joints.Count - 1; i++)
            {
                Vector3 _v1 = chain.joints[i + 1].pos - chain.joints[i].pos;
                Vector3 _v2 = GenericMath.TransformVector(chain.joints[i].localAxis, chain.joints[i].rot);

                Quaternion _offset = GenericMath.RotateFromTo(_v1, _v2);
                chain.joints[i].rot = GenericMath.ApplyQuaternion(_offset, chain.joints[i].rot);
                chain.joints[i].ApplyVirtualMap(true, true);
            }
        }
    }
}
