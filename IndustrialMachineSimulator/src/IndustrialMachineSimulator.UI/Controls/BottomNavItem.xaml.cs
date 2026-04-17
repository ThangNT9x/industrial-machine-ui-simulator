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

namespace IndustrialMachineSimulator.UI.Controls
{
    /// <summary>
    /// Interaction logic for BottomNavItem.xaml
    /// </summary>
    public partial class BottomNavItem : UserControl
    {
        public BottomNavItem()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty IconGlyphProperty = 
            DependencyProperty.Register(
                nameof(IconGlyph),
                typeof(string),
                typeof(BottomNavItem),
                new PropertyMetadata(string.Empty) );
            
        public string IconGlyph
        {
            get => (string)GetValue(IconGlyphProperty);
            set=> SetValue(IconGlyphProperty, value);
        }
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                nameof(Label),
                typeof(string),
                typeof(BottomNavItem),
                new PropertyMetadata(string.Empty) );
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set=> SetValue(LabelProperty, value);
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(BottomNavItem),
                new PropertyMetadata(null));
        public ICommand Command
        {
            get=>(ICommand)GetValue(CommandProperty);
            set=> SetValue(CommandProperty, value);
        }
        public static readonly DependencyProperty IsItemEnabledProperty =
            DependencyProperty.Register(
                nameof(IsItemEnabled),
                typeof(bool),
                typeof(BottomNavItem),
                new PropertyMetadata(true)
                );
        public bool IsItemEnabled
        {
            get=>(bool)GetValue(IsItemEnabledProperty);
            set=> SetValue(IsItemEnabledProperty, value);
        }
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
                nameof(IsActive),
                typeof(bool),
                typeof(BottomNavItem),
                new PropertyMetadata(false));
        public bool IsActive
        { 
            get=>(bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }
        public static readonly DependencyProperty IconNormalProperty =
            DependencyProperty.Register(
                nameof(IconNormal),
                typeof(string),
                typeof(BottomNavItem),
                new PropertyMetadata(string.Empty)
                );
        public string IconNormal
        {
            get=>(string)GetValue(IconNormalProperty);
            set=>SetValue(IconNormalProperty, value);
        }
        public static readonly DependencyProperty IconActiveProperty =
            DependencyProperty.Register(
                nameof(IconActive),
                typeof(string),
                typeof(BottomNavItem),
                new PropertyMetadata(string.Empty)
                );
        public string IconActive
        {
            get => (string)GetValue(IconActiveProperty);
            set => SetValue(IconNormalProperty, value);
        }

    }
}
