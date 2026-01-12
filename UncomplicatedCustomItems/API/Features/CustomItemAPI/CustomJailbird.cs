using InventorySystem.Items.Jailbird;
using LabApi.Events.Arguments.PlayerEvents;
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;
using UncomplicatedCustomItems.Events.Handlers;
using UncomplicatedCustomItems.Events.Arguments.JailbirdEvents;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomJailbird : APICustomItem
    {
        /// <summary>
        /// Gets or sets the amount of damage dealt with a Jailbird melee hit.
        /// </summary>
        public float MeleeDamage { get; set; } = 3f;

        /// <summary>
        /// Gets or sets the amount of damage dealt with a Jailbird charge hit.
        /// </summary>
        public float ChargeDamage { get; set; } = 3f;

        /// <summary>
        /// Gets or sets the amount of time in seconds that the <see cref="CustomPlayerEffects.Flashed"/> effect will be applied on being hit.
        /// </summary>
        public float FlashDuration { get; set; } = 3f;

        /// <summary>
        /// Gets or sets the radius of the Jailbird's hit register.
        /// </summary>
        public float Radius { get; set; } = 3f;

        /// <summary>
        /// Gets or sets the current <see cref="JailbirdWearState"/> of the <see cref="JailbirdItem"/>
        /// </summary>
        public JailbirdWearState WearState { get; set; }

        public override void RegisterEvents()
        {
            PlayerEvent.ProcessedJailbirdMessage += new LabApi.Events.LabEventHandler<PlayerProcessedJailbirdMessageEventArgs>(InternalOnMessageProcessed);
            PlayerEvent.ProcessingJailbirdMessage += new LabApi.Events.LabEventHandler<PlayerProcessingJailbirdMessageEventArgs>(InternalOnMessageProcessing);
            JailbirdEvents.ChangingWearState += new LabApi.Events.LabEventHandler<ChangingWearStateEventArgs>(InternalOnWearStateChanging);
            JailbirdEvents.ChangedWearState += new LabApi.Events.LabEventHandler<ChangedWearStateEventArgs>(InternalOnWearStateChanged);

            base.RegisterEvents();
        }

        public override void UnregisterEvents()
        {
            PlayerEvent.ProcessedJailbirdMessage -= new LabApi.Events.LabEventHandler<PlayerProcessedJailbirdMessageEventArgs>(InternalOnMessageProcessed);
            PlayerEvent.ProcessingJailbirdMessage -= new LabApi.Events.LabEventHandler<PlayerProcessingJailbirdMessageEventArgs>(InternalOnMessageProcessing);
            JailbirdEvents.ChangingWearState -= new LabApi.Events.LabEventHandler<ChangingWearStateEventArgs>(InternalOnWearStateChanging);
            JailbirdEvents.ChangedWearState -= new LabApi.Events.LabEventHandler<ChangedWearStateEventArgs>(InternalOnWearStateChanged);

            base.UnregisterEvents();
        }

        private void InternalOnWearStateChanging(ChangingWearStateEventArgs ev)
        {
            if (Check(ev.JailbirdItem))
                OnChangingWearState(ev);
        }

        private void InternalOnWearStateChanged(ChangedWearStateEventArgs ev)
        {
            if (Check(ev.JailbirdItem))
                OnChangedWearState(ev);
        }

        private void InternalOnMessageProcessed(PlayerProcessedJailbirdMessageEventArgs ev)
        {
            if (Check(ev.JailbirdItem))
                OnProcessedJailbirdMessage(ev);
        }

        private void InternalOnMessageProcessing(PlayerProcessingJailbirdMessageEventArgs ev)
        {
            if (Check(ev.JailbirdItem))
                OnProcessingJailbirdMessage(ev);
        }

        protected virtual void OnChangingWearState(ChangingWearStateEventArgs ev) { }
        protected virtual void OnChangedWearState(ChangedWearStateEventArgs ev) { }
        protected virtual void OnProcessedJailbirdMessage(PlayerProcessedJailbirdMessageEventArgs ev) { }
        protected virtual void OnProcessingJailbirdMessage(PlayerProcessingJailbirdMessageEventArgs ev) { }
    }
}