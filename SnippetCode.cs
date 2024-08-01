using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using ThunderRoad.Skill.Spell;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


namespace SnippetCode
{
    public static class SnippetCode
    {
    }
    /*// C# 11 feature:
    public class GenericAttribute<T> : Attribute { }
    public class GenericType<T>
    {
        [GenericAttribute<string>()]
        public string Method() => default;
    }*/
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
    /// <param name="layer">Layer</param>
    /// <returns>An array of RaycastHit containing all hits within the specified cone-shaped region.</returns>
    public static RaycastHit[] ConeCastAll(this Vector3 origin, float maxRadius, Vector3 direction, float maxDistance,
        float coneAngle, int layer = Physics.DefaultRaycastLayers)
    {
        RaycastHit[] sphereCastHits = Physics.SphereCastAll(origin, maxRadius, direction, maxDistance, layer,
            QueryTriggerInteraction.Ignore);
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
                bool hitRigidbody = sphereCastHits[i].rigidbody is Rigidbody physicBody
                                    && Vector3.Angle(direction, physicBody.transform.position - origin) <
                                    coneAngle * multiplier;

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

    public static Vector3 PosAboveBackOfHand(this RagdollHand hand, float factor = 1f) => hand.transform.position -
        hand.transform.right * 0.1f * factor + hand.transform.forward * 0.2f * factor;

    public static Quaternion GetFlyDirRefLocalRotation(this Item item) =>
        Quaternion.Inverse(item.transform.rotation) * item.flyDirRef.rotation;

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

    public static void HapticTick(this RagdollHand hand, float intensity = 1, float frequency = 10) =>
        PlayerControl.input.Haptic(hand.side, intensity, frequency);

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

    public static bool XBigger(this Vector3 vec) => vec.x > vec.y && vec.x > vec.z;

    public static bool YBigger(this Vector3 vec) => vec.y > vec.x && vec.y > vec.z;

    public static bool ZBigger(this Vector3 vec) => vec.z > vec.x && vec.z > vec.y;
    public static bool XSmaller(this Vector3 vec) => vec.x < vec.y && vec.x < vec.z;

    public static bool YSmaller(this Vector3 vec) => vec.y < vec.x && vec.y < vec.z;

    public static bool ZSmaller(this Vector3 vec) => vec.z < vec.x && vec.z < vec.y;

    public static float XAbsBiggerValue(this Vector3 vec) =>
        Mathf.Abs(vec.x) > Mathf.Abs(vec.y) && Mathf.Abs(vec.x) > Mathf.Abs(vec.z) ? vec.x : float.MinValue;

    public static float YAbsBiggerValue(this Vector3 vec) =>
        Mathf.Abs(vec.y) > Mathf.Abs(vec.x) && Mathf.Abs(vec.y) > Mathf.Abs(vec.z) ? vec.y : float.MinValue;

    public static float ZAbsBiggerValue(this Vector3 vec) =>
        Mathf.Abs(vec.z) > Mathf.Abs(vec.x) && Mathf.Abs(vec.z) > Mathf.Abs(vec.y) ? vec.z : float.MinValue;

    public static Vector3 Velocity(this RagdollHand hand) =>
        Player.local.transform.rotation * hand.playerHand.controlHand.GetHandVelocity();

    public static RagdollHand DominantHand()
    {
        return Player.currentCreature.GetHand(GameManager.options.uiPointerHand);
    }

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

    public static List<Creature> CreaturesInRadius(this Vector3 position, float radius, bool targetAliveCreature = true,
        bool targetDeadCreature = false, bool targetPlayer = false)
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

    public static List<Creature> CreaturesInConeRadius(this Vector3 position, float radius, Vector3 directionOfCone,
        float angleOfCone, bool targetAliveCreature = true, bool targetDeadCreature = false, bool targetPlayer = false)
    {
        List<Creature> creatureDetected = new List<Creature>();
        for (int i = Creature.allActive.Count - 1; i >= 0; i--)
        {
            if (((Creature.allActive[i].GetChest() - position).sqrMagnitude < radius * radius)
                && (targetAliveCreature ? true : Creature.allActive[i].state == Creature.State.Dead)
                && (targetDeadCreature ? true : Creature.allActive[i].state != Creature.State.Dead)
                && (targetPlayer ? true : !Creature.allActive[i].isPlayer)
                && (Vector3.Angle(Creature.allActive[i].GetChest() - position, directionOfCone) <= (angleOfCone / 2f)))
                creatureDetected.Add(Creature.allActive[i]);
        }

        return creatureDetected;
    }

    /// <summary>
    /// Return the farthest creature inside a cone radius
    /// </summary>
    /// <param name="position">Position to check from</param>
    /// <param name="radius">Radius of the check</param>
    /// <param name="directionOfCone">Direction of the center of the cone</param>
    /// <param name="angleOfCone">Spread angle of the cone</param>
    /// <param name="targetAliveCreature">Target Alive creatures</param>
    /// <param name="targetDeadCreature">Target Dead creatures</param>
    /// <param name="targetPlayer">Target the player</param>
    /// <param name="creatureToExclude">Creature to exclude in the check</param>
    /// <returns></returns>
    public static Creature FarthestCreatureInConeRadius(this Vector3 position, float radius, Vector3 directionOfCone,
        float angleOfCone, bool targetAliveCreature = true, bool targetDeadCreature = false, bool targetPlayer = false,
        Creature creatureToExclude = null)
    {
        List<Creature> creatureDetected = new List<Creature>();
        for (int i = Creature.allActive.Count - 1; i >= 0; i--)
        {
            if (Creature.allActive[i] != creatureToExclude && ((Creature.allActive[i].GetChest() - position)
                                                               .sqrMagnitude < radius * radius)
                                                           && (targetAliveCreature || Creature.allActive[i].state ==
                                                               Creature.State.Dead)
                                                           && (targetDeadCreature || Creature.allActive[i].state !=
                                                               Creature.State.Dead)
                                                           && (targetPlayer || !Creature.allActive[i].isPlayer)
                                                           && (Vector3.Angle(
                                                               Creature.allActive[i].GetChest() - position,
                                                               directionOfCone) <= (angleOfCone / 2f)))
                creatureDetected.Add(Creature.allActive[i]);
        }

        if (creatureDetected.Count <= 0)
            return null;
        float lastRadius = 0f;
        Creature lastCreature = null;
        foreach (Creature creature in creatureDetected)
        {
            float thisRadius = (position - creature.GetChest()).sqrMagnitude;
            if (!(thisRadius >= lastRadius * lastRadius)) continue;
            lastRadius = thisRadius;
            lastCreature = creature;
        }

        return lastCreature;
    }

    /// <summary>
    /// Return the closest creature inside a cone radius
    /// </summary>
    /// <param name="position">Position to check from</param>
    /// <param name="radius">Radius of the check</param>
    /// <param name="directionOfCone">Direction of the center of the cone</param>
    /// <param name="angleOfCone">Spread angle of the cone</param>
    /// <param name="targetAliveCreature">Target Alive creatures</param>
    /// <param name="targetDeadCreature">Target Dead creatures</param>
    /// <param name="targetPlayer">Target the player</param>
    /// <param name="creatureToExclude">Creature to exclude in the check</param>
    /// <returns></returns>
    public static Creature ClosestCreatureInConeRadius(this Vector3 position, float radius, Vector3 directionOfCone,
        float angleOfCone, bool targetAliveCreature = true, bool targetDeadCreature = false, bool targetPlayer = false,
        Creature creatureToExclude = null)
    {
        List<Creature> creatureDetected = new List<Creature>();
        for (int i = Creature.allActive.Count - 1; i >= 0; i--)
        {
            if (Creature.allActive[i] != creatureToExclude && ((Creature.allActive[i].GetChest() - position)
                                                               .sqrMagnitude < radius * radius)
                                                           && (targetAliveCreature || Creature.allActive[i].state ==
                                                               Creature.State.Dead)
                                                           && (targetDeadCreature || Creature.allActive[i].state !=
                                                               Creature.State.Dead)
                                                           && (targetPlayer || !Creature.allActive[i].isPlayer)
                                                           && (Vector3.Angle(
                                                               Creature.allActive[i].GetChest() - position,
                                                               directionOfCone) <= (angleOfCone / 2f)))
                creatureDetected.Add(Creature.allActive[i]);
        }

        if (creatureDetected.Count <= 0)
            return null;


        float lastRadius = 0f;
        Creature lastCreature = null;
        foreach (Creature creature in creatureDetected)
        {
            float thisRadius = (position - creature.GetChest()).sqrMagnitude;
            if (!(thisRadius <= lastRadius * lastRadius)) continue;
            lastRadius = thisRadius;
            lastCreature = creature;
        }

        return lastCreature;
    }

    /// <summary>
    /// Return the most centered creature inside a cone radius
    /// </summary>
    /// <param name="position">Position to check from</param>
    /// <param name="radius">Radius of the check</param>
    /// <param name="directionOfCone">Direction of the center of the cone</param>
    /// <param name="angleOfCone">Spread angle of the cone</param>
    /// <param name="targetAliveCreature">Target Alive creatures</param>
    /// <param name="targetDeadCreature">Target Dead creatures</param>
    /// <param name="targetPlayer">Target the player</param>
    /// <param name="creatureToExclude">Creature to exclude in the check</param>
    /// <returns></returns>
    public static Creature CenteredCreatureInConeRadius(this Vector3 position, float radius, Vector3 directionOfCone,
        float angleOfCone, bool targetAliveCreature = true, bool targetDeadCreature = false, bool targetPlayer = false,
        Creature creatureToExclude = null)
    {
        List<Creature> creatureDetected = new List<Creature>();
        for (int i = Creature.allActive.Count - 1; i >= 0; i--)
        {
            if (Creature.allActive[i] != creatureToExclude && ((Creature.allActive[i].GetChest() - position)
                                                               .sqrMagnitude < radius * radius)
                                                           && (targetAliveCreature
                                                               ? true
                                                               : Creature.allActive[i].state == Creature.State.Dead)
                                                           && (targetDeadCreature
                                                               ? true
                                                               : Creature.allActive[i].state != Creature.State.Dead)
                                                           && (targetPlayer ? true : !Creature.allActive[i].isPlayer)
                                                           && (Vector3.Angle(
                                                               Creature.allActive[i].GetChest() - position,
                                                               directionOfCone) <= (angleOfCone / 2f)))
                creatureDetected.Add(Creature.allActive[i]);
        }

        if (creatureDetected.Count <= 0)
            return null;

        float lastAngle = Mathf.Infinity;
        Creature lastCreature = null;
        foreach (Creature creature in creatureDetected)
        {
            Vector3 directionTowardT = creature.GetChest() - position;
            float thisAngle = Vector3.Angle(directionTowardT, directionOfCone);
            if (thisAngle <= lastAngle * lastAngle)
            {
                lastAngle = thisAngle;
                lastCreature = creature;
            }
        }

        return lastCreature;
    }

    public static Creature RandomCreatureInRadius(this Vector3 position, float radius, bool targetAliveCreature = true,
        bool targetDeadCreature = false, bool targetPlayer = false, Creature creatureToExclude = null,
        bool includeCreatureExcludedIfDefault = false)
    {
        List<Creature> creatureDetected = new List<Creature>();

        for (int i = Creature.allActive.Count - 1; i >= 0; i--)
        {
            if ((includeCreatureExcludedIfDefault ||
                 Creature.allActive[i] != creatureToExclude)
                && ((Creature.allActive[i].GetChest() - position).sqrMagnitude < radius * radius)
                && (targetAliveCreature || Creature.allActive[i].state == Creature.State.Dead)
                && (targetDeadCreature || Creature.allActive[i].state != Creature.State.Dead)
                && (targetPlayer || !Creature.allActive[i].isPlayer))
            {
                creatureDetected.Add(Creature.allActive[i]);
            }
        }

        return creatureDetected.Count <= 0 ? null : creatureDetected[Random.Range(0, creatureDetected.Count)];
    }

    /// <summary>
    /// Return the closest creature inside a radius
    /// </summary>
    /// <param name="position">Position to check from</param>
    /// <param name="radius">Radius of the check</param>
    /// <param name="targetAliveCreature">Target Alive creatures</param>
    /// <param name="targetDeadCreature">Target Dead creatures</param>
    /// <param name="targetPlayer">Target the player</param>
    /// <param name="creatureToExclude">Creature to exclude in the check</param>
    /// <returns></returns>
    public static Creature ClosestCreatureInRadius(this Vector3 position, float radius, bool targetAliveCreature = true,
        bool targetDeadCreature = false, bool targetPlayer = false, Creature creatureToExclude = null)
    {
        List<Creature> creatureDetected = new List<Creature>();
        for (int i = Creature.allActive.Count - 1; i >= 0; i--)
        {
            if (Creature.allActive[i] != creatureToExclude && ((Creature.allActive[i].GetChest() - position)
                                                               .sqrMagnitude < radius * radius)
                                                           && (targetAliveCreature
                                                               ? true
                                                               : Creature.allActive[i].state == Creature.State.Dead)
                                                           && (targetDeadCreature
                                                               ? true
                                                               : Creature.allActive[i].state != Creature.State.Dead)
                                                           && (targetPlayer ? true : !Creature.allActive[i].isPlayer))
                creatureDetected.Add(Creature.allActive[i]);
        }

        if (creatureDetected.Count <= 0)
            return null;

        float lastRadius = Mathf.Infinity;
        float thisRadius;
        Creature lastCreature = null;
        foreach (Creature creature in creatureDetected)
        {
            thisRadius = (position - creature.GetChest()).sqrMagnitude;
            if (thisRadius <= lastRadius)
            {
                lastRadius = thisRadius;
                lastCreature = creature;
            }
        }

        return lastCreature;
    }

    /// <summary>
    /// Return the closest creature from a list from a position
    /// </summary>
    /// <param name="position">Position to check from</param>
    /// <param name="targetAliveCreature">Target alive creatures</param>
    /// <param name="targetDeadCreature">Target dead creatures</param>
    /// <param name="targetPlayer">Target the player</param>
    /// <param name="creatureToExclude">Creature to exclude from the check</param>
    /// <param name="targetSlicedCreature">Target sliced creature</param>
    /// <returns></returns>
    public static Creature ClosestCreatureInListFromPosition(this Vector3 position, bool targetAliveCreature = true,
        bool targetDeadCreature = false, bool targetPlayer = false, Creature creatureToExclude = null,
        bool targetSlicedCreature = true)
    {
        List<Creature> creatureDetected = new List<Creature>();
        for (int i = Creature.allActive.Count - 1; i >= 0; i--)
        {
            bool hasPartSliced = false;
            foreach (RagdollPart part in Creature.allActive[i].ragdoll.parts)
            {
                if (part.isSliced)
                    hasPartSliced = true;
            }

            if (Creature.allActive[i] != creatureToExclude
                && (targetAliveCreature || Creature.allActive[i].state == Creature.State.Dead)
                && (targetDeadCreature || Creature.allActive[i].state != Creature.State.Dead)
                && (targetPlayer || !Creature.allActive[i].isPlayer)
                && (targetSlicedCreature || !hasPartSliced))
                creatureDetected.Add(Creature.allActive[i]);
        }

        if (creatureDetected.Count <= 0)
            return null;
        float lastRadius = Mathf.Infinity;
        Creature lastCreature = null;
        foreach (Creature creature in creatureDetected)
        {
            float thisRadius = (position - creature.GetChest()).sqrMagnitude;
            if (thisRadius <= lastRadius)
            {
                lastRadius = thisRadius;
                lastCreature = creature;
            }
        }

        return lastCreature;
    }

    /// <summary>
    /// Return the farthest creature from a list from a position
    /// </summary>
    /// <param name="creatures">List of Creatures</param>
    /// <param name="position">Position to check from</param>
    /// <returns></returns>
    public static Creature FarthestCreatureInListFromPosition(this List<Creature> creatures, Vector3 position)
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

    /// <summary>
    /// Depenetrate the target item
    /// </summary>
    /// <param name="item">Item to depenetrate</param>
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
    /// Release the item held and eventually throw it
    /// </summary>
    /// <param name="item">Item to Release</param>
    /// <param name="throwing">Throw it or not</param>
    public static void Drop(this Item item, bool throwing = false)
    {
        foreach (RagdollHand handler in item.handlers)
        {
            handler.UnGrab(throwing);
        }
    }

    /// <summary>
    /// Get a creature's random part
    /// </summary>
    /// <param name="creature">Creature where the part need to be targeted</param>
    /// <param name="mask">Mask Apply (write it in binary : 0b00011111111111) : 1 means get the part, 0 means don't get the part : in the order of the bit from left to right : 
    /// Tail, RightWing, LeftWing, RightFoot, LeftFoot, RightLeg, LeftLeg, RightHand, LeftHand, RightArm, LeftArm, Torso, Neck, Head</param>
    /// <param name="targetSlicedPart"></param>
    /// <returns></returns>
    public static RagdollPart GetRandomRagdollPart(this Creature creature, int mask = 0b00011111111111,
        bool targetSlicedPart = false)
    {
        List<RagdollPart> ragdollParts = new List<RagdollPart>();
        for (int i = 0; i < creature.ragdoll.parts.Count; i++)
        {
            RagdollPart part = creature.ragdoll.parts[i];
            if ((mask & (int)part.type) > 0 && (targetSlicedPart || !part.isSliced))
                ragdollParts.Add(part);
        }

        if (ragdollParts.Count > 0)
            return ragdollParts[Random.Range(0, ragdollParts.Count)];
        else
            return null;
    }

    /// <summary>
    /// Get a creature's random part excluding the list it already has
    /// </summary>
    /// <param name="creature">Creature where the part need to be targeted</param>
    /// <param name="ragdollParts">List where you exclude the parts</param>
    /// <param name="targetSlicedPart">Get sliced part</param>
    /// <returns></returns>
    public static RagdollPart GetRandomRagdollPartExcludingList(this Creature creature, List<RagdollPart> ragdollParts,
        bool targetSlicedPart = false)
    {
        List<RagdollPart> ragdollPartsFinal = new List<RagdollPart>();
        for (int i = 0; i < creature.ragdoll.parts.Count; i++)
        {
            RagdollPart ragdollPart = creature.ragdoll.parts[i];
            if (ragdollParts.Count == 0 ||
                !ragdollParts.Contains(ragdollPart) && (targetSlicedPart || !ragdollPart.isSliced))
                ragdollPartsFinal.Add(ragdollPart);
        }

        return ragdollPartsFinal.Count > 0 ? ragdollPartsFinal[Random.Range(0, ragdollPartsFinal.Count)] : null;
    }

    /// <summary>
    /// Return if a wave has started or not
    /// </summary>
    /// <returns></returns>
    public static bool ReturnWaveStarted()
    {
        int nbWaveStarted = 0;
        foreach (WaveSpawner waveSpawner in WaveSpawner.instances)
        {
            if (waveSpawner.isRunning)
            {
                nbWaveStarted++;
            }
        }

        return nbWaveStarted != 0;
    }

    public static Damager ReturnSlashDamager(Item item)
    {
        Damager slashDamager = null;
        foreach (CollisionHandler collisionHandler in item.collisionHandlers)
        {
            foreach (Damager damager in collisionHandler.damagers)
            {
                if (damager.direction == Damager.Direction.ForwardAndBackward)
                    slashDamager = damager;
            }
        }

        if (slashDamager != null)
            return slashDamager;
        else
            return null;
    }

    public static Damager ReturnPierceDamager(Item item)
    {
        Damager pierceDamager = null;
        foreach (CollisionHandler collisionHandler in item.collisionHandlers)
        {
            foreach (Damager damager in collisionHandler.damagers)
            {
                if (damager.direction == Damager.Direction.Forward)
                    pierceDamager = damager;
            }
        }

        if (pierceDamager != null)
            return pierceDamager;
        else
            return null;
    }

    public static Damager ReturnBluntDamager(Item item)
    {
        Damager bluntDamager = null;
        foreach (CollisionHandler collisionHandler in item.collisionHandlers)
        {
            foreach (Damager damager in collisionHandler.damagers)
            {
                if (damager.direction == Damager.Direction.All)
                    bluntDamager = damager;
            }
        }

        if (bluntDamager != null)
            return bluntDamager;
        else
            return null;
    }

    public static float SetAxisLocalPositionOfHandle(this Handle handle, float value)
    {
        return value * (handle.axisLength * 0.5f);
    }

    public static Vector3 FromToDirection(this Vector3 from, Vector3 to)
    {
        return to - from;
    }

    /// <summary>
    /// Add a force that attracts when coef is positive and repulse when is negative
    /// </summary>
    public static void Attraction_Repulsion_Force(this Rigidbody rigidbody, Vector3 origin, Vector3 attractedRb,
        bool useDistance, float coef)
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
    public static void Attraction_Repulsion_ForceNoMass(this Rigidbody rigidbody, Vector3 origin, Vector3 attractedRb,
        bool useDistance, float coef)
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
        return enumerable.Aggregate((curMin, x) =>
            (curMin == null || (comparator(x).CompareTo(comparator(curMin)) < 0)) ? x : curMin);
    }

