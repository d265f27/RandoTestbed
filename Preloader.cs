using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SocialPlatforms;
using UnityEngine;
using System.Security.Permissions;
using System.Collections;
using UnityEngine.SceneManagement;
using OriModding.BF.Core;
using System.Drawing;
using System.Security.Policy;
using System.Reflection;
using HarmonyLib;

namespace RandoTestbed
{
    public class Preloader : MonoBehaviour
    {
        public static PreloadBootstrap preloadBootstrap = null;
        public static Transform jumperEnemy = null; // Old style.
        public static Transform plant = null; // Old style.


        public static bool isPreloaded = false;
        public static bool shouldStart = false;
        public static bool hasStarted = false;
        public static List<string> ourOrder = new List<string>();

        public static int preloadingIndex = -1;
        public static bool isWaiting = false;
        public static RuntimeSceneMetaData currentSceneMetaData = null;
        public static SceneManagerScene currentSceneManagerScene = null;

        public static List<string> BannedScenes = new List<string>() { "IntroLogos", "titleScreenSwallowsNest", "worldMapScene" };
        public static List<string> RequiredScenes = new List<string>();
        public static Dictionary<string, List<Action<SceneRoot>>> sceneActions = new Dictionary<string, List<Action<SceneRoot>>>();

        public static Dictionary<string, Transform> entities = new Dictionary<string, Transform>();
        public static bool OnlyRequiredScenes = true;

        public void Awake()
        {
            preloadBootstrap = this.gameObject.AddComponent<PreloadBootstrap>();
            
            sceneActions["upperGladesHollowTreeSplitB"] = new List<Action<SceneRoot>>() {
                LoadJumperEnemy, LoadPlant, PreloadStarSlugEnemy, PreloadSpitterEnemy
            };
            sceneActions["forlornRuinsKuroHideStreamlined"] = new List<Action<SceneRoot>>() {
                PreloadShootingSpider, PreloadAcidSlug, PreloadRammingEnemy, PreloadKamikazeSoot
            };


            //Checking Scene: upperGladesHollowTreeSplitB
            //MapStone: 39879169 1224343826 656285073 - 1724752157 - upperGladesHollowTreeSplitB / mapStone

            //Checking Scene: forlornRuinsKuroHideStreamlined
            //CollectablePlaceholder: -1208456571 1248142723 - 1600446330 1363770001 - forlornRuinsKuroHideStreamlined / orbs / largeExpOrbPlaceholder
            //CollectablePlaceholder: -1333697651 1090314812 1790528661 - 777845254 - forlornRuinsKuroHideStreamlined / orbs / mediumExpOrbPlaceholder
            //CollectablePlaceholder: -1717737207 1096778724 977121459 - 303618998 - forlornRuinsKuroHideStreamlined / orbs / largeExpOrbPlaceholder
            //ShootingSpiderPlaceholder: 360763959 1231584389 1925204642 803166660 - forlornRuinsKuroHideStreamlined/*setups/kuroHide/shootingSpiderEnemySetup/shootingSpiderPlaceholder
            //AcidSlugEnemyPlaceholder: 1684967355 1144746183 -986585162 -1640722891 - forlornRuinsKuroHideStreamlined/enemies/acidSlugEnemyPlaceholder
            //AcidSlugEnemyPlaceholder: -136190767 1168953463 -1912459624 2046930285 - forlornRuinsKuroHideStreamlined/enemies/acidSlugEnemyPlaceholder
            //JumperEnemyPlaceholder: 692748525 1125809836 -543104598 -2048984876 - forlornRuinsKuroHideStreamlined/enemies/jumperEnemyPlaceholder
            //RammingEnemyPlaceholder: -783175900 1236470039 -308399995 142577975 - forlornRuinsKuroHideStreamlined/enemies/rammingEnemySetup/rammingEnemyPlaceholder
            //ShootingSpiderPlaceholder: -1076599592 1300159476 2092559798 1376594991 - forlornRuinsKuroHideStreamlined/enemies/shootingSpiderEnemySetup/shootingSpiderPlaceholder
            //KamikazeSootEnemyPlaceholder: -1015536465 1079804107 1499239085 -1996289438 - forlornRuinsKuroHideStreamlined/kamikazeSootEnemyPlaceholder
            //ChargeFlameWall: 2014579407 1164325780 1399366826 -192348871 - forlornRuinsKuroHideStreamlined/*leverSetup/platformBranchSetup/transformAnimator/sunkenGladesStompTree



            //SceneBootstrap.RegisterHandler(preloadBootstrap.SetupBootstrap, "d265f27");

            //UberStates.init();
            //ShouldStart = true;
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                if (!shouldStart && !hasStarted && !isPreloaded)
                {
                    Plugin.Log("Received start input.");
                    shouldStart = true;
                }
            }
            Progress();
        }

