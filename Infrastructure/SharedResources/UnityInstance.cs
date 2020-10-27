using System.Windows;
using Prism.Ioc;

namespace Infrastructure.SharedResources {
    public static class UnityInstance {
        public const string CONTAINER_NAME = nameof(CONTAINER_NAME);
        public static IContainerProvider GetContainer() => (IContainerProvider) Application.Current.Resources[CONTAINER_NAME];
    }
}