using System;
using System.Collections;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using WalkThereYourself;


namespace WalkThereYourself
{
    [BepInPlugin("WalkThereYourself.DolfeMods.PeaksOfYore", "WalkThereYourself", "1.0.0")]
    public class Class1 : BaseUnityPlugin
    {
        public static bool IDKHowToNameVariables = true;

        void Awake()
        {
            Harmony harmony = new Harmony("WalkThereYourself.DolfeMods.PeaksOfYore");
            harmony.PatchAll();

            string bundlePath = Path.Combine(Paths.PluginPath, "WalkThereYourself", "WalkScene");
            LoadAssets(bundlePath);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode lsm)
        {

            if (scene.name == "WalkThereYourself")
            {
                GameObject LOADACTUALSCENE = GameObject.Find("___LOADACTUALSCENE");
                LOADACTUALSCENE.AddComponent<LoadWaterfall>();
            }
        }

        static void LoadAssets(string path)
        {
            AssetBundle loadedBundle = AssetBundle.LoadFromFile(path);
            if (loadedBundle == null)
            {
                Debug.LogError("Failed to load AssetBundle!");
                return;
            }


            string[] scenePaths = loadedBundle.GetAllScenePaths();
            if (scenePaths.Length > 0)
            {
                string sceneName = Path.GetFileNameWithoutExtension(scenePaths[0]);
                Debug.Log($"Found scene: {sceneName}");
            }
            else
            {
                Debug.LogError("No scenes found in the AssetBundle!");
            }
        }
    }

    public class LoadWaterfall : MonoBehaviour
    {
        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("PlayersEquipment"))
            {
                SceneManager.LoadScene("Category4_1_FrozenWaterfall");
            }
        }
    }
}
// Category4CabinLeave

[HarmonyPatch(typeof(Category4CabinLeave), "ExitCabinEvent")]
class CabinLeavePatch
{
    [HarmonyPrefix]
    public static bool ExitCabinEventPrefix(Category4CabinLeave __instance,
        DisableCabin4Flag ___disableCabin4Flag,
        bool ___enteringFrozenWaterfall,
        CameraLook ___camX,
        Transform ___playerCamTransform,
        Transform ___playerCamTransformY,
        Animation ___playerAnimation,
        AnimationClip ___exitCabin4Clip,
        float ___lerpSpeed,
        Transform ___doorCameraPosition,
        CanvasGroup ___blackscreen)
    {
        __instance.StartCoroutine(CustomExitCabinEvent(
            ___disableCabin4Flag, ___enteringFrozenWaterfall, ___camX,
            ___playerCamTransform, ___playerCamTransformY, ___playerAnimation,
            ___exitCabin4Clip, ___lerpSpeed, ___doorCameraPosition, ___blackscreen
        ));
        return false; // Skip the original method
    }

    private static IEnumerator CustomExitCabinEvent(
        DisableCabin4Flag disableCabin4Flag,
        bool enteringFrozenWaterfall,
        CameraLook camX,
        Transform playerCamTransform,
        Transform playerCamTransformY,
        Animation playerAnimation,
        AnimationClip exitCabin4Clip,
        float lerpSpeed,
        Transform doorCameraPosition,
        CanvasGroup blackscreen)
    {
        disableCabin4Flag.AnimDisableCabinFlag();
        enteringFrozenWaterfall = true;
        camX.doFollow = false;

        Vector3 originalCamPos = playerCamTransform.position;
        Quaternion originalCamRot = playerCamTransform.rotation;
        Quaternion originalCamRotY = playerCamTransformY.rotation;

        bool playingAnim = false;
        float elapsed2 = 0f;

        while (elapsed2 < 1f)
        {
            elapsed2 += lerpSpeed * Time.deltaTime;
            playerCamTransform.position = Vector3.Lerp(originalCamPos, doorCameraPosition.position, elapsed2 / 1f);
            playerCamTransform.rotation = Quaternion.Lerp(originalCamRot, Quaternion.Euler(0f, -90f, 0f), elapsed2 / 1f);
            playerCamTransformY.rotation = Quaternion.Lerp(originalCamRotY, playerCamTransformY.parent.rotation, elapsed2 / 1f);

            if (elapsed2 > 0.6f && !playingAnim)
            {
                playingAnim = true;
                playerAnimation.clip = exitCabin4Clip;
                playerAnimation.Play();
            }
            yield return null;
        }

        yield return new WaitForSeconds(6.5f);
        elapsed2 = 0f;

        while (elapsed2 < 1f)
        {
            elapsed2 += 2f * Time.deltaTime;
            blackscreen.alpha = elapsed2;
            AudioListener.volume = Mathf.Lerp(1f, 0f, elapsed2);
            yield return null;
        }

        AudioListener.volume = 0f;
        enteringFrozenWaterfall = false;
        SceneManager.LoadScene(Class1.IDKHowToNameVariables ? "WalkThereYourself" : "Category4_1_FrozenWaterfall");
    }
}
