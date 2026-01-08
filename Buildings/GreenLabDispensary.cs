using UnityEngine;
using MAPI.Core;
using MAPI.Building;
using MAPI.Utils;
using MAPI.S1;
using MAPI.Gltf;
using S1API.Products;
using S1API.Doors;
using S1API.Misc;
using MAPITesting.Utils;
using MAPI.Building.Components;
using MAPI.Building.Config;
using MAPI.Building.Interior;

namespace MAPITesting.Buildings
{
    /// <summary>
    /// The Green Lab - A marijuana dispensary building.
    /// Features: Industrial chic, polished concrete, vibrant green accents, open flow.
    /// </summary>
    public static class GreenLabDispensary
    {
        #region Constants

        private const string BuildingName = "TheGreenLab_Dispensary";
        private const string OpeningHours = "OPEN 24/7";

        // Brand colors
        private static readonly Color KushGreen = new Color(0.29f, 0.48f, 0.29f); // #4A7A4A
        private static readonly Color MatteBlack = new Color(0.1f, 0.1f, 0.1f);

        // Default spawn location (at the docks)
        private static readonly Vector3 DefaultPosition = new Vector3(-90.7707f, -2.435f, -27.9408f);
        private static readonly Quaternion DefaultRotation = Quaternion.Euler(0f, 330f, 0f);

        #endregion

        #region Storage Tracking

        private class StorageToPopulate
        {
            public GameObject StorageObject;
            public int MaxItems;
            public int QuantityPerProduct;

            public StorageToPopulate(GameObject storageObject, int maxItems, int quantityPerProduct)
            {
                StorageObject = storageObject;
                MaxItems = maxItems;
                QuantityPerProduct = quantityPerProduct;
            }
        }

        private static readonly List<StorageToPopulate> PendingStoragePopulation = new List<StorageToPopulate>();

        private static S1API.Doors.DoorController? _door;
        private static S1API.Misc.ModularSwitch? _doorLockSwitch;

        #endregion

        #region Public API

        /// <summary>
        /// Build and spawn the dispensary at the default location (docks).
        /// </summary>
        public static GameObject Spawn()
        {
            return Spawn(DefaultPosition, DefaultRotation);
        }

        /// <summary>
        /// Build and spawn the dispensary at a custom location.
        /// </summary>
        private static GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            // Create industrial palette with green accents
            var palette = new BuildingPalette
            {
                FloorMaterial = Materials.ConcreteLightGrey,
                FloorColor = new Color(0.75f, 0.75f, 0.75f),
                WallMaterial = Materials.GraniteDullSalmonLighter,
                WallColor = new Color(0.94f, 0.94f, 0.94f),
                CeilingMaterial = Materials.ConcreteLightGrey,
                CeilingColor = new Color(0.94f, 0.94f, 0.94f),
                TrimMaterial = Materials.BrickWallRed,
                TrimColor = KushGreen,
                PillarMaterial = Materials.BrickWallRed,
                PillarColor = new Color(0.6f, 0.3f, 0.2f),
                AccentColor = new Color(0.29f, 0.48f, 0.29f),
                AccentMaterial = Materials.ConcreteClothingStoreDarkGreen,
                LightColor = new Color(1f, 0.98f, 0.95f),
                LightIntensity = 1.2f
            };

            // Build using semantic API
            var builder = new BuildingBuilder(BuildingName)
                .WithConfig(BuildingConfig.Large)
                .WithPalette(palette)
                
                // Structure
                .AddFloor()
                .AddCeiling()
                .AddWalls(
                    southDoor: true,      // Main Entrance
                    eastWindow: true,     // Morning light / Product visibility
                    westWindow: true)     // Afternoon light / Lounge ambiance
                
                // Decoration
                .AddRoofTrim(height: 0.3f, material: palette.TrimMaterial)
                .AddSecondaryRoofTrim(height: 0.15f, material: palette.AccentMaterial)
                .AddCornerPillars(width: 0.5f, material: palette.TrimMaterial)
                .AddFoundation(height: 5.0f, expandX: 0.3f, expandZ: 0.3f)
                
