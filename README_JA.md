# UI Toolkit Binding

[![license](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
![](https://img.shields.io/badge/Unity-6.0-green.svg)

[English README is here](README.md)

```INotifyBindablePropertyChanged```を実装するときに定型文を大幅に削減するのに役立つ UI ツールキット用の C# ソース ジェネレーターです。

## セットアップ

### 要件

* Unity6.0 以上

### インストール

To install the software, follow the steps below.

1. Window > Package Manager を選択
2. 「+」ボタン > Add package from git URL を選択
3. 以下の順番に入力してインストール

```
https://github.com/kyw-kyw/UIToolkitBinding.git?path=src/UIToolkitBinding.UnityPackage
```

あるいはPackages/manifest.jsonを開き、dependenciesブロックに以下を追記

```json
{
    "dependencies": {
        "jp.kyw-kyw.ui-tool-kit-binding": "https://github.com/kyw-kyw/UIToolkitBinding.git?path=src/UIToolkitBinding.UnityPackage"
    }
}
```

## 使い方

オリジナルコード:

```cs
using UIToolkitBinding;

namespace Sample
{
    // この属性によって、ソースジェネレーターに認識させます。
    // partialキーワードが必須です。
    [UITKDataSourceObject]
    public partial class Counter
    {
        /*
        生成されるアクセス修飾子は DeclaredAccessibility、SetterAccessibilityによって指定可能です

        生成されるプロパティの名前には、フィールド名から派生した UpperCamelCase 形式が自動的に使用されます。
        ソースジェネレーターは、_lowerCamel または m_lowerCamel 命名規則を使用したフィールドも認識できます。(_count -> Count / m_count -> Count)
        それ以外の場合、ソースフィールド名の最初の文字は大文字に変換されます。
        */
        [UITKBindableField]
        private int count;
    }
}

```

生成されるコード:

```cs
using System;
using System.Collections.Generic;
using UIToolkitBinding;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sample
{
    partial class Counter : INotifyBindablePropertyChanged
    {
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        [CreateProperty] 
        public int Count
        {
            get => count;
            set
            {
                if (global::System.Collections.Generic.EqualityComparer<int>.Default.Equals(count, value)) return;
                var oldValue = count;
                OnCountChanging();
                OnCountChanging(value);
                OnCountChanging(oldValue, value);
                count = value;
                OnCountChanged();
                OnCountChanged(value);
                OnCountChanged(oldValue, value);
                propertyChanged?.Invoke(this, GeneratedEventArgsCache.Count);
            }
        }

        protected void NotifyPropertyChanged(BindablePropertyChangedEventArgs e)
        {
            propertyChanged?.Invoke(this, e);
        }

        protected bool SetProperty<T>(ref T field, in T value, in BindablePropertyChangedEventArgs eventArgs)
        {
            if (global::System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value)) return false;

            field = value;
            propertyChanged?.Invoke(this, eventArgs);
            return true;
        }

        partial void OnCountChanging();
        partial void OnCountChanging(int newValue);
        partial void OnCountChanging(int oldValue, int newValue);
        partial void OnCountChanged();
        partial void OnCountChanged(int newValue);
        partial void OnCountChanged(int oldValue, int newValue);
    }
}

```
