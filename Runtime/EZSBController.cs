/* Author: Hazmhox */
/* Modified by Techwoof*/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VAMEZSoftBones
{
    public class EZSBController : MonoBehaviour
    {
        private EZSoftBone EZSBComp;
        private EZSoftBoneMaterial EZSBMat;
        public void Start()
        {
            // Adding a suffix to the EZSB element to be able to control it
            EZSBComp = GetComponent<EZSoftBone>();
            if (EZSBComp != null)
            {
                this.name = this.name + "_EZSBC";
                EZSBMat = EZSBComp.material;
            }
        }
        public void EZSBToggleBones(bool state = false)
        {
            if (EZSBComp != null)
            {
                EZSBComp.enabled = state;
            }
        }
        public void EZSBChangeGravity(Vector3 val)
        {
            EZSBComp.gravity = val;
        }

        public void EZSBChangeDamping(float val)
        {
            if (EZSBMat != null)
            {
                EZSBComp.material.damping = val;
            }
        }

        public void EZSBChangeStifness(float val)
        {
            if (EZSBMat != null)
            {
                EZSBComp.material.stiffness = val;
            }
        }

        public void EZSBChangeResistance(float val)
        {
            if (EZSBMat != null)
            {
                EZSBComp.material.resistance = val;
            }
        }

        public void EZSBChangeSlackness(float val)
        {
            if (EZSBMat != null)
            {
                EZSBComp.material.slackness = val;
            }
        }

        public void EZSBChangeUpdateMode(int val = 0)
        {
            if (EZSBComp != null)
            {
                switch (val)
                {
                    case 1:
                        EZSBComp.deltaTimeMode = EZSoftBone.DeltaTimeMode.UnscaledDeltaTime;
                        break;
                    case 2:
                        EZSBComp.deltaTimeMode = EZSoftBone.DeltaTimeMode.Constant;
                        break;
                    default:
                        EZSBComp.deltaTimeMode = EZSoftBone.DeltaTimeMode.DeltaTime;
                        break;
                }

            }
        }

        public void EZSBChangeConstantDeltatime(float val = 0.03f)
        {
            if (EZSBComp != null)
            {
                EZSBComp.constantDeltaTime = val;
            }
        }

        public void EZSBChangeIterations(int val = 1)
        {
            if (EZSBComp != null)
            {
                EZSBComp.iterations = val;
            }
        }

        //Techwoof: Added new parameters
        public void EZSBChangeCollisionFeedback(float val)
        {
            if (EZSBComp != null)
            {
                EZSBComp.collisionFeedback = val;
            }
        }

        public void EZSBChangeCollisionFeedbackFalloff(float val)
        {
            if (EZSBComp != null)
            {
                EZSBComp.collisionFeedbackFalloff = val;
            }
        }

        public void EZSBChangeCollisionFeedbackDepth(int val)
        {
            if (EZSBComp != null)
            {
                EZSBComp.collisionFeedbackDepth = val;
            }
        }

        public void EZSBChangeBackwardConstraintStrength(float val)
        {
            if (EZSBComp != null)
            {
                EZSBComp.backwardConstraintStrength = val;
            }
        }

        public void EZSBChangeBackwardConstraintPasses(int val)
        {
            if (EZSBComp != null)
            {
                EZSBComp.backwardConstraintPasses = val;
            }
        }

        public void EZSBChangeChildOrientationFollow(float val)
        {
            if (EZSBComp != null)
            {
                EZSBComp.childOrientationFollow = val;
            }
        }

        public void EZSBChangeCollisionChildOrientationFollow(float val)
        {
            if (EZSBComp != null)
            {
                EZSBComp.collisionChildOrientationFollow = val;
            }
        }

        public void EZSBChangeCollisionChildOrientationFalloff(float val)
        {
            if (EZSBComp != null)
            {
                EZSBComp.collisionChildOrientationFalloff = val;
            }
        }

        public void EZSBChangeCollisionChildOrientationDepth(int val)
        {
            if (EZSBComp != null)
            {
                EZSBComp.collisionChildOrientationDepth = val;
            }
        }

        public void EZSBChangeCollideWithNativeColliders(bool state)
        {
            if (EZSBComp != null)
            {
                EZSBComp.collideWithNativeColliders = state;
            }
        }

        public void EZSBChangeNativeColliderBufferSize(int val)
        {
            if (EZSBComp != null)
            {
                EZSBComp.nativeColliderBufferSize = val;
            }
        }
        /*public void EZSBChangeRadius(int val)
        {
            if (EZSBComp != null)
            {
                EZSBComp.radius = val;
            }
        }*/
    }
}