        public static void Progress()
        {
            if (isPreloaded)
            {
                return;
            }
            if (shouldStart && !hasStarted)
            {
                System.Random random = new System.Random(25);
                shouldStart = false;
                hasStarted = true;
                Plugin.Log("Starting preloading.");

                if (OnlyRequiredScenes)
                {
                    ourOrder = new List<string>(sceneActions.Keys);
                }
                else
                {
                    List<string> tempList = new List<string>();
                    //public List<RuntimeSceneMetaData> AllScenes = new List<RuntimeSceneMetaData>();
                    for (int i = 0; i < Core.Scenes.Manager.AllScenes.Count; i++)
                    {
                        string name = Core.Scenes.Manager.AllScenes[i].Scene;
                        if (!BannedScenes.Contains(name))
                        {
                            tempList.Add(name);
                        }
                    }
                    ourOrder = tempList.OrderBy(x => random.Next()).ToList();
                }
                preloadingIndex = 0;
                isWaiting = false;
            }

            if (isWaiting)
            {
                // See if the current one is in the list and is fully loaded.
                if (SceneIsLoaded(ourOrder[preloadingIndex]))
                {
                    currentSceneManagerScene = Core.Scenes.Manager.GetSceneManagerScene(ourOrder[preloadingIndex]);
                    Plugin.Log("Finished loading scene " + ourOrder[preloadingIndex] + " " + currentSceneManagerScene.LoadingTime.ToString());
                    if (OnlyRequiredScenes)
                    {
                        GrabFromScene(currentSceneManagerScene.SceneRoot);
                    }
                    else
                    {
                        CheckScene(currentSceneManagerScene.SceneRoot);
                    }
                    Core.Scenes.Manager.UnloadScene(currentSceneManagerScene, false, true);
                    isWaiting = false;
                    preloadingIndex++;
                    if (preloadingIndex >= ourOrder.Count)
                    {
                        isPreloaded = true;
                        Plugin.Log("Finished preloading.");
                    }
                }
                else if (SceneHasDisappeared(ourOrder[preloadingIndex]))
                {
                    // FIXME Look, if our scene actually disappears then we don't actually finish preloading, we need to fix it.
                    Plugin.Log("Finished loading scene " + ourOrder[preloadingIndex] + " disappeared");
                    isWaiting = false;
                    preloadingIndex++;
                    if (preloadingIndex >= ourOrder.Count)
                    {
                        isPreloaded = true;
                        Plugin.Log("Finished preloading.");
                    }

                }

                return;
            }
            else if (preloadingIndex >= 0)
            {
                Plugin.Log("Started loading scene " + ourOrder[preloadingIndex]);
                currentSceneMetaData = GetSceneManagerSceneFromAll(ourOrder[preloadingIndex]);
                Core.Scenes.Manager.AdditivelyLoadSceneWithoutDependencies(currentSceneMetaData, true, true);
                isWaiting = true;
            }
        }

