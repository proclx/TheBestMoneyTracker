using System;
using System.Windows; 

namespace MoneyRules.UI.Utils
{
    // Створюємо "імена" для наших тем
    public enum Theme
    {
        Light,
        Dark
    }

    public static class ThemeManager
    {
        public static void SwitchTheme(Theme theme)
        {
            // 1. Отримуємо доступ до ресурсів всієї програми
            //    (ВКАЗАНО ПОВНИЙ ШЛЯХ, ЩОБ УНИКНУТИ ПОМИЛКИ)
            var resources = System.Windows.Application.Current.Resources.MergedDictionaries;

            // 2. Очищуємо старі словники (файли) тем.
            for (int i = resources.Count - 1; i >= 0; i--)
            {
                var resource = resources[i];
                if (resource.Source != null && resource.Source.OriginalString.Contains("Themes/"))
                {
                    resources.RemoveAt(i);
                }
            }

            // 3. Створюємо новий словник (тему)
            ResourceDictionary newTheme = new ResourceDictionary();

            // 4. Визначаємо, який XAML-файл завантажити
            switch (theme)
            {
                case Theme.Light:
                    newTheme.Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative);
                    break;
                case Theme.Dark:
                    newTheme.Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
                    break;
            }

            // 5. Додаємо нову тему в ресурси програми
            resources.Add(newTheme);
        }
    }
}
