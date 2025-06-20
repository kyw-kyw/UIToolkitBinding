﻿# UI Toolkit Binding

[![license](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
![](https://img.shields.io/badge/Unity-6.0-green.svg)

[日本語版READMEはこちら](README_JA.md)

C# Source Generator for UI Toolkit that will help greatly reduce boilerplate when implement ```INotifyBindablePropertyChanged```.

## Setup

### Requirements

* Unity6.0 or higher

### Installs

To install the software, follow the steps below.

1. Open Package Manager from Window > Package Manager
2. Click on the "+" button > Add package from git URL
3. Enter the following URL:

```
https://github.com/kyw-kyw/UIToolkitBinding.git?path=src/UIToolkitBinding.UnityPackage
```

Or open Packages/manifest.json and add the following to the dependencies block:

```json
{
    "dependencies": {
        "jp.kyw-kyw.ui-tool-kit-binding": "https://github.com/kyw-kyw/UIToolkitBinding.git?path=src/UIToolkitBinding.UnityPackage"
    }
}
```

## Basic Usage

Original source (manually written):

```cs
using UIToolkitBinding;

namespace Sample
{
    // This attribute allows UIToolkitBinding's source generator to recognize.
    // partial keyword must be required.
    [UITKDataSourceObject]
    public partial class Counter
    {
        /*
        Access modifiers for generated properties can be specified by DeclaredAccessibility and SetterAccessibility.

        The generated properties will automatically use the UpperCamelCase format for their names, which will be derived from the field names.
        The generator can also recognize fields using either _lowerCamel or m_lowerCamel naming scheme. (_count -> Count / m_count -> Count)
        Otherwise, the first character in the source field name will be converted to uppercase
        */
        [UITKBindableField]
        private int count;
    }
}
```

Generated source:

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
