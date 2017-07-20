using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Konachan.Controls
{
    class ScalableGrid : Grid
    {
        private TransformGroup transformGroup;
        private ScaleTransform scaleTransform;
        private TranslateTransform translateTransform;
        public ScalableGrid()
        {
            scaleTransform = new ScaleTransform();
            transformGroup = new TransformGroup();
            translateTransform = new TranslateTransform();
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(translateTransform);
            RenderTransform = transformGroup;
            ManipulationMode = ManipulationModes.System | ManipulationModes.Scale;
            ManipulationDelta += ScalableGrid_ManipulationDelta;
            Loaded += ScalableGrid_Loaded;
            SizeChanged += (a, b) =>
            {
                scaleTransform.CenterX = ActualWidth / 2;
                scaleTransform.CenterY = ActualHeight / 2;
            };
            DoubleTapped += ScalableGrid_DoubleTapped;
        }

        private void ScalableGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            scaleTransform.ScaleX = scaleTransform.ScaleY = 1;
            translateTransform.X = translateTransform.Y = 0;
            ManipulationMode = ManipulationModes.System | ManipulationModes.Scale;
        }

        private void ScalableGrid_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Loaded -= ScalableGrid_Loaded;
            scaleTransform.CenterX = ActualWidth / 2;
            scaleTransform.CenterY = ActualHeight / 2;
        }

        private void ScalableGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (scaleTransform.ScaleX == 1 && scaleTransform.ScaleY == 1)
            {
                ManipulationMode = ManipulationModes.System | ManipulationModes.Scale;
            }
            else
            {
                ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.Scale | ManipulationModes.TranslateInertia;
            }
            if (scaleTransform.ScaleX < 3 && scaleTransform.ScaleX > 0.8) 
            {
                scaleTransform.ScaleX *= e.Delta.Scale;
            }
            if (scaleTransform.ScaleY < 3 && scaleTransform.ScaleY > 0.8) 
            {
                scaleTransform.ScaleY *= e.Delta.Scale;
            }
            translateTransform.X += e.Delta.Translation.X;
            translateTransform.Y += e.Delta.Translation.Y;
        }
    }
}
