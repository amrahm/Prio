using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class DependsOnPropertyAttribute : Attribute {
    public readonly string dependence;
    public DependsOnPropertyAttribute(string otherProperty) => dependence = otherProperty;
}

public abstract class NotifyPropertyWithDependencies : INotifyPropertyChanged {
    private readonly Dictionary<string, List<string>> _dependencyMap;

    protected NotifyPropertyWithDependencies() {
        _dependencyMap = new Dictionary<string, List<string>>();

        foreach(var property in GetType().GetProperties()) {
            foreach(var dependsAttr in property.GetCustomAttributes<DependsOnPropertyAttribute>()) {
                string dependence = dependsAttr.dependence;
                if(!_dependencyMap.ContainsKey(dependence))
                    _dependencyMap.Add(dependence, new List<string>());
                _dependencyMap[dependence].Add(property.Name);
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected  void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        var handler = PropertyChanged;
        if(handler == null) return;

        handler(this, new PropertyChangedEventArgs(propertyName));

        if(!_dependencyMap.ContainsKey(propertyName)) return;

        foreach(string dependentProperty in _dependencyMap[propertyName])
            handler(this, new PropertyChangedEventArgs(dependentProperty));
    }
}