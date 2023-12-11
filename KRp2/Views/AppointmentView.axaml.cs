using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
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
using ReactiveUI;

namespace KR2.Views; 

public partial class AppointmentView : ReactiveUserControl<TableViewModelBase<ConsultationAppointment>> {
    public AppointmentView() {
        InitializeComponent();
        ViewModel = new(
            () => new AppDatabase().Appointments
                .Include(x => x.Owner)
                .Include(x => x.Consultation)
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

    private async void RemoveItem(ConsultationAppointment? i) {
        if (i is null) {
            return;
        }

        var result = await MessageBoxUtils.ShowYesNoDialog(
                         "Подтверждение",
                         $"Вы действительно хотите удалить запись"
                     );
        if (result is not ContentDialogResult.Primary) return;
        await using var db = new AppDatabase();
        db.Appointments.Remove(i);
        await db.SaveChangesAsync();
        ViewModel?.RemoveLocal(i);
    }

    private async Task NewItem() {
        var itemToEdit = new ConsultationAppointment();
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

        dialog.AddControlValidation<ConsultationAppointment>(stack.Children, async item => {
            if (item is null) return;
            item = item.Clone();
            await using var db = new AppDatabase();
            db.Attach(item);
            db.Appointments.Add(item);
            await db.SaveChangesAsync();
            ViewModel!.AddLocal(item);
        });
        await dialog.ShowAsync();
    }

    private async void EditItem(ConsultationAppointment? i) {
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

        dialog.AddControlValidation<ConsultationAppointment>(stack.Children, async newItem => {
            if (newItem is null) return;
            await using var db = new AppDatabase();
            db.Attach(newItem);
            db.Appointments.Update(newItem);
            await db.SaveChangesAsync();
            ViewModel!.ReplaceItem(i, newItem);
        });

        await dialog.ShowAsync();
    }

    private static readonly Dictionary<int, Func<ConsultationAppointment, object>> OrderSelectors = new() {
        { 1, it => it.Id },
        { 2, it => it.OwnerId },
        { 3, it => it.ConsultationId },
    };

    private static readonly Dictionary<int, Func<string, Func<ConsultationAppointment, bool>>> FilterSelectors = new() {
        { 1, query => it => it.Id.ToString().Contains(query) },
        { 2, query => it => it.Owner.FullName.Contains(query, StringComparison.InvariantCultureIgnoreCase) },
        { 3, query => it => it.Consultation.Title.Contains(query, StringComparison.InvariantCultureIgnoreCase) },
    };

    private static object DefaultOrderSelector(ConsultationAppointment it) => it.Id;

    private static Func<ConsultationAppointment, bool> DefaultFilterSelector(string query)
        => it => it.Id.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Owner.FullName.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Consultation.Title.Contains(query, StringComparison.InvariantCultureIgnoreCase);

    private StackPanel GenerateDialogPanel(ConsultationAppointment item) {
        using var db = new AppDatabase();
        var owners = db.Owners.ToList();
        var consultations = db.Consultations.ToList();

        var stack = new StackPanel {
            Spacing = 15,
            Children = {
                new ComboBox() {
                    PlaceholderText = "Человек",
                    ItemsSource = owners,
                    [!SelectingItemsControl.SelectedItemProperty] = new Binding("Owner"),
                    [!SelectingItemsControl.SelectedValueProperty] = new Binding("OwnerId"),
                    DisplayMemberBinding = new Binding("FullName"),
                    SelectedValueBinding = new Binding("Id"),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                },
                new ComboBox() {
                    PlaceholderText = "Консультация",
                    ItemsSource = consultations,
                    [!SelectingItemsControl.SelectedItemProperty] = new Binding("Consultation"),
                    [!SelectingItemsControl.SelectedValueProperty] = new Binding("ConsultationId"),
                    DisplayMemberBinding = new Binding("Title"),
                    SelectedValueBinding = new Binding("Id"),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                },
            }
        };

        return stack;
    }
}