using System;
using InventorySystem.Items.ThrowableProjectiles;
using UncomplicatedCustomItems.API.Features.Helper;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Components
{
    public class CollisionHandler : MonoBehaviour
    {
        private bool initialized;

        public GameObject Owner { get; private set; }

        public EffectGrenade Grenade { get; private set; }

        public void Init(GameObject owner, ThrownProjectile grenade)
        {
            Owner = owner;
            Grenade = (EffectGrenade)grenade;
            initialized = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            try
            {
                if (initialized)
                {
                    if (Owner == null)
                    {
                        LogManager.Error("Owner is null!");
                    }
                    if (Grenade == null)
                    {
                        LogManager.Error("Grenade is null!");
                    }
                    if (collision == null)
                    {
                        LogManager.Error("collision is null!");
                    }
                    if (!collision.collider)
                    {
                        LogManager.Error("water :|");
                    }
                    if (collision.collider.gameObject == null)
                    {
                        LogManager.Error("Null collider gameobject");
                    }
                    if (!(collision.collider.gameObject == Owner) && !collision.collider.gameObject.TryGetComponent<EffectGrenade>(out var _))
                    {
                        Grenade.TargetTime = 0.10000000149011612;
                    }
                }
            }
            catch (Exception arg)
            {
                LogManager.Error(string.Format("{0} error:\n{1}", "OnCollisionEnter", arg));
                Destroy(this);
            }
        }
    }
}