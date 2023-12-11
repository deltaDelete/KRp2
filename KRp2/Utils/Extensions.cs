using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace KR2.Utils; 


public static class Extensions {
    /// <summary>
    /// Конвертирует HEX литерал в цвет
    /// </summary>
    /// <param name="hex">
    /// HEX или uint, байты которого соответствуют цветам, то есть 0x TT RR GG BB
    /// где TT непрозрачность, может быть опущена
    /// RR, GG, BB цвета красный, зеленый, синий соответственно
    /// </param>
    /// <returns></returns>
    public static Color ToColor(this uint hex) {
        var bytes = BitConverter.GetBytes(hex);
        if (bytes.Length == 3) {
            return Color.FromArgb(0xFF, bytes[0], bytes[1], bytes[2]);
        }
        
        return Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]);
    }

    public static void Assign<T, TProperty>(this T? item, Expression<Func<T, TProperty>> selector, TProperty value) {
        if (item is null) {
            return;
        }

        if (selector.Body is not MemberExpression { Member: PropertyInfo propertyInfo })
        {
            throw new ArgumentException("The expression is not a member access expression or does not have access to a property.", nameof(selector));
        }
        
        propertyInfo.SetValue(item, value);
    }
    
    /// <summary>
    /// Выключает PrimaryButton диалогового окна, если один из контролов содержит класс :error
    /// </summary>
    /// <param name="dialog">Диалоговое окно, в котором будет реализована проверка</param>
    /// <param name="controls">Список с проверяемыми контролами</param>
    /// <param name="action">Действие по завершению</param>
    /// <typeparam name="T">Тип принимаемый действием</typeparam>
    /// <returns>Комманду, выполняемую ContentDialog</returns>
    public static void AddControlValidation<T>(this ContentDialog dialog, Controls controls, Func<T, Task> action) {
        var hasAnyErrors = controls.Select(
            control =>
                control.Classes.ObserveCollectionChanges()
                       .Select(
                           x =>
                               !(x.Sender as Classes)!
                                   .Contains(":error")
                       )
        );
        var isValid = hasAnyErrors.Aggregate((o, o1) =>
                                                 o.CombineLatest(o1, (b, b1) => b && b1)
        );
        isValid.Subscribe(x => {
            var btn = dialog.GetVisualDescendants().FirstOrDefault(x => x.Name == "PrimaryButton") as Button;
            btn.Assign(x => x.IsEnabled, x);
        });

        var primaryCommand = ReactiveUI.ReactiveCommand.CreateFromTask(action, isValid);
        dialog.PrimaryButtonCommand = primaryCommand;
    }
    
    /// <summary>
    /// Создает глубокую копию
    /// </summary>
    /// <param name="source">Объект подлежащий копированию</param>
    /// <typeparam name="T">Тип клона</typeparam>
    /// <returns>Полная (наверное) копия объекта</returns>
    public static T Clone<T>(this T source)
    {
        // var serialized = System.Text.Json.JsonSerializer.Serialize(source, new JsonSerializerOptions(JsonSerializerOptions.Default) {
        //     MaxDepth = 3,
        // });
        var serialized = JsonConvert.SerializeObject(source, JsonSerializerSettings);
        return System.Text.Json.JsonSerializer.Deserialize<T>(serialized)!;
    }

    private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings() {
        MaxDepth = 3,
        NullValueHandling = NullValueHandling.Include,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };
}