                // Lighting
                .AddLights(intensity: 1.2f, color: new Color(1f, 0.98f, 0.95f));

            GameObject building = builder.Build();
            
            // Post-build customizations
            AddExteriorSign(building);
            AdjustWallsForDoubleDoors(building);
            AddDoubleDoors(building, OpeningHours);
            DecorateInterior(building);

            // Setup door access control
            SetupDoorLockControl();

            // Position the building
            building.transform.position = position;
            building.transform.rotation = rotation;

            Debug.Log($"[GreenLabDispensary] Spawned '{BuildingName}' at {position}");

            return building;
        }

        #endregion

        #region Private Methods - Decoration

        private static void DecorateInterior(GameObject building)
        {
            Debug.Log("[GreenLabDispensary] ========== DecorateInterior START ==========");
            
            if (building == null)
            {
                Debug.LogError("[GreenLabDispensary] DecorateInterior called with null building!");
                return;
            }

            Debug.Log($"[GreenLabDispensary] Decorating interior of building '{building.name}'");

            // Use InteriorBuilder for all interior object placement
            var interior = new InteriorBuilder(building.transform);

            // -- Back Wall (North) --
            // Create a main counter area using Display Cabinets
            // Room width is 12m (0 to 12). Center is 6.
            // Room depth is 10m (0 to 10). Back wall is at Z=10.
            
            // Center Cabinet
            interior.AddPrefab(
                Prefabs.DisplayCabinet, 
                new Vector3(6f, 0f, 9.2f), 
                Quaternion.Euler(0f, 180f, 0f), 
                networked: true,
                onCreated: PopulateStorage);

            // Left Cabinet
            interior.AddPrefab(
                Prefabs.DisplayCabinet, 
                new Vector3(3.5f, 0f, 9.2f), 
                Quaternion.Euler(0f, 180f, 0f), 
                networked: true,
                onCreated: PopulateStorage);

            // Right Cabinet
            interior.AddPrefab(
                Prefabs.DisplayCabinet, 
                new Vector3(8.5f, 0f, 9.2f), 
                Quaternion.Euler(0f, 180f, 0f), 
                networked: true,
                onCreated: PopulateStorage);

            // -- Side Walls (Storage) --
            
            // West Wall (Left side, X=0) - Wall Mounted Shelves
            interior.AddPrefab(
                Prefabs.WallMountedShelf,
                new Vector3(0.2f, 1.8f, 3f),
                Quaternion.Euler(0f, 90f, 0f),
                networked: true,
                onCreated: PopulateStorage);

            interior.AddPrefab(
                Prefabs.WallMountedShelf,
                new Vector3(0.2f, 1.8f, 7f),
                Quaternion.Euler(0f, 90f, 0f),
                networked: true,
                onCreated: PopulateStorage);

            // East Wall (Right side, X=12) - Wall Mounted Shelves
            interior.AddPrefab(
                Prefabs.WallMountedShelf,
                new Vector3(11.8f, 1.8f, 3f),
                Quaternion.Euler(0f, -90f, 0f),
                networked: true,
                onCreated: PopulateStorage);

            interior.AddPrefab(
                Prefabs.WallMountedShelf,
                new Vector3(11.8f, 1.8f, 7f),
                Quaternion.Euler(0f, -90f, 0f),
                networked: true,
                onCreated: PopulateStorage);

            // -- Entrance Area --

            // Neon Open Sign - mounted on wall near entrance
            AddNeonOpenSign(building);

            // ATM on West Wall (left side when facing entrance from inside)
            // Back of ATM against wall, facing east (into the room)
            // Note: ATM components cause exceptions when enabled on MAPI-spawned objects, so we spawn it as decoration only
            interior.AddPrefab(
                Prefabs.ATM,
                new Vector3(4.25f, 0.4f, 0.45f),
                Quaternion.Euler(0f, 0f, 0f),
                networked: true,
                enableComponents: false);

            // Desk Pedestal on East Wall (right side when facing entrance from inside)
            // Facing west (opposite to ATM)
            var frontDeskObj = Meshes.DeskPedestal.Instantiate(
                "DeskPedestal",
                new Vector3(7.4f, 0.5f, 1.0f),
                Quaternion.Euler(270f, 0f, 0f),
                building.transform);

            if (frontDeskObj != null)
            {
                Meshes.Computer.Instantiate(
                    "Computer",
                    new Vector3(-0.15f, 0.625f, 0.575f),
                    Quaternion.Euler(0f, 0f, 180f),
                    frontDeskObj.transform);
            
                Meshes.Keyboard.Instantiate(
                    "Keyboard",
                    new Vector3(0.15f, 0.625f, 0.5f),
                    Quaternion.Euler(0f, 0f, 180f),
                    frontDeskObj.transform);

                // ModularSwitch on the desk for door access control
                var modularSwitch = Prefabs.ModularSwitch.InstantiateNetworked();
                if (modularSwitch != null)
                {
                    modularSwitch.transform.SetParent(frontDeskObj.transform);
                    modularSwitch.transform.localPosition = new Vector3(0.33f, 0.3f, 0.3f);
                    modularSwitch.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                    modularSwitch.transform.localScale = new Vector3(1f, 1f, 1.15f);

                    // Store reference for door control setup (wrap GameObject in ModularSwitch)
                    _doorLockSwitch = new ModularSwitch(modularSwitch);
                }
            }

            // -- Lounge Area (Center/Front) --
            
            // Coffee Table (Center)
            interior.AddPrefab(
                Prefabs.CoffeeTable, 
                new Vector3(6f, 0f, 4f), 
                Quaternion.Euler(0f, 0f, 0f), 
                networked: true,
                onCreated: PopulateStorage);

            // -- Decorative Items --
            // Vases on top of display cabinets (back wall)
            Meshes.Vase.Instantiate(
                "Vase",
                new Vector3(6f, 1.5f, 9.2f),
                Quaternion.Euler(0f, 0f, 0f),
                building.transform);
            Meshes.Vase.Instantiate(
                "Vase",
                new Vector3(3.5f, 1.5f, 9.2f),
                Quaternion.Euler(0f, 0f, 0f),
                building.transform);
            Meshes.Vase.Instantiate(
                "Vase",
                new Vector3(8.5f, 1.5f, 9.2f),
                Quaternion.Euler(0f, 0f, 0f),
                building.transform);

            // Build the interior (organizes all objects under an Interior folder)
            interior.Build();

            Debug.Log("[GreenLabDispensary] ========== DecorateInterior COMPLETE ==========");
        }

