using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

[AddComponentMenu("Localization/Asset/Localize Animator Event")]
public class LocalizeAnimatorEvent : LocalizedAssetEvent<RuntimeAnimatorController, LocalizedAsset<RuntimeAnimatorController>, UnityEvent<RuntimeAnimatorController>>
{
}