using System.Windows;
using System.Windows.Controls;

namespace Infrastructure.SharedResources {
    public class DropableCanvas : Canvas {
        public DropableCanvas() {
            AllowDrop = true;
            Drop += DropableCanvas_Drop;
        }

        void DropableCanvas_Drop(object sender, DragEventArgs e) {
            if(e.Data.GetDataPresent(typeof(DropableCanvasDragDropData))) {
                if(e.Data.GetData(typeof(DropableCanvasDragDropData)) is DropableCanvasDragDropData dragdropdata) {
                    // Remove usercontrol from its starting panel
                    dragdropdata.SourcePanel.Children.Remove(dragdropdata.UserControl);

                    // Position the usercontrol on this canvas at the drop point
                    Point dropPoint = e.GetPosition(this);
                    SetLeft(dragdropdata.UserControl, dropPoint.X - dragdropdata.OffsetPoint.X);
                    SetTop(dragdropdata.UserControl, dropPoint.Y - dragdropdata.OffsetPoint.Y);

                    // Add the usercontrol to this canvas
                    Children.Add(dragdropdata.UserControl);
                }
            }
            e.Handled = true;
        }
    }

    public class DropableCanvasDragDropData {
        public DropableCanvasDragDropData(Panel srcpanel, UserControl control, Point pt) {
            SourcePanel = srcpanel;
            UserControl = control;
            OffsetPoint = pt;
        }

        public Panel SourcePanel { get; protected set; }
        public UserControl UserControl { get; protected set; }
        public Point OffsetPoint { get; protected set; }
    }
}