        #endregion

        #region Private Methods - Doors

        private static void AddDoubleDoors(GameObject building, string openingHoursText)
        {
            if (building == null) return; 
            
            // Use PrefabPlacer's built-in door placement
            var prefabPlacer = new PrefabPlacer(building.transform);
            GameObject? doorsObj = prefabPlacer.PlaceSlidingDoors(
                new Vector3(6f, -0.058f, 0.0655f),
                Quaternion.Euler(0f, 180f, 0f),
                openingHoursText,
                Materials.MetalDarkGrey);

            // Get door controller components - only wrap objects that have the component
            if (doorsObj != null)
            {
                _door = new DoorController(doorsObj);
            }
            else
            {
                Debug.LogWarning("[GreenLabDispensary] doorsObj is null from PlaceSlidingDoors");
            }
        }

        #endregion

        #region Private Methods - Wall Adjustments

        private static void AdjustWallsForDoubleDoors(GameObject building)
        {
            if (building == null) return;

            Transform walls = building.transform.Find("Walls");
            if (walls == null)
            {
                Debug.LogWarning("[GreenLabDispensary] Walls container not found");
                return;
            }

            // Find the SouthWall container
            Transform southWall = walls.Find("SouthWall");
            if (southWall == null)
            {
                Debug.LogWarning("[GreenLabDispensary] SouthWall container not found");
                return;
            }

            // SouthWall_Left
            Transform left = southWall.Find("SouthWall_Left");
            if (left != null)
            {
                left.localPosition = new Vector3(2.5f, 1.9f, 0f);
                left.localScale = new Vector3(5f, 4f, 0.2f);
                Debug.Log("[GreenLabDispensary] Adjusted SouthWall_Left");
            }
            else
            {
                Debug.LogWarning("[GreenLabDispensary] SouthWall_Left not found");
            }

            // SouthWall_Top
            Transform top = southWall.Find("SouthWall_Top");
            if (top != null)
            {
                top.localPosition = new Vector3(6f, 3.4f, 0f);
                top.localScale = new Vector3(2f, 2.5f, 0.2f);
                Debug.Log("[GreenLabDispensary] Adjusted SouthWall_Top");
            }
            else
            {
                Debug.LogWarning("[GreenLabDispensary] SouthWall_Top not found");
            }

            // SouthWall_Right
            Transform right = southWall.Find("SouthWall_Right");
            if (right != null)
            {
                right.localPosition = new Vector3(9.5f, 2f, 0f);
                right.localScale = new Vector3(5f, 4f, 0.2f);
                Debug.Log("[GreenLabDispensary] Adjusted SouthWall_Right");
            }
            else
            {
                Debug.LogWarning("[GreenLabDispensary] SouthWall_Right not found");
            }
        }

