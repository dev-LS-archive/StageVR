using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Events;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

[AddComponentMenu("Localization/Asset/Localize Font(Text) Event")]
public class LocalizeFontEvent : LocalizedAssetEvent<Font, LocalizedFont, UnityEvent<Font>>
{
}
