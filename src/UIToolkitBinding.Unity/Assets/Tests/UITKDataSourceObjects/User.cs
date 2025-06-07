using UnityEngine.UIElements;

namespace UIToolkitBinding.Unity.Tests
{
    internal class User
    {
        public string Name { get; set; }
    }

    [UITKDataSourceObject]
    internal partial class BindableUser
    {
        internal readonly BindablePropertyChangedEventArgs NameChanged = new("Name");
        public BindableUser(User user)
        {
            UserProxy = user;
        }

        public User UserProxy { get; }

        public string Name
        {
            get => UserProxy.Name;
            set => SetProperty(UserProxy.Name, value, UserProxy, (user, name) => user.Name = name, NameChanged);
        }
    }

    [UITKDataSourceObject]
    internal partial class UITKUser
    {
        [UITKBindableField]
        string name;
    }
}