        #endregion

        #region Private Methods - Signage

        private static void AddExteriorSign(GameObject building)
        {
            // Create a glowing green cross mounted on roof
            GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "DispensarySign";
            sign.transform.SetParent(building.transform);

            // Mount on roof (room height is 4m, place at Y=4.5m)
            sign.transform.localPosition = new Vector3(6f, 4.5f, -0.15f);
            sign.transform.localScale = new Vector3(1.0f, 1.0f, 0.2f);

            // Create cross arms
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.transform.SetParent(sign.transform);
            arm.transform.localPosition = Vector3.zero;
            arm.transform.localRotation = Quaternion.identity;
            arm.transform.localScale = new Vector3(0.3f, 3.0f, 1.0f);

            GameObject arm2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm2.transform.SetParent(sign.transform);
            arm2.transform.localPosition = Vector3.zero;
            arm2.transform.localRotation = Quaternion.identity;
            arm2.transform.localScale = new Vector3(3.0f, 0.3f, 1.0f);

            // Make it glow green
            Material signMat = MaterialPresets.Emissive(new Color(0f, 1f, 0f), 2.0f);

            if (sign.GetComponent<Renderer>() != null) sign.GetComponent<Renderer>().material = signMat;
            if (arm.GetComponent<Renderer>() != null) arm.GetComponent<Renderer>().material = signMat;
            if (arm2.GetComponent<Renderer>() != null) arm2.GetComponent<Renderer>().material = signMat;
        }

        private static void AddNeonOpenSign(GameObject building)
        {
            if (building == null)
            {
                Debug.LogWarning("[GreenLabDispensary] AddNeonOpenSign called with null building!");
                return;
            }

            byte[]? glbBytes = EmbeddedResourceLoader.LoadBytes("MAPITesting.Resources.neon_open_sign.glb");
            if (glbBytes == null)
            {
                Debug.LogWarning("[GreenLabDispensary] Failed to load neon open sign from embedded resource");
                return;
            }

            GameObject? sign = new GltfImporter()
                .SetEmissionIntensity(4.0f)
                .Load(glbBytes);
            
            if (sign == null)
            {
                Debug.LogWarning("[GreenLabDispensary] Failed to load neon open sign from GLB bytes");
                return;
            }

            sign.name = "NeonOpenSign";
            sign.transform.SetParent(building.transform);

            sign.transform.localPosition = new Vector3(12f, 2.4f, 4.85f);
            sign.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            sign.transform.localScale = Vector3.one * 0.3f;

            Debug.Log("[GreenLabDispensary] Neon open sign loaded and placed");
        }

