using System;
using UnityEngine.UIElements;

namespace Sample
{
    public class CounterView : IDisposable
    {
        readonly VisualElement root;
        readonly Button countUpButton;
        readonly Button countDownButton;
        readonly Button resetButton;
        readonly Button quitButton;

        public event Action OnCountUpButtonClicked;
        public event Action OnCountDownButtonClicked;
        public event Action OnResetButtonClicked;
        public event Action OnQuitButtonClicked;

        public CounterView(VisualElement root)
        {
            this.root = root.Q<VisualElement>("counter__main-container");
            countUpButton = root.Q<Button>("counter-add-button");
            countDownButton = root.Q<Button>("counter-sub-button");
            resetButton = root.Q<Button>("counter-reset-button");
            quitButton = root.Q<Button>("counter-quit-button");
            RegisterCallbacks();
        }

        public void Bind(Counter counter)
        {
            root.dataSource = counter;
        }

        public void Unbind()
        {
            root.dataSource = null;
        }

        public void Dispose()
        {
            UnregisterCallbacks();
        }

        void RegisterCallbacks()
        {
            countUpButton.RegisterCallback<ClickEvent>(ClickEvent_CountUpButton);
            countDownButton.RegisterCallback<ClickEvent>(ClickEvent_CountDownButton);
            resetButton.RegisterCallback<ClickEvent>(ClickEvent_ResetButton);
            quitButton.RegisterCallback<ClickEvent>(ClickEvent_QuitButton);
        }

        void UnregisterCallbacks()
        {
            countUpButton.UnregisterCallback<ClickEvent>(ClickEvent_CountUpButton);
            countDownButton.UnregisterCallback<ClickEvent>(ClickEvent_ResetButton);
            resetButton.UnregisterCallback<ClickEvent>(ClickEvent_ResetButton);
            quitButton.UnregisterCallback<ClickEvent>(ClickEvent_QuitButton);
        }

        void ClickEvent_CountUpButton(ClickEvent evt)
        {
            OnCountUpButtonClicked?.Invoke();
        }

        void ClickEvent_CountDownButton(ClickEvent evt)
        {
            OnCountDownButtonClicked?.Invoke();
        }

        void ClickEvent_ResetButton(ClickEvent evt)
        {
            OnResetButtonClicked?.Invoke();
        }

        void ClickEvent_QuitButton(ClickEvent evt)
        {
            OnQuitButtonClicked?.Invoke();
        }
    }
}
