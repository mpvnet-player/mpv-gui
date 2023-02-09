﻿
using System.Windows;
using System.Windows.Input;

namespace mpvgui.WinFormsWPF;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        DataContext = this;
        ContentBlock.Text = App.About;
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e) => Close();
    protected override void OnMouseDown(MouseButtonEventArgs e) => Close();

    public static Theme? Theme => Theme.Current;
}
