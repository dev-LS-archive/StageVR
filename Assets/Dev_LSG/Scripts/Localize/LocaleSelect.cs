using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;
using UnityEngine.Serialization;

public class LocaleSelect : MonoBehaviour
{
    public UnityEvent selectedEvent;
    public void LocaleSelected(int index)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        selectedEvent.Invoke();
    }
}
