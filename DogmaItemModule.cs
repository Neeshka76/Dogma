using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;

namespace Dogma
{
    public class DogmaItemModule : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            // Will add the script of the blade when the item is loaded by the catalog
            item.gameObject.AddComponent<DogmaItem>().module = this;
        }
    }

    public class DogmaItem : ThunderBehaviour
    {
        // This is the reference of the ItemModule, mainly used to link the class DogmaItem to this class
        public DogmaItemModule module { get; internal set; }
        // Reference of the item
        private Item item;
        // Number of hands holding the items
        private int nbHandsOnItem = 0;
        // bools for the action button
        private bool buttonActionPressed;
        private bool buttonActionPressedEnteringState;
        private DogmaState state;
        //Timers
        private float timeInState;
        private float maxTimeInSharpDogmaState = 8f;
        private float cooldownFromSharpDogmaState = 5f;
        private float cooldownFromOverchargedDogmaState = 10f;
        private float timerCooldown;
        private string damagerSlashDefaultId;
        private string damagerSlashSharpId;
        private string damagerPierceDefaultId;
        private string damagerPierceSharpId;
        private string colliderGroupDefaultName;
        private string colliderGroupSharpName;
        private EffectData SFXCharge;
        private EffectInstance SFXChargeInstance;
        private EffectData FXExplosion;
        private EffectInstance FXExplosionInstance;
        private EffectData SFXOverheat;
        private EffectInstance SFXOverheatInstance;
        private EffectData SFXRestored;
        private EffectInstance SFXRestoredInstance;
        private EffectData SFXSiva;
        private EffectInstance SFXSivaInstance;
        private ParticleSystem trailVFX;
        private ParticleSystem smokeVFX;
        private ParticleSystem eSmokeVFX;
        private ParticleSystem overchargeVFX;
        private Color originalColor;
        private Color overheatColor = Snippet.HDRColor(new Color(255f, 67f, 0f) / 255f, 3f);
        private Material materialItem;
        // Used to differentiate the states of the sword
        private enum DogmaState
        {
            Idle,
            Sharp,
            Overcharged,
            Coolingdown
        }
        private DogmaState previousState;
        /// <summary>
        /// Called at the first frame of the script and only once
        /// </summary>
        public void Awake()
        {
            item = GetComponent<Item>();
            // Subscribe to some events
            item.OnHandleReleaseEvent += Item_OnHandleReleaseEvent;
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            item.OnGrabEvent += Item_OnGrabEvent;
            item.OnDespawnEvent += Item_OnDespawnEvent;
            // Set the states to a default value
            state = DogmaState.Idle;
            previousState = state;
            timeInState = 0f;
            // Get the SFXs/VFXs
            SFXCharge = Catalog.GetData<EffectData>("Dogma.SFXCharge");
            FXExplosion = Catalog.GetData<EffectData>("Dogma.FXExplosion");
            SFXOverheat = Catalog.GetData<EffectData>("Dogma.SFXOverheat");
            SFXRestored = Catalog.GetData<EffectData>("Dogma.SFXRestored");
            SFXSiva = Catalog.GetData<EffectData>("Dogma.SFXSiva");
            // Grab the length of the Siva effect to have the Sharp mode being at the same time the effect is
            Catalog.LoadAssetAsync<AudioContainer>("Hitsuu.Dogma.SFXSiva", resultAudioContainer =>
            {
                maxTimeInSharpDogmaState = resultAudioContainer.sounds[0].length;
            }, "SivaSFX");
            Catalog.LoadAssetAsync<AudioContainer>("Hitsuu.Dogma.SFXOverheat", resultAudioContainer =>
            {
                cooldownFromSharpDogmaState = resultAudioContainer.sounds[0].length;
            }, "OverheatSFX");
            // ids for the damagers and the collidergroups
            damagerSlashDefaultId = "DogmaPierceDefault";
            damagerSlashSharpId = "DogmaSlashEnhanced";
            damagerPierceDefaultId = "DogmaPierceDefault";
            damagerPierceSharpId = "DogmaPierceEnhanced";
            colliderGroupDefaultName = "BladeDogmaDefault";
            colliderGroupSharpName = "BladeDogmaEnhanced";
            // Get the reference of the effect to play the trail effect
            trailVFX = item.GetCustomReference<ParticleSystem>("Trail");
            trailVFX.Stop();
            // Get the reference of the effect to play the smoke effect
            smokeVFX = item.GetCustomReference<ParticleSystem>("Smoke");
            smokeVFX.Stop();
            // Get the reference of the effect to play the explosion smoke effect
            eSmokeVFX = item.GetCustomReference<ParticleSystem>("ESmoke");
            eSmokeVFX.Stop();
            // Get the reference of the effect to play the overcharge effect
            overchargeVFX = item.GetCustomReference<ParticleSystem>("Overcharge");
            overchargeVFX.Stop();
            materialItem = item.renderers[0].material;
            originalColor = materialItem.GetColor("_EmissionColor");
        }
        /// <summary>
        /// This is called when the item is despawn, used mainly to unsubscribe the events
        /// </summary>
        /// <param name="eventTime"></param>
        private void Item_OnDespawnEvent(EventTime eventTime)
        {
            // Happens only at the start of the despawn
            if (eventTime == EventTime.OnStart)
            {
                // Unsubscribe to the events subscribed to avoid memory leaks
                item.OnHandleReleaseEvent -= Item_OnHandleReleaseEvent;
                item.OnHeldActionEvent -= Item_OnHeldActionEvent;
                item.OnGrabEvent -= Item_OnGrabEvent;
                item.OnDespawnEvent -= Item_OnDespawnEvent;
            }
        }
        /// <summary>
        /// Is called when a hand grab a handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="ragdollHand"></param>
        private void Item_OnGrabEvent(Handle handle, RagdollHand ragdollHand)
        {
            // Use to increase the number of hands holding the item
            nbHandsOnItem++;
        }
        /// <summary>
        /// Is called when button is pressed (can be trigger, spellbutton)
        /// </summary>
        /// <param name="ragdollHand"></param>
        /// <param name="handle"></param>
        /// <param name="action"></param>
        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            // Will raise a flag to activate an action only if the button is pressed
            if (action == Interactable.Action.AlternateUseStart && !buttonActionPressed)
            {
                buttonActionPressed = true;
            }
            // Will lower a flag to activate an action only if the button is unpressed
            if (action == Interactable.Action.AlternateUseStop && buttonActionPressed)
            {
                buttonActionPressed = false;
                buttonActionPressedEnteringState = false;
            }
        }
        /// <summary>
        /// Is called when a hand ungrab a handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="ragdollHand"></param>
        /// <param name="throwing"></param>
        private void Item_OnHandleReleaseEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
        {
            // Use to decrease the number of hands holding the item
            nbHandsOnItem--;
        }
        /// <summary>
        /// Run at every frame
        /// </summary>
        public void Update()
        {
            timeInState += Time.deltaTime;
            switch (state)
            {
                // Will execute this every time the state is Idle
                case DogmaState.Idle:
                    // Happens when the spell wheel button is pressed and it's one handed
                    if (nbHandsOnItem == 1 && buttonActionPressed)
                    {
                        SwitchDogmaState(DogmaState.Sharp);
                        // Play the sound of Siva
                        SFXSivaInstance = SFXSiva.Spawn(item.transform);
                        SFXSivaInstance.Play();
                        EnhanceTheBlade(true);
                    }
                    // Happens when the spell wheel button is pressed and it's two handed
                    if (nbHandsOnItem > 1 && buttonActionPressed)
                    {
                        SwitchDogmaState(DogmaState.Overcharged);
                        // It's to keep track of the pressed button so it doesn't trigger the explosion directly after
                        buttonActionPressedEnteringState = buttonActionPressed;
                        // Play the sound of Charge
                        SFXChargeInstance = SFXCharge.Spawn(item.transform);
                        SFXChargeInstance.Play();
                        // Play VFX Overcharging
                        overchargeVFX.Play();
                    }
                    break;
                // Will execute this every time the state is Sharp
                case DogmaState.Sharp:
                    // End of the Enhanced mod based on the timer set
                    if (timeInState > maxTimeInSharpDogmaState)
                    {
                        SwitchDogmaState(DogmaState.Coolingdown);
                        // Play the sound of Overheat
                        SFXOverheatInstance = SFXOverheat.Spawn(item.transform);
                        SFXOverheatInstance.Play();
                    }
                    break;
                // Will execute this every time the state is Overcharged
                case DogmaState.Overcharged:
                    // Make the weapon glowing between the original color and the overheat color (Orange/Red)
                    materialItem.SetColor("_EmissionColor", Color.Lerp(originalColor, overheatColor, Mathf.PingPong(timeInState, 1.5f) / 1.5f));
                    // it needs to have the button released before being pressed again to trigger the explosion
                    if (buttonActionPressed && !buttonActionPressedEnteringState)
                    {
                        // Stop VFX Overcharging
                        overchargeVFX.Stop();
                        // Call the Explosion method
                        ExplosionOvercharged();
                        SwitchDogmaState(DogmaState.Coolingdown);
                        // Play the sound of Overheat (unused)
                        //SFXOverheatInstance = SFXOverheat.Spawn(item.transform);
                        //SFXOverheatInstance.Play();
                        // Start a coroutine that will fade back to the original color
                        StartCoroutine(ChangeColor(originalColor));
                    }
                    break;
                // Will execute this every time the state is Coolingdown
                case DogmaState.Coolingdown:
                    // If comes from the Enhanced mode, set the correct timer and remove the enhanced effect of the blade
                    if (previousState == DogmaState.Sharp)
                    {
                        timerCooldown = cooldownFromSharpDogmaState;
                        EnhanceTheBlade(false);
                        // Play the VFX of the cooldown mod
                        smokeVFX.Play();
                    }
                    // If comes from the Overcharged mode, set the correct timer
                    if (previousState == DogmaState.Overcharged)
                    {
                        timerCooldown = cooldownFromOverchargedDogmaState;
                        eSmokeVFX.Play();
                    }


                    // End of the cooldown
                    if (timeInState > timerCooldown)
                    {
                        timerCooldown = 0f;
                        SwitchDogmaState(DogmaState.Idle);
                        // Play the sound of Restored
                        SFXRestoredInstance = SFXRestored.Spawn(item.transform);
                        SFXRestoredInstance.Play();
                        if (smokeVFX.isPlaying)
                            smokeVFX.Stop();
                        if(eSmokeVFX.isPlaying)
                            eSmokeVFX.Stop();
                    }
                    break;
            }
        }
        /// <summary>
        /// This allow to change color with a smooth blend
        /// </summary>
        /// <param name="newColor"></param>
        /// <returns></returns>
        public IEnumerator ChangeColor(Color newColor)
        {
            float tts = 1.5f;
            float timeElapsed = 0f;
            Color toHitC = newColor;
            // Save the current emission color
            Color CurrentC = materialItem.GetColor("_EmissionColor");
            while (timeElapsed <= tts)
            {
                // lerp the value from the current color to the value you want (toHitC)
                materialItem.SetColor("_EmissionColor", Color.Lerp(CurrentC, toHitC, timeElapsed / tts));
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            materialItem.SetColor("_EmissionColor", newColor);
            yield break;
        }

        /// <summary>
        /// Switch the DogmaState with the newState passed in parameter and memorise the previous state then reset the timer counting the time in the state to zero
        /// </summary>
        /// <param name="newState"></param>
        private void SwitchDogmaState(DogmaState newState)
        {
            previousState = state;
            state = newState;
            timeInState = 0f;
        }
        /// <summary>
        /// Activate/Deactivate the Enhanced mod of the blade 
        /// </summary>
        /// <param name="active"></param>
        private void EnhanceTheBlade(bool active)
        {
            // If you activate it
            if (active)
            {
                //Replace the colliderGroups from default to sharp
                foreach (ColliderGroup group in item.colliderGroups)
                {
                    if (group.name == "Blades")
                    {
                        group.data = Catalog.GetData<ColliderGroupData>(colliderGroupSharpName);
                    }
                }
                // Change the default damagers into sharp damagers
                foreach (CollisionHandler collisionHandler in item.collisionHandlers)
                {
                    foreach (Damager damager in collisionHandler.damagers)
                    {
                        // Check the ids then switch to the new one for the slash
                        if (damager.data.id == damagerSlashDefaultId)
                            damager.Load(Catalog.GetData<DamagerData>(damagerSlashSharpId), collisionHandler);
                        // Check the ids then switch to the new one for the pierce
                        if (damager.data.id == damagerPierceDefaultId)
                            damager.Load(Catalog.GetData<DamagerData>(damagerPierceSharpId), collisionHandler);
                    }
                }
                // Play the trail effect
                trailVFX.Play();
            }
            // If you deactivate it
            else
            {
                // Replace the colliderGroups from sharp to default
                foreach (ColliderGroup group in item.colliderGroups)
                {
                    if (group.name == "Blades")
                    {
                        group.data = Catalog.GetData<ColliderGroupData>(colliderGroupDefaultName);
                    }
                }
                // Change the sharp damagers into default damagers
                foreach (CollisionHandler collisionHandler in item.collisionHandlers)
                {
                    foreach (Damager damager in collisionHandler.damagers)
                    {
                        // Check the ids then switch to the new one for the slash
                        if (damager.data.id == damagerSlashSharpId)
                            damager.Load(Catalog.GetData<DamagerData>(damagerSlashDefaultId), collisionHandler);
                        // Check the ids then switch to the new one for the pierce
                        if (damager.data.id == damagerPierceSharpId)
                            damager.Load(Catalog.GetData<DamagerData>(damagerPierceDefaultId), collisionHandler);
                    }
                }
                // Stop the trail effet
                trailVFX.Stop();
            }

        }
        /// <summary>
        /// Play the FX of the explosion and push the creatures away (except the player)
        /// </summary>
        private void ExplosionOvercharged()
        {
            // Play effects
            // Play the sound of Explosion
            FXExplosionInstance = FXExplosion.Spawn(item.transform);
            FXExplosionInstance.Play();
            // Use a custom methods that allow me to get all the creatures in a radius and grab them only if they are alive or dead and don't target the player
            List<Creature> creatures = Snippet.CreaturesInRadius(item.transform.position, 5f, true, true, false).ToList();
            // Read through each creatures of the list the method returned
            foreach (Creature creature in creatures)
            {
                // If the creature is alive, destabilized them
                if (creature.state == Creature.State.Alive)
                {
                    creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                }
                // Add force to each part of the creature
                foreach (RagdollPart part in creature.ragdoll.parts)
                {
                    // It's an explosion force so it's based on a position
                    part.physicBody.AddExplosionForce(25f, item.transform.position, 10f, 0.5f, ForceMode.VelocityChange);
                    // Check if it's the hand or feet or head then slice them if it is
                    if (part.IsImportant() && part.type != RagdollPart.Type.Torso)
                    {
                        creature.ragdoll.TrySlice(part);
                    }
                }
                // Forcefully kill the creature
                creature.Kill();
            }
        }
    }
}
