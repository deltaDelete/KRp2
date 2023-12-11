using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

namespace KR2.Utils; 


public class MessageBoxUtils
{
    public static Task<ContentDialogResult> ShowSimpleMessageBox(string title, string message) {
        var dialog = new ContentDialog() {
            Title = title,
            PrimaryButtonText = "Ок",
            Content = new StackPanel {
                Spacing = 15,
                Children = {
                    new TextBlock() {
                        Text = message
                    }
                }
            },
        };
        return dialog.ShowAsync();
    }
    
    public static Task<ContentDialogResult> ShowYesNoDialog(string title, string message) {
        var dialog = new ContentDialog() {
            Title = title,
            PrimaryButtonText = "Да",
            SecondaryButtonText = "Нет",
            Content = new StackPanel {
                Spacing = 15,
                Children = {
                    new TextBlock() {
                        Text = message
                    }
                }
            },
        };
        return dialog.ShowAsync();
    }
}