using System.Collections.Generic;
using System.Windows;

namespace Infrastructure.SharedResources {
    public interface IDraggable {
        public List<UIElement> ChildDraggables { get; }
    }

    public static class DraggableHelpers {
        public static void InitializeDraggable<T>(this T child) where T : UIElement, IDraggable {
            child.TryFindAncestor<IDraggable>()?.ChildDraggables.Add(child);
        }
    }
}