        private static void AddForSaleSign(GameObject building)
        {
            if (building == null)
            {
                Debug.LogWarning("[GreenLabDispensary] AddNeonOpenSign called with null building!");
                return;
            }

            var forSaleSignTemplate = GameObject.Find("@Properties/DocksWarehouse/ForSaleSign/");
            var sign = MeshRef.Clone(forSaleSignTemplate, Vector3.zero, Quaternion.identity);
            
            if (sign == null)
            {
                Debug.LogWarning("[GreenLabDispensary] Failed to load neon open sign from GLB bytes");
                return;
            }
            
            sign.transform.localPosition = new Vector3(10f, 2f, -0.15f);
            sign.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }

        #endregion

        #region Private Methods - Door Control

        private static void SetupDoorLockControl()
        {
            if (_doorLockSwitch == null)
            {
                Debug.LogWarning("[GreenLabDispensary] Cannot setup door control - missing references");
                return;
            }

            if (_door == null)
            {
                Debug.LogWarning("[GreenLabDispensary] Cannot setup door control - door is null");
                return;
            }

            DoorAccessControl.SetupLockControl(_doorLockSwitch, _door);

            _door.OnDoorOpenedAny += () =>
                Debug.Log("[GreenLabDispensary] Door opened");
        }

        #endregion

        #region Private Methods - Product Population

        /// <summary>
        /// Queues a storage GameObject for population when Main scene initializes.
        /// Storage will be filled with 20g of packaged weed products in jars.
        /// </summary>
        /// <param name="storageObject">The GameObject containing a StorageEntity component.</param>
        private static void PopulateStorage(GameObject storageObject)
        {
            if (storageObject == null)
            {
                Debug.LogWarning("[GreenLabDispensary] Cannot queue null storage object for population");
                return;
            }

            Debug.Log($"[GreenLabDispensary] Queuing storage '{storageObject.name}' for population after Main scene init");
            PendingStoragePopulation.Add(new StorageToPopulate(storageObject, 0, 0)); // Parameters no longer used
        }

        /// <summary>
        /// Populates all queued storage containers with products.
        /// Should be called after Main scene initialization when the registry is available.
        /// </summary>
        public static void PopulateAllStorage()
        {
            Debug.Log($"[GreenLabDispensary] PopulateAllStorage called! Populating {PendingStoragePopulation.Count} queued storage containers");

            if (PendingStoragePopulation.Count == 0)
            {
                Debug.LogWarning("[GreenLabDispensary] No storage containers to populate");
                return;
            }

            int successCount = 0;
            int failCount = 0;

            foreach (var storageInfo in PendingStoragePopulation)
            {
                if (storageInfo.StorageObject == null)
                {
                    Debug.LogWarning("[GreenLabDispensary] Skipping null storage object (was it destroyed?)");
                    failCount++;
                    continue;
                }

                Debug.Log($"[GreenLabDispensary] Populating storage '{storageInfo.StorageObject.name}'...");
                int addedCount = ProductPopulator.PopulateFromGameObject(storageInfo.StorageObject, "jar", 20);

                if (addedCount > 0)
                {
                    Debug.Log($"[GreenLabDispensary] ✓ Successfully populated '{storageInfo.StorageObject.name}' with {addedCount} products");
                    successCount++;
                }
                else if (addedCount == 0)
                {
                    Debug.LogWarning($"[GreenLabDispensary] ⚠ No products were added to '{storageInfo.StorageObject.name}' - storage may be full or no products available");
                    failCount++;
                }
                else
                {
                    Debug.LogError($"[GreenLabDispensary] ✗ Failed to find StorageEntity on '{storageInfo.StorageObject.name}'");
                    failCount++;
                }
            }

            Debug.Log($"[GreenLabDispensary] Storage population complete! Success: {successCount}, Failed: {failCount}");

            // Clear the pending list
            PendingStoragePopulation.Clear();
        }

        #endregion
    }
}
