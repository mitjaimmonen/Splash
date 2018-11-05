using UnityEngine;

namespace Generics.Dynamics
{
    /// <summary>
    /// Math functions
    /// </summary>
    public static class GenericMath
    {
        /// <summary>
        /// Apply the rotation through Quaternion Multipication
        /// </summary>
        /// <param name="_qA"></param>
        /// <param name="_qB"></param>
        /// <returns>the final quaternion from _qB applied over _qA</returns>
        public static Quaternion ApplyQuaternion(Quaternion _qA, Quaternion _qB)
        {
            Quaternion qr = Quaternion.identity;
            Vector3 va = new Vector3(_qA.x, _qA.y, _qA.z);
            Vector3 vb = new Vector3(_qB.x, _qB.y, _qB.z);
            qr.w = _qA.w * _qB.w - Vector3.Dot(va, vb);
            Vector3 vr = Vector3.Cross(va, vb) + _qA.w * vb + _qB.w * va;
            qr.x = vr.x;
            qr.y = vr.y;
            qr.z = vr.z;
            return qr;
        }

        /// <summary>
        /// Create a Quaternion from an axis and an angle
        /// </summary>
        /// <param name="_axis"></param>
        /// <param name="_angle"></param>
        /// <returns></returns>
        public static Quaternion QuaternionFromAngleAxis(float _angle, Vector3 _axis)
        {
            Quaternion q = Quaternion.identity;

            _axis.Normalize();
            _angle *= Mathf.Deg2Rad;

            q.x = _axis.x * Mathf.Sin(_angle / 2f);
            q.y = _axis.y * Mathf.Sin(_angle / 2f);
            q.z = _axis.z * Mathf.Sin(_angle / 2f);
            q.w = Mathf.Cos(_angle / 2f);

            return q;
        }

        /// <summary>
        /// Get the angle and the axis that makes up a Quaternion
        /// </summary>
        /// <param name="quaternion"></param>
        /// <param name="_angle"></param>
        /// <param name="_axis"></param>
        /// <returns></returns>
        public static Quaternion QuaternionToAngleAxis(Quaternion quaternion, out float _angle, out Vector3 _axis)
        {
            _angle = 0f;
            _axis = Vector3.zero;

            _angle = 2 * Mathf.Acos(quaternion.w) * Mathf.Rad2Deg;
            _axis.x = quaternion.x / Mathf.Sqrt(1 - Mathf.Pow(quaternion.w, 2f));
            _axis.y = quaternion.y / Mathf.Sqrt(1 - Mathf.Pow(quaternion.w, 2f));
            _axis.z = quaternion.z / Mathf.Sqrt(1 - Mathf.Pow(quaternion.w, 2f));

            return quaternion;
        }

        /// <summary>
        /// Linearly Interpolate 2 Vectors
        /// </summary>
        /// <param name="_from"></param>
        /// <param name="_to"></param>
        /// <param name="_weight"></param>
        /// <returns></returns>
        public static Vector3 Interpolate(Vector3 _from, Vector3 _to, float _weight)
        {
            _weight = Mathf.Clamp(_weight, 0f, 1f);
            Vector3 _local = new Vector3((1 - _weight) * _from.x + _weight * _to.x, (1 - _weight) * _from.y + _weight * _to.y, (1 - _weight) * _from.z + _weight * _to.z);
            return _local;
        }

