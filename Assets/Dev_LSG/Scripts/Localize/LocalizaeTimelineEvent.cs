using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Playables;

[AddComponentMenu("Localization/Asset/Localize Timeline Event")]
public class LocalizaeTimelineEvent : LocalizedAssetEvent<PlayableAsset, LocalizedAsset<PlayableAsset>, UnityEvent<PlayableAsset>>
{
}
