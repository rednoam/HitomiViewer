using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HitomiViewer.Style
{
    /// <summary>
    /// tag.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class tag : UserControl
    {
        public Tag.Types TagType
        {
            get { return (Tag.Types)GetValue(TagTypeProperty); }
            set { SetValue(TagTypeProperty, value); }
        }
        public string TagName
        {
            get { return (string)GetValue(TagNameProperty); }
            set { SetValue(TagNameProperty, value); }
        }
        public Brush TagColor {
            get { return (Brush)GetValue(TagColorProperty); }
            set { SetValue(TagColorProperty, value); }
        }

        public tag()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static readonly DependencyProperty TagTypeProperty
            = DependencyProperty.Register(
                "TagType",
                typeof(Tag.Types),
                typeof(tag),
                new PropertyMetadata(HitomiViewer.Tag.Types.tag)
            );
        public static readonly DependencyProperty TagNameProperty
            = DependencyProperty.Register(
                "TagName",
                typeof(string),
                typeof(tag),
                new PropertyMetadata("태그없음")
            );
        public static readonly DependencyProperty TagColorProperty
            = DependencyProperty.Register(
                "TagColor",
                typeof(Brush),
                typeof(tag),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 94, 94)))
            );
    }
}
