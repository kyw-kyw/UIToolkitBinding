#nullable enable

using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sample
{
    public class CounterPresenter : MonoBehaviour
    {
        [SerializeField] UIDocument document = null!;

        Counter counter = null!;
        CounterView view = null!;

        void Awake()
        {
            counter = SaveFile.Load() ?? new();
            view = new CounterView(document.rootVisualElement);
            RegisterCallbacks();
        }

        void OnEnable()
        {
            view.Bind(counter);
        }

        void OnDisable()
        {
            view.Unbind();
        }

        void OnDestroy()
        {
            view.Dispose();
        }

        void RegisterCallbacks()
        {
            view.OnCountUpButtonClicked += OnCountUpButtonClicked;
            view.OnCountDownButtonClicked += OnCountDownButtonClicked;
            view.OnResetButtonClicked += OnResetButtonClicked;
            view.OnQuitButtonClicked += OnCloseButtonClicked;
        }

        void UnregisterCallbacks()
        {
            view.OnCountUpButtonClicked -= OnCountUpButtonClicked;
            view.OnCountDownButtonClicked -= OnCountDownButtonClicked;
            view.OnResetButtonClicked -= OnResetButtonClicked;
            view.OnQuitButtonClicked -= OnCloseButtonClicked;
        }

        void OnCountUpButtonClicked()
        {
            counter.CountUp();
        }

        void OnCountDownButtonClicked()
        {
            counter.CountDown();
        }

        void OnResetButtonClicked()
        {
            counter.Reset();
        }

        void OnCloseButtonClicked()
        {
            SaveFile.Save(counter);
            Application.Quit();
        }

        static class SaveFile
        {
            static string path = Path.Combine(Application.persistentDataPath, "counter.json");

            public static Counter? Load()
            {
                if (!File.Exists(path)) return null;

                var json = File.ReadAllText(path);
                return JsonUtility.FromJson<Counter>(json);
            }

            public static void Save(Counter counter)
            {
                var json = JsonUtility.ToJson(counter);
                File.WriteAllText(path, json);
            }
        }
    }
}