    /// <summary>
    /// Rotate the circle
    /// </summary>
    /// <param name="origin">Origin of the circle</param>
    /// <param name="forwardDirection">Forward direction of the circle (axis of rotation)</param>
    /// <param name="upDirection">Up direction of the circle (must be perpendicular to the forwardDirection)</param>
    /// <param name="radius">Radius from the center of the circle</param>
    /// <param name="speed">Speed factor</param>
    /// <param name="nbElementsAroundCircle">number of element around the circle</param>
    /// <param name="i">index of the element</param>
    /// <returns></returns>
    public static Vector3 RotateCircle(this Vector3 origin, Vector3 forwardDirection, Vector3 upDirection, float radius,
        float speed, int nbElementsAroundCircle, int i)
    {
        return origin + Quaternion.AngleAxis(i * 360f / nbElementsAroundCircle + speed, forwardDirection) *
            upDirection * radius;
    }

    /// <summary>
    /// Create the circle
    /// </summary>
    /// <param name="origin">Origin of the circle</param>
    /// <param name="forwardDirection">Forward direction of the circle (axis of rotation)</param>
    /// <param name="upDirection">Up direction of the circle (must be perpendicular to the forwardDirection)</param>
    /// <param name="radius">Radius from the center of the circle</param>
    /// <param name="nbElementsAroundCircle">number of element around the circle</param>
    /// <param name="i">index of the element</param>
    /// <returns></returns>
    public static Vector3 PosAroundCircle(this Vector3 origin, Vector3 forwardDirection, Vector3 upDirection,
        float radius, int nbElementsAroundCircle, int i)
    {
        return origin + Quaternion.AngleAxis(i * 360f / nbElementsAroundCircle, forwardDirection) * upDirection *
            radius;
    }

    /// <summary>
    /// Create a simple joint (Configurable)
    /// </summary>
    /// <param name="source">Source rigidbody</param>
    /// <param name="target">Target rigidbody</param>
    /// <param name="spring">Spring value</param>
    /// <param name="damper">Damper value</param>
    /// <returns></returns>
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

    /// <summary>
    /// Create a Configurable joint : slingshot (lock some axis)
    /// </summary>
    /// <param name="source">Source rigidbody</param>
    /// <param name="target">Target rigidbody</param>
    /// <param name="spring">Spring value</param>
    /// <param name="damper">Damper value</param>
    /// <returns></returns>
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

