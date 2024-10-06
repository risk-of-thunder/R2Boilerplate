using BepInEx;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ToggledChargedGauntlet
{
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]

    [BepInPlugin(
        "com.MyName.IHerebyGrantPermissionToDeprecateMyModFromThunderstoreBecauseIHaveNotChangedTheName",
        "IHerebyGrantPermissionToDeprecateMyModFromThunderstoreBecauseIHaveNotChangedTheName",
        "1.0.0")]
    public class CustomSkillTutorial : BaseUnityPlugin
    {
        public void Awake()
        {
            // First we must load our survivor's Body prefab. For this tutorial, we are making a skill for Commando
            // If you would like to load a different survivor, you can find the key for their Body prefab at the following link
            // https://xiaoxiao921.github.io/GithubActionCacheTest/assetPathsDump.html
            GameObject loaderBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/LoaderBody.prefab").WaitForCompletion();


            // Now we add our skill to one of the survivor's skill families
            // You can change component.primary to component.secondary, component.utility and component.special
            SkillLocator skillLocator = loaderBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily skillFamily = skillLocator.utility.skillFamily;

            // Now we must create a SkillDef
            skillFamily.variants[0].skillDef.activationState = new SerializableEntityStateType(typeof(ToggledChargeFist));








        }
    }
}