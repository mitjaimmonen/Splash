using UnityEngine;

namespace Generics.Dynamics
{
    /// <summary>
    /// A simple iterative solver
    /// </summary>
    public static class CyclicDescendSolver
    {
        /// <summary>
        /// Solve the IK for a chain
        /// </summary>
        /// <param name="chain"></param>
        public static bool Process(Core.Chain chain)
        {
            if (chain.joints.Count <= 0) return false;

            chain.MapVirtualJoints();

            for (int j = 0; j < chain.iterations; j++)
            {
                for (int i = chain.joints.Count - 1; i >= 0; i--)
                {
                    float _weight = chain.weight * chain.joints[i].weight;  //relative weight

                    //direction vectors
                    Vector3 _v0 = chain.GetIKtarget() - chain.joints[i].joint.position;
                    Vector3 _v1 = chain.joints[chain.joints.Count - 1].joint.position - chain.joints[i].joint.position;

                    //Weight
                    Quaternion _sourceRotation = chain.joints[i].joint.rotation;
                    Quaternion _targetRotation = Quaternion.Lerp(Quaternion.identity, GenericMath.RotateFromTo(_v0, _v1), _weight);

                    //Rotation Math
                    chain.joints[i].rot = Quaternion.Lerp(_sourceRotation, GenericMath.ApplyQuaternion(_targetRotation, _sourceRotation), _weight);
                    chain.joints[i].ApplyVirtualMap(false, true);
                    chain.joints[i].ApplyRestrictions();
                }
            }

            return true;
        }
    }
}