    /// <summary>
    /// Create a Configurable joint (that is strong) that can be with limit axis motion
    /// </summary>
    /// <param name="source">Source rigidbody</param>
    /// <param name="target">Target rigidbody</param>
    /// <param name="massScale">Mass scaling, the bigger the less the target rigidbody will matter</param>
    /// <param name="forceRot"></param>
    /// <param name="springPos"></param>
    /// <param name="damperPos"></param>
    /// <param name="forcePos"></param>
    /// <param name="springRot"></param>
    /// <param name="damperRot"></param>
    /// <param name="limitMotion">Limit the motion</param>
    /// <param name="lockMotion"></param>
    /// <returns></returns>
    public static ConfigurableJoint StrongJointFixed(Rigidbody source, Rigidbody target, float massScale = 30f,
        float springPos = 2000f, float damperPos = 40f, float forcePos = 100f, float springRot = 1000f,
        float damperRot = 40f, float forceRot = 100f, bool limitMotion = false, bool lockMotion = false)
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
            positionSpring = springPos,
            positionDamper = damperPos,
            maximumForce = forcePos
        };
        JointDrive rotDrive = new JointDrive
        {
            positionSpring = springRot,
            positionDamper = damperRot,
            maximumForce = forceRot
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
        else if (lockMotion)
        {
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
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

    /// <summary>
    /// Create a Configurable joint that attract ragdollParts to an item
    /// </summary>
    /// <param name="projectile">Source item</param>
    /// <param name="attractedRagdollPart">Target RagdollPart</param>
    /// <returns></returns>
    public static ConfigurableJoint CreateJointToProjectileForCreatureAttraction(this Item projectile,
        RagdollPart attractedRagdollPart)
    {
        JointDrive jointDrive = new JointDrive();
        jointDrive.positionSpring = 1f;
        jointDrive.positionDamper = 0.2f;
        SoftJointLimit softJointLimit = new SoftJointLimit();
        softJointLimit.limit = 0.15f;
        SoftJointLimitSpring linearLimitSpring = new SoftJointLimitSpring();
        linearLimitSpring.spring = 1f;
        linearLimitSpring.damper = 0.2f;
        ConfigurableJoint joint = attractedRagdollPart.gameObject.AddComponent<ConfigurableJoint>();
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

    /// <summary>
    /// Create a Fixed joint (sticky joint)
    /// </summary>
    /// <param name="source">Source rigidbody</param>
    /// <param name="target">Target rigidbody</param>
    /// <returns></returns>
    public static FixedJoint JointFixed(this Rigidbody source, Rigidbody target)
    {
        FixedJoint joint = target.gameObject.AddComponent<FixedJoint>();
        joint.anchor = Vector3.zero;
        joint.connectedBody = source;
        joint.connectedAnchor = Vector3.zero;
        joint.massScale = 10000f;
        joint.connectedMassScale = 0.00001f;
        joint.breakForce = Mathf.Infinity;
        joint.breakTorque = Mathf.Infinity;
        return joint;
    }

    /// <summary>
    /// Create a Spring joint that is a bit like a Yoyo
    /// </summary>
    /// <param name="hand">Source hand</param>
    /// <param name="targetRb">Target rigidbody</param>
    /// <param name="distance">Max distance of the joint</param>
    /// <returns></returns>
    public static SpringJoint YoyoJoint(RagdollHand hand, Rigidbody targetRb, float distance)
    {
        SpringJoint joint = targetRb.gameObject.AddComponent<SpringJoint>();
        joint.connectedBody = hand.physicBody.rigidBody;
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = Vector3.zero;
        joint.maxDistance = distance;
        joint.spring = 1000f;
        joint.tolerance = 0.1f;
        return joint;
    }

    /// <summary>
    /// Destroy the Joint
    /// </summary>
    /// <param name="rigidbody">Rigidbody where the joint is attached</param>
    public static void DestroyJoint(this Rigidbody rigidbody)
    {
        if (rigidbody.gameObject.GetComponent<ConfigurableJoint>())
        {
            Object.Destroy(rigidbody.gameObject.GetComponent<ConfigurableJoint>());
        }

        if (rigidbody.gameObject.GetComponent<CharacterJoint>())
        {
            Object.Destroy(rigidbody.gameObject.GetComponent<CharacterJoint>());
        }

        if (rigidbody.gameObject.GetComponent<SpringJoint>())
        {
            Object.Destroy(rigidbody.gameObject.GetComponent<SpringJoint>());
        }

        if (rigidbody.gameObject.GetComponent<HingeJoint>())
        {
            Object.Destroy(rigidbody.gameObject.GetComponent<HingeJoint>());
        }
    }

    /// <summary>
    /// Ignore/Activate the collider between a ragdoll and a collider
    /// </summary>
    /// <param name="ragdoll">Ragdoll to ignore</param>
    /// <param name="collider">Collider to ignore</param>
    /// <param name="ignore">Ignore or not</param>
    public static void IgnoreCollider(this Ragdoll ragdoll, Collider collider, bool ignore = true)
    {
        foreach (RagdollPart part in ragdoll.parts)
        {
            part.IgnoreCollider(collider, ignore);
        }
    }

    /// <summary>
    /// Ignore/Activate the collider between a ragdollPart and a collider
    /// </summary>
    /// <param name="part">RagdollPart to ignore</param>
    /// <param name="collider">Collider to ignore</param>
    /// <param name="ignore">Ignore or not</param>
    public static void IgnoreCollider(this RagdollPart part, Collider collider, bool ignore = true)
    {
        foreach (Collider itemCollider in part.colliderGroup.colliders)
        {
            Physics.IgnoreCollision(collider, itemCollider, ignore);
        }
    }

    /// <summary>
    /// Ignore/Activate the collider between an item and a collider
    /// </summary>
    /// <param name="item">Item to ignore</param>
    /// <param name="collider">Collider to ignore</param>
    /// <param name="ignore">Ignore or not</param>
    public static void IgnoreCollider(this Item item, Collider collider, bool ignore)
    {
        foreach (ColliderGroup cg in item.colliderGroups)
        {
            foreach (Collider itemCollider in cg.colliders)
            {
                Physics.IgnoreCollision(collider, itemCollider, ignore);
            }
        }
    }

    /// <summary>
    /// Ignore/Activate the collider between two items
    /// </summary>
    /// <param name="item">Item 1 to ignore</param>
    /// <param name="itemIgnored">Item 2 to ignore</param>
    /// <param name="ignore">Ignore or not</param>
    public static void IgnoreCollidephysicBodyBetweenItems(this Item item, Item itemIgnored, bool ignore = true)
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

    public static void IgnoreAllItemsListCollisions(this List<Item> items, bool ignore = true)
    {
        for (int i = 0; i < items.Count; i++)
        {
            Item itemPart1 = items[i];
            // Avoiding to take in account already done group
            for (int j = i; j < items.Count; j++)
            {
                Item itemPart2 = items[j];
                if (itemPart1 == itemPart2)
                    continue;
                for (int i1 = 0; i1 < itemPart1.colliderGroups.Count; i1++)
                {
                    ColliderGroup colliderGroup1 = itemPart1.colliderGroups[i1];
                    for (int j1 = 0; j1 < itemPart2.colliderGroups.Count; j1++)
                    {
                        ColliderGroup colliderGroup2 = itemPart2.colliderGroups[j1];
                        for (int i2 = 0; i2 < colliderGroup1.colliders.Count; i2++)
                        {
                            Collider collider1 = colliderGroup1.colliders[i2];
                            for (int j2 = 0; j2 < colliderGroup2.colliders.Count; j2++)
                            {
                                Collider collider2 = colliderGroup2.colliders[j2];
                                Physics.IgnoreCollision(collider1, collider2, ignore);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Ignore the collision between an item and a Creature + item they are holding
    /// </summary>
    /// <param name="item">Item to ignore</param>
    /// <param name="creature">Creature to ignore</param>
    public static void AddIgnoreRagdollAndItemHoldingCollision(Item item, Creature creature)
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

    public static void CreateHandle(this Handle handle)
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    HandlePose handlePose = handle.AddOrientation(k == 0 ? Side.Left : Side.Right, Vector3.zero,
                        Quaternion.AngleAxis((j * 180), Vector3.forward) * Quaternion.AngleAxis((i * 180), Vector3.up));
                    if (i == 0 && j == 0)
                    {
                        if (k == 0)
                        {
                            handle.orientationDefaultRight = handlePose;
                        }
                        else
                        {
                            handle.orientationDefaultLeft = handlePose;
                        }
                    }
                }
            }
        }
    }

    public static void SetTkHandle(this Item item, bool active)
    {
        for (int i = item.handles.Count - 1; i >= 0; --i)
        {
            for (int j = item.handles[i].telekinesisHandlers.Count - 1; j >= 0; j--)
            {
                if (!active && item.handles[i].telekinesisHandlers[j] != null)
                    item.handles[i].telekinesisHandlers[j].telekinesis.TryRelease();
            }

            item.handles[i].SetTelekinesis(active);
            item.handles[i].data.allowTelekinesis = active;
        }
    }

    public static void SetTouchHandle(this Item item, bool active)
    {
        for (int i = item.handles.Count - 1; i >= 0; --i)
        {
            if (!active && item.handles[i].IsHanded())
                item.handles[i].Release();
            item.handles[i].SetTouch(active);
            item.handles[i].data.disableTouch = !active;
        }
    }

    /// <summary>
    /// return the head, torso, leftHand, rightHand, leftFoot and rightFoot of the creature
    /// </summary>
    public static List<RagdollPart> RagdollPartsImportantList(this Creature creature)
    {
        List<RagdollPart> ragdollPartsimportant = new List<RagdollPart>
        {
            creature.GetPart(RagdollPart.Type.Head),
            creature.GetPart(RagdollPart.Type.Torso),
            creature.GetPart(RagdollPart.Type.LeftHand),
            creature.GetPart(RagdollPart.Type.RightHand),
            creature.GetPart(RagdollPart.Type.LeftFoot),
            creature.GetPart(RagdollPart.Type.RightFoot)
        };
        return ragdollPartsimportant;
    }

    /// <summary>
    /// return the leftHand, rightHand, leftFoot and rightFoot of the creature
    /// </summary>
    public static List<RagdollPart> RagdollPartsExtremitiesBodyList(this Creature creature)
    {
        List<RagdollPart> ragdollPartsimportant = new List<RagdollPart>
        {
            creature.GetPart(RagdollPart.Type.LeftHand),
            creature.GetPart(RagdollPart.Type.RightHand),
            creature.GetPart(RagdollPart.Type.LeftFoot),
            creature.GetPart(RagdollPart.Type.RightFoot)
        };
        return ragdollPartsimportant;
    }

    /// <summary>
    /// Give a random position around the creature with an offset and a radius (not vertically)
    /// </summary>
    /// <param name="creature"></param>
    /// <param name="offset"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public static Vector3 RandomPositionAroundCreatureInRadius(this Creature creature, Vector3 offset, float radius)
    {
        return creature.transform.position + offset +
               new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
    }

    /// <summary>
    /// Return a position from a position and an angle and a distance.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="angle"></param>
    /// <param name="axis"></param>
    /// <param name="upDir"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public static Vector3 CalculatePositionFromAngleWithDistance(this Vector3 position, float angle, Vector3 axis,
        Vector3 upDir, float distance)
    {
        return position + Quaternion.AngleAxis(angle, axis) * upDir * distance;
    }

    /// <summary>
    /// Return the position of from a position and an angle and a distance.
    /// </summary>
    /// <param name="creature"></param>
    /// <param name="offset"></param>
    /// <param name="radius"></param>
    /// <param name="maxAngle"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public static Vector3 RandomPositionAroundCreatureInRadiusAngle(this Creature creature, Vector3 offset,
        float radius, float maxAngle, Vector3 direction, float distance)
    {
        return GetHead(creature).transform.position + offset +
               Quaternion.AngleAxis(Random.Range(-maxAngle, maxAngle), creature.transform.up) * direction *
               Random.Range(0f, radius) * distance;
    }

    public static Vector3 RotatePointAroundPivot(this Vector3 point, Vector3 pivot, Vector3 axisOfPivot,
        float degreesOfRotation)
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.AngleAxis(degreesOfRotation, axisOfPivot) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    public static void DebugPosition(this Vector3 position, string textToDisplay)
    {
        Debug.Log("SnippetCode : " + textToDisplay + " : " + "Position X : " + position.x +
                  "; Position Y : " + position.y + "; Position Z : " + position.z);
    }

    public static void DebugRotation(this Quaternion rotation, string textToDisplay)
    {
        Debug.Log("SnippetCode : " + textToDisplay + " : " + "Rotation X : " + rotation.x +
                  "; Rotation Y : " + rotation.y + "; Rotation Z : " + rotation.z);
    }

    public static void DebugPositionAndRotation(this Transform transform, string textToDisplay)
    {
        Debug.Log("SnippetCode : " + textToDisplay + " : " + "Position X : " + transform.position.x +
                  "; Position Y : " + transform.position.y + "; Position Z : " +
                  transform.position.z);
        Debug.Log("SnippetCode : " + textToDisplay + " : " + "Rotation X : " + transform.rotation.x +
                  "; Rotation Y : " + transform.rotation.y + "; Rotation Z : " +
                  transform.rotation.z);
    }

    private static IEnumerator LerpMovement(this Vector3 positionToReach, Quaternion rotationToReach, Item itemToMove,
        float durationOfMvt)
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
                //itemToMove.physicBody.position = Vector3.Lerp(positionOrigin, positionToReach, time / durationOfMvt);
                //itemToMove.physicBody.rotation = Quaternion.Lerp(orientationOrigin, rotationToReach, time / durationOfMvt);
                itemToMove.transform.position = Vector3.Lerp(positionOrigin, positionToReach, time / durationOfMvt);
                itemToMove.transform.rotation =
                    Quaternion.Lerp(orientationOrigin, rotationToReach, time / durationOfMvt);
                time += Time.deltaTime;
                yield return null;
            }
        }

        //itemToMove.physicBody.position = positionToReach;
        foreach (ColliderGroup colliderGroup in itemToMove.colliderGroups)
        {
            foreach (Collider collider in colliderGroup.colliders)
            {
                collider.enabled = true;
            }
        }
    }


    public static IEnumerable<GameObject> GetGameObjectsChildrenOfGameObject(this GameObject gameObject,
        bool allInactive = true, bool deepLevels = false)
    {
        List<GameObject> gameObjects = new List<GameObject>();
        if (deepLevels)
        {
            List<Transform> transforms = gameObject?.GetComponentsInChildren<Transform>(allInactive).ToList();
            if (transforms != null)
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

    public static IEnumerable<GameObject> GetGameObjectsParentOfGameObject(this GameObject gameObject,
        bool allInactive = true, bool deepLevels = false)
    {
        List<GameObject> gameObjects = new List<GameObject>();
        if (deepLevels)
        {
            List<Transform> transforms = gameObject?.GetComponentsInParent<Transform>(allInactive).ToList();
            if (transforms != null)
                foreach (Transform t in transforms)
                {
                    gameObjects.Add(t.gameObject);
                }
        }
        else
        {
            if (gameObject.transform.parent)
            {
                // Grab only the actives
                if (gameObject.transform.parent.gameObject.activeSelf || allInactive)
                {
                    gameObjects.Add(gameObject.transform.parent.gameObject);
                }
            }
        }

        return gameObjects;
    }


    public static void ListAllGameObjectsChildrenOfGameObjectAndComponents(this GameObject gameObject,
        bool allInactive = true, bool deepLevels = false)
    {
        int i = 0;
        DebugLog($"Gameobject parent {i} {gameObject.name}", "yellow");
        foreach (GameObject go in GetGameObjectsChildrenOfGameObject(gameObject, allInactive, deepLevels))
        {
            DebugLog(
                $"Gameobject {i} {go.name} of parent : {(go.transform.parent ? go.transform.parent.gameObject.name : "")}",
                go.activeInHierarchy ? "cyan" : "red");
            int j = 0;
            foreach (Component component in GetComponentsOfGameObject(go, allInactive))
            {
                DebugLog($"Gameobject {i} {go.name} : Component {j} of {component.name}; Type : {component.GetType()}",
                    component.gameObject.activeInHierarchy ? "lime" : "#af0000ff");
                j++;
            }

            i++;
        }
    }

    public static void ListAllGameObjectsChildrenOfGameObject(this GameObject gameObject, bool allInactive = true,
        bool deepLevels = false)
    {
        int i = 0;
        DebugLog($"Gameobject parent {i} {gameObject.name}", "yellow");
        foreach (GameObject go in GetGameObjectsChildrenOfGameObject(gameObject, allInactive, deepLevels))
        {
            DebugLog(
                $"Gameobject {i} {go.name} of parent : {(go.transform.parent ? go.transform.parent.gameObject.name : "")}",
                go.activeInHierarchy ? "cyan" : "red");
            i++;
        }
    }

    public static void ListAllGameObjectsParentOfGameObject(this GameObject gameObject, bool allInactive = true,
        bool deepLevels = false)
    {
        int i = 0;
        DebugLog($"Gameobject child {i} {gameObject.name}", "yellow");
        List<GameObject> list = (List<GameObject>)GetGameObjectsParentOfGameObject(gameObject, allInactive, deepLevels);
        for (int j = 0; j < list.Count - 1; j++)
        {
            DebugLog($"Gameobject {i} {list[j + 1].name} parent of : {list[j].name}",
                list[j + 1].activeInHierarchy ? "cyan" : "red");
            i++;
        }
    }

    public static void ListAllGameObjectsParentsOfGameObjectAndComponents(this GameObject gameObject,
        bool allInactive = true, bool deepLevels = false)
    {
        int i = 0;
        DebugLog($"Gameobject child {i} {gameObject.name}", "yellow");
        foreach (GameObject go in GetGameObjectsParentOfGameObject(gameObject, allInactive, deepLevels))
        {
            DebugLog(
                $"Gameobject {i} {go.name} of parent : {(go.transform.parent ? go.transform.parent.gameObject.name : "")}",
                go.activeInHierarchy ? "cyan" : "red");
            int j = 0;
            foreach (Component component in GetComponentsOfGameObject(go, allInactive))
            {
                DebugLog($"Gameobject {i} {go.name} : Component {j} of {component.name}; Type : {component.GetType()}",
                    component.gameObject.activeInHierarchy ? "lime" : "#af0000ff");
                j++;
            }

            i++;
        }
    }

    public static IEnumerable<Component> GetComponentsOfGameObject(this GameObject gameObject, bool allInactive,
        bool deepLevels = false)
    {
        if (allInactive)
            return gameObject.GetComponents(typeof(Component));
        else
        {
            List<Component> components = gameObject.GetComponents(typeof(Component)).ToList();
            for (int i = components.Count - 1; i >= 0; i--)
            {
                if (!components[i].gameObject.activeSelf)
                    components.RemoveAt(i);
            }

            return components;
        }
    }

    public static void ListAllComponentsOfGameObject(this GameObject gameObject, bool allInactive = true)
    {
        int i = 0;
        foreach (Component component in GetComponentsOfGameObject(gameObject, allInactive))
        {
            DebugLog($"Component {i} of {component.name}; Type : {component.GetType()}",
                component.gameObject.activeInHierarchy ? "lime" : "#af0000ff");
            i++;
        }
    }

    public static void ListAllComponentsOfGameObjects()
    {
        List<GameObject> goList = Object.FindObjectsOfType<GameObject>().ToList();
        foreach (GameObject gameObject in goList)
        {
            Debug.Log($"GameObject Name : {gameObject.name}");
            Debug.Log($"GameObject Tag : {gameObject.tag}");
            ListAllComponentsOfGameObject(gameObject);
        }
    }

    /// <summary>
    /// Finds and returns objects of a specific type within the Unity scene.
    /// </summary>
    /// <typeparam name="T">The type of objects to search for (must derive from Component).</typeparam>
    /// <returns>An IEnumerable containing all objects of the specified type found in the scene.</returns>
    public static IEnumerable<T> FindObjectsOfType<T>() where T : Component
    {
        return Object.FindObjectsOfType<T>();
    }

    public static IEnumerable<Light> LightInARadius(this Vector3 position, float radius)
    {
        List<Light> lights = Object.FindObjectsOfType<Light>().ToList();
        for (int i = lights.Count - 1; i >= 0; i--)
        {
            if ((lights[i].transform.position - position).sqrMagnitude >= radius * radius)
                lights.RemoveAt(i);
        }

        return lights;
    }

    public static List<Light> ListOfLightsInItems(this IEnumerable<Item> items)
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
    public static List<Light> ListOfLightsInGameObject(this IEnumerable<GameObject> gameObjects)
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
                    if (item.data.type == dataType)
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

    public static List<Item> ItemsInRadiusAroundItem(this Vector3 position, Item thisItem, float radius)
    {
        List<Item> list = new List<Item>();
        for (int i = Item.allActive.Count - 1; i >= 0; i--)
        {
            if (((Item.allActive[i].transform.position - position).sqrMagnitude < radius * radius) && !thisItem)
                list.Add(Item.allActive[i]);
        }

        return list;
    }

    public static List<Item> ItemsInRadius(Vector3 position, float radius, bool ignoreFlyingItem = true,
        bool ignoreThrownItem = true, Item itemToExclude = null)
    {
        Collider[] colliders = Physics.OverlapSphere(position, radius, Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
        List<Item> itemsList = new List<Item>();
        foreach (Collider collider in colliders)
        {
            if (collider.attachedRigidbody?.GetComponent<CollisionHandler>()?.item is Item item)
            {
                if (!itemsList.Contains(item)
                    && (ignoreFlyingItem || item.isFlying)
                    && (ignoreThrownItem || item.isThrowed)
                    && (item != itemToExclude))
                {
                    itemsList.Add(item);
                }
            }
        }

        return itemsList;
    }

    public static List<Item> ItemsInConeRadius(this Vector3 position, float radius, Vector3 directionOfCone,
        float angleOfCone, bool targetFlyingItem = true, bool targetThrownItem = true, Item itemToExclude = null)
    {
        Collider[] colliders = Physics.OverlapSphere(position, radius, Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
        List<Item> itemsList = new List<Item>();
        foreach (Collider collider in colliders)
        {
            if (collider.attachedRigidbody?.GetComponent<CollisionHandler>()?.item is Item item)
            {
                Vector3 directionTowardT = item.transform.position - position;
                float angleFromConeCenter = Vector3.Angle(directionTowardT, directionOfCone);
                if (!itemsList.Contains(item)
                    && (!targetFlyingItem || item.isFlying)
                    && (!targetThrownItem || item.isThrowed)
                    && (item != itemToExclude) && angleFromConeCenter <= (angleOfCone / 2f))
                {
                    itemsList.Add(item);
                }
            }
        }

        return itemsList;
    }

    public static Item ClosestItemInConeRadius(this Vector3 position, float radius, Vector3 directionOfCone,
        float angleOfCone, bool ignoreFlyingItem = true, bool ignoreThrownItem = true, Item itemToExclude = null)
    {
        Collider[] colliders = Physics.OverlapSphere(position, radius, Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
        List<Item> itemsList = new List<Item>();
        foreach (Collider collider in colliders)
        {
            if (collider.attachedRigidbody?.GetComponent<CollisionHandler>()?.item is Item item)
            {
                Vector3 directionTowardT = item.transform.position - position;
                float angleFromConeCenter = Vector3.Angle(directionTowardT, directionOfCone);
                if (!itemsList.Contains(item)
                    && (ignoreFlyingItem || item.isFlying)
                    && (ignoreThrownItem || item.isThrowed)
                    && (item != itemToExclude) && angleFromConeCenter <= (angleOfCone / 2f))
                {
                    itemsList.Add(item);
                }
            }
        }

        if (itemsList.Count <= 0)

            return null;
        float lastRadius = Mathf.Infinity;
        Item lastItem = null;
        foreach (Item item in itemsList)
        {
            float thisRadius = (position - item.transform.position).sqrMagnitude;
            if (thisRadius <= lastRadius * lastRadius)
            {
                lastRadius = thisRadius;
                lastItem = item;
            }
        }

        return lastItem;
    }

    public static Item CenteredItemInConeRadius(this Vector3 position, float radius, Vector3 directionOfCone,
        float angleOfCone, bool ignoreFlyingItem = true, bool ignoreThrownItem = true, bool ignoreKinematicItem = true,
        bool ignoreHand = true, Item itemToExclude = null)
    {
        Collider[] colliders = Physics.OverlapSphere(position, radius, Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
        List<Item> itemsList = new List<Item>();
        foreach (Collider collider in colliders)
        {
            if (collider.attachedRigidbody?.GetComponent<CollisionHandler>()?.item is Item item)
            {
                Vector3 directionTowardT = item.transform.position - position;
                float angleFromConeCenter = Vector3.Angle(directionTowardT, directionOfCone);
                if (!itemsList.Contains(item)
                    && (ignoreFlyingItem || item.isFlying)
                    && (ignoreThrownItem || item.isThrowed)
                    && (!ignoreKinematicItem || !item.physicBody.isKinematic)
                    && (!ignoreHand || !item.mainHandler)
                    && (item != itemToExclude) && angleFromConeCenter <= (angleOfCone / 2f))
                {
                    itemsList.Add(item);
                }
            }
        }

        if (itemsList.Count <= 0)
            return null;
        float lastAngle = Mathf.Infinity;
        Item lastItem = null;
        foreach (Item item in itemsList)
        {
            Vector3 directionTowardT = item.transform.position - position;
            float thisAngle = Vector3.Angle(directionTowardT, directionOfCone);
            if (!(thisAngle <= lastAngle * lastAngle)) continue;
            lastAngle = thisAngle;
            lastItem = item;
        }

        return lastItem;
    }

    public static Item ClosestItemInListFromPosition(this List<Item> items, Vector3 position)
    {
        float lastRadius = Mathf.Infinity;
        float thisRadius;
        Item lastItem = null;
        foreach (Item item in items)
        {
            thisRadius = (position - item.transform.position).sqrMagnitude;
            if (thisRadius <= lastRadius * lastRadius)
            {
                lastRadius = thisRadius;
                lastItem = item;
            }
        }

        return lastItem;
    }

    public static Item FarthestItemInListFromPosition(this List<Item> items, Vector3 position)
    {
        float lastRadius = 0f;
        float thisRadius;
        Item lastItem = null;
        foreach (Item item in items)
        {
            thisRadius = (position - item.transform.position).sqrMagnitude;
            if (thisRadius >= lastRadius * lastRadius)
            {
                lastRadius = thisRadius;
                lastItem = item;
            }
        }

        return lastItem;
    }

    public static Item FarthestItemInConeRadius(this Vector3 position, float radius, Vector3 directionOfCone,
        float angleOfCone, bool targetFlyingItem = true, bool targetThrownItem = true, Item itemToExclude = null)
    {
        Collider[] colliders = Physics.OverlapSphere(position, radius, Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
        List<Item> itemsList = new List<Item>();
        foreach (Collider collider in colliders)
        {
            if (collider.attachedRigidbody?.GetComponent<CollisionHandler>()?.item is Item item)
            {
                Vector3 directionTowardT = item.transform.position - position;
                float angleFromConeCenter = Vector3.Angle(directionTowardT, directionOfCone);
                if (!itemsList.Contains(item)
                    && (!targetFlyingItem || item.isFlying)
                    && (!targetThrownItem || item.isThrowed)
                    && (item != itemToExclude) && angleFromConeCenter <= (angleOfCone / 2f))
                {
                    itemsList.Add(item);
                }
            }
        }

        if (itemsList.Count <= 0)

            return null;
        float lastRadius = 0f;
        Item lastItem = null;
        foreach (Item item in itemsList)
        {
            float thisRadius = (position - item.transform.position).sqrMagnitude;
            if (thisRadius >= lastRadius * lastRadius)
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

        Collider[] colliders = Physics.OverlapSphere(thisItem.transform.position, radius, Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
        List<Item> itemsList = new List<Item>();
        foreach (Collider collider in colliders)
        {
            if (collider.attachedRigidbody?.GetComponent<CollisionHandler>()?.item is not Item item) continue;
            if (item == thisItem) continue;
            float thisRadius = (collider.ClosestPoint(thisItem.transform.position) - thisItem.transform.position)
                .sqrMagnitude;
            if (!(thisRadius < radius * radius) || !(thisRadius < lastRadius)) continue;
            lastRadius = thisRadius;
            lastCollider = collider;
        }

        return lastCollider?.attachedRigidbody?.GetComponent<CollisionHandler>().item == null
            ? thisItem
            : lastCollider.attachedRigidbody.GetComponent<CollisionHandler>().item;
    }

    public static Item ClosestItemAroundOverlapSphere(this Vector3 position, float radius)
    {
        float lastRadius = Mathf.Infinity;
        Collider lastCollider = null;

        Collider[] colliders = Physics.OverlapSphere(position, radius, Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
        List<Item> itemsList = new List<Item>();
        foreach (Collider collider in colliders)
        {
            if (collider.attachedRigidbody?.GetComponent<CollisionHandler>()?.item is not Item item) continue;
            float thisRadius = (collider.ClosestPoint(position) - position).sqrMagnitude;
            if (!(thisRadius < radius * radius) || !(thisRadius < lastRadius)) continue;
            lastRadius = thisRadius;
            lastCollider = collider;
        }

        if (lastCollider?.attachedRigidbody?.GetComponent<CollisionHandler>().item == null)
        {
            return null;
        }
        else
        {
            return lastCollider.attachedRigidbody.GetComponent<CollisionHandler>().item;
        }
    }


    public static RagdollPart ClosestRagdollPartAroundItemOverlapSphere(this Item thisItem, float radius,
        bool targetPlayer = false)
    {
        float lastRadius = Mathf.Infinity;
        Collider lastCollider = null;
        List<Collider> colliders = Physics.OverlapSphere(thisItem.transform.position, radius,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore).ToList();
        foreach (Collider collider in colliders)
        {
            if (collider.attachedRigidbody?.GetComponent<CollisionHandler>()?.ragdollPart == null) continue;
            if (!targetPlayer &&
                Player.local.creature.ragdoll.parts.Contains(collider.attachedRigidbody
                    ?.GetComponent<CollisionHandler>()?.ragdollPart)) continue;
            float thisRadius = (collider.ClosestPoint(thisItem.transform.position) - thisItem.transform.position)
                .sqrMagnitude;
            if (!(thisRadius < radius * radius) || !(thisRadius < lastRadius)) continue;
            lastRadius = thisRadius;
            lastCollider = collider;
        }

        return lastCollider?.attachedRigidbody?.GetComponent<CollisionHandler>().ragdollPart == null
            ? null
            : lastCollider.attachedRigidbody.GetComponent<CollisionHandler>().ragdollPart;
    }

    /// <summary>
    /// Get the closestRagdollPart of a creature
    /// </summary>
    /// <param name="origin">Origin position</param>
    /// <param name="creature">Creature where the part need to be targeted</param>
    /// <param name="mask">Mask Apply (write it in binary : 0b00011111111111) : 1 means get the part, 0 means don't get the part : in the order of the bit from left to right : 
    /// Tail, RightWing, LeftWing, RightFoot, LeftFoot, RightLeg, LeftLeg, RightHand, LeftHand, RightArm, LeftArm, Torso, Neck, Head</param>
    /// <param name="partToExclude">Part to exclude in case it's the same part (for random case)</param>
    public static RagdollPart ClosestRagdollPart(this Vector3 origin, Creature creature, int mask = 0b00011111111111,
        RagdollPart partToExclude = null)
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
    /// Get the farthestRagdollPart of a creature
    /// </summary>
    /// <param name="origin">Origin position</param>
    /// <param name="creature">Creature where the part need to be targeted</param>
    /// <param name="mask">Mask Apply (write it in binary : 0b00011111111111) : 1 means get the part, 0 means don't get the part : in the order of the bit from left to right : 
    /// Tail, RightWing, LeftWing, RightFoot, LeftFoot, RightLeg, LeftLeg, RightHand, LeftHand, RightArm, LeftArm, Torso, Neck, Head</param>
    /// <param name="partToExclude">Part to exclude in case it's the same part (for random case)</param>
    public static RagdollPart FarthestRagdollPart(this Vector3 origin, Creature creature, int mask = 0b00011111111111,
        RagdollPart partToExclude = null)
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


    public static Vector3 HomingTarget(PhysicBody physicBody, Vector3 targetPosition, float initialDistance,
        float forceFactor, float offSetInitialDistance = 0.25f, float distanceToStick = 0f)
    {
        return Vector3.Lerp(physicBody.velocity,
            (targetPosition - physicBody.transform.position).normalized *
            Vector3.Distance(targetPosition, physicBody.transform.position) * forceFactor,
            Vector3.Distance(targetPosition, physicBody.transform.position)
                .Remap01(initialDistance + offSetInitialDistance, distanceToStick));
    }

    public static Vector3 HomingBehaviour(PhysicBody physicBody, Vector3 targetPosition, float initialTime,
        float forceFactor = 30f, float speed = 1f)
    {
        return Quaternion.Slerp(Quaternion.identity,
                   Quaternion.FromToRotation(physicBody.velocity, targetPosition - physicBody.transform.position),
                   Time.deltaTime * forceFactor * Mathf.Clamp01((Time.time - initialTime) / 0.5f))
               * physicBody.velocity.normalized * Vector3.Distance(targetPosition, physicBody.transform.position) *
               speed;
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

    public static RagdollPart GetRagdollPartByName(string partName)
    {
        RagdollPart part = null;
        if (!string.IsNullOrEmpty(partName))
        {
            foreach (RagdollPart ragdollPart in Player.local.creature.ragdoll.parts)
            {
                if (ragdollPart.name == partName)
                {
                    part = ragdollPart;
                    break;
                }
            }
        }

        return part;
    }

    public static void ThrowFireball(this Vector3 origin, Vector3 directionToShoot, float forceOfThrow = 30f,
        float distanceToShootFrom = 1f, Creature ignoredCreature = null)
    {
        Vector3 positionToSpawn;
        positionToSpawn = origin + directionToShoot.normalized * distanceToShootFrom;
        Catalog.GetData<ItemData>("DynamicProjectile").SpawnAsync(projectile =>
        {
            projectile.DisallowDespawn = true;
            projectile.physicBody.useGravity = false;
            projectile.physicBody.velocity = Vector3.zero;
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

    public static void ThrowMeteor(this Vector3 origin, Vector3 directionToShoot, Creature thrower,
        bool useGravity = true, float factorOfThrow = 1f, float distanceToShootFrom = 0.5f,
        bool ignoreCollision = false)
    {
        EffectData meteorEffectData = Catalog.GetData<EffectData>("Meteor");
        EffectData meteorExplosionEffectData = Catalog.GetData<EffectData>("MeteorExplosion");
        float meteorVelocity = 7f;
        float meteorExplosionDamage = 20f;
        float meteorExplosionPlayerDamage = 20f;
        float meteorExplosionRadius = 10f;
        AnimationCurve meteorIntensityCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 0.5f, 1f);
        SpellCastCharge meteorImbueSpellData = Catalog.GetData<SpellCastCharge>("Fire");

        Vector3 positionToSpawn = origin + directionToShoot.normalized * (distanceToShootFrom + 0.15f);
        Catalog.GetData<ItemData>("Meteor").SpawnAsync(item =>
        {
            item.DisallowDespawn = true;
            item.physicBody.useGravity = useGravity;
            item.IgnoreCollision(ignoreCollision);
            ItemMagicAreaProjectile component = item.GetComponent<ItemMagicAreaProjectile>();
            if (component != null)
            {
                _ = component;
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
                component.OnCreatureAreaHit += (_, creature) =>
                    creature.Damage(new CollisionInstance(new DamageStruct(DamageType.Energy,
                        creature.isPlayer ? meteorExplosionPlayerDamage : meteorExplosionDamage)));
                component.OnHit += collision => MeteorExplosion(collision.contactPoint, meteorExplosionRadius, thrower);
                component.guidance = GuidanceMode.NonGuided;
                component.guidanceAmount = 0f;
                component.speed = meteorVelocity;
                component.effectIntensityCurve = meteorIntensityCurve;
                item.physicBody.AddForce(directionToShoot * meteorVelocity * factorOfThrow, ForceMode.Impulse);
                component.Fire(directionToShoot, meteorEffectData, null, Player.currentCreature.ragdoll);
            }

            _ = item;
        }, positionToSpawn, Quaternion.LookRotation(directionToShoot, Vector3.up));
    }

    private static void MeteorImbueItem(ColliderGroup group) =>
        group?.imbue?.Transfer(Catalog.GetData<SpellCastCharge>("Fire"), group.imbue.maxEnergy * 2f);

    private static void MeteorExplosion(this Vector3 position, float radius, Creature thrower)
    {
        HashSet<Rigidbody> rigidbodySet = new HashSet<Rigidbody>();
        HashSet<Creature> hitCreatures = new HashSet<Creature>();
        float meteorExplosionForce = 20f;
        float meteorExplosionPlayerForce = 5f;
        LayerMask explosionLayerMask = 232799233;
        foreach (Collider collider in Physics.OverlapSphere(position, radius, explosionLayerMask,
                     QueryTriggerInteraction.Ignore))
        {
            if (collider.attachedRigidbody && !rigidbodySet.Contains(collider.attachedRigidbody))
            {
                float explosionForce = meteorExplosionForce;
                Creature componentInParent = collider.attachedRigidbody.GetComponentInParent<Creature>();
                if (componentInParent != null && componentInParent != thrower && !componentInParent.isKilled &&
                    !componentInParent.isPlayer && !hitCreatures.Contains(componentInParent))
                {
                    componentInParent.ragdoll.SetState(Ragdoll.State.Destabilized);
                    hitCreatures.Add(componentInParent);
                }

                if (collider.attachedRigidbody.GetComponentInParent<Player>() != null)
                    explosionForce = meteorExplosionPlayerForce;
                rigidbodySet.Add(collider.attachedRigidbody);
                collider.attachedRigidbody.AddExplosionForce(explosionForce, position, radius, 1f,
                    ForceMode.VelocityChange);
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
        creature.currentLocomotion.physicBody.AddForce(
            new Vector3(0f,
                curve.Evaluate(Mathf.InverseLerp(0f, gravityValue, -creature.currentLocomotion.velocity.y)) *
                gravityValue * factor, 0f), ForceMode.Acceleration);
    }

    public static bool MoveRightHandCloserToCenterOfBodyFast()
    {
        return Player.local.creature.handRight.physicBody.velocity.sqrMagnitude > 10f
               && Vector3.SignedAngle(Player.local.creature.transform.forward,
                   Vector3.Cross(Player.local.creature.handRight.physicBody.velocity,
                       Player.local.creature.transform.right), Player.local.transform.forward) < 90f;
    }

    public static bool MoveLeftHandCloserToCenterOfBodyFast()
    {
        return Player.local.creature.handLeft.physicBody.velocity.sqrMagnitude > 10f
               && Vector3.SignedAngle(Player.local.creature.transform.forward,
                   Vector3.Cross(Player.local.creature.handLeft.physicBody.velocity,
                       Player.local.creature.transform.right), Player.local.transform.forward) < 90f;
    }

    public static bool MoveBothHandCloserToCenterOfBodyFast()
    {
        return MoveLeftHandCloserToCenterOfBodyFast() && MoveRightHandCloserToCenterOfBodyFast();
    }

    public static bool BothHandAligned(float distance = 0.75f)
    {
        return Vector3.Dot(Player.local.handLeft.ragdollHand.PointDir, Player.local.handRight.ragdollHand.PointDir) >
               -1f &&
               Vector3.Dot(Player.local.handLeft.ragdollHand.PointDir, Player.local.handRight.ragdollHand.PointDir) <
               -0.5f && Vector3.Distance(Player.local.handLeft.ragdollHand.transform.position,
                   Player.local.handRight.ragdollHand.transform.position) > distance;
    }

    public static float SpeedSinSpinReverse(this float speed, float speedStrength = 480f, float timeLapse = 5)
    {
        AnimationCurve curve = CurveSinSpinReverseSpeed();
        float factorSpeed = curve.Evaluate((Time.time / timeLapse) % 1) * speedStrength;
        return speed + Time.fixedDeltaTime * factorSpeed;
    }

    public static float RadiusSinSpinReverse(this float radius, float radiusStrength = 0.3f, float timeLapse = 5)
    {
        AnimationCurve curve = CurveSinSpinReverseRadius();
        float factorRadius = curve.Evaluate((Time.time / timeLapse) % 1) * radiusStrength;
        return radius + Time.fixedDeltaTime * factorRadius;
    }

    public static void ImbueItem(this Item item, string id)
    {
        //SpellCastCharge magic = Catalog.GetData<SpellCastCharge>(ID, true);
        SpellCastCharge magic = Catalog.GetData<SpellCastCharge>(id).Clone();
        if (!magic.imbueEnabled) return;
        //SpellData.Level level = magic.level;
        //magic.Load(item.mainHandler.caster, level);
        foreach (Imbue imbue in item.imbues)
        {
            if (imbue.energy < imbue.maxEnergy)
            {
                imbue.Transfer(magic, imbue.maxEnergy);
            }
        }
    }

    public static void ImbueItem(this Item item, string id, float energy)
    {
        //SpellCastCharge magic = Catalog.GetData<SpellCastCharge>(ID, true);
        SpellCastCharge magic = Catalog.GetData<SpellCastCharge>(id).Clone();
        if (!magic.imbueEnabled) return;
        //SpellData.Level level = magic.level;
        //magic.Load(item.mainHandler.caster, level);
        foreach (Imbue imbue in item.imbues)
        {
            if (imbue.energy < imbue.maxEnergy)
            {
                imbue.Transfer(magic, energy - imbue.energy);
            }
        }
    }

    public static void ImbueItem(this Item item, SpellCaster caster, string id)
    {
        SpellCastCharge magic = Catalog.GetData<SpellCastCharge>(id).Clone();
        if (!magic.imbueEnabled) return;
        magic.Load(caster);
        foreach (Imbue imbue in item.imbues)
        {
            if (imbue.energy < imbue.maxEnergy)
            {
                imbue.Transfer(magic, imbue.maxEnergy);
            }
        }
    }

    public static string ReturnImbueId(this Item item)
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

    public static List<string> ReturnImbuesId(this Item item)
    {
        List<string> imbuesId = new List<string>();
        foreach (Imbue imbue in item.imbues)
        {
            if (imbue.energy > 0.0f)
            {
                imbuesId.Add(imbue.spellCastBase.id);
            }
        }

        return imbuesId;
    }

    public static string ReturnImbueId(this Item item, out Dictionary<string, float> energies)
    {
        string imbueId = null;
        energies = new Dictionary<string, float>();
        foreach (Imbue imbue in item.imbues)
        {
            if (imbue.energy > 0.0f)
            {
                imbueId = imbue.spellCastBase.id;
                energies.Add(imbueId, imbue.energy);
            }
        }

        return imbueId;
    }

    public static List<string> ReturnImbuesId(this Item item, out Dictionary<string, float> energies)
    {
        List<string> imbuesId = new List<string>();
        energies = new Dictionary<string, float>();
        foreach (Imbue imbue in item.imbues)
        {
            if (imbue.energy > 0.0f)
            {
                imbuesId.Add(imbue.spellCastBase.id);
                energies.Add(imbue.spellCastBase.id, imbue.energy);
            }
        }

        return imbuesId;
    }

    public static bool HasImbue(this Item item)
    {
        foreach (Imbue imbue in item.imbues)
        {
            if (imbue.energy > 0.0f)
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasImbue(this Item item, out List<Imbue> imbues)
    {
        imbues = new List<Imbue>();
        foreach (Imbue imbue in item.imbues)
        {
            if (imbue.energy > 0.0f)
            {
                imbues.Add(imbue);
                return true;
            }
        }

        return false;
    }

    public static void UnImbueItem(this Item item)
    {
        foreach (Imbue imbue in item.imbues)
        {
            if (imbue.energy > 0.0f)
            {
                imbue.energy = 0.0f;
            }
        }
    }

    public static bool ImbueBelowLevelItem(this Item item, float level)
    {
        bool levelBelowOk = false;
        foreach (Imbue imbue in item.imbues)
        {
            if (!(imbue.energy < level)) continue;
            levelBelowOk = true;
            break;
        }

        return levelBelowOk;
    }

    /// <summary>
    /// Add the spell change material to the spell wheel.
    /// </summary>
    public static void ShowSpell(SpellData spellData)
    {
        if (!Player.currentCreature.mana.spells.Contains(spellData))
            Player.currentCreature.mana.AddSpell(spellData);
    }

    private static void ForBothCasters(Action<SpellCaster> func)
    {
        func(Player.currentCreature.mana.casterLeft);
        func(Player.currentCreature.mana.casterRight);
    }

    /// <summary>
    /// Remove the spell change material to the spell wheel.
    /// </summary>
    public static void HideSpell(SpellData spellData)
    {
        ForBothCasters(caster =>
        {
            if ((caster?.spellInstance)?.id != spellData.id)
                return;
            caster.UnloadSpell();
        });
    }

    public static Color HDRColor(Color color, float intensity)
    {
        return color * Mathf.Pow(2, intensity);
    }

    public static GameObject AddZoneToGameObject(this Transform transform, float radius, bool useDebug = false,
        bool colliderIsTrigger = true, LayerName layerName = LayerName.ItemAndRagdollOnly,
        string shaderName = "Sprites/Default", string physicMaterialString = "Flesh")
    {
        GameObject zone;
        zone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        zone.GetComponent<Collider>().isTrigger = colliderIsTrigger;
        //Vector3 endPoint = FindEndPoint();
        //Ray ray = new Ray(transform.position, endPoint - transform.position);
        //float distanceHit = Vector3.Distance(transform.position, endPoint);
        zone.transform.SetParent(transform);
        zone.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Vector3.forward);
        zone.transform.localPosition = Vector3.zero;
        //zoneDoT.transform.localScale = new Vector3(radiusOfDetection, distanceHit / 2, radiusOfDetection);
        zone.transform.localScale = new Vector3(radius, radius, radius);
        zone.gameObject.layer = GameManager.GetLayer(layerName);
        zone.GetComponent<MeshRenderer>().material = new Material(Shader.Find(shaderName));
        zone.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.blue);
        zone.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        zone.GetComponent<MeshRenderer>().forceRenderingOff = !useDebug;
        zone.GetComponent<Collider>().material = new PhysicMaterial(physicMaterialString);
        return zone;
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

    public static void AddDebugPointToGo(this GameObject gameObject, Color colorParam, bool useLineRenderer = true,
        Light light = null)
    {
        Color color;
        if (!useLineRenderer) return;
        if (light != null)
        {
            switch (light.type)
            {
                case LightType.Spot:
                    color = new Color(255, 0, 0);
                    break;
                case LightType.Point:
                    color = new Color(255, 127, 0);
                    break;
                case LightType.Rectangle:
                    color = new Color(255, 0, 255);
                    break;
                case LightType.Directional:
                    color = new Color(0, 0, 255);
                    break;
                // Area light
                default:
                    color = new Color(0, 127, 255);
                    break;
            }
        }
        else
        {
            color = new Color(255, 255, 255);
        }

        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.003f;
        lineRenderer.endWidth = 0.003f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = colorParam;
        lineRenderer.endColor = colorParam;
    }

    public static void RefreshDebugPointOfGo(this GameObject gameObject)
    {
        gameObject?.GetComponent<LineRenderer>()?.SetPosition(0, gameObject.transform.position);
        gameObject?.GetComponent<LineRenderer>()
            ?.SetPosition(1, Player.local.handRight.ragdollHand.fingerIndex.tip.position);
    }

    public static void RefreshDebugPointOfGo(this GameObject gameObject, GameObject target)
    {
        gameObject?.GetComponent<LineRenderer>()?.SetPosition(0, gameObject.transform.position);
        gameObject?.GetComponent<LineRenderer>()?.SetPosition(1, target.transform.position);
    }

    public static void RefreshDebugPointOfGo(this GameObject gameObject, Transform target)
    {
        gameObject?.GetComponent<LineRenderer>()?.SetPosition(0, gameObject.transform.position);
        gameObject?.GetComponent<LineRenderer>()?.SetPosition(1, target.position);
    }

    public static string GetPath(this Transform current)
    {
        if (current.parent == null)
            return "/" + current.name;
        return GetPath(current.parent) + "/" + current.name;
    }

    public static void AddHolderPoint(Item item, Vector3 position)
    {
        GameObject go = new GameObject("HolderPoint");
        go.transform.SetParent(item.gameObject.transform);
        go.transform.position = Vector3.zero;
        go.transform.localPosition = position;
        go.transform.rotation = Quaternion.Euler(90f, 180f, 0f);
        item.holderPoint = go.transform;
    }

    public static Damager AddDamager(this Item item, string damagerName, string colliderGroupName)
    {
        GameObject go = new GameObject(damagerName);
        go.transform.SetParent(item.gameObject.transform);
        Damager damager = go.gameObject.AddComponent<Damager>();
        damager = SetDamager(item, damager, damagerName, colliderGroupName);
        return damager;
    }

    private static Damager SetDamager(this Item item, Damager damager, string damagerName, string colliderGroupName,
        Damager.Direction direction = Damager.Direction.All, float penetrationLength = 0f, float penetrationDepth = 0f,
        bool penetrationExitOnMaxDepth = false)
    {
        damager.name = damagerName;
        List<ColliderGroup> colliderGroups = item.colliderGroups;
        foreach (ColliderGroup colliderGroup in colliderGroups)
        {
            if (colliderGroup.name == colliderGroupName)
            {
                damager.colliderGroup = colliderGroup;
                break;
            }
        }

        damager.direction = direction;
        damager.penetrationLength = penetrationLength;
        damager.penetrationDepth = penetrationDepth;
        damager.penetrationExitOnMaxDepth = penetrationExitOnMaxDepth;
        return damager;
    }

    public static Holder AddHolderSlots(this Item item, string holderName, string interactableID, Vector3 touchCenter,
        Vector3 positionOnItem, Interactable.HandSide allowedHandSide = Interactable.HandSide.Both,
        float axisLength = 0f, Holder.DrawSlot drawSlot = Holder.DrawSlot.None, float touchRadius = 0.04f,
        bool useAnchor = true, int nbSlots = 1, float spacingSlots = 0.04f)
    {
        GameObject go = new GameObject(holderName);
        go.transform.SetParent(item.gameObject.transform);
        go.transform.localPosition = positionOnItem;
        Holder holderSlot = go.gameObject.AddComponent<Holder>();
        holderSlot = SetHolderSlots(holderSlot, interactableID, touchCenter, allowedHandSide, axisLength, drawSlot,
            touchRadius, useAnchor, nbSlots, spacingSlots);
        return holderSlot;
    }

    private static Holder SetHolderSlots(this Holder holder, string interactableID, Vector3 touchCenter,
        Interactable.HandSide allowedHandSide = Interactable.HandSide.Both, float axisLength = 0f,
        Holder.DrawSlot drawSlot = Holder.DrawSlot.None, float touchRadius = 0.04f, bool useAnchor = true,
        int nbSlots = 1, float spacingSlots = 0.04f)
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
            slot.transform.localPosition = new Vector3(axe == 1 ? spacing * i : 0f, axe == 2 ? spacing * i : 0f,
                axe == 3 ? spacing * i : 0f);
            holder.slots.Add(slot.transform);
        }

        return holder;
    }

    public static void DebugLog(string text, string color = "white", bool isWarning = false, bool isError = false,
        bool useTime = true)
    {
        if (color != "white")
        {
            string message = $"<color={color}>{text}</color>";
            string time = $" <color=orange>{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}</color>";
            if (useTime)
                message += time;
            if (isError)
            {
                Debug.LogError(message);
            }
            else if (isWarning)
            {
                Debug.LogWarning(message);
            }
            else
            {
                Debug.Log(message);
            }
        }
        else
        {
            string message = $"{text}";
            string time = $" <color=orange>{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}</color>";
            if (useTime)
                message += time;
            if (isError)
            {
                Debug.LogError(message);
            }
            else if (isWarning)
            {
                Debug.LogWarning(message);
            }
            else
            {
                Debug.Log(message);
            }
        }
    }

    // Generic method to transform each element in a list
    public static List<TResult> Select<T, TResult>(this List<T> inputList, Func<T, TResult> transformer)
    {
        List<TResult> result = new List<TResult>();

        foreach (T element in inputList)
        {
            result.Add(transformer(element));
        }

        return result;
    }

    // Generic method to transform a collection into a list
    private static List<T> ToList<T>(IEnumerable<T> inputCollection)
    {
        List<T> result = new List<T>();

        foreach (T element in inputCollection)
        {
            result.Add(element);
        }

        return result;
    }

    // Generic method to transform an array into a list
    private static List<T> ToList<T>(this T[] inputArray)
    {
        List<T> result = new List<T>();

        foreach (T element in inputArray)
        {
            result.Add(element);
        }

        return result;
    }

    // Generic method to filter elements based on a predicate
    private static List<T> Where<T>(this IEnumerable<T> inputCollection, Func<T, bool> predicate)
    {
        List<T> result = new List<T>();

        foreach (T element in inputCollection)
        {
            if (predicate(element))
            {
                result.Add(element);
            }
        }

        return result;
    }

    // Generic method to remove duplicate elements from a collection
    private static List<T> Distinct<T>(this IEnumerable<T> inputCollection)
    {
        HashSet<T> uniqueSet = new HashSet<T>();
        List<T> result = new List<T>();

        foreach (T element in inputCollection)
        {
            if (uniqueSet.Add(element))
            {
                result.Add(element);
            }
        }

        return result;
    }

    // Generic method to find the first element based on a condition or default
    private static T FirstOrDefault<T>(this IEnumerable<T> inputCollection, Func<T, bool> condition)
    {
        foreach (T element in inputCollection)
        {
            if (condition(element))
            {
                return element;
            }
        }

        return default; // Default value if no matching element is found
    }

    // Generic method to check if any element satisfies a condition
    private static bool Any<T>(this IEnumerable<T> inputCollection, Func<T, bool> condition)
    {
        foreach (T element in inputCollection)
        {
            if (condition(element))
            {
                return true; // Return true if any element satisfies the condition
            }
        }

        return false; // Return false if no element satisfies the condition
    }

    // Generic method to perform custom aggregation on elements
    private static T Aggregate<T>(this IEnumerable<T> inputCollection, Func<T, T, T> aggregationFunction)
    {
        using (var enumerator = inputCollection.GetEnumerator())
        {
            if (!enumerator.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements.");
            }

            T result = enumerator.Current;

            while (enumerator.MoveNext())
            {
                result = aggregationFunction(result, enumerator.Current);
            }

            return result;
        }
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
        creature.OnDespawnEvent += Creature_OnDespawnEvent;
        creature.OnKillEvent += Creature_OnKillEvent;
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

    private void Creature_OnKillEvent(CollisionInstance collisionInstance, EventTime eventTime)
    {
        if (eventTime != EventTime.OnStart)
            return;
        creature.animator.speed = orgAnimatorSpeed;
        creature.brain.Load(brainId);
        creature.ragdoll.creature.brain.instance.GetModule<BrainModuleSpeak>().audioPitchRange = orgSpeakPitchRange;
    }

    private void Creature_OnDespawnEvent(EventTime eventTime)
    {
        if (eventTime != EventTime.OnStart)
            return;
        Dispose();
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
                ragdollPart.physicBody.constraints = RigidbodyConstraints.FreezeAll;
            }
            //Debug.Log("Freeze Arrow : Is Frozen");
        }

        if (isFrozen && endOfFreeze != true)
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
            Dispose();
        }
    }

    public void Dispose()
    {
        creature.animator.speed = orgAnimatorSpeed;
        //creature.locomotion.speedModifiers = orgLocomotionSpeed;
        creature.brain.Load(brainId);
        foreach (RagdollPart ragdollPart in creature.ragdoll.parts)
        {
            ragdollPart.physicBody.constraints = RigidbodyConstraints.None;
            ragdollPart.ragdoll.RemovePhysicModifier(this);
        }

        //creature.brain.RemoveNoStandUpModifier(this);
        creature.ragdoll.creature.brain.instance.GetModule<BrainModuleSpeak>().audioPitchRange = orgSpeakPitchRange;
        //int index = 0;
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

public class SlowCreatureBehaviour : MonoBehaviour
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
    private BrainModuleSpeak moduleSpeak;
    private Vector2 orgCreatureSpeakPitch;

    public void Init(float start, float duration, float ratio, bool restoreVelocityAfterEffect = true,
        float blendDuration = 0f, bool playEffect = false)
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
        orgCreatureVelocityPart = new List<Vector3>();
        orgCreatureAngularVelocityPart = new List<Vector3>();
        orgCreatureDragPart = new List<float>();
        orgCreatureAngularDragPart = new List<float>();
    }

    public void Start()
    {
        creature.OnDespawnEvent += Creature_OnDespawnEvent;
        creature.OnKillEvent += Creature_OnKillEvent;
        brainId = creature.ragdoll.creature.brain.instance.id;
        orgAnimatorSpeed = creature.animator.speed;
        foreach (RagdollPart part in creature.ragdoll.parts)
        {
            orgCreatureDragPart.Add(part.physicBody.drag);
        }

        foreach (RagdollPart part in creature.ragdoll.parts)
        {
            orgCreatureAngularDragPart.Add(part.physicBody.angularDrag);
        }

        orgLocomotionDrag = creature.locomotion.physicBody.drag;
        orgLocomotionAngularDrag = creature.locomotion.physicBody.angularDrag;
        moduleSpeak = creature.brain.instance.GetModule<BrainModuleSpeak>();
        orgCreatureSpeakPitch = moduleSpeak.audioPitchRange;
    }

    private void Creature_OnDespawnEvent(EventTime eventTime)
    {
        if (eventTime != EventTime.OnStart)
            return;
        Dispose();
    }

    private void Creature_OnKillEvent(CollisionInstance collisionInstance, EventTime eventTime)
    {
        if (eventTime != EventTime.OnStart)
            return;
        Dispose();
    }

    public void Update()
    {
        if (!hasStarted && creature.isKilled)
        {
            Dispose();
            return;
        }

        // Wait for the start
        if (hasStarted != true)
        {
            timerStart -= Time.deltaTime;
            timerStart = Mathf.Clamp(timerStart, 0, orgTimerStart);
            if (timerStart <= 0.0f)
            {
                //orgLocomotionSpeed = creature.locomotion.speed;
                orgCreatureVelocity = creature.locomotion.physicBody.velocity;
                orgCreatureAngularVelocity = creature.locomotion.physicBody.angularVelocity;
                foreach (RagdollPart part in creature.ragdoll.parts)
                {
                    orgCreatureVelocityPart.Add(part.physicBody.velocity);
                }

                foreach (RagdollPart part in creature.ragdoll.parts)
                {
                    orgCreatureAngularVelocityPart.Add(part.physicBody.angularVelocity);
                }

                creature.brain.AddNoStandUpModifier(this);
                hasStarted = true;
            }
        }

        // Slow is blended
        if (hasStarted && isSlowed != true)
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
            //creature.locomotion.speed = Mathf.Lerp(orgAnimatorSpeed * ratioSlow / factor, orgLocomotionSpeed, timephysicBodylend);
            creature.locomotion.physicBody.velocity = Vector3.Lerp(orgCreatureVelocity * ratioSlow / factor,
                orgCreatureVelocity, timerBlend);
            creature.locomotion.physicBody.angularVelocity =
                Vector3.Lerp(orgCreatureAngularVelocity * ratioSlow / factor, orgCreatureAngularVelocity, timerBlend);
            creature.locomotion.physicBody.drag = Mathf.Lerp(factor * 100f, orgLocomotionDrag, timerBlend);
            creature.locomotion.physicBody.angularDrag =
                Mathf.Lerp(factor * 100f, orgLocomotionAngularDrag, timerBlend);
            for (int i = creature.ragdoll.parts.Count - 1; i >= 0; --i)
            {
                creature.ragdoll.parts[i].ragdoll.SetPhysicModifier(this, 0, 0, factor * 100f, factor * 100f);
                creature.ragdoll.parts[i].physicBody.velocity = Vector3.Lerp(
                    orgCreatureVelocityPart[i] * ratioSlow / factor, orgCreatureVelocityPart[i], timerBlend);
                creature.ragdoll.parts[i].physicBody.angularVelocity = Vector3.Lerp(
                    orgCreatureAngularVelocityPart[i] * ratioSlow / factor, orgCreatureAngularVelocityPart[i],
                    timerBlend);
                creature.ragdoll.parts[i].physicBody.drag =
                    Mathf.Lerp(factor * 100f, orgCreatureDragPart[i], timerBlend);
                creature.ragdoll.parts[i].physicBody.angularDrag =
                    Mathf.Lerp(factor * 100f, orgCreatureAngularDragPart[i], timerBlend);
            }

            moduleSpeak.audioPitchRange = Vector2.Lerp(orgCreatureSpeakPitch * ratioSlow / factor,
                orgCreatureSpeakPitch, timerBlend);

            if (timerBlend <= 0.0f)
            {
                isSlowed = true;
                creature.GetPart(RagdollPart.Type.Torso).physicBody.rigidBody.freezeRotation = true;
                //creature.brain.Stop();
                //creature.brain.StopAllCoroutines();
                //creature.locomotion.MoveStop();
                //creature.StopAnimation();
            }
        }

        // Slow is active and wait for the end of the duration
        if (isSlowed && endOfSlow != true)
        {
            timerDuration = Mathf.Clamp(timerDuration, 0, orgTimerDuration);
            timerDuration -= Time.deltaTime;
            if (timerDuration <= 0.0f)
            {
                endOfSlow = true;
            }
        }

        if (endOfSlow)
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

            creature.GetPart(RagdollPart.Type.Torso).physicBody.rigidBody.freezeRotation = false;
            creature.brain.RemoveNoStandUpModifier(this);
            creature.locomotion.physicBody.velocity = Vector3.zero;
            creature.locomotion.physicBody.angularVelocity = Vector3.zero;
            creature.locomotion.physicBody.drag = orgLocomotionDrag;
            creature.locomotion.physicBody.angularDrag = orgLocomotionAngularDrag;
            for (int i = creature.ragdoll.parts.Count - 1; i >= 0; --i)
            {
                creature.ragdoll.parts[i].ragdoll.RemovePhysicModifier(this);
                creature.ragdoll.parts[i].physicBody.velocity = Vector3.zero;
                creature.ragdoll.parts[i].physicBody.angularVelocity = Vector3.zero;
                creature.ragdoll.parts[i].physicBody.drag = orgCreatureDragPart[i];
                creature.ragdoll.parts[i].physicBody.angularDrag = orgCreatureAngularDragPart[i];
            }

            moduleSpeak.audioPitchRange = orgCreatureSpeakPitch;
            if (restoreVelocity && hasStarted)
            {
                creature.locomotion.physicBody.AddForce(orgCreatureVelocity, ForceMode.VelocityChange);
                creature.locomotion.physicBody.AddForce(orgCreatureAngularVelocity, ForceMode.VelocityChange);
                for (int i = creature.ragdoll.parts.Count - 1; i >= 0; --i)
                {
                    creature.ragdoll.parts[i].physicBody.AddForce(orgCreatureVelocityPart[i], ForceMode.VelocityChange);
                    creature.ragdoll.parts[i].physicBody
                        .AddTorque(orgCreatureAngularVelocityPart[i], ForceMode.VelocityChange);
                }
            }
        }

        creature.OnKillEvent -= Creature_OnKillEvent;
        creature.OnDespawnEvent -= Creature_OnDespawnEvent;
        //creature.brain.Load(brainId);
        Destroy(this);
    }
}

public class LightningBeam : ThunderBehaviour
{
    protected EffectData beamEffectData;
    protected EffectInstance beamEffect;
    public string beamEffectId;
    public LayerMask beamMask = 144718849;
    public float beamForce = 50f;
    protected SpellCastCharge imbueSpell;
    public string imbueSpellId;
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
    public string statusEffectId = "Electrocute";
    public bool instantBreakBreakables = true;
    public StatusData statusData;
    private EffectInstance beamImpactEffect;
    ParticleSystem.CollisionModule collisionModule = new ParticleSystem.CollisionModule();
    ParticleSystem.CollisionModule childCollisionModule = new ParticleSystem.CollisionModule();
    public bool isCasting;
    private SpellCastLightning instance;
    private float duration;
    private float startTime;
    public float movementSpeedMult = 0.8f;

    private SpellCaster caster;

    protected ManagedLoops ManagedLoops => ManagedLoops.Update;


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
        instance = Catalog.GetData<SpellCastLightning>("Lightning");
        duration = durationOfBeam;
        startTime = Time.time;
        caster = gameObject.GetComponent<SpellCaster>();
        Fire(true);
    }

    protected override void ManagedUpdate()
    {
        base.ManagedUpdate();
        if (Time.time - lastZap > zapInterval)
        {
            lastZap = Time.time + Random.Range(-0.5f, 0.5f);
            instance?.ShockInRadius(beamRay.origin, 3f);
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
            //instance.spellCaster.mana.creature?.locomotion.SetAllSpeedModifiers(this, movementSpeedMult);
            beamEffect = beamEffectData.Spawn(beamStart);
            //if (instance.spellCaster.mana.creature?.isPlayer != null)
            //{
            //    beamEffect.SetHaptic(HapticDevice.LeftController | HapticDevice.RightController,
            //        Catalog.gameData.haptics.telekinesisThrow);
            //}
            beamEffect?.Play(0, false, false);
            if (beamEffect != null)
            {
                foreach (EffectParticle effectParticle in beamEffect.effects.OfType<EffectParticle>())
                {
                    collisionModule = effectParticle.rootParticleSystem.collision;
                    collisionModule.collidesWith = beamMask;
                    foreach (EffectParticleChild effectParticleChild in effectParticle.childs)
                    {
                        childCollisionModule = effectParticleChild.particleSystem.collision;
                        childCollisionModule.collidesWith = beamMask;
                    }
                }
            }

            beamStart.transform.SetPositionAndRotation(beamRay.origin, Quaternion.LookRotation(beamRay.direction));
            //instance.spellCaster.ragdollHand?.playerHand.link.SetJointModifier(this,
            //    beamHandPositionSpringMultiplier, beamHandPositionDamperMultiplier,
            //    beamHandRotationSpringMultiplier, beamHandRotationDamperMultiplier,
            //    beamHandLocomotionVelocityCorrectionMultiplier);
        }
        else if (beamActive)
        {
            this.beamEffect?.End();
            this.beamEffect = null;
            //instance.spellCaster.ragdollHand?.playerHand.link.RemoveJointModifier(this);
            //instance.spellCaster.mana.creature?.locomotion.RemoveSpeedModifier(this);
            beamActive = false;
            this.hookedCreature?.Unhook();
            hookedCreature = null;
            this.beamImpactEffect?.End();
            this.beamImpactEffect = null;
        }

        if (!beamActive)
            return;
        //instance.spellCaster.ragdollHand?.playerHand.controlHand.HapticLoop(this, 1f, 0.01f);
        beamStart.transform.SetPositionAndRotation(beamRay.origin,
            Quaternion.Slerp(beamStart.transform.rotation, Quaternion.LookRotation(beamRay.direction),
                Time.deltaTime * 3f));
        if (hookedCreature && Vector3.Angle(beamRay.direction,
                hookedCreature.creature.ragdoll.GetPart(RagdollPart.Type.Torso).transform.position -
                beamRay.origin) >
            beamHookMaxAngle)
        {
            hookedCreature.Unhook();
            hookedCreature = null;
        }

        if (Physics.SphereCast(beamRay, 0.3f, out RaycastHit raycastHit, 25f, Utils.GetLayerMask(new LayerName[]
            {
                LayerName.BodyLocomotion
            }), QueryTriggerInteraction.Collide) && raycastHit.collider.gameObject.CompareTag("DefenseCollider"))
        {
            Creature componentInParent = raycastHit.collider.GetComponentInParent<Creature>();
            if (componentInParent != null)
            {
                creaturesHit.Add(componentInParent);
                componentInParent.ragdoll.forcePhysic.Add(this);
            }
        }

        if (!Physics.SphereCast(beamRay, 0.1f, out RaycastHit raycastHit2, 20f, beamMask,
                QueryTriggerInteraction.Ignore))
        {
            beamHitPoint.SetPositionAndRotation(beamRay.GetPoint(20f),
                Quaternion.LookRotation(-beamRay.direction));
            beamImpactEffect?.End(false, -1f);
            beamImpactEffect = null;
            return;
        }

        beamHitPoint.SetPositionAndRotation(raycastHit2.point + beamRay.direction * 5f,
            Quaternion.LookRotation(-beamRay.direction));
        beamImpactEffect = beamImpactEffectData?.Spawn(beamHitPoint, true, null, false);
        beamImpactEffect?.Play(0, false, false);

        if (raycastHit2.rigidbody == null)
        {
            return;
        }

        Rigidbody rigidbody = raycastHit2.rigidbody;
        SimpleBreakable simpleBreakable =
            (rigidbody != null) ? rigidbody.GetComponentInParent<SimpleBreakable>() : null;
        if (simpleBreakable != null)
        {
            simpleBreakable.Break();
        }

        Rigidbody rigidbody2 = raycastHit2.rigidbody;
        CollisionHandler collisionHandler =
            (rigidbody2 != null) ? rigidbody2.GetComponent<CollisionHandler>() : null;
        if (collisionHandler == null)
        {
            return;
        }

        if (collisionHandler.isBreakable && !collisionHandler.breakable.contactBreakOnly)
        {
            collisionHandler.breakable.Break();
            for (int i = 0; i < collisionHandler.breakable.subBrokenItems.Count; i++)
            {
                PhysicBody physicBody = collisionHandler.breakable.subBrokenItems[i].physicBody;
                if (physicBody)
                {
                    physicBody.AddForceAtPosition(beamRay.direction * beamForce, raycastHit2.point,
                        ForceMode.VelocityChange);
                }
            }

            for (int j = 0; j < collisionHandler.breakable.subBrokenBodies.Count; j++)
            {
                PhysicBody physicBody2 = collisionHandler.breakable.subBrokenBodies[j];
                if (physicBody2)
                {
                    physicBody2.AddForceAtPosition(beamRay.direction * beamForce, raycastHit2.point,
                        ForceMode.VelocityChange);
                }
            }
        }

        collisionHandler.physicBody.AddForceAtPosition(beamRay.direction * beamForce, raycastHit2.point,
            ForceMode.VelocityChange);
        if (collisionHandler.isItem)
        {
            ColliderGroup componentInParent2 = raycastHit2.collider.GetComponentInParent<ColliderGroup>();
            if (componentInParent2 != null && componentInParent2.imbue)
            {
                componentInParent2.imbue.Transfer(imbueSpell, imbueAmount * Time.deltaTime,
                    caster.mana.creature);
                return;
            }
        }
        else
        {
            RagdollPart ragdollPart = collisionHandler.ragdollPart;
            if (ragdollPart != null && ragdollPart.ragdoll.creature != caster.mana.creature &&
                !ragdollPart.isSliced)
            {
                Creature creature = ragdollPart.ragdoll.creature;
                if (Time.time - lastDamageTick > damageDelay)
                {
                    lastDamageTick = Time.time;
                    creature.Damage(new CollisionInstance(new DamageStruct(DamageType.Energy, damageAmount)
                    {
                        pushLevel = 2
                    }, null, null)
                    {
                        casterHand = instance.spellCaster,
                        contactPoint = raycastHit2.point,
                        contactNormal = raycastHit2.normal,
                        targetColliderGroup = raycastHit2.collider.GetComponentInParent<ColliderGroup>()
                    });
                    creature.Inflict(statusData, this, 5f, null, true);
                    TryHookCreature(creature);
                }

                if (Time.time - lastChainTick <= chainDelay)
                {
                    return;
                }

                lastChainTick = Time.time;
                Chain(creature);
                creaturesHit.Add(creature);
            }
        }
    }

    private void TryHookCreature(Creature creature)
    {
        if (hookedCreature?.creature == creature)
            return;
        hookedCreature?.Unhook();
        hookedCreature = creature.gameObject.GetOrAddComponent<LightningHookMergeUp>();
        hookedCreature.Hook(this);
    }

    private void Chain(Creature creature)
    {
        RagdollPart part1 = creature.ragdoll.GetPart(RagdollPart.Type.Torso);
        int maxExclusive = Physics.OverlapSphereNonAlloc(part1.transform.position, chainRadius, collidersHit,
            LayerMask.GetMask("BodyLocomotion"), QueryTriggerInteraction.Ignore);
        if (maxExclusive <= 0)
            return;
        Creature component = collidersHit[Random.Range(0, maxExclusive)].GetComponent<Creature>();
        if (component == null)
            return;
        RagdollPart part2 = component.ragdoll.GetPart(RagdollPart.Type.Torso);
        EffectInstance effectInstance = chainEffectData.Spawn(creature.transform.position, creature.transform.rotation,
            null, null, true, null, false, 1f, 1f);
        effectInstance.SetSource(creature.ragdoll.GetPart(RagdollPart.Type.Torso).transform);
        effectInstance.SetTarget(part2.transform);
        effectInstance.Play();
        component.Inflict(statusData, this, 5f);
        component.TryPush(Creature.PushType.Magic,
            (part2.transform.position - part1.transform.position).normalized * 2f, 1);
    }

    public void Fire(bool active)
    {
        if (active)
        {
            beamEffect?.End(false, -1f);
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
            beamEffect?.End(false, -1f);
            beamEffect = null;
            foreach (Creature creature in creaturesHit)
            {
                if (creature != null)
                {
                    creature.ragdoll.forcePhysic.Remove(this);
                }
            }

            creaturesHit.Clear();
            beamImpactEffect?.End(false, -1f);
            beamImpactEffect = null;
            hookedCreature?.Unhook();
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

public class LightningHookMergeUp : MonoBehaviour
{
    public Creature creature;
    private SpringJoint joint;
    private Rigidbody jointRb;
    private LightningBeam spell;
    private bool active;

    private void Awake()
    {
        creature = GetComponent<Creature>();
        creature.OnDespawnEvent += delegate(EventTime time)
        {
            if (time == EventTime.OnStart)
            {
                Destroy(this);
            }
        };
        BrainModuleHitReaction module = creature.brain.instance.GetModule<BrainModuleHitReaction>(false);
        if (module != null && module.resistDestabilizeFromMagic)
        {
            return;
        }

        jointRb = new GameObject(creature.name + " Lightning Hook Joint RB").AddComponent<Rigidbody>();
        jointRb.isKinematic = true;
        jointRb.useGravity = false;
    }

    public void Hook(LightningBeam hookingSpell)
    {
        if (active)
        {
            return;
        }

        spell = hookingSpell;
        RagdollPart part = creature.ragdoll.GetPart(RagdollPart.Type.Torso);
        BrainModuleHitReaction module = creature.brain.instance.GetModule<BrainModuleHitReaction>(false);
        if (module != null && module.resistDestabilizeFromMagic)
        {
            active = true;
            return;
        }

        creature.ragdoll.SetState(Ragdoll.State.Destabilized);
        creature.brain.AddNoStandUpModifier(this);
        creature.ragdoll.forcePhysic.Add(this);
        creature.ragdoll.SetPhysicModifier(this, new float?(0f), 1f, -1f, -1f, null);
        jointRb.transform.position = part.transform.position;
        joint = part.physicBody.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = jointRb;
        joint.connectedAnchor = Vector3.zero;
        joint.anchor = Vector3.zero;
        joint.spring = spell.beamHookSpring;
        joint.damper = spell.beamHookDamper;
        active = true;
    }

    public void Unhook()
    {
        if (!active)
        {
            return;
        }

        spell = null;
        active = false;
        if (joint != null)
        {
            Destroy(joint);
        }

        creature.brain.RemoveNoStandUpModifier(this);
        creature.ragdoll.forcePhysic.Remove(this);
        creature.ragdoll.RemovePhysicModifier(this);
    }

    private void Update()
    {
        if (spell == null)
        {
            return;
        }

        if (!active || jointRb == null)
        {
            return;
        }

        Vector3 position = jointRb.transform.position;
        jointRb.transform.SetPositionAndRotation(
            Vector3.Lerp(position, spell.beamHitPoint.position, Time.deltaTime * spell.beamHookSpeed),
            Quaternion.LookRotation(position - spell.beamStart.position));
    }

    private void OnDestroy()
    {
        Destroy(joint);
    }
}

#endregion