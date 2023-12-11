using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;
using KR2.Models;
using KR2.Utils;
using KR2.ViewModels;

namespace KR2.Views;

public partial class ConsultationView : ReactiveUserControl<TableViewModelBase<Consultation>> {
    public ConsultationView() {
        InitializeComponent();
        ViewModel = new(
            () => new AppDatabase().Consultations
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

    private async void RemoveItem(Consultation? i) {
        if (i is null) {
            return;
        }

        var result = await MessageBoxUtils.ShowYesNoDialog(
                         "Подтверждение",
                         $"Вы действительно хотите удалить запись"
                     );
        if (result is not ContentDialogResult.Primary) return;
        await using var db = new AppDatabase();
        db.Consultations.Remove(i);
        await db.SaveChangesAsync();
        ViewModel?.RemoveLocal(i);
    }

    private async Task NewItem() {
        var itemToEdit = new Consultation();
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

        dialog.AddControlValidation<Consultation>(stack.Children, async item => {
            if (item is null) return;
            item = item.Clone();
            await using var db = new AppDatabase();
            db.Attach(item);
            db.Consultations.Add(item);
            await db.SaveChangesAsync();
            ViewModel!.AddLocal(item);
        });
        await dialog.ShowAsync();
    }

    private async void EditItem(Consultation? i) {
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

        dialog.AddControlValidation<Consultation>(stack.Children, async newItem => {
            if (newItem is null) return;
            await using var db = new AppDatabase();
            db.Attach(newItem);
            db.Consultations.Update(newItem);
            await db.SaveChangesAsync();
            ViewModel!.ReplaceItem(i, newItem);
        });

        await dialog.ShowAsync();
    }

    private static readonly Dictionary<int, Func<Consultation, object>> OrderSelectors = new() {
        { 1, it => it.Id },
        { 2, it => it.Title },
        { 3, it => it.Date },
        { 4, it => it.Time },
    };

    private static readonly Dictionary<int, Func<string, Func<Consultation, bool>>> FilterSelectors = new() {
        { 1, query => it => it.Id.ToString().Contains(query) },
        { 2, query => it => it.Title.Contains(query, StringComparison.InvariantCultureIgnoreCase) }, {
            3,
            query => it => it.Date.ToString("dd.MM.yyyy").Contains(query, StringComparison.InvariantCultureIgnoreCase)
        },
        { 4, query => it => it.Time.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase) },
    };

    private static object DefaultOrderSelector(Consultation it) => it.Id;

    private static Func<Consultation, bool> DefaultFilterSelector(string query)
        => it => it.Id.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Title.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Date.ToString("dd.MM.yyyy").Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Time.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase);

    private StackPanel GenerateDialogPanel(Consultation item) {
        var stack = new StackPanel {
            Spacing = 15,
            Children = {
                new TextBox() {
                    Watermark = "Название",
                    [!TextBox.TextProperty] = new Binding("Title"),
                },
                new DatePicker() {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    [!DatePicker.SelectedDateProperty] = new Binding("Date")
                },
                new TimePicker() {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    [!TimePicker.SelectedTimeProperty] = new Binding("Time")
                }
            }
        };

        return stack;
    }
}