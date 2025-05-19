using HarmonyLib;
using InventorySystem.Items.Keycards;
using System;
using System.Collections.Generic;
using System.Reflection;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.HarmonyElements.Utilities
{
    /// <summary>
    /// Provides utility methods for interacting with keycard details,
    /// </summary>
    public static class KeycardUtils
    {
        /// <summary>
        /// Attempts to remove the keycard detail information associated with a specific serial number
        /// from the internal database managed by KeycardDetailSynchronizer.
        /// </summary>
        /// <param name="serial">The serial number of the keycard detail to remove.</param>
        /// <returns>
        /// Returns <see langword="true"/> if the keycard detail was successfully found and removed;
        /// otherwise, returns <see langword="false"/> if the database field couldn't be accessed,
        /// the database object was null or not the expected type, the serial wasn't found,
        /// or an exception occurred during the process.
        /// </returns>
        public static bool RemoveKeycardDetail(ushort serial)
        {
            try
            {
                FieldInfo databaseField = AccessTools.Field(typeof(KeycardDetailSynchronizer), "Database");

                if (databaseField == null)
                {
                    LogManager.Error("Could not find Database field in KeycardDetailSynchronizer.");
                    return false;
                }

                object databaseObject = databaseField.GetValue(null);
                if (databaseObject == null)
                {
                    LogManager.Error("KeycardDetailSynchronizer.Database field is null.");
                    return false;
                }
                if (databaseObject is Dictionary<ushort, ArraySegment<byte>> databaseDict)
                {
                    bool wasRemoved = databaseDict.Remove(serial);
                    LogManager.Debug($"Attempted to remove keycard detail for serial {serial}. Result: {wasRemoved}");
                    return wasRemoved;
                }
                else
                {
                    LogManager.Error($"KeycardDetailSynchronizer.Database field is not the expected Dictionary type. Found: {databaseObject.GetType().FullName}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"An exception occurred while trying to remove keycard detail for serial {serial}: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }
    }
}
