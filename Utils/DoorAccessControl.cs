using S1API.Doors;
using S1API.Misc;
using UnityEngine;

namespace MAPITesting.Utils
{
    /// <summary>
    /// Helper class to link a ModularSwitch with DoorControllers for access control.
    /// </summary>
    public static class DoorAccessControl
    {
        /// <summary>
        /// Sets up a modular switch to control access to doors.
        /// </summary>
        /// <param name="modularSwitch">The modular switch instance.</param>
        /// <param name="doors">Array of door controllers to control.</param>
        public static void SetupLockControl(ModularSwitch modularSwitch, params DoorController[] doors)
        {
            if (modularSwitch == null)
            {
                Debug.LogError("[DoorAccessControl] Modular switch is null");
                return;
            }

            if (doors == null || doors.Length == 0)
            {
                Debug.LogWarning("[DoorAccessControl] No doors provided");
                return;
            }

            // Sync initial state with switch state
            bool isLocked = !modularSwitch.IsOn;
            
            modularSwitch.SetInteractionMessages("Lock Doors", "Unlock Doors");

            // Apply initial state to doors
            UpdateDoorAccess(doors, isLocked);

            modularSwitch.OnToggled += (isOn) =>
            {
                isLocked = !isOn;
                UpdateDoorAccess(doors, isLocked);
            };

            foreach (var door in doors)
            {
                door.OnDoorOpenedAny += () =>
                {
                    if (!isLocked)
                    {
                        Debug.Log("[DoorAccessControl] Door opened while unlocked");
                    }
                };
            }

            Debug.Log($"[DoorAccessControl] Set up lock control for {doors.Length} doors. Initial state: {(isLocked ? "Locked" : "Unlocked")}");
        }

        private static void UpdateDoorAccess(DoorController[] doors, bool locked)
        {
            foreach (var door in doors)
            {
                if (locked)
                {
                    door.PlayerAccess = DoorAccess.Closed;
                    door.OpenableByNPCs = false;
                }
                else
                {
                    door.PlayerAccess = DoorAccess.Open;
                    door.OpenableByNPCs = true;
                }
            }
        }
    }
}