        public static bool SceneIsLoaded(string sceneName)
        {
            foreach (SceneManagerScene sceneManagerScene in Core.Scenes.Manager.ActiveScenes)
            {
                if (sceneManagerScene.MetaData.Scene == sceneName)
                {
                    if (sceneManagerScene.CurrentState == SceneManagerScene.State.Loading || sceneManagerScene.CurrentState == SceneManagerScene.State.LoadingCancelled)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        public static bool SceneHasDisappeared(string sceneName)
        {
            foreach (SceneManagerScene sceneManagerScene in Core.Scenes.Manager.ActiveScenes)
            {
                if (sceneManagerScene.MetaData.Scene == sceneName)
                {
                    return false;
                }
            }
            return true;
        }

        public static RuntimeSceneMetaData GetSceneManagerSceneFromAll(string sceneName)
        {
            for (int i = 0; i < Core.Scenes.Manager.AllScenes.Count; i++)
            {
                if (Core.Scenes.Manager.AllScenes[i].Scene == sceneName)
                {
                    return Core.Scenes.Manager.AllScenes[i];
                }
            }
            return null;
        }

        public static void GrabFromScene(SceneRoot sceneRoot)
        {
            if (sceneActions.ContainsKey(sceneRoot.name))
            {
                foreach (Action<SceneRoot> sceneAction in sceneActions[sceneRoot.name])
                {
                    sceneAction.Invoke(sceneRoot);
                }

            }
        }

        private static void CheckScene(SceneRoot sceneRoot)
        {
            Plugin.Log("Checking Scene: " + sceneRoot.name);

            // Iterate over DifficultyCondition.
            DifficultyCondition[] difficulties = sceneRoot.transform.GetComponentsInChildren<DifficultyCondition>();
            foreach (DifficultyCondition condition in difficulties)
            {
                Plugin.Log("    DifficultyCondition: " + condition.transform.GetNicePath());
            }
            // Iterate over CollectablePlaceholder.
            CollectablePlaceholder[] collectables = sceneRoot.transform.GetComponentsInChildren<CollectablePlaceholder>();
            foreach (CollectablePlaceholder collectable in collectables)
            {
                Plugin.Log("    CollectablePlaceholder: " + collectable.MoonGuid.ToString() + " - " + collectable.transform.GetNicePath());
            }
            // Iterate over KeystonePickup.
            KeystonePickup[] keystones = sceneRoot.transform.GetComponentsInChildren<KeystonePickup>();
            foreach (KeystonePickup keystone in keystones)
            {
                Plugin.Log("    KeystonePickup: " + keystone.MoonGuid.ToString() + " - " + keystone.transform.GetNicePath());
            }
            // Iterate over MapstonePickup.
            MapStonePickup[] mapstones = sceneRoot.transform.GetComponentsInChildren<MapStonePickup>();
            foreach (MapStonePickup mapstone in mapstones)
            {
                Plugin.Log("    MapStonePickup: " + mapstone.MoonGuid.ToString() + " - " + mapstone.transform.GetNicePath());
            }

            // Iterate over Plant.
            PetrifiedPlant[] plants = sceneRoot.transform.GetComponentsInChildren<PetrifiedPlant>();
            foreach (PetrifiedPlant thisPlant in plants)
            {
                Plugin.Log("    PetrifiedPlant: " + thisPlant.MoonGuid.ToString() + " - " + thisPlant.transform.GetNicePath());
            }
            // Iterate over Map pedestals
            MapStone[] pedestals = sceneRoot.transform.GetComponentsInChildren<MapStone>();
            foreach (MapStone pedestal in pedestals)
            {
                Plugin.Log("    MapStone: " + pedestal.MoonGuid.ToString() + " - " + pedestal.transform.GetNicePath());
            }
            // Iterate over trees
            GetAbilityPedestal[] trees = sceneRoot.transform.GetComponentsInChildren<GetAbilityPedestal>();
            foreach (GetAbilityPedestal tree in trees)
            {
                Plugin.Log("    GetAbilityPedestal: " + tree.MoonGuid.ToString() + " - " + tree.transform.GetNicePath());
            }
            // Iterate over RespawningPlaceholder
            RespawningPlaceholder[] respawners = sceneRoot.transform.GetComponentsInChildren<RespawningPlaceholder>();
            foreach (RespawningPlaceholder respawner in respawners)
            {
                Plugin.Log("    " + respawner.GetType().Name + ": " + respawner.MoonGuid.ToString() + " - " + respawner.transform.GetNicePath());
            }
            // Iterate over keystone doors.
            DoorWithSlots[] keystonedoors = sceneRoot.transform.GetComponentsInChildren<DoorWithSlots>();
            foreach (DoorWithSlots keystonedoor in keystonedoors)
            {
                Plugin.Log("    DoorWithSlots (" + keystonedoor.NumberOfOrbsRequired.ToString() + "): " + keystonedoor.MoonGuid.ToString() + " - " + keystonedoor.transform.GetNicePath());
            }
            // Iterate over energy doors.
            EnergyDoor[] energydoors = sceneRoot.transform.GetComponentsInChildren<EnergyDoor>();
            foreach (EnergyDoor energydoor in energydoors)
            {
                Plugin.Log("    EnergyDoor (" + energydoor.AmountOfEnergyRequired.ToString() + "): " + energydoor.MoonGuid.ToString() + " - " + energydoor.transform.GetNicePath());
            }
            // Iterate over charge flame walls.
            ChargeFlameWall[] chargeflamewalls = sceneRoot.transform.GetComponentsInChildren<ChargeFlameWall>();
            foreach (ChargeFlameWall chargeflamewall in chargeflamewalls)
            {
                Plugin.Log("    ChargeFlameWall: " + chargeflamewall.MoonGuid.ToString() + " - " + chargeflamewall.transform.GetNicePath());
            }
            // Iterate over stomp posts.
            StompPost[] stompposts = sceneRoot.transform.GetComponentsInChildren<StompPost>();
            foreach (StompPost stomppost in stompposts)
            {
                Plugin.Log("    StompPost: " + stomppost.MoonGuid.ToString() + " - " + stomppost.transform.GetNicePath());
            }
            // Iterate over Grenade doors.
            List<Transform> grenadedoors = sceneRoot.transform.FindChildrenByName("*ignitableSpiritTorchPuzzle");
            foreach (Transform grenadedoor in grenadedoors)
            {
                Plugin.Log("    Grenade Door: " + grenadedoor.transform.GetNicePath());
            }
            // Iterate over Stompable Floors.
            StompableFloor[] stompablefloors = sceneRoot.transform.GetComponentsInChildren<StompableFloor>();
            foreach (StompableFloor stompablefloor in stompablefloors)
            {
                Plugin.Log("    StompableFloor: " + stompablefloor.MoonGuid.ToString() + " - " + stompablefloor.transform.GetNicePath());
            }
            // Iterate over Springs.
            Spring[] Springs = sceneRoot.transform.GetComponentsInChildren<Spring>();
            foreach (Spring spring in Springs)
            {
                Plugin.Log("    Spring: " + spring.transform.GetNicePath());
            }
            Plugin.Log("Finished Scene: " + sceneRoot.name);
        }

        public static void CheckOwners(Transform transform)
        {
            GuidOwner[] owners = transform.gameObject.GetComponentsInChildren<GuidOwner>();
            if (owners.Count() > 15)
            {
                Plugin.Log("Found " + owners.Count().ToString() + " GuidOwners. TOO MANY!");
            }
            else
            {
                Plugin.Log("Found " + owners.Count().ToString() + " GuidOwners.");
            }
        }

        public static void SetGuidAndSave(SceneRoot sceneRoot, GuidOwner owner, MoonGuid guid)
        {
            owner.MoonGuid = guid;

            if (owner is SaveSerialize)
            {
                (owner as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
            }
        }

        public static void PreloadGeneric<T>(SceneRoot sceneRoot, string name, MoonGuid storageGuid, bool useParent = false, int index = -1) where T : MonoBehaviour
        {
            Plugin.Log("Loading generic " + name);
            T item = null;
            if (index == -1)
            {
                item = sceneRoot.transform.GetComponentInChildren<T>();
            }
            else
            {
                T[] items = sceneRoot.transform.GetComponentsInChildren<T>();
                if (items.Length > index)
                {
                    item = items[index];
                }
            }
            if (item == null)
            {
                Plugin.Log("Could not find " + name);
                return;
            }
            Transform transform = item.transform;
            if (useParent)
            {
                transform = transform.parent;
            }
            CheckOwners(transform);
            Transform clone = CloneForPreloading(transform, name, storageGuid);
            clone.gameObject.SetActive(false);
            clone.parent = null;
            //GameObject.DontDestroyOnLoad(clone.gameObject); FIXME Is this actually needed? What gets destroyed if we do it intelligently.
            FixRenderers(item.transform, clone);
            entities[name] = clone;
        }


        private static Transform CloneForPreloading(Transform obj, string name, MoonGuid guid)
        {
            // temporarily fiddle with the original object's active status to prevent the clone from instantly awaking if it shouldn't
            bool originalActive = obj.gameObject.activeSelf;
            if (!obj.gameObject.activeInHierarchy)
            {
                obj.gameObject.SetActive(false);
            }
            Transform clone = UnityEngine.Object.Instantiate<Transform>(obj);
            if (name != null)
            {
                clone.gameObject.name = name;
            }
            // reinstate active status after the clone is part of the hierarchy
            obj.gameObject.SetActive(originalActive);
            //clone.gameObject.SetActive(originalActive);
            // Unlike CloneObject we don't want to register this prefab as something needing to be saved.
            // But giving it a unique MoonGuid is nice.
            int twiddleCount = 0;
            foreach (GuidOwner owner in clone.gameObject.FindComponentsInChildren<GuidOwner>())
            {
                owner.MoonGuid = TwiddleGuid(guid, twiddleCount);
                twiddleCount++;
            }
            return clone;
        }


        public static MoonGuid TwiddleGuid(MoonGuid originalGuid, int copyNumber)
        {
            // CopyNumber 0 will be the initial Guid, other 1-15.
            // Note, using this to twiddle an already twiddled guid will make twiddles of the original
            // guid.
            if (copyNumber > 15)
            {
                copyNumber = 15;
            }
            uint[] values = {0x40000000u, 0x50000000u, 0x60000000u, 0x70000000u,
                         0x80000000u, 0x90000000u, 0xA0000000u, 0xB0000000u,
                         0xC0000000u, 0xD0000000u, 0xE0000000u, 0xF0000000u,
                         0x00000000u, 0x10000000u, 0x20000000u, 0x30000000u};
            uint modified = (unchecked((uint)originalGuid.B) & 0x0FFFFFFFu) | values[copyNumber];

            return new MoonGuid(originalGuid.A, unchecked((int)modified), originalGuid.C, originalGuid.D);
        }

        public static MoonGuid UntwiddleGuid(MoonGuid moonGuid)
        {
            // Untwiddles a MoonGuid for comparison to normal MoonGuids. It is safe to untwiddle a normal MoonGuid.
            uint modified = (unchecked((uint)moonGuid.B) & 0x0FFFFFFFu) | 0x40000000u;
            return new MoonGuid(moonGuid.A, unchecked((int)modified), moonGuid.C, moonGuid.D);
        }

        private static void FixRenderers(Transform original, Transform copy)
        {
            // Reset textures so they don't unload.
            Renderer[] allRenderersCopy = copy.gameObject.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < allRenderersCopy.Length; i++)
            {
                Material newMaterial = new Material(allRenderersCopy[i].material);
                allRenderersCopy[i].material = newMaterial;
            }
        }


        public static void PreloadShootingSpider(SceneRoot sceneRoot)
        {
            PreloadGeneric<ShootingSpiderPlaceholder>(sceneRoot, "ShootingSpider", new MoonGuid(124156, 222533, 533, 1456), useParent: true, index: 1);
        }

        public static void PreloadAcidSlug(SceneRoot sceneRoot)
        {
            PreloadGeneric<AcidSlugEnemyPlaceholder>(sceneRoot, "AcidSlug", new MoonGuid(1256, 43222533, 533, 1456));
        }
        public static void PreloadKamikazeSoot(SceneRoot sceneRoot)
        {
            PreloadGeneric<KamikazeSootEnemyPlaceholder>(sceneRoot, "KamikazeSoot", new MoonGuid(1256, 22251433, 533, 1456));
        }

        public static void PreloadSpitterEnemy(SceneRoot sceneRoot)
        {
            PreloadGeneric<SpitterEnemyPlaceholder>(sceneRoot, "Spitter", new MoonGuid(1256, 222533, 533, 1456));
        }

        public static void PreloadStarSlugEnemy(SceneRoot sceneRoot)
        {
            PreloadGeneric<StarSlugEnemyPlaceholder>(sceneRoot, "StarSlug", new MoonGuid(1241, 243243, 134, 4211));
        }

        public static void PreloadRammingEnemy(SceneRoot sceneRoot)
        {
            PreloadGeneric<RammingEnemyPlaceholder>(sceneRoot, "RammingEnemy", new MoonGuid(141256, 222533, 533, 1456));
            Transform zone = new GameObject().transform;
            zone.parent = entities["RammingEnemy"];
            zone.name = "zone";
            RammingEnemyPlaceholder placeholder = entities["RammingEnemy"].GetComponent<RammingEnemyPlaceholder>();
            placeholder.Zones = [zone];
            //            [Error: Unity Log] NullReferenceException: Object reference not set to an instance of an object
            //Stack trace:
            //            RammingEnemy.ZoneRectanglesContain(Vector2 position)
            //RammingEnemy.get_EnemyInsideZone()
            //RammingEnemy.PlayerInsideZone()
            //RammingEnemy.CanSeePlayer()
            //RammingEnemy.< Start > m__C0()
            //fsm.FuncCondition.Validate(IContext context)
            //fsm.TransitionManager.ProcessTransitionList(fsm.StateMachine stateMachine, System.Collections.Generic.List`1 conditionAndStatePairList)
            //fsm.TransitionManager.Process(fsm.StateMachine stateMachine)
            //EntityController.FixedUpdate()
        }

        public static void LoadPlant(SceneRoot sceneRoot)
        {
            // FIXME This should be improved to the generic load.
            Plugin.Log("Getting plant.");
            MoonGuid blueWallGuid2 = new MoonGuid(1070970374, 1136043472, -2002326226, -153215581);
            // petrifiedPlant - 10 GuidOwners.
            PetrifiedPlant originalPlant = sceneRoot.transform.GetComponentInChildren<PetrifiedPlant>();
            plant = CloneForPreloading(originalPlant.transform, "PetrifiedPlant", blueWallGuid2);
            plant.gameObject.SetActive(false);
            plant.parent = null;
            GameObject.DontDestroyOnLoad(plant.gameObject);
            FixRenderers(originalPlant.transform, plant);
            CheckOwners(plant);
        }

        public static void LoadJumperEnemy(SceneRoot sceneRoot)
        {
            // FIXME This should be improved to the generic load.
            Plugin.Log("Getting jumper enemy");
            JumperEnemyPlaceholder jumper = sceneRoot.transform.GetComponentInChildren<JumperEnemyPlaceholder>();
            if (jumper == null)
            {
                Plugin.Log("Could not find jumper!.");
                return;
            }
            MoonGuid blueWallGuid = new MoonGuid(1070970374, 1136043472, -2002326366, -1532129681);
            jumperEnemy = CloneForPreloading(jumper.transform, "JumperEnemyPlaceholder", blueWallGuid);
            //blueWall = RandomizerBootstrap.CloneObject(sceneRoot, wall, "blueWall");
            jumperEnemy.gameObject.SetActive(false);
            jumperEnemy.parent = null;
            GameObject.DontDestroyOnLoad(jumperEnemy.gameObject);
            FixRenderers(jumper.transform, jumperEnemy);
            CheckOwners(jumperEnemy);
        }

        public static Transform PlacePlant(SceneRoot sceneRoot, Vector3 position, MoonGuid moonGuid)
        {
            Transform clone = UnityEngine.Object.Instantiate<Transform>(plant);
            clone.parent = sceneRoot.transform;
            clone.gameObject.GetComponentInChildren<PetrifiedPlant>().SetSceneRoot(sceneRoot.MetaData.SceneMoonGuid); // FIXME Is this needed?
            clone.position = position;
            int copyNumber = 1;
            foreach (GuidOwner owner in clone.gameObject.FindComponentsInChildren<GuidOwner>())
            {
                MoonGuid guid = TwiddleGuid(moonGuid, copyNumber);
                copyNumber++;
                SetGuidAndSave(sceneRoot, owner, guid);
            }
            clone.gameObject.SetActive(true);
            return clone;
        }

        public static Transform PlaceGeneric(SceneRoot sceneRoot, string name, Vector3 position, MoonGuid moonGuid)
        {
            if (!entities.ContainsKey(name))
            {
                Plugin.Log("Unable to place " + name);
                return null;
            }
            Transform clone = UnityEngine.Object.Instantiate<Transform>(entities[name]);
            clone.parent = sceneRoot.transform;
            //clone.gameObject.GetComponentInChildren<PetrifiedPlant>().SetSceneRoot(sceneRoot.MetaData.SceneMoonGuid);
            clone.position = position;
            int copyNumber = 1;
            foreach (GuidOwner owner in clone.gameObject.FindComponentsInChildren<GuidOwner>())
            {
                MoonGuid guid = TwiddleGuid(moonGuid, copyNumber);
                copyNumber++;
                SetGuidAndSave(sceneRoot, owner, guid);
            }
            clone.gameObject.SetActive(true);
            return clone;
        }

        public static Transform PlaceJumperEnemy(SceneRoot sceneRoot, Vector3 position, MoonGuid moonGuid)
        {
            Transform clone = UnityEngine.Object.Instantiate<Transform>(jumperEnemy);
            clone.parent = sceneRoot.transform;
            //clone.gameObject.GetComponentInChildren<PetrifiedPlant>().SetSceneRoot(sceneRoot.MetaData.SceneMoonGuid);
            clone.position = position;
            int copyNumber = 1;
            foreach (GuidOwner owner in clone.gameObject.FindComponentsInChildren<GuidOwner>())
            {
                MoonGuid guid = TwiddleGuid(moonGuid, copyNumber);
                copyNumber++;
                SetGuidAndSave(sceneRoot, owner, guid);
            }
            clone.gameObject.SetActive(true);
            return clone;
        }
    }


}
