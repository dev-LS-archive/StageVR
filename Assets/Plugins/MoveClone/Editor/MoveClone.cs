#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace NKStudio
{
    public class MoveClone : Editor
    {
        private static bool _shiftHeld;

        private static readonly Label DuplicateLabel = new Label("Enable Duplicate");

        [InitializeOnLoadMethod]
        private static void Init()
        {
            DuplicateLabel.style.color = new StyleColor(Color.green);
            DuplicateLabel.style.marginLeft = new StyleLength(10);
#if UNITY_2019
            DuplicateLabel.style.marginTop = new StyleLength(30);      
#else
            DuplicateLabel.style.marginTop = new StyleLength(10);
#endif
            SceneView.duringSceneGui += MoveCloneSystem;
        }

        private void OnDisable() => SceneView.duringSceneGui -= MoveCloneSystem;

        private static void MoveCloneSystem(SceneView obj)
        {
            //이벤트를 받아온다.
            Event current = Event.current;

            //쉬프트 키를 누르고 땜을 체크한다.
            switch (current.type)
            {
                case EventType.KeyUp when current.keyCode == KeyCode.A:
                    obj.rootVisualElement.Remove(DuplicateLabel);
                    _shiftHeld = false;
                    break;
                case EventType.KeyDown when current.keyCode == KeyCode.A:
                    obj.rootVisualElement.Add(DuplicateLabel);
                    _shiftHeld = true;
                    break;
            }

            //쉬프트 키가 안눌려 있거나, 마우스 클릭을 안했다면 동작하지 않는다.
            if (!_shiftHeld || current.type != EventType.MouseDown || current.button != 0) return;

            Unsupported.CopyGameObjectsToPasteboard();
            Unsupported.PasteGameObjectsFromPasteboard();
            
            _shiftHeld = false;
        }
    }
#endif
}