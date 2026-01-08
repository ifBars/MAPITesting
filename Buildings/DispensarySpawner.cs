using UnityEngine;

namespace MAPITesting.Buildings
{
    /// <summary>
    /// Orchestrates spawning of semantic buildings for testing.
    /// Each building type has its own dedicated class for organization.
    /// </summary>
    public static class DispensarySpawner
    {
        #region Fields

        private static GameObject? _currentBuilding;

        #endregion

        #region Public API
        
        /// <summary>
        /// Check if a building is currently spawned.
        /// </summary>
        public static bool HasBuilding => _currentBuilding != null;

        /// <summary>
        /// Spawn the Green Lab dispensary building at the default location (docks).
        /// </summary>
        public static void SpawnDispensaryBuilding()
        {
            // Clean up any existing building
            RemoveBuilding();

            // Spawn the dispensary
            _currentBuilding = GreenLabDispensary.Spawn();

            Debug.Log("[SemanticBuildingSpawner] Spawned 'The Green Lab' Dispensary at the docks");
        }
        
        #endregion

        /// <summary>
        /// Remove the currently spawned building.
        /// </summary>
        private static void RemoveBuilding()
        {
            if (_currentBuilding != null)
            {
                UnityEngine.Object.Destroy(_currentBuilding);
                _currentBuilding = null;
                Debug.Log("[SemanticBuildingSpawner] Removed semantic building");
            }
        }
    }
}