        /// <summary>
        /// The angle between 2 vectors
        /// </summary>
        /// <param name="_v0"></param>
        /// <param name="_v1"></param>
        /// <returns>the angle between the vectors</returns>
        public static float VectorsAngle(Vector3 _v0, Vector3 _v1)
        {
            _v0.Normalize();
            _v1.Normalize();

            float _dot = Vector3.Dot(_v0, _v1);
            _dot = Mathf.Acos(Mathf.Clamp(_dot, -1f, 1f));

            return _dot * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Returns the angle between 2 rotations
        /// </summary>
        /// <param name="_q1"></param>
        /// <param name="_q2"></param>
        /// <returns></returns>
        public static float QuaternionAngle(Quaternion _q1, Quaternion _q2)
        {
            float dot = Quaternion.Dot(_q1, _q2);
            return 2f * Mathf.Acos(Mathf.Clamp01(dot)) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// the _source vector will rotate to point at the _target vector
        /// </summary>
        /// <param name="_source"></param>
        /// <param name="_target"></param>
        /// <returns></returns>
        public static Quaternion RotateFromTo(Vector3 _source, Vector3 _target)
        {
            _source.Normalize();
            _target.Normalize();

            Quaternion q = QuaternionFromAngleAxis(VectorsAngle(_source, _target), Vector3.Cross(_source, _target).normalized);

            return Quaternion.Inverse(q);
        }

        /// <summary>
        /// Rotate a vector by a quaternion
        /// </summary>
        /// <param name="_v"></param>
        /// <param name="_q"></param>
        /// <returns>the vector's new coordinates after being transformed by a quaternion</returns>
        public static Vector3 TransformVector(Vector3 _v, Quaternion _q)
        {
            Quaternion _qv = new Quaternion(_v.x, _v.y, _v.z, 0f);
            Quaternion _qr = ApplyQuaternion(_q, _qv);
            _qr = ApplyQuaternion(_qr, Quaternion.Inverse(_q));
            return new Vector3(_qr.x, _qr.y, _qr.z);
        }

        /// <summary>
        /// Creates a rotation which will always be perpendicular to the _normal and parallel to the surface
        /// </summary>
        /// <param name="_normal"></param>
        /// <returns></returns>
        public static Quaternion RotationLookAt(Vector3 _normal)
        {
            Quaternion _local = Quaternion.identity;
            _local = Quaternion.LookRotation(-_normal);
            return _local;
        }

        /// <summary>
        /// Get the axis of self to target
        /// coordination system independent
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Vector3 GetLocalAxisToTarget(Transform self, Vector3 target)
        {
            Quaternion identity = Quaternion.Inverse(self.rotation);
            return TransformVector((target - self.position).normalized, identity);
        }

        /// <summary>
        /// check if the obj is inside the boundries of the joint's rotation cone
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool ConeBounded(Core.Joint joint, Vector3 obj)
        {
            Vector3 dir = obj - joint.pos;
            float angle = VectorsAngle(dir, joint.pos + TransformVector(joint.axis, joint.rot));
            return joint.maxAngle >= angle;
        }

        /// <summary>
        /// Get the next close point on the surface of the joint's rotation cone
        /// </summary>
        /// <param name="joint"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Vector3 GetConeNextPoint(Core.Joint joint, Vector3 obj)
        {
            //this algorithm is not as accurate as an optimisation partial-differential problem, but its simple and strightforwrd.
            //accuracy doesnt matter in this case

            if (ConeBounded(joint, obj)) return obj;

            Vector3 jointPos = joint.pos;
            Vector3 dir = obj - jointPos;
            Vector3 axis = TransformVector(joint.axis, joint.rot);

            float currAngle = VectorsAngle(dir, jointPos + axis);
            float d = Mathf.Cos(currAngle * Mathf.Deg2Rad) * dir.magnitude;
            float x = d * (Mathf.Tan(currAngle * Mathf.Deg2Rad) - Mathf.Tan(joint.maxAngle * Mathf.Deg2Rad));

            Vector3 coneAxis = joint.joint.position + (TransformVector(axis * d, joint.rot));
            Vector3 rx = coneAxis - obj;
            float dot = Vector3.Dot(joint.joint.position + (TransformVector(axis, joint.rot)), dir.normalized);

            Vector3 point = (rx.normalized * x + obj) * Mathf.Clamp01(Mathf.Sign(dot)) + jointPos * Mathf.Clamp01(-Mathf.Sign(dot));
            return point;
        }

        /// <summary>
        /// Safely clamp a float
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Clamp(float value, float min, float max)
        {
            if (value.CompareTo(min) < 0) return min;
            else if (value.CompareTo(max) > 0) return max;
            else return value;
        }
    }
}
