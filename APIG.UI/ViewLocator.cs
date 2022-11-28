using System;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using APIG.UI.ViewModels;

namespace APIG.UI;

public class ViewLocator : IDataTemplate
{
    public IControl Build(object? data)
    {
        var name = data?.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name ?? string.Empty);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}