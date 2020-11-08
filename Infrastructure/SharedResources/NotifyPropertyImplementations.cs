using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Infrastructure.SharedResources {


    public abstract class NotifyPropertyChanged : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DependsOnPropertyAttribute : Attribute {
        public readonly string dependence;
        public DependsOnPropertyAttribute(string otherProperty) => dependence = otherProperty;
    }

    public abstract class NotifyPropertyWithDependencies : INotifyPropertyWithDependencies {
        public event PropertyChangedEventHandler PropertyChanged;
        public PropertyChangedEventHandler GetPropertyChangedEventHandler => PropertyChanged;
        public Dictionary<string, List<string>> DependencyMap { get; set; }

        protected NotifyPropertyWithDependencies() { this.InitializeDependencyMap(); }
    }

    public interface INotifyPropertyWithDependencies : INotifyPropertyChanged {
        PropertyChangedEventHandler GetPropertyChangedEventHandler { get; }
        public Dictionary<string, List<string>> DependencyMap { get; set; }
    }

    // ReSharper disable once InconsistentNaming
    public static class INotifyPropertyWithDependenciesExtensions {
        public static void InitializeDependencyMap(this INotifyPropertyWithDependencies inpwd) {
            inpwd.DependencyMap = new Dictionary<string, List<string>>();

            foreach(var property in inpwd.GetType().GetProperties()) {
                foreach(var dependsAttr in property.GetCustomAttributes<DependsOnPropertyAttribute>()) {
                    string dependence = dependsAttr.dependence;
                    if(!inpwd.DependencyMap.ContainsKey(dependence)) inpwd.DependencyMap.Add(dependence, new List<string>());
                    inpwd.DependencyMap[dependence].Add(property.Name);
                }
            }
        }

        public static void OnPropertyChanged(this INotifyPropertyWithDependencies inpwd, [CallerMemberName] string propertyName = null) {
            PropertyChangedEventHandler handler = inpwd.GetPropertyChangedEventHandler;
            if(handler == null) return;

            handler(inpwd, new PropertyChangedEventArgs(propertyName));

            if(!inpwd.DependencyMap.ContainsKey(propertyName)) return;

            foreach(string dependentProperty in inpwd.DependencyMap[propertyName])
                handler(inpwd, new PropertyChangedEventArgs(dependentProperty));
        }
    }
}