using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Events;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

[AddComponentMenu("Localization/Localize Animator Event")]
public class LocalizeAnimatorEvent : LocalizedAssetEvent<RuntimeAnimatorController, LocalizedAsset<RuntimeAnimatorController>, UnityEvent<RuntimeAnimatorController>>
{
}