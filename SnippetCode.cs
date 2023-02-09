using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;


namespace SnippetCode
{
    public static class SnippetCode
    {

    }
}

public static class Snippet
{


    public static float NegPow(this float input, float power) => Mathf.Pow(input, power) * (input / Mathf.Abs(input));
    public static float Pow(this float input, float power) => Mathf.Pow(input, power);
    public static float Sqrt(this float input) => Mathf.Sqrt(input);
    public static float Clamp(this float input, float low, float high) => Mathf.Clamp(input, low, high);
    public static float Remap(this float input, float inLow, float inHigh, float outLow, float outHigh)
        => (input - inLow) / (inHigh - inLow) * (outHigh - outLow) + outLow;

    public static float RemapClamp(this float input, float inLow, float inHigh, float outLow, float outHigh)
        => (Mathf.Clamp(input, inLow, inHigh) - inLow) / (inHigh - inLow) * (outHigh - outLow) + outLow;

    public static float Remap01(this float input, float inLow, float inHigh) => (input - inLow) / (inHigh - inLow);

    public static float RemapClamp01(this float input, float inLow, float inHigh)
        => (Mathf.Clamp(input, inLow, inHigh) - inLow) / (inHigh - inLow);

    public static float OneMinus(this float input) => Mathf.Clamp01(1 - input);

    public static float Randomize(this float input, float range) => input * Random.Range(1f - range, 1f + range);

    public static float Curve(this float time, params float[] values)
    {
        var curve = new AnimationCurve();
        int i = 0;
        foreach (var value in values)
        {
            curve.AddKey(i / ((float)values.Length - 1), value);
            i++;
        }

        return curve.Evaluate(time);
    }

    public static float MapOverCurve(this float time, params Tuple<float, float>[] points)
    {
        var curve = new AnimationCurve();
        foreach (var point in points)
        {
            curve.AddKey(new Keyframe(point.Item1, point.Item2));
        }

        return curve.Evaluate(time);
    }

    public static float MapOverCurve(this float time, params Tuple<float, float, float, float>[] points)
    {
        var curve = new AnimationCurve();
        foreach (var point in points)
        {
            curve.AddKey(new Keyframe(point.Item1, point.Item2, point.Item3, point.Item4));
        }

        return curve.Evaluate(time);
    }



    // Original idea from walterellisfun on github: https://github.com/walterellisfun/ConeCast/blob/master/ConeCastExtension.cs
    /// <summary>
    /// Like SphereCastAll but in a cone
    /// </summary>
    /// <param name="origin">Origin position</param>
    /// <param name="maxRadius">Maximum cone radius</param>
    /// <param name="direction">Cone direction</param>
    /// <param name="maxDistance">Maximum cone distance</param>
    /// <param name="coneAngle">Cone angle</param>
    public static RaycastHit[] ConeCastAll(this Vector3 origin, float maxRadius, Vector3 direction, float maxDistance, float coneAngle, int layer = Physics.DefaultRaycastLayers)
    {
        RaycastHit[] sphereCastHits = Physics.SphereCastAll(origin, maxRadius, direction, maxDistance, layer, QueryTriggerInteraction.Ignore);
        List<RaycastHit> coneCastHitList = new List<RaycastHit>();

        if (sphereCastHits.Length > 0)
        {
            for (int i = 0; i < sphereCastHits.Length; i++)
            {
                Vector3 hitPoint = sphereCastHits[i].point;
                Vector3 directionToHit = hitPoint - origin;
                float angleToHit = Vector3.Angle(direction, directionToHit);
                float multiplier = 1f;
                if (directionToHit.magnitude < 2f)
                    multiplier = 4f;
                bool hitRigidbody = sphereCastHits[i].rigidbody is Rigidbody rb
                                    && Vector3.Angle(direction, rb.transform.position - origin) < coneAngle * multiplier;

                if (angleToHit < coneAngle * multiplier || hitRigidbody)
                {
                    coneCastHitList.Add(sphereCastHits[i]);
                }
            }
        }
        return coneCastHitList.ToArray();
    }

