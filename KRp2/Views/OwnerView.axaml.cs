using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;
using KR2.Models;
using KR2.Utils;
using KR2.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace KR2.Views; 

public partial class OwnerView : ReactiveUserControl<TableViewModelBase<Owner>> {
    public OwnerView() {
        InitializeComponent();
        ViewModel = new(
            () => new AppDatabase().Owners
                .ToList(),
            OrderSelectors,
            DefaultOrderSelector,
            FilterSelectors,
            DefaultFilterSelector,
            EditItem,
            NewItem,
            RemoveItem
        );
    }

    private async void RemoveItem(Owner? i) {
        if (i is null) {
            return;
        }

        var result = await MessageBoxUtils.ShowYesNoDialog(
                         "Подтверждение",
                         $"Вы действительно хотите удалить запись"
                     );
        if (result is not ContentDialogResult.Primary) return;
        await using var db = new AppDatabase();
        db.Owners.Remove(i);
        await db.SaveChangesAsync();
        ViewModel?.RemoveLocal(i);
    }

    private async Task NewItem() {
        var itemToEdit = new Owner();
        var stack = GenerateDialogPanel(itemToEdit);

        var dialog = new ContentDialog() {
            Title = "Добавление записи",
            PrimaryButtonText = "Создать",
            CloseButtonText = "Закрыть",
            DataContext = itemToEdit,
            Content = stack,
            DefaultButton = ContentDialogButton.Primary,
            [!ContentDialog.PrimaryButtonCommandParameterProperty] = new Binding(".")
        };

        dialog.AddControlValidation<Owner>(stack.Children, async item => {
            if (item is null) return;
            item = item.Clone();
            await using var db = new AppDatabase();
            db.Attach(item);
            db.Owners.Add(item);
            await db.SaveChangesAsync();
            ViewModel!.AddLocal(item);
        });
        await dialog.ShowAsync();
    }

    private async void EditItem(Owner? i) {
        if (i is null) return;
        var stack = GenerateDialogPanel(i);
        var dialog = new ContentDialog() {
            Title = "Изменение записи",
            PrimaryButtonText = "Изменить",
            CloseButtonText = "Закрыть",
            DataContext = i.Clone(),
            Content = stack,
            DefaultButton = ContentDialogButton.Primary,
            [!ContentDialog.PrimaryButtonCommandParameterProperty] = new Binding(".")
        };

        dialog.AddControlValidation<Owner>(stack.Children, async newItem => {
            if (newItem is null) return;
            await using var db = new AppDatabase();
            db.Attach(newItem);
            db.Owners.Update(newItem);
            await db.SaveChangesAsync();
            ViewModel!.ReplaceItem(i, newItem);
        });

        await dialog.ShowAsync();
    }

    private static readonly Dictionary<int, Func<Owner, object>> OrderSelectors = new() {
        { 1, it => it.Id },
        { 2, it => it.FullName },
        { 3, it => it.Passport },
        { 4, it => it.BirthDate },
    };

    private static readonly Dictionary<int, Func<string, Func<Owner, bool>>> FilterSelectors = new() {
        { 1, query => it => it.Id.ToString().Contains(query) },
        { 2, query => it => it.FullName.Contains(query, StringComparison.InvariantCultureIgnoreCase) },
        { 3, query => it => it.Passport.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase) },
        { 4, query => it => it.BirthDate.ToString("dd.MM.yyyy").Contains(query, StringComparison.InvariantCultureIgnoreCase) },
    };

    private static object DefaultOrderSelector(Owner it) => it.Id;

    private static Func<Owner, bool> DefaultFilterSelector(string query)
        => it => it.Id.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.FullName.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Passport.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.BirthDate.ToString("dd.MM.yyyy").Contains(query, StringComparison.InvariantCultureIgnoreCase);

    private StackPanel GenerateDialogPanel(Owner item) {
        var stack = new StackPanel {
            Spacing = 15,
            Children = {
                new TextBox() {
                    Watermark = "ФИО",
                    [!TextBox.TextProperty] = new Binding("FullName"),
                },
                new NumericUpDown() {
                    FormatString = "0000 000000",
                    ShowButtonSpinner = false,
                    [!NumericUpDown.ValueProperty] = new Binding("Passport"),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                },
                new DatePicker() {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    [!DatePicker.SelectedDateProperty] = new Binding("BirthDate")
                }
            }
        };

        return stack;
    }
}