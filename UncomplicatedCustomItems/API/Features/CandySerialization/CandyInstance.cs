using System.Linq;
using InventorySystem.Items;
using InventorySystem.Items.Usables.Scp330;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.HarmonyElements.Patches;

namespace UncomplicatedCustomItems.API.Features.CandySerialization
{
    /// <summary>
    /// Candy Instancing for identifying specific candies by their Id
    /// </summary>
    public class CandyInstance
    {
        private static int _nextId = 1;

#nullable enable
        /// <summary>
        /// The <see cref="CustomItemAPI.APICustomItem"/> this candy instance is bound to.
        /// </summary>
        /// <remarks>
        /// Nullable
        /// </remarks>
        public APICustomItem? APICustomItem { get; private set; }

        /// <summary>
        /// The <see cref="Features.CustomItem"/> this candy instance is bound to.
        /// </summary>
        /// <remarks>
        /// Nullable
        /// </remarks>
        public CustomItem? CustomItem { get; private set; }
#nullable disable

        /// <summary>
        /// The Id of this candy instance.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The <see cref="CandyKindID"/> type this candy instance is
        /// </summary>
        public CandyKindID Kind { get; }

        private void DetermineType(object item)
        {
            if (item is APICustomItem apiItem)
                APICustomItem = apiItem;

            if (item is CustomItem customItem)
                CustomItem = customItem;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CandyInstance"/> class
        /// </summary>
        /// <param name="kind">The <see cref="CandyKindID"/> the candy is.</param>
        /// <param name="customItem">The CustomItem the candy is bound to. Can be either <see cref="Features.CustomItem"/> or <see cref="CustomItemAPI.APICustomItem"/></param>
        public CandyInstance(CandyKindID kind, object customItem)
        {
            Id = _nextId++;
            Kind = kind;
            DetermineType(customItem);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CandyInstance"/> class
        /// </summary>
        /// <param name="kind">The <see cref="CandyKindID"/> the candy is.</param>
        /// <param name="customItem">The CustomItem the candy is bound to. Can be either <see cref="Features.CustomItem"/> or <see cref="CustomItemAPI.APICustomItem"/></param>
        /// <param name="player">The <see cref="Player"/> the candy will be given to</param>
        public CandyInstance(CandyKindID kind, object customItem, Player player)
        {
            Id = _nextId++;
            Kind = kind;
            DetermineType(customItem);
            player.GiveCandy(kind, ItemAddReason.AdminCommand);
            
            Scp330CandyInstancePatch.GetInstances(player.Items.FirstOrDefault(i => i.Base is Scp330Bag).Base as Scp330Bag).Add(this);
            Scp330CandyInstancePatch.CandyRegistry[Id] = this;
        }
    }
}