    /// <summary>
    /// Get a component from the gameobject, or create it if it doesn't exist
    /// </summary>
    /// <typeparam name="T">The component type</typeparam>
    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        return obj.GetComponent<T>() ?? obj.AddComponent<T>();
    }

    /// <summary>
    /// Vector pointing away from the palm
    /// </summary>
    public static Vector3 PalmDir(this RagdollHand hand) => -hand.transform.forward;

    /// <summary>
    /// Vector pointing in the direction of the thumb
    /// </summary>
    public static Vector3 ThumbDir(this RagdollHand hand) => (hand.side == Side.Right) ? hand.transform.up : -hand.transform.up;

    /// <summary>
    /// Vector pointing away in the direction of the fingers
    /// </summary>
    public static Vector3 PointDir(this RagdollHand hand) => -hand.transform.right;

    /// <summary>
    /// Get a point above the player's hand
    /// </summary>
    public static Vector3 PosAboveBackOfHand(this RagdollHand hand, float factor = 1f) => hand.transform.position - hand.transform.right * 0.1f * factor + hand.transform.forward * 0.2f * factor;

    public static Quaternion GetFlyDirRefLocalRotation(this Item item) => Quaternion.Inverse(item.transform.rotation) * item.flyDirRef.rotation;

    public static void SetVFXProperty<T>(this EffectInstance effect, string name, T data)
    {
        if (effect == null)
            return;
        if (data is Vector3 v)
        {
            foreach (EffectVfx effectVfx in effect.effects.Where<Effect>(fx => fx is EffectVfx effectVfx17 && effectVfx17.vfx.HasVector3(name)))
                effectVfx.vfx.SetVector3(name, v);
        }
        else if (data is float f2)
        {
            foreach (EffectVfx effectVfx2 in effect.effects.Where<Effect>(fx => fx is EffectVfx effectVfx18 && effectVfx18.vfx.HasFloat(name)))
                effectVfx2.vfx.SetFloat(name, f2);
        }
        else if (data is int i3)
        {
            foreach (EffectVfx effectVfx2 in effect.effects.Where<Effect>(fx => fx is EffectVfx effectVfx19 && effectVfx19.vfx.HasInt(name)))
                effectVfx2.vfx.SetInt(name, i3);
        }
        else if (data is bool b4)
        {
            foreach (EffectVfx effectVfx2 in effect.effects.Where<Effect>(fx => fx is EffectVfx effectVfx20 && effectVfx20.vfx.HasBool(name)))
                effectVfx2.vfx.SetBool(name, b4);
        }
        else
        {
            if (!(data is Texture t5))
                return;
            foreach (EffectVfx effectVfx2 in effect.effects.Where<Effect>(fx => fx is EffectVfx effectVfx21 && effectVfx21.vfx.HasTexture(name)))
                effectVfx2.vfx.SetTexture(name, t5);
        }
    }
    public static object GetVFXProperty(this EffectInstance effect, string name)
    {
        foreach (Effect effect1 in effect.effects)
        {
            if (effect1 is EffectVfx effectVfx1)
            {
                if (effectVfx1.vfx.HasFloat(name))
                    return effectVfx1.vfx.GetFloat(name);
                if (effectVfx1.vfx.HasVector3(name))
                    return effectVfx1.vfx.GetVector3(name);
                if (effectVfx1.vfx.HasBool(name))
                    return effectVfx1.vfx.GetBool(name);
                if (effectVfx1.vfx.HasInt(name))
                    return effectVfx1.vfx.GetInt(name);
            }
        }
        return null;
    }

    public static void HapticTick(this RagdollHand hand, float intensity = 1, float frequency = 10) => PlayerControl.input.Haptic(hand.side, intensity, frequency);
    public static void PlayHapticClipOver(this RagdollHand hand, AnimationCurve curve, float duration)
    {
        hand.StartCoroutine(HapticPlayer(hand, curve, duration));
    }
    public static IEnumerator HapticPlayer(RagdollHand hand, AnimationCurve curve, float duration)
    {
        var time = Time.time;
        while (Time.time - time < duration)
        {
            hand.HapticTick(curve.Evaluate((Time.time - time) / duration));
            yield return 0;
        }
    }


    public static Vector3 zero = Vector3.zero;
    public static Vector3 one = Vector3.one;
    public static Vector3 forward = Vector3.forward;
    public static Vector3 right = Vector3.right;
    public static Vector3 up = Vector3.up;
    public static Vector3 back = Vector3.back;
    public static Vector3 left = Vector3.left;
    public static Vector3 down = Vector3.down;

    public static bool XBigger(this Vector3 vec) => Mathf.Abs(vec.x) > Mathf.Abs(vec.y) && Mathf.Abs(vec.x) > Mathf.Abs(vec.z);

    public static bool YBigger(this Vector3 vec) => Mathf.Abs(vec.y) > Mathf.Abs(vec.x) && Mathf.Abs(vec.y) > Mathf.Abs(vec.z);

    public static bool ZBigger(this Vector3 vec) => Mathf.Abs(vec.z) > Mathf.Abs(vec.x) && Mathf.Abs(vec.z) > Mathf.Abs(vec.y);

    public static Vector3 Velocity(this RagdollHand hand) => Player.local.transform.rotation * hand.playerHand.controlHand.GetHandVelocity();

    /// <summary>
    /// .Select(), but only when the output of the selection function is non-null
    /// </summary>
    public static IEnumerable<TOut> SelectNotNull<TIn, TOut>(this IEnumerable<TIn> enumerable, Func<TIn, TOut> func)
        => enumerable.Where(item => func(item) != null).Select(func);

    public static bool IsPlayer(this RagdollPart part) => part?.ragdoll?.creature.isPlayer == true;
    public static bool IsImportant(this RagdollPart part)
    {
        var type = part.type;
        return type == RagdollPart.Type.Head
               || type == RagdollPart.Type.Torso
               || type == RagdollPart.Type.LeftHand
               || type == RagdollPart.Type.RightHand
               || type == RagdollPart.Type.LeftFoot
               || type == RagdollPart.Type.RightFoot;
    }
    /// <summary>
    /// Get a creature's part from a PartType
    /// </summary>
    public static RagdollPart GetPart(this Creature creature, RagdollPart.Type partType)
        => creature.ragdoll.GetPart(partType);

    /// <summary>
    /// Get a creature's head
    /// </summary>
    public static RagdollPart GetHead(this Creature creature) => creature.ragdoll.headPart;

    /// <summary>
    /// Get a creature's torso
    /// </summary>
    public static RagdollPart GetTorso(this Creature creature) => creature.GetPart(RagdollPart.Type.Torso);

    public static Vector3 GetChest(this Creature creature) => Vector3.Lerp(creature.GetTorso().transform.position,
        creature.GetHead().transform.position, 0.5f);
    public static IEnumerable<Creature> CreaturesInRadius(this Vector3 position, float radius, bool targetAliveCreature = true, bool targetDeadCreature = false, bool targetPlayer = false)
    {
        List<Creature> creatureDetected = new List<Creature>();
        for (int i = Creature.allActive.Count - 1; i >= 0; i--)
        {
            if (((Creature.allActive[i].GetChest() - position).sqrMagnitude < radius * radius)
        && (targetAliveCreature ? true : Creature.allActive[i].state == Creature.State.Dead)
        && (targetDeadCreature ? true : Creature.allActive[i].state != Creature.State.Dead)
        && (targetPlayer ? true : !Creature.allActive[i].isPlayer))
                creatureDetected.Add(Creature.allActive[i]);
        }
        return creatureDetected;
    }

    public static IEnumerable<Creature> CreaturesInConeRadius(this Vector3 position, float radius, Vector3 directionOfCone, float angleOfCone, bool targetAliveCreature = true, bool targetDeadCreature = false, bool targetPlayer = false)
    {
        List<Creature> creatureDetected = new List<Creature>();
        for (int i = Creature.allActive.Count - 1; i >= 0; i--)
        {
            if (((Creature.allActive[i].GetChest() - position).sqrMagnitude < radius * radius)
        && (targetAliveCreature ? true : Creature.allActive[i].state == Creature.State.Dead)
        && (targetDeadCreature ? true : Creature.allActive[i].state != Creature.State.Dead)
        && (targetPlayer ? true : !Creature.allActive[i].isPlayer)
        && (Vector3.Angle(Creature.allActive[i].transform.position - position, directionOfCone) <= (angleOfCone / 2f)))
                creatureDetected.Add(Creature.allActive[i]);
        }
        return creatureDetected.ToList();
    }

    public static Creature RandomCreatureInRadius(this Vector3 position, float radius, bool targetAliveCreature = true, bool targetDeadCreature = false, bool targetPlayer = false, Creature creatureToExclude = null, bool includeCreatureExcludedIfDefault = false)
    {
        List<Creature> creatureDetected = new List<Creature>();

        for (int i = Creature.allActive.Count - 1; i >= 0; i--)
        {
            if ((includeCreatureExcludedIfDefault || !includeCreatureExcludedIfDefault && Creature.allActive[i] != creatureToExclude)
        && ((Creature.allActive[i].GetChest() - position).sqrMagnitude < radius * radius)
        && (targetAliveCreature ? true : Creature.allActive[i].state == Creature.State.Dead)
        && (targetDeadCreature ? true : Creature.allActive[i].state != Creature.State.Dead)
        && (targetPlayer ? true : !Creature.allActive[i].isPlayer))
            {
                creatureDetected.Add(Creature.allActive[i]);
            }
        }

        if (creatureDetected.Count != 0)
        {
            return creatureDetected[Random.Range(0, creatureDetected.Count)];
        }
        else
        {
            return null;
        }
    }

    public static Creature ClosestCreatureInRadius(this Vector3 position, float radius, bool targetAliveCreature = true, bool targetDeadCreature = false, bool targetPlayer = false, Creature creatureToExclude = null)
    {
        List<Creature> creatureDetected = new List<Creature>();
        for (int i = Creature.allActive.Count - 1; i >= 0; i--)
        {
            if (Creature.allActive[i] != creatureToExclude && ((Creature.allActive[i].GetChest() - position).sqrMagnitude < radius * radius)
        && (targetAliveCreature ? true : Creature.allActive[i].state == Creature.State.Dead)
        && (targetDeadCreature ? true : Creature.allActive[i].state != Creature.State.Dead)
        && (targetPlayer ? true : !Creature.allActive[i].isPlayer))
                creatureDetected.Add(Creature.allActive[i]);
        }
        if (creatureDetected != null)
        {
            float lastRadius = Mathf.Infinity;
            float thisRadius;
            Creature lastCreature = null;
            foreach (Creature creature in creatureDetected)
            {
                thisRadius = (position - creature.transform.position).sqrMagnitude;
                if (thisRadius <= lastRadius)
                {
                    lastRadius = thisRadius;
                    lastCreature = creature;
                }
            }
            return lastCreature;
        }
        else
        {
            return null;
        }
    }

    public static Creature ClosestCreatureInListFromPosition(this List<Creature> creatures, Vector3 position)
    {
        float lastRadius = Mathf.Infinity;
        float thisRadius;
        Creature lastCreature = null;
        foreach (Creature creature in creatures)
        {
            thisRadius = (position - creature.transform.position).sqrMagnitude;
            if (thisRadius <= lastRadius)
            {
                lastRadius = thisRadius;
                lastCreature = creature;
            }
        }
        return lastCreature;
    }
    public static Creature FarestCreatureInListFromPosition(this List<Creature> creatures, Vector3 position)
    {
        float lastRadius = 0f;
        float thisRadius;
        Creature lastCreature = null;
        foreach (Creature creature in creatures)
        {
            thisRadius = (position - creature.GetChest()).sqrMagnitude;
            if (thisRadius >= lastRadius)
            {
                lastRadius = thisRadius;
                lastCreature = creature;
            }
        }
        return lastCreature;
    }

    public static void Depenetrate(this Item item)
    {
        foreach (var handler in item.collisionHandlers)
        {
            foreach (var damager in handler.damagers)
            {
                damager.UnPenetrateAll();
            }
        }
    }
    /// <summary>
    /// Get a creature's random part
    /// </summary>
    /// <param name="creature">Creature where the part need to be targeted</param>
    /// <param name="mask">Mask Apply (write it in binary : 0b00011111111111) : 1 means get the part, 0 means don't get the part : in the order of the bit from left to right : 
    /// Tail, RightWing, LeftWing, RightFoot, LeftFoot, RightLeg, LeftLeg, RightHand, LeftHand, RightArm, LeftArm, Torso, Neck, Head</param>
    public static RagdollPart GetRandomRagdollPart(this Creature creature, int mask = 0b00011111111111)
    {
        List<RagdollPart> ragdollParts = new List<RagdollPart>();
        foreach (RagdollPart part in creature.ragdoll.parts)
        {
            if ((mask & (int)part.type) > 0)
                ragdollParts.Add(part);
        }
        return ragdollParts[Random.Range(0, ragdollParts.Count)];

        /*for(int i = creature.ragdoll.parts.Count - 1; i >= 0; i--)
        {
            if (!((mask & (int)creature.ragdoll.parts[i].type) > 0))
                creature.ragdoll.parts.RemoveAt(i);
        }
        return creature.ragdoll.parts[Random.Range(0, creature.ragdoll.parts.Count)];*/
    }
    public static bool returnWaveStarted()
    {
        int nbWaveStarted = 0;
        foreach (WaveSpawner waveSpawner in WaveSpawner.instances)
        {
            if (waveSpawner.isRunning)
            {
                nbWaveStarted++;
            }
        }
        return nbWaveStarted != 0 ? true : false;
    }

    public static Vector3 FromToDirection(this Vector3 from, Vector3 to)
    {
        return to - from;
    }
    /// <summary>
    /// Add a force that attracts when coef is positive and repulse when is negative
    /// </summary>
    public static void Attraction_Repulsion_Force(this Rigidbody rigidbody, Vector3 origin, Vector3 attractedRb, bool useDistance, float coef)
    {
        Vector3 direction = FromToDirection(attractedRb, origin).normalized;
        if (useDistance)
        {
            float distance = FromToDirection(attractedRb, origin).magnitude;
            rigidbody.AddForce(direction * (coef / distance) / (rigidbody.mass / 2), ForceMode.VelocityChange);
        }
        else
        {
            rigidbody.AddForce(direction * coef / (rigidbody.mass / 2), ForceMode.VelocityChange);
        }
    }
    /// <summary>
    /// Add a force that attracts when coef is positive and repulse when is negative
    /// </summary>
    public static void Attraction_Repulsion_ForceNoMass(this Rigidbody rigidbody, Vector3 origin, Vector3 attractedRb, bool useDistance, float coef)
    {
        Vector3 direction = FromToDirection(attractedRb, origin).normalized;
        if (useDistance)
        {
            float distance = FromToDirection(attractedRb, origin).magnitude;
            rigidbody.AddForce(direction * (coef / distance), ForceMode.VelocityChange);
        }
        else
        {
            rigidbody.AddForce(direction * coef, ForceMode.VelocityChange);
        }
    }


    /// <summary>
    /// Return the minimum entry in an interator using a custom comparable function
    /// </summary>
    public static T MinBy<T>(this IEnumerable<T> enumerable, Func<T, IComparable> comparator)
    {
        if (!enumerable.Any())
            return default;
        return enumerable.Aggregate((curMin, x) => (curMin == null || (comparator(x).CompareTo(comparator(curMin)) < 0)) ? x : curMin);
    }

    public static Vector3 RotateCircle(this Vector3 origin, Vector3 forwardDirection, Vector3 upDirection, float radius, float speed, int nbElementsAroundCircle, int i)
    {
        return origin + Quaternion.AngleAxis(i * 360f / nbElementsAroundCircle + speed, forwardDirection) * upDirection * radius;
    }

    public static Vector3 PosAroundCircle(this Vector3 origin, Vector3 forwardDirection, Vector3 upDirection, float radius, int nbElementsAroundCircle, int i)
    {
        return origin + Quaternion.AngleAxis(i * 360f / nbElementsAroundCircle, forwardDirection) * upDirection * radius;
    }

    public static ConfigurableJoint CreateSimpleJoint(Rigidbody source, Rigidbody target, float spring, float damper)
    {
        Quaternion orgRotation = source.transform.rotation;
        source.transform.rotation = target.transform.rotation;
        var joint = source.gameObject.AddComponent<ConfigurableJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.targetRotation = Quaternion.identity;
        joint.anchor = source.centerOfMass;
        joint.connectedAnchor = target.centerOfMass;
        joint.connectedBody = target;
        JointDrive posDrive = new JointDrive
        {
            positionSpring = spring,
            positionDamper = damper,
            maximumForce = Mathf.Infinity
        };
        JointDrive rotDrive = new JointDrive
        {
            positionSpring = 1000,
            positionDamper = 10,
            maximumForce = Mathf.Infinity
        };
        joint.rotationDriveMode = RotationDriveMode.XYAndZ;
        joint.xDrive = posDrive;
        joint.yDrive = posDrive;
        joint.zDrive = posDrive;
        joint.angularXDrive = rotDrive;
        joint.angularYZDrive = rotDrive;
        source.transform.rotation = orgRotation;
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;
        joint.xMotion = ConfigurableJointMotion.Free;
        joint.yMotion = ConfigurableJointMotion.Free;
        joint.zMotion = ConfigurableJointMotion.Free;
        return joint;
    }

    public static ConfigurableJoint CreateSlingshotJoint(Rigidbody source, Rigidbody target, float spring, float damper)
    {
        Quaternion orgRotation = source.transform.rotation;
        //source.transform.rotation = target.transform.rotation;
        ConfigurableJoint joint = source.gameObject.AddComponent<ConfigurableJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.targetRotation = Quaternion.identity;
        //joint.anchor = source.centerOfMass;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = target.centerOfMass;
        joint.connectedBody = target;
        JointDrive posDrive = new JointDrive
        {
            positionSpring = spring,
            positionDamper = damper,
            maximumForce = Mathf.Infinity
        };
        JointDrive emptyDrive = new JointDrive
        {
            positionSpring = 0f,
            positionDamper = 0f,
            maximumForce = Mathf.Infinity
        };
        SoftJointLimit softJointLimit = new SoftJointLimit
        {
            limit = 0.76f,
            bounciness = 0f,
            contactDistance = 0f
        };
        joint.linearLimit = softJointLimit;
        joint.rotationDriveMode = RotationDriveMode.XYAndZ;
        joint.xDrive = emptyDrive;
        joint.yDrive = posDrive;
        joint.zDrive = emptyDrive;
        joint.angularXDrive = emptyDrive;
        joint.angularYZDrive = emptyDrive;
        joint.slerpDrive = emptyDrive;
        source.transform.rotation = orgRotation;
        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.massScale = 15f;
        return joint;
    }

    public static ConfigurableJoint StrongJointFixed(Rigidbody source, Rigidbody target, float massScale = 30f, bool limitMotion = false)
    {
        ConfigurableJoint joint;
        joint = target.gameObject.AddComponent<ConfigurableJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.targetRotation = Quaternion.identity;
        joint.anchor = Vector3.zero;
        joint.connectedBody = source;
        joint.connectedAnchor = Vector3.zero;
        joint.rotationDriveMode = RotationDriveMode.XYAndZ;
        JointDrive posDrive = new JointDrive
        {
            positionSpring = 2000,
            positionDamper = 40,
            maximumForce = 100
        };
        JointDrive rotDrive = new JointDrive
        {
            positionSpring = 1000,
            positionDamper = 40,
            maximumForce = 100
        };
        joint.xDrive = posDrive;
        joint.yDrive = posDrive;
        joint.zDrive = posDrive;
        joint.angularXDrive = rotDrive;
        joint.angularYZDrive = rotDrive;
        joint.massScale = massScale;
        joint.connectedMassScale = 1 / massScale;
        if (limitMotion)
        {
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;
            joint.xMotion = ConfigurableJointMotion.Limited;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Limited;
        }
        else
        {
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;
            joint.xMotion = ConfigurableJointMotion.Free;
            joint.yMotion = ConfigurableJointMotion.Free;
            joint.zMotion = ConfigurableJointMotion.Free;
        }

        return joint;
    }

    public static ConfigurableJoint CreateJointToProjectileForCreatureAttraction(this Item projectile, RagdollPart attractedRagdollPart, ConfigurableJoint joint)
    {
        JointDrive jointDrive = new JointDrive();
        jointDrive.positionSpring = 1f;
        jointDrive.positionDamper = 0.2f;
        SoftJointLimit softJointLimit = new SoftJointLimit();
        softJointLimit.limit = 0.15f;
        SoftJointLimitSpring linearLimitSpring = new SoftJointLimitSpring();
        linearLimitSpring.spring = 1f;
        linearLimitSpring.damper = 0.2f;
        joint = attractedRagdollPart.gameObject.AddComponent<ConfigurableJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.targetRotation = Quaternion.identity;
        joint.anchor = Vector3.zero;
        joint.connectedBody = projectile.GetComponent<Rigidbody>();
        joint.connectedAnchor = Vector3.zero;
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;
        joint.linearLimitSpring = linearLimitSpring;
        joint.linearLimit = softJointLimit;
        joint.angularXLimitSpring = linearLimitSpring;
        joint.xDrive = jointDrive;
        joint.yDrive = jointDrive;
        joint.zDrive = jointDrive;
        joint.massScale = 10000f;
        joint.connectedMassScale = 0.00001f;
        return joint;
    }

    public static FixedJoint JointFixed(this Rigidbody source, Rigidbody target)
    {
        FixedJoint joint = new FixedJoint();
        joint = target.gameObject.AddComponent<FixedJoint>();
        joint.anchor = Vector3.zero;
        joint.connectedBody = source;
        joint.connectedAnchor = Vector3.zero;
        joint.massScale = 0.00001f;
        joint.connectedMassScale = 10000f;
        joint.breakForce = Mathf.Infinity;
        joint.breakTorque = Mathf.Infinity;
        return joint;
    }

    public static SpringJoint YoyoJoint(RagdollHand hand, Rigidbody itemRB, SpringJoint joint, float distance)
    {
        joint = itemRB.GetComponent<Item>().gameObject.AddComponent<SpringJoint>();
        joint.connectedBody = hand.rb;
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = Vector3.zero;
        joint.maxDistance = distance;
        joint.spring = 1000f;
        joint.tolerance = 0.1f;
        return joint;
    }

    public static void DestroyJoint(this Rigidbody rb)
    {
        if (rb.gameObject.GetComponent<ConfigurableJoint>())
        {
            UnityEngine.Object.Destroy(rb.gameObject.GetComponent<ConfigurableJoint>());
        }
        if (rb.gameObject.GetComponent<CharacterJoint>())
        {
            UnityEngine.Object.Destroy(rb.gameObject.GetComponent<CharacterJoint>());
        }
        if (rb.gameObject.GetComponent<SpringJoint>())
        {
            UnityEngine.Object.Destroy(rb.gameObject.GetComponent<SpringJoint>());
        }
        if (rb.gameObject.GetComponent<HingeJoint>())
        {
            UnityEngine.Object.Destroy(rb.gameObject.GetComponent<HingeJoint>());
        }
    }

    public static void IgnoreCollider(this Ragdoll ragdoll, Collider collider, bool ignore = true)
    {
        foreach (RagdollPart part in ragdoll.parts)
        {
            part.IgnoreCollider(collider, ignore);
        }
    }

    public static void IgnoreCollider(this RagdollPart part, Collider collider, bool ignore = true)
    {
        foreach (Collider itemCollider in part.colliderGroup.colliders)
        {
            Physics.IgnoreCollision(collider, itemCollider, ignore);
        }
    }

    public static void IgnoreCollider(this Item item, Collider collider, bool ignore)
    {
        foreach (ColliderGroup cg in item.colliderGroups)
        {
            foreach (var itemCollider in cg.colliders)
            {
                Physics.IgnoreCollision(collider, itemCollider, ignore);
            }
        }
    }

    public static void IgnoreColliderBetweenItem(this Item item, Item itemIgnored, bool ignore = true)
    {
        foreach (ColliderGroup colliderGroup1 in item.colliderGroups)
        {
            foreach (Collider collider1 in colliderGroup1.colliders)
            {
                foreach (ColliderGroup colliderGroup2 in itemIgnored.colliderGroups)
                {
                    foreach (Collider collider2 in colliderGroup2.colliders)
                        Physics.IgnoreCollision(collider1, collider2, ignore);
                }
            }
        }
        if (ignore)
        {
            item.ignoredItem = item;
        }
        else
        {
            item.ignoredItem = null;
        }
    }


    public static void addIgnoreRagdollAndItemHoldingCollision(Item item, Creature creature)
    {
        foreach (ColliderGroup colliderGroup in item.colliderGroups)
        {
            foreach (Collider collider in colliderGroup.colliders)
                creature.ragdoll.IgnoreCollision(collider, true);
        }
        item.ignoredRagdoll = creature.ragdoll;

        if (creature.handLeft.grabbedHandle?.item != null)
        {
            foreach (ColliderGroup colliderGroup1 in item.colliderGroups)
            {
                foreach (Collider collider1 in colliderGroup1.colliders)
                {
                    foreach (ColliderGroup colliderGroup2 in creature.handLeft.grabbedHandle.item.colliderGroups)
                    {
                        foreach (Collider collider2 in colliderGroup2.colliders)
                            Physics.IgnoreCollision(collider1, collider2, true);
                    }
                }
            }
            item.ignoredItem = creature.handLeft.grabbedHandle.item;
        }

        if (creature.handRight.grabbedHandle?.item != null)
        {
            foreach (ColliderGroup colliderGroup1 in item.colliderGroups)
            {
                foreach (Collider collider1 in colliderGroup1.colliders)
                {
                    foreach (ColliderGroup colliderGroup2 in creature.handRight.grabbedHandle.item.colliderGroups)
                    {
                        foreach (Collider collider2 in colliderGroup2.colliders)
                            Physics.IgnoreCollision(collider1, collider2, true);
                    }
                }
            }
            item.ignoredItem = creature.handRight.grabbedHandle.item;
        }
    }

    public static void IgnoreCollision(this Item item, bool ignore = true)
    {
        foreach (ColliderGroup cg in item.colliderGroups)
        {
            foreach (Collider collider in cg.colliders)
            {
                collider.enabled = !ignore;
            }
        }
    }

    /// <summary>
    /// return the head, torso, leftHand, rightHand, leftFoot and rightFoot of the creature
    /// </summary>
    public static List<RagdollPart> RagdollPartsImportantList(this Creature creature)
    {
        List<RagdollPart> ragdollPartsimportant = new List<RagdollPart> {
                creature.GetPart(RagdollPart.Type.Head),
                creature.GetPart(RagdollPart.Type.Torso),
                creature.GetPart(RagdollPart.Type.LeftHand),
                creature.GetPart(RagdollPart.Type.RightHand),
                creature.GetPart(RagdollPart.Type.LeftFoot),
                creature.GetPart(RagdollPart.Type.RightFoot)};
        return ragdollPartsimportant;
    }
    /// <summary>
    /// return the leftHand, rightHand, leftFoot and rightFoot of the creature
    /// </summary>
    public static List<RagdollPart> RagdollPartsExtremitiesBodyList(this Creature creature)
    {
        List<RagdollPart> ragdollPartsimportant = new List<RagdollPart> {
                creature.GetPart(RagdollPart.Type.LeftHand),
                creature.GetPart(RagdollPart.Type.RightHand),
                creature.GetPart(RagdollPart.Type.LeftFoot),
                creature.GetPart(RagdollPart.Type.RightFoot)};
        return ragdollPartsimportant;
    }

    public static Vector3 RandomPositionAroundCreatureInRadius(this Creature creature, Vector3 offset, float radius)
    {
        return creature.transform.position + offset + new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
    }

    public static Vector3 CalculatePositionFromAngleWithDistance(this Vector3 position, float angle, Vector3 axis, Vector3 upDir, float distance)
    {
        return position + Quaternion.AngleAxis(angle, axis) * upDir * distance;
    }

    public static Vector3 RandomPositionAroundCreatureInRadiusAngle(this Creature creature, Vector3 offset, float radius, float maxAngle, Vector3 direction, float distance)
    {
        return GetHead(creature).transform.position + offset + Quaternion.AngleAxis(Random.Range(-maxAngle, maxAngle), creature.transform.up) * direction * Random.Range(0f, radius) * distance;
    }

    public static void DebugPosition(this Vector3 position, string textToDisplay)
    {
        Debug.Log("SnippetCode : " + textToDisplay + " : " + "Position X : " + position.x.ToString() + "; Position Y : " + position.y.ToString() + "; Position Z : " + position.z.ToString());
    }
    public static void DebugRotation(this Quaternion rotation, string textToDisplay)
    {
        Debug.Log("SnippetCode : " + textToDisplay + " : " + "Rotation X : " + rotation.x.ToString() + "; Rotation Y : " + rotation.y.ToString() + "; Rotation Z : " + rotation.z.ToString());
    }
    public static void DebugPositionAndRotation(this Transform transform, string textToDisplay)
    {
        Debug.Log("SnippetCode : " + textToDisplay + " : " + "Position X : " + transform.position.x.ToString() + "; Position Y : " + transform.position.y.ToString() + "; Position Z : " + transform.position.z.ToString());
        Debug.Log("SnippetCode : " + textToDisplay + " : " + "Rotation X : " + transform.rotation.x.ToString() + "; Rotation Y : " + transform.rotation.y.ToString() + "; Rotation Z : " + transform.rotation.z.ToString());
    }

    private static IEnumerator LerpMovement(this Vector3 positionToReach, Quaternion rotationToReach, Item itemToMove, float durationOfMvt)
    {
        foreach (ColliderGroup colliderGroup in itemToMove.colliderGroups)
        {
            foreach (Collider collider in colliderGroup.colliders)
            {
                collider.enabled = false;
            }
        }
        float time = 0;
        Vector3 positionOrigin = itemToMove.transform.position;
        Quaternion orientationOrigin = itemToMove.transform.rotation;
        if (positionToReach != positionOrigin)
        {
            while (time < durationOfMvt)
            {
                //itemToMove.isFlying = true;
                //itemToMove.rb.position = Vector3.Lerp(positionOrigin, positionToReach, time / durationOfMvt);
                //itemToMove.rb.rotation = Quaternion.Lerp(orientationOrigin, rotationToReach, time / durationOfMvt);
                itemToMove.transform.position = Vector3.Lerp(positionOrigin, positionToReach, time / durationOfMvt);
                itemToMove.transform.rotation = Quaternion.Lerp(orientationOrigin, rotationToReach, time / durationOfMvt);
                time += Time.deltaTime;
                yield return null;
            }
        }
        //itemToMove.rb.position = positionToReach;
        foreach (ColliderGroup colliderGroup in itemToMove.colliderGroups)
        {
            foreach (Collider collider in colliderGroup.colliders)
            {
                collider.enabled = true;
            }
        }
    }


    public static IEnumerable<GameObject> GetGameObjectsChildrenOfGameObject(this GameObject gameObject, bool allInactive = true, bool deepLevels = false)
    {
        List<GameObject> gameObjects = new List<GameObject>();
        if (deepLevels)
        {
            List<Transform> transforms = gameObject.GetComponentsInChildren<Transform>().ToList();
            foreach (Transform t in transforms)
            {
                gameObjects.Add(t.gameObject);
            }
        }
        else
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                // Grab only the actives
                if (gameObject.transform.GetChild(i).gameObject.activeSelf || allInactive)
                {
                    gameObjects.Add(gameObject.transform.GetChild(i).gameObject);
                }
            }
        }
        return gameObjects;
    }


    public static void listAllGameObjectsChildrenOfGameObjectAndComponents(this GameObject gameObject, bool allInactive = true, bool deepLevels = false)
    {
        int i = 0;
        Debug.Log($"Gameobject parent {i} {gameObject.name}");
        foreach (GameObject go in GetGameObjectsChildrenOfGameObject(gameObject, allInactive, deepLevels))
        {
            Debug.Log($"Gameobject {i} {go.name} of parent : {gameObject.name}");
            listAllComponentsOfGameObject(go);
            i++;
        }
    }

    public static void listAllGameObjectsChildrenOfGameObject(this GameObject gameObject, bool allInactive = true, bool deepLevels = false)
    {
        int i = 0;
        Debug.Log($"Gameobject parent {i} {gameObject.name}");
        foreach (GameObject go in GetGameObjectsChildrenOfGameObject(gameObject, allInactive, deepLevels))
        {
            Debug.Log($"Gameobject {i} {go.name} of parent : {gameObject.name}");
            i++;
        }
    }

    private static IEnumerable<Component> GetComponentsOfGameObject(this GameObject gameObject, bool allInactive, bool deepLevels = false)
    {
        return allInactive ? gameObject.GetComponents(typeof(Component)) : gameObject.GetComponents(typeof(Component)).Where(component => component.gameObject.activeSelf);
    }

    public static void listAllComponentsOfGameObject(this GameObject gameObject, bool allInactive = true)
    {
        int i = 0;
        foreach (Component component in GetComponentsOfGameObject(gameObject, allInactive))
        {
            Debug.Log($"Component {i} of {component.name}; Type : {component.GetType()}");
            i++;
        }
    }

    public static void listAllComponentsOfGameObjects()
    {
        List<GameObject> GOList = GameObject.FindObjectsOfType<GameObject>().ToList();
        foreach (GameObject gameObject in GOList)
        {
            Debug.Log($"Gameobject Name : {gameObject.name}");
            Debug.Log($"Gameobject Tag : {gameObject.tag}");
            listAllComponentsOfGameObject(gameObject);
        }
    }

    public static IEnumerable<GameObject> listAllGameObject()
    {
        return GameObject.FindObjectsOfType<GameObject>();
    }

    // Need to find a solution for that
    /*public static IEnumerable<T> ListOfType<T>(this T type)
    {
        return GameObject.FindObjectsOfType<type>();
    }*/
    public static IEnumerable<Light> LightInARadius(this Vector3 position, float radius)
    {
        return GameObject.FindObjectsOfType<Light>().Where(item => (item.transform.position - position).sqrMagnitude < radius * radius);
    }

    public static IEnumerable<Light> ListOfLightsInItems(this IEnumerable<Item> items)
    {
        Light[] lights;
        List<Light> listLights = new List<Light>();
        foreach (Item item in items)
        {
            lights = item.GetComponents<Light>();
            listLights.AddRange(lights);
            lights = item.GetComponentsInChildren<Light>();
            listLights.AddRange(lights);
            lights = item.GetComponentsInParent<Light>();
            listLights.AddRange(lights);
        }
        return listLights;
    }

    // Maybe useless
    public static IEnumerable<Light> ListOfLightsInGameObject(this IEnumerable<GameObject> gameObjects)
    {
        Light[] lights;
        List<Light> listLights = new List<Light>();
        foreach (GameObject gameObject in gameObjects)
        {
            lights = gameObject.GetComponents<Light>();
            listLights.AddRange(lights);
            lights = gameObject.GetComponentsInChildren<Light>();
            listLights.AddRange(lights);
            lights = gameObject.GetComponentsInParent<Light>();
            listLights.AddRange(lights);
        }
        return listLights;
    }

    public static List<Item> GetItemsOnCreature(this Creature creature, ItemData.Type? dataType = null)
    {
        List<Item> list = new List<Item>();
        foreach (Holder holder in creature.holders)
        {
            foreach (Item item in holder.items)
            {
                if (dataType.HasValue)
                {
                    if (item.data.type == dataType && dataType.HasValue)
                    {
                        list.Add(item);
                    }
                }
                else
                {
                    list.Add(item);
                }
            }
        }
        if (creature.handLeft.grabbedHandle?.item != null)
        {
            list.Add(creature.handLeft.grabbedHandle.item);
        }
        if (creature.handRight.grabbedHandle?.item != null)
        {
            list.Add(creature.handRight.grabbedHandle.item);
        }
        if (creature.mana.casterLeft.telekinesis.catchedHandle?.item != null)
        {
            list.Add(creature.mana.casterLeft.telekinesis.catchedHandle?.item);
        }
        if (creature.mana.casterRight.telekinesis.catchedHandle?.item != null)
        {
            list.Add(creature.mana.casterRight.telekinesis.catchedHandle?.item);
        }
        return list;
    }
    public static IEnumerable<Item> ItemsInRadiusAroundItem(this Vector3 position, Item thisItem, float radius)
    {
        List<Item> list = new List<Item>();
        for (int i = Item.allActive.Count - 1; i >= 0; i--)
        {
            if (((Item.allActive[i].transform.position - position).sqrMagnitude < radius * radius) && !thisItem)
                list.Add(Item.allActive[i]);
        }
        return list;
    }

    public static IEnumerable<Item> ItemsInRadius(Vector3 position, float radius, bool ignoreFlyingItem = true, bool ignoreThrownItem = true, Item itemToExclude = null)
    {
        Collider[] colliders = Physics.OverlapSphere(position, radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        List<Item> itemsList = new List<Item>();
        foreach (Collider collider in colliders)
        {
            if (collider.attachedRigidbody?.GetComponent<CollisionHandler>()?.item is Item item)
            {
                if (!itemsList.Contains(item)
                    && (ignoreFlyingItem ? true : item.isFlying)
                    && (ignoreThrownItem ? true : item.isThrowed)
                    && (item != itemToExclude))
                {
                    itemsList.Add(item);
                }
            }
        }
        return itemsList;
    }
    public static IEnumerable<Item> ItemsInConeRadius(this Vector3 position, float radius, Vector3 directionOfCone, float angleOfCone, bool targetFlyingItem = true, bool targetThrownItem = true, Item itemToExclude = null)
    {
        Collider[] colliders = Physics.OverlapSphere(position, radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        List<Item> itemsList = new List<Item>();
        foreach (Collider collider in colliders)
        {
            if (collider.attachedRigidbody?.GetComponent<CollisionHandler>()?.item is Item item)
            {
                Vector3 directionTowardT = item.transform.position - position;
                float angleFromConeCenter = Vector3.Angle(directionTowardT, directionOfCone);
                if (!itemsList.Contains(item)
                && (targetFlyingItem ? item.isFlying : true)
                && (targetThrownItem ? item.isThrowed : true)
                && (item != itemToExclude) && angleFromConeCenter <= (angleOfCone / 2f))
                {
                    itemsList.Add(item);
                }
            }
        }
        return itemsList;
    }

    public static Item ClosestItemAroundItem(this Item thisItem, float radius)
    {
        float lastRadius = Mathf.Infinity;
        Item lastItem = null;
        float thisRadius;
        foreach (Item item in Item.allActive)
        {
            if (item == thisItem)
                continue;
            thisRadius = (item.transform.position - thisItem.transform.position).sqrMagnitude;
            if (thisRadius < radius * radius && thisRadius < lastRadius)
            {
                lastRadius = thisRadius;
                lastItem = item;
            }
        }
        return lastItem;
    }

    public static Item ClosestItemAroundItemOverlapSphere(this Item thisItem, float radius)
    {
        float lastRadius = Mathf.Infinity;
        Collider lastCollider = null;
        float thisRadius;

        Collider[] colliders = Physics.OverlapSphere(thisItem.transform.position, radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        List<Item> itemsList = new List<Item>();
        foreach (Collider collider in colliders)
        {
            if (collider.attachedRigidbody?.GetComponent<CollisionHandler>()?.item is Item item)
            {
                if (item != thisItem)
                {
                    thisRadius = (collider.ClosestPoint(thisItem.transform.position) - thisItem.transform.position).sqrMagnitude;
                    if (thisRadius < radius * radius && thisRadius < lastRadius)
                    {
                        lastRadius = thisRadius;
                        lastCollider = collider;
                    }
                }
            }
        }
        if (lastCollider?.attachedRigidbody?.GetComponent<CollisionHandler>().item == null)
        {
            return thisItem;
        }
        else
        {
            return lastCollider.attachedRigidbody.GetComponent<CollisionHandler>().item;
        }
    }


    public static RagdollPart ClosestRagdollPartAroundItemOverlapSphere(this Item thisItem, float radius, bool targetPlayer = false)
    {
        float lastRadius = Mathf.Infinity;
        Collider lastCollider = null;
        float thisRadius;
        List<Collider> colliders = Physics.OverlapSphere(thisItem.transform.position, radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
            .Distinct().Where(coll => coll.attachedRigidbody?.GetComponent<CollisionHandler>()?.ragdollPart != null && targetPlayer ? true : !Player.local.creature.ragdoll.parts.Contains(coll.attachedRigidbody?.GetComponent<CollisionHandler>()?.ragdollPart)).ToList();
        foreach (Collider collider in colliders)
        {
            thisRadius = (collider.ClosestPoint(thisItem.transform.position) - thisItem.transform.position).sqrMagnitude;
            if (thisRadius < radius * radius && thisRadius < lastRadius)
            {
                lastRadius = thisRadius;
                lastCollider = collider;
            }
        }
        if (lastCollider?.attachedRigidbody?.GetComponent<CollisionHandler>().ragdollPart == null)
        {
            return null;
        }
        else
        {
            return lastCollider.attachedRigidbody.GetComponent<CollisionHandler>().ragdollPart;
        }
    }

    /// <summary>
    /// Get the closestRagdollPart of a creature
    /// </summary>
    /// <param name="origin">Origin position</param>
    /// <param name="creature">Creature where the part need to be targeted</param>
    /// <param name="mask">Mask Apply (write it in binary : 0b00011111111111) : 1 means get the part, 0 means don't get the part : in the order of the bit from left to right : 
    /// Tail, RightWing, LeftWing, RightFoot, LeftFoot, RightLeg, LeftLeg, RightHand, LeftHand, RightArm, LeftArm, Torso, Neck, Head</param>
    /// <param name="partToExclude">Part to exclude in case it's the same part (for random case)</param>
    public static RagdollPart ClosestRagdollPart(this Vector3 origin, Creature creature, int mask = 0b00011111111111, RagdollPart partToExclude = null)
    {
        float lastRadius = Mathf.Infinity;
        float thisRadius;
        RagdollPart lastRagdollPart = null;
        foreach (RagdollPart part in creature.ragdoll.parts)
        {
            if (((mask & (int)part.type) > 0) && part != partToExclude)
            {
                thisRadius = (part.transform.position - origin).sqrMagnitude;
                if (thisRadius <= lastRadius)
                {
                    lastRadius = thisRadius;
                    lastRagdollPart = part;
                }
            }
        }
        return lastRagdollPart;
    }

    /// <summary>
    /// Get the farestRagdollPart of a creature
    /// </summary>
    /// <param name="origin">Origin position</param>
    /// <param name="creature">Creature where the part need to be targeted</param>
    /// <param name="mask">Mask Apply (write it in binary : 0b00011111111111) : 1 means get the part, 0 means don't get the part : in the order of the bit from left to right : 
    /// Tail, RightWing, LeftWing, RightFoot, LeftFoot, RightLeg, LeftLeg, RightHand, LeftHand, RightArm, LeftArm, Torso, Neck, Head</param>
    /// <param name="partToExclude">Part to exclude in case it's the same part (for random case)</param>
    public static RagdollPart FarestRagdollPart(this Vector3 origin, Creature creature, int mask = 0b00011111111111, RagdollPart partToExclude = null)
    {
        float lastRadius = 0f;
        float thisRadius;
        RagdollPart lastRagdollPart = null;
        foreach (RagdollPart part in creature.ragdoll.parts)
        {
            if (((mask & (int)part.type) > 0) && part != partToExclude)
            {
                thisRadius = (part.transform.position - origin).sqrMagnitude;
                if (thisRadius >= lastRadius)
                {
                    lastRadius = thisRadius;
                    lastRagdollPart = part;
                }
            }
        }
        return lastRagdollPart;
    }

    public static int ReturnNbFreeSlotOnCreature(this Creature creature)
    {
        int nbFreeSlots = 0;
        foreach (Holder holder in creature.holders)
        {
            if (holder.currentQuantity != 0)
            {
                nbFreeSlots++;
            }
        }
        return nbFreeSlots;
    }


    public static Vector3 HomingTarget(Item projectile, Vector3 targetPosition, float initialDistance, float forceFactor, float offSetInitialDistance = 0.25f, float distanceToStick = 0f)
    {
        return Vector3.Lerp(projectile.rb.velocity,
            (targetPosition - projectile.transform.position).normalized * Vector3.Distance(targetPosition, projectile.transform.position) * forceFactor,
            Vector3.Distance(targetPosition, projectile.transform.position).Remap01(initialDistance + offSetInitialDistance, distanceToStick));
    }

    public static Vector3 HomingBehaviour(Item projectile, Vector3 targetPosition, float initialTime, float forceFactor = 30f, float speed = 1f)
    {
        return Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(projectile.rb.velocity, targetPosition - projectile.transform.position), Time.deltaTime * forceFactor * Mathf.Clamp01((Time.time - initialTime) / 0.5f))
            * projectile.rb.velocity.normalized * Vector3.Distance(targetPosition, projectile.transform.position) * speed;
    }

    // Thank you Wully !
    public static bool DidPlayerParry(CollisionInstance collisionInstance)
    {
        if (collisionInstance.sourceColliderGroup?.collisionHandler.item?.mainHandler?.creature.player)
            return true;
        if (!collisionInstance.targetColliderGroup?.collisionHandler.item?.mainHandler?.creature.player)
            return false;
        return true;
    }

    public static GameObject AddZoneToGameObject(this Transform transform, float radius, bool useDebug = false, LayerName layerName = LayerName.ItemAndRagdollOnly)
    {
        GameObject zone;
        zone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        zone.GetComponent<Collider>().isTrigger = true;
        //Vector3 endPoint = FindEndPoint();
        //Ray ray = new Ray(transform.position, endPoint - transform.position);
        //float distanceHit = Vector3.Distance(transform.position, endPoint);
        zone.transform.SetParent(transform);
        zone.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Vector3.forward);
        zone.transform.localPosition = Vector3.zero;
        //zoneDoT.transform.localScale = new Vector3(radiusOfDetection, distanceHit / 2, radiusOfDetection);
        zone.transform.localScale = new Vector3(radius, radius, radius);
        zone.gameObject.layer = GameManager.GetLayer(layerName);
        zone.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Sprites/Default"));
        zone.GetComponent<MeshRenderer>().material.color = Color.blue;
        zone.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        zone.GetComponent<MeshRenderer>().forceRenderingOff = !useDebug;
        return zone;
    }

    public static void ThrowFireball(this Vector3 origin, Vector3 directionToShoot, float forceOfThrow = 30f, float distanceToShootFrom = 1f, Creature ignoredCreature = null)
    {
        Vector3 positionToSpawn;
        positionToSpawn = origin + directionToShoot.normalized * distanceToShootFrom;
        Catalog.GetData<ItemData>("DynamicProjectile").SpawnAsync(projectile =>
        {
            projectile.disallowDespawn = true;
            projectile.rb.useGravity = false;
            projectile.rb.velocity = Vector3.zero;
            foreach (CollisionHandler collisionHandler in projectile.collisionHandlers)
            {
                foreach (Damager damager in collisionHandler.damagers)
                    damager.Load(Catalog.GetData<DamagerData>("Fireball"), collisionHandler);
            }
            ItemMagicProjectile component = projectile.GetComponent<ItemMagicProjectile>();
            if (component)
            {
                component.guidance = GuidanceMode.NonGuided;
                component.speed = 0;
                component.allowDeflect = true;
                component.deflectEffectData = Catalog.GetData<EffectData>("HitFireBallDeflect");
                component.Fire(directionToShoot * forceOfThrow, Catalog.GetData<EffectData>("SpellFireball"));
            }
            projectile.isThrowed = true;
            projectile.isFlying = true;
            projectile.Throw(flyDetection: Item.FlyDetection.Forced);
            if (ignoredCreature)
            {
                projectile.IgnoreRagdollCollision(ignoredCreature.ragdoll);
            }
        }, positionToSpawn, Quaternion.LookRotation(directionToShoot, Vector3.up));
    }
    public static void ThrowMeteor(this Vector3 origin, Vector3 directionToShoot, Creature thrower, bool useGravity = true, float factorOfThrow = 1f, float distanceToShootFrom = 0.5f, bool ignoreCollision = false)
    {
        Item meteor = new Item();
        EffectData meteorEffectData = Catalog.GetData<EffectData>("Meteor");
        EffectData meteorExplosionEffectData = Catalog.GetData<EffectData>("MeteorExplosion");
        float meteorVelocity = 7f;

        float meteorExplosionDamage = 20f;
        float meteorExplosionPlayerDamage = 20f;
        float meteorExplosionRadius = 10f;
        AnimationCurve meteorIntensityCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 0.5f, 1f);
        SpellCastCharge meteorImbueSpellData = Catalog.GetData<SpellCastCharge>("Fire");
        ItemMagicAreaProjectile projectile;

        Vector3 positionToSpawn;
        positionToSpawn = origin + directionToShoot.normalized * (distanceToShootFrom + 0.15f);
        Catalog.GetData<ItemData>("Meteor").SpawnAsync(item =>
        {
            item.disallowDespawn = true;
            item.rb.useGravity = useGravity;
            item.IgnoreCollision(ignoreCollision);
            ItemMagicAreaProjectile component = item.GetComponent<ItemMagicAreaProjectile>();
            if (component != null)
            {
                projectile = component;
                component.explosionEffectData = Catalog.GetData<EffectData>("MeteorExplosion");
                component.areaRadius = meteorExplosionRadius;
                component.OnHandlerHit += (hit, handler) =>
                {
                    if (!handler.isItem)
                        return;
                    MeteorImbueItem(hit.targetColliderGroup);
                };
                component.OnHandlerAreaHit += (collider, handler) =>
                {
                    if (!handler.isItem)
                        return;
                    MeteorImbueItem(collider.GetComponentInParent<ColliderGroup>());
                };
                component.OnCreatureAreaHit += (collider, creature) => creature.Damage(new CollisionInstance(new DamageStruct(DamageType.Energy, creature.isPlayer ? meteorExplosionPlayerDamage : meteorExplosionDamage)));
                component.OnHit += collision => MeteorExplosion(collision.contactPoint, meteorExplosionRadius, thrower);
                component.guidance = GuidanceMode.NonGuided;
                component.guidanceAmount = 0f;
                component.speed = meteorVelocity;
                component.effectIntensityCurve = meteorIntensityCurve;
                item.rb.AddForce(directionToShoot * meteorVelocity * factorOfThrow, ForceMode.Impulse);
                component.Fire(directionToShoot, meteorEffectData, null, Player.currentCreature.ragdoll);
            }
            meteor = item;
        }, positionToSpawn, Quaternion.LookRotation(directionToShoot, Vector3.up));
    }

    private static void MeteorImbueItem(ColliderGroup group) => group?.imbue?.Transfer(Catalog.GetData<SpellCastCharge>("Fire"), group.imbue.maxEnergy * 2f);

    private static void MeteorExplosion(this Vector3 position, float radius, Creature thrower)
    {
        HashSet<Rigidbody> rigidbodySet = new HashSet<Rigidbody>();
        HashSet<Creature> hitCreatures = new HashSet<Creature>();
        float meteorExplosionForce = 20f;
        float meteorExplosionPlayerForce = 5f;
        LayerMask explosionLayerMask = 232799233;
        foreach (Collider collider in Physics.OverlapSphere(position, radius, explosionLayerMask, QueryTriggerInteraction.Ignore))
        {
            if (collider.attachedRigidbody && !rigidbodySet.Contains(collider.attachedRigidbody))
            {
                float explosionForce = meteorExplosionForce;
                Creature componentInParent = collider.attachedRigidbody.GetComponentInParent<Creature>();
                if (componentInParent != null && componentInParent != thrower && !componentInParent.isKilled && !componentInParent.isPlayer && !hitCreatures.Contains(componentInParent))
                {
                    componentInParent.ragdoll.SetState(Ragdoll.State.Destabilized);
                    hitCreatures.Add(componentInParent);
                }
                if (collider.attachedRigidbody.GetComponentInParent<Player>() != null)
                    explosionForce = meteorExplosionPlayerForce;
                rigidbodySet.Add(collider.attachedRigidbody);
                collider.attachedRigidbody.AddExplosionForce(explosionForce, position, radius, 1f, ForceMode.VelocityChange);
            }
        }
    }

    public static float PingPongValue(float min, float max, float speed)
    {
        return Mathf.Lerp(min, max, Mathf.PingPong(Time.time * speed, 1));
    }

    // Sin with a slight curve to slow when going to the reverse side
    public static AnimationCurve CurveSinSpinReverseRadius()
    {
        Keyframe[] keyframes;
        keyframes = new Keyframe[5];
        keyframes[0] = new Keyframe(0.0f, 0.0f, 0f, 15f);
        keyframes[1] = new Keyframe(0.25f, 0.15f, -15f, -15f);
        keyframes[2] = new Keyframe(0.5f, 0.0f, 15f, 15f);
        keyframes[3] = new Keyframe(0.75f, -0.15f, -15f, -15f);
        keyframes[4] = new Keyframe(1.0f, 0.0f, 15f, 15f);
        return new AnimationCurve(keyframes);
    }

    // Sin with a slight curve to slow when going to the reverse side
    public static AnimationCurve CurveSinSpinReverseSpeed()
    {
        Keyframe[] keyframes;
        keyframes = new Keyframe[5];
        keyframes[0] = new Keyframe(0.0f, 0.0f, 0f, 0f);
        keyframes[1] = new Keyframe(0.25f, 1.0f, 0f, 0f);
        keyframes[2] = new Keyframe(0.5f, 0.0f, 0.75f, 0.75f);
        keyframes[3] = new Keyframe(0.75f, -1.0f, 0f, 0f);
        keyframes[4] = new Keyframe(1.0f, 0.0f, 0f, 0f);
        return new AnimationCurve(keyframes);
    }




    // Representation of the curve (roughly)
    //                                                                                                                  *
    //                                                                    *
    //                                                   *
    //                                          *
    //                                 *
    //                          *
    //                    *
    //                *
    //              *
    //            *
    //          *
    //        *
    //      *
    //     *
    //    *
    //    *
    //   *
    //   *
    //  *  
    //  * 
    // *
    // *
    //*
    //*

    public static AnimationCurve CurveSlowDown()
    {
        Keyframe[] keyframes;
        keyframes = new Keyframe[3];
        keyframes[0] = new Keyframe(0.0f, 0.0f, 0f, 5f);
        keyframes[1] = new Keyframe(0.25f, 0.75f, 1f, 1f);
        keyframes[2] = new Keyframe(1.0f, 1.0f, 0f, 0f);
        return new AnimationCurve(keyframes);
    }

    public static void SlowDownFallCreature(Creature creature = null, float factor = 3f, float gravityValue = 9.81f)
    {
        if (creature == null)
            return;
        AnimationCurve curve = CurveSlowDown();
        creature.currentLocomotion.rb.AddForce(new Vector3(0f, curve.Evaluate(Mathf.InverseLerp(0f, gravityValue, -creature.currentLocomotion.velocity.y)) * gravityValue * factor, 0f), ForceMode.Acceleration);
    }

    public static bool MoveRightHandCloserToCenterOfBodyFast()
    {
        return Player.local.creature.handRight.rb.velocity.sqrMagnitude > 10f
            && Vector3.SignedAngle(Player.local.creature.transform.forward, Vector3.Cross(Player.local.creature.handRight.rb.velocity, Player.local.creature.transform.right), Player.local.transform.forward) < 90f;
    }

    public static bool MoveLeftHandCloserToCenterOfBodyFast()
    {
        return Player.local.creature.handLeft.rb.velocity.sqrMagnitude > 10f
            && Vector3.SignedAngle(Player.local.creature.transform.forward, Vector3.Cross(Player.local.creature.handLeft.rb.velocity, Player.local.creature.transform.right), Player.local.transform.forward) < 90f;
    }

    public static bool MoveBothHandCloserToCenterOfBodyFast()
    {
        return MoveLeftHandCloserToCenterOfBodyFast() && MoveRightHandCloserToCenterOfBodyFast();
    }

    public static bool BothHandAligned(float distance = 0.75f)
    {
        return Vector3.Dot(Player.local.handLeft.ragdollHand.PointDir(), Player.local.handRight.ragdollHand.PointDir()) > -1f && Vector3.Dot(Player.local.handLeft.ragdollHand.PointDir(), Player.local.handRight.ragdollHand.PointDir()) < -0.5f && Vector3.Distance(Player.local.handLeft.ragdollHand.transform.position, Player.local.handRight.ragdollHand.transform.position) > distance;
    }

    public static float SpeedSinSpinReverse(this float speed, float speedStrength = 480f, float timeLapse = 5)
    {
        AnimationCurve curve = CurveSinSpinReverseSpeed();
        float factorSpeed = curve.Evaluate((Time.time / timeLapse) % 1) * speedStrength;
        return speed += Time.fixedDeltaTime * factorSpeed;
    }

    public static float RadiusSinSpinReverse(this float radius, float radiusStrength = 0.3f, float timeLapse = 5)
    {
        AnimationCurve curve = CurveSinSpinReverseRadius();
        float factorRadius = curve.Evaluate((Time.time / timeLapse) % 1) * radiusStrength;
        return radius += Time.fixedDeltaTime * factorRadius;
    }
    public static void ImbueItem(this Item item, string ID)
    {
        SpellCastCharge magic = Catalog.GetData<SpellCastCharge>(ID, true);
        foreach (Imbue imbue in item.imbues)
        {
            if (imbue.energy < imbue.maxEnergy)
            {
                imbue.Transfer(magic, imbue.maxEnergy);
            }
        }
    }

    public static string returnImbueId(this Item item)
    {
        string imbueId = null;
        foreach (Imbue imbue in item.imbues)
        {
            if (imbue.energy > 0.0f)
            {
                imbueId = imbue.spellCastBase.id;
            }
        }
        return imbueId;
    }

    public static void UnImbueItem(this Item item)
    {
        foreach (Imbue imbue in item.imbues)
        {
            if (imbue.energy < 0.0f)
            {
                imbue.energy = 0.0f;
            }
        }
    }

    public static bool imbueBelowLevelItem(this Item item, float level)
    {
        bool levelBelowOK = false;
        foreach (Imbue imbue in item.imbues)
        {
            if (imbue.energy < level)
            {
                levelBelowOK = true;
                break;
            }
        }
        return levelBelowOK;
    }

    public static Color HDRColor(Color color, float intensity)
    {
        return color * Mathf.Pow(2, intensity);
    }

    public static GameObject CreateDebugPoint(bool useLineRenderer = true, Light light = null)
    {
        LineRenderer lineRenderer;
        Color color;
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gameObject.transform.localScale = Vector3.one * 0.1f;
        if (useLineRenderer)
        {
            if (light != null)
            {
                if (light.type == LightType.Spot)
                {
                    color = new Color(255, 0, 0);
                }
                else if (light.type == LightType.Point)
                {
                    color = new Color(255, 127, 0);
                }
                else if (light.type == LightType.Rectangle)
                {
                    color = new Color(255, 0, 255);
                }
                else if (light.type == LightType.Directional)
                {
                    color = new Color(0, 0, 255);
                }
                // Area light
                else
                {
                    color = new Color(0, 127, 255);
                }
            }
            else
            {
                color = new Color(255, 255, 255);
            }
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.003f;
            lineRenderer.endWidth = 0.003f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
        return gameObject;
    }
    public static void AddDebugPointToGO(this GameObject gameObject, Color colorParam, bool useLineRenderer = true, Light light = null)
    {
        LineRenderer lineRenderer;
        Color color;
        if (useLineRenderer)
        {
            if (light != null)
            {
                if (light.type == LightType.Spot)
                {
                    color = new Color(255, 0, 0);
                }
                else if (light.type == LightType.Point)
                {
                    color = new Color(255, 127, 0);
                }
                else if (light.type == LightType.Rectangle)
                {
                    color = new Color(255, 0, 255);
                }
                else if (light.type == LightType.Directional)
                {
                    color = new Color(0, 0, 255);
                }
                // Area light
                else
                {
                    color = new Color(0, 127, 255);
                }
            }
            else
            {
                color = new Color(255, 255, 255);
            }
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.003f;
            lineRenderer.endWidth = 0.003f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            if (colorParam == null)
                lineRenderer.startColor = color;
            else
                lineRenderer.startColor = colorParam;
            if (colorParam == null)
                lineRenderer.endColor = color;
            else
                lineRenderer.endColor = colorParam;
        }
    }

    public static void RefreshDebugPointOfGO(this GameObject gameObject)
    {
        gameObject?.GetComponent<LineRenderer>()?.SetPosition(0, gameObject.transform.position);
        gameObject?.GetComponent<LineRenderer>()?.SetPosition(1, Player.local.handRight.ragdollHand.fingerIndex.tip.position);
    }

    public static string GetPath(this Transform current)
    {
        if (current.parent == null)
            return "/" + current.name;
        return GetPath(current.parent) + "/" + current.name;
    }

    public static void AddHolderPoint(Item item, Vector3 position)
    {
        GameObject GO = new GameObject("HolderPoint");
        GO.transform.SetParent(item.gameObject.transform);
        GO.transform.position = Vector3.zero;
        GO.transform.localPosition = position == null ? new Vector3(0f, 0f, 0f) : position;
        GO.transform.rotation = Quaternion.Euler(90f, 180f, 0f);
        item.holderPoint = GO.transform;
    }
    public static Damager AddDamager(this Item item, string damagerName, string colliderGroupName)
    {
        GameObject GO = new GameObject(damagerName);
        GO.transform.SetParent(item.gameObject.transform);
        Damager damager = GO.gameObject.AddComponent<Damager>();
        damager = SetDamager(item, damager, damagerName, colliderGroupName);
        return damager;
    }

    private static Damager SetDamager(this Item item, Damager damager, string damagerName, string colliderGroupName, Damager.Direction direction = Damager.Direction.All, float penetrationLength = 0f, float penetrationDepth = 0f, bool penetrationExitOnMaxDepth = false)
    {
        damager.name = damagerName;
        damager.colliderGroup = item.colliderGroups.Where(colliderGroup => colliderGroup.name == colliderGroupName).FirstOrDefault();
        damager.direction = direction;
        damager.penetrationLength = penetrationLength;
        damager.penetrationDepth = penetrationDepth;
        damager.penetrationExitOnMaxDepth = penetrationExitOnMaxDepth;
        return damager;
    }

    public static Holder AddHolderSlots(this Item item, string holderName, string interactableID, Vector3 touchCenter, Vector3 positionOnItem, Interactable.HandSide allowedHandSide = Interactable.HandSide.Both, float axisLength = 0f, Holder.DrawSlot drawSlot = Holder.DrawSlot.None, float touchRadius = 0.04f, bool useAnchor = true, int nbSlots = 1, float spacingSlots = 0.04f)
    {
        GameObject GO = new GameObject(holderName);
        GO.transform.SetParent(item.gameObject.transform);
        GO.transform.localPosition = positionOnItem;
        Holder holderSlot = GO.gameObject.AddComponent<Holder>();
        holderSlot = SetHolderSlots(holderSlot, interactableID, touchCenter, allowedHandSide, axisLength, drawSlot, touchRadius, useAnchor, nbSlots, spacingSlots);
        return holderSlot;
    }

    private static Holder SetHolderSlots(this Holder holder, string interactableID, Vector3 touchCenter, Interactable.HandSide allowedHandSide = Interactable.HandSide.Both, float axisLength = 0f, Holder.DrawSlot drawSlot = Holder.DrawSlot.None, float touchRadius = 0.04f, bool useAnchor = true, int nbSlots = 1, float spacingSlots = 0.04f)
    {
        holder.interactableId = interactableID;
        holder.allowedHandSide = allowedHandSide;
        holder.axisLength = axisLength;
        holder.touchRadius = touchRadius;
        holder.touchCenter = touchCenter;
        holder.drawSlot = drawSlot;
        holder.useAnchor = useAnchor;
        holder.SetSlots(nbSlots, spacingSlots);
        return holder;
    }

    private static Holder SetSlots(this Holder holder, int nbSlots, float spacing, int axe = 2)
    {
        holder.slots = new List<Transform>();
        for (int i = 0; i < nbSlots; i++)
        {
            GameObject slot = new GameObject($"Slot{i + 1}");
            slot.transform.SetParent(holder.gameObject.transform);
            slot.transform.localRotation = holder.gameObject.transform.rotation * Quaternion.Euler(90f, 180f, -180f);
            slot.transform.localPosition = new Vector3(axe == 1 ? spacing * i : 0f, axe == 2 ? spacing * i : 0f, axe == 3 ? spacing * i : 0f);
            holder.slots.Add(slot.transform);
        }
        return holder;
    }
    public static void DebugLog(string text, bool useColor = false, string color = "white")
    {
        if (useColor)
        { 
            Debug.Log($"<color={text}></color> {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}");
        }
        else
        {
            Debug.Log($"{text} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}");
        }
    }

    #region Classes :
    public class PenetrateItem : CollisionHandler
    {
        public delegate void PenetrateEvent(CollisionInstance collisionInstance);
        public event PenetrateEvent OnPenetrateStart;
        public event PenetrateEvent OnPenetrateStop;
        private bool isPenetrating;

        public void InvokePenetrateStart(CollisionInstance collisionInstance)
        {
            PenetrateEvent penetrateStartEvent = OnPenetrateStart;
            if (penetrateStartEvent == null)
                return;
            penetrateStartEvent(collisionInstance);
            isPenetrating = true;
        }
        public void InvokePenetrateStop(CollisionInstance collisionInstance)
        {
            PenetrateEvent penetrateStopEvent = OnPenetrateStop;
            if (penetrateStopEvent == null)
                return;
            penetrateStopEvent(collisionInstance);
            isPenetrating = false;
        }
        protected override void ManagedOnEnable()
        {
            base.ManagedOnEnable();
            isPenetrating = false;
            OnCollisionStartEvent += PenetrateItem_OnCollisionStartEvent;
            OnCollisionStopEvent += PenetrateItem_OnCollisionStopEvent;
        }
        private void PenetrateItem_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            if (!isPenetrating)
            {
                InvokePenetrateStart(collisionInstance);
            }
        }
        private void PenetrateItem_OnCollisionStopEvent(CollisionInstance collisionInstance)
        {
            if (isPenetrating)
            {
                InvokePenetrateStart(collisionInstance);
            }
        }
        protected override void ManagedOnDisable()
        {
            base.ManagedOnDisable();
            isPenetrating = false;
            OnCollisionStartEvent -= PenetrateItem_OnCollisionStartEvent;
            OnCollisionStopEvent -= PenetrateItem_OnCollisionStopEvent;
        }
    }

    public enum Step
    {
        Enter,
        Update,
        Exit
    }

    public class Zone : MonoBehaviour
    {
        public float distance;
        public float radius;
        public CapsuleCollider collider;
        private HashSet<CollisionHandler> handlers;
        private HashSet<Creature> creatures;
        private HashSet<Item> items;

        public void Start()
        {
            handlers = new HashSet<CollisionHandler>();
            creatures = new HashSet<Creature>();
            items = new HashSet<Item>();
            gameObject.layer = GameManager.GetLayer(LayerName.ItemAndRagdollOnly);
            collider = gameObject.AddComponent<CapsuleCollider>();
            collider.center = Vector3.forward * (distance / 2 - 0.3f);
            collider.radius = radius;
            collider.height = distance + 0.3f;
            collider.direction = 2;
            collider.isTrigger = true;
            Begin();
        }

        public virtual void Begin() { }

        public void Update()
        {
            foreach (var handler in handlers) OnHandlerEvent(handler, Step.Update);
            foreach (var item in items) OnItemEvent(item, Step.Update);
            foreach (var creature in creatures) CreatureEvent(creature, Step.Update);
        }

        public void Despawn()
        {
            foreach (CollisionHandler handler in handlers) OnHandlerEvent(handler, Step.Exit);
            foreach (Item item in items) OnItemEvent(item, Step.Exit);
            foreach (Creature creature in creatures) CreatureEvent(creature, Step.Exit);
            OnDespawn();
            Destroy(gameObject);
        }

        public virtual void OnDespawn() { }


        public void OnTriggerEnter(Collider collider)
        {
            var handler = collider.attachedRigidbody?.GetComponent<CollisionHandler>();
            if (!handler || collider.attachedRigidbody.isKinematic) return;
            if (!handlers.Contains(handler))
            {
                handlers.Add(handler);
                if (handler.ragdollPart?.ragdoll.creature.isPlayer != true)
                {
                    OnHandlerEvent(handler, Step.Enter);
                }
            }

            if (handler.item is Item item)
            {
                if (!items.Contains(item))
                {
                    OnItemEvent(item, Step.Enter);
                    items.Add(item);
                }
            }

            if (handler.ragdollPart?.ragdoll.creature is Creature creature)
            {
                if (!creatures.Contains(creature))
                {
                    creatures.Add(creature);
                    CreatureEvent(creature, Step.Enter);
                }
            }
        }

        public bool HasItemHandlers(Item item) => handlers.Any(handler => handler.item == item);
        public bool HasCreatureHandlers(Creature creature) => handlers.Any(handler => handler.ragdollPart?.ragdoll.creature == creature);
        public void OnTriggerExit(Collider collider)
        {
            var handler = collider.attachedRigidbody?.GetComponent<CollisionHandler>();
            if (!handler || collider.attachedRigidbody.isKinematic) return;
            if (handlers.Contains(handler))
            {
                handlers.Remove(handler);
                if (handler.ragdollPart?.ragdoll.creature.isPlayer != true)
                {
                    OnHandlerEvent(handler, Step.Exit);
                }
            }

            if (handler.item is Item item && items.Contains(item) && !HasItemHandlers(item))
            {
                items.Remove(item);
                OnItemEvent(item, Step.Exit);
            }
            if (handler.ragdollPart?.ragdoll.creature is Creature creature && creatures.Contains(creature) && !HasCreatureHandlers(creature))
            {
                creatures.Remove(creature);
                CreatureEvent(creature, Step.Exit);
            }
        }

        public void CreatureEvent(Creature creature, Step step)
        {
            OnCreatureEvent(creature, step);
            if (creature.isPlayer)
            {
                OnPlayerEvent(step);
            }
            else
            {
                OnNPCEvent(creature, step);
            }
        }


        public virtual void OnNPCEvent(Creature creature, Step step) { }
        public virtual void OnPlayerEvent(Step step) { }
        public virtual void OnCreatureEvent(Creature creature, Step step) { }
        public virtual void OnItemEvent(Item item, Step step) { }
        public virtual void OnHandlerEvent(CollisionHandler handler, Step step) { }
    }

    public class FreezeBehaviour : MonoBehaviour
    {
        private string brainId;
        private float timeOfFreezing = 1.5f;
        private Creature creature;

        private float orgAnimatorSpeed;
        private float targetAnimatorSpeed = 0f;
        private float animatorSpeed;
        //private float orgLocomotionSpeed;
        //private float targetLocomotionSpeed = 0f;
        //private float locomotionSpeed;
        private Vector2 orgSpeakPitchRange;
        private Vector2 targetSpeakPitchRange = Vector2.zero;
        private Vector2 speakPitchRange;
        private float orgBlendFreezeValue = 1f;
        private float targetBlendFreezeValue = 0f;
        private float blendFreezeValue;
        private Color colorFreeze = new Color(0.24644f, 0.5971831f, 0.735849f);
        private Color orgColorHair;
        private Color targetColorHair = Color.cyan;
        private Color colorHair;
        private Color orgColorHairSecondary;
        private Color targetColorHairSecondary;
        private Color colorHairSecondary;
        private Color orgColorHairSpecular;
        private Color targetColorHairSpecular;
        private Color colorHairSpecular;
        private Color orgColorEyesIris;
        private Color targetColorEyesIris;
        private Color colorEyesIris;
        private Color orgColorEyesSclera;
        private Color targetColorEyesSclera;
        private Color colorEyesSclera;
        private Color orgColorSkin;
        private Color targetColorSkin;
        private Color colorSkin;
        //private Material freezeMaterial;
        //private List<Material> targetMaterials;
        //private Texture targetTexture;
        //AsyncOperationHandle<Material> handleFreezeMaterial = Addressables.LoadAssetAsync<Material>("Neeshka.TestFreeze.Freeze_Mat");
        //Counting timers
        private float timerSlow = 1f;
        private float timerFreeze;

        private bool isFrozen = false;
        private bool endOfFreeze = false;

        private float totalTimeOfFreezeRagdoll = Random.Range(7.0f, 10.0f);

        public void Init(float timerSlowing, float timerFreezeRagdoll)
        {
            creature = GetComponent<Creature>();
            creature.OnDespawnEvent += time =>
            {
                if (time == EventTime.OnStart)
                {
                    Disable();
                }
            };
            creature.OnKillEvent += (collisionInstance, time) =>
            {
                if (time == EventTime.OnStart)
                {
                    creature.animator.speed = orgAnimatorSpeed;
                    creature.brain.Load(brainId);
                    creature.ragdoll.creature.brain.instance.GetModule<BrainModuleSpeak>().audioPitchRange = orgSpeakPitchRange;
                }
            };
            orgAnimatorSpeed = creature.animator.speed;
            //orgLocomotionSpeed = creature.locomotion.speed;
            orgSpeakPitchRange = creature.ragdoll.creature.brain.instance.GetModule<BrainModuleSpeak>().audioPitchRange;
            orgColorHair = creature.GetColor(Creature.ColorModifier.Hair);
            orgColorHairSecondary = creature.GetColor(Creature.ColorModifier.HairSecondary);
            orgColorHairSpecular = creature.GetColor(Creature.ColorModifier.HairSpecular);
            orgColorEyesIris = creature.GetColor(Creature.ColorModifier.EyesIris);
            orgColorEyesSclera = creature.GetColor(Creature.ColorModifier.EyesSclera);
            orgColorSkin = creature.GetColor(Creature.ColorModifier.Skin);
            timerSlow = timerSlowing;
            totalTimeOfFreezeRagdoll = timerFreezeRagdoll;
            timerFreeze = totalTimeOfFreezeRagdoll - timeOfFreezing;
            targetColorHair = colorFreeze;
            targetColorHairSecondary = colorFreeze;
            targetColorHairSpecular = colorFreeze;
            targetColorEyesIris = colorFreeze;
            targetColorEyesSclera = colorFreeze;
            targetColorSkin = colorFreeze;
            /*targetMaterials = new List<Material>();
            freezeMaterial = handleFreezeMaterial.WaitForCompletion();
            freezeMaterial = Instantiate(freezeMaterial);
            Debug.Log("Freeze Arrow : Freezing Mat name: " + freezeMaterial.name);
            Debug.Log("Freeze Arrow : Freezing Mat Properties Name : ");
            foreach(string names in  freezeMaterial.GetTexturePropertyNames())
            {
                Debug.Log("Freeze Arrow : Properties Name : " + names);
            }
            Debug.Log("Freeze Arrow : Freezing Mat Shader : " + freezeMaterial.shader.name);
            Debug.Log("Freeze Arrow : Freezing Mat Shader Property : ");
            for(int i = 0; i < freezeMaterial.shader.GetPropertyCount(); i++)
            {
                Debug.Log("Freeze Arrow : Properties Name : " + freezeMaterial.shader.GetPropertyName(i));
            }
            //orgBlendFreezeValue = freezeMaterial.GetFloat("BlendValue");
            creature.renderers.ForEach(i =>
            {
                targetMaterials.Add(i.renderer.material);
                foreach(string name in i.renderer.material.GetTexturePropertyNames())
                {
                    Debug.Log("Freeze Arrow : Texture Properties Name : " + name);
                }
                // Does not work
                targetTexture = i.renderer.material.mainTexture != null ? i.renderer.material.mainTexture : i.renderer.material.GetTexture("_BaseMap");
                targetTexture = i.renderer.material.mainTexture;
                Debug.Log("Freeze Arrow : Freezing Ori texture : " + targetTexture.name);
                i.renderer.material = freezeMaterial;
                
                i.renderer.material.SetTexture("_TextureSecond", targetTexture);
                Debug.Log("Freeze Arrow : Freezing texture applied : " + i.renderer.material.GetTexture("_TextureSecond").name);
            });*/
            //creature.renderers.ForEach(i => Catalog.LoadAssetAsync<Material>("Neeshka.TestFreeze.Freeze_Shader", mat => i.renderer.material = mat, "handleFreezeMaterial"));
        }

        public void UpdateSpeed()
        {
            creature.animator.speed = animatorSpeed;
            //creature.locomotion.speed = locomotionSpeed;
            creature.ragdoll.creature.brain.instance.GetModule<BrainModuleSpeak>().audioPitchRange = speakPitchRange;
            /*creature.renderers.ForEach(i =>
            {
                i.renderer.material.SetFloat("_BlendValue", blendFreezeValue);
            });*/
            creature.SetColor(colorHair, Creature.ColorModifier.Hair, true);
            creature.SetColor(colorHairSecondary, Creature.ColorModifier.HairSecondary, true);
            creature.SetColor(colorHairSpecular, Creature.ColorModifier.HairSpecular, true);
            creature.SetColor(colorEyesIris, Creature.ColorModifier.EyesIris, true);
            creature.SetColor(colorEyesSclera, Creature.ColorModifier.EyesSclera, true);
            creature.SetColor(colorSkin, Creature.ColorModifier.Skin, true);
        }

        public void Update()
        {
            if (isFrozen != true)
            {
                timerSlow -= Time.deltaTime / timeOfFreezing;
                timerSlow = Mathf.Clamp(timerSlow, 0, timeOfFreezing);
                animatorSpeed = Mathf.Lerp(targetAnimatorSpeed, orgAnimatorSpeed, timerSlow);
                //locomotionSpeed = Mathf.Lerp(targetLocomotionSpeed, orgLocomotionSpeed, timerSlow);
                speakPitchRange = Vector2.Lerp(targetSpeakPitchRange, orgSpeakPitchRange, timerSlow);
                blendFreezeValue = Mathf.Lerp(targetBlendFreezeValue, orgBlendFreezeValue, timerSlow);
                //Debug.Log("Freeze Arrow : blendFreezeValue : " + blendFreezeValue);
                colorHair = Color.Lerp(targetColorHair, orgColorHair, timerSlow);
                colorHairSecondary = Color.Lerp(targetColorHairSecondary, orgColorHairSecondary, timerSlow);
                colorHairSpecular = Color.Lerp(targetColorHairSpecular, orgColorHairSpecular, timerSlow);
                colorEyesIris = Color.Lerp(targetColorEyesIris, orgColorEyesIris, timerSlow);
                colorEyesSclera = Color.Lerp(targetColorEyesSclera, orgColorEyesSclera, timerSlow);
                colorSkin = Color.Lerp(targetColorSkin, orgColorSkin, timerSlow);

                UpdateSpeed();
                //Debug.Log("Freeze Arrow : Freezing : " + timerSlow.ToString("00.00"));
            }
            if (timerSlow <= 0.0f && isFrozen != true)
            {
                brainId = creature.ragdoll.creature.brain.instance.id;
                isFrozen = true;
                creature.brain.Stop();
                creature.StopAnimation();
                creature.brain.StopAllCoroutines();
                creature.locomotion.MoveStop();
                //creature.brain.AddNoStandUpModifier(this);
                foreach (RagdollPart ragdollPart in creature.ragdoll.parts)
                {
                    ragdollPart.rb.constraints = RigidbodyConstraints.FreezeAll;
                }
                //Debug.Log("Freeze Arrow : Is Frozen");
            }
            if (isFrozen == true && endOfFreeze != true)
            {
                timerFreeze = Mathf.Clamp(timerFreeze, 0, timerFreeze);
                timerFreeze -= Time.deltaTime;
                //Debug.Log("Freeze Arrow : IsFrozen : " + timerFreeze.ToString("00.00"));
                if (timerFreeze <= 0.0f)
                {
                    endOfFreeze = true;
                    //Debug.Log("Freeze Arrow : End of Freeze");
                }
            }
            if (endOfFreeze)
            {
                Disable();
            }
        }
        public void Disable()
        {
            creature.animator.speed = orgAnimatorSpeed;
            //creature.locomotion.speedModifiers = orgLocomotionSpeed;
            creature.brain.Load(brainId);
            foreach (RagdollPart ragdollPart in creature.ragdoll.parts)
            {
                ragdollPart.rb.constraints = RigidbodyConstraints.None;
                ragdollPart.ragdoll.RemovePhysicModifier(this);
            }
            //creature.brain.RemoveNoStandUpModifier(this);
            creature.ragdoll.creature.brain.instance.GetModule<BrainModuleSpeak>().audioPitchRange = orgSpeakPitchRange;
            int index = 0;
            /*creature.renderers.ForEach(i =>
            {
                i.renderer.material = targetMaterials[index];
                index++;
            });*/
            creature.SetColor(orgColorHair, Creature.ColorModifier.Hair, true);
            creature.SetColor(orgColorHairSecondary, Creature.ColorModifier.HairSecondary, true);
            creature.SetColor(orgColorHairSpecular, Creature.ColorModifier.HairSpecular, true);
            creature.SetColor(orgColorEyesIris, Creature.ColorModifier.EyesIris, true);
            creature.SetColor(orgColorEyesSclera, Creature.ColorModifier.EyesSclera, true);
            creature.SetColor(orgColorSkin, Creature.ColorModifier.Skin, true);
            Destroy(this);
            //creature.speak.GetField("audioSource") as AudioSource).pitch = orgSpeakPitch;
        }
    }

    public class SlowBehaviour : MonoBehaviour
    {
        private Creature creature;
        private string brainId;
        private float orgAnimatorSpeed;
        //private float orgLocomotionSpeed;
        private bool hasStarted = false;
        private bool isSlowed = false;
        private bool endOfSlow = false;
        private float timerStart;
        private float orgTimerStart;
        private float timerDuration;
        private float orgTimerDuration;
        private float timerBlend;
        private float orgTimerBlend;
        private float ratioSlow;
        private float orgRatioSlow;
        private bool playVFX;
        private bool restoreVelocity;
        private Vector3 orgCreatureVelocity;
        private Vector3 orgCreatureAngularVelocity;
        private List<Vector3> orgCreatureVelocityPart;
        private List<Vector3> orgCreatureAngularVelocityPart;
        private List<float> orgCreatureDragPart;
        private List<float> orgCreatureAngularDragPart;
        private float orgLocomotionDrag;
        private float orgLocomotionAngularDrag;
        private float factor = 10f;

        public void Init(float start, float duration, float ratio, bool restoreVelocityAfterEffect = true, float blendDuration = 0f, bool playEffect = false)
        {
            timerStart = start;
            orgTimerStart = start;
            timerDuration = duration;
            orgTimerDuration = duration;
            ratioSlow = ratio;
            orgRatioSlow = ratio;
            timerBlend = blendDuration;
            orgTimerBlend = blendDuration;
            playVFX = playEffect;
            restoreVelocity = restoreVelocityAfterEffect;
        }

        public void Awake()
        {
            creature = GetComponent<Creature>();
            creature.OnDespawnEvent += time =>
            {
                if (time == EventTime.OnStart)
                {
                    Dispose();
                }
            };
            creature.OnKillEvent += Creature_OnKillEvent;
        }

        private void Creature_OnKillEvent(CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart)
            {
                Dispose();
            }
        }

        public void Update()
        {
            // Wait for the start
            if (hasStarted != true)
            {
                timerStart -= Time.deltaTime;
                timerStart = Mathf.Clamp(timerStart, 0, orgTimerStart);
                if (timerStart <= 0.0f)
                {
                    brainId = creature.ragdoll.creature.brain.instance.id;
                    orgAnimatorSpeed = creature.animator.speed;
                    //orgLocomotionSpeed = creature.locomotion.speed;
                    orgCreatureVelocity = creature.locomotion.rb.velocity;
                    orgCreatureAngularVelocity = creature.locomotion.rb.angularVelocity;
                    orgCreatureVelocityPart = creature.ragdoll.parts.Select(part => part.rb.velocity).ToList();
                    orgCreatureAngularVelocityPart = creature.ragdoll.parts.Select(part => part.rb.angularVelocity).ToList();
                    orgCreatureDragPart = creature.ragdoll.parts.Select(part => part.rb.drag).ToList();
                    orgCreatureAngularDragPart = creature.ragdoll.parts.Select(part => part.rb.angularDrag).ToList();
                    orgLocomotionDrag = creature.locomotion.rb.drag;
                    orgLocomotionAngularDrag = creature.locomotion.rb.angularDrag;
                    hasStarted = true;
                }
            }

            // Slow is blended
            if (hasStarted == true && isSlowed != true)
            {
                if (orgTimerBlend != 0f)
                {
                    timerBlend -= Time.deltaTime / orgTimerBlend;
                    timerBlend = Mathf.Clamp(timerBlend, 0, orgTimerBlend);
                }
                else
                {
                    timerBlend = 0f;
                }

                creature.animator.speed = Mathf.Lerp(orgAnimatorSpeed * ratioSlow / factor, orgAnimatorSpeed, timerBlend);
                //creature.locomotion.speed = Mathf.Lerp(orgAnimatorSpeed * ratioSlow / factor, orgLocomotionSpeed, timerBlend);
                creature.locomotion.rb.velocity = new Vector3(Mathf.Lerp(orgCreatureVelocity.x * ratioSlow / factor, orgCreatureVelocity.x, timerBlend),
                                                            Mathf.Lerp(orgCreatureVelocity.y * ratioSlow / factor, orgCreatureVelocity.y, timerBlend),
                                                            Mathf.Lerp(orgCreatureVelocity.z * ratioSlow / factor, orgCreatureVelocity.z, timerBlend));
                creature.locomotion.rb.angularVelocity = new Vector3(Mathf.Lerp(orgCreatureAngularVelocity.x * ratioSlow / factor, orgCreatureAngularVelocity.x, timerBlend),
                                                                    Mathf.Lerp(orgCreatureAngularVelocity.y * ratioSlow / factor, orgCreatureAngularVelocity.y, timerBlend),
                                                                    Mathf.Lerp(orgCreatureAngularVelocity.z * ratioSlow / factor, orgCreatureAngularVelocity.z, timerBlend));
                creature.locomotion.rb.drag = Mathf.Lerp(factor * 100f, orgLocomotionDrag, timerBlend);
                creature.locomotion.rb.angularDrag = Mathf.Lerp(factor * 100f, orgLocomotionAngularDrag, timerBlend);
                for (int i = creature.ragdoll.parts.Count - 1; i >= 0; --i)
                {
                    creature.ragdoll.parts[i].ragdoll.SetPhysicModifier(this, 0, 0, factor * 100f, factor * 100f);
                    creature.ragdoll.parts[i].rb.velocity = new Vector3(Mathf.Lerp(orgCreatureVelocityPart[i].x * ratioSlow / factor, orgCreatureVelocityPart[i].x, timerBlend),
                                                                        Mathf.Lerp(orgCreatureVelocityPart[i].x * ratioSlow / factor, orgCreatureVelocityPart[i].y, timerBlend),
                                                                        Mathf.Lerp(orgCreatureVelocityPart[i].x * ratioSlow / factor, orgCreatureVelocityPart[i].z, timerBlend));
                    creature.ragdoll.parts[i].rb.angularVelocity = new Vector3(Mathf.Lerp(orgCreatureAngularVelocityPart[i].x * ratioSlow / factor, orgCreatureAngularVelocityPart[i].x, timerBlend),
                                                                                Mathf.Lerp(orgCreatureAngularVelocityPart[i].y * ratioSlow / factor, orgCreatureAngularVelocityPart[i].y, timerBlend),
                                                                                Mathf.Lerp(orgCreatureAngularVelocityPart[i].z * ratioSlow / factor, orgCreatureAngularVelocityPart[i].z, timerBlend));
                    creature.ragdoll.parts[i].rb.drag = Mathf.Lerp(factor * 100f, orgCreatureDragPart[i], timerBlend);
                    creature.ragdoll.parts[i].rb.angularDrag = Mathf.Lerp(factor * 100f, orgCreatureAngularDragPart[i], timerBlend);
                }

                if (timerBlend <= 0.0f)
                {
                    isSlowed = true;
                    creature.GetPart(RagdollPart.Type.Torso).rb.freezeRotation = true;
                    //creature.brain.Stop();
                    //creature.brain.StopAllCoroutines();
                    //creature.locomotion.MoveStop();
                    //creature.StopAnimation();
                }
            }

            // Slow is active and wait for the end of the duration
            if (isSlowed == true && endOfSlow != true)
            {
                timerDuration = Mathf.Clamp(timerDuration, 0, orgTimerDuration);
                timerDuration -= Time.deltaTime;
                if (timerDuration <= 0.0f)
                {
                    endOfSlow = true;
                }
            }
            if (endOfSlow == true)
            {
                Dispose();
            }
        }
        public void Dispose()
        {
            if (creature != null)
            {
                creature.animator.speed = orgAnimatorSpeed;
                //creature.locomotion.speed = orgLocomotionSpeed;
                foreach (RagdollPart ragdollPart in creature.ragdoll.parts)
                {
                    ragdollPart.ragdoll.RemovePhysicModifier(this);
                }
                creature.GetPart(RagdollPart.Type.Torso).rb.freezeRotation = false;
                if (restoreVelocity && hasStarted)
                {
                    creature.locomotion.rb.velocity = orgCreatureVelocity;
                    creature.locomotion.rb.angularVelocity = orgCreatureAngularVelocity;
                    creature.locomotion.rb.drag = orgLocomotionDrag;
                    creature.locomotion.rb.angularDrag = orgLocomotionAngularDrag;
                    for (int i = creature.ragdoll.parts.Count - 1; i >= 0; --i)
                    {
                        creature.ragdoll.parts[i].rb.velocity = orgCreatureVelocityPart[i];
                        creature.ragdoll.parts[i].rb.angularVelocity = orgCreatureAngularVelocityPart[i];
                        creature.ragdoll.parts[i].rb.drag = orgCreatureDragPart[i];
                        creature.ragdoll.parts[i].rb.angularDrag = orgCreatureAngularDragPart[i];
                    }
                }
            }
            creature.OnKillEvent -= Creature_OnKillEvent;
            //creature.brain.Load(brainId);
            Destroy(this);
        }
    }
    public class LightningBeam : MonoBehaviour
    {
        protected EffectData beamEffectData;
        protected EffectInstance beamEffect;
        public LayerMask beamMask = 144718849;
        public float beamForce = 50f;
        protected SpellCastCharge imbueSpell;
        public float imbueAmount = 10f;
        public float damageDelay = 0.5f;
        public float damageAmount = 10f;
        public AnimationCurve beamForceCurve = new AnimationCurve(new Keyframe[3]
        {
      new Keyframe(0.0f, 10f),
      new Keyframe(0.05f, 25f),
      new Keyframe(0.1f, 10f)
        });
        public float beamHandPositionSpringMultiplier = 1f;
        public float beamHandPositionDamperMultiplier = 1f;
        public float beamHandRotationSpringMultiplier = 0.2f;
        public float beamHandRotationDamperMultiplier = 0.6f;
        public float beamHandLocomotionVelocityCorrectionMultiplier = 1f;
        public float beamLocomotionPushForce = 10f;
        public float beamCastMinHandAngle = 20f;
        public string beamImpactEffectId;
        protected EffectData beamImpactEffectData;
        public float chainRadius = 4f;
        public float chainDelay = 1f;
        protected EffectData electrocuteEffectData;
        protected EffectData chainEffectData;
        public bool beamActive;
        public Ray beamRay;
        public Transform beamStart;
        public Transform beamHitPoint;
        protected float lastDamageTick;
        protected float lastChainTick;
        protected Collider[] collidersHit;
        protected HashSet<Creature> creaturesHit;
        public float beamHookDamper = 150f;
        public float beamHookSpring = 1000f;
        public float beamHookSpeed = 20f;
        public float beamHookMaxAngle = 30f;
        public float zapInterval = 0.7f;
        private LightningHookMergeUp hookedCreature;
        private float lastZap;
        private EffectInstance beamImpactEffect;
        ParticleSystem.CollisionModule collisionModule = new ParticleSystem.CollisionModule();
        ParticleSystem.CollisionModule childCollisionModule = new ParticleSystem.CollisionModule();
        public bool isCasting;
        private SpellCastLightning instance;
        private float duration;
        private float startTime;
        public void Init(Vector3 origin, Vector3 directionOfBeam, float durationOfBeam)
        {
            beamEffectData = Catalog.GetData<EffectData>("SpellLightningMergeBeam");
            imbueSpell = Catalog.GetData<SpellCastCharge>("Lightning");
            chainEffectData = Catalog.GetData<EffectData>("SpellLightningBolt");
            electrocuteEffectData = Catalog.GetData<EffectData>("ImbueLightningRagdoll");
            beamImpactEffectData = Catalog.GetData<EffectData>("SpellLightningMergeBeamImpact");
            collidersHit = new Collider[20];
            beamForceCurve.postWrapMode = WrapMode.Loop;
            creaturesHit = new HashSet<Creature>();
            beamRay.origin = origin;
            beamRay.direction = directionOfBeam;
            instance = new SpellCastLightning();
            duration = durationOfBeam;
            startTime = Time.time;
            Fire(true);
        }

        public void Update()
        {
            if (Time.time - lastZap > zapInterval)
            {
                lastZap = Time.time + Random.Range(-0.5f, 0.5f);
                instance.ShockInRadius(beamRay.origin, 3f);
            }
            //End the beam
            if (startTime < Time.time - duration)
            {
                Dispose();
                return;
            }
            if (!beamActive)
            {
                beamActive = true;
                beamEffect = beamEffectData.Spawn(beamStart);
                EffectInstance effectInstance = beamEffect;
                if (effectInstance != null)
                {
                    effectInstance.SetIntensity(1f);
                }
                EffectInstance effectInstance2 = beamEffect;
                if (effectInstance2 != null)
                {
                    effectInstance2.Play();
                }
                if (beamEffect != null)
                {
                    foreach (EffectParticle effectParticle in beamEffect.effects.OfType<EffectParticle>())
                    {

                        collisionModule = effectParticle.rootParticleSystem.collision;
                        collisionModule.collidesWith = beamMask;
                        foreach (EffectParticleChild child in effectParticle.childs)
                        {
                            childCollisionModule = child.particleSystem.collision;
                            childCollisionModule.collidesWith = beamMask;
                        }
                    }
                }
                beamStart.transform.SetPositionAndRotation(beamRay.origin, Quaternion.LookRotation(beamRay.direction));
            }
            if (!beamActive)
                return;
            beamStart.transform.SetPositionAndRotation(beamRay.origin, Quaternion.Slerp(beamStart.transform.rotation, Quaternion.LookRotation(beamRay.direction), Time.deltaTime * 3f));
            if (hookedCreature && Vector3.Angle(beamRay.direction, hookedCreature.creature.ragdoll.GetPart(RagdollPart.Type.Torso).transform.position - beamRay.origin) > beamHookMaxAngle)
            {
                hookedCreature.Unhook();
                hookedCreature = null;
            }
            RaycastHit hitInfo;
            if (!Physics.SphereCast(beamRay, 0.1f, out hitInfo, 20f, beamMask, QueryTriggerInteraction.Ignore))
            {
                beamHitPoint.SetPositionAndRotation(beamRay.GetPoint(20f), Quaternion.LookRotation(-beamRay.direction));
                if (beamImpactEffect != null)
                {
                    beamImpactEffect.End(false, -1f);
                    beamImpactEffect = null;
                }
                return;
            }
            beamHitPoint.SetPositionAndRotation(hitInfo.point + beamRay.direction * 5f, Quaternion.LookRotation(-beamRay.direction));
            if (beamEffectData != null && beamImpactEffect == null)
            {
                beamImpactEffect = beamEffectData.Spawn(beamHitPoint);
                beamImpactEffect.Play();
            }
            if (hitInfo.collider.GetComponentInParent<Creature>() != null)
            {
                Creature componentInParent = hitInfo.collider.GetComponentInParent<Creature>();
                if (componentInParent != null)
                {
                    creaturesHit.Add(componentInParent);
                    componentInParent.ragdoll.AddPhysicToggleModifier(this);
                }
            }
            if (hitInfo.rigidbody == null)
                return;
            CollisionHandler component = hitInfo.rigidbody.GetComponent<CollisionHandler>();
            if (component == null)
                return;
            component.rb.AddForceAtPosition(beamRay.direction * beamForce, hitInfo.point, ForceMode.VelocityChange);
            if (component.isItem)
            {
                ColliderGroup componentInParent2 = hitInfo.collider.GetComponentInParent<ColliderGroup>();
                if (componentInParent2 != null && componentInParent2.imbue)
                {
                    componentInParent2.imbue.Transfer(imbueSpell, imbueAmount * Time.deltaTime);
                    return;
                }
            }
            else
            {
                RagdollPart ragdollPart = component.ragdollPart;
                if (ragdollPart == null || !(ragdollPart.ragdoll.creature != Player.local.creature))
                    return;
                Creature creature = ragdollPart.ragdoll.creature;
                if (creature != null)
                {
                    if (Time.time - lastDamageTick > damageDelay)
                    {
                        lastDamageTick = Time.time;
                        creature.Damage(new CollisionInstance(new DamageStruct(DamageType.Energy, damageAmount)
                        {
                            pushLevel = 2
                        })
                        {
                            casterHand = Player.local.creature.handRight.caster,
                            contactPoint = hitInfo.point,
                            contactNormal = hitInfo.normal,
                            targetColliderGroup = hitInfo.collider.GetComponentInParent<ColliderGroup>()
                        });
                        creature.TryElectrocute(1f, 5f, true, false, electrocuteEffectData);
                        TryHookCreature(creature);
                    }
                    if (Time.time - lastChainTick <= chainDelay)
                        return;
                    lastChainTick = Time.time;
                    Chain(creature);
                    creaturesHit.Add(creature);
                }
            }
        }

        private void TryHookCreature(Creature creature)
        {
            LightningHookMergeUp lightningHook = hookedCreature;
            if (((lightningHook != null) ? lightningHook.creature : null) == creature)
            {
                return;
            }
            LightningHookMergeUp lightningHook2 = hookedCreature;
            if (lightningHook2 != null)
            {
                lightningHook2.Unhook();
            }
            hookedCreature = creature.gameObject.GetOrAddComponent<LightningHookMergeUp>();
            hookedCreature.Hook(this);
        }

        private void Chain(Creature creature)
        {
            RagdollPart part = creature.ragdoll.GetPart(RagdollPart.Type.Torso);
            int num = Physics.OverlapSphereNonAlloc(part.transform.position, chainRadius, collidersHit, LayerMask.GetMask(new string[]
            {
                "BodyLocomotion"
            }), QueryTriggerInteraction.Ignore);
            if (num <= 0)
                return;
            Creature component = collidersHit[Random.Range(0, num)].GetComponent<Creature>();
            if (component == null)
                return;
            RagdollPart part2 = component.ragdoll.GetPart(RagdollPart.Type.Torso);
            Transform transform = creature.transform;
            EffectInstance effectInstance = chainEffectData.Spawn(transform.position, transform.rotation);
            effectInstance.SetSource(creature.ragdoll.GetPart(RagdollPart.Type.Torso).transform);
            effectInstance.SetTarget(part2.transform);
            effectInstance.Play();
            component.TryElectrocute(1f, 5f, true, false, electrocuteEffectData);
            component.TryPush(Creature.PushType.Magic, (part2.transform.position - part.transform.position).normalized * 2f, 1, 0);
        }

        public void Fire(bool active)
        {
            if (active)
            {
                EffectInstance effectInstance = beamEffect;
                if (effectInstance != null)
                {
                    effectInstance.End(false, -1f);
                }
                beamEffect = null;
                if (beamStart == null)
                {
                    beamStart = new GameObject("Beam Target").transform;
                }
                if (beamHitPoint == null)
                {
                    beamHitPoint = new GameObject("Beam Hit").transform;
                }
            }
            else
            {
                EffectInstance effectInstance2 = beamEffect;
                if (effectInstance2 != null)
                {
                    effectInstance2.End(false, -1f);
                }
                beamEffect = null;
                foreach (Creature creature in creaturesHit)
                {
                    creature.ragdoll.RemovePhysicToggleModifier(this);
                }
                if (beamImpactEffect != null)
                {
                    beamImpactEffect.End(false, -1f);
                    beamImpactEffect = null;
                }
                LightningHookMergeUp lightningHook = hookedCreature;
                if (lightningHook != null)
                {
                    lightningHook.Unhook();
                }
                hookedCreature = null;
            }
            beamActive = false;
        }

        private void Dispose()
        {
            Fire(false);
            Destroy(this);
        }
    }
    private class LightningHookMergeUp : MonoBehaviour
    {
        public Creature creature;
        private SpringJoint joint;
        private Rigidbody jointRb;
        private LightningBeam beamSpell;
        private bool active;

        private void Awake()
        {
            creature = GetComponent<Creature>();
            jointRb = new GameObject(creature.name + " Lightning Hook Joint RB").AddComponent<Rigidbody>();
            jointRb.isKinematic = true;
            jointRb.useGravity = false;
            creature.OnDespawnEvent += time =>
            {
                if (time != EventTime.OnStart)
                    return;
                Destroy(this);
            };
        }
        public void Hook(LightningBeam hookingbeam)
        {
            if (active)
                return;
            beamSpell = hookingbeam;
            RagdollPart part = creature.ragdoll.GetPart(RagdollPart.Type.Torso);
            creature.ragdoll.SetState(Ragdoll.State.Destabilized);
            creature.brain.AddNoStandUpModifier(this);
            creature.ragdoll.AddPhysicToggleModifier(this);
            creature.ragdoll.SetPhysicModifier(this, 0.0f);
            jointRb.transform.position = part.transform.position;
            joint = part.rb.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedBody = jointRb;
            joint.connectedAnchor = Vector3.zero;
            joint.anchor = Vector3.zero;
            joint.spring = beamSpell.beamHookSpring;
            joint.damper = beamSpell.beamHookDamper;
            active = true;
        }

        public void Unhook()
        {
            if (!active)
                return;
            active = false;
            Destroy(joint);
            creature.brain.RemoveNoStandUpModifier(this);
            creature.ragdoll.RemovePhysicToggleModifier(this);
            creature.ragdoll.RemovePhysicModifier(this);
        }
        private void OnDestroy() => Destroy(joint);
    }
    #endregion
}