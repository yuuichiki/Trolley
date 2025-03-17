

using System.Windows;
using System.Windows.Controls.Primitives;

namespace Trolley.Helpers
{
  public class InfoButton : ButtonBase
  {
    public static readonly DependencyProperty InfoProperty = DependencyProperty.Register(nameof (Info), typeof (string), typeof (InfoButton), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty CornProperty = DependencyProperty.Register(nameof (Corn), typeof (string), typeof (InfoButton), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty CornFontSizeProperty = DependencyProperty.Register(nameof (CornFontSize), typeof (string), typeof (InfoButton), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ButtonContentProperty = DependencyProperty.Register(nameof (ButtonContent), typeof (string), typeof (InfoButton), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ContentForegroundProperty = DependencyProperty.Register(nameof (ContentForeground), typeof (string), typeof (InfoButton), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ContentFontSizeProperty = DependencyProperty.Register(nameof (ContentFontSize), typeof (string), typeof (InfoButton), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register(nameof (ImageWidth), typeof (string), typeof (InfoButton), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ImageHeightProperty = DependencyProperty.Register(nameof (ImageHeight), typeof (string), typeof (InfoButton), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ContentMarginProperty = DependencyProperty.Register(nameof (ContentMargin), typeof (string), typeof (InfoButton), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ImageBackgroundProperty = DependencyProperty.Register(nameof (ImageBackground), typeof (string), typeof (InfoButton), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ImageMarginProperty = DependencyProperty.Register(nameof (ImageMargin), typeof (string), typeof (InfoButton), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ButtonOrientationProperty = DependencyProperty.Register(nameof (ButtonOrientation), typeof (string), typeof (InfoButton), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ButtonBackgroundProperty = DependencyProperty.Register(nameof (ButtonBackground), typeof (string), typeof (InfoButton), new PropertyMetadata((object) ""));

    public string Info
    {
      get => (string) this.GetValue(InfoButton.InfoProperty);
      set => this.SetValue(InfoButton.InfoProperty, (object) value);
    }

    public string Corn
    {
      get => (string) this.GetValue(InfoButton.CornProperty);
      set => this.SetValue(InfoButton.CornProperty, (object) value);
    }

    public string CornFontSize
    {
      get => (string) this.GetValue(InfoButton.CornFontSizeProperty);
      set => this.SetValue(InfoButton.CornFontSizeProperty, (object) value);
    }

    public string ButtonContent
    {
      get => (string) this.GetValue(InfoButton.ButtonContentProperty);
      set => this.SetValue(InfoButton.ButtonContentProperty, (object) value);
    }

    public string ContentForeground
    {
      get => (string) this.GetValue(InfoButton.ContentForegroundProperty);
      set => this.SetValue(InfoButton.ContentForegroundProperty, (object) value);
    }

    public string ContentFontSize
    {
      get => (string) this.GetValue(InfoButton.ContentFontSizeProperty);
      set => this.SetValue(InfoButton.ContentFontSizeProperty, (object) value);
    }

    public string ImageWidth
    {
      get => (string) this.GetValue(InfoButton.ImageWidthProperty);
      set => this.SetValue(InfoButton.ImageWidthProperty, (object) value);
    }

    public string ImageHeight
    {
      get => (string) this.GetValue(InfoButton.ImageHeightProperty);
      set => this.SetValue(InfoButton.ImageHeightProperty, (object) value);
    }

    public string ContentMargin
    {
      get => (string) this.GetValue(InfoButton.ContentMarginProperty);
      set => this.SetValue(InfoButton.ContentMarginProperty, (object) value);
    }

    public string ImageBackground
    {
      get => (string) this.GetValue(InfoButton.ImageBackgroundProperty);
      set => this.SetValue(InfoButton.ImageBackgroundProperty, (object) value);
    }

    public string ImageMargin
    {
      get => (string) this.GetValue(InfoButton.ImageMarginProperty);
      set => this.SetValue(InfoButton.ImageMarginProperty, (object) value);
    }

    public string ButtonOrientation
    {
      get => (string) this.GetValue(InfoButton.ButtonOrientationProperty);
      set => this.SetValue(InfoButton.ButtonOrientationProperty, (object) value);
    }

    public string ButtonBackground
    {
      get => (string) this.GetValue(InfoButton.ButtonBackgroundProperty);
      set => this.SetValue(InfoButton.ButtonBackgroundProperty, (object) value);
    }
  }
}
