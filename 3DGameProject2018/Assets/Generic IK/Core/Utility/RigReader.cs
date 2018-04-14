﻿using UnityEngine;

namespace Generics.Dynamics
{
    /// <summary>
    /// Detects the Rig
    /// </summary>
    public class RigReader
    {
        public enum RigType { Generic, Humanoid }

        public Animator animator;
        public RigType rigType { get; private set; }

        public bool initiated { get; private set; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rig">The main animator component</param>
        public RigReader(Animator rig)
        {
            //if(rig == null)
            //{
            //    Debug.LogError("The assigned Animator component is null. The system will consider the rig Generic");
            //}

            animator = rig;
            rigType = rig && rig.isHuman ? RigType.Humanoid : RigType.Generic;

            if (rigType == RigType.Generic) initiated = true;
            else
            {
                ReadHumanoidRig();
                initiated = true;
            }
        }


        /// <summary>
        /// the right side of the lowerbody limbs
        /// </summary>
        public struct LowerbodyRight
        {
            public Core.Joint upperLeg;
            public Core.Joint lowerLeg;
            public Core.Joint foot;
        }
        public LowerbodyRight r_lowerbody;

        /// <summary>
        /// the left side of the lowerbody limbs
        /// </summary>
        public struct LowerbodyLeft
        {
            public Core.Joint upperLeg;
            public Core.Joint lowerLeg;
            public Core.Joint foot;
        }
        public LowerbodyLeft l_lowerbody;

        /// <summary>
        /// the right side of the upperbody limbs
        /// </summary>
        public struct UpperbodyRight
        {
            public Core.Joint shoulder;
            public Core.Joint upperArm;
            public Core.Joint lowerArm;
            public Core.Joint hand;
        }
        public UpperbodyRight r_upperbody;

        /// <summary>
        /// the left side of the upperbody limbs
        /// </summary>
        public struct UpperbodyLeft
        {
            public Core.Joint shoulder;
            public Core.Joint upperArm;
            public Core.Joint lowerArm;
            public Core.Joint hand;
        }
        public UpperbodyLeft l_upperbody;

        /// <summary>
        /// the spine of the character, a typical humanoid has 3 spine components
        /// </summary>
        public struct Spine
        {
            public Core.Joint spine;
            public Core.Joint chest;
        }
        public Spine spine;

        public Core.Joint h_root { get; private set; }
        public Core.Joint h_head { get; private set; }

        /// <summary>
        /// Assign the transforms from the animator's avatar;
        /// </summary>
        private void ReadHumanoidRig()
        {
            h_root = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.Hips) };
            h_head = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.Head) };

            spine.spine = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.Spine) };
            spine.chest = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.Chest) };

            r_upperbody.shoulder = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.RightShoulder) };
            r_upperbody.upperArm = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.RightUpperArm) };
            r_upperbody.lowerArm = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.RightLowerArm) };
            r_upperbody.hand = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.RightHand) };

            l_upperbody.shoulder = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.LeftShoulder) };
            l_upperbody.upperArm = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm) };
            l_upperbody.lowerArm = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm) };
            l_upperbody.hand = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.LeftHand) };

            r_lowerbody.upperLeg = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg) };
            r_lowerbody.lowerLeg = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg) };
            r_lowerbody.foot = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.RightFoot) };

            l_lowerbody.upperLeg = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg) };
            l_lowerbody.lowerLeg = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg) };
            l_lowerbody.foot = new Core.Joint { joint = animator.GetBoneTransform(HumanBodyBones.LeftFoot) };

        }

        /// <summary>
        /// Build the right arm IK chain
        /// </summary>
        /// <returns></returns>
        public Core.Chain RightArmChain()
        {
            if (!IsReady()) return null;
            if (rigType != RigType.Humanoid) return null;
            Core.Chain chain = new Core.Chain();

            chain.joints.Add(r_upperbody.shoulder);
            chain.joints.Add(r_upperbody.upperArm);
            chain.joints.Add(r_upperbody.lowerArm);
            chain.joints.Add(r_upperbody.hand);

            chain.InitiateJoints();

            return chain;
        }

        /// <summary>
        /// Build the left arm IK chain
        /// </summary>
        /// <returns></returns>
        public Core.Chain LeftArmChain()
        {
            if (!IsReady()) return null;
            if (rigType != RigType.Humanoid) return null;
            Core.Chain chain = new Core.Chain();

            chain.joints.Add(l_upperbody.shoulder);
            chain.joints.Add(l_upperbody.upperArm);
            chain.joints.Add(l_upperbody.lowerArm);
            chain.joints.Add(l_upperbody.hand);

            chain.InitiateJoints();

            return chain;
        }

        /// <summary>
        /// Build the right leg chain
        /// </summary>
        /// <returns></returns>
        public Core.Chain RightLegChain()
        {
            if (!IsReady()) return null;
            if (rigType != RigType.Humanoid) return null;
            Core.Chain chain = new Core.Chain();

            chain.joints.Add(r_lowerbody.upperLeg);
            chain.joints.Add(r_lowerbody.lowerLeg);
            chain.joints.Add(r_lowerbody.foot);

            chain.InitiateJoints();

            return chain;
        }

        /// <summary>
        /// Build the right leg chain
        /// </summary>
        /// <returns></returns>
        public Core.Chain LeftLegChain()
        {
            if (!IsReady()) return null;
            if (rigType != RigType.Humanoid) return null;
            Core.Chain chain = new Core.Chain();

            chain.joints.Add(l_lowerbody.upperLeg);
            chain.joints.Add(l_lowerbody.lowerLeg);
            chain.joints.Add(l_lowerbody.foot);

            chain.InitiateJoints();

            return chain;
        }

        /// <summary>
        /// Check if the system has been initiated
        /// </summary>
        /// <returns></returns>
        private bool IsReady()
        {
            if (!initiated)
            {
                Debug.LogWarning("Please initiate the Rig Reader first by calling the Constructor");
            }

            return initiated;
        }
    }
}
