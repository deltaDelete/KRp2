using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;

namespace KR2.Views;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
    }

    private void NavView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e) {
        if (e.SelectedItem is NavigationViewItem { Tag: Type type }) {
            NavFrame.Navigate(type, null, new SlideNavigationTransitionInfo() {
                Effect = SlideNavigationTransitionEffect.FromBottom
            });
        }
    }
}