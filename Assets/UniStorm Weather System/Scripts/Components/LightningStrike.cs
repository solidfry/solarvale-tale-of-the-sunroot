using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniStorm.Utility
{
    public class LightningStrike : MonoBehaviour
    {
        [HideInInspector]
        public GameObject LightningStrikeFire;
        [HideInInspector]
        public GameObject LightningStrikeEffect;
        [HideInInspector]
        public Vector3 HitPosition;
        [HideInInspector]
        public bool PlayerDetected = false;
        [HideInInspector]
        public int GroundStrikeOdds = 50;
        int RaycastDistance = 75;
        [HideInInspector]
        public bool LightningGenerated = false;
        [HideInInspector]
        public LayerMask DetectionLayerMask;
        [HideInInspector]
        public bool ObjectDetected = false;
        [HideInInspector]
        public string FireTag = "Finish";
        [HideInInspector]
        public List<string> LightningFireTags = new List<string>();
        [HideInInspector]
        public GameObject HitObject;
        [HideInInspector]
        public string PlayerTag = "Player";
        [HideInInspector]
        public int EmeraldAIRagdollForce = 500;
        [HideInInspector]
        public int EmeraldAILightningDamage = 500;
        [HideInInspector]
        public bool EmeraldAIAgentDetected = false;
        [HideInInspector]
        public GameObject HitAgent;
        [HideInInspector]
        public string EmeraldAITag = "Respawn";

        void Start()
        {
            UniStormSystem UniStormSystemTemp = FindObjectOfType<UniStormSystem>();
            UniStormSystemTemp.m_LightningStrikeSystem = GetComponent<LightningStrike>();
            GroundStrikeOdds = UniStormSystemTemp.LightningGroundStrikeOdds;
            LightningStrikeEffect = UniStormSystemTemp.LightningStrikeEffect;
            LightningStrikeFire = UniStormSystemTemp.LightningStrikeFire;
            DetectionLayerMask = UniStormSystemTemp.DetectionLayerMask;
            LightningFireTags = UniStormSystemTemp.LightningFireTags;
            GetComponent<SphereCollider>().radius = UniStormSystemTemp.LightningDetectionDistance;
            PlayerTag = UniStormSystem.Instance.PlayerTag;
            EmeraldAITag = UniStormSystem.Instance.EmeraldAITag;
            EmeraldAIRagdollForce = UniStormSystem.Instance.EmeraldAIRagdollForce;
            EmeraldAILightningDamage = UniStormSystem.Instance.EmeraldAILightningDamage;
            HitPosition = Vector3.zero + new Vector3(0, 1000, 0);
        }

        void OnTriggerEnter(Collider C)
        {
            if (C.gameObject.layer != 2 && C.GetComponent<Terrain>() == null && (DetectionLayerMask.value & 1 << C.gameObject.layer) != 0)
            {
                if (C.tag == PlayerTag)
                {
                    HitPosition = C.transform.position;
                    HitObject = C.gameObject;
                    PlayerDetected = true;
                }
                else if (C.tag != PlayerTag && C.tag != EmeraldAITag && UniStormSystem.Instance.LightningStrikesEmeraldAI == UniStormSystem.EnableFeature.Enabled || 
                    UniStormSystem.Instance.LightningStrikesEmeraldAI == UniStormSystem.EnableFeature.Disabled && C.tag != PlayerTag)
                {
                    ObjectDetected = true;
                    HitPosition = C.transform.position;
                    HitObject = C.gameObject;
                }
                else if (C.tag == EmeraldAITag && UniStormSystem.Instance.LightningStrikesEmeraldAI == UniStormSystem.EnableFeature.Enabled)
                {
#if EMERALD_AI_PRESENT
                    EmeraldAIAgentDetected = true;
                    HitPosition = C.transform.position;
                    HitAgent = C.gameObject;
                    FindObjectOfType<LightningSystem>().EndingPoint.position = HitAgent.transform.position;
#endif
                }
            }
        }

        public void CreateLightningStrike()
        {
            RaycastHit hit;

            int Roll = Random.Range(1, 101);

            if (Roll <= GroundStrikeOdds)
            {
                RaycastDistance = 250;
            }
            else
            {
                RaycastDistance = 0;
            }

            if (!ObjectDetected)
            {
                HitPosition = transform.position;
            }

            if (Physics.Raycast(new Vector3(HitPosition.x, HitPosition.y + 40, HitPosition.z), -transform.up, out hit, RaycastDistance, DetectionLayerMask))
            {
                Vector3 pos = hit.point;
                LightningGenerated = true;
                UniStormSystem.Instance.LightningStruckObject = HitObject;

                if (hit.collider.GetComponent<Terrain>() != null && !ObjectDetected)
                {
                    HitPosition = new Vector3(pos.x, hit.collider.GetComponent<Terrain>().SampleHeight(hit.point) + 0.5f, pos.z);
                }
                else
                {
                    HitPosition = new Vector3(HitPosition.x, pos.y + 0.5f, HitPosition.z);
                }

                if (!PlayerDetected && !EmeraldAIAgentDetected)
                {
                    //If our hit object contains a LightningFireTag, start a fire.
                    if (LightningFireTags.Contains(hit.collider.tag))
                    {
                        GameObject HitEffect = UniStormPool.Spawn(LightningStrikeFire, HitPosition, Quaternion.identity);
                        HitEffect.transform.SetParent(hit.collider.transform);
                    }

                    UniStormPool.Spawn(LightningStrikeEffect, HitPosition, Quaternion.identity);
                    UniStormSystem.Instance.OnLightningStrikeObjectEvent.Invoke();
                }
                else if (PlayerDetected)
                {
                    if (LightningFireTags.Contains(hit.collider.tag))
                    {
                        UniStormPool.Spawn(LightningStrikeFire, HitPosition, Quaternion.identity);
                    }

                    UniStormPool.Spawn(LightningStrikeEffect, HitPosition, Quaternion.identity);
                    UniStormSystem.Instance.OnLightningStrikePlayerEvent.Invoke();
                }
                else if (EmeraldAIAgentDetected)
                {
#if EMERALD_AI_PRESENT
                    if (UniStormSystem.Instance.LightningStrikesEmeraldAI == UniStormSystem.EnableFeature.Enabled)
                    {
                        HitPosition = HitAgent.transform.position;
                        UniStormSystem.Instance.LightningStruckObject = HitAgent;
                        if (HitAgent.GetComponent<EmeraldAI.EmeraldAISystem>() != null)
                        {
                            HitAgent.GetComponent<EmeraldAI.EmeraldAISystem>().Damage(EmeraldAILightningDamage, EmeraldAI.EmeraldAISystem.TargetType.Player, HitAgent.transform, EmeraldAIRagdollForce);
                        }
                    }

                    //If our hit object contains a LightningFireTag, start a fire.
                    if (LightningFireTags.Contains(hit.collider.tag))
                    {
                        GameObject HitEffect = UniStormPool.Spawn(LightningStrikeFire, HitAgent.transform.position+new Vector3(0,-1.7f,0), Quaternion.identity);
                        HitEffect.transform.SetParent(hit.collider.transform);
                    }

                    UniStormPool.Spawn(LightningStrikeEffect, HitAgent.transform.position, Quaternion.identity);
                    UniStormSystem.Instance.OnLightningStrikeObjectEvent.Invoke();
                    EmeraldAIAgentDetected = false;
                    StartCoroutine("ResetDelay");
#endif
                }

                LightningGenerated = false;
                ObjectDetected = false;
                PlayerDetected = false;
                EmeraldAIAgentDetected = false;
                HitObject = null;
                HitAgent = null;
            }
        }

        IEnumerator ResetDelay ()
        {
            yield return new WaitForSeconds(0.1f);
            EmeraldAIAgentDetected = false;
            HitObject = null;
            HitAgent = null;
        }
    }
}